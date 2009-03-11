#region MIT license
// 
// MIT license
//
// Copyright (c) 2009 Novell, Inc.
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
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Linq;

namespace DbLinq.Null {

	class NullConnection : DbConnection
	{
		public NullConnection ()
		{
		}

		public override string ConnectionString {get; set;}
		public override string Database {get {return "NullDatabase";}}
		public override string DataSource {get {return "NullDataSource";}}
		public override string ServerVersion {get {return "0.0";}}
		public override ConnectionState State {get {return ConnectionState.Closed;}}

		public override void ChangeDatabase (string databaseName)
		{
			throw new NotSupportedException ();
		}

		public override void Close ()
		{
		}

		public override void Open ()
		{
		}

		protected override DbTransaction BeginDbTransaction (IsolationLevel level)
		{
			throw new NotSupportedException ();
		}

		protected override DbCommand CreateDbCommand ()
		{
			return new NullCommand ();
		}
	}

	class NullParameter : DbParameter
	{
		public override DbType DbType {get; set;}
		public override ParameterDirection Direction {get; set;}
		public override bool IsNullable {get; set;}
		public override string ParameterName {get; set;}
		public override int Size {get; set;}
		public override string SourceColumn {get; set;}
		public override bool SourceColumnNullMapping {get; set;}
		public override DataRowVersion SourceVersion {get; set;}
		public override object Value {get; set;}

		public override void ResetDbType ()
		{
			throw new NotSupportedException ();
		}
	}

	class DbParameterCollection<TParameter> : DbParameterCollection
		where TParameter : DbParameter
	{
		List<TParameter> parameters = new List<TParameter> ();

		public DbParameterCollection ()
		{
		}

		public override int Count {get {return parameters.Count;}}
		public override bool IsFixedSize {get {return false;}}
		public override bool IsReadOnly {get {return false;}}
		public override bool IsSynchronized {get {return false;}}
		public override object SyncRoot {get {return parameters;}}

		public override int Add (object value)
		{
			if (!(value is TParameter))
				throw new ArgumentException ("wrong type", "value");
			parameters.Add ((TParameter) value);
			return parameters.Count-1;
		}

		public override void AddRange (Array values)
		{
			foreach (TParameter p in values)
				Add (p);
		}

		public override void Clear ()
		{
			parameters.Clear ();
		}

		public override bool Contains (object value)
		{
			return parameters.Contains ((TParameter) value);
		}

		public override bool Contains (string value)
		{
			return parameters.Any (p => p.ParameterName == value);
		}

		public override void CopyTo (Array array, int index)
		{
			((ICollection) parameters).CopyTo (array, index);
		}

		public override IEnumerator GetEnumerator ()
		{
			return parameters.GetEnumerator ();
		}

		public override int IndexOf (object value)
		{
			return parameters.IndexOf ((TParameter) value);
		}

		public override int IndexOf (string value)
		{
			for (int i = 0; i < parameters.Count; ++i)
				if (parameters [i].ParameterName == value)
					return i;
			return -1;
		}

		public override void Insert (int index, object value)
		{
			parameters.Insert (index, (TParameter) value);
		}

		public override void Remove (object value)
		{
			parameters.Remove ((TParameter) value);
		}

		public override void RemoveAt (int index)
		{
			parameters.RemoveAt (index);
		}

		public override void RemoveAt (string value)
		{
			int idx = IndexOf (value);
			if (idx >= 0)
				parameters.RemoveAt (idx);
		}

		protected override DbParameter GetParameter (int index)
		{
			return parameters [index];
		}

		protected override DbParameter GetParameter (string value)
		{
			return parameters.Where (p => p.ParameterName == value)
				.FirstOrDefault ();
		}

		protected override void SetParameter (int index, DbParameter value)
		{
			parameters [index] = (TParameter) value;
		}

		protected override void SetParameter (string index, DbParameter value)
		{
			parameters [IndexOf (value)] = (TParameter) value;
		}
	}

	class NullCommand : DbCommand
	{
		DbParameterCollection<NullParameter> parameters = new DbParameterCollection<NullParameter> ();

		public NullCommand ()
		{
		}

		public override string CommandText { get; set; }
		public override int CommandTimeout { get; set; }
		public override CommandType CommandType { get; set; }
		public override bool DesignTimeVisible { get; set; }
		public override UpdateRowSource UpdatedRowSource { get; set; }

		protected override DbConnection DbConnection { get; set; }
		protected override DbParameterCollection DbParameterCollection {get {return parameters;}}
		protected override DbTransaction DbTransaction { get; set; }

		public override void Cancel ()
		{
		}

		public override int ExecuteNonQuery ()
		{
			throw new NotSupportedException ();
		}

		public override object ExecuteScalar ()
		{
			throw new NotSupportedException ();
		}

		public override void Prepare ()
		{
		}

		protected override DbParameter CreateDbParameter ()
		{
			return new NullParameter ();
		}

		protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior)
		{
			throw new NotSupportedException ();
		}
	}
}

