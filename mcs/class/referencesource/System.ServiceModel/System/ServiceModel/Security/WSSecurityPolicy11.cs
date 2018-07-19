//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;

    class WSSecurityPolicy11 : WSSecurityPolicy
    {
        public const string WsspNamespace = @"http://schemas.xmlsoap.org/ws/2005/07/securitypolicy";

        public override string WsspNamespaceUri
        {
            get { return WSSecurityPolicy11.WsspNamespace; }
        }

        public override bool IsSecurityVersionSupported(MessageSecurityVersion version)
        {
            return version == MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10 ||
                version == MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11 ||
                version == MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
        }

        public override MessageSecurityVersion GetSupportedMessageSecurityVersion(SecurityVersion version)
        {
                return (version == SecurityVersion.WSSecurity10) ? MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10 : MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
        }

        public override TrustDriver TrustDriver
        {
            get
            {
                return new WSTrustFeb2005.DriverFeb2005(new SecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11, WSSecurityTokenSerializer.DefaultInstance));
            }
        }

        // WS-SecurityPolicy 11 should still use the mssp namespace for MustNotSendCancel
        public override XmlElement CreateWsspMustNotSendCancelAssertion(bool requireCancel)
        {
            if (!requireCancel)
            {
                XmlElement result = CreateMsspAssertion(MustNotSendCancelName);
                return result;
            }
            else
            {
                return null;
            }
        }

        // WS-SecurityPolicy 11 should still use the mssp namespace for MustNotSendCancel
        public override bool TryImportWsspMustNotSendCancelAssertion(ICollection<XmlElement> assertions, out bool requireCancellation)
        {
            requireCancellation = !TryImportMsspAssertion(assertions, MustNotSendCancelName);
            return true;
        }

        public override XmlElement CreateWsspHttpsTokenAssertion(MetadataExporter exporter, HttpsTransportBindingElement httpsBinding)
        {
            XmlElement result = CreateWsspAssertion(HttpsTokenName);
            result.SetAttribute(RequireClientCertificateName, httpsBinding.RequireClientCertificate ? TrueName : FalseName);
            return result;
        }

        public override bool TryImportWsspHttpsTokenAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, HttpsTransportBindingElement httpsBinding)
        {
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }

            bool result;
            XmlElement assertion;

            if (TryImportWsspAssertion(assertions, HttpsTokenName, out assertion))
            {
                result = true;
                string v = assertion.GetAttribute(RequireClientCertificateName);
                try 
                {
                    httpsBinding.RequireClientCertificate = XmlUtil.IsTrue(v);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    if (e is NullReferenceException)
                        throw;

                    importer.Errors.Add(new MetadataConversionError(SR.GetString(SR.UnsupportedBooleanAttribute, RequireClientCertificateName, e.Message), false));
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        public override XmlElement CreateWsspTrustAssertion(MetadataExporter exporter, SecurityKeyEntropyMode keyEntropyMode)
        {
            return CreateWsspTrustAssertion(Trust10Name, exporter, keyEntropyMode);
        }

        public override bool TryImportWsspTrustAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion)
        {
            return TryImportWsspTrustAssertion(Trust10Name, importer, assertions, binding, out assertion);
        }
    }
}
