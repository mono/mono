//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.ServiceModel.Security;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;

    public sealed class AddressHeaderCollection : ReadOnlyCollection<AddressHeader>
    {
        static AddressHeaderCollection emptyHeaderCollection = new AddressHeaderCollection();

        public AddressHeaderCollection()
            : base(new List<AddressHeader>())
        {
        }

        public AddressHeaderCollection(IEnumerable<AddressHeader> addressHeaders)
            : base(new List<AddressHeader>(addressHeaders))
        {
            // avoid allocating an enumerator when possible
            IList<AddressHeader> collection = addressHeaders as IList<AddressHeader>;
            if (collection != null)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i] == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MessageHeaderIsNull0)));
                }
            }
            else
            {
                foreach (AddressHeader addressHeader in addressHeaders)
                {
                    if (addressHeaders == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MessageHeaderIsNull0)));
                }
            }
        }

        internal static AddressHeaderCollection EmptyHeaderCollection
        {
            get { return emptyHeaderCollection; }
        }

        int InternalCount
        {
            get
            {
                if (this == (object)emptyHeaderCollection)
                    return 0;
                return Count;
            }
        }

        public void AddHeadersTo(Message message)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");

            for (int i = 0; i < InternalCount; i++)
            {
#pragma warning suppress 56506 // [....], Message.Headers can never be null
                message.Headers.Add(this[i].ToMessageHeader());
            }
        }

        public AddressHeader[] FindAll(string name, string ns)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            if (ns == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));

            List<AddressHeader> results = new List<AddressHeader>();
            for (int i = 0; i < Count; i++)
            {
                AddressHeader header = this[i];
                if (header.Name == name && header.Namespace == ns)
                {
                    results.Add(header);
                }
            }

            return results.ToArray();
        }

        public AddressHeader FindHeader(string name, string ns)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            if (ns == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));

            AddressHeader matchingHeader = null;

            for (int i = 0; i < Count; i++)
            {
                AddressHeader header = this[i];
                if (header.Name == name && header.Namespace == ns)
                {
                    if (matchingHeader != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MultipleMessageHeaders, name, ns)));
                    matchingHeader = header;
                }
            }

            return matchingHeader;
        }

        internal bool IsEquivalent(AddressHeaderCollection col)
        {
            if (InternalCount != col.InternalCount)
                return false;

            StringBuilder builder = new StringBuilder();
            Dictionary<string, int> myHeaders = new Dictionary<string, int>();
            PopulateHeaderDictionary(builder, myHeaders);

            Dictionary<string, int> otherHeaders = new Dictionary<string, int>();
            col.PopulateHeaderDictionary(builder, otherHeaders);

            if (myHeaders.Count != otherHeaders.Count)
                return false;

            foreach (KeyValuePair<string, int> pair in myHeaders)
            {
                int count;
                if (otherHeaders.TryGetValue(pair.Key, out count))
                {
                    if (count != pair.Value)
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        internal void PopulateHeaderDictionary(StringBuilder builder, Dictionary<string, int> headers)
        {
            string key;
            for (int i = 0; i < InternalCount; ++i)
            {
                builder.Remove(0, builder.Length);
                key = this[i].GetComparableForm(builder);
                if (headers.ContainsKey(key))
                {
                    headers[key] = headers[key] + 1;
                }
                else
                {
                    headers.Add(key, 1);
                }
            }
        }

        internal static AddressHeaderCollection ReadServiceParameters(XmlDictionaryReader reader)
        {
            return ReadServiceParameters(reader, false);
        }

        internal static AddressHeaderCollection ReadServiceParameters(XmlDictionaryReader reader, bool isReferenceProperty)
        {
            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                reader.Skip();
                return null;
            }
            else
            {
                reader.ReadStartElement();
                List<AddressHeader> headerList = new List<AddressHeader>();
                while (reader.IsStartElement())
                {
                    headerList.Add(new BufferedAddressHeader(reader, isReferenceProperty));
                }
                reader.ReadEndElement();
                return new AddressHeaderCollection(headerList);
            }
        }

        internal bool HasReferenceProperties
        {
            get
            {
                for (int i = 0; i < InternalCount; i++)
                    if (this[i].IsReferenceProperty)
                        return true;
                return false;
            }
        }

        internal bool HasNonReferenceProperties
        {
            get
            {
                for (int i = 0; i < InternalCount; i++)
                    if (!this[i].IsReferenceProperty)
                        return true;
                return false;
            }
        }

        internal void WriteReferencePropertyContentsTo(XmlDictionaryWriter writer)
        {
            for (int i = 0; i < InternalCount; i++)
                if (this[i].IsReferenceProperty)
                    this[i].WriteAddressHeader(writer);
        }

        internal void WriteNonReferencePropertyContentsTo(XmlDictionaryWriter writer)
        {
            for (int i = 0; i < InternalCount; i++)
                if (!this[i].IsReferenceProperty)
                    this[i].WriteAddressHeader(writer);
        }

        internal void WriteContentsTo(XmlDictionaryWriter writer)
        {
            for (int i = 0; i < InternalCount; i++)
                this[i].WriteAddressHeader(writer);
        }
    }
}
