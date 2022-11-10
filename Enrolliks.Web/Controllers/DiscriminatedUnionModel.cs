using System;

namespace Enrolliks.Web.Controllers
{
    public class DiscriminatedUnionModel<T>
        where T : notnull
    {
        private T _value = default!;

        public string Tag { get; private set; } = null!;

        public T Value
        {
            get => _value;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                _value = value;
                Tag = value.GetType().Name;
            }
        }
    }
}
