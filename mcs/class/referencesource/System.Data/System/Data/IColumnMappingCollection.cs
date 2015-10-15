//------------------------------------------------------------------------------
// <copyright file="IColumnMappingCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    public interface IColumnMappingCollection : System.Collections.IList {

        object this[string index] {
            get;
            set;
        }

        IColumnMapping Add(string sourceColumnName, string dataSetColumnName);

        bool Contains(string sourceColumnName);

        IColumnMapping GetByDataSetColumn(string dataSetColumnName);

        int IndexOf(string sourceColumnName);

        void RemoveAt(string sourceColumnName);
    }
}
