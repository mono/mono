//------------------------------------------------------------------------------
// <copyright file="ProvidersHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Web.Compilation;
    using System.Collections.Specialized;
    using System;
    using System.Security;
    using System.Security.Permissions;

    public static class ProvidersHelper {
        ///////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////
        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
        public static ProviderBase InstantiateProvider(ProviderSettings providerSettings, Type providerType)
        {
            ProviderBase provider = null;
            try {
                string pnType = (providerSettings.Type == null) ? null : providerSettings.Type.Trim();
                if (string.IsNullOrEmpty(pnType))
                    throw new ArgumentException(SR.GetString(SR.Provider_no_type_name));
                Type t = ConfigUtil.GetType(pnType, "type", providerSettings, true, true);

                if (!providerType.IsAssignableFrom(t))
                    throw new ArgumentException(SR.GetString(SR.Provider_must_implement_type, providerType.ToString()));
                provider = (ProviderBase)HttpRuntime.CreatePublicInstance(t);

                // Because providers modify the parameters collection (i.e. delete stuff), pass in a clone of the collection
                NameValueCollection pars = providerSettings.Parameters;
                NameValueCollection cloneParams = new NameValueCollection(pars.Count, StringComparer.Ordinal);
                foreach (string key in pars)
                    cloneParams[key] = pars[key];
                provider.Initialize(providerSettings.Name, cloneParams);
            } catch (Exception e) {
                if (e is ConfigurationException)
                    throw;
                throw new ConfigurationErrorsException(e.Message, e, providerSettings.ElementInformation.Properties["type"].Source, providerSettings.ElementInformation.Properties["type"].LineNumber);
            }

            return provider;
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
        public static void InstantiateProviders(ProviderSettingsCollection configProviders, ProviderCollection providers, Type providerType)
        {
            foreach (ProviderSettings ps in configProviders) {
                providers.Add(InstantiateProvider(ps, providerType));
            }
        }
    }
}
