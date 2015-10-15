//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;
    using System.Xml;
    using System.Xml.Serialization;

    public class RsaEndpointIdentity : EndpointIdentity
    {
        public RsaEndpointIdentity(string publicKey)
        {
            if (publicKey == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("publicKey");

            base.Initialize(Claim.CreateRsaClaim(ToRsa(publicKey)));
        }

        public RsaEndpointIdentity(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");

#pragma warning suppress 56506 // A Certificate Public key can never be null.
            RSA rsa = certificate.PublicKey.Key as RSA;
            if (rsa == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PublicKeyNotRSA)));

            base.Initialize(Claim.CreateRsaClaim(rsa));
        }

        public RsaEndpointIdentity(Claim identity)
        {
            if (identity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");

            // PreSharp 
#pragma warning suppress 56506 // Claim.ClaimType will never return null
            if (!identity.ClaimType.Equals(ClaimTypes.Rsa))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.UnrecognizedClaimTypeForIdentity, identity.ClaimType, ClaimTypes.Rsa));

            base.Initialize(identity);
        }

        internal RsaEndpointIdentity(XmlDictionaryReader reader)
        {
            reader.ReadStartElement(XD.XmlSignatureDictionary.RsaKeyValue, XD.XmlSignatureDictionary.Namespace);
            byte[] modulus = Convert.FromBase64String(reader.ReadElementString(XD.XmlSignatureDictionary.Modulus.Value, XD.XmlSignatureDictionary.Namespace.Value));
            byte[] exponent = Convert.FromBase64String(reader.ReadElementString(XD.XmlSignatureDictionary.Exponent.Value, XD.XmlSignatureDictionary.Namespace.Value));
            reader.ReadEndElement();
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters parameters = new RSAParameters();
            parameters.Exponent = exponent;
            parameters.Modulus = modulus;
            rsa.ImportParameters(parameters);
            base.Initialize(Claim.CreateRsaClaim(rsa));
        }

        internal override void WriteContentsTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");

            writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.KeyInfo, XD.XmlSignatureDictionary.Namespace);
            writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.RsaKeyValue, XD.XmlSignatureDictionary.Namespace);
            RSA rsa = (RSA)this.IdentityClaim.Resource;
            RSAParameters parameters = rsa.ExportParameters(false);
            writer.WriteElementString(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.Modulus, XD.XmlSignatureDictionary.Namespace, Convert.ToBase64String(parameters.Modulus));
            writer.WriteElementString(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.Exponent, XD.XmlSignatureDictionary.Namespace, Convert.ToBase64String(parameters.Exponent));
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        static RSA ToRsa(string keyString)
        {
            if (keyString == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyString");

            RSA rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(keyString);

            return rsa;
        }
    }
}
