//------------------------------------------------------------------------------
// <copyright file="AuthenticationModulesSection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System.Configuration;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading;

    public sealed class AuthenticationModulesSection : ConfigurationSection
    {
        public AuthenticationModulesSection()
        {
            this.properties.Add(this.authenticationModules);
        }

        protected override void PostDeserialize()
        {
            // Perf optimization. If the configuration is coming from machine.config
            // It is safe and we don't need to check for permissions.
            if (EvaluationContext.IsMachineLevel)
                return;

            try {
                ExceptionHelper.UnmanagedPermission.Demand();
            } catch (Exception exception) {
                throw new ConfigurationErrorsException(
                              SR.GetString(SR.net_config_section_permission, 
                                           ConfigurationStrings.AuthenticationModulesSectionName),
                              exception);
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public AuthenticationModuleElementCollection AuthenticationModules
        {
            get { return (AuthenticationModuleElementCollection)this[this.authenticationModules]; }
        }

        protected override void InitializeDefault()
        {
#if !FEATURE_PAL // Security
            this.AuthenticationModules.Add(
                new AuthenticationModuleElement(typeof(NegotiateClient).AssemblyQualifiedName));
            this.AuthenticationModules.Add(
                new AuthenticationModuleElement(typeof(KerberosClient).AssemblyQualifiedName));
            this.AuthenticationModules.Add(
                new AuthenticationModuleElement(typeof(NtlmClient).AssemblyQualifiedName));
            this.AuthenticationModules.Add(
                new AuthenticationModuleElement(typeof(DigestClient).AssemblyQualifiedName));
            this.AuthenticationModules.Add(
                new AuthenticationModuleElement(typeof(BasicClient).AssemblyQualifiedName));
#endif // !FEATURE_PAL // Security
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return this.properties; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty authenticationModules =
            new ConfigurationProperty(null, typeof(AuthenticationModuleElementCollection), null,
                    ConfigurationPropertyOptions.IsDefaultCollection);
    }

    internal sealed class AuthenticationModulesSectionInternal
    {
        internal AuthenticationModulesSectionInternal(AuthenticationModulesSection section)
        {
            if (section.AuthenticationModules.Count > 0)
            {
                this.authenticationModules = new List<Type>(section.AuthenticationModules.Count);
                foreach(AuthenticationModuleElement authenticationModuleElement in section.AuthenticationModules)
                {
                    Type type = null;

                    try
                    {
                        type = Type.GetType(authenticationModuleElement.Type, true, true);

                        // verify that its of the proper type of object
                        if (!typeof(IAuthenticationModule).IsAssignableFrom(type))
                        {
                            throw new InvalidCastException(SR.GetString(SR.net_invalid_cast, type.FullName, "IAuthenticationModule"));
                        }
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception)) throw;

                        throw new ConfigurationErrorsException(SR.GetString(SR.net_config_authenticationmodules), exception);
                    }

                    this.authenticationModules.Add(type);
                }
            }
        }

        internal List<Type> AuthenticationModules
        {
            get
            {
                List<Type> retval = this.authenticationModules;
                if (retval == null)
                {
                    retval = new List<Type>(0);
                }
                return retval;
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

        static internal AuthenticationModulesSectionInternal GetSection()
        {
            lock (AuthenticationModulesSectionInternal.ClassSyncObject)
            {
                AuthenticationModulesSection section = PrivilegedConfigurationManager.GetSection(ConfigurationStrings.AuthenticationModulesSectionPath) as AuthenticationModulesSection;
                if (section == null)
                    return null;

                return new AuthenticationModulesSectionInternal(section);
            }
        }

        List<Type> authenticationModules = null;
        static object classSyncObject = null;
    }
}
