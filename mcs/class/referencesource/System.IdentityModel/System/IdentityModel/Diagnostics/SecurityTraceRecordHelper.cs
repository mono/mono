
namespace System.IdentityModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IdentityModel.Diagnostics;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.Security.Authentication.ExtendedProtection;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;

    class SecurityTraceRecord : TraceRecord
    {
        String traceName;
        internal SecurityTraceRecord(String traceName)
        {
            if (string.IsNullOrEmpty(traceName))
                this.traceName = "Empty";
            else
                this.traceName = traceName;
        }

        internal override string EventId { get { return BuildEventId(traceName); } }
    }

    internal static class SecurityTraceRecordHelper
    {
        internal static void TraceServiceNameBindingOnServer(string serviceBindingNameSentByClient, string defaultServiceBindingNameOfServer, ServiceNameCollection serviceNameCollectionConfiguredOnServer)
        {
            TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ServiceBindingCheck, SR.GetString(SR.TraceCodeServiceBindingCheck), new ServiceBindingNameTraceRecord(serviceBindingNameSentByClient, defaultServiceBindingNameOfServer, serviceNameCollectionConfiguredOnServer), null, null);
        }

        internal static void TraceChannelBindingInformation(ExtendedProtectionPolicyHelper policyHelper, bool isServer, ChannelBinding channelBinding)
        {
            TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ChannelBindingCheck, SR.GetString(SR.TraceCodeChannelBindingCheck), new ChannelBindingNameTraceRecord(policyHelper, isServer, channelBinding), null, null);
        }

        class ServiceBindingNameTraceRecord : SecurityTraceRecord
        {
            string serviceBindingNameSentByClient;
            string defaultServiceBindingNameOfServer;
            ServiceNameCollection serviceNameCollectionConfiguredOnServer;

            public ServiceBindingNameTraceRecord(string serviceBindingNameSentByClient, string defaultServiceBindingNameOfServer, ServiceNameCollection serviceNameCollectionConfiguredOnServer)
                : base("ServiceBindingCheckAfterSpNego")
            {
                this.serviceBindingNameSentByClient = serviceBindingNameSentByClient;
                this.defaultServiceBindingNameOfServer = defaultServiceBindingNameOfServer;
                this.serviceNameCollectionConfiguredOnServer = serviceNameCollectionConfiguredOnServer;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                xml.WriteComment(SR.GetString(SR.ServiceNameFromClient));
                xml.WriteElementString("ServiceName", this.serviceBindingNameSentByClient);

                xml.WriteComment(SR.GetString(SR.ServiceNameOnService));
                xml.WriteStartElement("ServiceNameCollection");
                if (this.serviceNameCollectionConfiguredOnServer == null || this.serviceNameCollectionConfiguredOnServer.Count < 1)
                {
                    xml.WriteElementString("ServiceName", this.defaultServiceBindingNameOfServer);
                }
                else
                {
                    foreach (string serviceName in this.serviceNameCollectionConfiguredOnServer)
                    {
                        xml.WriteElementString("ServiceName", serviceName);
                    }
                }

                xml.WriteFullEndElement();

            }
        }

        class ChannelBindingNameTraceRecord : SecurityTraceRecord
        {
            ExtendedProtectionPolicyHelper policyHelper;
            bool isServer;
            bool channelBindingUsed;
            ChannelBinding channelBinding;

            public ChannelBindingNameTraceRecord(ExtendedProtectionPolicyHelper policyHelper, bool isServer, ChannelBinding channelBinding)
                : base("SpNegoChannelBindingInformation")
            {
                this.policyHelper = policyHelper;
                this.isServer = isServer;
                this.channelBindingUsed = false;
                this.channelBinding = channelBinding;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;
                if (this.policyHelper != null)
                {
                    xml.WriteElementString("PolicyEnforcement", this.policyHelper.PolicyEnforcement.ToString());
                    xml.WriteElementString("ProtectionScenario", this.policyHelper.ProtectionScenario.ToString());

                    xml.WriteStartElement("ServiceNameCollection");

                    if (this.policyHelper.ServiceNameCollection != null && this.policyHelper.ServiceNameCollection.Count > 0)
                    {
                        foreach (string serviceName in this.policyHelper.ServiceNameCollection)
                        {
                            xml.WriteElementString("ServiceName", serviceName);
                        }
                    }

                    xml.WriteFullEndElement();

                    if (this.isServer)
                    {
                        this.channelBindingUsed = this.policyHelper.ShouldAddChannelBindingToASC();
                    }
                    else
                    {
                        this.channelBindingUsed = this.policyHelper.ChannelBinding != null;
                    }

                    xml.WriteElementString("ChannelBindingUsed", this.channelBindingUsed.ToString());

                    if (this.channelBinding != null && this.policyHelper.PolicyEnforcement != PolicyEnforcement.Never && this.channelBindingUsed == true)
                    {
                        ExtendedProtectionPolicy extendedProtection = new ExtendedProtectionPolicy(policyHelper.PolicyEnforcement, channelBinding);
                        xml.WriteElementString("ChannelBindingData", GetBase64EncodedChannelBindingData(extendedProtection));
                    }
                }
                else
                {
                    // This is the case for KerberosRequestorSecurityToken where policyHelper is null.
                    if (this.channelBinding != null)
                    {
                        xml.WriteElementString("ChannelBindingUsed", "true");

                        // We do not know the PolicyEnforcement value here on the client side and we can not pass Never 
                        //as ExtendedProtectionPolicy constructor would throw on PolicyEnforcement.Never
                        ExtendedProtectionPolicy extendedProtection = new ExtendedProtectionPolicy(PolicyEnforcement.WhenSupported, channelBinding);
                        xml.WriteElementString("ChannelBindingData", GetBase64EncodedChannelBindingData(extendedProtection));
                    }
                    else
                    {
                        xml.WriteElementString("ChannelBindingUsed", "false");
                        xml.WriteElementString("ChannelBindingData", null);
                    }
                }

            }

            internal string GetBase64EncodedChannelBindingData(ExtendedProtectionPolicy extendedProtectionPolicy)
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, extendedProtectionPolicy);
                byte[] channelBindingData = ms.GetBuffer();
                return Convert.ToBase64String(channelBindingData, Base64FormattingOptions.None);
            }
        }

        /// <summary>
        /// Used to serialize a token to the trace.  Used in multiple places.
        /// </summary>
        internal class TokenTraceRecord : SecurityTraceRecord
        {
            const string ElementName = "TokenTraceRecord";

            SecurityToken _securityToken;

            public TokenTraceRecord(SecurityToken securityToken)
                : base(ElementName)
            {
                _securityToken = securityToken;
            }

            void WriteSessionToken(XmlWriter writer, SessionSecurityToken sessionToken)
            {
                SessionSecurityTokenHandler ssth = GetOrCreateSessionSecurityTokenHandler();

                XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
                ssth.WriteToken(dictionaryWriter, sessionToken);
            }

            private static SessionSecurityTokenHandler GetOrCreateSessionSecurityTokenHandler()
            {
                SecurityTokenHandlerCollection defaultHandlers = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
                SessionSecurityTokenHandler ssth = defaultHandlers[typeof(SessionSecurityToken)] as SessionSecurityTokenHandler;

                if (ssth == null)
                {
                    ssth = new SessionSecurityTokenHandler();
                    defaultHandlers.AddOrReplace(ssth);
                }

                return ssth;
            }

            internal override void WriteTo(XmlWriter writer)
            {
                writer.WriteStartElement(ElementName);
                writer.WriteAttributeString(DiagnosticStrings.NamespaceTag, EventId);

                writer.WriteStartElement("SecurityToken");
                writer.WriteAttributeString("Type", _securityToken.GetType().ToString());

                if (_securityToken is SessionSecurityToken)
                {
                    WriteSessionToken(writer, _securityToken as SessionSecurityToken);
                }
                else
                {
                    SecurityTokenHandlerCollection sthc = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
                    if (sthc.CanWriteToken(_securityToken))
                    {
                        {
                            sthc.WriteToken(writer, _securityToken);
                        }
                    }
                    else
                    {
                        writer.WriteElementString("Warning", SR.GetString(SR.TraceUnableToWriteToken, _securityToken.GetType().ToString()));
                    }
                }

                writer.WriteEndElement();
            }
        }
    }
}
