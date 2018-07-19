///----------- ----------- ----------- ----------- ----------- ----------- -----------
/// <copyright file="DeflateStream.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>                               
///
///----------- ----------- ----------- ----------- ----------- ----------- -----------
///

using System.Diagnostics;
using System.Threading;
using System.Security.Permissions;
using System.Diagnostics.Contracts;

namespace System.IO.Compression {
   
    public class DeflateStream : Stream {

        internal const int DefaultBufferSize = 8192;

        internal delegate void AsyncWriteDelegate(byte[] array, int offset, int count, bool isAsync);

        //private const String OrigStackTrace_ExceptionDataKey = "ORIGINAL_STACK_TRACE";

        private Stream _stream;
        private CompressionMode _mode;
        private bool _leaveOpen;
        private Inflater inflater;
        private IDeflater deflater;        
        private byte[] buffer;
        
        private int asyncOperations; 
        private readonly AsyncCallback m_CallBack;
        private readonly AsyncWriteDelegate m_AsyncWriterDelegate;

        private IFileFormatWriter formatWriter;
        private bool wroteHeader;
        private bool wroteBytes;

        private enum WorkerType : byte { Managed, ZLib, Unknown };
        private static volatile WorkerType deflaterType = WorkerType.Unknown;


        public DeflateStream(Stream stream, CompressionMode mode)
            : this(stream, mode, false) {
        }              
        
        public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen) {
           
            if(stream == null )
                throw new ArgumentNullException("stream");

            if (CompressionMode.Compress != mode && CompressionMode.Decompress != mode)
                throw new ArgumentException(SR.GetString(SR.ArgumentOutOfRange_Enum), "mode");

            _stream = stream;
            _mode = mode;
            _leaveOpen = leaveOpen;

            switch (_mode) {

                case CompressionMode.Decompress:

                    if (!_stream.CanRead) {
                        throw new ArgumentException(SR.GetString(SR.NotReadableStream), "stream");
                    }

                    inflater = new Inflater();

                    m_CallBack = new AsyncCallback(ReadCallback); 
                    break;

                case CompressionMode.Compress:

                    if (!_stream.CanWrite) {
                        throw new ArgumentException(SR.GetString(SR.NotWriteableStream), "stream");
                    }

                    deflater = CreateDeflater(null);
                    
                    m_AsyncWriterDelegate = new AsyncWriteDelegate(this.InternalWrite);
                    m_CallBack = new AsyncCallback(WriteCallback); 

                    break;                        

            }  // switch (_mode)

            buffer = new byte[DefaultBufferSize];
        }

        // Implies mode = Compress
        public DeflateStream(Stream stream, CompressionLevel compressionLevel)

            : this(stream, compressionLevel, false) {
        }

        // Implies mode = Compress
        public DeflateStream(Stream stream, CompressionLevel compressionLevel, bool leaveOpen) {

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanWrite)
                throw new ArgumentException(SR.GetString(SR.NotWriteableStream), "stream");

            // Checking of compressionLevel is passed down to the IDeflater implementation as it
            // is a pluggable component that completely encapsulates the meaning of compressionLevel.
            
            Contract.EndContractBlock();

            _stream = stream;
            _mode = CompressionMode.Compress;
            _leaveOpen = leaveOpen;

            deflater = CreateDeflater(compressionLevel);

            m_AsyncWriterDelegate = new AsyncWriteDelegate(this.InternalWrite);
            m_CallBack = new AsyncCallback(WriteCallback);
           
