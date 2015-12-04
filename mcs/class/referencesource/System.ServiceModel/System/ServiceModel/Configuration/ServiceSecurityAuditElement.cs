//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Configuration;
    using System.ServiceModel.Security;

    public sealed partial class ServiceSecurityAuditElement : BehaviorExtensionElement
    {
        public ServiceSecurityAuditElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.AuditLogLocation, DefaultValue = ServiceSecurityAuditBehavior.defaultAuditLogLocation)]
        [ServiceModelEnumValidator(typeof(AuditLogLocationHelper))]
        public AuditLogLocation AuditLogLocation
        {
            get { return (AuditLogLocation)base[ConfigurationStrings.AuditLogLocation]; }
            set { base[ConfigurationStrings.AuditLogLocation] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.SuppressAuditFailure, DefaultValue = ServiceSecurityAuditBehavior.defaultSuppressAuditFailure)]
        public bool SuppressAuditFailure 
        {
            get { return (bool)base[ConfigurationStrings.SuppressAuditFailure]; }
            set { base[ConfigurationStrings.SuppressAuditFailure] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServiceAuthorizationAuditLevel, DefaultValue = ServiceSecurityAuditBehavior.defaultServiceAuthorizationAuditLevel)]
        [ServiceModelEnumValidator(typeof(AuditLevelHelper))]
        public AuditLevel ServiceAuthorizationAuditLevel
        {
            get { return (AuditLevel)base[ConfigurationStrings.ServiceAuthorizationAuditLevel]; }
            set { base[ConfigurationStrings.ServiceAuthorizationAuditLevel] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MessageAuthenticationAuditLevel, DefaultValue = ServiceSecurityAuditBehavior.defaultMessageAuthenticationAuditLevel)]
        [ServiceModelEnumValidator(typeof(AuditLevelHelper))]
        public AuditLevel MessageAuthenticationAuditLevel
        {
            get { return (AuditLevel)base[ConfigurationStrings.MessageAuthenticationAuditLevel]; }
            set { base[ConfigurationStrings.MessageAuthenticationAuditLevel] = value; }
        }
        
        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            ServiceSecurityAuditElement source = (ServiceSecurityAuditElement)from;
#pragma warning suppress 56506 //[....]; base.CopyFrom() checks for 'from' being null
            this.AuditLogLocation = source.AuditLogLocation;
            this.SuppressAuditFailure = source.SuppressAuditFailure;
            this.ServiceAuthorizationAuditLevel = source.ServiceAuthorizationAuditLevel;
            this.MessageAuthenticationAuditLevel = source.MessageAuthenticationAuditLevel;
        }

        protected internal override object CreateBehavior()
        {
            ServiceSecurityAuditBehavior behavior = new ServiceSecurityAuditBehavior();
            behavior.AuditLogLocation = this.AuditLogLocation;
            behavior.SuppressAuditFailure = this.SuppressAuditFailure;
            behavior.ServiceAuthorizationAuditLevel = this.ServiceAuthorizationAuditLevel;
            behavior.MessageAuthenticationAuditLevel = this.MessageAuthenticationAuditLevel;
            return behavior;
        }

        public override Type BehaviorType
        {
            get { return typeof(ServiceSecurityAuditBehavior); }
        }
    }
}



