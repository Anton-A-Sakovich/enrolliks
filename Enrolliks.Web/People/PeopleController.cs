﻿using System.Threading.Tasks;
using Enrolliks.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.People
{
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly IPeopleRepository _repository;

        public PeopleController(IPeopleRepository storage)
        {
            _repository = storage;
        }

        [HttpGet("[controller]/list")]
        public async Task<IActionResult> GetAll()
        {
            var people = await _repository.GetAllAsync();
            return Ok(people);
        }

        [HttpPost("[controller]/create")]
        public async Task<IActionResult> Create(CreatePersonModel personModel)
        {
            if (string.IsNullOrWhiteSpace(personModel.Name))
            {
                return StatusCode(400);
            }

            bool exists = await _repository.ExistsAsync(personModel.Name);
            if (exists)
            {
                return StatusCode(409);
            }

            var person = new Person(personModel.Name);
            await _repository.CreateAsync(person);
            return Ok();
        }

        [HttpDelete("[controller]/delete")]
        public async Task<IActionResult> Delete(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return StatusCode(400);
            }

            bool exists = await _repository.ExistsAsync(name);
            if (!exists)
            {
                return StatusCode(404);
            }

            await _repository.DeleteAsync(name);
            return Ok();
        }
    }
}