            buffer = new byte[DefaultBufferSize];
        }        

        private static IDeflater CreateDeflater(CompressionLevel? compressionLevel) {

            switch (GetDeflaterType()) {

                case WorkerType.Managed:
                    return new DeflaterManaged();

                case WorkerType.ZLib:
                    if (compressionLevel.HasValue)
                        return new DeflaterZLib(compressionLevel.Value);
                    else
                        return new DeflaterZLib();

                default:
                    // We do not expect this to ever be thrown.
                    // But this is better practice than returning null.
                    throw new SystemException("Program entered an unexpected state.");
            }
        }

        #if !SILVERLIGHT
        [System.Security.SecuritySafeCritical]
        #endif
        private static WorkerType GetDeflaterType() {

            // Let's not worry about race conditions:
            // Yes, we risk initialising the singleton multiple times.
            // However, initialising the singleton multiple times has no bad consequences, and is fairly cheap.

            if (WorkerType.Unknown != deflaterType)
                return deflaterType;
                        
            #if !SILVERLIGHT  // Do this on Desktop.  CLRConfig doesn't exist on CoreSys nor Silverlight.

            // CLRConfig is internal in mscorlib and is a friend
            if (System.CLRConfig.CheckLegacyManagedDeflateStream())
                return (deflaterType = WorkerType.Managed);

            #endif

            #if !SILVERLIGHT || FEATURE_NETCORE  // Only skip this for Silverlight, which doesn't ship ZLib.

            if (!CompatibilitySwitches.IsNetFx45LegacyManagedDeflateStream)
                return (deflaterType = WorkerType.ZLib);

            #endif

            return (deflaterType = WorkerType.Managed);
        }

        internal void SetFileFormatReader(IFileFormatReader reader) {
            if (reader != null) {
                inflater.SetFileFormatReader(reader);
            }
        }

        internal void SetFileFormatWriter(IFileFormatWriter writer) {
            if (writer != null) {
                formatWriter = writer;
            }
        }

        public Stream BaseStream {
            get {
                return _stream;
            }
        }

        public override bool CanRead { 
            get {
                if( _stream == null) {
                    return false;      
                }

                return (_mode == CompressionMode.Decompress && _stream.CanRead);
            }
        }

        public override bool CanWrite { 
            get {
                if( _stream == null) {
                    return false;                
                }
                
                return (_mode == CompressionMode.Compress && _stream.CanWrite);
            }
        }

        public override bool CanSeek { 
            get {                
                return false;
            }
        }

        public override long Length { 
            get {
                throw new NotSupportedException(SR.GetString(SR.NotSupported));            
            }
        }

        public override long Position { 
            get {
                throw new NotSupportedException(SR.GetString(SR.NotSupported));            
            } 
            
            set {
                throw new NotSupportedException(SR.GetString(SR.NotSupported));            
            }
        }
        
        public override void Flush() {            
            EnsureNotDisposed();
            return;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException(SR.GetString(SR.NotSupported));            
        }

        public override void SetLength(long value) {
            throw new NotSupportedException(SR.GetString(SR.NotSupported));            
        }

        public override int Read(byte[] array, int offset, int count) {

            EnsureDecompressionMode();
            ValidateParameters(array, offset, count);
            EnsureNotDisposed();
            
            int bytesRead;
            int currentOffset = offset;
            int remainingCount = count;

            while(true) {
                bytesRead = inflater.Inflate(array, currentOffset, remainingCount); 
                currentOffset += bytesRead;
                remainingCount -= bytesRead;

                if( remainingCount == 0) {
                    break;
                }
               
                if (inflater.Finished() ) { 
                    // if we finished decompressing, we can't have anything left in the outputwindow.
                    Debug.Assert(inflater.AvailableOutput == 0, "We should have copied all stuff out!");
                    break;                    
                }                

                Debug.Assert(inflater.NeedsInput(), "We can only run into this case if we are short of input");

                int bytes = _stream.Read(buffer, 0, buffer.Length);
                if( bytes == 0) {
                    break;      //Do we want to throw an exception here?
                }

                inflater.SetInput(buffer, 0 , bytes);
            }

            return count - remainingCount;
        }        

        private void ValidateParameters(byte[] array, int offset, int count) {

            if (array==null)
                throw new ArgumentNullException("array");           
            
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");          

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");           

            if (array.Length - offset < count)
                throw new ArgumentException(SR.GetString(SR.InvalidArgumentOffsetCount));           
        }

        private void EnsureNotDisposed() {

            if (_stream == null)
                throw new ObjectDisposedException(null, SR.GetString(SR.ObjectDisposed_StreamClosed));           
        }

        private void EnsureDecompressionMode() {

            if( _mode != CompressionMode.Decompress)
                throw new InvalidOperationException(SR.GetString(SR.CannotReadFromDeflateStream));           
        }

        private void EnsureCompressionMode() {

            if( _mode != CompressionMode.Compress)
                throw new InvalidOperationException(SR.GetString(SR.CannotWriteToDeflateStream));           
        }

