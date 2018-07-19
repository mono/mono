//------------------------------------------------------------------------------
// <copyright file="WebRequestModulesSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System.Configuration;
    using System.Collections;
    using System.Globalization;
    using System.Net;    
    using System.Reflection;
    using System.Threading;
    public sealed class WebRequestModulesSection : ConfigurationSection
    {
        public WebRequestModulesSection() 
        {
            this.properties.Add(this.webRequestModules);
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
                                           ConfigurationStrings.WebRequestModulesSectionName),
                              exception);
            }
        }

        protected override void InitializeDefault()
        {
            this.WebRequestModules.Add(
                new WebRequestModuleElement(ConfigurationStrings.Https, typeof(HttpRequestCreator)));
            this.WebRequestModules.Add(
                new WebRequestModuleElement(ConfigurationStrings.Http, typeof(HttpRequestCreator)));
            this.WebRequestModules.Add(
                new WebRequestModuleElement(ConfigurationStrings.File, typeof(FileWebRequestCreator)));
            this.WebRequestModules.Add(
                new WebRequestModuleElement(ConfigurationStrings.Ftp, typeof(FtpWebRequestCreator)));
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return this.properties; }
        }

        [ConfigurationProperty("", IsDefaultCollection=true )]
        public WebRequestModuleElementCollection WebRequestModules
        {
            get { return (WebRequestModuleElementCollection)this[this.webRequestModules]; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty webRequestModules =
            new ConfigurationProperty(null, typeof(WebRequestModuleElementCollection), null,
                    ConfigurationPropertyOptions.IsDefaultCollection);
    }

    internal sealed class WebRequestModulesSectionInternal
    {
        internal WebRequestModulesSectionInternal(WebRequestModulesSection section)
        {
            if (section.WebRequestModules.Count > 0)
            {
                this.webRequestModules = new ArrayList(section.WebRequestModules.Count);
                foreach(WebRequestModuleElement webRequestModuleElement in section.WebRequestModules)
                {
                    try
                    {
                        this.webRequestModules.Add(new WebRequestPrefixElement(webRequestModuleElement.Prefix, webRequestModuleElement.Type));
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception)) throw;

                        throw new ConfigurationErrorsException(SR.GetString(SR.net_config_webrequestmodules), exception);
                    }
                }
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

        static internal WebRequestModulesSectionInternal GetSection()
        {
            lock (WebRequestModulesSectionInternal.ClassSyncObject)
            {
                WebRequestModulesSection section = PrivilegedConfigurationManager.GetSection(ConfigurationStrings.WebRequestModulesSectionPath) as WebRequestModulesSection;
                if (section == null)
                    return null;

                return new WebRequestModulesSectionInternal(section);
            }
        }

        internal ArrayList WebRequestModules
        {
            get 
            {
                ArrayList retval = this.webRequestModules;
                if (retval == null)
                {
                    retval = new ArrayList(0);
                }
                return retval; 
            }
        }

        static object classSyncObject = null;
        ArrayList webRequestModules = null;
    }
}
