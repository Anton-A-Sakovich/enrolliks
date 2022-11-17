using System.Threading.Tasks;
using AutoMapper;
using Enrolliks.Persistence.People;
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
    }
}
