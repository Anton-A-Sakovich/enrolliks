using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace Enrolliks.Persistence.Tests
{
    internal class AssertionsBuilder<T> where T : class
    {
        private readonly IList<Action<T>> _assertions = new List<Action<T>>();

        public AssertionsBuilder<T> Assert<TResult>(Func<T, TResult> action, IResolveConstraint constraint)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (constraint is null) throw new ArgumentNullException(nameof(constraint));

            _assertions.Add(instance => NUnit.Framework.Assert.That(() => action(instance), constraint));

            return this;
        }

        public void Test(T instance)
        {
            NUnit.Framework.Assert.Multiple(() =>
            {
                foreach (var assertion in _assertions)
                    assertion(instance);
            });
        }
    }
}
