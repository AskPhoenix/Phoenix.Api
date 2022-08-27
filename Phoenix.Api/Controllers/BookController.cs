using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ApplicationController
    {
        private readonly BookRepository _bookRepository;

        public BookController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<BookController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _bookRepository = new(phoenixContext);
        }

        [HttpGet]
        public IEnumerable<BookApi>? Get()
        {
            _logger.LogInformation("Api -> Book -> Get");

            if (!this.CheckUserAuth())
                return null;

            return this.PhoenixUser?
                .Courses
                .SelectMany(c => c.Books)
                .Select(b => new BookApi(b));
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
