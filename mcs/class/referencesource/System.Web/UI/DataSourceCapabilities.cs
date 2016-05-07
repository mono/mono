//------------------------------------------------------------------------------
// <copyright file="DataSourceCapabilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {
    

    [Flags]
    public enum DataSourceCapabilities {

        None = 0x0,

        Sort = 0x1,

        Page = 0x2,

        RetrieveTotalRowCount = 0x4
    }
}
