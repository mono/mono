//------------------------------------------------------------------------------
// <copyright file="IDataParameter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    public interface IDataParameter {

        DbType DbType {
            get;
            set;
        }

        ParameterDirection Direction {
            get;
            set;
        }

         Boolean IsNullable {
            get;
        }

        String ParameterName {
            get;
            set;
        }

        String SourceColumn {
            get;
            set;
        }

        DataRowVersion SourceVersion {
            get;
            set;
        }

        object Value {
            get;
            set;
        }
    }
}
