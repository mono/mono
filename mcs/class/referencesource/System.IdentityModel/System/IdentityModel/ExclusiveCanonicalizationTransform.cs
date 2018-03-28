//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.IO;
    using System.Security.Cryptography;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Text;
    using System.Xml;

    class ExclusiveCanonicalizationTransform : Transform
    {
        bool includeComments;
        string algorithm;
        string inclusiveNamespacesPrefixList;
        string[] inclusivePrefixes;
        string inclusiveListElementPrefix = ExclusiveC14NStrings.Prefix;
        string prefix = XmlSignatureStrings.Prefix;
        readonly bool isCanonicalizationMethod;

        public ExclusiveCanonicalizationTransform()
            : this(false)
        {
        }

        public ExclusiveCanonicalizationTransform(bool isCanonicalizationMethod)
            : this(isCanonicalizationMethod, false)
        {            
        }

        public ExclusiveCanonicalizationTransform(bool isCanonicalizationMethod, bool includeComments)
        {
            this.isCanonicalizationMethod = isCanonicalizationMethod;
            this.includeComments = includeComments;
            this.algorithm = includeComments ? XD.SecurityAlgorithmDictionary.ExclusiveC14nWithComments.Value : XD.SecurityAlgorithmDictionary.ExclusiveC14n.Value;
        }

        public override string Algorithm
        {
            get
            {
                return algorithm;                
            }
        }
        
        public bool IncludeComments
        {
            get
            {
                return this.includeComments;
            }
        }

        public string InclusiveNamespacesPrefixList
        {
            get
            {
                return this.inclusiveNamespacesPrefixList;
            }
            set
            {
                this.inclusiveNamespacesPrefixList = value;
                this.inclusivePrefixes = TokenizeInclusivePrefixList(value);
            }
        }

        public override bool NeedsInclusiveContext
        {
            get { return GetInclusivePrefixes() != null; }
        }

        public string[] GetInclusivePrefixes()
        {
            return this.inclusivePrefixes;
        }

        CanonicalizationDriver GetConfiguredDriver(SignatureResourcePool resourcePool)
        {
            CanonicalizationDriver driver = resourcePool.TakeCanonicalizationDriver();
            driver.IncludeComments = this.IncludeComments;
            driver.SetInclusivePrefixes(this.inclusivePrefixes);
            return driver;
        }

        // multi-transform case, inefficient path
        public override object Process(object input, SignatureResourcePool resourcePool, DictionaryManager dictionaryManager)
        {
            if (input is XmlReader)
            {
                CanonicalizationDriver driver = GetConfiguredDriver(resourcePool);
                driver.SetInput(input as XmlReader);
                return driver.GetMemoryStream();
            }
            else if (input is ISecurityElement)
            {
                MemoryStream stream = new MemoryStream();
                XmlDictionaryWriter utf8Writer = resourcePool.TakeUtf8Writer();
                utf8Writer.StartCanonicalization(stream, false, null);
                (input as ISecurityElement).WriteTo(utf8Writer, dictionaryManager);
                utf8Writer.EndCanonicalization();
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedInputTypeForTransform, input.GetType())));
            }
        }

        // common single-transform case; fold directly into a digest
        public override byte[] ProcessAndDigest(object input, SignatureResourcePool resourcePool, string digestAlgorithm, DictionaryManager dictionaryManager)
        {
            HashAlgorithm hash = resourcePool.TakeHashAlgorithm(digestAlgorithm);
            ProcessAndDigest(input, resourcePool, hash, dictionaryManager);
            return hash.Hash;
        }

        public void ProcessAndDigest(object input, SignatureResourcePool resourcePool, HashAlgorithm hash, DictionaryManager dictionaryManger)
        {
            HashStream hashStream = resourcePool.TakeHashStream(hash);

            XmlReader reader = input as XmlReader;
            if (reader != null)
            {
                ProcessReaderInput(reader, resourcePool, hashStream);
            }
            else if (input is ISecurityElement)
            {
                XmlDictionaryWriter utf8Writer = resourcePool.TakeUtf8Writer();
                utf8Writer.StartCanonicalization(hashStream, this.IncludeComments, GetInclusivePrefixes());
                (input as ISecurityElement).WriteTo(utf8Writer, dictionaryManger);
                utf8Writer.EndCanonicalization();
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedInputTypeForTransform, input.GetType())));
            }

            hashStream.FlushHash();
        }

        void ProcessReaderInput(XmlReader reader, SignatureResourcePool resourcePool, HashStream hashStream)
        {
            reader.MoveToContent();
            XmlDictionaryReader dictionaryReader = reader as XmlDictionaryReader;
            if (dictionaryReader != null && dictionaryReader.CanCanonicalize)
            {
                dictionaryReader.StartCanonicalization(hashStream, this.IncludeComments, GetInclusivePrefixes());
                dictionaryReader.Skip();
                dictionaryReader.EndCanonicalization();
            }
            else
            {
                CanonicalizationDriver driver = GetConfiguredDriver(resourcePool);
                driver.SetInput(reader);
                driver.WriteTo(hashStream);
            }
        }

        public override void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager, bool preserveComments)
        {
            XmlDictionaryString elementName = this.isCanonicalizationMethod ?
                dictionaryManager.XmlSignatureDictionary.CanonicalizationMethod : dictionaryManager.XmlSignatureDictionary.Transform;
            reader.MoveToStartElement(elementName, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            bool isEmptyElement = reader.IsEmptyElement;
            algorithm = reader.GetAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.ID0001, dictionaryManager.XmlSignatureDictionary.Algorithm, reader.LocalName)));
            }
            
            if (algorithm == dictionaryManager.SecurityAlgorithmDictionary.ExclusiveC14nWithComments.Value)
            {
                // to include comments in canonicalization, two conditions need to be met
                // 1. the Reference must be an xpointer.
                // 2. the transform must be #withComments
                includeComments = preserveComments && true;
            }
            else if (algorithm == dictionaryManager.SecurityAlgorithmDictionary.ExclusiveC14n.Value)
            {
                includeComments = false;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.ID6005, algorithm)));
            }

            reader.Read();
            reader.MoveToContent();

            if (!isEmptyElement)
            {
                if (reader.IsStartElement(dictionaryManager.ExclusiveC14NDictionary.InclusiveNamespaces, dictionaryManager.ExclusiveC14NDictionary.Namespace))
                {
                    reader.MoveToStartElement(dictionaryManager.ExclusiveC14NDictionary.InclusiveNamespaces, dictionaryManager.ExclusiveC14NDictionary.Namespace);
                    this.inclusiveListElementPrefix = reader.Prefix;
                    bool emptyElement = reader.IsEmptyElement;
                    // We treat PrefixList as optional Attribute.
                    this.InclusiveNamespacesPrefixList = reader.GetAttribute(dictionaryManager.ExclusiveC14NDictionary.PrefixList, null);
                    reader.Read();
                    if (!emptyElement)
                        reader.ReadEndElement();
                }
                reader.MoveToContent();
                reader.ReadEndElement(); // Transform
            }
        }

        public override void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            XmlDictionaryString elementName = this.isCanonicalizationMethod ?
                dictionaryManager.XmlSignatureDictionary.CanonicalizationMethod : dictionaryManager.XmlSignatureDictionary.Transform;
            writer.WriteStartElement(this.prefix, elementName, dictionaryManager.XmlSignatureDictionary.Namespace);
            writer.WriteAttributeString(dictionaryManager.XmlSignatureDictionary.Algorithm, null, algorithm);
            
            if (this.InclusiveNamespacesPrefixList != null)
            {
                writer.WriteStartElement(this.inclusiveListElementPrefix, dictionaryManager.ExclusiveC14NDictionary.InclusiveNamespaces, dictionaryManager.ExclusiveC14NDictionary.Namespace);
                writer.WriteAttributeString(dictionaryManager.ExclusiveC14NDictionary.PrefixList, null, this.InclusiveNamespacesPrefixList);
                writer.WriteEndElement(); // InclusiveNamespaces
            }

            writer.WriteEndElement(); // Transform
        }

        static string[] TokenizeInclusivePrefixList(string prefixList)
        {
            if (prefixList == null)
            {
                return null;
            }
            string[] prefixes = prefixList.Split(null);
            int count = 0;
            for (int i = 0; i < prefixes.Length; i++)
            {
                string prefix = prefixes[i];
                if (prefix == "#default")
                {
                    prefixes[count++] = string.Empty;
                }
                else if (prefix.Length > 0)
                {
                    prefixes[count++] = prefix;
                }
            }
            if (count == 0)
            {
                return null;
            }
            else if (count == prefixes.Length)
            {
                return prefixes;
            }
            else
            {
                string[] result = new string[count];
                Array.Copy(prefixes, result, count);
                return result;
            }
        }
    }
}
