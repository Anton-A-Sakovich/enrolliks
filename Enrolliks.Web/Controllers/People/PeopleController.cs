using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Enrolliks.Persistence.People;
using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.Controllers.People
{
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    public class PeopleController : ControllerBase
    {
        private readonly IPeopleManager _manager;
        private readonly IMapper _mapper;

        public PeopleController(IPeopleManager manager, IMapper mapper)
        {
            _manager = manager;
            _mapper = mapper;
        }

        [HttpGet("api/people")]
        [ProducesResponseType(typeof(IList<Person>), 200, "application/json")]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllAsync()
        {
            var peopleResult = await _manager.GetAllAsync();
            return peopleResult switch
            {
                IGetAllPeopleResult.Success(IList<Person> people) => Ok(people),
                IGetAllPeopleResult.RepositoryFailure => StatusCode(500),
                _ => throw new SwitchFailureException(),
            };
        }

        [HttpPost("api/people/create")]
        [ProducesResponseType(typeof(Person), 201, "application/json")]
        [ProducesResponseType(typeof(PersonValidationErrorsModel), 400, "application/json")]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create(CreatePersonModel personModel)
        {
            var originalPerson = new Person(Name: personModel.Name);

            var createResult = await _manager.CreateAsync(originalPerson);
            return createResult switch
            {
                ICreatePersonResult.Success(var createdPerson)
                    => Created(Url.ActionLink(action: nameof(Delete), values: new { name = createdPerson.Name }) ?? "", createdPerson),
                ICreatePersonResult.Conflict => Conflict(),
                ICreatePersonResult.RepositoryFailure => StatusCode(500),
                ICreatePersonResult.ValidationFailure(var errors) => BadRequest(_mapper.Map<PersonValidationErrorsModel>(errors)),
                _ => throw new SwitchFailureException()
            };
        }

        [HttpDelete("api/people/{name}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute]string name)
        {
            var deleteResult = await _manager.DeleteAsync(name);
            return deleteResult switch
            {
                IDeletePersonResult.NotFound => NotFound(),
                IDeletePersonResult.RepositoryFailure => StatusCode(500),
                IDeletePersonResult.Success => NoContent(),
                _ => throw new SwitchFailureException()
            };
        }
    }
}
