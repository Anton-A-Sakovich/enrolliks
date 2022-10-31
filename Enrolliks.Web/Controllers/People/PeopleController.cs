using System.Threading.Tasks;
using Enrolliks.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.Controllers.People
{
    public class PeopleController : PageController
    {
        private readonly IPeopleRepository _repository;

        public PeopleController(IPeopleRepository storage)
        {
            _repository = storage;
        }

        public async Task<IActionResult> Index()
        {
            var people = await _repository.GetAllAsync();
            return Page(people);
        }

        public IActionResult Create()
        {
            return Page();
        }

        [HttpPost("api/[controller]/create")]
        public async Task<IActionResult> Create([FromBody]CreatePersonModel personModel)
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

        [HttpDelete("api/[controller]/delete")]
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
