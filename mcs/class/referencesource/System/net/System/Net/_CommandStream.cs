// ------------------------------------------------------------------------------
// <copyright file="CommandStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------
//


namespace System.Net {

    using System.Collections;
    using System.IO;
    using System.Security.Cryptography.X509Certificates ;
    using System.Net.Sockets;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Security.Authentication;


    /// <devdoc>
    /// <para>
    ///     Impliments basic sending and receiving of network commands.
    ///     Handles generic parsing of server responses and provides
    ///     a Pipeline sequencing mechnism for sending the commands to the
    ///     server.
    /// </para>
    /// </devdoc>
    internal class CommandStream : PooledStream {

        private static readonly AsyncCallback m_WriteCallbackDelegate = new AsyncCallback(WriteCallback);
        private static readonly AsyncCallback m_ReadCallbackDelegate = new AsyncCallback(ReadCallback);

        private bool m_RecoverableFailure;

        //
        // Active variables used for the command state machine
        //

        protected WebRequest        m_Request;
        protected bool              m_Async;
        private   bool              m_Aborted;

        protected PipelineEntry []  m_Commands;
        protected int               m_Index;
        private bool                m_DoRead;
        private bool                m_DoSend;
        private ResponseDescription m_CurrentResponseDescription;
        protected string            m_AbortReason;

        const int _WaitingForPipeline = 1;
        const int _CompletedPipeline  = 2;


        /// <devdoc>
        ///    <para>
        ///     Setups and Creates a NetworkStream connection to the server
        ///     perform any initalization if needed
        ///    </para>
        /// </devdoc>
        internal CommandStream(
            ConnectionPool connectionPool,
            TimeSpan lifetime,
            bool checkLifetime
            ) : base(connectionPool, lifetime, checkLifetime) {
                m_Decoder = m_Encoding.GetDecoder();
        }


        internal virtual void Abort(Exception e) {
            GlobalLog.Print("CommandStream"+ValidationHelper.HashString(this)+"::Abort() - closing control Stream");

            lock (this) {
                if (m_Aborted)
                    return;
                m_Aborted = true;
                CanBePooled = false;
            }

            try {
                base.Close(0);
            }
            finally {
                if (e != null) {
                    InvokeRequestCallback(e);
                } else {
                    InvokeRequestCallback(null);
                }
            }
        }

        /// <summary>
        ///    <para>Used to reset the connection</para>
        /// </summary>
        protected override void Dispose(bool disposing) {
            GlobalLog.Print("CommandStream"+ValidationHelper.HashString(this)+"::Close()");
            InvokeRequestCallback(null);

            // Do not call base.Dispose(bool), which would close the web request.
            // This stream effectively should be a wrapper around a web 
            // request that does not own the web request.
        }

        /// <summary>
        ///    <para>A WebRequest can use a different Connection after an Exception is set, or a null is passed
        ///         to mark completion.  We shouldn't continue calling the Request.RequestCallback after that point</para>
        /// </summary>
        protected void InvokeRequestCallback(object obj) {
            WebRequest webRequest = m_Request;
            if (webRequest != null) {
                webRequest.RequestCallback(obj);
            }
        }

        /// <summary>
        ///    <para>Indicates that we caught an error that should allow us to resubmit a request</para>
        /// </summary>
        internal bool RecoverableFailure {
            get {
                return m_RecoverableFailure;
            }
        }

        /// <summary>
        ///    <para>We only offer recovery, if we're at the start of the first command</para>
        /// </summary>
        protected void MarkAsRecoverableFailure() {
            if (m_Index <= 1) {
                m_RecoverableFailure = true;
            }
        }

        /// <devdoc>
        ///    <para>
        ///     Setups and Creates a NetworkStream connection to the server
        ///     perform any initalization if needed
        ///    </para>
        /// </devdoc>

        internal Stream SubmitRequest(WebRequest request, bool async, bool readInitalResponseOnConnect) {
            ClearState();
            UpdateLifetime();
            PipelineEntry [] commands = BuildCommandsList(request);
            InitCommandPipeline(request, commands, async);
            if(readInitalResponseOnConnect && JustConnected){
                m_DoSend = false;
                m_Index = -1;
            }
            return ContinueCommandPipeline();
        }

        protected virtual void ClearState() {
            InitCommandPipeline(null, null, false);
        }

        protected virtual PipelineEntry [] BuildCommandsList(WebRequest request) {
            return null;
        }

