// 
// System.IO.InternalBufferOverflowException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.Serialization;

namespace System.IO {
	[Serializable]
	public class InternalBufferOverflowException : SystemException {

		#region Constructors

		public InternalBufferOverflowException ()
			: base ("Internal buffer overflow occurred.")
		{
		}

		public InternalBufferOverflowException (string message)
			: base (message)
		{
		}

		protected InternalBufferOverflowException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public InternalBufferOverflowException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		#endregion // Constructors
	}
}
