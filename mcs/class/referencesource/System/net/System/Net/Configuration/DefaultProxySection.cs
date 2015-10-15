//------------------------------------------------------------------------------
// <copyright file="DefaultProxySection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ComponentModel;


    public sealed class DefaultProxySection : ConfigurationSection
    {
        public DefaultProxySection()
        {
            this.properties.Add(this.bypasslist);
            this.properties.Add(this.module);
            this.properties.Add(this.proxy);
            this.properties.Add(this.enabled);
            this.properties.Add(this.useDefaultCredentials);
        }

        protected override void PostDeserialize()
        {
            // Perf optimization. If the configuration is coming from machine.config
            // It is safe and we don't need to check for permissions.
            if (EvaluationContext.IsMachineLevel)
                return;

            try {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
            } catch (Exception exception) {
                throw new ConfigurationErrorsException(
                              SR.GetString(SR.net_config_section_permission, 
                                           ConfigurationStrings.DefaultProxySectionName),
                              exception);
            }
        }
         
        [ConfigurationProperty(ConfigurationStrings.BypassList)]
        public BypassElementCollection BypassList
        {
            get { return (BypassElementCollection)this[this.bypasslist]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Module)]
        public ModuleElement Module
        {
            get { return (ModuleElement)this[this.module]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return this.properties; }
        }
        
        [ConfigurationProperty(ConfigurationStrings.Proxy)]
        public ProxyElement Proxy
        {
            get { return (ProxyElement)this[this.proxy]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Enabled, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool) this[this.enabled]; }
            set { this[this.enabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseDefaultCredentials, DefaultValue = false)]
        public bool UseDefaultCredentials
        {
            get { return (bool) this[this.useDefaultCredentials]; }
            set { this[this.useDefaultCredentials] = value; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty bypasslist =
            new ConfigurationProperty(ConfigurationStrings.BypassList, 
                                      typeof(BypassElementCollection), 
                                      null,
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty module =
            new ConfigurationProperty(ConfigurationStrings.Module, 
                                      typeof(ModuleElement), 
                                      null,
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty proxy =
            new ConfigurationProperty(ConfigurationStrings.Proxy, 
                                      typeof(ProxyElement), 
                                      null,
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty enabled =
            new ConfigurationProperty(ConfigurationStrings.Enabled, 
                                      typeof(bool), 
                                      true, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty useDefaultCredentials =
            new ConfigurationProperty(ConfigurationStrings.UseDefaultCredentials, 
                                      typeof(bool), 
                                      false,
                                      ConfigurationPropertyOptions.None);


        // This allows us to prevent parent settings (machine.config) from propegating to higher config (app.config), unless
        // the higher config doesn't contain the section at all.  That is, overriding defaultProxy is all-or-nothing.
        // Template from Microsoft.
        protected override void Reset(ConfigurationElement parentElement)
        {
            // Ignore the parentElement parameter by changing it to the default settings
            DefaultProxySection defaultElement = new DefaultProxySection();

            // Initialize the parentElement to the right set of defaults (not needed now,
            // but this will avoid errors in the future if SetDefaults is ever overridden in this class.
            // ConfigurationElement::InitializeDefault is a no-op, so you aren’t hurting perf by anything
            // measurable. 
            defaultElement.InitializeDefault();

            // Finally, pass it to the base class to do the “right things”
            base.Reset(defaultElement);
        }
    }

    internal sealed class DefaultProxySectionInternal
    {
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        internal DefaultProxySectionInternal(DefaultProxySection section)
        {
            // If enabled is false, skip everything.
            if (!section.Enabled)
            {
                return;
            }

            // If nothing else is set, use the system default.
            if (section.Proxy.AutoDetect == ProxyElement.AutoDetectValues.Unspecified &&
                section.Proxy.ScriptLocation == null &&
                String.IsNullOrEmpty(section.Module.Type) &&
                section.Proxy.UseSystemDefault != ProxyElement.UseSystemDefaultValues.True &&
                section.Proxy.ProxyAddress == null &&
                section.Proxy.BypassOnLocal == ProxyElement.BypassOnLocalValues.Unspecified &&
                section.BypassList.Count == 0)
            {
                // Old-style indication to turn off the proxy.
                if (section.Proxy.UseSystemDefault == ProxyElement.UseSystemDefaultValues.False)
                {
                    this.webProxy = new EmptyWebProxy();

                    // Intentionally ignoring UseDefaultCredentials in this case.
                    return;
                }

                // Suspend impersonation.
                try {
                    new SecurityPermission(SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.UnmanagedCode).Assert();
#if !FEATURE_PAL
                    using(WindowsIdentity.Impersonate(IntPtr.Zero))
#endif // !FEATURE_PAL
                    {
                        CodeAccessPermission.RevertAssert();
                        this.webProxy = new WebRequest.WebProxyWrapper(new WebProxy(true));
                    }
                } catch {
                    throw;
                }
            }
            else
            {
                // First, check out if we are using a different module type
                if (!String.IsNullOrEmpty(section.Module.Type))
                {
                    Type theType = Type.GetType(section.Module.Type, true, true);
                    
                    if ((theType.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.Public)
                        throw new ConfigurationErrorsException(SR.GetString(SR.net_config_proxy_module_not_public));
                    
                    // verify that its of the proper type of IWebProxy
                    if (!typeof(IWebProxy).IsAssignableFrom(theType))
                    {
                        throw new InvalidCastException(SR.GetString(SR.net_invalid_cast,
                                                                    theType.FullName,
                                                                    "IWebProxy"));
                    }
                    this.webProxy = (IWebProxy)Activator.CreateInstance(
                                    theType,
                                    BindingFlags.CreateInstance
                                    | BindingFlags.Instance
                                    | BindingFlags.NonPublic
                                    | BindingFlags.Public,
                                    null,          // Binder
                                    new object[0], // no arguments
                                    CultureInfo.InvariantCulture
                                    );
                }
                else if (section.Proxy.UseSystemDefault == ProxyElement.UseSystemDefaultValues.True &&
                         section.Proxy.AutoDetect == ProxyElement.AutoDetectValues.Unspecified &&
                         section.Proxy.ScriptLocation == null)
                {
                    // Suspend impersonation.  This setting is deprecated but required for Everett compat.
                    try {
                        new SecurityPermission(SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.UnmanagedCode).Assert();
#if !FEATURE_PAL
                        using(WindowsIdentity.Impersonate(IntPtr.Zero))
#endif // !FEATURE_PAL
                        {
                            CodeAccessPermission.RevertAssert();
                            this.webProxy = new WebProxy(false);
                        }
                    } catch {
                        throw;
                    }
                }
                else
                {
                    this.webProxy = new WebProxy();
                }

                WebProxy tempProxy = this.webProxy as WebProxy;

                if (tempProxy != null)
                {
                    if (section.Proxy.AutoDetect != ProxyElement.AutoDetectValues.Unspecified)
                    {
                        tempProxy.AutoDetect = section.Proxy.AutoDetect == ProxyElement.AutoDetectValues.True;
                    }
                    if (section.Proxy.ScriptLocation != null)
                    {
                        tempProxy.ScriptLocation = section.Proxy.ScriptLocation;
                    }
                    if (section.Proxy.BypassOnLocal != ProxyElement.BypassOnLocalValues.Unspecified)
                    {
                        tempProxy.BypassProxyOnLocal = section.Proxy.BypassOnLocal == ProxyElement.BypassOnLocalValues.True;
                    }
                    if (section.Proxy.ProxyAddress != null)
                    {
                        tempProxy.Address = section.Proxy.ProxyAddress;
                    }
                    int bypassListSize = section.BypassList.Count;
                    if (bypassListSize > 0)
                    {
                        string[] bypassList = new string[section.BypassList.Count];
                        for (int index = 0; index < bypassListSize; ++index)
                        {
                            bypassList[index] = section.BypassList[index].Address;
                        }
                        tempProxy.BypassList = bypassList;
                    }

                    // Wrap it if type not explicitly specified in Module.
                    if (section.Module.Type == null)
                    {
                        this.webProxy = new WebRequest.WebProxyWrapper(tempProxy);
                    }
                }
            }

            // Now apply UseDefaultCredentials if there's a proxy.
            if (this.webProxy != null && section.UseDefaultCredentials)
            {
                this.webProxy.Credentials = SystemNetworkCredential.defaultCredential;
            }
        }

        internal static object ClassSyncObject
        {
            get
            {
                if (classSyncObject == null)
                {
                    object o = new object();
                    Interlocked.CompareExchange(ref classSyncObject, o, null);
                }
                return classSyncObject;
            }
        }

        static internal DefaultProxySectionInternal GetSection()
        {
            lock (DefaultProxySectionInternal.ClassSyncObject)
            {
                DefaultProxySection section = PrivilegedConfigurationManager.GetSection(ConfigurationStrings.DefaultProxySectionPath) as DefaultProxySection;
                if (section == null)
                    return null;

                try
                {
                    return new DefaultProxySectionInternal(section);
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception)) throw;

                    throw new ConfigurationErrorsException(SR.GetString(SR.net_config_proxy), exception);
                }
            }
        }

        internal IWebProxy WebProxy
        {
            get { return this.webProxy; }
        }

        private IWebProxy webProxy;
        private static object classSyncObject;
    }
}
