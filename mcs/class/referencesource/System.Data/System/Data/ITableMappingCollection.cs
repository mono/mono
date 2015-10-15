//------------------------------------------------------------------------------
// <copyright file="ITableMappingCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    public interface ITableMappingCollection : System.Collections.IList {

        object this[string index] {
            get;
            set;
        }

        ITableMapping Add(string sourceTableName, string dataSetTableName);

        bool Contains(string sourceTableName);

        ITableMapping GetByDataSetTable(string dataSetTableName);

        int IndexOf(string sourceTableName);

        void RemoveAt(string sourceTableName);
    }
}
