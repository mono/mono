//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    class EndpointAddressProcessor
    {
        internal static readonly QNameKeyComparer QNameComparer = new QNameKeyComparer();

        // QName Attributes
        internal static readonly string XsiNs = XmlSchema.InstanceNamespace;
        internal const string SerNs = "http://schemas.microsoft.com/2003/10/Serialization/";
        internal const string TypeLN = "type";
        internal const string ItemTypeLN = "ItemType";
        internal const string FactoryTypeLN = "FactoryType";

        // Pooling
        internal EndpointAddressProcessor next;

        StringBuilder builder;
        byte[] resultData;

        internal EndpointAddressProcessor(int length)
        {
            this.builder = new StringBuilder();
            this.resultData = new byte[length];
        }

        internal EndpointAddressProcessor Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        internal static string GetComparableForm(StringBuilder builder, XmlReader reader)
        {
            List<Attr> attrSet = new List<Attr>();
            int valueLength = -1;
            while (!reader.EOF)
            {
                XmlNodeType type = reader.MoveToContent();
                switch (type)
                {
                    case XmlNodeType.Element:
                        CompleteValue(builder, valueLength);
                        valueLength = -1;

                        builder.Append("<");
                        AppendString(builder, reader.LocalName);
                        builder.Append(":");
                        AppendString(builder, reader.NamespaceURI);
                        builder.Append(" ");

                        // Scan attributes
                        attrSet.Clear();
                        if (reader.MoveToFirstAttribute())
                        {
                            do
                            {
                                // Ignore namespaces
                                if (reader.Prefix == "xmlns" || reader.Name == "xmlns")
                                {
                                    continue;
                                }
                                if (reader.LocalName == AddressingStrings.IsReferenceParameter && reader.NamespaceURI == Addressing10Strings.Namespace)
                                {
                                    continue;  // ignore IsReferenceParameter
                                }

                                string val = reader.Value;
                                if ((reader.LocalName == TypeLN && reader.NamespaceURI == XsiNs) ||
                                    (reader.NamespaceURI == SerNs && (reader.LocalName == ItemTypeLN || reader.LocalName == FactoryTypeLN)))
                                {
                                    string local, ns;
                                    XmlUtil.ParseQName(reader, val, out local, out ns);
                                    val = local + "^" + local.Length.ToString(CultureInfo.InvariantCulture) + ":" + ns + "^" + ns.Length.ToString(CultureInfo.InvariantCulture);
                                }
                                else if (reader.LocalName == XD.UtilityDictionary.IdAttribute.Value && reader.NamespaceURI == XD.UtilityDictionary.Namespace.Value)
                                {
                                    // ignore wsu:Id attributes added by security to sign the header
                                    continue;
                                }
                                attrSet.Add(new Attr(reader.LocalName, reader.NamespaceURI, val));
                            } while (reader.MoveToNextAttribute());
                        }
                        reader.MoveToElement();

                        if (attrSet.Count > 0)
                        {
                            attrSet.Sort();
                            for (int i = 0; i < attrSet.Count; ++i)
                            {
                                Attr a = attrSet[i];

                                AppendString(builder, a.local);
                                builder.Append(":");
                                AppendString(builder, a.ns);
                                builder.Append("=\"");
                                AppendString(builder, a.val);
                                builder.Append("\" ");
                            }
                        }

                        if (reader.IsEmptyElement)
                            builder.Append("></>");  // Should be the same as an empty tag.
                        else
                            builder.Append(">");
                        break;

                    case XmlNodeType.EndElement:
                        CompleteValue(builder, valueLength);
                        valueLength = -1;
                        builder.Append("</>");
                        break;

                    // Need to escape CDATA values
                    case XmlNodeType.CDATA:
                        CompleteValue(builder, valueLength);
                        valueLength = -1;

                        builder.Append("<![CDATA[");
                        AppendString(builder, reader.Value);
                        builder.Append("]]>");
                        break;

                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.Text:
                        if (valueLength < 0)
                            valueLength = builder.Length;

                        builder.Append(reader.Value);
                        break;

                    default:
                        // Do nothing
                        break;
                }
                reader.Read();
            }
            return builder.ToString();
        }

        static void AppendString(StringBuilder builder, string s)
        {
            builder.Append(s);
            builder.Append("^");
            builder.Append(s.Length.ToString(CultureInfo.InvariantCulture));
        }

        static void CompleteValue(StringBuilder builder, int startLength)
        {
            if (startLength < 0)
                return;

            int len = builder.Length - startLength;
            builder.Append("^");
            builder.Append(len.ToString(CultureInfo.InvariantCulture));
        }

        internal void Clear(int length)
        {
            if (this.resultData.Length == length)
            {
                Array.Clear(this.resultData, 0, this.resultData.Length);
            }
            else
            {
                this.resultData = new byte[length];
            }
        }

        internal void ProcessHeaders(Message msg, Dictionary<QName, int> qnameLookup, Dictionary<string, HeaderBit[]> headerLookup)
        {
            string key;
            HeaderBit[] bits;
            QName qname;
            MessageHeaders headers = msg.Headers;
            for (int j = 0; j < headers.Count; ++j)
            {
                qname.name = headers[j].Name;
                qname.ns = headers[j].Namespace;
                if (headers.MessageVersion.Addressing == AddressingVersion.WSAddressing10
                    && !headers[j].IsReferenceParameter)
                {
                    continue;
                }
                if (qnameLookup.ContainsKey(qname))
                {
                    builder.Remove(0, builder.Length);
                    XmlReader reader = headers.GetReaderAtHeader(j).ReadSubtree();
                    reader.Read();  // Needed after call to ReadSubtree
                    key = GetComparableForm(builder, reader);

                    if (headerLookup.TryGetValue(key, out bits))
                    {
                        SetBit(bits);
                    }
                }
            }
        }

        internal void SetBit(HeaderBit[] bits)
        {
            if (bits.Length == 1)
            {
                this.resultData[bits[0].index] |= bits[0].mask;
            }
            else
            {
                byte[] results = this.resultData;
                for (int i = 0; i < bits.Length; ++i)
                {
                    if ((results[bits[i].index] & bits[i].mask) == 0)
                    {
                        results[bits[i].index] |= bits[i].mask;
                        break;
                    }
                }
            }
        }

        internal bool TestExact(byte[] exact)
        {
            Fx.Assert(this.resultData.Length == exact.Length, "");

            byte[] results = this.resultData;
            for (int i = 0; i < exact.Length; ++i)
            {
                if (results[i] != exact[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal bool TestMask(byte[] mask)
        {
            if (mask == null)
            {
                return true;
            }

            byte[] results = this.resultData;
            for (int i = 0; i < mask.Length; ++i)
            {
                if ((results[i] & mask[i]) != mask[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal struct QName
        {
            internal string name;
            internal string ns;
        }

        internal class QNameKeyComparer : IComparer<QName>, IEqualityComparer<QName>
        {
            internal QNameKeyComparer()
            {
            }

            public int Compare(QName x, QName y)
            {
                int i = string.CompareOrdinal(x.name, y.name);
                if (i != 0)
                    return i;

                return string.CompareOrdinal(x.ns, y.ns);
            }

            public bool Equals(QName x, QName y)
            {
                int i = string.CompareOrdinal(x.name, y.name);
                if (i != 0)
                    return false;

                return string.CompareOrdinal(x.ns, y.ns) == 0;
            }

            public int GetHashCode(QName obj)
            {
                return obj.name.GetHashCode() ^ obj.ns.GetHashCode();
            }
        }

        internal struct HeaderBit
        {
            internal int index;
            internal byte mask;

            internal HeaderBit(int bitNum)
            {
                this.index = bitNum / 8;
                this.mask = (byte)(1 << (bitNum % 8));
            }

            internal void AddToMask(ref byte[] mask)
            {
                if (mask == null)
                {
                    mask = new byte[this.index + 1];
                }
                else if (mask.Length <= this.index)
                {
                    Array.Resize(ref mask, this.index + 1);
                }

                mask[this.index] |= this.mask;
            }
        }

        class Attr : IComparable<Attr>
        {
            internal string local;
            internal string ns;
            internal string val;
            string key;

            internal Attr(string l, string ns, string v)
            {
                this.local = l;
                this.ns = ns;
                this.val = v;
                this.key = ns + ":" + l;
            }

            public int CompareTo(Attr a)
            {
                return string.Compare(this.key, a.key, StringComparison.Ordinal);
            }
        }
    }
}
