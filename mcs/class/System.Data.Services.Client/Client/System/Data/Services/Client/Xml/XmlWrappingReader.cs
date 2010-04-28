//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client.Xml
{
    #region Namespaces.

    using System.Xml;
    using System.Xml.Schema;
    using System.Collections.Generic;
    using System.Diagnostics;

    #endregion Namespaces.

    internal class XmlWrappingReader : XmlReader, IXmlLineInfo
    {
        #region Private fields.

        private XmlReader reader;

        private IXmlLineInfo readerAsIXmlLineInfo;

        private Stack<XmlBaseState> xmlBaseStack;

        private string previousReaderBaseUri;

        #endregion Private fields.

        internal XmlWrappingReader(XmlReader baseReader)
        {
            this.Reader = baseReader;
        }

        #region Properties.

        public override int AttributeCount
        {
            get
            {
                return this.reader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                if (this.xmlBaseStack != null && this.xmlBaseStack.Count > 0)
                {
                    return this.xmlBaseStack.Peek().BaseUri.AbsoluteUri;
                }
                else if (!String.IsNullOrEmpty(this.previousReaderBaseUri))
                {
                    return this.previousReaderBaseUri;
                }

                return this.reader.BaseURI;
            }
        }

        public override bool CanResolveEntity
        {
            get
            {
                return this.reader.CanResolveEntity;
            }
        }

        public override int Depth
        {
            get
            {
                return this.reader.Depth;
            }
        }


        public override bool EOF
        {
            get
            {
                return this.reader.EOF;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return this.reader.HasAttributes;
            }
        }

        public override bool HasValue
        {
            get
            {
                return this.reader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this.reader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.reader.IsEmptyElement;
            }
        }

        public virtual int LineNumber
        {
            get
            {
                if (this.readerAsIXmlLineInfo != null)
                {
                    return this.readerAsIXmlLineInfo.LineNumber;
                }

                return 0;
            }
        }

        public virtual int LinePosition
        {
            get
            {
                if (this.readerAsIXmlLineInfo != null)
                {
                    return this.readerAsIXmlLineInfo.LinePosition;
                }

                return 0;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.reader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.reader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.reader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.reader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.reader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.reader.Prefix;
            }
        }

#if !ASTORIA_LIGHT

        public override char QuoteChar
        {
            get
            {
                return this.reader.QuoteChar;
            }
        }

#endif

        public override ReadState ReadState
        {
            get
            {
                return this.reader.ReadState;
            }
        }

#if !ASTORIA_LIGHT

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return this.reader.SchemaInfo;
            }
        }
#endif

        public override XmlReaderSettings Settings
        {
            get
            {
                return this.reader.Settings;
            }
        }

        public override string Value
        {
            get
            {
                return this.reader.Value;
            }
        }

        public override Type ValueType
        {
            get
            {
                return this.reader.ValueType;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.reader.XmlLang;
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                return this.reader.XmlSpace;
            }
        }

        protected XmlReader Reader
        {
            get
            {
                return this.reader;
            }

            set
            {
                this.reader = value;
                this.readerAsIXmlLineInfo = value as IXmlLineInfo;
            }
        }

        #endregion Properties.

        #region Methods.

        public override void Close()
        {
            this.reader.Close();
        }

        public override string GetAttribute(int i)
        {
            return this.reader.GetAttribute(i);
        }

        public override string GetAttribute(string name)
        {
            return this.reader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return this.reader.GetAttribute(name, namespaceURI);
        }

        public virtual bool HasLineInfo()
        {
            return ((this.readerAsIXmlLineInfo != null) && this.readerAsIXmlLineInfo.HasLineInfo());
        }

        public override string LookupNamespace(string prefix)
        {
            return this.reader.LookupNamespace(prefix);
        }

        public override void MoveToAttribute(int i)
        {
            this.reader.MoveToAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            return this.reader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return this.reader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            return this.reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return this.reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this.reader.MoveToNextAttribute();
        }

        public override bool Read()
        {
            if (this.reader.NodeType == XmlNodeType.EndElement)
            {
                this.PopXmlBase();
            }
            else
            {
                this.reader.MoveToElement();
                if (this.reader.IsEmptyElement)
                {
                    this.PopXmlBase();
                }
            }

            bool result = this.reader.Read();
            if (result) 
            {
                if (this.reader.NodeType == XmlNodeType.Element &&
                    this.reader.HasAttributes) 
                {
                    string baseAttribute = this.reader.GetAttribute(XmlConstants.XmlBaseAttributeNameWithPrefix);
                    if (String.IsNullOrEmpty(baseAttribute))
                    {
                        return result;
                    }

                    Uri newBaseUri = null;
                    newBaseUri = Util.CreateUri(baseAttribute, UriKind.RelativeOrAbsolute);

                    if (this.xmlBaseStack == null)
                    {
                        this.xmlBaseStack = new Stack<XmlBaseState>();
                    }

                    if (this.xmlBaseStack.Count > 0)
                    {
                        newBaseUri = Util.CreateUri(this.xmlBaseStack.Peek().BaseUri, newBaseUri);
                    }

                    this.xmlBaseStack.Push(new XmlBaseState(newBaseUri, this.reader.Depth));
                }
            }

            return result;
        }

        public override bool ReadAttributeValue()
        {
            return this.reader.ReadAttributeValue();
        }

        public override void ResolveEntity()
        {
            this.reader.ResolveEntity();
        }

        public override void Skip()
        {
            this.reader.Skip();
        }

        internal static XmlWrappingReader CreateReader(string currentBaseUri, XmlReader newReader)
        {
            Debug.Assert(!(newReader is XmlWrappingReader), "The new reader must not be a xmlWrappingReader");
            XmlWrappingReader reader = new XmlWrappingReader(newReader);
            reader.previousReaderBaseUri = currentBaseUri;
            return reader;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.reader != null)
            {
                ((IDisposable)this.reader).Dispose();
            }

            base.Dispose(disposing);
        }

        private void PopXmlBase()
        {
            if (this.xmlBaseStack != null && this.xmlBaseStack.Count > 0 && this.reader.Depth == this.xmlBaseStack.Peek().Depth)
            {
                this.xmlBaseStack.Pop();
            }
        }

        #endregion Methods.

        #region Private Class

        private class XmlBaseState
        {
            internal XmlBaseState(Uri baseUri, int depth)
            {
                this.BaseUri = baseUri;
                this.Depth = depth;
            }

            public Uri BaseUri
            {
                get;
                private set;
            }

            public int Depth
            {
                get;
                private set;
            }
        }

        #endregion    
    }
}
