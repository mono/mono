//---------------------------------------------------------------------
// <copyright file="TableDetailsCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml;
using System.Data.Common;
using System.Globalization;
using System.Data;

namespace System.Data.Entity.Design.SsdlGenerator
{
    /// <summary>
    /// Strongly typed DataTable for TableDetails
    /// </summary>
    [Serializable]
    internal sealed class TableDetailsCollection : System.Data.DataTable, System.Collections.IEnumerable
    {
        [NonSerialized]
        private System.Data.DataColumn _columnCatalog;
        [NonSerialized]
        private System.Data.DataColumn _columnSchema;
        [NonSerialized]
        private System.Data.DataColumn _columnTable;
        [NonSerialized]
        private System.Data.DataColumn _columnFieldOrdinal;
        [NonSerialized]
        private System.Data.DataColumn _columnFieldColumn;
        [NonSerialized]
        private System.Data.DataColumn _columnIsNullable;
        [NonSerialized]
        private System.Data.DataColumn _columnDataType;
        [NonSerialized]
        private System.Data.DataColumn _columnMaximumLength;
        [NonSerialized]
        private System.Data.DataColumn _columnDateTimePrecision;
        [NonSerialized]
        private System.Data.DataColumn _columnPrecision;
        [NonSerialized]
        private System.Data.DataColumn _columnScale;
        [NonSerialized]
        private System.Data.DataColumn _columnIsIdentity;
        [NonSerialized]
        private System.Data.DataColumn _columnIsServerGenerated;
        [NonSerialized]
        private System.Data.DataColumn _columnIsPrimaryKey;

        /// <summary>
        /// Constructs a TableDetailsDataTable
        /// </summary>
        public TableDetailsCollection()
        {
            this.TableName = "TableDetails";
            // had to remove these because of an fxcop violation
            //BeginInit();
            InitClass();
            //EndInit();
        }

        /// <summary>
        /// Constructs a new instance TableDetailsDataTable with a given SerializationInfo and StreamingContext
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        internal TableDetailsCollection(System.Runtime.Serialization.SerializationInfo serializationInfo,
            System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            UpdateMemberFieldsAfterDeserialization();
        }

        /// <summary>
        /// Gets the Catalog column
        /// </summary>
        public System.Data.DataColumn CatalogColumn
        {
            get
            {
                return this._columnCatalog;
            }
        }

        /// <summary>
        /// Gets the Schema column
        /// </summary>
        public System.Data.DataColumn SchemaColumn
        {
            get
            {
                return this._columnSchema;
            }
        }

        /// <summary>
        /// Gets the TableName column
        /// </summary>
        public System.Data.DataColumn TableNameColumn
        {
            get
            {
                return this._columnTable;
            }
        }

        /// <summary>
        /// Gets the ColumnName column
        /// </summary>
        public System.Data.DataColumn ColumnNameColumn
        {
            get
            {
                return this._columnFieldColumn;
            }
        }

        /// <summary>
        /// Gets the IsNullable column
        /// </summary>
        public System.Data.DataColumn IsNullableColumn
        {
            get
            {
                return this._columnIsNullable;
            }
        }

        /// <summary>
        /// Gets the DataType column
        /// </summary>
        public System.Data.DataColumn DataTypeColumn
        {
            get
            {
                return this._columnDataType;
            }
        }

        /// <summary>
        /// Gets the MaximumLength column
        /// </summary>
        public System.Data.DataColumn MaximumLengthColumn
        {
            get
            {
                return this._columnMaximumLength;
            }
        }

        /// <summary>
        /// Gets the Precision column
        /// </summary>
        public System.Data.DataColumn PrecisionColumn
        {
            get
            {
                return this._columnPrecision;
            }
        }

        /// <summary>
        /// Gets the Precision column
        /// </summary>
        public System.Data.DataColumn DateTimePrecisionColumn
        {
            get
            {
                return this._columnDateTimePrecision;
            }
        }

        /// <summary>
        /// Gets the Scale column
        /// </summary>
        public System.Data.DataColumn ScaleColumn
        {
            get
            {
                return this._columnScale;
            }
        }

        /// <summary>
        /// Gets the IsIdentityColumn column
        /// </summary>
        public System.Data.DataColumn IsIdentityColumn
        {
            get
            {
                return this._columnIsIdentity;
            }
        }

         /// <summary>
        /// Gets the IsIdentityColumn column
        /// </summary>
        public System.Data.DataColumn IsServerGeneratedColumn
        {
            get
            {
                return this._columnIsServerGenerated;
            }
        }

