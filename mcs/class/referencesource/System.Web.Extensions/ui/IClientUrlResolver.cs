//------------------------------------------------------------------------------
// <copyright file="IClientUrlResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;

    internal interface IClientUrlResolver {
        string AppRelativeTemplateSourceDirectory { get; }
        string ResolveClientUrl(string relativeUrl);
    }
}
