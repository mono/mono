/*++
Copyright (c) 2000 Microsoft Corporation

Module Name:

    _StreamFramer.cs

Abstract:


Author:

    Mauro Ottaviani   original implementation
    Alexei Vopilov    20-Jul-2002 made it generic enough
                      (still not perfect, consider IStreamFramer interface)

Revision History:

--*/

namespace System.Net {
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.ComponentModel;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;


    internal class StreamFramer {
        private Stream m_Transport;
        private bool m_Eof;


        private FrameHeader m_WriteHeader    = new FrameHeader();
        private FrameHeader m_CurReadHeader  = new FrameHeader();
        private FrameHeader m_ReadVerifier   = new FrameHeader(FrameHeader.IgnoreValue,
                                                               FrameHeader.IgnoreValue,
                                                               FrameHeader.IgnoreValue);

        //private const int   c_DefaultBufferSize = 1024;
        //private int         m_BufferSize  = c_DefaultBufferSize;
        //private byte[]      m_ReadBuffer  = new byte[FrameHeader.SizeOf + m_BufferSize];
        //private int         m_CurReadOffset;

        private byte[]    m_ReadHeaderBuffer;
        private byte[]    m_WriteHeaderBuffer;

        private readonly AsyncCallback m_ReadFrameCallback;
        private readonly AsyncCallback m_BeginWriteCallback;


        private NetworkStream m_NetworkStream;  //optimizing writes

        public StreamFramer(Stream Transport) {
            if (Transport == null || Transport == Stream.Null) {
                throw new ArgumentNullException("Transport");
            }
            m_Transport = Transport;
            if(m_Transport.GetType() == typeof(NetworkStream)){
                m_NetworkStream = Transport as NetworkStream;
            }
            m_ReadHeaderBuffer = new byte[m_CurReadHeader.Size];
            m_WriteHeaderBuffer = new byte[m_WriteHeader.Size];

            m_ReadFrameCallback = new AsyncCallback(ReadFrameCallback);
            m_BeginWriteCallback = new AsyncCallback(BeginWriteCallback);

        }

        /*
        // Consider removing.
        public FrameHeader m_ReadVerifierHeader {
            get {
                return m_ReadVerifier;
            }
            // May not be called while IO is in progress
            set {
                m_ReadVerifier = value;
                m_CurReadHeader = m_ReadVerifier.Clone();
                m_ReadHeaderBuffer = new byte[m_CurReadHeader.Size];
            }
        }
        */

        public FrameHeader ReadHeader {
            get {
                return m_CurReadHeader;
            }
        }

        public FrameHeader WriteHeader {
            get {
                return m_WriteHeader;
            }
            /*
            // Consider removing.
            // May not be called while IO is in progress
            set {
                m_WriteHeader = value;
                m_WriteHeaderBuffer = new byte[m_WriteHeader.Size];
            }
            */
        }

        public Stream Transport {
            get {
                return m_Transport;
            }
        }

        /*
        // Consider removing.
        public bool EndOfFile {
            get {
                return m_Eof;
            }
        }
        */

        /*
        // Consider removing.
        public bool CanRead {
            get {
                return Transport.CanRead;
            }
        }
        */

        /*
        // Consider removing.
        public bool CanWrite {
            get {
                return Transport.CanWrite;
            }
        }
        */