       /// <summary>
        /// Gets the IsPrimaryKey column
        /// </summary>
        public System.Data.DataColumn IsPrimaryKeyColumn
        {
            get
            {
                return this._columnIsPrimaryKey;
            }
        }

        /// <summary>
        /// Gets an enumerator over the rows.
        /// </summary>
        /// <returns>The row enumerator</returns>
        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.Rows.GetEnumerator();
        }

        /// <summary>
        /// Creates an instance of this table
        /// </summary>
        /// <returns>The newly created instance.</returns>
        protected override System.Data.DataTable CreateInstance()
        {
            return new TableDetailsCollection();
        }

        private const string CATALOG = "CatalogName";
        private const string SCHEMA = "SchemaName";
        private const string TABLE = "TableName";
        private const string COLUMN = "ColumnName";
        private const string ORDINAL = "Ordinal";
        private const string IS_NULLABLE = "IsNullable";
        private const string DATA_TYPE = "DataType";
        private const string MAX_LENGTH = "MaximumLength";
        private const string PRECISION = "Precision";
        private const string DATETIMEPRECISION = "DateTimePrecision";
        private const string SCALE = "Scale";
        private const string IS_IDENTITY = "IsIdentity";
        private const string IS_SERVERGENERATED = "IsServerGenerated";
        private const string IS_PRIMARYKEY = "IsPrimaryKey";

        private void InitClass()
        {
            this._columnCatalog = new System.Data.DataColumn(CATALOG, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnCatalog);
            this._columnSchema = new System.Data.DataColumn(SCHEMA, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnSchema);
            this._columnTable = new System.Data.DataColumn(TABLE, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnTable);
            this._columnFieldColumn = new System.Data.DataColumn(COLUMN, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnFieldColumn);
            this._columnFieldOrdinal = new System.Data.DataColumn(ORDINAL, typeof(int), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnFieldOrdinal);
            this._columnIsNullable = new System.Data.DataColumn(IS_NULLABLE, typeof(bool), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnIsNullable);
            this._columnDataType = new System.Data.DataColumn(DATA_TYPE, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnDataType);
            this._columnMaximumLength = new System.Data.DataColumn(MAX_LENGTH, typeof(int), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnMaximumLength);
            this._columnPrecision = new System.Data.DataColumn(PRECISION, typeof(int), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnPrecision);
            this._columnDateTimePrecision = new System.Data.DataColumn(DATETIMEPRECISION, typeof(int), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnDateTimePrecision);
            this._columnScale = new System.Data.DataColumn(SCALE, typeof(int), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnScale);
            this._columnIsIdentity = new System.Data.DataColumn(IS_IDENTITY, typeof(bool), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnIsIdentity);
            this._columnIsServerGenerated = new System.Data.DataColumn(IS_SERVERGENERATED, typeof(bool), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnIsServerGenerated);
            this._columnIsPrimaryKey = new System.Data.DataColumn(IS_PRIMARYKEY, typeof(bool), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnIsPrimaryKey);
        }

        private void UpdateMemberFieldsAfterDeserialization()
        {
            this._columnCatalog = base.Columns[CATALOG];
            this._columnSchema = base.Columns[SCHEMA];
            this._columnTable = base.Columns[TABLE];
            this._columnFieldColumn = base.Columns[COLUMN];
            this._columnFieldOrdinal = base.Columns[ORDINAL];
            this._columnIsNullable = base.Columns[IS_NULLABLE];
            this._columnDataType = base.Columns[DATA_TYPE];
            this._columnMaximumLength = base.Columns[MAX_LENGTH];
            this._columnPrecision = base.Columns[PRECISION];
            this._columnDateTimePrecision = base.Columns[DATETIMEPRECISION];
            this._columnScale = base.Columns[SCALE];
            this._columnIsIdentity = base.Columns[IS_IDENTITY];
            this._columnIsServerGenerated = base.Columns[IS_SERVERGENERATED];
            this._columnIsPrimaryKey = base.Columns[IS_PRIMARYKEY];
        }

        /// <summary>
        /// Create a new row from a DataRowBuilder object.
        /// </summary>
        /// <param name="builder">The builder to create the row from.</param>
        /// <returns>The row that was created.</returns>
        protected override System.Data.DataRow NewRowFromBuilder(System.Data.DataRowBuilder builder)
        {
            return new TableDetailsRow(builder);
        }

        /// <summary>
        /// Gets the Type that this row is.
        /// </summary>
        /// <returns>The type of this row.</returns>
        protected override System.Type GetRowType()
        {
            return typeof(TableDetailsRow);
        }

  }
}
