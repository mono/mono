//
// System.Data.SqlClient.DbEnumerator.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.Data;

namespace System.Data.Common {
	public class DbEnumerator : IEnumerator
	{
		#region Fields

		IDataReader reader;
		bool closeReader;
		SchemaInfo[] schema;
		FieldNameLookup lookup;
		int fieldCount;
	
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
			this.lookup = new FieldNameLookup ();
			this.fieldCount = 0;
			LoadSchema (reader.GetSchemaTable ());
		}

		#endregion // Constructors

		#region Properties

		public virtual object Current {
			get { 
				object[] values = new object[fieldCount];
				reader.GetValues (values);
				return new DbDataRecord (schema, values, lookup); 
			}
		}

		#endregion // Properties

		#region Methods

		public void LoadSchema (DataTable schemaTable)
		{
			ArrayList list = new ArrayList ();
			foreach (DataRow row in schemaTable.Rows) {
				SchemaInfo columnSchema = new SchemaInfo ();
				lookup.Add ((string) row["ColumnName"]);

				columnSchema.ColumnName = (string) row ["ColumnName"];
				columnSchema.ColumnOrdinal = (int) row ["ColumnOrdinal"];
				columnSchema.TableName = (string) row ["BaseTableName"];
				columnSchema.Nullable = (bool) row ["AllowDBNull"];
				columnSchema.Writable = ! (bool) row ["IsReadOnly"];

				if (row["NumericPrecision"] != DBNull.Value)
					columnSchema.NumericPrecision = (byte) row["NumericPrecision"];
				else
					columnSchema.NumericPrecision = (byte) 0;

				if (row["NumericScale"] != DBNull.Value)
					columnSchema.NumericScale = (byte) row["NumericScale"];
				else
					columnSchema.NumericScale = (byte) 0;
				list.Add (columnSchema);
				fieldCount += 1;
			}
			schema = (SchemaInfo[]) list.ToArray (typeof (SchemaInfo));
		}

		public virtual bool MoveNext ()
		{
			if (reader.Read ()) 
				return true;
			if (closeReader)
				reader.Close ();
			return false;
		}

		public virtual void Reset ()
		{
			throw new InvalidOperationException ("This enumerator can only go forward.");	
		}
		
		#endregion // Methods
	}
}
