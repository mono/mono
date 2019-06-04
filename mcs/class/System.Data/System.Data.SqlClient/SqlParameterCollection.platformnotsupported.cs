// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace System.Data.SqlClient
{
	public partial class SqlParameterCollection : DbParameterCollection , IDataParameterCollection, IList, ICollection, IEnumerable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlParameterCollection is not supported on the current platform.";

		internal SqlParameterCollection () {}

		protected override DbParameter GetParameter (int index)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override DbParameter GetParameter (string parameterName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override void SetParameter (int index, DbParameter value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override void SetParameter (string parameterName, DbParameter value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override int Add (object value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlParameter Add (SqlParameter value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlParameter AddWithValue (string parameterName, object value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size, string sourceColumn)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void Clear ()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override bool Contains (object value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override bool Contains (string value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public bool Contains (SqlParameter value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void CopyTo (Array array, int index)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override IEnumerator GetEnumerator ()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override int IndexOf (object value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override int IndexOf (string parameterName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public int IndexOf (SqlParameter value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void Insert (int index, object value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void Insert (int index, SqlParameter value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void Remove (object value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void Remove (SqlParameter value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void RemoveAt (int index)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void RemoveAt (string parameterName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void AddRange (Array values)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void AddRange (SqlParameter [] values)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void CopyTo (SqlParameter [] array, int index)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override int Count
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override bool IsFixedSize
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override bool IsReadOnly
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override bool IsSynchronized
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override object SyncRoot
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlParameter this [int index] {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlParameter this [string parameterName] {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
		
		internal bool IsDirty {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