        protected Exception GenerateException(WebExceptionStatus status, Exception innerException) {
            return new WebException(
                            NetRes.GetWebStatusString("net_connclosed", status),
                            innerException,
                            status,
                            null /* no response */ );
        }


        protected Exception GenerateException(FtpStatusCode code, string statusDescription, Exception innerException) {

            return new WebException(SR.GetString(SR.net_servererror,NetRes.GetWebStatusCodeString(code, statusDescription)),
                                    innerException,WebExceptionStatus.ProtocolError,null );
        }


        protected void InitCommandPipeline(WebRequest request, PipelineEntry [] commands, bool async) {
            m_Commands = commands;
            m_Index = 0;
            m_Request = request;
            m_Aborted = false;
            m_DoRead = true;
            m_DoSend = true;
            m_CurrentResponseDescription = null;
            m_Async = async;
            m_RecoverableFailure = false;
            m_AbortReason = string.Empty;
        }

        internal void CheckContinuePipeline() 
        {
            if (m_Async)
                return;
            try {
                ContinueCommandPipeline();
            } catch (Exception e) {
                Abort(e);
            }
        }

        ///     Pipelined command resoluton, how this works:
        ///     a list of commands that need to be sent to the FTP server are spliced together into a array,
        ///     each command such STOR, PORT, etc, is sent to the server, then the response is parsed into a string,
        ///     with the response, the delegate is called, which returns an instruction (either continue, stop, or read additional
        ///     responses from server).
        ///
        /// When done calling Close() to Notify ConnectionGroup that we are free
        protected Stream ContinueCommandPipeline()
        {
            // In async case, The BeginWrite can actually result in a
            // series of synchronous completions that eventually close
            // the connection. So we need to save the members that 
            // we need to access, since they may not be valid after 
            // BeginWrite returns
            bool async = m_Async;
            while (m_Index < m_Commands.Length)
            {
                if (m_DoSend)
                {
                    if (m_Index < 0)
                        throw new InternalException();

                    byte[] sendBuffer = Encoding.GetBytes(m_Commands[m_Index].Command);
                    if (Logging.On) 
                    {
                        string sendCommand = m_Commands[m_Index].Command.Substring(0, m_Commands[m_Index].Command.Length-2);
                        if (m_Commands[m_Index].HasFlag(PipelineEntryFlags.DontLogParameter))
                        {
                            int index = sendCommand.IndexOf(' ');
                            if (index != -1)
                            sendCommand = sendCommand.Substring(0, index) + " ********";
                        }
                        Logging.PrintInfo(Logging.Web, this, SR.GetString(SR.net_log_sending_command, sendCommand));
                    }
                    try {
                        if (async) {
                            BeginWrite(sendBuffer, 0, sendBuffer.Length, m_WriteCallbackDelegate, this);
                        } else {
                            Write(sendBuffer, 0, sendBuffer.Length);
                        }
                    } catch (IOException) {
                        MarkAsRecoverableFailure();
                        throw;
                    } catch {
                        throw;
                    }

                    if (async) {
                        return null;
                    }
                }

                Stream stream = null;
                bool isReturn = PostSendCommandProcessing(ref stream);
                if (isReturn)
                {
                    return stream;
                }
            }

            lock (this)
            {
                Close();
            }

            return null;
        }
        //
        private bool PostSendCommandProcessing(ref Stream stream)
        {
/*
            ** I don;t see how this code can be still relevant, remove it of no problems observed **

            //
            // This is a general race condition in Sync mode, if the server returns an error
            // after we open the data connection, we will be off reading the data connection,
            // and not the control connection. The best we can do is try to poll, and in the
            // the worst case, we will timeout on establishing the data connection.
            //
            if (!m_DoRead && !m_Async) {
                m_DoRead = Poll(100 * 1000, SelectMode.SelectRead);   // Poll is in Microseconds.
            }
*/
            if (m_DoRead)
            {
                // In async case, The next call can actually result in a
                // series of synchronous completions that eventually close
                // the connection. So we need to save the members that 
                // we need to access, since they may not be valid after the 
                // next call returns
                bool async               = m_Async;
                int index                = m_Index;
                PipelineEntry[] commands = m_Commands;

                try {
                    ResponseDescription response = ReceiveCommandResponse();
                    if (async) {
                        return true;
                    }
                    m_CurrentResponseDescription = response;
                } catch {
                    // If we get an exception on the QUIT command (which is 
                    // always the last command), ignore the final exception
                    // and continue with the pipeline regardlss of sync/async
                    if (index < 0 || index >= commands.Length ||
                        commands[index].Command != "QUIT\r\n")
                        throw;
                }
            }
            return PostReadCommandProcessing(ref stream);
        }
        //
        private bool PostReadCommandProcessing(ref Stream stream)
        {
            if (m_Index >= m_Commands.Length)
                return false;

            // Set up front to prevent a race condition on result == PipelineInstruction.Pause
            m_DoSend = false;
            m_DoRead = false;

            PipelineInstruction result;
            PipelineEntry entry;
            if(m_Index == -1)
                entry = null;
            else
                entry = m_Commands[m_Index];

            // Final QUIT command may get exceptions since the connectin 
            // may be already closed by the server. So there is no response 
            // to process, just advance the pipeline to continue
            if (m_CurrentResponseDescription == null && entry.Command == "QUIT\r\n")
                result = PipelineInstruction.Advance;
            else 
                result = PipelineCallback(entry, m_CurrentResponseDescription, false, ref stream);

            if (result == PipelineInstruction.Abort)
            {
                Exception exception;
                if (m_AbortReason != string.Empty)
                    exception = new WebException(m_AbortReason);
                else
                    exception = GenerateException(WebExceptionStatus.ServerProtocolViolation, null);
                Abort(exception);
                throw exception;
            }
            else if (result == PipelineInstruction.Advance)
            {
                m_CurrentResponseDescription = null;
                m_DoSend = true;
                m_DoRead = true;
                m_Index++;

            }
            else if (result == PipelineInstruction.Pause)
            {
                //
                // PipelineCallback did an async operation and will have to re-enter again
                // Hold on for now
                //
                return true;
            }
            else if (result == PipelineInstruction.GiveStream)
            {
                //
                // We will have another response coming, don't send
                //
                m_CurrentResponseDescription = null;
                m_DoRead = true;
                if (m_Async)
                {
                    // If they block in the requestcallback we should still continue the pipeline
                    ContinueCommandPipeline();
                    InvokeRequestCallback(stream);
                }
                return true;
            }
            else if (result == PipelineInstruction.Reread)
            {
                // Another response is expected after this one
                m_CurrentResponseDescription = null;
                m_DoRead = true;
            }
            return false;
        }

