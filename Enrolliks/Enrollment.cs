using System;
using System.Collections.Generic;

namespace Enrolliks
{
    public record class Enrollment(
        Person Person,
        Project Project,
        DateTime UtcStart,
        DateTime? UtcEnd,
        IEnumerable<Skill> PracticedSkills);
}
