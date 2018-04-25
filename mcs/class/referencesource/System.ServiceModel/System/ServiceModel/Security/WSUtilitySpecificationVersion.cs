//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.IO;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.ServiceModel.Security.Tokens;
    using System.Threading;
    using System.Globalization;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;
    
    using CanonicalFormWriter = System.IdentityModel.CanonicalFormWriter;
    using SignatureResourcePool = System.IdentityModel.SignatureResourcePool;
    using HashStream = System.IdentityModel.HashStream;

    abstract class WSUtilitySpecificationVersion
    {
        internal static readonly string[] AcceptedDateTimeFormats = new string[]
        {
            "yyyy-MM-ddTHH:mm:ss.fffffffZ",
            "yyyy-MM-ddTHH:mm:ss.ffffffZ",
            "yyyy-MM-ddTHH:mm:ss.fffffZ",
            "yyyy-MM-ddTHH:mm:ss.ffffZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:ss.ffZ",
            "yyyy-MM-ddTHH:mm:ss.fZ",
            "yyyy-MM-ddTHH:mm:ssZ"
        };

        readonly XmlDictionaryString namespaceUri;

        internal WSUtilitySpecificationVersion(XmlDictionaryString namespaceUri)
        {
            this.namespaceUri = namespaceUri;
        }

        public static WSUtilitySpecificationVersion Default
        {
            get { return OneDotZero; }
        }

        internal XmlDictionaryString NamespaceUri
        {
            get { return this.namespaceUri; }
        }

        public static WSUtilitySpecificationVersion OneDotZero
        {
            get { return WSUtilitySpecificationVersionOneDotZero.Instance; }
        }

        internal abstract bool IsReaderAtTimestamp(XmlDictionaryReader reader);

        internal abstract SecurityTimestamp ReadTimestamp(XmlDictionaryReader reader, string digestAlgorithm, SignatureResourcePool resourcePool);

        internal abstract void WriteTimestamp(XmlDictionaryWriter writer, SecurityTimestamp timestamp);

        internal abstract void WriteTimestampCanonicalForm(Stream stream, SecurityTimestamp timestamp, byte[] buffer);

        sealed class WSUtilitySpecificationVersionOneDotZero : WSUtilitySpecificationVersion
        {
            static readonly WSUtilitySpecificationVersionOneDotZero instance = new WSUtilitySpecificationVersionOneDotZero();

            WSUtilitySpecificationVersionOneDotZero()
                : base(XD.UtilityDictionary.Namespace)
            {
            }

            public static WSUtilitySpecificationVersionOneDotZero Instance
            {
                get { return instance; }
            }

            internal override bool IsReaderAtTimestamp(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(XD.UtilityDictionary.Timestamp, XD.UtilityDictionary.Namespace);
            }

            internal override SecurityTimestamp ReadTimestamp(XmlDictionaryReader reader, string digestAlgorithm, SignatureResourcePool resourcePool)
            {
                bool canonicalize = digestAlgorithm != null && reader.CanCanonicalize;
                HashStream hashStream = null;

                reader.MoveToStartElement(XD.UtilityDictionary.Timestamp, XD.UtilityDictionary.Namespace);
                if (canonicalize)
                {
                    hashStream = resourcePool.TakeHashStream(digestAlgorithm);
                    reader.StartCanonicalization(hashStream, false, null);
                }
                string id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                reader.ReadStartElement();

                reader.ReadStartElement(XD.UtilityDictionary.CreatedElement, XD.UtilityDictionary.Namespace);
                DateTime creationTimeUtc = reader.ReadContentAsDateTime().ToUniversalTime();
                reader.ReadEndElement();

                DateTime expiryTimeUtc;
                if (reader.IsStartElement(XD.UtilityDictionary.ExpiresElement, XD.UtilityDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    expiryTimeUtc = reader.ReadContentAsDateTime().ToUniversalTime();
                    reader.ReadEndElement();
                }
                else
                {
                    expiryTimeUtc = SecurityUtils.MaxUtcDateTime;
                }

                reader.ReadEndElement();

                byte[] digest;
                if (canonicalize)
                {
                    reader.EndCanonicalization();
                    digest = hashStream.FlushHashAndGetValue();
                }
                else
                {
                    digest = null;
                }
                return new SecurityTimestamp(creationTimeUtc, expiryTimeUtc, id, digestAlgorithm, digest);
            }

            internal override void WriteTimestamp(XmlDictionaryWriter writer, SecurityTimestamp timestamp)
            {
                writer.WriteStartElement(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.Timestamp, XD.UtilityDictionary.Namespace);
                writer.WriteAttributeString(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, timestamp.Id);

                writer.WriteStartElement(XD.UtilityDictionary.CreatedElement, XD.UtilityDictionary.Namespace);
                char[] creationTime = timestamp.GetCreationTimeChars();
                writer.WriteChars(creationTime, 0, creationTime.Length);
                writer.WriteEndElement(); // wsu:Created

                writer.WriteStartElement(XD.UtilityDictionary.ExpiresElement, XD.UtilityDictionary.Namespace);
                char[] expiryTime = timestamp.GetExpiryTimeChars();
                writer.WriteChars(expiryTime, 0, expiryTime.Length);
                writer.WriteEndElement(); // wsu:Expires

                writer.WriteEndElement();
            }

            internal override void WriteTimestampCanonicalForm(Stream stream, SecurityTimestamp timestamp, byte[] workBuffer)
            {
                TimestampCanonicalFormWriter.Instance.WriteCanonicalForm(
                    stream,
                    timestamp.Id, timestamp.GetCreationTimeChars(), timestamp.GetExpiryTimeChars(),
                    workBuffer);
            }
        }

        sealed class TimestampCanonicalFormWriter : CanonicalFormWriter
        {
            const string timestamp = UtilityStrings.Prefix + ":" + UtilityStrings.Timestamp;
            const string created = UtilityStrings.Prefix + ":" + UtilityStrings.CreatedElement;
            const string expires = UtilityStrings.Prefix + ":" + UtilityStrings.ExpiresElement;
            const string idAttribute = UtilityStrings.Prefix + ":" + UtilityStrings.IdAttribute;
            const string ns = "xmlns:" + UtilityStrings.Prefix + "=\"" + UtilityStrings.Namespace + "\"";

            const string xml1 = "<" + timestamp + " " + ns + " " + idAttribute + "=\"";
            const string xml2 = "\"><" + created + ">";
            const string xml3 = "</" + created + "><" + expires + ">";
            const string xml4 = "</" + expires + "></" + timestamp + ">";

            readonly byte[] fragment1;
            readonly byte[] fragment2;
            readonly byte[] fragment3;
            readonly byte[] fragment4;

            static readonly TimestampCanonicalFormWriter instance = new TimestampCanonicalFormWriter();

            TimestampCanonicalFormWriter()
            {
                UTF8Encoding encoding = CanonicalFormWriter.Utf8WithoutPreamble;
                this.fragment1 = encoding.GetBytes(xml1);
                this.fragment2 = encoding.GetBytes(xml2);
                this.fragment3 = encoding.GetBytes(xml3);
                this.fragment4 = encoding.GetBytes(xml4);
            }

            public static TimestampCanonicalFormWriter Instance
            {
                get { return instance; }
            }

            public void WriteCanonicalForm(Stream stream, string id, char[] created, char[] expires, byte[] workBuffer)
            {
                stream.Write(this.fragment1, 0, this.fragment1.Length);
                EncodeAndWrite(stream, workBuffer, id);
                stream.Write(this.fragment2, 0, this.fragment2.Length);
                EncodeAndWrite(stream, workBuffer, created);
                stream.Write(this.fragment3, 0, this.fragment3.Length);
                EncodeAndWrite(stream, workBuffer, expires);
                stream.Write(this.fragment4, 0, this.fragment4.Length);
            }
        }
    }
}

