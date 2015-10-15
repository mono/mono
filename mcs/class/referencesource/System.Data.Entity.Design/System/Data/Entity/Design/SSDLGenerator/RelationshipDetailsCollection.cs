//---------------------------------------------------------------------
// <copyright file="RelationshipDetailsCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml;
using System.Data.Common;
using System.Globalization;
using System.Data;

namespace System.Data.Entity.Design.SsdlGenerator
{
    /// <summary>
    /// Strongly type data table for holding the RelationshipDetails
    /// </summary>
    [Serializable]
    internal sealed class RelationshipDetailsCollection : System.Data.DataTable, System.Collections.IEnumerable
    {
        [NonSerialized]
        private System.Data.DataColumn _columnPKCatalog;
        [NonSerialized]
        private System.Data.DataColumn _columnPKSchema;
        [NonSerialized]
        private System.Data.DataColumn _columnPKTable;
        [NonSerialized]
        private System.Data.DataColumn _columnPKColumn;
        [NonSerialized]
        private System.Data.DataColumn _columnFKCatalog;
        [NonSerialized]
        private System.Data.DataColumn _columnFKSchema;
        [NonSerialized]
        private System.Data.DataColumn _columnFKTable;
        [NonSerialized]
        private System.Data.DataColumn _columnFKColumn;
        [NonSerialized]
        private System.Data.DataColumn _columnOrdinal;
        [NonSerialized]
        private System.Data.DataColumn _columnRelationshipName;
        [NonSerialized]
        private System.Data.DataColumn _columnRelationshipId;
        [NonSerialized]
        private System.Data.DataColumn _columnRelationshipIsCascadeDelete;

        /// <summary>
        /// Constructs a RelationsipDetailsDataTable
        /// </summary>
        public RelationshipDetailsCollection()
        {
            this.TableName = "RelationshipDetails";
            // had to remove these because of an fxcop violation
            //BeginInit();
            InitClass();
            //EndInit();
        }

        /// <summary>
        /// Constructs a new instance RelationshipDetailDataTable with a given SerializationInfo and StreamingContext
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        internal RelationshipDetailsCollection(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            UpdateMemberFieldsAfterDeserialization();
        }


        /// <summary>
        /// Gets the PkCatalog column
        /// </summary>
        public System.Data.DataColumn PKCatalogColumn
        {
            get
            {
                return this._columnPKCatalog;
            }
        }


        /// <summary>
        /// Gets the PkSchema column
        /// </summary>
        public System.Data.DataColumn PKSchemaColumn
        {
            get
            {
                return this._columnPKSchema;
            }
        }


        /// <summary>
        /// Gets the PkTable column
        /// </summary>
        public System.Data.DataColumn PKTableColumn
        {
            get
            {
                return this._columnPKTable;
            }
        }


        /// <summary>
        /// Gets the PkColumn column
        /// </summary>
        public System.Data.DataColumn PKColumnColumn
        {
            get
            {
                return this._columnPKColumn;
            }
        }


        /// <summary>
        /// Gets the FkCatalog column
        /// </summary>
        public System.Data.DataColumn FKCatalogColumn
        {
            get
            {
                return this._columnFKCatalog;
            }
        }


        /// <summary>
        /// Gets the FkSchema column
        /// </summary>
        public System.Data.DataColumn FKSchemaColumn
        {
            get
            {
                return this._columnFKSchema;
            }
        }

        /// <summary>
        /// Gets the FkTable column
        /// </summary>
        public System.Data.DataColumn FKTableColumn
        {
            get
            {
                return this._columnFKTable;
            }
        }

        /// <summary>
        /// Gets the FkColumn column
        /// </summary>
        public System.Data.DataColumn FKColumnColumn
        {
            get
            {
                return this._columnFKColumn;
            }
        }


        /// <summary>
        /// Gets the Ordinal column
        /// </summary>
        public System.Data.DataColumn OrdinalColumn
        {
            get
            {
                return this._columnOrdinal;
            }
        }


        /// <summary>
        /// Gets the RelationshipName column
        /// </summary>
        public System.Data.DataColumn RelationshipNameColumn
        {
            get
            {
                return this._columnRelationshipName;
            }
        }

