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

            var verify = () =>
            {
                mock.VerifyAll();

                // We cannot rely on the exception thrown by the mock for calls without a setup to fail a test.
                // The exception might be swallowed in the tested code, therefore we need to throw it outside of the tested code.
                if (mockBehavior == MockBehavior.Strict)
                    mock.VerifyNoOtherCalls();
            };

            return (mock.Object, verify);
        }
    }
}
