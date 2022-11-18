using System;
using System.Collections.Generic;
using Moq;

namespace Enrolliks.Persistence.Tests
{
    internal class MockBuilder<T> where T : class
    {
        private readonly IList<Action<Mock<T>>> _setups = new List<Action<Mock<T>>>();

        public MockBuilder<T> Setup(Action<Mock<T>> setup)
        {
            if (setup is null) throw new ArgumentNullException(nameof(setup));
            _setups.Add(setup);
            return this;
        }

        public (T MockedObject, Action Verify) Build(MockBehavior mockBehavior = MockBehavior.Strict)
        {
            var mock = new Mock<T>(mockBehavior);

            foreach (var setup in _setups)
                setup(mock);

            var verify = () => mock.VerifyAll();

            return (mock.Object, verify);
        }
    }
}
