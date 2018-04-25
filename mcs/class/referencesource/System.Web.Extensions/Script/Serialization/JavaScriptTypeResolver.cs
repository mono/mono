//------------------------------------------------------------------------------
// <copyright file="JavaScriptTypeResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Serialization {
    using System;
    using System.Web;

    public abstract class JavaScriptTypeResolver {
        public abstract Type ResolveType(string id);
        public abstract string ResolveTypeId(Type type);
    }
}
