//------------------------------------------------------------------------------
// <copyright file="SiteMap.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Copyright (c) 2002 Microsoft Corporation
 */

namespace System.Web {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration.Provider;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Hosting;

    public static class SiteMap {

        internal const string SectionName = "system.web/siteMap";

        private static SiteMapProviderCollection _providers;
        private static SiteMapProvider _provider;
        private static object _lockObject = new object();

        private static bool _configEnabledEvaluated;
        private static bool _enabled;


        public static SiteMapNode CurrentNode {
            get {
                return Provider.CurrentNode;
            }
        }

        public static bool Enabled {
            get {
                if (!_configEnabledEvaluated) {
                    SiteMapSection config = RuntimeConfig.GetAppConfig().SiteMap;
                    _enabled = (config != null && config.Enabled);

                    _configEnabledEvaluated = true;
                }

                return _enabled;
            }
        }


        public static SiteMapProvider Provider {
            get {
                Initialize();
                return _provider;
            }
        }


        public static SiteMapProviderCollection Providers {
            get {
                Initialize();
                return _providers;
            }
        }


        public static SiteMapNode RootNode {
            get {
                SiteMapProvider rootProvider = Provider.RootProvider;
                SiteMapNode rootNode = rootProvider.RootNode;

                if (rootNode == null) {
                    String name = ((ProviderBase)rootProvider).Name;
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapProvider_Invalid_RootNode, name));
                }

                return rootNode;

            }
        }


        public static event SiteMapResolveEventHandler SiteMapResolve {
            add {
                Provider.SiteMapResolve += value;
            }
            remove {
                Provider.SiteMapResolve -= value;
            }
        }

        private static void Initialize() {
            if (_providers != null)
                return;

            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);

            lock (_lockObject) {
                if (_providers != null)
                    return;

                SiteMapSection config = RuntimeConfig.GetAppConfig().SiteMap;
                if (config == null) {
                    // just return an empty collection so that designer will work.
                    _providers = new SiteMapProviderCollection();
                    return;
                }

                if (!config.Enabled)
                    throw new InvalidOperationException(SR.GetString(SR.SiteMap_feature_disabled, SiteMap.SectionName));

                // Make sure the default provider exists.
                config.ValidateDefaultProvider();

                _providers = config.ProvidersInternal;
                _provider = _providers[config.DefaultProvider];
                _providers.SetReadOnly();
            }
        }
    }

    public sealed class SiteMapProviderCollection :  ProviderCollection {

        public override void Add(ProviderBase provider) {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is SiteMapProvider))
                throw new ArgumentException(SR.GetString(SR.Provider_must_implement_the_interface, provider.GetType().Name, typeof(SiteMapProvider).Name), "provider");

            Add((SiteMapProvider)provider);
        }


        public void Add(SiteMapProvider provider) {
            if (provider == null) {
                throw new ArgumentNullException("provider");
            }

            base.Add(provider);
        }


        public void AddArray(SiteMapProvider [] providerArray) {
            if (providerArray == null) {
                throw new ArgumentNullException("providerArray");
            }

            foreach (SiteMapProvider provider in providerArray) {
                if (this[provider.Name] != null)
                    throw new ArgumentException(SR.GetString(SR.SiteMapProvider_Multiple_Providers_With_Identical_Name, provider.Name));

                Add(provider);
            }
        }


        public new SiteMapProvider this[string name] {
            get { 
                return (SiteMapProvider) base[name];
            }
        }
    }


    public delegate SiteMapNode SiteMapResolveEventHandler(Object sender, SiteMapResolveEventArgs e);

    public class SiteMapResolveEventArgs : EventArgs {
        private HttpContext _context;
        private SiteMapProvider _provider;

        public SiteMapResolveEventArgs(HttpContext context, SiteMapProvider provider) {
            _context = context;
            _provider = provider;
        }

        public SiteMapProvider Provider {
            get {
                return _provider;
            }
        }

        public HttpContext Context {
            get {
                return _context;
            }
        }
    }
}
