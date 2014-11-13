//------------------------------------------------------------------------------
// <copyright file="IColumnMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    public interface IColumnMapping {

        string DataSetColumn {
            get;
            set;
        }

        string SourceColumn {
            get;
            set;
        }
    }
}