#if !FEATURE_NETCORE
        [HostProtection(ExternalThreading=true)]
#endif
        public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState) {
            
            EnsureDecompressionMode();

            // We use this checking order for compat to earlier versions:
            if (asyncOperations != 0)
                throw new InvalidOperationException(SR.GetString(SR.InvalidBeginCall));

            ValidateParameters(array, offset, count);
            EnsureNotDisposed();                       

            Interlocked.Increment(ref asyncOperations);

            try {                

                DeflateStreamAsyncResult userResult = new DeflateStreamAsyncResult(
                                                            this, asyncState, asyncCallback, array, offset, count);
                userResult.isWrite = false;

                // Try to read decompressed data in output buffer
                int bytesRead = inflater.Inflate(array, offset, count);
                if( bytesRead != 0) {
                    // If decompression output buffer is not empty, return immediately.
                    // 'true' means we complete synchronously.
                    userResult.InvokeCallback(true, (object) bytesRead);                      
                    return userResult;
                }

                if (inflater.Finished() ) {  
                    // end of compression stream
                    userResult.InvokeCallback(true, (object) 0);  
                    return userResult;
                }
                    
                // If there is no data on the output buffer and we are not at 
                // the end of the stream, we need to get more data from the base stream
                _stream.BeginRead(buffer, 0, buffer.Length, m_CallBack, userResult);   
                userResult.m_CompletedSynchronously &= userResult.IsCompleted;            
                
                return userResult;

            } catch {
                Interlocked.Decrement( ref asyncOperations);
                throw;
            }
        }

        // callback function for asynchrous reading on base stream
        private void ReadCallback(IAsyncResult baseStreamResult) {

            DeflateStreamAsyncResult outerResult = (DeflateStreamAsyncResult) baseStreamResult.AsyncState;
            outerResult.m_CompletedSynchronously &= baseStreamResult.CompletedSynchronously;
            int bytesRead = 0;

            try {

                EnsureNotDisposed();

                bytesRead = _stream.EndRead(baseStreamResult);

                if (bytesRead <= 0 ) {
                    // This indicates the base stream has received EOF
                    outerResult.InvokeCallback((object) 0);  
                    return;
                }
    
                // Feed the data from base stream into decompression engine
                inflater.SetInput(buffer, 0 , bytesRead);
                bytesRead = inflater.Inflate(outerResult.buffer, outerResult.offset, outerResult.count); 

                if (bytesRead == 0 && !inflater.Finished()) {  

                    // We could have read in head information and didn't get any data.
                    // Read from the base stream again.   
                    // Need to solve recusion.
                    _stream.BeginRead(buffer, 0, buffer.Length, m_CallBack, outerResult);                   

                } else {
                    outerResult.InvokeCallback((object) bytesRead);              
                }
            } catch (Exception exc) {
                // Defer throwing this until EndRead where we will likely have user code on the stack.
                outerResult.InvokeCallback(exc); 
                return;
            }
        }

        public override int EndRead(IAsyncResult asyncResult) {

            EnsureDecompressionMode();
            CheckEndXxxxLegalStateAndParams(asyncResult);

            // We checked that this will work in CheckEndXxxxLegalStateAndParams:
            DeflateStreamAsyncResult deflateStrmAsyncResult = (DeflateStreamAsyncResult) asyncResult;

            AwaitAsyncResultCompletion(deflateStrmAsyncResult);

            Exception previousException = deflateStrmAsyncResult.Result as Exception;
            if (previousException != null) {
                // Rethrowing will delete the stack trace. Let's help future debuggers:                
                //previousException.Data.Add(OrigStackTrace_ExceptionDataKey, previousException.StackTrace);
                throw previousException;
            }

            return (int) deflateStrmAsyncResult.Result;
        }

        public override void Write(byte[] array, int offset, int count) {
            EnsureCompressionMode();
            ValidateParameters(array, offset, count);
            EnsureNotDisposed();
            InternalWrite(array, offset, count, false);
        }

        // isAsync always seems to be false. why do we have it?
        internal void InternalWrite(byte[] array, int offset, int count, bool isAsync) {

            DoMaintenance(array, offset, count);            

            // Write compressed the bytes we already passed to the deflater:

            WriteDeflaterOutput(isAsync);

            // Pass new bytes through deflater and write them too:

            deflater.SetInput(array, offset, count);
            WriteDeflaterOutput(isAsync);
        }


        private void WriteDeflaterOutput(bool isAsync) {

            while (!deflater.NeedsInput()) {

                int compressedBytes = deflater.GetDeflateOutput(buffer);
                if (compressedBytes > 0)
                    DoWrite(buffer, 0, compressedBytes, isAsync);
            }
        }

        private void DoWrite(byte[] array, int offset, int count, bool isAsync) {
            Debug.Assert(array != null);
            Debug.Assert(count != 0);

            if (isAsync) {
                IAsyncResult result = _stream.BeginWrite(array, offset, count, null, null);
                _stream.EndWrite(result);

            } else {
                _stream.Write(array, offset, count);
            }
        }

        // Perform deflate-mode maintenance required due to custom header and footer writers
        // (e.g. set by GZipStream):
        private void DoMaintenance(byte[] array, int offset, int count) {

            // If no bytes written, do nothing:
            if (count <= 0)
                return;

            // Note that stream contains more than zero data bytes:
            wroteBytes = true;

            // If no header/footer formatter present, nothing else to do:
            if (formatWriter == null)
                return;            

            // If formatter has not yet written a header, do it now:
            if (!wroteHeader) {
                byte[] b = formatWriter.GetHeader();
                _stream.Write(b, 0, b.Length);
                wroteHeader = true;
            }
            
            // Inform formatter of the data bytes written:
            formatWriter.UpdateWithBytesRead(array, offset, count);            
        }

        // This is called by Dispose:
        private void PurgeBuffers(bool disposing) {

            if (!disposing)
                return;

            if (_stream == null)
                return;

            Flush();

            if (_mode != CompressionMode.Compress)
                return;

            // Some deflaters (e.g. ZLib write more than zero bytes for zero bytes inpuits.
            // This round-trips and we should be ok with this, but our legacy managed deflater
            // always wrote zero output for zero input and upstack code (e.g. GZipStream)
            // took dependencies on it. Thus, make sure to only "flush" when we actually had
            // some input:
            if (wroteBytes) {

                // Compress any bytes left:                        
                WriteDeflaterOutput(false);

                // Pull out any bytes left inside deflater:
                bool finished;
                do {
                    int compressedBytes;
                    finished = deflater.Finish(buffer, out compressedBytes);

                    if (compressedBytes > 0)
                        DoWrite(buffer, 0, compressedBytes, false);

                } while (!finished);

            }

            // Write format footer:
            if (formatWriter != null && wroteHeader) {
                byte[] b = formatWriter.GetFooter();
                _stream.Write(b, 0, b.Length);
            }            
        }

        protected override void Dispose(bool disposing) {

            try {

                PurgeBuffers(disposing);

            } finally {

                // Close the underlying stream even if PurgeBuffers threw.
                // Stream.Close() may throw here (may or may not be due to the same error).
                // In this case, we still need to clean up internal resources, hence the inner finally blocks.
                try {                    

                    if(disposing && !_leaveOpen && _stream != null) 
                        _stream.Close();                    

                } finally {

                    _stream = null;

                    try {

                        if (deflater != null)
                            deflater.Dispose();

                    } finally {

                        deflater = null;
                        base.Dispose(disposing);
                    }
                }  // finally
            }  // finally
        }  // Dispose


