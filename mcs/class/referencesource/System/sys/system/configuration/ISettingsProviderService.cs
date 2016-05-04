//------------------------------------------------------------------------------
// <copyright file="ISettingsProviderService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Configuration {
    /// <devdoc>
    ///     The IPersistComponentSettings interface enables components hosted in an application to persist their
    ///     settings in a manner transparent to the application. However, in some cases, the application may want to 
    ///     override the provider(s) specified by a component. For example, at design time, we may want to persist
    ///     settings differently. This service enables this scenario. The ApplicationSettingsBase class queries this
    ///     service from the owner component's site.
    /// </devdoc>
    public interface ISettingsProviderService {

        /// <devdoc>
        ///     Queries the service whether it wants to override the provider for the given SettingsProperty. If it
        ///     doesn't want to, it should return null, in which the provider will remain unchanged.
        /// </devdoc>
        SettingsProvider GetSettingsProvider(SettingsProperty property);
    }
}

