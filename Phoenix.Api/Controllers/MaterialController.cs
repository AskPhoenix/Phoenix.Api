using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Phoenix.Api.Models.Api;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MaterialController : BaseController
    {
        private readonly ILogger<MaterialController> _logger;
        private readonly Repository<Material> _materialRepository;

        public MaterialController(PhoenixContext phoenixContext, ILogger<MaterialController> logger) : base(phoenixContext, logger)
        {
            this._logger = logger;
            this._materialRepository = new Repository<Material>(phoenixContext);
        }

        [HttpGet("{id}")]
        public async Task<IMaterial> Get(int id)
        {
            this._logger.LogInformation($"Api -> Material -> Get{id}");

            Material material = await this._materialRepository.find(id);

            return new MaterialApi
            {
                id = material.Id,
                Chapter = material.Chapter,
                Section = material.Section,
                Comments = material.Comments,
                Book = material.Book != null ? new BookApi
                {
                    id = material.Book.Id,
                    Name = material.Book.Name,
                    Publisher = material.Book.Publisher,
                    Info = material.Book.Info
                } : null,
                Exam = material.Exam != null ? new ExamApi
                {
                    id = material.Exam.Id,
                    Name = material.Exam.Name,
                    Comments = material.Exam.Comments
                } : null,
            };
        }

        [HttpPost]
        public async Task<MaterialApi> Post([FromBody] MaterialApi materialApi)
        {
            this._logger.LogInformation("Api -> Material -> Post");

            Material material = new Material
            {
                Id = materialApi.id,
                Chapter = materialApi.Chapter,
                Section = materialApi.Section,
                Comments = materialApi.Comments,
                BookId = materialApi.Book?.id,
                ExamId = materialApi.Exam.id
            };

            material = this._materialRepository.create(material);

            material = await this._materialRepository.find(material.Id);

            return new MaterialApi
            {
                id = material.Id,
                Chapter = material.Chapter,
                Section = material.Section,
                Comments = material.Comments,
                Book = material.Book != null ? new BookApi
                {
                    id = material.Book.Id,
                    Name = material.Book.Name,
                    Publisher = material.Book.Publisher,
                    Info = material.Book.Info
                } : null,
                Exam = material.Exam != null ? new ExamApi
                {
                    id = material.Exam.Id,
                    Name = material.Exam.Name,
                    Comments = material.Exam.Comments
                } : null,
            };
        }

        [HttpPut("{id}")]
        public async Task<MaterialApi> Put(int id, [FromBody] MaterialApi materialApi)
        {
            this._logger.LogInformation("Api -> Material -> Put");

            Material material = new Material
            {
                Id = materialApi.id,
                Chapter = materialApi.Chapter,
                Section = materialApi.Section,
                Comments = materialApi.Comments,
                BookId = materialApi.Book?.id,
                ExamId = materialApi.Exam.id
            };

            material = this._materialRepository.update(material);

            material = await this._materialRepository.find(material.Id);

            return new MaterialApi
            {
                id = material.Id,
                Chapter = material.Chapter,
                Section = material.Section,
                Comments = material.Comments,
                Book = material.Book != null ? new BookApi
                {
                    id = material.Book.Id,
                    Name = material.Book.Name,
                    Publisher = material.Book.Publisher,
                    Info = material.Book.Info
                } : null,
                Exam = material.Exam != null ? new ExamApi
                {
                    id = material.Exam.Id,
                    Name = material.Exam.Name,
                    Comments = material.Exam.Comments
                } : null,
            };
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            this._logger.LogInformation($"Api -> Material -> Get -> {id}");

            this._materialRepository.delete(id);
        }

    }
}
