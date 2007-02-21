//
// Mono.Data.Sqlite.SqliteParameter.cs
//
// Represents a parameter to a SqliteCommand, and optionally, its mapping to 
// DataSet columns.
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
#if !NET_2_0
using System;
using System.Data;

namespace Mono.Data.Sqlite
{
	public class SqliteParameter : IDbDataParameter
	{

		#region Fields
		
		private string name;
		private DbType type;
		private DbType originalType;
		private bool typeSet;
		private string source_column;
		private ParameterDirection direction;
		private DataRowVersion row_version;
		private object param_value;
		private byte precision;
		private byte scale;
		private int size;
		private bool isNullable;
		private bool sourceColumnNullMapping;
		
		#endregion

		#region Constructors and destructors
		
		public SqliteParameter ()
		{
			type = DbType.String;
			direction = ParameterDirection.Input;
			isNullable = true;
		}
		
		public SqliteParameter (string name, DbType type)
		{
			this.name = name;
			this.type = type;
			isNullable = true;
		}
		
		public SqliteParameter (string name, object value)
		{
			this.name = name;
			type = DbType.String;
			param_value = value;
			direction = ParameterDirection.Input;
			isNullable = true;
		}
		
		public SqliteParameter (string name, DbType type, int size) : this (name, type)
		{
			this.size = size;
		}
		
		public SqliteParameter (string name, DbType type, int size, string src_column) : this (name ,type, size)
		{
			source_column = src_column;
		}
		
		#endregion

		#region Properties
		
		public DbType DbType {
			get { return type; }
			set {
				if (!typeSet) {
					originalType = type;
					typeSet = true;
				}
				type = value;
				if (!typeSet)
					originalType = type;
			}
		}
	
		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}
	
		public bool IsNullable {
			get { return isNullable; }
		}

		public string ParameterName {
			get { return name; }
			set { name = value; }
		}
	
		public byte Precision {
			get { return precision; }
			set { precision = value; }
		}
		
		public byte Scale {
			get { return scale; }
			set { scale = value; }
		}

		public int Size {
			get { return size; }
			set { size = value; }
		}
		
		public string SourceColumn {
			get { return source_column; }
			set { source_column = value; }
		}

		public DataRowVersion SourceVersion {
			get { return row_version; }
			set { row_version = value; }
		}
		
		public object Value {
			get { return param_value; }
			set { param_value = value; }
		}
		
		#endregion
	}
}
#endif
