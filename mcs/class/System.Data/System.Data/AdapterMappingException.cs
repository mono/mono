//
// System.Data.AdapterMappingException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {
	public class AdapterMappingException : InvalidOperationException
	{
		#region Constructors

		public AdapterMappingException ()
			: base (Locale.GetText ("An AdapterMappingException has occurred."))
		{
		}

		public AdapterMappingException (string s)
			: base (s)
		{
		}

		public AdapterMappingException (string s, Exception innerException)
			: base (s, innerException)
		{
		}

		public AdapterMappingException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		#endregion // Constructors
	}
}

#endif // NET_1_2
