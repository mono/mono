//------------------------------------------------------------------------------
// <copyright file="Groupbybehavior.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    public enum GroupByBehavior { 
        Unknown        = 0,
        NotSupported   = 1,
        Unrelated      = 2,
        MustContainAll = 3,
        ExactMatch     = 4        
    }
}

