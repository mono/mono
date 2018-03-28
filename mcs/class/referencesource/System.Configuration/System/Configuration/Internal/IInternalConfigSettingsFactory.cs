//------------------------------------------------------------------------------
// <copyright file="IInternalConfigSettingsFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Security;
using System.Collections.Specialized;
using System.Configuration;
using ClassConfiguration=System.Configuration.Configuration;

//
// This file contains most of the interfaces that allow System.Web, Venus, and 
// Whitehorse to customize configuration in some way.
//
// The goal of the design of customization is to only require other MS assemblies
// to create an instance of an internal object via Activator.CreateInstance(), and then
// use these objects through *public* System.Configuration.Internal interfaces. 
// We do not want extenders to have to use reflection to call a method - it is slow,
// not typesafe, and more difficult to promote correct use of the internal object.
//
namespace System.Configuration.Internal {

    //
    // Used by System.Web.Configuration.HttpConfigurationSystem to set the 
    // configuration system for the ConfigurationSettings class.
    //
    [System.Runtime.InteropServices.ComVisible(false)]
    public interface IInternalConfigSettingsFactory {
        void SetConfigurationSystem(IInternalConfigSystem internalConfigSystem, bool initComplete);
        void CompleteInit();
    }
}
