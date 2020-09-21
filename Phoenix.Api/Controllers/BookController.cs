using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phoenix.Api.Models.Api;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Route("api/[controller]")]
    public class BookController : BaseController
    {
        private readonly ILogger<BookController> _logger;
        private readonly Repository<Book> _bookRepository;

        public BookController(PhoenixContext phoenixContext, ILogger<BookController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._bookRepository = new Repository<Book>(phoenixContext);
        }

        [HttpGet]
        public async Task<IEnumerable<IBook>> Get()
        {
            this._logger.LogInformation("Api -> Book -> Get");

            IQueryable<Book> books = this._bookRepository.find();

            return await books.Select(book => new BookApi
            {
                id = book.Id,
                Name = book.Name,
                Publisher = book.Publisher,
                Info = book.Info
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IBook> Get(int id)
        {
            this._logger.LogInformation($"Api -> Book -> Get{id}");

            Book book = await this._bookRepository.find(id);

            return new BookApi
            {
                id = book.Id,
                Name = book.Name,
                Publisher = book.Publisher,
                Info = book.Info
            };
        }

    }
}
