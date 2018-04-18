//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;
    using System.Runtime;

    public abstract class XmlDictionaryWriter : XmlWriter
    {
        // Derived classes that implements WriteBase64Async should override 
        // this flag to true so that the base XmlDictionaryWriter would use the 
        // faster WriteBase64Async APIs instead of the default BeginWrite() implementation.
        internal virtual bool FastAsync
        {
            get { return false; }
        }

        internal virtual AsyncCompletionResult WriteBase64Async(AsyncEventArgs<XmlWriteBase64AsyncArguments> state)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override Task WriteBase64Async(byte[] buffer, int index, int count)
        {
            return Task.Factory.FromAsync(this.BeginWriteBase64, this.EndWriteBase64, buffer, index, count, null);
        }

        internal virtual IAsyncResult BeginWriteBase64(byte[] buffer, int index, int count, AsyncCallback callback, object state)
        {
            return new WriteBase64AsyncResult(buffer, index, count, this, callback, state);
        }

        internal virtual void EndWriteBase64(IAsyncResult result)
        {
            WriteBase64AsyncResult.End(result);
        }

        static public XmlDictionaryWriter CreateBinaryWriter(Stream stream)
        {
            return CreateBinaryWriter(stream, null);
        }

        static public XmlDictionaryWriter CreateBinaryWriter(Stream stream, IXmlDictionary dictionary)
        {
            return CreateBinaryWriter(stream, dictionary, null);
        }

        static public XmlDictionaryWriter CreateBinaryWriter(Stream stream, IXmlDictionary dictionary, XmlBinaryWriterSession session)
        {
            return CreateBinaryWriter(stream, dictionary, session, true);
        }

        static public XmlDictionaryWriter CreateBinaryWriter(Stream stream, IXmlDictionary dictionary, XmlBinaryWriterSession session, bool ownsStream)
        {
            XmlBinaryWriter writer = new XmlBinaryWriter();
            writer.SetOutput(stream, dictionary, session, ownsStream);
            return writer;
        }

        static public XmlDictionaryWriter CreateTextWriter(Stream stream)
        {
            return CreateTextWriter(stream, Encoding.UTF8, true);
        }

        static public XmlDictionaryWriter CreateTextWriter(Stream stream, Encoding encoding)
        {
            return CreateTextWriter(stream, encoding, true);
        }

        static public XmlDictionaryWriter CreateTextWriter(Stream stream, Encoding encoding, bool ownsStream)
        {
            XmlUTF8TextWriter writer = new XmlUTF8TextWriter();
            writer.SetOutput(stream, encoding, ownsStream);
            return writer;
        }

        static public XmlDictionaryWriter CreateMtomWriter(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo)
        {
            return CreateMtomWriter(stream, encoding, maxSizeInBytes, startInfo, null, null, true, true);
        }

        static public XmlDictionaryWriter CreateMtomWriter(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream)
        {
            XmlMtomWriter writer = new XmlMtomWriter();
            writer.SetOutput(stream, encoding, maxSizeInBytes, startInfo, boundary, startUri, writeMessageHeaders, ownsStream);
            return writer;
        }

        static public XmlDictionaryWriter CreateDictionaryWriter(XmlWriter writer)
        {
            if (writer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");

            XmlDictionaryWriter dictionaryWriter = writer as XmlDictionaryWriter;

            if (dictionaryWriter == null)
            {
                dictionaryWriter = new XmlWrappedWriter(writer);
            }

            return dictionaryWriter;
        }

        public void WriteStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            WriteStartElement((string)null, localName, namespaceUri);
        }

        public virtual void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            WriteStartElement(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
        }

        public void WriteStartAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            WriteStartAttribute((string)null, localName, namespaceUri);
        }

        public virtual void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            WriteStartAttribute(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
        }

        public void WriteAttributeString(XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
        {
            WriteAttributeString((string)null, localName, namespaceUri, value);
        }

        public virtual void WriteXmlnsAttribute(string prefix, string namespaceUri)
        {
            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            if (prefix == null)
            {
                if (LookupPrefix(namespaceUri) != null)
                    return;
#pragma warning suppress 56506 // Microsoft, namespaceUri is already checked
                prefix = namespaceUri.Length == 0 ? string.Empty : string.Concat("d", namespaceUri.Length.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            WriteAttributeString("xmlns", prefix, null, namespaceUri);
        }

        public virtual void WriteXmlnsAttribute(string prefix, XmlDictionaryString namespaceUri)
        {
            WriteXmlnsAttribute(prefix, XmlDictionaryString.GetString(namespaceUri));
        }

        public virtual void WriteXmlAttribute(string localName, string value)
        {
            WriteAttributeString("xml", localName, null, value);
        }

        public virtual void WriteXmlAttribute(XmlDictionaryString localName, XmlDictionaryString value)
        {
            WriteXmlAttribute(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(value));
        }

        public void WriteAttributeString(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
        {
            WriteStartAttribute(prefix, localName, namespaceUri);
            WriteString(value);
            WriteEndAttribute();
        }

        public void WriteElementString(XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
        {
            WriteElementString((string)null, localName, namespaceUri, value);
        }

        public void WriteElementString(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
        {
            WriteStartElement(prefix, localName, namespaceUri);
            WriteString(value);
            WriteEndElement();
        }

        public virtual void WriteString(XmlDictionaryString value)
        {
            WriteString(XmlDictionaryString.GetString(value));
        }

        public virtual void WriteQualifiedName(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            if (namespaceUri == null)
                namespaceUri = XmlDictionaryString.Empty;
#pragma warning suppress 56506 // Microsoft, XmlDictionaryString.Empty is never null
            WriteQualifiedName(localName.Value, namespaceUri.Value);
        }

        public virtual void WriteValue(XmlDictionaryString value)
        {
            WriteValue(XmlDictionaryString.GetString(value));
        }

        public virtual void WriteValue(IStreamProvider value)
        {
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));

            Stream stream = value.GetStream();
            if (stream == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlInvalidStream)));
            int blockSize = 256;
            int bytesRead = 0;
            byte[] block = new byte[blockSize];
            while (true)
            {
                bytesRead = stream.Read(block, 0, blockSize);
                if (bytesRead > 0)
                    WriteBase64(block, 0, bytesRead);
                else
                    break;
                if (blockSize < 65536 && bytesRead == blockSize)
                {
                    blockSize = blockSize * 16;
                    block = new byte[blockSize];
                }
            }
            value.ReleaseStream(stream);
        }

        public virtual Task WriteValueAsync(IStreamProvider value)
        {
            return Task.Factory.FromAsync(this.BeginWriteValue, this.EndWriteValue, value, null);
        }

        internal virtual IAsyncResult BeginWriteValue(IStreamProvider value, AsyncCallback callback, object state)
        {
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));

            if (this.FastAsync)
            {
                return new WriteValueFastAsyncResult(this, value, callback, state);
            }
            else
            {
                return new WriteValueAsyncResult(this, value, callback, state);
            }
        }

        internal virtual void EndWriteValue(IAsyncResult result)
        {
            if (this.FastAsync)
            {
                WriteValueFastAsyncResult.End(result);
            }
            else
            {
                WriteValueAsyncResult.End(result);
            }
        }

        class WriteValueFastAsyncResult : AsyncResult
        {
            bool completed;
            int blockSize;
            byte[] block;
            int bytesRead;
            Stream stream;
            Operation nextOperation;
            IStreamProvider streamProvider;
            XmlDictionaryWriter writer;
            AsyncEventArgs<XmlWriteBase64AsyncArguments> writerAsyncState;
            XmlWriteBase64AsyncArguments writerAsyncArgs;

            static AsyncCallback onReadComplete = Fx.ThunkCallback(OnReadComplete);
            static AsyncEventArgsCallback onWriteComplete;

            public WriteValueFastAsyncResult(XmlDictionaryWriter writer, IStreamProvider value, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.streamProvider = value;
                this.writer = writer;
                this.stream = value.GetStream();
                if (this.stream == null)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlInvalidStream)));
                }
                this.blockSize = 256;
                this.bytesRead = 0;
                this.block = new byte[this.blockSize];
                this.nextOperation = Operation.Read;
                this.ContinueWork(true);
            }

            void CompleteAndReleaseStream(bool completedSynchronously, Exception completionException = null)
            {
                if (completionException == null)
                {
                    // only release stream when no exception (mirrors sync behaviour)
                    this.streamProvider.ReleaseStream(this.stream);
                    this.stream = null;
                }

                this.Complete(completedSynchronously, completionException);
            }

            void ContinueWork(bool completedSynchronously, Exception completionException = null)
            {
                // Individual Reads or writes may complete sync or async. A callback however 
                // will always all ContinueWork() with CompletedSynchronously=false this flag 
                // is used to complete this AsyncResult.
                try
                {
                    while (true)
                    {
                        if (this.nextOperation == Operation.Read)
                        {
                            if (ReadAsync() != AsyncCompletionResult.Completed)
                            {
                                return;
                            }
                        }
                        else if (this.nextOperation == Operation.Write)
                        {
                            if (WriteAsync() != AsyncCompletionResult.Completed)
                            {
                                return;
                            }
                        }
                        else if (this.nextOperation == Operation.Complete)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    if (completedSynchronously)
                    {
                        throw;
                    }

                    if (completionException == null)
                    {
                        completionException = ex;
                    }
                }

                if (!completed)
                {
                    this.completed = true;
                    this.CompleteAndReleaseStream(completedSynchronously, completionException);
                }
            }

            AsyncCompletionResult ReadAsync()
            {
                IAsyncResult result = this.stream.BeginRead(this.block, 0, blockSize, onReadComplete, this);
                if (result.CompletedSynchronously)
                {
                    this.HandleReadComplete(result);
                    return AsyncCompletionResult.Completed;
                }

                return AsyncCompletionResult.Queued;
            }

            void HandleReadComplete(IAsyncResult result)
            {
                this.bytesRead = this.stream.EndRead(result);
                if (this.bytesRead > 0)
                {
                    this.nextOperation = Operation.Write;
                }
                else
                {
                    this.nextOperation = Operation.Complete;
                }
            }

            static void OnReadComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                WriteValueFastAsyncResult thisPtr = (WriteValueFastAsyncResult)result.AsyncState;
                bool sucess = false;
                try
                {
                    thisPtr.HandleReadComplete(result);
                    sucess = true;
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    completionException = ex;
                }

                if (!sucess)
                {
                    thisPtr.nextOperation = Operation.Complete;
                }

                thisPtr.ContinueWork(false, completionException);
            }

            AsyncCompletionResult WriteAsync()
            {
                if (this.writerAsyncState == null)
                {
                    this.writerAsyncArgs = new XmlWriteBase64AsyncArguments();
                    this.writerAsyncState = new AsyncEventArgs<XmlWriteBase64AsyncArguments>();
                }

                if (onWriteComplete == null)
                {
                    onWriteComplete = new AsyncEventArgsCallback(OnWriteComplete);
                }

                this.writerAsyncArgs.Buffer = this.block;
                this.writerAsyncArgs.Offset = 0;
                this.writerAsyncArgs.Count = this.bytesRead;

                this.writerAsyncState.Set(onWriteComplete, this.writerAsyncArgs, this);
                if (this.writer.WriteBase64Async(this.writerAsyncState) == AsyncCompletionResult.Completed)
                {
                    this.HandleWriteComplete();
                    this.writerAsyncState.Complete(true);
                    return AsyncCompletionResult.Completed;
                }

                return AsyncCompletionResult.Queued;
            }

            void HandleWriteComplete()
            {
                this.nextOperation = Operation.Read;
                // Adjust block size for next read
                if (this.blockSize < 65536 && this.bytesRead == this.blockSize)
                {
                    this.blockSize = this.blockSize * 16;
                    this.block = new byte[this.blockSize];
                }
            }

            static void OnWriteComplete(IAsyncEventArgs asyncState)
            {
                WriteValueFastAsyncResult thisPtr = (WriteValueFastAsyncResult)asyncState.AsyncState;
                Exception completionException = null;
                bool success = false;
                try
                {
                    if (asyncState.Exception != null)
                    {
                        completionException = asyncState.Exception;
                    }
                    else
                    {
                        thisPtr.HandleWriteComplete();
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    completionException = ex;
                }

                if (!success)
                {
                    thisPtr.nextOperation = Operation.Complete;
                }

                thisPtr.ContinueWork(false, completionException);
            }

            internal static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteValueFastAsyncResult>(result);
            }

            enum Operation
            {
                Read,
                Write,
                Complete
            }
        }

        class WriteValueAsyncResult : AsyncResult
        {
            enum Operation
            {
                Read,
                Write
            }

            int blockSize;
            byte[] block;
            int bytesRead;
            Stream stream;
            Operation operation = Operation.Read;
            IStreamProvider streamProvider;
            XmlDictionaryWriter writer;
            Func<IAsyncResult, WriteValueAsyncResult, bool> writeBlockHandler;

            static Func<IAsyncResult, WriteValueAsyncResult, bool> handleWriteBlock = HandleWriteBlock;
            static Func<IAsyncResult, WriteValueAsyncResult, bool> handleWriteBlockAsync = HandleWriteBlockAsync;
            static AsyncCallback onContinueWork = Fx.ThunkCallback(OnContinueWork);

            public WriteValueAsyncResult(XmlDictionaryWriter writer, IStreamProvider value, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.streamProvider = value;
                this.writer = writer;
                this.writeBlockHandler = this.writer.Settings != null && this.writer.Settings.Async ? handleWriteBlockAsync : handleWriteBlock;
                this.stream = value.GetStream();
                if (this.stream == null)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlInvalidStream)));
                }
                this.blockSize = 256;
                this.bytesRead = 0;
                this.block = new byte[blockSize];     // 

                bool completeSelf = ContinueWork(null);

                if (completeSelf)
                {
                    this.CompleteAndReleaseStream(true, null);
                }
            }

            void AdjustBlockSize()
            {
                if (this.blockSize < 65536 && this.bytesRead == this.blockSize)
                {
                    this.blockSize = this.blockSize * 16;
                    this.block = new byte[this.blockSize];
                }
            }

            void CompleteAndReleaseStream(bool completedSynchronously, Exception completionException)
            {
                if (completionException == null)
                {
                    // only release stream when no exception (mirrors sync behaviour)
                    this.streamProvider.ReleaseStream(this.stream);
                    this.stream = null;
                }

                this.Complete(completedSynchronously, completionException);
            }

            //returns whether or not the entire operation is completed
            bool ContinueWork(IAsyncResult result)
            {
                while (true)
                {
                    if (this.operation == Operation.Read)
                    {
                        if (HandleReadBlock(result))
                        {
                            // Read completed (sync or async, doesn't matter) 
                            if (this.bytesRead > 0)
                            {
                                // allow loop to continue at Write
                                operation = Operation.Write;
                            }
                            else
                            {
                                // Exit loop, entire source stream has been read.  
                                return true;
                            }
                        }
                        else
                        {
                            // Read completed async, jump out of loop, callback resumes at writing.
                            return false;
                        }
                    }
                    else
                    {
                        if (this.writeBlockHandler(result, this))
                        {
                            // Write completed (sync or async, doesn't matter) 
                            AdjustBlockSize();
                            operation = Operation.Read;
                        }
                        else
                        {
                            // Write completed async, jump out of loop, callback should resume at reading
                            return false;
                        }
                    }
                    result = null;
                }
            }

            //returns whether or not I completed.
            bool HandleReadBlock(IAsyncResult result)
            {
                if (result == null)
                {
                    result = this.stream.BeginRead(this.block, 0, blockSize, onContinueWork, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                this.bytesRead = this.stream.EndRead(result);
                return true;
            }

            //returns whether or not I completed.
            static bool HandleWriteBlock(IAsyncResult result, WriteValueAsyncResult thisPtr)
            {
                if (result == null)
                {
                    result = thisPtr.writer.BeginWriteBase64(thisPtr.block, 0, thisPtr.bytesRead, onContinueWork, thisPtr);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                thisPtr.writer.EndWriteBase64(result);
                return true;
            }

            //returns whether or not I completed.
            static bool HandleWriteBlockAsync(IAsyncResult result, WriteValueAsyncResult thisPtr)
            {
                Task task = (Task)result;

                if (task == null)
                {
                    task = thisPtr.writer.WriteBase64Async(thisPtr.block, 0, thisPtr.bytesRead);
                    task.AsAsyncResult(onContinueWork, thisPtr);
                    return false;
                }

                task.GetAwaiter().GetResult();

                return true;
            }

            static void OnContinueWork(IAsyncResult result)
            {
                // If result is a Task we shouldn't check for Synchronous completion 
                // We should only return if we're in the async completion path and if the result completed synchronously.
                if (result.CompletedSynchronously && !(result is Task))
                {
                    return;
                }

                Exception completionException = null;
                WriteValueAsyncResult thisPtr = (WriteValueAsyncResult)result.AsyncState;
                bool completeSelf = false;

                try
                {
                    completeSelf = thisPtr.ContinueWork(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    completeSelf = true;
                    completionException = ex;
                }

                if (completeSelf)
                {
                    thisPtr.CompleteAndReleaseStream(false, completionException);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteValueAsyncResult>(result);
            }
        }

        public virtual void WriteValue(UniqueId value)
        {
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            WriteString(value.ToString());
        }

        public virtual void WriteValue(Guid value)
        {
            WriteString(value.ToString());
        }

        public virtual void WriteValue(TimeSpan value)
        {
            WriteString(XmlConvert.ToString(value));
        }

        public virtual bool CanCanonicalize
        {
            get
            {
                return false;
            }
        }

        public virtual void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual void EndCanonicalization()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        void WriteElementNode(XmlDictionaryReader reader, bool defattr)
        {
            XmlDictionaryString localName;
            XmlDictionaryString namespaceUri;
            if (reader.TryGetLocalNameAsDictionaryString(out localName) && reader.TryGetNamespaceUriAsDictionaryString(out namespaceUri))
            {
                WriteStartElement(reader.Prefix, localName, namespaceUri);
            }
            else
            {
                WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
            }
            if (defattr || (!reader.IsDefault && (reader.SchemaInfo == null || !reader.SchemaInfo.IsDefault)))
            {
                if (reader.MoveToFirstAttribute())
                {
                    do
                    {
                        if (reader.TryGetLocalNameAsDictionaryString(out localName) && reader.TryGetNamespaceUriAsDictionaryString(out namespaceUri))
                        {
                            WriteStartAttribute(reader.Prefix, localName, namespaceUri);
                        }
                        else
                        {
                            WriteStartAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        }
                        while (reader.ReadAttributeValue())
                        {
                            if (reader.NodeType == XmlNodeType.EntityReference)
                            {
                                WriteEntityRef(reader.Name);
                            }
                            else
                            {
                                WriteTextNode(reader, true);
                            }
                        }
                        WriteEndAttribute();
                    }
                    while (reader.MoveToNextAttribute());
                    reader.MoveToElement();
                }
            }
            if (reader.IsEmptyElement)
            {
                WriteEndElement();
            }
        }

        void WriteArrayNode(XmlDictionaryReader reader, string prefix, string localName, string namespaceUri, Type type)
        {
            if (type == typeof(bool))
                BooleanArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(Int16))
                Int16ArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(Int32))
                Int32ArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(Int64))
                Int64ArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(float))
                SingleArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(double))
                DoubleArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(decimal))
                DecimalArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(DateTime))
                DateTimeArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(Guid))
                GuidArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(TimeSpan))
                TimeSpanArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else
            {
                WriteElementNode(reader, false);
                reader.Read();
            }
        }

        void WriteArrayNode(XmlDictionaryReader reader, string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Type type)
        {
            if (type == typeof(bool))
                BooleanArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(Int16))
                Int16ArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(Int32))
                Int32ArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(Int64))
                Int64ArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(float))
                SingleArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(double))
                DoubleArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(decimal))
                DecimalArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(DateTime))
                DateTimeArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(Guid))
                GuidArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else if (type == typeof(TimeSpan))
                TimeSpanArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            else
            {
                WriteElementNode(reader, false);
                reader.Read();
            }
        }

        void WriteArrayNode(XmlDictionaryReader reader, Type type)
        {
            XmlDictionaryString localName;
            XmlDictionaryString namespaceUri;
            if (reader.TryGetLocalNameAsDictionaryString(out localName) && reader.TryGetNamespaceUriAsDictionaryString(out namespaceUri))
                WriteArrayNode(reader, reader.Prefix, localName, namespaceUri, type);
            else
                WriteArrayNode(reader, reader.Prefix, reader.LocalName, reader.NamespaceURI, type);
        }

        protected virtual void WriteTextNode(XmlDictionaryReader reader, bool isAttribute)
        {
            XmlDictionaryString value;
            if (reader.TryGetValueAsDictionaryString(out value))
            {
                WriteString(value);
            }
            else
            {
                WriteString(reader.Value);
            }
            if (!isAttribute)
            {
                reader.Read();
            }
        }

        public override void WriteNode(XmlReader reader, bool defattr)
        {
            XmlDictionaryReader dictionaryReader = reader as XmlDictionaryReader;
            if (dictionaryReader != null)
                WriteNode(dictionaryReader, defattr);
            else
                base.WriteNode(reader, defattr);
        }

        public virtual void WriteNode(XmlDictionaryReader reader, bool defattr)
        {
            if (reader == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            int d = (reader.NodeType == XmlNodeType.None ? -1 : reader.Depth);
            do
            {
                XmlNodeType nodeType = reader.NodeType;
                Type type;
                if (nodeType == XmlNodeType.Text || nodeType == XmlNodeType.Whitespace || nodeType == XmlNodeType.SignificantWhitespace)
                {
                    // This will advance if necessary, so we don't need to call Read() explicitly
                    WriteTextNode(reader, false);
                }
                else if (reader.Depth > d && reader.IsStartArray(out type))
                {
                    WriteArrayNode(reader, type);
                }
                else
                {
                    // These will not advance, so we must call Read() explicitly
                    switch (nodeType)
                    {
                        case XmlNodeType.Element:
                            WriteElementNode(reader, defattr);
                            break;
                        case XmlNodeType.CDATA:
                            WriteCData(reader.Value);
                            break;
                        case XmlNodeType.EntityReference:
                            WriteEntityRef(reader.Name);
                            break;
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:
                            WriteProcessingInstruction(reader.Name, reader.Value);
                            break;
                        case XmlNodeType.DocumentType:
                            WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
                            break;
                        case XmlNodeType.Comment:
                            WriteComment(reader.Value);
                            break;
                        case XmlNodeType.EndElement:
                            WriteFullEndElement();
                            break;
                    }
                    if (!reader.Read())
                        break;
                }
            }
            while (d < reader.Depth || (d == reader.Depth && reader.NodeType == XmlNodeType.EndElement));
        }

        void CheckArray(Array array, int offset, int count)
        {
            if (array == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("array"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > array.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, array.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > array.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, array.Length - offset)));
        }

        // bool
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Int16
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, Int16[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Int16[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Int32
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, Int32[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Int32[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Int64
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, Int64[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Int64[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // float
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, float[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // double
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, double[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // decimal
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // DateTime
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Guid
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // TimeSpan
        public virtual void WriteArray(string prefix, string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                WriteStartElement(prefix, localName, namespaceUri);
                WriteValue(array[offset + i]);
                WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
        {
            WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        class WriteBase64AsyncResult : ScheduleActionItemAsyncResult
        {
            byte[] buffer;
            int index;
            int count;
            XmlDictionaryWriter writer;

            public WriteBase64AsyncResult(byte[] buffer, int index, int count, XmlDictionaryWriter writer, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(writer != null, "writer should never be null");

                this.buffer = buffer;
                this.index = index;
                this.count = count;
                this.writer = writer;

                Schedule();
            }

            protected override void OnDoWork()
            {
                this.writer.WriteBase64(this.buffer, this.index, this.count);
            }
        }

        class XmlWrappedWriter : XmlDictionaryWriter
        {
            XmlWriter writer;
            int depth;
            int prefix;

            public XmlWrappedWriter(XmlWriter writer)
            {
                this.writer = writer;
                this.depth = 0;
            }

            public override void Close()
            {
                writer.Close();
            }

            public override void Flush()
            {
                writer.Flush();
            }

            public override string LookupPrefix(string namespaceUri)
            {
                return writer.LookupPrefix(namespaceUri);
            }

            public override void WriteAttributes(XmlReader reader, bool defattr)
            {
                writer.WriteAttributes(reader, defattr);
            }

            public override void WriteBase64(byte[] buffer, int index, int count)
            {
                writer.WriteBase64(buffer, index, count);
            }

            public override void WriteBinHex(byte[] buffer, int index, int count)
            {
                writer.WriteBinHex(buffer, index, count);
            }

            public override void WriteCData(string text)
            {
                writer.WriteCData(text);
            }

            public override void WriteCharEntity(char ch)
            {
                writer.WriteCharEntity(ch);
            }

            public override void WriteChars(char[] buffer, int index, int count)
            {
                writer.WriteChars(buffer, index, count);
            }

            public override void WriteComment(string text)
            {
                writer.WriteComment(text);
            }

            public override void WriteDocType(string name, string pubid, string sysid, string subset)
            {
                writer.WriteDocType(name, pubid, sysid, subset);
            }

            public override void WriteEndAttribute()
            {
                writer.WriteEndAttribute();
            }

            public override void WriteEndDocument()
            {
                writer.WriteEndDocument();
            }

            public override void WriteEndElement()
            {
                writer.WriteEndElement();
                this.depth--;
            }

            public override void WriteEntityRef(string name)
            {
                writer.WriteEntityRef(name);
            }

            public override void WriteFullEndElement()
            {
                writer.WriteFullEndElement();
            }

            public override void WriteName(string name)
            {
                writer.WriteName(name);
            }

            public override void WriteNmToken(string name)
            {
                writer.WriteNmToken(name);
            }

            public override void WriteNode(XmlReader reader, bool defattr)
            {
                writer.WriteNode(reader, defattr);
            }

            public override void WriteProcessingInstruction(string name, string text)
            {
                writer.WriteProcessingInstruction(name, text);
            }

            public override void WriteQualifiedName(string localName, string namespaceUri)
            {
                writer.WriteQualifiedName(localName, namespaceUri);
            }

            public override void WriteRaw(char[] buffer, int index, int count)
            {
                writer.WriteRaw(buffer, index, count);
            }

            public override void WriteRaw(string data)
            {
                writer.WriteRaw(data);
            }

            public override void WriteStartAttribute(string prefix, string localName, string namespaceUri)
            {
                writer.WriteStartAttribute(prefix, localName, namespaceUri);
                this.prefix++;
            }

            public override void WriteStartDocument()
            {
                writer.WriteStartDocument();
            }

            public override void WriteStartDocument(bool standalone)
            {
                writer.WriteStartDocument(standalone);
            }

            public override void WriteStartElement(string prefix, string localName, string namespaceUri)
            {
                writer.WriteStartElement(prefix, localName, namespaceUri);
                this.depth++;
                this.prefix = 1;
            }

            public override WriteState WriteState
            {
                get
                {
                    return writer.WriteState;
                }
            }

            public override void WriteString(string text)
            {
                writer.WriteString(text);
            }

            public override void WriteSurrogateCharEntity(char lowChar, char highChar)
            {
                writer.WriteSurrogateCharEntity(lowChar, highChar);
            }

            public override void WriteWhitespace(string whitespace)
            {
                writer.WriteWhitespace(whitespace);
            }

            public override void WriteValue(object value)
            {
                writer.WriteValue(value);
            }

            public override void WriteValue(string value)
            {
                writer.WriteValue(value);
            }

            public override void WriteValue(bool value)
            {
                writer.WriteValue(value);
            }

            public override void WriteValue(DateTime value)
            {
                writer.WriteValue(value);
            }

            public override void WriteValue(double value)
            {
                writer.WriteValue(value);
            }

            public override void WriteValue(int value)
            {
                writer.WriteValue(value);
            }

            public override void WriteValue(long value)
            {
                writer.WriteValue(value);
            }

#if DECIMAL_FLOAT_API
            public override void WriteValue(decimal value)
            {
                writer.WriteValue(value);
            }

            public override void WriteValue(float value)
            {
                writer.WriteValue(value);
            }
#endif
            public override void WriteXmlnsAttribute(string prefix, string namespaceUri)
            {
                if (namespaceUri == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
                if (prefix == null)
                {
                    if (LookupPrefix(namespaceUri) != null)
                        return;

                    if (namespaceUri.Length == 0)
                    {
                        prefix = string.Empty;
                    }
                    else
                    {
                        string depthStr = this.depth.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                        string prefixStr = this.prefix.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                        prefix = string.Concat("d", depthStr, "p", prefixStr);
                    }
                }
                WriteAttributeString("xmlns", prefix, null, namespaceUri);
            }

            public override string XmlLang
            {
                get
                {
                    return writer.XmlLang;
                }
            }

            public override XmlSpace XmlSpace
            {
                get
                {
                    return writer.XmlSpace;
                }
            }
        }
    }
}
