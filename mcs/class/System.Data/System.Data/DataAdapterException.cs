//
// System.Data.DataAdapterException.cs
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
	public class DataAdapterException : InvalidOperationException
	{
		#region Constructors

		public DataAdapterException ()
			: base (Locale.GetText ("A DataAdapterException has occurred."))
		{
		}

		public DataAdapterException (string s)
			: base (s)
		{
		}

		public DataAdapterException (string s, Exception innerException)
			: base (s, innerException)
		{
		}

		public DataAdapterException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		#endregion // Constructors
	}
}

#endif // NET_1_2
