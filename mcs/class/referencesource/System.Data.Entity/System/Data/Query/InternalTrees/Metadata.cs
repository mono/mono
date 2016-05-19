//---------------------------------------------------------------------
// <copyright file="Metadata.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Diagnostics;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// Describes metadata about a table
    /// </summary>
    internal class TableMD
    {
        private List<ColumnMD> m_columns;
        private List<ColumnMD> m_keys;

        private EntitySetBase m_extent; // null for transient tables
        private bool m_flattened;

        /// <summary>
        /// private initializer
        /// </summary>
        /// <param name="extent">the entity set corresponding to this table (if any)</param>
        private TableMD(EntitySetBase extent)
        {
            m_columns = new List<ColumnMD>();
            m_keys = new List<ColumnMD>();
            m_extent = extent;
        }

        /// <summary>
        /// Create a typed-table definition corresponding to an entityset (if specified)
        /// 
        /// The table has exactly one column - the type of the column is specified by 
        /// the "type" parameter. This table is considered to be un-"flattened"
        /// </summary>
        /// <param name="type">type of each element (row) of the table</param>
        /// <param name="extent">entityset corresponding to the table (if any)</param>
        internal TableMD(TypeUsage type, EntitySetBase extent)
            : this(extent)
        {
            m_columns.Add(new ColumnMD(this, "element", type));
            m_flattened = !PlanCompiler.TypeUtils.IsStructuredType(type);
        }

        /// <summary>
        /// Creates a "flattened" table definition. 
        /// 
        /// The table has one column for each specified property in the "properties" parameter. 
        /// The name and datatype of each table column are taken from the corresponding property.
        /// 
        /// The keys of the table (if any) are those specified in the "keyProperties" parameter
        /// 
        /// The table may correspond to an entity set (if the entityset parameter was non-null)
        /// </summary>
        /// <param name="properties">prperties corresponding to columns of the table</param>
        /// <param name="keyProperties"></param>
        /// <param name="extent">entityset corresponding to the table (if any)</param>
        internal TableMD(IEnumerable<EdmProperty> properties, IEnumerable<EdmMember> keyProperties,
            EntitySetBase extent)
            : this(extent)
        {
            Dictionary<string, ColumnMD> columnMap = new Dictionary<string, ColumnMD>();
            m_flattened = true;

            foreach (EdmProperty p in properties)
            {
                ColumnMD newColumn = new ColumnMD(this, p);
                m_columns.Add(newColumn);
                columnMap[p.Name] = newColumn;
            }
            foreach (EdmMember p in keyProperties)
            {
                ColumnMD keyColumn;
                if (!columnMap.TryGetValue(p.Name, out keyColumn))
                {
                    Debug.Assert(false, "keyMember not in columns?");
                }
                else
                {
                    m_keys.Add(keyColumn);
                }
            }
        }

        /// <summary>
        /// The extent metadata (if any)
        /// </summary>
        internal EntitySetBase Extent { get { return m_extent; } }

        /// <summary>
        /// List of columns of this table
        /// </summary>
        internal List<ColumnMD> Columns { get { return m_columns; } }

        /// <summary>
        /// Keys for this table
        /// </summary>
        internal List<ColumnMD> Keys { get { return m_keys; } }

        /// <summary>
        /// Is this table a "flat" table?
        /// </summary>
        internal bool Flattened { get { return m_flattened; } }

        /// <summary>
        /// String form - for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (m_extent != null ? m_extent.Name : "Transient");
        }
    }

    /// <summary>
    /// Describes information about each column
    /// </summary>
    internal class ColumnMD
    {
        private string m_name;
        private TypeUsage m_type;
        private EdmMember m_property;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="table">Table containing this column</param>
        /// <param name="name">Column name</param>
        /// <param name="type">Datatype of the column</param>
        internal ColumnMD(TableMD table, string name, TypeUsage type)
        {
            m_name = name;
            m_type = type;
        }

        /// <summary>
        /// More useful default constructor
        /// </summary>
        /// <param name="table">table containing this column</param>
        /// <param name="property">property describing this column</param>
        internal ColumnMD(TableMD table, EdmMember property)
            : this(table, property.Name, property.TypeUsage)
        {
            m_property = property;
        }

        /// <summary>
        /// Column Name
        /// </summary>
        internal string Name { get { return m_name; } }

        /// <summary>
        /// Datatype of the column
        /// </summary>
        internal TypeUsage Type { get { return m_type; } }

        /// <summary>
        /// Is this column nullable ?
        /// </summary>
        internal bool IsNullable 
        {
            get
            {
                return (m_property == null) || TypeSemantics.IsNullable(m_property);
            }
        }

        /// <summary>
        /// debugging help
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_name;
        }
    }

    /// <summary>
    /// Represents one instance of a table. Contains the table metadata
    /// </summary>
    internal class Table
    {
        private TableMD m_tableMetadata;
        private VarList m_columns;
        private VarVec m_referencedColumns;
        private VarVec m_keys;
        private VarVec m_nonnullableColumns;
        private int m_tableId;

        internal Table(Command command, TableMD tableMetadata, int tableId)
        {
            m_tableMetadata = tableMetadata;
            m_columns = Command.CreateVarList();
            m_keys = command.CreateVarVec();
            m_nonnullableColumns = command.CreateVarVec();
            m_tableId = tableId;

            Dictionary<string, ColumnVar> columnVarMap = new Dictionary<string, ColumnVar>();
            foreach (ColumnMD c in tableMetadata.Columns)
            {
                ColumnVar v = command.CreateColumnVar(this, c);
                columnVarMap[c.Name] = v;
                if (!c.IsNullable)
                {
                    m_nonnullableColumns.Set(v);
                }
            }

            foreach (ColumnMD c in tableMetadata.Keys)
            {
                ColumnVar v = columnVarMap[c.Name];
                m_keys.Set(v);
            }

            m_referencedColumns = command.CreateVarVec(m_columns);
        }

        /// <summary>
        /// Metadata for the table instance
        /// </summary>
        internal TableMD TableMetadata { get { return m_tableMetadata; } }

        /// <summary>
        /// List of column references
        /// </summary>
        internal VarList Columns { get { return m_columns; } }

        /// <summary>
        /// Get the list of all referenced columns. 
        /// </summary>
        internal VarVec ReferencedColumns
        {
            get { return m_referencedColumns; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal VarVec NonNullableColumns { get { return m_nonnullableColumns; } }

        /// <summary>
        /// List of keys
        /// </summary>
        internal VarVec Keys { get { return m_keys; } }

        /// <summary>
        /// (internal) id for this table instance
        /// </summary>
        internal int TableId { get { return m_tableId; } }

        /// <summary>
        /// String form - for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}::{1}", m_tableMetadata.ToString(), this.TableId); ;
        }
    }
}
