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
		DataRow row;

		#region Constructors
#if NET_1_1
                public DBConcurrencyException ()
                        : base ()
                {
                }
#endif
		public DBConcurrencyException (string message)
			: base (message)
		{
		}

		public DBConcurrencyException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

#if NET_1_2
		public DBConcurrencyException (DataRow[] dataRows, string message, Exception inner)
			: base (message, inner)
		{
		}
#endif
		#endregion // Constructors

		#region Properties

		public DataRow Row {
			get { return row; }
			set { row = value;} // setting the row has no effect
		}

#if NET_1_2
		[MonoTODO]
		public int RowCount {
			get { throw new NotImplementedException (); }
		}
#endif

		#endregion // Properties

		#region Methods

#if NET_1_2
		[MonoTODO]
		public void CopyToRows (DataRow[] array)
		{
			throw new NotImplementedException ();
		}
#endif

#if NET_1_2
		[MonoTODO]
		public void CopyToRows (DataRow[] array, int ArrayIndex)
		{
			throw new NotImplementedException ();
		}
#endif
		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}

		#endregion // Methods
	}
}
