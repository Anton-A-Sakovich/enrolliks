using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Enrolliks.Persistence.Skills;
using Microsoft.EntityFrameworkCore;

namespace Enrolliks.Persistence.EntityFramework.Skills
{
    internal class SkillsRepository : ISkillsRepository
    {
        private readonly EnrolliksContext _context;
        private readonly IMapper _mapper;

        public SkillsRepository(EnrolliksContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICreateSkillResult> CreateAsync(Skill skillToCreate)
        {
            if (skillToCreate is null) throw new ArgumentNullException(nameof(skillToCreate));

            var entity = _mapper.Map<Skill, SkillEntity>(skillToCreate);
            _context.Skills.Add(entity);
            await _context.SaveChangesAsync();

            var createdSkill = _mapper.Map<SkillEntity, Skill>(entity);
            return new ICreateSkillResult.Created(createdSkill);
        }

        public async Task<IDeleteSkillResult> DeleteAsync(string id)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));

            var entity = new SkillEntity { Id = id };
            _context.Skills.Remove(entity);
            await _context.SaveChangesAsync();

            return new IDeleteSkillResult.Deleted();
        }

        public async Task<ISkillExistsResult> ExistsAsync(string id)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));

            bool exists = await _context.Skills.AnyAsync(skill => skill.Id == id);
            return new ISkillExistsResult.Success(exists);
        }

        public async Task<ISkillWithNameExistsResult> ExistsWithNameAsync(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            bool exists = await _context.Skills.AnyAsync(skill => skill.Name == name);
            return new ISkillWithNameExistsResult.Success(exists);
        }

        public async Task<IGetManySkillsResult> GetAllAsync()
        {
            var skills = await _mapper.ProjectTo<Skill>(_context.Skills.AsNoTracking()).ToListAsync();
            return new IGetManySkillsResult.Success(skills);
        }

        public async Task<IGetOneSkillResult> GetOneAsync(string id)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));

            var entity = await _context.Skills.AsNoTracking().FirstOrDefaultAsync(skill => skill.Id == id);
            return entity is null
                ? new IGetOneSkillResult.NotFound()
                : new IGetOneSkillResult.Success(_mapper.Map<SkillEntity, Skill>(entity));
        }

        public async Task<IUpdateSkillResult> UpdateAsync(Skill skillToUpdate)
        {
            if (skillToUpdate is null) throw new ArgumentNullException(nameof(skillToUpdate));
            if (skillToUpdate.Id is null) throw new ArgumentException("A skill to update must have a not null ID", nameof(skillToUpdate));

            var entity = await _context.Skills.FirstOrDefaultAsync(skill => skill.Id == skillToUpdate.Id);
            if (entity is null) return new IUpdateSkillResult.NotFound();

            _mapper.Map(skillToUpdate, entity);
            await _context.SaveChangesAsync();

            var updatedSkill = _mapper.Map<SkillEntity, Skill>(entity);
            return new IUpdateSkillResult.Success(updatedSkill);
        }
    }
}
