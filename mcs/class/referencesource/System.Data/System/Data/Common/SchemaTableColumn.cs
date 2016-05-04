//------------------------------------------------------------------------------
// <copyright file="SchemaTableColumn.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    // required columns that DataAdapter.Fill(Schema) will pay attention to
    // when constructing new DataColumns to add to a DataTable
    public static class SchemaTableColumn {
        
        public static readonly string ColumnName               = "ColumnName";
        public static readonly string ColumnOrdinal            = "ColumnOrdinal";
        public static readonly string ColumnSize               = "ColumnSize";
        public static readonly string NumericPrecision         = "NumericPrecision";
        public static readonly string NumericScale             = "NumericScale";

        public static readonly string DataType                 = "DataType";
        public static readonly string ProviderType             = "ProviderType";
        public static readonly string NonVersionedProviderType = "NonVersionedProviderType";
        
        public static readonly string IsLong                   = "IsLong";
        public static readonly string AllowDBNull              = "AllowDBNull";
        public static readonly string IsAliased                = "IsAliased";
        public static readonly string IsExpression             = "IsExpression";
        public static readonly string IsKey                    = "IsKey";
        public static readonly string IsUnique                 = "IsUnique";
        
        public static readonly string BaseSchemaName           = "BaseSchemaName";
        public static readonly string BaseTableName            = "BaseTableName";
        public static readonly string BaseColumnName           = "BaseColumnName";
    }    
}

