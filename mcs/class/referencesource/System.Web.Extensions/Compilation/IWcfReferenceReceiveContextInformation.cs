//------------------------------------------------------------------------------
// <copyright file="IWcfReferenceReceiveContextInformation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace System.Web.Compilation
{
    /// <summary>
    /// This interface should be implemented by a wsdl or policy import extension if it wants to receive
    ///   extra context information from the WCF Service Reference tools.  Any extension 
    ///   files associated with the .svcmap file of a WCF service reference (e.g., Reference.config or
    ///   custom extension files added through extensibility) may be received through this interface.
    ///   In addition, a service provider is passed in which may be queried for additional information.
    ///   Note that any changes made to the information in serviceReferenceExtensionFileContents will
    ///   not be persisted.
    ///   
    /// Important note: any interface received from the service provider which is specific to Visual
    ///   Studio will not be available to an import extension when being run in the context of an ASP.NET
    ///   build provider, which is the case when a WCF service reference is being compiled in an ASP.NET
    ///   project.  The extension files are always available, so they do not have this limitation.
    /// </summary>
    public interface IWcfReferenceReceiveContextInformation
    {
        void ReceiveImportContextInformation(
            IDictionary<string, byte[]> serviceReferenceExtensionFileContents, 
            IServiceProvider serviceProvider);
    }
}