        public System.Data.DataColumn RelationshipIdColumn
        {
            get
            {
                return this._columnRelationshipId;
            }
        }

        /// <summary>
        /// Gets the IsCascadeDelete value
        /// </summary>
        public System.Data.DataColumn RelationshipIsCascadeDeleteColumn
        {
            get
            {
                return this._columnRelationshipIsCascadeDelete;
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
            return new RelationshipDetailsCollection();
        }

        private const string PK_CATALOG = "PkCatalog";
        private const string PK_SCHEMA = "PkSchema";
        private const string PK_TABLE = "PkTable";
        private const string PK_COLUMN = "PkColumn";
        private const string FK_CATALOG = "FkCatalog";
        private const string FK_SCHEMA = "FkSchema";
        private const string FK_TABLE = "FkTable";
        private const string FK_COLUMN = "FkColumn";
        private const string ORDINAL = "Ordinal";
        private const string RELATIONSHIP_NAME = "RelationshipName";
        private const string RELATIONSHIP_ID = "RelationshipId";
        private const string RELATIONSHIP_IsCascadeDelete = "IsCascadeDelete";

        private void InitClass()
        {
            this._columnPKCatalog = new System.Data.DataColumn(PK_CATALOG, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnPKCatalog);
            this._columnPKSchema = new System.Data.DataColumn(PK_SCHEMA, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnPKSchema);
            this._columnPKTable = new System.Data.DataColumn(PK_TABLE, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnPKTable);
            this._columnPKColumn = new System.Data.DataColumn(PK_COLUMN, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnPKColumn);
            this._columnFKCatalog = new System.Data.DataColumn(FK_CATALOG, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnFKCatalog);
            this._columnFKSchema = new System.Data.DataColumn(FK_SCHEMA, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnFKSchema);
            this._columnFKTable = new System.Data.DataColumn(FK_TABLE, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnFKTable);
            this._columnFKColumn = new System.Data.DataColumn(FK_COLUMN, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnFKColumn);
            this._columnOrdinal = new System.Data.DataColumn(ORDINAL, typeof(int), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnOrdinal);
            this._columnRelationshipName = new System.Data.DataColumn(RELATIONSHIP_NAME, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnRelationshipName);
            this._columnRelationshipId = new System.Data.DataColumn(RELATIONSHIP_ID, typeof(string), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnRelationshipId);
            this._columnRelationshipIsCascadeDelete = new System.Data.DataColumn(RELATIONSHIP_IsCascadeDelete, typeof(bool), null, System.Data.MappingType.Element);
            base.Columns.Add(this._columnRelationshipIsCascadeDelete);
        }

        private void UpdateMemberFieldsAfterDeserialization()
        {
            this._columnPKCatalog = base.Columns[PK_CATALOG];
            this._columnPKSchema = base.Columns[PK_SCHEMA];
            this._columnPKTable = base.Columns[PK_TABLE];
            this._columnPKColumn = base.Columns[PK_COLUMN];
            this._columnFKCatalog = base.Columns[FK_CATALOG];
            this._columnFKSchema = base.Columns[FK_SCHEMA];
            this._columnFKTable = base.Columns[FK_TABLE];
            this._columnFKColumn = base.Columns[FK_COLUMN];
            this._columnOrdinal = base.Columns[ORDINAL];
            this._columnRelationshipName = base.Columns[RELATIONSHIP_NAME];
            this._columnRelationshipId = base.Columns[RELATIONSHIP_ID];
            this._columnRelationshipIsCascadeDelete = base.Columns[RELATIONSHIP_IsCascadeDelete];
        }

        /// <summary>
        /// Create a new row from a DataRowBuilder object.
        /// </summary>
        /// <param name="builder">The builder to create the row from.</param>
        /// <returns>The row that was created.</returns>
        protected override System.Data.DataRow NewRowFromBuilder(System.Data.DataRowBuilder builder)
        {
            return new RelationshipDetailsRow(builder);
        }


        /// <summary>
        /// Gets the Type that this row is.
        /// </summary>
        /// <returns>The type of this row.</returns>
        protected override System.Type GetRowType()
        {
            return typeof(RelationshipDetailsRow);
        }
    }

}
