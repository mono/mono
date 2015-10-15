//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security.Tokens;
    using System.IO;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Mail;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Principal;

    static class SctClaimSerializer
    {
        static void SerializeSid(SecurityIdentifier sid, SctClaimDictionary dictionary, XmlDictionaryWriter writer)
        {
            byte[] sidBytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(sidBytes, 0);
            writer.WriteBase64(sidBytes, 0, sidBytes.Length);
        }

        static void WriteRightAttribute(Claim claim, SctClaimDictionary dictionary, XmlDictionaryWriter writer)
        {
            if (Rights.PossessProperty.Equals(claim.Right))
                return;
            writer.WriteAttributeString(dictionary.Right, dictionary.EmptyString, claim.Right);
        }

        static string ReadRightAttribute(XmlDictionaryReader reader, SctClaimDictionary dictionary)
        {
            string right = reader.GetAttribute(dictionary.Right, dictionary.EmptyString);
            return String.IsNullOrEmpty(right) ? Rights.PossessProperty : right;
        }

        static void WriteSidAttribute(SecurityIdentifier sid, SctClaimDictionary dictionary, XmlDictionaryWriter writer)
        {
            byte[] sidBytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(sidBytes, 0);
            writer.WriteAttributeString(dictionary.Sid, dictionary.EmptyString, Convert.ToBase64String(sidBytes));
        }

        static SecurityIdentifier ReadSidAttribute(XmlDictionaryReader reader, SctClaimDictionary dictionary)
        {
            byte[] sidBytes = Convert.FromBase64String(reader.GetAttribute(dictionary.Sid, dictionary.EmptyString));
            return new SecurityIdentifier(sidBytes, 0);
        }

        public static void SerializeClaim(Claim claim, SctClaimDictionary dictionary, XmlDictionaryWriter writer, XmlObjectSerializer serializer)
        {
            // the order in which known claim types are checked is optimized for use patterns
            if (claim == null)
            {
                writer.WriteElementString(dictionary.NullValue, dictionary.EmptyString, string.Empty);
                return;
            }
            else if (ClaimTypes.Sid.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.WindowsSidClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                SerializeSid((SecurityIdentifier)claim.Resource, dictionary, writer);
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.DenyOnlySid.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.DenyOnlySidClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                SerializeSid((SecurityIdentifier)claim.Resource, dictionary, writer);
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.X500DistinguishedName.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.X500DistinguishedNameClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                byte[] rawData = ((X500DistinguishedName)claim.Resource).RawData;
                writer.WriteBase64(rawData, 0, rawData.Length);
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.Thumbprint.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.X509ThumbprintClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                byte[] thumbprint = (byte[])claim.Resource;
                writer.WriteBase64(thumbprint, 0, thumbprint.Length);
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.Name.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.NameClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string)claim.Resource);
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.Dns.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.DnsClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string)claim.Resource);
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.Rsa.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.RsaClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString(((RSA)claim.Resource).ToXmlString(false));
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.Email.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.MailAddressClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString(((MailAddress)claim.Resource).Address);
                writer.WriteEndElement();
                return;
            }
            else if (claim == Claim.System)
            {
                writer.WriteElementString(dictionary.SystemClaim, dictionary.EmptyString, string.Empty);
                return;
            }
            else if (ClaimTypes.Hash.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.HashClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                byte[] hash = (byte[])claim.Resource;
                writer.WriteBase64(hash, 0, hash.Length);
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.Spn.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.SpnClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string)claim.Resource);
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.Upn.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.UpnClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string)claim.Resource);
                writer.WriteEndElement();
                return;
            }
            else if (ClaimTypes.Uri.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.UrlClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString(((Uri)claim.Resource).AbsoluteUri);
                writer.WriteEndElement();
                return;
            }
            else
            {
                // this is an extensible claim... need to delegate to xml object serializer
                serializer.WriteObject(writer, claim);
            }
        }

        public static void SerializeClaimSet(ClaimSet claimSet, SctClaimDictionary dictionary, XmlDictionaryWriter writer, XmlObjectSerializer serializer, XmlObjectSerializer claimSerializer)
        {
            if (claimSet is X509CertificateClaimSet)
            {
                X509CertificateClaimSet x509ClaimSet = (X509CertificateClaimSet)claimSet;
                writer.WriteStartElement(dictionary.X509CertificateClaimSet, dictionary.EmptyString);
                byte[] rawData = x509ClaimSet.X509Certificate.RawData;
                writer.WriteBase64(rawData, 0, rawData.Length);
                writer.WriteEndElement();
            }
            else if (claimSet == ClaimSet.System)
            {
                writer.WriteElementString(dictionary.SystemClaimSet, dictionary.EmptyString, String.Empty);
            }
            else if (claimSet == ClaimSet.Windows)
            {
                writer.WriteElementString(dictionary.WindowsClaimSet, dictionary.EmptyString, String.Empty);
            }
            else if (claimSet == ClaimSet.Anonymous)
            {
                writer.WriteElementString(dictionary.AnonymousClaimSet, dictionary.EmptyString, String.Empty);
            }
            else if (claimSet is WindowsClaimSet || claimSet is DefaultClaimSet)
            {
                writer.WriteStartElement(dictionary.ClaimSet, dictionary.EmptyString);
                writer.WriteStartElement(dictionary.PrimaryIssuer, dictionary.EmptyString);
                if (claimSet.Issuer == claimSet)
                {
                    writer.WriteElementString(dictionary.NullValue, dictionary.EmptyString, string.Empty);
                }
                else
                {
                    SerializeClaimSet(claimSet.Issuer, dictionary, writer, serializer, claimSerializer);
                }
                writer.WriteEndElement();

                foreach (Claim claim in claimSet)
                {
                    writer.WriteStartElement(dictionary.Claim, dictionary.EmptyString);
                    SerializeClaim(claim, dictionary, writer, claimSerializer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            else
            {
                serializer.WriteObject(writer, claimSet);
            }
        }

        public static Claim DeserializeClaim(XmlDictionaryReader reader, SctClaimDictionary dictionary, XmlObjectSerializer serializer)
        {
            if (reader.IsStartElement(dictionary.NullValue, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return null;
            }
            else if (reader.IsStartElement(dictionary.WindowsSidClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] sidBytes = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Sid, new SecurityIdentifier(sidBytes, 0), right);
            }
            else if (reader.IsStartElement(dictionary.DenyOnlySidClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] sidBytes = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.DenyOnlySid, new SecurityIdentifier(sidBytes, 0), right);
            }
            else if (reader.IsStartElement(dictionary.X500DistinguishedNameClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] rawData = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.X500DistinguishedName, new X500DistinguishedName(rawData), right);
            }
            else if (reader.IsStartElement(dictionary.X509ThumbprintClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] thumbprint = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Thumbprint, thumbprint, right);
            }
            else if (reader.IsStartElement(dictionary.NameClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string name = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Name, name, right);
            }
            else if (reader.IsStartElement(dictionary.DnsClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string dns = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Dns, dns, right);
            }
            else if (reader.IsStartElement(dictionary.RsaClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string rsaXml = reader.ReadString();
                reader.ReadEndElement();

                System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
                rsa.FromXmlString(rsaXml);
                return new Claim(ClaimTypes.Rsa, rsa, right);
            }
            else if (reader.IsStartElement(dictionary.MailAddressClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string address = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Email, new System.Net.Mail.MailAddress(address), right);
            }
            else if (reader.IsStartElement(dictionary.SystemClaim, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return Claim.System;
            }
            else if (reader.IsStartElement(dictionary.HashClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] hash = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Hash, hash, right);
            }
            else if (reader.IsStartElement(dictionary.SpnClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string spn = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Spn, spn, right);
            }
            else if (reader.IsStartElement(dictionary.UpnClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string upn = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Upn, upn, right);
            }
            else if (reader.IsStartElement(dictionary.UrlClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string url = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Uri, new Uri(url), right);
            }
            else
            {
                return (Claim)serializer.ReadObject(reader);
            }
        }

        public static ClaimSet DeserializeClaimSet(XmlDictionaryReader reader, SctClaimDictionary dictionary, XmlObjectSerializer serializer, XmlObjectSerializer claimSerializer)
        {
            if (reader.IsStartElement(dictionary.NullValue, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return null;
            }
            else if (reader.IsStartElement(dictionary.X509CertificateClaimSet, dictionary.EmptyString))
            {
                reader.ReadStartElement();
                byte[] rawData = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new X509CertificateClaimSet(new X509Certificate2(rawData), false);
            }
            else if (reader.IsStartElement(dictionary.SystemClaimSet, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return ClaimSet.System;
            }
            else if (reader.IsStartElement(dictionary.WindowsClaimSet, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return ClaimSet.Windows;
            }
            else if (reader.IsStartElement(dictionary.AnonymousClaimSet, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return ClaimSet.Anonymous;
            }
            else if (reader.IsStartElement(dictionary.ClaimSet, dictionary.EmptyString))
            {
                ClaimSet issuer = null;
                List<Claim> claims = new List<Claim>();
                reader.ReadStartElement();

                if (reader.IsStartElement(dictionary.PrimaryIssuer, dictionary.EmptyString))
                {
                    reader.ReadStartElement();
                    issuer = DeserializeClaimSet(reader, dictionary, serializer, claimSerializer);
                    reader.ReadEndElement();
                }

                while (reader.IsStartElement())
                {
                    reader.ReadStartElement();
                    claims.Add(DeserializeClaim(reader, dictionary, claimSerializer));
                    reader.ReadEndElement();
                }

                reader.ReadEndElement();
                return issuer != null ? new DefaultClaimSet(issuer, claims) : new DefaultClaimSet(claims);
            }
            else
            {
                return (ClaimSet)serializer.ReadObject(reader);
            }
        }

        public static void SerializeIdentities(AuthorizationContext authContext, SctClaimDictionary dictionary, XmlDictionaryWriter writer, XmlObjectSerializer serializer)
        {
            object obj;
            IList<IIdentity> identities;
            if (authContext.Properties.TryGetValue(SecurityUtils.Identities, out obj))
            {
                identities = obj as IList<IIdentity>;
                if (identities != null && identities.Count > 0)
                {
                    writer.WriteStartElement(dictionary.Identities, dictionary.EmptyString);
                    for (int i = 0; i < identities.Count; ++i)
                    {
                        SerializePrimaryIdentity(identities[i], dictionary, writer, serializer);
                    }
                    writer.WriteEndElement();
                }
            }
        }

        static void SerializePrimaryIdentity(IIdentity identity, SctClaimDictionary dictionary, XmlDictionaryWriter writer, XmlObjectSerializer serializer)
        {
            if (identity != null && identity != SecurityUtils.AnonymousIdentity)
            {
                writer.WriteStartElement(dictionary.PrimaryIdentity, dictionary.EmptyString);
                if (identity is WindowsIdentity)
                {
                    WindowsIdentity wid = (WindowsIdentity)identity;
                    writer.WriteStartElement(dictionary.WindowsSidIdentity, dictionary.EmptyString);
                    WriteSidAttribute(wid.User, dictionary, writer);

                    // This is to work around WOW64 
                    string authenticationType = null;
                    using (WindowsIdentity self = WindowsIdentity.GetCurrent())
                    {
                        // is owner or admin?  AuthenticationType could throw un-authorized exception
                        if ((self.User == wid.Owner) || 
                            (wid.Owner != null && self.Groups.Contains(wid.Owner)) || 
                            (wid.Owner != SecurityUtils.AdministratorsSid && self.Groups.Contains(SecurityUtils.AdministratorsSid)))
                        {
                            authenticationType = wid.AuthenticationType;
                        }
                    }
                    if (!String.IsNullOrEmpty(authenticationType))
                        writer.WriteAttributeString(dictionary.AuthenticationType, dictionary.EmptyString, authenticationType);
                    writer.WriteString(wid.Name);
                    writer.WriteEndElement();
                }
                else if (identity is WindowsSidIdentity)
                {
                    WindowsSidIdentity wsid = (WindowsSidIdentity)identity;
                    writer.WriteStartElement(dictionary.WindowsSidIdentity, dictionary.EmptyString);
                    WriteSidAttribute(wsid.SecurityIdentifier, dictionary, writer);
                    if (!String.IsNullOrEmpty(wsid.AuthenticationType))
                        writer.WriteAttributeString(dictionary.AuthenticationType, dictionary.EmptyString, wsid.AuthenticationType);
                    writer.WriteString(wsid.Name);
                    writer.WriteEndElement();
                }
                else if (identity is GenericIdentity)
                {
                    GenericIdentity genericIdentity = (GenericIdentity)identity;
                    writer.WriteStartElement(dictionary.GenericIdentity, dictionary.EmptyString);
                    if (!String.IsNullOrEmpty(genericIdentity.AuthenticationType))
                        writer.WriteAttributeString(dictionary.AuthenticationType, dictionary.EmptyString, genericIdentity.AuthenticationType);
                    writer.WriteString(genericIdentity.Name);
                    writer.WriteEndElement();
                }
                else
                {
                    serializer.WriteObject(writer, identity);
                }
                writer.WriteEndElement();
            }
        }

        public static IList<IIdentity> DeserializeIdentities(XmlDictionaryReader reader, SctClaimDictionary dictionary, XmlObjectSerializer serializer)
        {
            List<IIdentity> identities = null;
            if (reader.IsStartElement(dictionary.Identities, dictionary.EmptyString))
            {
                identities = new List<IIdentity>();
                reader.ReadStartElement();
                while (reader.IsStartElement(dictionary.PrimaryIdentity, dictionary.EmptyString))
                {
                    IIdentity identity = DeserializePrimaryIdentity(reader, dictionary, serializer);
                    if (identity != null && identity != SecurityUtils.AnonymousIdentity)
                    {
                        identities.Add(identity);
                    }
                }
                reader.ReadEndElement();
            }
            return identities;
        }

        static IIdentity DeserializePrimaryIdentity(XmlDictionaryReader reader, SctClaimDictionary dictionary, XmlObjectSerializer serializer)
        {
            IIdentity identity = null;
            if (reader.IsStartElement(dictionary.PrimaryIdentity, dictionary.EmptyString))
            {
                reader.ReadStartElement();
                if (reader.IsStartElement(dictionary.WindowsSidIdentity, dictionary.EmptyString))
                {
                    SecurityIdentifier sid = ReadSidAttribute(reader, dictionary);
                    string authenticationType = reader.GetAttribute(dictionary.AuthenticationType, dictionary.EmptyString);
                    reader.ReadStartElement();
                    string name = reader.ReadContentAsString();
                    identity = new WindowsSidIdentity(sid, name, authenticationType ?? String.Empty);
                    reader.ReadEndElement();
                }
                else if (reader.IsStartElement(dictionary.GenericIdentity, dictionary.EmptyString))
                {
                    string authenticationType = reader.GetAttribute(dictionary.AuthenticationType, dictionary.EmptyString);
                    reader.ReadStartElement();
                    string name = reader.ReadContentAsString();
                    identity = SecurityUtils.CreateIdentity(name, authenticationType ?? String.Empty);
                    reader.ReadEndElement();
                }
                else
                {
                    identity = (IIdentity)serializer.ReadObject(reader);
                }
                reader.ReadEndElement();
            }
            return identity;
        }
    }
}
