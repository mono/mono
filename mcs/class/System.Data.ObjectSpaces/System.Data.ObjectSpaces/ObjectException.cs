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
using System.Globalization;
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
		public ObjectException ()
			: base (Locale.GetText ("An object exception has occurred."))
		{
		}

		[MonoTODO()]
		public ObjectException (String s)
			: base (s)
		{
		}

		[MonoTODO()]
		public ObjectException (String s, Exception innerException)
			: base (s, innerException)
		{
		}
	}
}

#endif
