using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ApplicationController
    {
        private readonly BookRepository _bookRepository;

        public BookController(
            ILogger<BookController> logger,
            ApplicationUserManager userManager,
            PhoenixContext phoenixContext)
            : base(logger, userManager)
        {
            _bookRepository = new(phoenixContext);
        }

        // TODO: Return Action Result with object, instead of the object itself ?
        [HttpGet]
        public IEnumerable<BookApi>? Get()
        {
            _logger.LogInformation("Api -> Book -> Get");

            if (!this.CheckUserAuth())
                return null;

            return this.AppUser?.User.Courses
                .SelectMany(c => c.Books.Select(b => new BookApi(b)));
        }

        [HttpGet("{id}")]
        public async Task<BookApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Book -> Get {id}", id);

            if (!this.CheckUserAuth())
                return null;

            var book = await _bookRepository.FindPrimaryAsync(id);
            if (book is null)
                return null;

            if (!book.Courses.Any(c => c.Users.Any(u => u.AspNetUserId == this.AppUser!.Id)))
            {
                _logger.LogError("User with id {UserId} " +
                    "is not authorized to access book with id {id}", id);
                return null;
            }
            
            return new BookApi(book);
        }
    }
}
