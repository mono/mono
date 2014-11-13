//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Description;
    using System.Transactions;
    using System.ServiceModel.Channels;
    using System.Runtime.CompilerServices;
    using System.Net.Security;
    using System.ServiceModel.Security;

    [AttributeUsage(ServiceModelAttributeTargets.ServiceContract, Inherited = false, AllowMultiple = false)]
    public sealed class ServiceContractAttribute : Attribute
    {
        Type callbackContract = null;
        string configurationName;
        string name;
        string ns;
        SessionMode sessionMode;
        ProtectionLevel protectionLevel = ProtectionLevel.None;
        bool hasProtectionLevel = false;

        public string ConfigurationName
        {
            get { return this.configurationName; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == string.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.SFxConfigurationNameCannotBeEmpty)));
                }
                this.configurationName = value;
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == string.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.SFxNameCannotBeEmpty)));
                }
                name = value;
            }
        }

        public string Namespace
        {
            get { return ns; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    NamingHelper.CheckUriProperty(value, "Namespace");
                ns = value;
            }
        }

        public ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.protectionLevel = value;
                this.hasProtectionLevel = true;
            }
        }

        public bool HasProtectionLevel
        {
            get { return this.hasProtectionLevel; }
        }

        public SessionMode SessionMode
        {
            get { return this.sessionMode; }
            set
            {
                if (!SessionModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.sessionMode = value;
            }
        }

        public Type CallbackContract
        {
            get { return this.callbackContract; }
            set { this.callbackContract = value; }
        }
    }
}
