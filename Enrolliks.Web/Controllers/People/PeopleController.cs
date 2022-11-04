using System;
using System.Threading.Tasks;
using Enrolliks.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.Controllers.People
{
    public class PeopleController : PageController
    {
        private readonly IPeopleManager _manager;

        public PeopleController(IPeopleManager manager)
        {
            _manager = manager;
        }

        public async Task<IActionResult> Index()
        {
            var peopleResult = await _manager.GetAllAsync();
            var peopleDto = new GetAllPeopleResultModel(peopleResult);
            return Page(peopleDto);
        }

        [HttpPost("api/[controller]/create")]
        public async Task<IActionResult> Create([FromBody]CreatePersonModel personModel)
        {
            var originalPerson = new Person(Name: personModel.Name!);

            var createResult = await _manager.CreateAsync(originalPerson);
            return createResult switch
            {
                ICreatePersonResult.Success(var createdPerson) => Ok(createdPerson),
                ICreatePersonResult.Conflict => Conflict(),
                ICreatePersonResult.RepositoryFailure => StatusCode(500),
                ICreatePersonResult.ValidationFailure(var errors) => BadRequest(errors),
                _ => throw new ApplicationException("The switch cases were incomplete.")
            };
        }

        [HttpDelete("api/[controller]/delete")]
        public async Task<IActionResult> Delete([FromBody]string? name)
        {
            var deleteResult = await _manager.DeleteAsync(name!);
            return deleteResult switch
            {
                IDeletePersonResult.NotFound => NotFound(),
                IDeletePersonResult.RepositoryFailure => StatusCode(500),
                IDeletePersonResult.Success => NoContent(),
                _ => throw new ApplicationException("The switch cases were incomplete.")
            };
        }
    }
}
