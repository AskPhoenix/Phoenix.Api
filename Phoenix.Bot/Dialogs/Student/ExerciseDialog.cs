﻿using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Phoenix.Bot.Extensions;
using Phoenix.Bot.Helpers;
using Phoenix.DataHandle.Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Phoenix.Bot.Helpers.CardHelper;

namespace Phoenix.Bot.Dialogs.Student
{
    public class ExerciseDialog : ComponentDialog
    {
        private readonly PhoenixContext _phoenixContext;
        private readonly BotState _conversationState;

        private readonly IStatePropertyAccessor<Course[]> _courses;
        private readonly IStatePropertyAccessor<int> _selCourse;

        private static class WaterfallNames
        {
            public const string Course          = "StudentExercise_Course_WaterfallDialog";
            public const string Lecture         = "StudentExercise_Lecture_WaterfallDialog";
            public const string LecturePast     = "StudentExercise_LecturePast_WaterfallDialog";
            public const string LectureOther    = "StudentExercise_LectureOther_WaterfallDialog";
            public const string Homework        = "StudentExercise_Homework_WaterfallDialog";
        }

        public ExerciseDialog(PhoenixContext phoenixContext, ConversationState conversationState) :
            base(nameof(ExerciseDialog))
        {
            _phoenixContext = phoenixContext;
            _conversationState = conversationState;

            _courses = _conversationState.CreateProperty<Course[]>("Courses");
            _selCourse = _conversationState.CreateProperty<int>("SelCourse");

            AddDialog(new UnaccentedChoicePrompt(nameof(UnaccentedChoicePrompt)));
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), null, "fr-fr"));

            AddDialog(new WaterfallDialog(WaterfallNames.Course,
                new WaterfallStep[]
                {
                    CourseStepAsync,
                    CourseSelectStepAsync
                }));

            AddDialog(new WaterfallDialog(WaterfallNames.Lecture,
                new WaterfallStep[]
                {
                    LectureStepAsync
                }));

            AddDialog(new WaterfallDialog(WaterfallNames.LecturePast,
                new WaterfallStep[]
                {
                    PastLectureAskStepAsync,
                    PastLectureStepAsync,
                    PastLectureSelectStepAsync
                }));

            AddDialog(new WaterfallDialog(WaterfallNames.LectureOther,
                new WaterfallStep[]
                {
                    OtherLectureStepAsync,
                    OtherLectureSelectStepAsync,
                    OtherLectureNotFoundStepAsync,
                    OtherLectureAgainStepAsync
                }));

            AddDialog(new WaterfallDialog(WaterfallNames.Homework,
                new WaterfallStep[]
                {
                    HomeworkStepAsync,
                    HomeworkPageStepAsync,
                    HomeworkOtherStepAsync
                }));
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            string FbId = innerDc.Context.Activity.From.Id;
            var courses = _phoenixContext.Course.Where(c => c.StudentCourse.
                Any(sc => sc.CourseId == c.Id && sc.Student.AspNetUser.FacebookId == FbId)).ToArray();
            await _courses.SetAsync(innerDc.Context, courses);
            
            if (courses == null || courses.Length == 0)
            {
                await innerDc.Context.SendActivityAsync("Απ' ό,τι φαίνεται δεν έχεις εγγραφεί σε κάποιο μάθημα προς το παρόν.");
                return await innerDc.EndDialogAsync(null, cancellationToken);
            }

            InitialDialogId = courses.Length == 1 ? InitialDialogId = WaterfallNames.Lecture : WaterfallNames.Course;

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override Task OnEndDialogAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default)
        {
            Task.Run(async () => 
            {
                await _courses.DeleteAsync(context);
                await _selCourse.DeleteAsync(context);
            });

            return base.OnEndDialogAsync(context, instance, reason, cancellationToken);
        }

        #region Course Waterfall Dialog

        private async Task<DialogTurnResult> CourseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            => await stepContext.PromptAsync(
                nameof(UnaccentedChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Για ποιο μάθημα θα ήθελες να δεις τις εργασίες σου;"),
                    RetryPrompt = MessageFactory.Text("Παρακαλώ επίλεξε ή πληκτρολόγησε ένα από τα παρακάτω μαθήματα:"),
                    Choices = ChoiceFactory.ToChoices((await _courses.GetAsync(stepContext.Context)).Select(c => c.Name).ToList())
                });

        private async Task<DialogTurnResult> CourseSelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            => await stepContext.BeginDialogAsync(WaterfallNames.Lecture, (stepContext.Result as FoundChoice)?.Index, cancellationToken);

        #endregion

        #region Lecture Waterfall Dialog

        private async Task<DialogTurnResult> LectureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await _selCourse.SetAsync(stepContext.Context, stepContext.Options is int ? (int)stepContext.Options : 0);
            var courses = await _courses.GetAsync(stepContext.Context);
            var selCourse = await _selCourse.GetAsync(stepContext.Context);

            var lecWithHw = _phoenixContext.Lecture.
                Where(l => l.CourseId == courses[selCourse].Id && l.Homework.Count > 0);
            if (lecWithHw.Count() == 0)
            {
                await stepContext.Context.SendActivityAsync("Δεν υπάρχουν ακόμα εργασίες για αυτό το μάθημα.");
                await stepContext.Context.SendActivityAsync("Απόλαυσε τον ελέυθερο χρόνο σου! 😎");

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            var grNow = DialogHelper.GreeceLocalTime();
            if (lecWithHw.Any(l => l.StartDateTime >= grNow))
            {
                var nextLec = lecWithHw.
                    Include(l => l.Homework).
                    Where(l => l.StartDateTime >= grNow).
                    ToList().
                    Aggregate((nl, l) => l.StartDateTime < nl.StartDateTime ? l : nl);

                bool singular = nextLec.Homework.Count == 1;
                await stepContext.Context.SendActivityAsync($"{(singular ? "Η" : "Οι")} εργασί{(singular ? "α" : "ες")} με την κοντινότερη προθεσμία " +
                    $"είναι για τις {nextLec.StartDateTime:m} και είναι {(singular ? "η" : "οι")} παρακάτω:");

                return await stepContext.BeginDialogAsync(WaterfallNames.Homework, nextLec.Id, cancellationToken);
            }

            return await stepContext.BeginDialogAsync(WaterfallNames.LecturePast, null, cancellationToken);
        }

        #endregion

        #region Lecture Past Waterfall Dialog

        private async Task<DialogTurnResult> PastLectureAskStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Δεν έχεις νέες εργασίες για αυτό το μάθημα!");

            return await stepContext.PromptAsync(
            nameof(UnaccentedChoicePrompt),
            new PromptOptions
            {
                Prompt = MessageFactory.Text("Θα ήθελες να δεις παλαιότερες εργασίες σου;"),
                RetryPrompt = MessageFactory.Text("Παρακαλώ απάντησε με ένα Ναι ή Όχι:"),
                Choices = new Choice[] { new Choice("Ναι"), new Choice("Όχι, ευχαριστώ") { Synonyms = new List<string> { "Όχι" } } }
            });
        }

        private async Task<DialogTurnResult> PastLectureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var courses = await _courses.GetAsync(stepContext.Context);
            var selCourse = await _selCourse.GetAsync(stepContext.Context);

            if ((stepContext.Result as FoundChoice).Index == 1)
            {
                await stepContext.Context.SendActivityAsync("Εντάξει! Όποτε θέλεις μπορείς να ελέγξεις ξανά για νέες εργασίες! 😊");
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            var grNow = DialogHelper.GreeceLocalTime();

            var pLecDates = _phoenixContext.Lecture.
                Where(l => l.CourseId == courses[selCourse].Id && l.StartDateTime < grNow && l.Homework.Count > 0).
                OrderByDescending(h => h.StartDateTime).
                Take(5).
                Select(l => $"{l.StartDateTime.Day}/{l.StartDateTime.Month}").
                ToArray();

            await stepContext.Context.SendActivityAsync("Ωραία! Παρακάτω θα βρεις μερικές από τις πιο πρόσφατες ημερομηνίες που είχες μάθημα.");

            var prompt = ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(pLecDates), 
                text: "Επίλεξε μία από αυτές ή πληκτρολόγησε κάποια άλλη παρακάτω:");
            var repropmt = ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(pLecDates),
                text: "Η επιθυμητή ημερομηνία θα πρέπει να είναι στη μορφή ημέρα/μήνας (π.χ. 24/4)):");

            return await stepContext.PromptAsync(
                nameof(DateTimePrompt),
                new PromptOptions
                {
                    Prompt = (Activity)prompt,
                    RetryPrompt = (Activity)repropmt
                });
        }

        private async Task<DialogTurnResult> PastLectureSelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            => await stepContext.ReplaceDialogAsync(WaterfallNames.LectureOther, stepContext.Result, cancellationToken);

        #endregion

        #region Homework Waterfall Dialog

        private async Task<DialogTurnResult> HomeworkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            int lecId = Convert.ToInt32(stepContext.Options);

            var pageAcsr = _conversationState.CreateProperty<int>("HomeworkPage");
            int page = await pageAcsr.GetAsync(stepContext.Context);

            string fbId = stepContext.Context.Activity.From.Id;
            var lecDate = _phoenixContext.Lecture.Find(lecId).StartDateTime;
            bool forPastLec = lecDate < DialogHelper.GreeceLocalTime();
            decimal? grade = null;
            const int pageSize = 3;

            var paginatedHw = _phoenixContext.Homework.
                Include(h => h.Exercise.Book).
                ToList().
                Where(h => h.ForLectureId == lecId).
                Where((_, i) => i >= pageSize * page && i < pageSize * (page + 1));

            foreach (var hw in paginatedHw)
            {
                await stepContext.Context.SendActivityAsync(new Activity(type: ActivityTypes.Typing));

                var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
                card.BackgroundImage = new AdaptiveBackgroundImage("https://www.bot.askphoenix.gr/assets/4f5d75_sq.png");
                card.Body.Add(new AdaptiveTextBlockHeaderLight(hw.Exercise.Name));
                card.Body.Add(new AdaptiveRichFactSetLight("Βιβλίο ", hw.Exercise.Book.Name));
                card.Body.Add(new AdaptiveRichFactSetLight("Σελίδα ", hw.Exercise.Page.ToString(), separator: true));
                if (forPastLec)
                {
                    grade = _phoenixContext.StudentExercise.
                        SingleOrDefault(se => se.ExerciseId == hw.Exercise.Id && se.Student.AspNetUser.FacebookId == fbId)?.
                        Grade;
                    card.Body.Add(new AdaptiveRichFactSetLight("Βαθμός ", grade.ToString() ?? "-", separator: true));
                }
                card.Body.Add(new AdaptiveRichFactSetLight("Σχόλια ", string.IsNullOrEmpty(hw.Exercise.Info) ? "-" : hw.Exercise.Info, separator: true));

                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Attachment(new Attachment(contentType: AdaptiveCard.ContentType, content: JObject.FromObject(card))));
            }

            int hwCount = _phoenixContext.Homework.Count(h => h.ForLectureId == lecId);
            if (pageSize * (page + 1) < hwCount)
            {
                int hwLeft = hwCount - (pageSize * page + paginatedHw.Count());
                int showMoreNum = hwLeft <= pageSize ? hwLeft : pageSize;
                bool singular = hwLeft == 1;
                string promptText = page == 0 ? 
                    $"Αυτές ήταν οι {pageSize} πρώτες εργασίες που έχεις για τις {lecDate:m}." : 
                    $"Υπάρχ{(singular ? "ει" : "ουν")} ακόμη {hwLeft} εργασί{(singular ? "α" : "ες")} για τις {lecDate:m}.";

                await pageAcsr.SetAsync(stepContext.Context, page + 1);

                return await stepContext.PromptAsync(
                    nameof(UnaccentedChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(promptText),
                        RetryPrompt = MessageFactory.Text("Παρακαλώ επίλεξε μία από τις παρακάτω απαντήσεις:"),
                        Choices = new Choice[] { new Choice($"Εμφάνιση {showMoreNum} ακόμη"), new Choice("Ολοκλήρωση")}
                    });
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> HomeworkPageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is FoundChoice foundChoice && foundChoice.Index == 0)
                return await stepContext.ReplaceDialogAsync(WaterfallNames.Homework, stepContext.Options, cancellationToken);

            await _conversationState.CreateProperty<int>("HomeworkPage").DeleteAsync(stepContext.Context);

            return await stepContext.PromptAsync(
                nameof(UnaccentedChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Θα ήθελες να δεις εργασίες για άλλη ημερομηνία;"),
                    RetryPrompt = MessageFactory.Text("Παρακαλώ απάντησε με ένα Ναι ή Όχι:"),
                    Choices = new Choice[] { new Choice("Ναι"), new Choice("Όχι, ευχαριστώ") { Synonyms = new List<string> { "Όχι" } } }
                });
        }

        private async Task<DialogTurnResult> HomeworkOtherStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            => await stepContext.ReplaceDialogAsync(WaterfallNames.LectureOther, stepContext.Result, cancellationToken);

        #endregion

        #region Lecture Other Waterfall Dialog

        private async Task<DialogTurnResult> OtherLectureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Options is IList<DateTimeResolution>)
                return await stepContext.NextAsync(stepContext.Options, cancellationToken);

            if (stepContext.Options is FoundChoice foundChoice && foundChoice.Index == 1)
            {
                await stepContext.Context.SendActivityAsync("ΟΚ! 😊");
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            var courses = await _courses.GetAsync(stepContext.Context);
            var selCourse = await _selCourse.GetAsync(stepContext.Context);

            var grNow = DialogHelper.GreeceLocalTime();
            var lecDates = _phoenixContext.Lecture.
                    Where(l => l.CourseId == courses[selCourse].Id && l.Homework.Count > 0).
                    Select(l => l.StartDateTime).
                    Take(5).
                    OrderByDescending(d => d).
                    Select(d => $"{d.Day}/{d.Month}").
                    ToList();

            var prompt = ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(lecDates),
                text: "Επίλεξε μία από τις παρακάτω ημερομηνίες ή γράψε μια άλλη:");
            var reprompt = ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(lecDates),
                text: "Η επιθυμητή ημερομηνία θα πρέπει να είναι στη μορφή ημέρα/μήνας (π.χ. 24/4)):");

            return await stepContext.PromptAsync(
                nameof(DateTimePrompt),
                new PromptOptions
                {
                    Prompt = (Activity)prompt,
                    RetryPrompt = (Activity)reprompt
                });
        }

        private async Task<DialogTurnResult> OtherLectureSelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var courses = await _courses.GetAsync(stepContext.Context);
            var selCourse = await _selCourse.GetAsync(stepContext.Context);

            var selDate = DialogHelper.ResolveDateTime(stepContext.Result as IList<DateTimeResolution>);
            var lec = _phoenixContext.Lecture.
                Include(l => l.Homework).
                FirstOrDefault(l => l.CourseId == courses[selCourse].Id && l.StartDateTime.Date == selDate.Date);

            if (lec == null)
            {
                lec = _phoenixContext.Lecture.
                    Where(l => l.CourseId == courses[selCourse].Id && l.Homework.Count > 0).
                    ToList().
                    Aggregate((l, cl) => Math.Abs((l.StartDateTime - selDate).Days) < Math.Abs((cl.StartDateTime - selDate).Days) ? l : cl);

                await stepContext.Context.SendActivityAsync($"Δεν υπάρχει διάλεξη για αυτό το μάθημα στις {selDate:m}.");
                await stepContext.Context.SendActivityAsync($"Βρήκα όμως για την πιο κοντινή της στις {lec.StartDateTime:m}:");

                return await stepContext.ReplaceDialogAsync(WaterfallNames.Homework, lec.Id, cancellationToken);
            }

            int hwCount = lec.Homework.Count;
            if (hwCount == 0)
            {
                await stepContext.Context.SendActivityAsync($"Δεν υπάρχουν εργασίες για τις {selDate:m}");
                return await stepContext.NextAsync(null, cancellationToken);
            }

            bool singular = hwCount == 1;
            await stepContext.Context.SendActivityAsync($"Για τις {selDate:m} έχεις τ{(singular ? "ην" : "ις")} παρακάτω εργασί{(singular ? "α" : "ες")}:");
            return await stepContext.ReplaceDialogAsync(WaterfallNames.Homework, lec.Id, cancellationToken);
        }

        private async Task<DialogTurnResult> OtherLectureNotFoundStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            => await stepContext.PromptAsync(
                nameof(UnaccentedChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Θα ήθελες να δοκιμάσεις ξανά με άλλη ημερομηνία;"),
                    RetryPrompt = MessageFactory.Text("Παρακαλώ απάντησε με ένα Ναι ή Όχι:"),
                    Choices = new Choice[] { new Choice("Ναι"), new Choice("Όχι, ευχαριστώ") { Synonyms = new List<string> { "Όχι" } } }
                });

        private async Task<DialogTurnResult> OtherLectureAgainStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            => await stepContext.ReplaceDialogAsync(WaterfallNames.LectureOther, stepContext.Result, cancellationToken);

        #endregion
    }
}
