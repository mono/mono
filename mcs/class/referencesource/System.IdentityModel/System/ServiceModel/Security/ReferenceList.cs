//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using DictionaryManager = System.IdentityModel.DictionaryManager;
    using ISecurityElement = System.IdentityModel.ISecurityElement;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    sealed class ReferenceList : ISecurityElement
    {
        internal static readonly XmlDictionaryString ElementName = XD.XmlEncryptionDictionary.ReferenceList;
        const string NamespacePrefix = XmlEncryptionStrings.Prefix;
        internal static readonly XmlDictionaryString NamespaceUri = EncryptedType.NamespaceUri;
        internal static readonly XmlDictionaryString UriAttribute = XD.XmlEncryptionDictionary.URI;
        List<string> referredIds = new List<string>();

        public ReferenceList()
        {
        }

        public int DataReferenceCount
        {
            get { return this.referredIds.Count; }
        }

        public bool HasId
        {
            get { return false; }
        }

        public string Id
        {
            get
            {
                // PreSharp 
                #pragma warning suppress 56503
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        public void AddReferredId(string id)
        {
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("id"));
            }
            this.referredIds.Add(id);
        }

        public bool ContainsReferredId(string id)
        {
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("id"));
            }
            return this.referredIds.Contains(id);
        }

        public string GetReferredId(int index)
        {
            return this.referredIds[index];
        }

        public void ReadFrom(XmlDictionaryReader reader)
        {
            reader.ReadStartElement(ElementName, NamespaceUri);
            while (reader.IsStartElement())
            {
                string id = DataReference.ReadFrom(reader);
                if (this.referredIds.Contains(id))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityMessageSerializationException(SR.GetString(SR.InvalidDataReferenceInReferenceList, "#" + id)));
                }
                this.referredIds.Add(id);
            }
            reader.ReadEndElement(); // ReferenceList
            if (this.DataReferenceCount == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityMessageSerializationException(SR.GetString(SR.ReferenceListCannotBeEmpty)));
            }
        }

        public bool TryRemoveReferredId(string id)
        {
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("id"));
            }
            return this.referredIds.Remove(id);
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            if (this.DataReferenceCount == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ReferenceListCannotBeEmpty)));
            }
            writer.WriteStartElement(NamespacePrefix, ElementName, NamespaceUri);
            for (int i = 0; i < this.DataReferenceCount; i++)
            {
                DataReference.WriteTo(writer, this.referredIds[i]);
            }
            writer.WriteEndElement(); // ReferenceList
        }

        static class DataReference
        {
            internal static readonly XmlDictionaryString ElementName = XD.XmlEncryptionDictionary.DataReference;
            internal static readonly XmlDictionaryString NamespaceUri = EncryptedType.NamespaceUri;

            public static string ReadFrom(XmlDictionaryReader reader)
            {
                string prefix;
                string uri = XmlHelper.ReadEmptyElementAndRequiredAttribute(reader, ElementName, NamespaceUri, ReferenceList.UriAttribute, out prefix);
                if (uri.Length < 2 || uri[0] != '#')
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityMessageSerializationException(SR.GetString(SR.InvalidDataReferenceInReferenceList, uri)));
                }
                return uri.Substring(1);
            }

            public static void WriteTo(XmlDictionaryWriter writer, string referredId)
            {
                writer.WriteStartElement(XD.XmlEncryptionDictionary.Prefix.Value, ElementName, NamespaceUri);
                writer.WriteStartAttribute(ReferenceList.UriAttribute, null);
                writer.WriteString("#");
                writer.WriteString(referredId);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
        }
    }
}
