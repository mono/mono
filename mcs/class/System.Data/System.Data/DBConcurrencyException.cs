//
// System.Data.DBConcurrencyException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class DBConcurrencyException : SystemException
	{
		public DBConcurrencyException (string message)
			: base (message)
		{
		}

		public DBConcurrencyException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public DataRow Row {
			get { return row; }
			set {} // setting the row has no effect
		}

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base (info, context)
		}
	}
}
