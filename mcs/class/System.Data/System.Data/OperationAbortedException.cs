//
// System.Data.OperationAbortedException.cs
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
	public class OperationAbortedException : SystemException
	{
		#region Constructors

		public OperationAbortedException ()
			: base (Locale.GetText ("An OperationAbortedException has occurred."))
		{
		}

		public OperationAbortedException (string s)
			: base (s)
		{
		}

		public OperationAbortedException (string s, Exception innerException)
			: base (s, innerException)
		{
		}

		public OperationAbortedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		#endregion // Constructors
	}
}

#endif // NET_1_2