#if !FEATURE_NETCORE
        [HostProtection(ExternalThreading=true)]
#endif
        public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState) {

            EnsureCompressionMode();            

            // We use this checking order for compat to earlier versions:
            if (asyncOperations != 0 )
                throw new InvalidOperationException(SR.GetString(SR.InvalidBeginCall));

            ValidateParameters(array, offset, count);
            EnsureNotDisposed();
           
            Interlocked.Increment(ref asyncOperations);

            try {                

                DeflateStreamAsyncResult userResult = new DeflateStreamAsyncResult(
                                                            this, asyncState, asyncCallback, array, offset, count);
                userResult.isWrite = true;

                m_AsyncWriterDelegate.BeginInvoke(array, offset, count, true, m_CallBack, userResult);
                userResult.m_CompletedSynchronously &= userResult.IsCompleted;            
                                
                return userResult;

            } catch {
                Interlocked.Decrement(ref asyncOperations);
                throw;
            }
        }

        // Callback function for asynchrous reading on base stream
        private void WriteCallback(IAsyncResult asyncResult) {

            DeflateStreamAsyncResult outerResult = (DeflateStreamAsyncResult) asyncResult.AsyncState;
            outerResult.m_CompletedSynchronously &= asyncResult.CompletedSynchronously;            

            try {

                m_AsyncWriterDelegate.EndInvoke(asyncResult);

            } catch (Exception exc) {
                // Defer throwing this until EndWrite where there is user code on the stack:
                outerResult.InvokeCallback(exc); 
                return;
            }
            outerResult.InvokeCallback(null);              
        }

        public override void EndWrite(IAsyncResult asyncResult) {

            EnsureCompressionMode();
            CheckEndXxxxLegalStateAndParams(asyncResult);

            // We checked that this will work in CheckEndXxxxLegalStateAndParams:
            DeflateStreamAsyncResult deflateStrmAsyncResult = (DeflateStreamAsyncResult) asyncResult;

            AwaitAsyncResultCompletion(deflateStrmAsyncResult);

            Exception previousException = deflateStrmAsyncResult.Result as Exception;
            if (previousException != null) {
                // Rethrowing will delete the stack trace. Let's help future debuggers:                
                //previousException.Data.Add(OrigStackTrace_ExceptionDataKey, previousException.StackTrace);
                throw previousException;
            }
        }

        private void CheckEndXxxxLegalStateAndParams(IAsyncResult asyncResult) {

            if (asyncOperations != 1)
                throw new InvalidOperationException(SR.GetString(SR.InvalidEndCall));

            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            EnsureNotDisposed();
            
            DeflateStreamAsyncResult myResult = asyncResult as DeflateStreamAsyncResult;

            // This should really be an ArgumentException, but we keep this for compat to previous versions:
            if (myResult == null)
                throw new ArgumentNullException("asyncResult");
        }

        private void AwaitAsyncResultCompletion(DeflateStreamAsyncResult asyncResult) {

            try {

                if (!asyncResult.IsCompleted)
                    asyncResult.AsyncWaitHandle.WaitOne();

            } finally {

                Interlocked.Decrement(ref asyncOperations);
                asyncResult.Close();  // this will just close the wait handle
            }
        }

    }  // public class DeflateStream

}  // namespace System.IO.Compression

