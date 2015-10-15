//------------------------------------------------------------------------------
// <copyright file="IDbDataParameter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    public interface IDbDataParameter : IDataParameter { // MDAC 68205

        byte Precision {
            get;
            set;
        }

        byte Scale {
            get;
            set;
        }

        int Size {
            get;
            set;
        }
    }
}
