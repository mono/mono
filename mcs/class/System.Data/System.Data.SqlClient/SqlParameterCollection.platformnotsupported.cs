//
// SqlParameterCollection.cs
//
// Author:
//       Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace System.Data.SqlClient
{
	public class SqlParameterCollection : DbParameterCollection , IDataParameterCollection, IList, ICollection, IEnumerable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlParameterCollection is not supported on the current platform.";

		SqlParameterCollection () {}

		protected override DbParameter GetParameter (int index)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override DbParameter GetParameter (string parameterName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void SetParameter (int index, DbParameter value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void SetParameter (string parameterName, DbParameter value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int Add (object value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlParameter Add (SqlParameter value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlParameter Add (string parameterName, object value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlParameter AddWithValue (string parameterName, object value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size, string sourceColumn)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Clear ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool Contains (object value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool Contains (string value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool Contains (SqlParameter value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void CopyTo (Array array, int index)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override IEnumerator GetEnumerator ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int IndexOf (object value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int IndexOf (string parameterName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int IndexOf (SqlParameter value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Insert (int index, object value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Insert (int index, SqlParameter value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Remove (object value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Remove (SqlParameter value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void RemoveAt (int index)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void RemoveAt (string parameterName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void AddRange (Array values)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AddRange (SqlParameter [] values)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void CopyTo (SqlParameter [] array, int index)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int Count {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsFixedSize {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsReadOnly {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsSynchronized {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override object SyncRoot {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public SqlParameter this [int index] {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public SqlParameter this [string parameterName] {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
	}
}
