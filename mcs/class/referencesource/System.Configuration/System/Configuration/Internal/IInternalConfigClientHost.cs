//------------------------------------------------------------------------------
// <copyright file="IInternalConfigClientHost.cs" company="Microsoft">
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
    // Provide support to Whitehorse for manipulating the configuration path, 
    // so the details of the path do not have to be known by Whitehorse.
    //
    [System.Runtime.InteropServices.ComVisible(false)]
    public interface IInternalConfigClientHost {
        bool IsExeConfig(string configPath);
        bool IsRoamingUserConfig(string configPath);
        bool IsLocalUserConfig(string configPath);
        string GetExeConfigPath();
        string GetRoamingUserConfigPath();
        string GetLocalUserConfigPath();
    }
}
