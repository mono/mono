//
// Mono.Data.SqliteClient.SqliteParameterCollection.cs
//
// Represents a collection of parameters relevant to a SqliteCommand as well as 
// their respective mappings to columns in a DataSet.
//
// Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//            Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//
// Copyright (C) 2002  Vladimir Vukicevic
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
using System.Data;
using System.Collections;

namespace Mono.Data.SqliteClient
{
	public class SqliteParameterCollection : IDataParameterCollection, IList
	{
	
		#region Fields
		
		ArrayList numeric_param_list = new ArrayList();
		Hashtable named_param_hash = new Hashtable();
		
		#endregion

		#region Private Methods
		
		private void CheckSqliteParam (object value)
		{
			if (!(value is SqliteParameter))
				throw new InvalidCastException("Can only use SqliteParameter objects");
		}

		private void RecreateNamedHash ()
		{
			for (int i = 0; i < numeric_param_list.Count; i++) {
				named_param_hash[((SqliteParameter) numeric_param_list[i]).ParameterName] = i;
			}
		}
		
		#endregion

		#region Properties
		
		object IList.this[int index] {
			get {
				return this[index];
			}
			set {
				CheckSqliteParam (value);
				this[index] = (SqliteParameter) value;
			}
		}
		
		object IDataParameterCollection.this[string parameterName] {
			get {
				return this[parameterName];
			}
			set {
				CheckSqliteParam (value);
				this[parameterName] = (SqliteParameter) value;
			}
		}
		
		public SqliteParameter this[string parameterName] {
			get {
				return this[(int) named_param_hash[parameterName]];
			}
			set {
				if (this.Contains (parameterName))
					numeric_param_list[(int) named_param_hash[parameterName]] = value;
				else          // uhm, do we add it if it doesn't exist? what does ms do?
					Add (value);
			}
		}
		
		public SqliteParameter this[int parameterIndex] {
			get {
				return (SqliteParameter) numeric_param_list[parameterIndex];
			}
			set {
				numeric_param_list[parameterIndex] = value;
			}
		}
		
		public int Count {
			get { return numeric_param_list.Count; }
		}
		
		public bool IsFixedSize {
			get { return false; }
		}
		
		public bool IsReadOnly {
			get { return false; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public object SyncRoot {
			get { return null; }
		}
		
		#endregion

		#region Public Methods
		
		public int Add (object value)
		{
			CheckSqliteParam (value);
			SqliteParameter sqlp = (SqliteParameter) value;
			if (named_param_hash.Contains (sqlp.ParameterName))
				throw new DuplicateNameException ("Parameter collection already contains given value.");
			
			named_param_hash[value] = numeric_param_list.Add (value);
			
			return (int) named_param_hash[value];
		}
		
		public SqliteParameter Add (SqliteParameter param)
		{
			Add ((object)param);
			return param;
		}
		
		public SqliteParameter Add (string name, object value)
		{
			return Add (new SqliteParameter (name, value));
		}
		
		public SqliteParameter Add (string name, DbType type)
		{
			return Add (new SqliteParameter (name, type));
		}
		
		public void Clear ()
		{
			numeric_param_list.Clear ();
			named_param_hash.Clear ();
		}
		
		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}
		
		bool IList.Contains (object value)
		{
			return Contains ((SqliteParameter) value);
		}
		
		public bool Contains (string parameterName)
		{
			return named_param_hash.Contains (parameterName);
		}
		
		public bool Contains (SqliteParameter param)
		{
			return Contains (param.ParameterName);
		}
		
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		int IList.IndexOf (object param)
		{
			return IndexOf ((SqliteParameter) param);
		}
		
		public int IndexOf (string parameterName)
		{
			return (int) named_param_hash[parameterName];
		}
		
		public int IndexOf (SqliteParameter param)
		{
			return IndexOf (param.ParameterName);
		}
		
		public void Insert (int index, object value)
		{
			CheckSqliteParam (value);
			if (numeric_param_list.Count == index) {
				Add (value);
				return;
			}
			
			numeric_param_list.Insert (index, value);
			RecreateNamedHash ();
		}
		
		public void Remove (object value)
		{
			CheckSqliteParam (value);
			RemoveAt ((SqliteParameter) value);
		}
		
		public void RemoveAt (int index)
		{
			RemoveAt (((SqliteParameter) numeric_param_list[index]).ParameterName);
		}
		
		public void RemoveAt (string parameterName)
		{
			if (!named_param_hash.Contains (parameterName))
				throw new ApplicationException ("Parameter " + parameterName + " not found");
			
			numeric_param_list.RemoveAt ((int) named_param_hash[parameterName]);
			named_param_hash.Remove (parameterName);
			
			RecreateNamedHash ();
		}
		
		public void RemoveAt (SqliteParameter param)
		{
			RemoveAt (param.ParameterName);
		}
		
		#endregion
	}
}
                
