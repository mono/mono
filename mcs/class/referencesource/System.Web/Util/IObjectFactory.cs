//------------------------------------------------------------------------------
// <copyright file="IObjectFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {

    using System;

    public interface IWebObjectFactory {
        object CreateInstance();
    }

    internal interface ITypedWebObjectFactory : IWebObjectFactory {
        // Type that will be instantiated by CreateInstance.  This is to allow the caller
        // to check base type validity *before* actually creating the instance.
        Type InstantiatedType { get; }
    }
}


