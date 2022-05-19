using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phoenix.DataHandle.Api.Models.Main;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : Controller
    {
        private readonly ILogger<BookController> _logger;
        private readonly BookRepository _bookRepository;

        public BookController(
            ILogger<BookController> logger,
            PhoenixContext phoenixContext)
        {
            _logger = logger;
            _bookRepository = new(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<BookApi>> GetAsync()
        {
            _logger.LogInformation("Api -> Book -> Get");

            return await _bookRepository.Find().Select(book => new BookApi(book)).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<BookApi?> GetAsync(int id)
        {
            _logger.LogInformation("Api -> Book -> Get {id}", id);

            var book = await _bookRepository.FindPrimaryAsync(id);
            if (book is null)
                return null;

           return new BookApi(book);
        }
    }
}
