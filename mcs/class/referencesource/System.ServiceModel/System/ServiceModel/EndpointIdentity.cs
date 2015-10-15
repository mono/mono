//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Net;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.DirectoryServices;
    using System.DirectoryServices.ActiveDirectory;
    using System.Security.Principal;
    using System.ServiceModel.Security;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;
    using System.Xml.Serialization;

    public abstract class EndpointIdentity
    {
        internal const StoreLocation defaultStoreLocation = StoreLocation.LocalMachine;
        internal const StoreName defaultStoreName = StoreName.My;
        internal const X509FindType defaultX509FindType = X509FindType.FindBySubjectDistinguishedName;

        Claim identityClaim;
        IEqualityComparer<Claim> claimComparer;

        protected EndpointIdentity()
        {
        }

        protected void Initialize(Claim identityClaim)
        {
            if (identityClaim == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identityClaim");

            Initialize(identityClaim, null);
        }

        protected void Initialize(Claim identityClaim, IEqualityComparer<Claim> claimComparer)
        {
            if (identityClaim == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identityClaim");

            this.identityClaim = identityClaim;
            this.claimComparer = claimComparer;
        }

        public Claim IdentityClaim
        {
            get
            {
                if (this.identityClaim == null)
                {
                    EnsureIdentityClaim();
                }
                return this.identityClaim;
            }
        }

        public static EndpointIdentity CreateIdentity(Claim identity)
        {
            if (identity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");

            // PreSharp 
#pragma warning suppress 56506 // Claim.ClaimType will never return null
            if (identity.ClaimType.Equals(ClaimTypes.Dns))
            {
                return new DnsEndpointIdentity(identity);
            }
            else if (identity.ClaimType.Equals(ClaimTypes.Spn))
            {
                return new SpnEndpointIdentity(identity);
            }
            else if (identity.ClaimType.Equals(ClaimTypes.Upn))
            {
                return new UpnEndpointIdentity(identity);
            }
            else if (identity.ClaimType.Equals(ClaimTypes.Rsa))
            {
                return new RsaEndpointIdentity(identity);
            }
            else
            {
                return new GeneralEndpointIdentity(identity);
            }
        }

        public static EndpointIdentity CreateDnsIdentity(string dnsName)
        {
            return new DnsEndpointIdentity(dnsName);
        }

        public static EndpointIdentity CreateSpnIdentity(string spnName)
        {
            return new SpnEndpointIdentity(spnName);
        }

        public static EndpointIdentity CreateUpnIdentity(string upnName)
        {
            return new UpnEndpointIdentity(upnName);
        }

        public static EndpointIdentity CreateRsaIdentity(string publicKey)
        {
            return new RsaEndpointIdentity(publicKey);
        }

        public static EndpointIdentity CreateRsaIdentity(X509Certificate2 certificate)
        {
            return new RsaEndpointIdentity(certificate);
        }

        public static EndpointIdentity CreateX509CertificateIdentity(X509Certificate2 certificate)
        {
            return new X509CertificateEndpointIdentity(certificate);
        }

        public static EndpointIdentity CreateX509CertificateIdentity(X509Certificate2 primaryCertificate, X509Certificate2Collection supportingCertificates)
        {
            return new X509CertificateEndpointIdentity(primaryCertificate, supportingCertificates);
        }

        internal static EndpointIdentity CreateX509CertificateIdentity(X509Chain certificateChain)
        {
            if (certificateChain == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificateChain");

            if (certificateChain.ChainElements.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.X509ChainIsEmpty));

            // The first element in the cert chain is the leaf certificate 
            // we want.
            X509Certificate2 primaryCertificate = certificateChain.ChainElements[0].Certificate;
            X509Certificate2Collection certificateCollection = new X509Certificate2Collection();
            for (int i = 1; i < certificateChain.ChainElements.Count; ++i)
            {
                certificateCollection.Add(certificateChain.ChainElements[i].Certificate);
            }

            return new X509CertificateEndpointIdentity(primaryCertificate, certificateCollection);
        }

        internal virtual void EnsureIdentityClaim()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == (object)this)
                return true;

            // as handles null do we need the double null check?
            if (obj == null)
                return false;

            EndpointIdentity otherIdentity = obj as EndpointIdentity;
            if (otherIdentity == null)
                return false;

            return Matches(otherIdentity.IdentityClaim);
        }

        public override int GetHashCode()
        {
            return GetClaimComparer().GetHashCode(this.IdentityClaim);
        }

        public override string ToString()
        {
            return "identity(" + this.IdentityClaim + ")";
        }

        internal bool Matches(Claim claim)
        {
            return GetClaimComparer().Equals(this.IdentityClaim, claim);
        }

        IEqualityComparer<Claim> GetClaimComparer()
        {
            if (this.claimComparer == null)
                this.claimComparer = Claim.DefaultComparer;

            return this.claimComparer;
        }

        internal static EndpointIdentity ReadIdentity(XmlDictionaryReader reader)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

            EndpointIdentity readIdentity = null;

            reader.MoveToContent();
            if (reader.IsEmptyElement)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnexpectedEmptyElementExpectingClaim, XD.AddressingDictionary.Identity.Value, XD.AddressingDictionary.IdentityExtensionNamespace.Value)));

            reader.ReadStartElement(XD.AddressingDictionary.Identity, XD.AddressingDictionary.IdentityExtensionNamespace);

            if (reader.IsStartElement(XD.AddressingDictionary.Spn, XD.AddressingDictionary.IdentityExtensionNamespace))
                readIdentity = new SpnEndpointIdentity(reader.ReadElementString());
            else if (reader.IsStartElement(XD.AddressingDictionary.Upn, XD.AddressingDictionary.IdentityExtensionNamespace))
                readIdentity = new UpnEndpointIdentity(reader.ReadElementString());
            else if (reader.IsStartElement(XD.AddressingDictionary.Dns, XD.AddressingDictionary.IdentityExtensionNamespace))
                readIdentity = new DnsEndpointIdentity(reader.ReadElementString());
            else if (reader.IsStartElement(XD.XmlSignatureDictionary.KeyInfo, XD.XmlSignatureDictionary.Namespace))
            {
                reader.ReadStartElement();
                if (reader.IsStartElement(XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace))
                {
                    readIdentity = new X509CertificateEndpointIdentity(reader);
                }
                else if (reader.IsStartElement(XD.XmlSignatureDictionary.RsaKeyValue, XD.XmlSignatureDictionary.Namespace))
                {
                    readIdentity = new RsaEndpointIdentity(reader);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnrecognizedIdentityType, reader.Name, reader.NamespaceURI)));
                }
                reader.ReadEndElement();
            }
            else if (reader.NodeType == XmlNodeType.Element)
            {
                //
                // Something unknown
                // 
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnrecognizedIdentityType, reader.Name, reader.NamespaceURI)));
            }
            else
            {
                //
                // EndpointIdentity element is empty or some other invalid xml
                //
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidIdentityElement)));
            }

            reader.ReadEndElement();

            return readIdentity;
        }

        internal void WriteTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");

            writer.WriteStartElement(XD.AddressingDictionary.Identity, XD.AddressingDictionary.IdentityExtensionNamespace);

            WriteContentsTo(writer);

            writer.WriteEndElement();
        }

        internal virtual void WriteContentsTo(XmlDictionaryWriter writer)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnrecognizedIdentityPropertyType, this.IdentityClaim.GetType().ToString())));
        }

    }

}
