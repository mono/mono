//
// System.Data.SqlClient.DbEnumerator.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
	public class DbEnumerator : IEnumerator
	{
		#region Fields

		readonly IDataReader reader;
		readonly bool closeReader;
		readonly SchemaInfo [] schema;
	
		#endregion // Fields

		#region Constructors

		public DbEnumerator (IDataReader reader) 
			: this (reader, false)
		{
		}

		public DbEnumerator (IDataReader reader, bool closeReader)
		{
			this.reader = reader;
			this.closeReader = closeReader;
			this.schema = LoadSchema (reader);
		}

		#endregion // Constructors

		#region Properties

		public object Current {
			get {
				// DbDataRecordImpl does not do copy of the array
				// and MoveNext would overwrite any previously filled data
				var values = new object [reader.FieldCount];
				reader.GetValues (values);

				// TODO: Should not allocate on every property call
				return new DbDataRecordImpl (schema, values); 
			}
		}

		#endregion // Properties

		#region Methods

		private static SchemaInfo[] LoadSchema (IDataReader reader)
		{
			int fieldCount = reader.FieldCount;
			SchemaInfo[] schema = new SchemaInfo [fieldCount];

			for(int i=0; i < fieldCount; i++) {
				SchemaInfo columnSchema = new SchemaInfo ();

				columnSchema.ColumnName = reader.GetName(i);
				columnSchema.ColumnOrdinal = i; 
				columnSchema.DataTypeName = reader.GetDataTypeName (i);
				columnSchema.FieldType = reader.GetFieldType (i);

				schema [i] = columnSchema;
			}

			return schema;
		}

		public bool MoveNext ()
		{
			if (reader.Read ()) 
				return true;
			if (closeReader)
				reader.Close ();
			return false;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void Reset ()
		{
			throw new NotSupportedException ();
		}
		
		#endregion // Methods
	}
}