        internal enum PipelineInstruction {
            Abort,          // aborts the pipeline
            Advance,        // advances to the next pipelined command
            Pause,          // Let async callback to continue the pipeline
            Reread,         // rereads from the command socket
            GiveStream,     // returns with open data stream, let stream close to continue
        }

        [Flags]
        internal enum PipelineEntryFlags {
            UserCommand           = 0x1,
            GiveDataStream        = 0x2,
            CreateDataConnection  = 0x4,
            DontLogParameter      = 0x8
        }

        internal class PipelineEntry {
            internal PipelineEntry(string command) {
                Command = command;
            }
            internal PipelineEntry(string command, PipelineEntryFlags flags) {
                Command = command;
                Flags = flags;
            }
            internal bool HasFlag(PipelineEntryFlags flags) {
                return (Flags & flags) != 0;
            }
            internal string Command;
            internal PipelineEntryFlags Flags;
        }

        protected virtual PipelineInstruction PipelineCallback(PipelineEntry entry, ResponseDescription response, bool timeout, ref Stream stream) {
            return PipelineInstruction.Abort;
        }

        //
        // I/O callback methods
        //

        /// <summary>
        ///    <para>Provides a wrapper for the async operations, so that the code can be shared with sync</para>
        /// </summary>
        private static void ReadCallback(IAsyncResult asyncResult) {
            ReceiveState state = (ReceiveState)asyncResult.AsyncState;
            try {
                Stream stream = (Stream)state.Connection;
                int bytesRead = 0;
                try {
                    bytesRead = stream.EndRead(asyncResult);
                    if (bytesRead == 0)
                        state.Connection.CloseSocket();
                } 
                catch (IOException) {
                    state.Connection.MarkAsRecoverableFailure();
                    throw;
                }
                catch {
                    throw;
                }

                state.Connection.ReceiveCommandResponseCallback(state, bytesRead);
            } catch (Exception e) {
                state.Connection.Abort(e);
            }
        }


