//
// System.Data.ObjectSpaces.ObjectException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Runtime.Serialization;

namespace System.Data.ObjectSpaces
{
        public class ObjectException : SystemException
        {
		[MonoTODO()]
		protected ObjectException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		[MonoTODO()]
		protected ObjectException ()
			: base ("An object exception has occurred.")
		{
		}

		[MonoTODO()]
		protected ObjectException (String s)
			: base (s)
		{
		}

		[MonoTODO()]
		protected ObjectException (String s, Exception innerException)
			: base (s, innerException)
		{
		}

		[MonoTODO ("Placeholder")]
		internal static ObjectException CreateObjectException ()
		{
			return new ObjectException ();
		}
	}
}

#endif
