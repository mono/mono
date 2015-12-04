//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ComponentModel;
    using System.Configuration;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;

    public sealed partial class ServiceAuthorizationElement : BehaviorExtensionElement
    {
        public ServiceAuthorizationElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.PrincipalPermissionMode, DefaultValue = PrincipalPermissionMode.UseWindowsGroups)]
        [ServiceModelEnumValidator(typeof(PrincipalPermissionModeHelper))]
        public PrincipalPermissionMode PrincipalPermissionMode
        {
            get { return (PrincipalPermissionMode)base[ConfigurationStrings.PrincipalPermissionMode]; }
            set { base[ConfigurationStrings.PrincipalPermissionMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.RoleProviderName, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string RoleProviderName
        {
            get { return (string)base[ConfigurationStrings.RoleProviderName]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.RoleProviderName] = value;
            }
        }
        
        [ConfigurationProperty(ConfigurationStrings.ImpersonateCallerForAllOperations, DefaultValue = ServiceAuthorizationBehavior.DefaultImpersonateCallerForAllOperations)]
        public bool ImpersonateCallerForAllOperations
        {
            get { return (bool)base[ConfigurationStrings.ImpersonateCallerForAllOperations]; }
            set { base[ConfigurationStrings.ImpersonateCallerForAllOperations] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ImpersonateOnSerializingReply, DefaultValue = ServiceAuthorizationBehavior.DefaultImpersonateOnSerializingReply)]
        public bool ImpersonateOnSerializingReply
        {
            get { return (bool)base[ConfigurationStrings.ImpersonateOnSerializingReply]; }
            set { base[ConfigurationStrings.ImpersonateOnSerializingReply] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServiceAuthorizationManagerType, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string ServiceAuthorizationManagerType
        {
            get { return (string)base[ConfigurationStrings.ServiceAuthorizationManagerType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.ServiceAuthorizationManagerType] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.AuthorizationPolicies)]
        public AuthorizationPolicyTypeElementCollection AuthorizationPolicies
        {
            get { return (AuthorizationPolicyTypeElementCollection)base[ConfigurationStrings.AuthorizationPolicies]; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            ServiceAuthorizationElement source = (ServiceAuthorizationElement)from;
#pragma warning suppress 56506 //[....]; base.CopyFrom() checks for 'from' being null
            this.PrincipalPermissionMode = source.PrincipalPermissionMode;
            this.RoleProviderName = source.RoleProviderName;
            this.ImpersonateCallerForAllOperations = source.ImpersonateCallerForAllOperations;
            this.ImpersonateOnSerializingReply = source.ImpersonateOnSerializingReply;
            this.ServiceAuthorizationManagerType = source.ServiceAuthorizationManagerType;
            AuthorizationPolicyTypeElementCollection srcAuthorizationPolicies = source.AuthorizationPolicies;
            AuthorizationPolicyTypeElementCollection dstAuthorizationPolicies = this.AuthorizationPolicies;
            for (int i = 0; i < srcAuthorizationPolicies.Count; ++i)
            {
                dstAuthorizationPolicies.Add(srcAuthorizationPolicies[i]);
            }
        }

        protected internal override object CreateBehavior()
        {
            ServiceAuthorizationBehavior behavior = new ServiceAuthorizationBehavior();
            behavior.PrincipalPermissionMode = this.PrincipalPermissionMode;
            string roleProviderName = this.RoleProviderName;
            if (!String.IsNullOrEmpty(roleProviderName))
            {
                behavior.RoleProvider = SystemWebHelper.GetRoleProvider(roleProviderName);
                if (behavior.RoleProvider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.InvalidRoleProviderSpecifiedInConfig, roleProviderName)));
                }
            }

            behavior.ImpersonateCallerForAllOperations = this.ImpersonateCallerForAllOperations;
            behavior.ImpersonateOnSerializingReply = this.ImpersonateOnSerializingReply;

            string serviceAuthorizationManagerType = this.ServiceAuthorizationManagerType;
            if (!String.IsNullOrEmpty(serviceAuthorizationManagerType))
            {
                Type type = Type.GetType(serviceAuthorizationManagerType, true);
                if (!typeof(ServiceAuthorizationManager).IsAssignableFrom(type))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigInvalidServiceAuthorizationManagerType, serviceAuthorizationManagerType, typeof(ServiceAuthorizationManager))));
                }
                behavior.ServiceAuthorizationManager = (ServiceAuthorizationManager)Activator.CreateInstance(type);
            }
            AuthorizationPolicyTypeElementCollection authorizationPolicies = this.AuthorizationPolicies;
            if (authorizationPolicies.Count > 0)
            {
                List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(authorizationPolicies.Count);
                for (int i = 0; i < authorizationPolicies.Count; ++i)
                {
                    Type type = Type.GetType(authorizationPolicies[i].PolicyType, true);
                    if (!typeof(IAuthorizationPolicy).IsAssignableFrom(type))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                            SR.GetString(SR.ConfigInvalidAuthorizationPolicyType, authorizationPolicies[i].PolicyType, typeof(IAuthorizationPolicy))));
                    }
                    policies.Add((IAuthorizationPolicy)Activator.CreateInstance(type));
                }
                behavior.ExternalAuthorizationPolicies = policies.AsReadOnly();
            }
            return behavior;
        }

        public override Type BehaviorType
        {
            get { return typeof(ServiceAuthorizationBehavior); }
        }
    }
}
