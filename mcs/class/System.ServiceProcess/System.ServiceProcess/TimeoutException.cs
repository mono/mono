//
// System.ServiceProcess.TimeoutException.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
//

using System;
using System.Runtime.Serialization;

namespace System.ServiceProcess {

        [Serializable]
        public class TimeoutException : SystemException
        {
                public TimeoutException () : base () { }

                public TimeoutException (string message) : base (message) { }

                public TimeoutException (SerializationInfo info, StreamingContext context) : base (info, context) { }
        }
}