        public byte[] ReadMessage() {
            if (m_Eof) {
                return null;
            }

            int offset = 0;
            byte[] buffer = m_ReadHeaderBuffer;

            int bytesRead;
            while (offset < buffer.Length) {
                bytesRead = Transport.Read(buffer, offset, buffer.Length - offset);
                if (bytesRead == 0) {
                    if (offset == 0) {
                        // m_Eof, return null
                        m_Eof = true;
                        return null;
                    }
                    else {
                        throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
                    }
                }
                offset += bytesRead;
            }
                                            
            m_CurReadHeader.CopyFrom(buffer, 0, m_ReadVerifier);
            if (m_CurReadHeader.PayloadSize > m_CurReadHeader.MaxMessageSize)
            {
                throw new InvalidOperationException(SR.GetString(SR.net_frame_size,
                                                               m_CurReadHeader.MaxMessageSize.ToString(NumberFormatInfo.InvariantInfo),
                                                               m_CurReadHeader.PayloadSize.ToString(NumberFormatInfo.InvariantInfo)));
            }

            buffer = new byte[m_CurReadHeader.PayloadSize];

            offset = 0;
            while (offset < buffer.Length) {
                bytesRead = Transport.Read(buffer, offset, buffer.Length - offset);
                if (bytesRead == 0) {
                    throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
                }
                offset += bytesRead;
            }
            return buffer;
        }

        public IAsyncResult BeginReadMessage(AsyncCallback asyncCallback, object stateObject) {
            WorkerAsyncResult workerResult;

            if (m_Eof){
                workerResult = new WorkerAsyncResult(this, stateObject, asyncCallback, null, 0, 0);
                workerResult.InvokeCallback(-1);
                return workerResult;
            }
            workerResult = new WorkerAsyncResult(this, stateObject, asyncCallback,
                                                                   m_ReadHeaderBuffer, 0,
                                                                   m_ReadHeaderBuffer.Length);

            IAsyncResult result = Transport.BeginRead(m_ReadHeaderBuffer, 0, m_ReadHeaderBuffer.Length,
                                      m_ReadFrameCallback, workerResult);
            if (result.CompletedSynchronously)
            {
                ReadFrameComplete(result);
            }

            return workerResult;
        }

        private void ReadFrameCallback(IAsyncResult transportResult)
        {
            GlobalLog.Assert(transportResult.AsyncState is WorkerAsyncResult, "StreamFramer::ReadFrameCallback|The state expected to be WorkerAsyncResult, received:{0}.", transportResult.GetType().FullName);
            if (transportResult.CompletedSynchronously)
            {
                return;
            }

            WorkerAsyncResult workerResult = (WorkerAsyncResult) transportResult.AsyncState;

            try
            {
                ReadFrameComplete(transportResult);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }

                if (!(e is IOException)) {
                    e = new System.IO.IOException(SR.GetString(SR.net_io_readfailure, e.Message), e);
                }

                // Let's call user callback and he call us back and we will throw
                workerResult.InvokeCallback(e);
            }
        }