        /// <summary>
        ///    <para>Provides a wrapper for the async write operations</para>
        /// </summary>
        private static void WriteCallback(IAsyncResult asyncResult) {
            CommandStream connection = (CommandStream)asyncResult.AsyncState;
            try {
                try {
                    connection.EndWrite(asyncResult);
                } 
                catch (IOException) {
                    connection.MarkAsRecoverableFailure();
                    throw;
                }
                catch {
                    throw;
                }
                Stream stream = null;
                if (connection.PostSendCommandProcessing(ref stream))
                    return;
                connection.ContinueCommandPipeline();
            } catch (Exception e) {
                connection.Abort(e);
            }
        }

        //
        // Read parsing methods and privates
        //

        private string m_Buffer = string.Empty;
        private Encoding m_Encoding = Encoding.UTF8;
        private Decoder m_Decoder;


        protected Encoding Encoding {
            get {
                return m_Encoding;
            }
            set {
                m_Encoding = value;
                m_Decoder = m_Encoding.GetDecoder();
            }
        }

        /// <summary>
        /// This function is called a derived class to determine whether a response is valid, and when it is complete.
        /// </summary>
        protected virtual bool CheckValid(ResponseDescription response, ref int validThrough, ref int completeLength) {
            return false;
        }

        /// <summary>
        /// Kicks off an asynchronous or sync request to receive a response from the server.
        /// Uses the Encoding <code>encoding</code> to transform the bytes received into a string to be
        /// returned in the GeneralResponseDescription's StatusDescription field.
        /// </summary>
        private ResponseDescription ReceiveCommandResponse()
        {
            // These are the things that will be needed to maintain state
            ReceiveState state = new ReceiveState(this);

            try
            {
                // If a string of nonzero length was decoded from the buffered bytes after the last complete response, then we
                // will use this string as our first string to append to the response StatusBuffer, and we will
                // forego a Connection.Receive here.
                if(m_Buffer.Length > 0)
                {
                    ReceiveCommandResponseCallback(state, -1);
                }
                else
                {
                    int bytesRead;

                    try {
                        if (m_Async) {
                            BeginRead(state.Buffer, 0, state.Buffer.Length, m_ReadCallbackDelegate, state);
                            return null;
                        } else {
                            bytesRead = Read(state.Buffer, 0, state.Buffer.Length);
                            if (bytesRead == 0)
                                CloseSocket();
                            ReceiveCommandResponseCallback(state, bytesRead);
                        }
                    } 
                    catch (IOException) {
                        MarkAsRecoverableFailure();
                        throw;
                    }
                    catch {
                        throw;
                    }
                }
            }
            catch(Exception e) {
                if (e is WebException)
                    throw;
                throw GenerateException(WebExceptionStatus.ReceiveFailure, e);
            }
            return state.Resp;
        }


