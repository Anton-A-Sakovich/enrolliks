using System;
using System.Runtime.Serialization;

namespace Enrolliks.Web.Controllers
{
    /// <summary>
    /// The exception that is thrown when none of the <c>switch</c> patterns were able to match the <c>switch</c> expression.
    /// </summary>
    internal class SwitchFailureException : ApplicationException
    {
        public SwitchFailureException() : base("The switch cases were incomplete.")
        {
        }

        protected SwitchFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
