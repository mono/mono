//
// System.Data.ObjectSpaces.ContextException.cs : The exception thrown when an ObjectContext encounters an error
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data.ObjectSpaces
{
        public class ContextException : ObjectException
        {
                [MonoTODO]        
                public ContextException () 
			: base (Locale.GetText ("A Context Exception has occurred."))
		{
		}
                
                [MonoTODO]
                protected ContextException (SerializationInfo info, StreamingContext context) 
			: base (info, context)
		{
		}
                                
                [MonoTODO]
                public ContextException (string message) 
			: base (message)
		{
		}

                [MonoTODO]
                public ContextException (string message, Exception innerException) 
			: base (message, innerException)
		{
		}
        }
}

#endif
