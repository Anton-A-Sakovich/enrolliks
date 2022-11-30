using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Enrolliks.Persistence.People;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static System.Net.Mime.MediaTypeNames.Application;

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

        [HttpPost("api/[controller]/create")]
        [ProducesResponseType(typeof(Person), Status201Created, Json)]
        [ProducesResponseType(typeof(PersonValidationErrorsModel), Status400BadRequest, Json)]
        [ProducesResponseType(Status409Conflict)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> Create(CreatePersonModel createPersonModel)
        {
            var personToCreate = _mapper.Map<CreatePersonModel, Person>(createPersonModel);
            var createResult = await _manager.CreateAsync(personToCreate);
            return createResult switch
            {
                ICreatePersonResult.Success(var createdPerson) => Created(
                    Url.Action(action: nameof(Delete), values: new { name = createdPerson.Name }) ?? createdPerson.Name, createdPerson),

                ICreatePersonResult.ValidationFailure(var errors) => BadRequest(
                    _mapper.Map<PersonValidationErrors, PersonValidationErrorsModel>(errors)),

                ICreatePersonResult.Conflict => Conflict((object?)null),
                ICreatePersonResult.RepositoryFailure => StatusCode(Status500InternalServerError),
                _ => throw new SwitchFailureException()
            };
        }

        [HttpDelete("api/[controller]/{name}")]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(typeof(object), Status404NotFound, Json)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] string name)
        {
            var deleteResult = await _manager.DeleteAsync(name);
            return deleteResult switch
            {
                IDeletePersonResult.Success => NoContent(),
                IDeletePersonResult.NotFound => NotFound(new object()),
                IDeletePersonResult.RepositoryFailure => StatusCode(Status500InternalServerError),
                _ => throw new SwitchFailureException()
            };
        }

        [HttpGet("api/[controller]")]
        [ProducesResponseType(typeof(IList<Person>), Status200OK, Json)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync()
        {
            var getAllResult = await _manager.GetAllAsync();
            return getAllResult switch
            {
                IGetAllPeopleResult.Success(IList<Person> people) => Ok(people),
                IGetAllPeopleResult.RepositoryFailure => StatusCode(Status500InternalServerError),
                _ => throw new SwitchFailureException(),
            };
        }
    }
}
