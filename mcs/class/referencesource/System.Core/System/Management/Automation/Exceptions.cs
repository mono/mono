using System;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;

namespace System.Management.Instrumentation
{
#region Instrumentation exceptions
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class InstrumentationBaseException : Exception
    {
        public InstrumentationBaseException()
            : base()
        {
        }

        public InstrumentationBaseException(string message)
            : base(message)
        {
        }

        public InstrumentationBaseException(string message, Exception innerException)
            : base(message, innerException)
        { 
        }

        protected InstrumentationBaseException(SerializationInfo info, StreamingContext context)
            :base (info, context)
        { 
        }
    }
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class InstrumentationException:InstrumentationBaseException
    {
        public InstrumentationException()
            : base()
        { 
        }

        public InstrumentationException(string message)
            : base(message)
        {

        }

        public InstrumentationException(Exception innerException)
            : base(null, innerException)
        {

        }

        public InstrumentationException(string message, Exception innerException)
            : base(message, innerException)
        { 
            
        }

        protected InstrumentationException(SerializationInfo info, StreamingContext context)
            :base (info, context)
        { 
        }
    }
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class InstanceNotFoundException : InstrumentationException
    { 
        public InstanceNotFoundException()
            : base()
        {
        }

        public InstanceNotFoundException(string message)
            : base(message)
        {
        }
        public InstanceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { 
        }
        protected InstanceNotFoundException(SerializationInfo info, StreamingContext context)
            :base (info, context)
        { 
        }
    }
#endregion
}
