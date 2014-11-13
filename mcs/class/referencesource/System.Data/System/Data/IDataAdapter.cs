//------------------------------------------------------------------------------
// <copyright file="IDataAdapter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    public interface IDataAdapter {

        MissingMappingAction MissingMappingAction {
            get;
            set;
        }

        MissingSchemaAction MissingSchemaAction {
            get;
            set;
        }

        ITableMappingCollection TableMappings {
            get;
        }

        DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType);

        int Fill(DataSet dataSet);

        IDataParameter[] GetFillParameters();

        int Update(DataSet dataSet);
    }
}
