//
// Mono.Data.Sqlite.SqliteParameterCollection.cs
//
// Represents a collection of parameters relevant to a SqliteCommand as well as 
// their respective mappings to columns in a DataSet.
//
//Author(s):		Vladimir Vukicevic  <vladimir@pobox.com>
//			Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//			Chris Turchin <chris@turchin.net>
//			Jeroen Zwartepoorte <jeroen@xs4all.nl>
//			Thomas Zoechling <thomas.zoechling@gmx.at>
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
#if !NET_2_0
using System;
using System.Data;
using System.Collections;

namespace Mono.Data.Sqlite
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
				throw new InvalidCastException ("Can only use SqliteParameter objects");
			SqliteParameter sqlp = value as SqliteParameter;
			if (sqlp.ParameterName == null || sqlp.ParameterName.Length == 0)
				sqlp.ParameterName = this.GenerateParameterName();
                 }

		private void RecreateNamedHash ()
		{
			for (int i = 0; i < numeric_param_list.Count; i++) 
			{
				named_param_hash[((SqliteParameter) numeric_param_list[i]).ParameterName] = i;
			}
		}

		//FIXME: if the user is calling Insert at various locations with unnamed parameters, this is not going to work....
		private string GenerateParameterName()
		{
			int		index	= this.Count + 1;
			string	name	= String.Empty;

			while (index > 0)
			{
				name = ":" + index.ToString();
					if (this.IndexOf(name) == -1)
					index = -1;
				else
				index++;
			}
			return name;
		}

		#endregion

		#region Properties
		
		object IList.this[int index] {
			get 
			{
				return this[index];
			}
			set 
			{
				CheckSqliteParam (value);
				this[index] = (SqliteParameter) value;
			}
		}

		object IDataParameterCollection.this[string parameterName] {
			get 
			{
				return this[parameterName];
			}
			set 
			{
				CheckSqliteParam (value);
				this[parameterName] = (SqliteParameter) value;
			}
		}
		
		private bool isPrefixed (string parameterName)
		{
			return parameterName.Length > 1 && (parameterName[0] == ':' || parameterName[0] == '$');
		}

		SqliteParameter GetParameter (int parameterIndex)
		{
			if (this.Count >= parameterIndex+1)
				return (SqliteParameter) numeric_param_list[parameterIndex];
			else          
				throw new IndexOutOfRangeException("The specified parameter index does not exist: " + parameterIndex.ToString());
		}

		SqliteParameter GetParameter (string parameterName)
		{
			if (this.Contains(parameterName))
				return this[(int) named_param_hash[parameterName]];
			else if (isPrefixed(parameterName) && this.Contains(parameterName.Substring(1)))
				return this[(int) named_param_hash[parameterName.Substring(1)]];
			else
				throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
		}

		void SetParameter (int parameterIndex, SqliteParameter parameter)
		{
			if (this.Count >= parameterIndex+1)
				numeric_param_list[parameterIndex] = parameter;
			else          
				throw new IndexOutOfRangeException("The specified parameter index does not exist: " + parameterIndex.ToString());
		}

		void SetParameter (string parameterName, SqliteParameter parameter)
		{
			if (this.Contains(parameterName))
				numeric_param_list[(int) named_param_hash[parameterName]] = parameter;
			else if (parameterName.Length > 1 && this.Contains(parameterName.Substring(1)))
				numeric_param_list[(int) named_param_hash[parameterName.Substring(1)]] = parameter;
			else
				throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
		}

		public SqliteParameter this[string parameterName] 
		{
			get { return GetParameter (parameterName); }
			set { SetParameter (parameterName, value); }
		}

		public SqliteParameter this[int parameterIndex]
		{
			get { return GetParameter (parameterIndex); }
			set { SetParameter (parameterIndex, value); }
		}

		public int Count {
			get { return this.numeric_param_list.Count; }
		}

		bool IList.IsFixedSize
		{
			get
			{
				return this.numeric_param_list.IsFixedSize;
			}
		}

		bool IList.IsReadOnly {
			get { return this.numeric_param_list.IsReadOnly; }
		}


		bool ICollection.IsSynchronized {
			get { return this.numeric_param_list.IsSynchronized; }
		}		

		object ICollection.SyncRoot {
			get { return this.numeric_param_list.SyncRoot; }
		}

		#endregion

		#region Public Methods

		public int Add (object value)
		{
			CheckSqliteParam (value);
			SqliteParameter sqlp = value as SqliteParameter;
			if (named_param_hash.Contains (sqlp.ParameterName))
				throw new DuplicateNameException ("Parameter collection already contains the a SqliteParameter with the given ParameterName.");
			named_param_hash[sqlp.ParameterName] = numeric_param_list.Add(value);
				return (int) named_param_hash[sqlp.ParameterName];
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
			this.numeric_param_list.CopyTo(array, index);
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
			return this.numeric_param_list.GetEnumerator();
		}
		
		int IList.IndexOf (object param)
		{
			return IndexOf ((SqliteParameter) param);
		}
		
		public int IndexOf (string parameterName)
		{
			if (isPrefixed (parameterName)){
				string sub = parameterName.Substring (1);
				if (named_param_hash.Contains(sub))
					return (int) named_param_hash [sub];
			}
			if (named_param_hash.Contains(parameterName))
				return (int) named_param_hash[parameterName];
			else 
				return -1;
		}
		
		public int IndexOf (SqliteParameter param)
		{
			return IndexOf (param.ParameterName);
		}
		
		public void Insert (int index, object value)
		{
			CheckSqliteParam (value);
			if (numeric_param_list.Count == index) 
			{
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
#endif
