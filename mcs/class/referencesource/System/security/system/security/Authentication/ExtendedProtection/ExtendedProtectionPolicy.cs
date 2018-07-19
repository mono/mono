//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace System.Security.Authentication.ExtendedProtection
{
    /// <summary>
    /// This class contains the necessary settings for specifying how Extended Protection 
    /// should behave. Use one of the Build* methods to create an instance of this type.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExtendedProtectionPolicyTypeConverter))]
    public class ExtendedProtectionPolicy : ISerializable
    {
        private const string policyEnforcementName = "policyEnforcement";
        private const string protectionScenarioName = "protectionScenario";
        private const string customServiceNamesName = "customServiceNames";
        private const string customChannelBindingName = "customChannelBinding";

        private ServiceNameCollection customServiceNames;
        private PolicyEnforcement policyEnforcement;
        private ProtectionScenario protectionScenario;
        private ChannelBinding customChannelBinding;

        public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement, 
                                        ProtectionScenario protectionScenario,
                                        ServiceNameCollection customServiceNames)
        {
            if (policyEnforcement == PolicyEnforcement.Never)
            {
                throw new ArgumentException(SR.GetString(SR.security_ExtendedProtectionPolicy_UseDifferentConstructorForNever), "policyEnforcement");
            }
            if (customServiceNames != null && customServiceNames.Count == 0)
            {
                throw new ArgumentException(SR.GetString(SR.security_ExtendedProtectionPolicy_NoEmptyServiceNameCollection), "customServiceNames");
            }

            this.policyEnforcement = policyEnforcement;
            this.protectionScenario = protectionScenario;
            this.customServiceNames = customServiceNames;
        }

        public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement,
                                        ProtectionScenario protectionScenario,
                                        ICollection customServiceNames)
            : this(policyEnforcement, protectionScenario, 
                   customServiceNames == null ? (ServiceNameCollection)null : new ServiceNameCollection(customServiceNames))
        {
        }

        public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement,
                                        ChannelBinding customChannelBinding)
        {
            if (policyEnforcement == PolicyEnforcement.Never)
            {
                throw new ArgumentException(SR.GetString(SR.security_ExtendedProtectionPolicy_UseDifferentConstructorForNever), "policyEnforcement");
            }
            if (customChannelBinding == null)
            {
                throw new ArgumentNullException("customChannelBinding");
            }

            this.policyEnforcement = policyEnforcement;
            this.protectionScenario = ProtectionScenario.TransportSelected;
            this.customChannelBinding = customChannelBinding;
        }

        public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement)
        {
            // this is the only constructor which allows PolicyEnforcement.Never.
            this.policyEnforcement = policyEnforcement;
            this.protectionScenario = ProtectionScenario.TransportSelected;
        }

        protected ExtendedProtectionPolicy(SerializationInfo info, StreamingContext context)
        {
            policyEnforcement = (PolicyEnforcement)info.GetInt32(policyEnforcementName);
            protectionScenario = (ProtectionScenario)info.GetInt32(protectionScenarioName);
            customServiceNames = (ServiceNameCollection)info.GetValue(customServiceNamesName, typeof(ServiceNameCollection));

            byte[] channelBindingData = (byte[])info.GetValue(customChannelBindingName, typeof(byte[]));
            if (channelBindingData != null)
            {
                customChannelBinding = SafeLocalFreeChannelBinding.LocalAlloc(channelBindingData.Length);
                Marshal.Copy(channelBindingData, 0, customChannelBinding.DangerousGetHandle(), channelBindingData.Length);
            }
        }

        public ServiceNameCollection CustomServiceNames
        {
            get { return customServiceNames; }
        }

        public PolicyEnforcement PolicyEnforcement
        {
            get { return policyEnforcement; }
        }

        public ProtectionScenario ProtectionScenario
        {
            get { return protectionScenario; }
        }

        public ChannelBinding CustomChannelBinding
        {
            get { return customChannelBinding; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ProtectionScenario=");
            sb.Append(protectionScenario.ToString());
            sb.Append("; PolicyEnforcement=");
            sb.Append(policyEnforcement.ToString());

            sb.Append("; CustomChannelBinding=");
            if (customChannelBinding == null)
            {
                sb.Append("<null>");
            }
            else
            {
                sb.Append(customChannelBinding.ToString());
            }

            sb.Append("; ServiceNames=");
            if (customServiceNames == null) 
            {
                sb.Append("<null>");
            }
            else 
            {
                bool first = true;
                foreach (string serviceName in customServiceNames) 
                {
                    if (first) 
                    {
                        first = false;
                    }
                    else 
                    {
                        sb.Append(", ");
                    }

                    sb.Append(serviceName);
                }
            }

            return sb.ToString();
        }

        public static bool OSSupportsExtendedProtection
        {
            get
            {
                return AuthenticationManager.OSSupportsExtendedProtection;
            }
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(policyEnforcementName, (int)policyEnforcement);
            info.AddValue(protectionScenarioName, (int)protectionScenario);
            info.AddValue(customServiceNamesName, customServiceNames, typeof(ServiceNameCollection));

            if (customChannelBinding == null)
            {
                info.AddValue(customChannelBindingName, null, typeof(byte[]));
            }
            else
            {
                byte[] channelBindingData = new byte[customChannelBinding.Size];
                Marshal.Copy(customChannelBinding.DangerousGetHandle(), channelBindingData, 0, customChannelBinding.Size);
                info.AddValue(customChannelBindingName, channelBindingData, typeof(byte[]));
            }
        }
    }
}
