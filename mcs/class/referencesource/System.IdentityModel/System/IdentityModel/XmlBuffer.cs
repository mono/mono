//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#define BINARY
namespace System.IdentityModel
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Xml;

    class XmlBuffer
    {
        List<Section> sections;
        byte[] buffer;
        int offset;
        BufferedOutputStream stream;
        BufferState bufferState;
        XmlDictionaryWriter writer;
        XmlDictionaryReaderQuotas quotas;

        enum BufferState
        {
            Created,
            Writing,
            Reading,
        }

        struct Section
        {
            int offset;
            int size;
            XmlDictionaryReaderQuotas quotas;

            public Section(int offset, int size, XmlDictionaryReaderQuotas quotas)
            {
                this.offset = offset;
                this.size = size;
                this.quotas = quotas;
            }

            public int Offset
            {
                get { return this.offset; }
            }

            public int Size
            {
                get { return this.size; }
            }

            public XmlDictionaryReaderQuotas Quotas
            {
                get { return this.quotas; }
            }
        }

        public XmlBuffer(int maxBufferSize)
        {
            if (maxBufferSize < 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize", maxBufferSize,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
            int initialBufferSize = Math.Min(512, maxBufferSize);
            stream = new BufferManagerOutputStream(SR.XmlBufferQuotaExceeded, initialBufferSize, maxBufferSize,
                BufferManager.CreateBufferManager(0, int.MaxValue));
            sections = new List<Section>(1);
        }

        public int BufferSize
        {
            get
            {
                Fx.Assert(bufferState == BufferState.Reading, "Buffer size shuold only be retrieved during Reading state");
                return buffer.Length;
            }
        }

        public int SectionCount
        {
            get { return this.sections.Count; }
        }

        public XmlDictionaryWriter OpenSection(XmlDictionaryReaderQuotas quotas)
        {
            if (bufferState != BufferState.Created)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidStateException());
            bufferState = BufferState.Writing;
            this.quotas = new XmlDictionaryReaderQuotas();
            quotas.CopyTo(this.quotas);
            if (this.writer == null)
            {
                this.writer = XmlDictionaryWriter.CreateBinaryWriter(stream, XD.Dictionary, null, true);
            }
            else
            {
                ((IXmlBinaryWriterInitializer)this.writer).SetOutput(stream, XD.Dictionary, null, true);
            }
            return this.writer;
        }

        public void CloseSection()
        {
            if (bufferState != BufferState.Writing)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidStateException());
            this.writer.Close();
            bufferState = BufferState.Created;
            int size = (int)stream.Length - offset;
            sections.Add(new Section(offset, size, this.quotas));
            offset += size;
        }

        public void Close()
        {
            if (bufferState != BufferState.Created)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidStateException());
            bufferState = BufferState.Reading;
            int bufferSize;
            buffer = stream.ToArray(out bufferSize);
            writer = null;
            stream = null;
        }

        Exception CreateInvalidStateException()
        {
            return new InvalidOperationException(SR.GetString(SR.XmlBufferInInvalidState));
        }

        public XmlDictionaryReader GetReader(int sectionIndex)
        {
            if (bufferState != BufferState.Reading)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidStateException());
            Section section = sections[sectionIndex];
            XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(buffer, section.Offset, section.Size, XD.Dictionary, section.Quotas, null, null);
            reader.MoveToContent();
            return reader;
        }

        public void WriteTo(int sectionIndex, XmlWriter writer)
        {
            if (bufferState != BufferState.Reading)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidStateException());
            XmlDictionaryReader reader = GetReader(sectionIndex);
            try
            {
                writer.WriteNode(reader, false);
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