        // IO COMPLETION CALLBACK
        //
        // This callback is responsible for getting complete protocol frame
        // First, it reads the header
        // Second, it determines the frame size
        // Third, loops while not all frame received or an error.
        //
        private void ReadFrameComplete(IAsyncResult transportResult)
        {
            do
            {
                GlobalLog.Assert(transportResult.AsyncState is WorkerAsyncResult, "StreamFramer::ReadFrameComplete|The state expected to be WorkerAsyncResult, received:{0}.", transportResult.GetType().FullName);
                WorkerAsyncResult workerResult = (WorkerAsyncResult) transportResult.AsyncState;

                int bytesRead = Transport.EndRead(transportResult);
                workerResult.Offset += bytesRead;

                GlobalLog.Assert(workerResult.Offset <= workerResult.End, "StreamFramer::ReadFrameCallback|WRONG: offset - end = {0}", workerResult.Offset - workerResult.End);

                if (bytesRead <= 0) {
                    // (by design) This indicates the stream has receives EOF
                    // If we are in the middle of a Frame - fail, otherwise - produce EOF
                    object result = null;
                    if (!workerResult.HeaderDone && workerResult.Offset == 0) {
                        result = (object)-1;
                    }
                    else {
                        result = new System.IO.IOException(SR.GetString(SR.net_frame_read_io));

                    }
                    workerResult.InvokeCallback(result);
                    return;
                }

                if (workerResult.Offset >= workerResult.End) {
                    if (!workerResult.HeaderDone) {
                        workerResult.HeaderDone = true;
                        // This indicates the header has been read succesfully
                        m_CurReadHeader.CopyFrom(workerResult.Buffer, 0, m_ReadVerifier);
                        int payloadSize = m_CurReadHeader.PayloadSize;
                        if (payloadSize < 0) {
                            // Let's call user callback and he call us back and we will throw
                            workerResult.InvokeCallback(new System.IO.IOException(SR.GetString(SR.net_frame_read_size)));
                        }
                        if (payloadSize == 0) {
                            // report emtpy frame (NOT eof!) to the caller, he might be interested in
                            workerResult.InvokeCallback(0);
                            return;
                        }
                        if (payloadSize > m_CurReadHeader.MaxMessageSize)
                        {
                            throw new InvalidOperationException(SR.GetString(SR.net_frame_size,
                                                                            m_CurReadHeader.MaxMessageSize.ToString(NumberFormatInfo.InvariantInfo),
                                                                            payloadSize.ToString(NumberFormatInfo.InvariantInfo)));
                        }
                        // Start reading the remaining frame data (note header does not count)
                        byte[] frame = new byte[payloadSize];
                        // Save the ref of the data block
                        workerResult.Buffer = frame;
                        workerResult.End = frame.Length;
                        workerResult.Offset = 0;
                        // Transport.BeginRead below will pickup those changes
                    }
                    else {
                        workerResult.HeaderDone = false; //reset for optional object reuse
                        workerResult.InvokeCallback(workerResult.End);
                        return;
                    }
                }
                // This means we need more data to complete the data block
                transportResult = Transport.BeginRead(workerResult.Buffer, workerResult.Offset, workerResult.End - workerResult.Offset,
                                            m_ReadFrameCallback, workerResult);
            } while(transportResult.CompletedSynchronously);
        }

        //
        // User will call this when workerResult gets signalled
        //
        // On Beginread User always gets back our WorkerAsyncResult
        // The Result property represents either a number of bytes read or an
        // exception put by our async state machine
        //
        public byte[] EndReadMessage(IAsyncResult asyncResult) {
            if (asyncResult==null) {
                throw new ArgumentNullException("asyncResult");
            }
            WorkerAsyncResult workerResult = asyncResult as WorkerAsyncResult;

            if (workerResult == null) {
                throw new ArgumentException(SR.GetString(SR.net_io_async_result, typeof(WorkerAsyncResult).FullName), "asyncResult");
            }
            if (!workerResult.InternalPeekCompleted) {
                workerResult.InternalWaitForCompletion();
            }

            if (workerResult.Result is Exception) {
                throw (Exception)(workerResult.Result);
            }

            int size = (int)workerResult.Result;
            if (size == -1) {
                m_Eof = true;
                return null;
            }
            else if (size == 0) {
                //empty frame
                return new byte[0];
            }

            return workerResult.Buffer;
        }

