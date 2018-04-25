//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    class MessageTraceRecord : TraceRecord
    {
        Message message;
        internal MessageTraceRecord(Message message)
        {
            this.message = message;
        }

        internal override string EventId
        {
            get { return BuildEventId("Message"); }
        }

        protected Message Message
        {
            get { return this.message; }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            if ((this.message != null) &&
                (this.message.State != MessageState.Closed) &&
                (this.message.Headers != null))
            {
                try
                {
                    xml.WriteStartElement("MessageProperties");
                    if (message.Properties.Encoder != null)
                    {
                        xml.WriteElementString("Encoder", message.Properties.Encoder.ToString());
                    }
                    xml.WriteElementString("AllowOutputBatching", message.Properties.AllowOutputBatching.ToString());
                    if (message.Properties.Security != null && message.Properties.Security.ServiceSecurityContext != null)
                    {
                        xml.WriteStartElement("Security");
                        xml.WriteElementString("IsAnonymous", message.Properties.Security.ServiceSecurityContext.IsAnonymous.ToString());
                        bool windowsIdentityUsed = message.Properties.Security.ServiceSecurityContext.WindowsIdentity != null &&
                            !string.IsNullOrEmpty(message.Properties.Security.ServiceSecurityContext.WindowsIdentity.Name);
                        xml.WriteElementString("WindowsIdentityUsed", windowsIdentityUsed.ToString());
                        if (DiagnosticUtility.ShouldTraceVerbose)
                        {
                            xml.WriteStartElement("Claims");
                            AuthorizationContext authContext = message.Properties.Security.ServiceSecurityContext.AuthorizationContext;
                            for (int i = 0; i < authContext.ClaimSets.Count; ++i)
                            {
                                ClaimSet claimSet = authContext.ClaimSets[i];
                                xml.WriteStartElement("ClaimSet");
                                xml.WriteAttributeString("ClrType", base.XmlEncode(claimSet.GetType().AssemblyQualifiedName));

                                for (int j = 0; j < claimSet.Count; ++j)
                                {
                                    Fx.Assert(null != claimSet[j], "Claim cannot be null");
                                    SecurityTraceRecordHelper.WriteClaim(xml, claimSet[j]);
                                }
                                xml.WriteEndElement();
                            }
                            xml.WriteEndElement();
                        }
                        xml.WriteEndElement();
                    }

                    if (message.Properties.Via != null)
                    {
                        xml.WriteElementString("Via", message.Properties.Via.ToString());
                    }

                    xml.WriteEndElement();

                    xml.WriteStartElement(MessageLogTraceRecord.MessageHeadersElementName);
                    for (int i = 0; i < this.message.Headers.Count; i++)
                    {
                        this.message.Headers.WriteHeader(i, xml);
                    }

                    xml.WriteEndElement();
                }
                catch (CommunicationException e)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.DiagnosticsFailedMessageTrace,
                            SR.GetString(SR.TraceCodeDiagnosticsFailedMessageTrace), e, message);
                    }
                }
            }
        }
    }
}
