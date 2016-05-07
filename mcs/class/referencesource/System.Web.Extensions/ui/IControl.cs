//------------------------------------------------------------------------------
// <copyright file="IControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;

    internal interface IControl : IClientUrlResolver {
        HttpContextBase Context {
            get;
        }
        bool DesignMode {
            get;
        }
    }
}