        //
        //
        //
        //
        public void WriteMessage(byte[] message) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }
            m_WriteHeader.PayloadSize = message.Length;
            m_WriteHeader.CopyTo(m_WriteHeaderBuffer, 0);

            if (m_NetworkStream != null && message.Length != 0) {
                BufferOffsetSize[] buffers = new BufferOffsetSize[2];
                buffers[0] = new BufferOffsetSize(m_WriteHeaderBuffer, 0, m_WriteHeaderBuffer.Length, false);
                buffers[1] = new BufferOffsetSize(message, 0, message.Length, false);
                m_NetworkStream.MultipleWrite(buffers);
            }
            else {
                Transport.Write(m_WriteHeaderBuffer, 0, m_WriteHeaderBuffer.Length);
                if (message.Length==0) {
                    return;
                }
                Transport.Write(message, 0, message.Length);
            }
        }

        //
        //
        //
        //
        public IAsyncResult BeginWriteMessage(byte[] message, AsyncCallback asyncCallback, object stateObject) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }

            m_WriteHeader.PayloadSize = message.Length;
            m_WriteHeader.CopyTo(m_WriteHeaderBuffer, 0);

            if (m_NetworkStream != null && message.Length != 0) {
                BufferOffsetSize[] buffers = new BufferOffsetSize[2];
                buffers[0] = new BufferOffsetSize(m_WriteHeaderBuffer, 0, m_WriteHeaderBuffer.Length, false);
                buffers[1] = new BufferOffsetSize(message, 0, message.Length, false);
                return m_NetworkStream.BeginMultipleWrite(buffers, asyncCallback, stateObject);
            }

            if (message.Length == 0) {
                return Transport.BeginWrite(m_WriteHeaderBuffer, 0, m_WriteHeaderBuffer.Length,
                                                   asyncCallback, stateObject);
            }
            //Will need two async writes
            // Prepare the second
            WorkerAsyncResult workerResult = new WorkerAsyncResult(this, stateObject, asyncCallback,
                                                                   message, 0, message.Length);
            // Charge the first
            IAsyncResult result = Transport.BeginWrite(m_WriteHeaderBuffer, 0, m_WriteHeaderBuffer.Length,
                                 m_BeginWriteCallback, workerResult);
            if (result.CompletedSynchronously)
            {
                BeginWriteComplete(result);
            }

            return workerResult;
        }

        private void BeginWriteCallback(IAsyncResult transportResult) {
            GlobalLog.Assert(transportResult.AsyncState is WorkerAsyncResult, "StreamFramer::BeginWriteCallback|The state expected to be WorkerAsyncResult, received:{0}.", transportResult.AsyncState.GetType().FullName);
            if (transportResult.CompletedSynchronously)
            {
                return;
            }

            WorkerAsyncResult workerResult = (WorkerAsyncResult) transportResult.AsyncState;

            try
            {
                BeginWriteComplete(transportResult);
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                {
                    throw;
                }

                workerResult.InvokeCallback(e);
            }
        }

        // IO COMPLETION CALLBACK
        //
        // Called when user IO request was wrapped to do several underlined IO
        //
        private void BeginWriteComplete(IAsyncResult transportResult)
        {
            do
            {
                WorkerAsyncResult workerResult = (WorkerAsyncResult)transportResult.AsyncState;

                //First, complete the previous portion write
                Transport.EndWrite(transportResult);
                //Check on exit criterion
                if (workerResult.Offset == workerResult.End) {
                    workerResult.InvokeCallback();
                    return;
                }
                //setup exit criterion
                workerResult.Offset = workerResult.End;
                //Write next portion (frame body) using Async IO
                transportResult = Transport.BeginWrite(workerResult.Buffer, 0, workerResult.End,
                                            m_BeginWriteCallback, workerResult);
            }
            while (transportResult.CompletedSynchronously);
        }

        public void EndWriteMessage(IAsyncResult asyncResult) {
            if (asyncResult==null) {
                throw new ArgumentNullException("asyncResult");
            }

            WorkerAsyncResult workerResult = asyncResult as WorkerAsyncResult;

            if (workerResult != null) {
                if (!workerResult.InternalPeekCompleted) {
                    workerResult.InternalWaitForCompletion();
                }
                if (workerResult.Result is Exception) {
                    throw (Exception)(workerResult.Result);
                }
            }
            else {
                Transport.EndWrite(asyncResult);
            }
        }

        /*
        // Consider removing.
        public void Close() {
            Transport.Close();
        }
        */
    }

    //
    // This class wraps an Async IO request
    // It is based on our internal LazyAsyncResult helper
    // - If ParentResult is not null then the base class (LazyAsyncResult) methods must not be used
    //
    // - If ParentResult == null, then real user IO request is wrapped
    //

    /*
    // Consider removing.
    internal delegate void WorkerCallback(WorkerAsyncResult result);
    */

    internal class WorkerAsyncResult : LazyAsyncResult {
        public byte[]   Buffer;
        public int      Offset;
        public int      End;
        public bool     IsWrite;
        public WorkerAsyncResult ParentResult;
        /*
        // Consider removing.
        public WorkerCallback StepDoneCallback;
        */
        public bool     HeaderDone; // This migth be reworked so we read both header and frame in one chunk
        public bool     HandshakeDone;

        public WorkerAsyncResult(object asyncObject, object asyncState,
                                   AsyncCallback savedAsyncCallback,
                                   byte[] buffer, int offset, int end)
            : base( asyncObject, asyncState, savedAsyncCallback) {

                Buffer      = buffer;
                Offset      = offset;
                End     = end;
        }

        /*
        // Consider removing.
        public WorkerAsyncResult(WorkerAsyncResult parentResult, byte[] buffer, int offset, int end)
             : base(null, null, null) {

                ParentResult = parentResult;
                Buffer      = buffer;
                Offset      = offset;
                End         = end;
        }
        */
    }

    // This guy describes the header used in framing of the stream data.
    internal class FrameHeader {
        public const int IgnoreValue    = -1;
        public const int HandshakeDoneId= 20;
        public const int HandshakeErrId = 21;
        public const int HandshakeId    = 22;
        public const int DefaultMajorV  = 1;
        public const int DefaultMinorV  = 0;


        private int     _MessageId;
        private int     _MajorV;
        private int     _MinorV;
        private int     _PayloadSize;

        public FrameHeader () {
            _MessageId = HandshakeId;
            _MajorV    = DefaultMajorV;
            _MinorV    = DefaultMinorV;
            _PayloadSize = -1;

        }

        public FrameHeader (int messageId, int majorV, int minorV) {
            _MessageId = messageId;
            _MajorV    = majorV;
            _MinorV    = minorV;
            _PayloadSize = -1;
        }

        /*
        // Consider removing.
        public FrameHeader Clone() {
            return new FrameHeader(_MessageId, _MajorV, _MinorV);
        }
        */

        public int Size {
            get {
                return 5;
            }
        }

        public int MaxMessageSize {
            get {
                return 0xFFFF;
            }
        }

        public  int     MessageId {
            get {
                return  _MessageId;
            }
            set {
                _MessageId = value;
            }
        }

        public  int     MajorV {
            get {
                return  _MajorV;
            }
        }

        public  int     MinorV {
            get {
                return  _MinorV;
            }
        }

        public int      PayloadSize {
            get {
                return  _PayloadSize;
            }
            set {
                if (value > MaxMessageSize) {
                    throw new ArgumentException(SR.GetString(SR.net_frame_max_size,
                        MaxMessageSize.ToString(NumberFormatInfo.InvariantInfo),
                        value.ToString(NumberFormatInfo.InvariantInfo)), "PayloadSize");
                }
                _PayloadSize = value;
            }
        }

        public void CopyTo(byte[] dest, int start) {
            dest[start++] = (byte)_MessageId;
            dest[start++] = (byte)_MajorV;
            dest[start++] = (byte)_MinorV;
            dest[start++] = (byte)((_PayloadSize >> 8) & 0xFF);
            dest[start]   = (byte)(_PayloadSize & 0xFF);

        }

        public void CopyFrom(byte[] bytes, int start, FrameHeader verifier) {
            _MessageId      = bytes[start++];
            _MajorV         = bytes[start++];
            _MinorV         = bytes[start++];
            _PayloadSize    = (int) ((bytes[start++]<<8) | bytes[start]);

            if (verifier.MessageId != FrameHeader.IgnoreValue && MessageId != verifier.MessageId) {
                throw new InvalidOperationException(SR.GetString(SR.net_io_header_id, "MessageId", MessageId, verifier.MessageId));
            }

            if (verifier.MajorV != FrameHeader.IgnoreValue && MajorV != verifier.MajorV) {
                throw new InvalidOperationException(SR.GetString(SR.net_io_header_id, "MajorV", MajorV, verifier.MajorV));
            }

            if (verifier.MinorV != FrameHeader.IgnoreValue && MinorV != verifier.MinorV) {
                throw new InvalidOperationException(SR.GetString(SR.net_io_header_id, "MinorV", MinorV, verifier.MinorV));
            }

        }
    }
}
