//
// System.Data.DBConcurrencyException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data
{
	[Serializable]
	public sealed class DBConcurrencyException : SystemException
	{
		DataRow [] rows;

		#region Constructors

		public DBConcurrencyException ()
#if NET_2_0
			: base ("Concurrency violation.")
#endif
		{
		}

		public DBConcurrencyException (string message)
			: base (message)
		{
		}

		public DBConcurrencyException (string message, Exception inner)
			: base (message, inner)
		{
		}

#if NET_2_0
		public
#else
		internal
#endif
		DBConcurrencyException (string message, Exception inner, DataRow[] dataRows)
			: base (message, inner)
		{
			rows = dataRows;
		}

		private DBConcurrencyException (SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

		#endregion // Constructors

		#region Properties

		public DataRow Row {
			get {
				if (rows != null)
					return rows [0];
				return null;
			}
			set { rows = new DataRow [] { value };}
		}

#if NET_2_0
		public int RowCount {
			get {
				if (rows != null)
					return rows.Length;
				return 0;
			}
		}
#endif

		#endregion // Properties

		#region Methods

#if NET_2_0
		[MonoTODO]
		public void CopyToRows (DataRow [] array)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyToRows (DataRow [] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}
#endif
		public override void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			if (si == null)
				throw new ArgumentNullException ("si");

			base.GetObjectData (si, context);
		}

		#endregion // Methods
	}
}
