//
// System.Data.DBConcurrencyException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {
	[Serializable]
	public sealed class DBConcurrencyException : SystemException
	{
		public DBConcurrencyException (string message)
			: base (message)
		{
		}

		public DBConcurrencyException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		DataRow row;

		public DataRow Row {
			get { return row; }
			set { row = value;} // setting the row has no effect
		}

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}
	}
}
