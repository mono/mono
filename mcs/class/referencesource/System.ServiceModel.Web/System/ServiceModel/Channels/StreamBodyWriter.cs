//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;
    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;

    public abstract class StreamBodyWriter : BodyWriter
    {
        // if isQuirkedTo40Behavior = true, does not try to write out <Binary> tags 
        //   this maintains compatibility for 4.0 implementers of a derived StreamBodyWriter if they 
        //   depended on behaviour where StreamBodyWriter isn't ByteStreamEncoder aware. 
        //   e.g., if they wrote their own <Binary> tags and relied on StreamBodyWriter to only write out the body. 
        //
        // if isQuirkedTo40Behavior = false, XmlWriterBackedStream will write out <Binary> tags (default in 4.5)
        readonly bool isQuirkedTo40Behavior;

        // externally accessible constructor quirked: 
        // if version <  4.5, XmlWriterBackedStream does not try to write out <Binary> tags
        // if version >= 4.5, XmlWriterBackedStream will write out <Binary> tags
        protected StreamBodyWriter(bool isBuffered)
            : this(isBuffered, !OSEnvironmentHelper.IsApplicationTargeting45)
        { }

        // internally accessible constructor allows derived types to determine whether StreamBodyWriter should be ByteStream aware
        // internal implementations SHOULD use isQuirkedTo40Behavior = false. 
        internal StreamBodyWriter(bool isBuffered, bool isQuirkedTo40Behavior)
            : base(isBuffered)
        {
            this.isQuirkedTo40Behavior = isQuirkedTo40Behavior;  
        }

        internal static StreamBodyWriter CreateStreamBodyWriter(Action<Stream> streamAction)
        {
            if (streamAction == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("actionOfStream");
            }
            return new ActionOfStreamBodyWriter(streamAction);
        }

        protected abstract void OnWriteBodyContents(Stream stream);

        protected override BodyWriter OnCreateBufferedCopy(int maxBufferSize)
        {
            using (BufferManagerOutputStream bufferedStream = new BufferManagerOutputStream(SR2.MaxReceivedMessageSizeExceeded, maxBufferSize))
            {
                this.OnWriteBodyContents(bufferedStream);
                int size;
                byte[] bytesArray = bufferedStream.ToArray(out size);
                return new BufferedBytesStreamBodyWriter(bytesArray, size);
            }
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            using (XmlWriterBackedStream stream = new XmlWriterBackedStream(writer, this.isQuirkedTo40Behavior))
            {
                OnWriteBodyContents(stream);
            }
        }

        class XmlWriterBackedStream : Stream
        {
            private const string StreamElementName = "Binary";
            private readonly bool isQuirkedTo40Behavior; 

            XmlWriter writer;

            public XmlWriterBackedStream(XmlWriter writer, bool isQuirkedTo40Behavior)
            {
                if (writer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
                }
                this.writer = writer;

                this.isQuirkedTo40Behavior = isQuirkedTo40Behavior;
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
                this.writer.Flush();
            }

            public override long Length
            {
                get
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.XmlWriterBackedStreamPropertyGetNotSupported, "Length")));
                }
            }

            public override long Position
            {
                get
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.XmlWriterBackedStreamPropertyGetNotSupported, "Position")));
                }
                set
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.XmlWriterBackedStreamPropertySetNotSupported, "Position")));
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.XmlWriterBackedStreamMethodNotSupported, "Read")));
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.XmlWriterBackedStreamMethodNotSupported, "BeginRead")));
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.XmlWriterBackedStreamMethodNotSupported, "EndRead")));
            }

            public override int ReadByte()
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.XmlWriterBackedStreamMethodNotSupported, "ReadByte")));
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.XmlWriterBackedStreamMethodNotSupported, "Seek")));
            }

            public override void SetLength(long value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.XmlWriterBackedStreamMethodNotSupported, "SetLength")));
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (writer.WriteState == WriteState.Content || this.isQuirkedTo40Behavior)
                {
                    // if isQuirkedTo40Behavior == true, maintains compatibility for 4.0 implementers of a derived StreamBodyWriter 
                    // if they depended on behaviour without state checks, e.g., if they wrote their own <Binary> tags 
                    this.writer.WriteBase64(buffer, offset, count);
                }
                else if (writer.WriteState == WriteState.Start)
                {
                    writer.WriteStartElement(StreamElementName, string.Empty);
                    this.writer.WriteBase64(buffer, offset, count);
                }
            }
        }

        class BufferedBytesStreamBodyWriter : StreamBodyWriter
        {
            byte[] array;
            int size;

            public BufferedBytesStreamBodyWriter(byte[] array, int size)
                : base(true, false)
            {
                this.array = array;
                this.size = size;
            }

            protected override void OnWriteBodyContents(Stream stream)
            {
                stream.Write(this.array, 0, this.size);
            }
        }

        class ActionOfStreamBodyWriter : StreamBodyWriter
        {
            Action<Stream> actionOfStream;

            public ActionOfStreamBodyWriter(Action<Stream> actionOfStream)
                : base(false, false)
            {
                this.actionOfStream = actionOfStream;
            }

            protected override void OnWriteBodyContents(Stream stream)
            {
                actionOfStream(stream);
            }
        }
    }
}