        /// <summary>
        /// ReceiveCommandResponseCallback is the main "while loop" of the ReceiveCommandResponse function family.
        /// In general, what is does is perform an EndReceive() to complete the previous retrieval of bytes from the
        /// server (unless it is using a buffered response)  It then processes what is received by using the
        /// implementing class's CheckValid() function, as described above. If the response is complete, it returns the single complete
        /// response in the GeneralResponseDescription created in BeginReceiveComamndResponse, and buffers the rest as described above.
        ///
        /// If the resposne is not complete, it issues another Connection.BeginReceive, with callback ReceiveCommandResponse2,
        /// so the action will continue at the next invocation of ReceiveCommandResponse2.
        /// </summary>
        /// <param name="asyncResult"></param>
        ///
        private void ReceiveCommandResponseCallback(ReceiveState state, int bytesRead)
        {
            // completeLength will be set to a nonnegative number by CheckValid if the response is complete:
            // it will set completeLength to the length of a complete response.
            int completeLength = -1;

            while (true)
            {
                int validThrough = state.ValidThrough; // passed to checkvalid


                // If we have a Buffered response (ie data was received with the last response that was past the end of that response)
                // deal with it as if we had just received it now instead of actually doing another receive
                if(m_Buffer.Length > 0)
                {
                    // Append the string we got from the buffer, and flush it out.
                    state.Resp.StatusBuffer.Append(m_Buffer);
                    m_Buffer = string.Empty;

                    // invoke checkvalid.
                    if(!CheckValid(state.Resp, ref validThrough, ref completeLength)) {
                        throw GenerateException(WebExceptionStatus.ServerProtocolViolation, null);
                    }
                }
                else // we did a Connection.BeginReceive.  Note that in this case, all bytes received are in the receive buffer (because bytes from
                    // the buffer were transferred there if necessary
                {
                    // this indicates the connection was closed.
                    if(bytesRead <= 0)  {
                        throw GenerateException(WebExceptionStatus.ServerProtocolViolation, null);
                    }

                    // decode the bytes in the receive buffer into a string, append it to the statusbuffer, and invoke checkvalid.
                    // Decoder automatically takes care of caching partial codepoints at the end of a buffer.

                    char[] chars = new char[m_Decoder.GetCharCount(state.Buffer, 0, bytesRead)];
                    int numChars = m_Decoder.GetChars(state.Buffer, 0, bytesRead, chars, 0, false);
                    
                    string szResponse = new string(chars, 0, numChars);

                    state.Resp.StatusBuffer.Append(szResponse);
                    if(!CheckValid(state.Resp, ref validThrough, ref completeLength))
                    {
                        throw GenerateException(WebExceptionStatus.ServerProtocolViolation, null);
                    }

                    // If the response is complete, then determine how many characters are left over...these bytes need to be set into Buffer.
                    if(completeLength >= 0)
                    {
                        int unusedChars = state.Resp.StatusBuffer.Length - completeLength;
                        if (unusedChars > 0) {
                            m_Buffer = szResponse.Substring(szResponse.Length-unusedChars, unusedChars);
                        }
                    }
                }

                // Now, in general, if the response is not complete, update the "valid through" length for the efficiency of checkValid.
                // and perform the next receive.
                // Note that there may NOT be bytes in the beginning of the receive buffer (even if there were partial characters left over after the
                // last encoding), because they get tracked in the Decoder.
                if(completeLength < 0)
                {
                    state.ValidThrough = validThrough;
                    try {
                        if (m_Async) {
                            BeginRead(state.Buffer, 0, state.Buffer.Length, m_ReadCallbackDelegate, state);
                            return;
                        } else {
                            bytesRead = Read(state.Buffer, 0, state.Buffer.Length);
                            if (bytesRead == 0)
                                CloseSocket();
                            continue;
                        }
                    } 
                    catch (IOException) {
                        MarkAsRecoverableFailure();
                        throw;
                    }
                    catch {
                        throw;
                    }
                }
                // the response is completed
                break;
            }


            // Otherwise, we have a complete response.
            string responseString = state.Resp.StatusBuffer.ToString();
            state.Resp.StatusDescription = responseString.Substring(0, completeLength);
            // set the StatusDescription to the complete part of the response.  Note that the Buffer has already been taken care of above.

            if (Logging.On) Logging.PrintInfo(Logging.Web, this, SR.GetString(SR.net_log_received_response, responseString.Substring(0, completeLength-2)));

            if (m_Async) {
                // Tell who is listening what was received.
                if (state.Resp != null) {
                    m_CurrentResponseDescription = state.Resp;
                }
                Stream stream = null;
                if (PostReadCommandProcessing(ref stream))
                    return;
                ContinueCommandPipeline();
            }
        }

    } // class CommandStream


    /// <summary>
    /// Contains the parsed status line from the server
    /// </summary>
    internal class ResponseDescription {
        internal const int NoStatus = -1;
        internal bool Multiline = false;

        internal int           Status = NoStatus;
        internal string        StatusDescription;
        internal StringBuilder StatusBuffer = new StringBuilder();

        internal string        StatusCodeString;

        internal bool PositiveIntermediate   { get { return (Status >= 100 && Status <= 199); } }
        internal bool PositiveCompletion     { get { return (Status >= 200 && Status <= 299); } }
        //internal bool PositiveAuthRelated { get { return (Status >= 300 && Status <= 399); } }
        internal bool TransientFailure { get { return (Status >= 400 && Status <= 499); }     }
        internal bool PermanentFailure { get { return (Status >= 500 && Status <= 599); }    }
        internal bool InvalidStatusCode { get { return (Status < 100 || Status > 599); }    }
    }


    /// <summary>
    /// State information that is used during ReceiveCommandResponse()'s async operations
    /// </summary>
    internal class ReceiveState
    {
        private const int bufferSize = 1024;

        internal ResponseDescription Resp;
        internal int ValidThrough;
        internal byte[] Buffer;
        internal CommandStream Connection;

        internal ReceiveState(CommandStream connection)
        {
            Connection = connection;
            Resp = new ResponseDescription();
            Buffer = new byte[bufferSize];  //1024
            ValidThrough = 0;
        }
    }



} // namespace System.Net
