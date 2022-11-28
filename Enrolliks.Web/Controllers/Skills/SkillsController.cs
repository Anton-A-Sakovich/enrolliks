using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Enrolliks.Persistence.Skills;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static System.Net.Mime.MediaTypeNames.Application;

namespace Enrolliks.Web.Controllers.Skills
{
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillsManager _manager;
        private readonly IMapper _mapper;

        public SkillsController(ISkillsManager manager, IMapper mapper)
        {
            _manager = manager;
            _mapper = mapper;
        }

        [HttpPost("api/[controller]/create")]
        [ProducesResponseType(typeof(Skill), Status201Created, Json)]
        [ProducesResponseType(typeof(SkillValidationErrorsModel), Status400BadRequest, Json)]
        [ProducesResponseType(typeof(CreateSkillConflictModel), Status409Conflict, Json)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> Create(CreateSkillModel createSkillModel)
        {
            var skillToCreate = _mapper.Map<CreateSkillModel, Skill>(createSkillModel);
            var createResult = await _manager.CreateAsync(skillToCreate);
            return createResult switch
            {
                ICreateSkillResult.Created(Skill createdSkill) => Created(
                    Url.Action(action: nameof(GetOne), values: new { id = createdSkill.Id }) ?? createdSkill.Id,
                    createdSkill),

                ICreateSkillResult.ValidationFailure(SkillValidationErrors errors) => BadRequest(
                    _mapper.Map<SkillValidationErrors, SkillValidationErrorsModel>(errors)),

                ICreateSkillResult.Conflict(string propertyName) => Conflict(
                    new CreateSkillConflictModel { PropertyName = propertyName }),

                ICreateSkillResult.RepositoryFailure => StatusCode(Status500InternalServerError),

                _ => throw new SwitchFailureException(),
            };
        }

        [HttpDelete("api/[controller]/{id}")]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            var deleteResult = await _manager.DeleteAsync(id);
            return deleteResult switch
            {
                IDeleteSkillResult.Deleted => NoContent(),
                IDeleteSkillResult.NotFound => NotFound(null),
                IDeleteSkillResult.RepositoryFailure => StatusCode(Status500InternalServerError),
                _ => throw new SwitchFailureException(),
            };
        }

        [HttpGet("api/[controller]")]
        [ProducesResponseType(typeof(IList<Skill>), Status200OK, Json)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var getAllResult = await _manager.GetAllAsync();
            return getAllResult switch
            {
                IGetManySkillsResult.Success(IList<Skill> skills) => Ok(skills),
                IGetManySkillsResult.RepositoryFailure => StatusCode(Status500InternalServerError),
                _ => throw new SwitchFailureException(),
            };
        }

        [HttpGet("api/[controller]/{id}")]
        [ProducesResponseType(typeof(Skill), Status200OK, Json)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> GetOne(string id)
        {
            var getOneResult = await _manager.GetOneAsync(id);
            return getOneResult switch
            {
                IGetOneSkillResult.Success(Skill skill) => Ok(skill),
                IGetOneSkillResult.NotFound => NotFound(null),
                IGetOneSkillResult.RepositoryFailure => StatusCode(Status500InternalServerError),
                _ => throw new SwitchFailureException(),
            };
        }

        [HttpPut("api/[controller]/{id}")]
        [ProducesResponseType(typeof(Skill), Status200OK, Json)]
        [ProducesResponseType(typeof(SkillValidationErrorsModel), Status400BadRequest, Json)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status409Conflict)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, UpdateSkillModel updateSkillModel)
        {
            updateSkillModel.Id = id;
            var skillToUpdate = _mapper.Map<UpdateSkillModel, Skill>(updateSkillModel);
            var updateResult = await _manager.UpdateAsync(skillToUpdate);
            return updateResult switch
            {
                IUpdateSkillResult.Success(Skill updatedSkill) => Ok(updatedSkill),

                IUpdateSkillResult.ValidationFailure(SkillValidationErrors errors) => BadRequest(
                    _mapper.Map<SkillValidationErrors, SkillValidationErrorsModel>(errors)),

                IUpdateSkillResult.NotFound => NotFound(null),
                IUpdateSkillResult.Conflict => Conflict((object?)null),
                IUpdateSkillResult.RepositoryFailure => StatusCode(Status500InternalServerError),
                _ => throw new SwitchFailureException(),
            };
        }
    }
}
