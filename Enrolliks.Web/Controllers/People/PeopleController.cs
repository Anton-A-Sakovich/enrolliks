using System.Threading.Tasks;
using AutoMapper;
using Enrolliks.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.Controllers.People
{
    public class PeopleController : PageController
    {
        private readonly IPeopleManager _manager;
        private readonly IMapper _mapper;

        public PeopleController(IPeopleManager manager, IMapper mapper)
        {
            _manager = manager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var peopleResult = await _manager.GetAllAsync();
            var peopleResultModel = _mapper.Map<DiscriminatedUnionModel<IGetAllPeopleResult>>(peopleResult);
            return Page(peopleResultModel);
        }

        [HttpPost("api/[controller]/create")]
        public async Task<IActionResult> Create([FromBody]CreatePersonModel personModel)
        {
            var originalPerson = new Person(Name: personModel.Name!);

            var createResult = await _manager.CreateAsync(originalPerson);
            return createResult switch
            {
                ICreatePersonResult.Success(var createdPerson) => Created(createdPerson.Name, createdPerson),
                ICreatePersonResult.Conflict => Conflict(),
                ICreatePersonResult.RepositoryFailure => StatusCode(500),
                ICreatePersonResult.ValidationFailure(var errors) => BadRequest(_mapper.Map<PersonValidationErrorsModel>(errors)),
                _ => throw new SwitchFailureException()
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
                _ => throw new SwitchFailureException()
            };
        }
    }
}
