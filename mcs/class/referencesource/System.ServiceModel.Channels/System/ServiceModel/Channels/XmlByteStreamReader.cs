//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Xml;

    abstract class XmlByteStreamReader : XmlDictionaryReader
    {
        string base64StringValue;
        bool closed;
        NameTable nameTable;

        protected ReaderPosition position;
        protected XmlDictionaryReaderQuotas quotas;
        bool readBase64AsString;
        protected XmlByteStreamReader(XmlDictionaryReaderQuotas quotas)
        {
            this.quotas = quotas;
            this.position = ReaderPosition.None; 
        }

        public override int AttributeCount
        {
            get { return 0; }
        }

        public override string BaseURI
        {
            get { return string.Empty; }
        }

        public override bool CanCanonicalize
        {
            get { return false; }
        }

        public override bool CanReadBinaryContent
        {
            get { return true; }
        }

        public override bool CanReadValueChunk
        {
            get { return false; }
        }

        public override bool CanResolveEntity
        {
            get { return false; }
        }

        public override int Depth
        {
            get { return (this.position == ReaderPosition.Content) ? 1 : 0; }
        }

        public override bool EOF
        {
            get { return (this.position == ReaderPosition.EOF); }
        }

        public override bool HasAttributes
        {
            get { return false; }
        }

        public override bool HasValue
        {
            get { return (this.position == ReaderPosition.Content); }
        }

        public override bool IsDefault
        {
            get { return false; }
        }

        public override bool IsEmptyElement
        {
            get { return false; }
        }

        public override string LocalName
        {
            get { return (this.position == ReaderPosition.StartElement) ? ByteStreamMessageUtility.StreamElementName : null; }
        }

        public override void MoveToStartElement()
        {
            base.MoveToStartElement();
            this.position = ReaderPosition.StartElement; 
        }

        public override XmlNameTable NameTable
        {
            get
            {
                if (this.nameTable == null)
                {
                    this.nameTable = new NameTable();
                    this.nameTable.Add(ByteStreamMessageUtility.StreamElementName);
                }
                return this.nameTable;
            }
        }

        public override string NamespaceURI
        {
            get { return string.Empty; }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                switch (position)
                {
                    case ReaderPosition.StartElement:
                        return XmlNodeType.Element;
                    case ReaderPosition.Content:
                        return XmlNodeType.Text;
                    case ReaderPosition.EndElement:
                        return XmlNodeType.EndElement;
                    default:
                        // and StreamPosition.EOF
                        return XmlNodeType.None;
                }
            }
        }

        public override string Prefix
        {
            get { return string.Empty; }
        }

        public override XmlDictionaryReaderQuotas Quotas
        {
            get { return this.quotas; }
        }

        public override ReadState ReadState
        {
            get
            {
                switch (this.position)
                {
                    case ReaderPosition.None:
                        return ReadState.Initial;
                    case ReaderPosition.StartElement:
                    case ReaderPosition.Content:
                    case ReaderPosition.EndElement:
                        return ReadState.Interactive;
                    case ReaderPosition.EOF:
                        return ReadState.Closed;
                    default:
                        Fx.Assert("Unknown ReadState hit in XmlByteStreamReader");
                        return ReadState.Error;
                }
            }
        }

        public override string Value
        {
            get
            {
                switch (this.position)
                {
                    case ReaderPosition.Content:
                        if (!this.readBase64AsString)
                        {
                            this.base64StringValue = Convert.ToBase64String(ReadContentAsBase64());
                            this.readBase64AsString = true;
                        }
                        return this.base64StringValue;

                    default:
                        return string.Empty;
                }
            }
        }

        public override void Close()
        {
            if (!this.closed)
            {
                try
                {
                    this.OnClose(); 
                }
                finally
                {
                    this.position = ReaderPosition.EOF;
                    this.closed = true;
                }
            }
        }

        protected bool IsClosed
        {
            get { return this.closed; }
        }

        protected virtual void OnClose()
        {            
        }

        public override string GetAttribute(int i)
        {
            throw FxTrace.Exception.ArgumentOutOfRange("i", i, SR.ArgumentNotInSetOfValidValues);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return null;
        }

        public override string GetAttribute(string name)
        {
            return null;
        }

        public override string LookupNamespace(string prefix)
        {
            if (prefix == string.Empty)
            {
                return string.Empty;
            }
            else if (prefix == "xml")
            {
                return ByteStreamMessageUtility.XmlNamespace;
            }
            else if (prefix == "xmlns")
            {
                return ByteStreamMessageUtility.XmlNamespaceNamespace;
            }
            else
            {
                return null;
            }
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return false;
        }

        public override bool MoveToAttribute(string name)
        {
            return false;
        }

        public override bool MoveToElement()
        {
            if (this.position == ReaderPosition.None)
            {
                this.position = ReaderPosition.StartElement;
                return true;
            }
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            return false;
        }

        public override bool Read()
        {
            switch (this.position)
            {
                case ReaderPosition.None:
                    position = ReaderPosition.StartElement;
                    return true;
                case ReaderPosition.StartElement:
                    position = ReaderPosition.Content;
                    return true;
                case ReaderPosition.Content:
                    position = ReaderPosition.EndElement;
                    return true;
                case ReaderPosition.EndElement:
                    position = ReaderPosition.EOF;
                    return false;
                case ReaderPosition.EOF:
                    return false;
                default:
                    // we should never get here
                    // it means we managed to get into some unknown position in the stream
                    Fx.Assert(false, "Unknown read position in XmlByteStreamReader");
                    return false;
            }
        }

        public override bool ReadAttributeValue()
        {
            return false;
        }
        
        public override abstract int ReadContentAsBase64(byte[] buffer, int index, int count);

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void ResolveEntity()
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public byte[] ToByteArray()
        {
            if (this.IsClosed)
            {
                throw FxTrace.Exception.AsError(
                    new ObjectDisposedException(SR.XmlReaderClosed));
            }
            return this.OnToByteArray();
        }

        protected abstract byte[] OnToByteArray();

        public Stream ToStream()
        { 
            if (this.IsClosed)
            {
                throw FxTrace.Exception.AsError(
                    new ObjectDisposedException(SR.XmlReaderClosed));
            }
            return this.OnToStream();
        }

        protected abstract Stream OnToStream();

        protected void EnsureInContent()
        {
            // This method is only being called from XmlByteStreamReader.ReadContentAsBase64.
            // We don't block if the position is None or StartElement since we want our XmlByteStreamReader
            // to be a little bit smarter when people just to access the content of the ByteStreamMessage.
            if (this.position == ReaderPosition.EndElement
             || this.position == ReaderPosition.EOF)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.ByteStreamReaderNotInByteStream(ByteStreamMessageUtility.StreamElementName)));
            }
            else
            {
                this.position = ReaderPosition.Content;
            }
        }

        protected enum ReaderPosition
        {
            None,
            StartElement,
            Content,
            EndElement,
            EOF
        }
    }
}
