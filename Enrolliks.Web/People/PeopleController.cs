using System.Threading.Tasks;
using Enrolliks.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.People
{
    public class PeopleController : Controller
    {
        private readonly IPeopleRepository _repository;

        public PeopleController(IPeopleRepository storage)
        {
            _repository = storage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var people = await _repository.GetAllAsync();
            return View(people);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePersonModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePersonModel personModel)
        {
            if (string.IsNullOrWhiteSpace(personModel.Name))
            {
                return RedirectToAction();
            }

            var person = new Person(personModel.Name);
            await _repository.CreateAsync(person);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name)
        {
            await _repository.DeleteAsync(name);
            return RedirectToAction(nameof(Index));
        }
    }
}
