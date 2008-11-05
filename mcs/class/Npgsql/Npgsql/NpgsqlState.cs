// created on 6/14/2002 at 7:56 PM

// Npgsql.NpgsqlState.cs
//
// Author:
//     Dave Joyner <d4ljoyn@yahoo.com>
//
//    Copyright (C) 2002 The Npgsql Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.


using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Net;
using System.Threading;

namespace Npgsql
{
    ///<summary> This class represents the base class for the state pattern design pattern
    /// implementation.
    /// </summary>
    ///
    internal abstract class NpgsqlState
    {
        protected static readonly Encoding ENCODING_UTF8 = Encoding.UTF8;
        private readonly String CLASSNAME = "NpgsqlState";
        protected static ResourceManager resman = new ResourceManager(typeof (NpgsqlState));

        internal NpgsqlState()
        {
        }

        public virtual void Open(NpgsqlConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Startup(NpgsqlConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Authenticate(NpgsqlConnector context, string password)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual IEnumerable<IServerResponseObject> QueryEnum(NpgsqlConnector context, NpgsqlCommand command)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public void Query(NpgsqlConnector context, NpgsqlCommand command)
        {
            IterateThroughAllResponses(QueryEnum(context, command));
        }

        public virtual void FunctionCall(NpgsqlConnector context, NpgsqlCommand command)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Parse(NpgsqlConnector context, NpgsqlParse parse)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Flush(NpgsqlConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual IEnumerable<IServerResponseObject> SyncEnum(NpgsqlConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public void TestNotify(NpgsqlConnector context)
        {
            //ZA  Hnotifytest CNOTIFY Z
            //Qlisten notifytest;notify notifytest; 
            Stream stm = context.Stream;
            string uuidString = "uuid" + Guid.NewGuid().ToString("N");
            PGUtil.WriteString("Qlisten " + uuidString + ";notify " + uuidString + ";", stm);
            Queue<byte> buffer = new Queue<byte>();
            byte[] convertBuffer = new byte[36];
            for (;;)
            {
                int newByte = stm.ReadByte();
                if (newByte == -1)
                {
                    throw new EndOfStreamException();
                }
                buffer.Enqueue((byte) newByte);
                if (buffer.Count > 35)
                {
                    buffer.CopyTo(convertBuffer, 0);
                    if (ENCODING_UTF8.GetString(convertBuffer) == uuidString)
                    {
                        for (;;)
                        {
                            switch (stm.ReadByte())
                            {
                                case -1:
                                    throw new EndOfStreamException();
                                case 'Z':
                                    context.Query(new NpgsqlCommand("UNLISTEN *", context));
                                    return;
                            }
                        }
                    }
                    else
                    {
                        buffer.Dequeue();
                    }
                }
            }
        }

        public void TestConnector(NpgsqlConnector context)
        {
            switch (context.BackendProtocolVersion)
            {
                case ProtocolVersion.Version2:
                    TestNotify(context);
                    break;
                case ProtocolVersion.Version3:
                    EmptySync(context);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void EmptySync(NpgsqlConnector context)
        {
            Stream stm = context.Stream;
            new NpgsqlSync().WriteToStream(stm);
            stm.Flush();
            Queue<int> buffer = new Queue<int>();
            //byte[] compareBuffer = new byte[6];
            int[] messageSought = new int[] {'Z', 0, 0, 0, 5};
            int newByte;
            for (;;)
            {
                switch (newByte = stm.ReadByte())
                {
                    case -1:
                        throw new EndOfStreamException();
                    case 'E':
                    case 'I':
                    case 'T':
                        if (buffer.Count > 4)
                        {
                            bool match = true;
                            int i = 0;
                            foreach (byte cmp in buffer)
                            {
                                if (cmp != messageSought[i++])
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                return;
                            }
                        }
                        break;
                    default:
                        buffer.Enqueue(newByte);
                        if (buffer.Count > 5)
                        {
                            buffer.Dequeue();
                        }
                        break;
                }
            }
        }

        public NpgsqlRowDescription Sync(NpgsqlConnector context)
        {
            NpgsqlRowDescription lastDescription = null;
            foreach (IServerResponseObject obj in SyncEnum(context))
            {
                if (obj is NpgsqlRowDescription)
                {
                    lastDescription = obj as NpgsqlRowDescription;
                }
                else
                {
                    if (obj is IDisposable)
                    {
                        (obj as IDisposable).Dispose();
                    }
                }
            }
            return lastDescription;
        }

        public virtual void Bind(NpgsqlConnector context, NpgsqlBind bind)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Execute(NpgsqlConnector context, NpgsqlExecute execute)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual IEnumerable<IServerResponseObject> ExecuteEnum(NpgsqlConnector context, NpgsqlExecute execute)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Describe(NpgsqlConnector context, NpgsqlDescribe describe)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void CancelRequest(NpgsqlConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        // COPY methods

        protected virtual void StartCopy(NpgsqlConnector context, NpgsqlCopyFormat copyFormat)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual byte[] GetCopyData(NpgsqlConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void SendCopyData(NpgsqlConnector context, byte[] buf, int off, int len)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void SendCopyDone(NpgsqlConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void SendCopyFail(NpgsqlConnector context, String message)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual NpgsqlCopyFormat CopyFormat
        {
            get { throw new InvalidOperationException("Internal Error! " + this); }
        }


        public virtual void Close(NpgsqlConnector context)
        {
            try
            {
                context.Stream.Close();
            }
            catch
            {
            }
            context.Stream = null;
            ChangeState(context, NpgsqlClosedState.Instance);
        }

        ///<summary>
        ///This method is used by the states to change the state of the context.
        /// </summary>
        protected static void ChangeState(NpgsqlConnector context, NpgsqlState newState)
        {
            context.CurrentState = newState;
        }

        ///<summary>
        /// This method is responsible to handle all protocol messages sent from the backend.
        /// It holds all the logic to do it.
        /// To exchange data, it uses a Mediator object from which it reads/writes information
        /// to handle backend requests.
        /// </summary>
        ///
        internal void ProcessBackendResponses(NpgsqlConnector context)
        {
            IterateThroughAllResponses(ProcessBackendResponsesEnum(context));
        }

        private static void IterateThroughAllResponses(IEnumerable<IServerResponseObject> ienum)
        {
            foreach (IServerResponseObject obj in ienum) //iterate until finished.
            {
                if (obj is IDisposable)
                {
                    (obj as IDisposable).Dispose();
                }
            }
        }

        private class ContextResetter : IDisposable
        {
            private readonly NpgsqlConnector _connector;

            public ContextResetter(NpgsqlConnector connector)
            {
                _connector = connector;
            }

            public void Dispose()
            {
                _connector.RequireReadyForQuery = true;
            }
        }

        ///<summary>
        /// This method is responsible to handle all protocol messages sent from the backend.
        /// It holds all the logic to do it.
        /// To exchange data, it uses a Mediator object from which it reads/writes information
        /// to handle backend requests.
        /// </summary>
        ///
        internal IEnumerable<IServerResponseObject> ProcessBackendResponsesEnum(NpgsqlConnector context)
        {
            try
            {
            // Process commandTimeout behavior.

            if ((context.Mediator.CommandTimeout > 0) &&
                (!context.Socket.Poll(1000000*context.Mediator.CommandTimeout, SelectMode.SelectRead)))
            {
                // If timeout occurs when establishing the session with server then
                // throw an exception instead of trying to cancel query. This helps to prevent loop as CancelRequest will also try to stablish a connection and sends commands.
                if (!((this is NpgsqlStartupState || this is NpgsqlConnectedState)))
                {
                    try
                    {
                        context.CancelRequest();
                    }
                    catch
                    {
                    }
                    //We should have gotten an error from CancelRequest(). Whether we did or not, what we
                    //really have is a timeout exception, and that will be less confusing to the user than
                    //"operation cancelled by user" or similar, so whatever the case, that is what we'll throw.
                    // Changed message again to report about the two possible timeouts: connection or command as the establishment timeout only was confusing users when the timeout was a command timeout.
                }
                throw new NpgsqlException(resman.GetString("Exception_ConnectionOrCommandTimeout"));
            }
            switch (context.BackendProtocolVersion)
            {
                case ProtocolVersion.Version2:
                    return ProcessBackendResponses_Ver_2(context);
                case ProtocolVersion.Version3:
                    return ProcessBackendResponses_Ver_3(context);
                default:
                    throw new NpgsqlException(resman.GetString("Exception_UnknownProtocol"));
            }
            
            }
            
            catch(ThreadAbortException)
            {
                try
                {
                    context.CancelRequest();
                    context.Close();
                }
                catch {}
                
                throw;
            }
                
        }

        protected IEnumerable<IServerResponseObject> ProcessBackendResponses_Ver_2(NpgsqlConnector context)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ProcessBackendResponses");

            using (new ContextResetter(context))
            {
                Stream stream = context.Stream;
                NpgsqlMediator mediator = context.Mediator;
                NpgsqlRowDescription lastRowDescription = null;
                List<NpgsqlError> errors = new List<NpgsqlError>();

                for (;;)
                {
                    // Check the first Byte of response.
                    switch ((BackEndMessageCode) stream.ReadByte())
                    {
                        case BackEndMessageCode.ErrorResponse:

                            {
                                NpgsqlError error = new NpgsqlError(context.BackendProtocolVersion, stream);
                                error.ErrorSql = mediator.SqlSent;

                                errors.Add(error);

                                NpgsqlEventLog.LogMsg(resman, "Log_ErrorResponse", LogLevel.Debug, error.Message);
                            }

                            // Return imediately if it is in the startup state or connected state as
                            // there is no more messages to consume.
                            // Possible error in the NpgsqlStartupState:
                            //        Invalid password.
                            // Possible error in the NpgsqlConnectedState:
                            //        No pg_hba.conf configured.

                            if (!context.RequireReadyForQuery)
                            {
                                throw new NpgsqlException(errors);
                            }

                            break;


                        case BackEndMessageCode.AuthenticationRequest:
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "AuthenticationRequest");
                            AuthenticationRequestType authType = (AuthenticationRequestType) PGUtil.ReadInt32(stream);
                            switch (authType)
                            {
                                case AuthenticationRequestType.AuthenticationOk:
                                    NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationOK", LogLevel.Debug);
                                    break;
                                case AuthenticationRequestType.AuthenticationClearTextPassword:
                                    NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationClearTextRequest", LogLevel.Debug);
                                    // Send the PasswordPacket.
                                    ChangeState(context, NpgsqlStartupState.Instance);
                                    context.Authenticate(context.Password);
                                    break;
                                case AuthenticationRequestType.AuthenticationMD5Password:
                                    NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationMD5Request", LogLevel.Debug);
                                    // Now do the "MD5-Thing"
                                    // for this the Password has to be:
                                    // 1. md5-hashed with the username as salt
                                    // 2. md5-hashed again with the salt we get from the backend


                                    MD5 md5 = MD5.Create();


                                    // 1.
                                    byte[] passwd = ENCODING_UTF8.GetBytes(context.Password);
                                    byte[] saltUserName = ENCODING_UTF8.GetBytes(context.UserName);

                                    byte[] crypt_buf = new byte[passwd.Length + saltUserName.Length];

                                    passwd.CopyTo(crypt_buf, 0);
                                    saltUserName.CopyTo(crypt_buf, passwd.Length);


                                    StringBuilder sb = new StringBuilder();
                                    byte[] hashResult = md5.ComputeHash(crypt_buf);
                                    foreach (byte b in hashResult)
                                    {
                                        sb.Append(b.ToString("x2"));
                                    }


                                    String prehash = sb.ToString();

                                    byte[] prehashbytes = ENCODING_UTF8.GetBytes(prehash);


                                    byte[] saltServer = new byte[4];
                                    stream.Read(saltServer, 0, 4);
                                    // Send the PasswordPacket.
                                    ChangeState(context, NpgsqlStartupState.Instance);


                                    // 2.

                                    crypt_buf = new byte[prehashbytes.Length + saltServer.Length];
                                    prehashbytes.CopyTo(crypt_buf, 0);
                                    saltServer.CopyTo(crypt_buf, prehashbytes.Length);

                                    sb = new StringBuilder("md5"); // This is needed as the backend expects md5 result starts with "md5"
                                    hashResult = md5.ComputeHash(crypt_buf);
                                    foreach (byte b in hashResult)
                                    {
                                        sb.Append(b.ToString("x2"));
                                    }

                                    context.Authenticate(sb.ToString());

                                    break;
                                default:
                                    // Only AuthenticationClearTextPassword and AuthenticationMD5Password supported for now.
                                    errors.Add(
                                        new NpgsqlError(context.BackendProtocolVersion,
                                                        String.Format(resman.GetString("Exception_AuthenticationMethodNotSupported"), authType)));
                                    throw new NpgsqlException(errors);
                            }
                            break;
                        case BackEndMessageCode.RowDescription:
                            yield return lastRowDescription = new NpgsqlRowDescriptionV2(stream, context.OidToNameMapping);
                            ;
                            break;
                        case BackEndMessageCode.DataRow:
                            yield return new ForwardsOnlyRow(new StringRowReaderV2(lastRowDescription, stream));
                            break;
                        case BackEndMessageCode.BinaryRow:
                            throw new NotSupportedException();
                        case BackEndMessageCode.ReadyForQuery:
                            ChangeState(context, NpgsqlReadyState.Instance);
                            if (errors.Count != 0)
                            {
                                throw new NpgsqlException(errors);
                            }
                            yield break;
                        case BackEndMessageCode.BackendKeyData:
                            context.BackEndKeyData = new NpgsqlBackEndKeyData(context.BackendProtocolVersion, stream);
                            break;
                        case BackEndMessageCode.NoticeResponse:
                            context.FireNotice(new NpgsqlError(context.BackendProtocolVersion, stream));
                            break;
                        case BackEndMessageCode.CompletedResponse:
                            yield return new CompletedResponse(stream);
                            break;

                        case BackEndMessageCode.CursorResponse:
                            // This is the cursor response message.
                            // It is followed by a C NULL terminated string with the name of
                            // the cursor in a FETCH case or 'blank' otherwise.
                            // In this case it should be always 'blank'.
                            // [FIXME] Get another name for this function.
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CursorResponse");

                            String cursorName = PGUtil.ReadString(stream);
                            // Continue waiting for ReadyForQuery message.
                            break;

                        case BackEndMessageCode.EmptyQueryResponse:
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "EmptyQueryResponse");
                            PGUtil.ReadString(stream);
                            break;

                        case BackEndMessageCode.NotificationResponse:
                            context.FireNotification(new NpgsqlNotificationEventArgs(stream, false));
                            if (context.IsNotificationThreadRunning)
                            {
                                yield break;
                            }
                            break;
                        case BackEndMessageCode.IO_ERROR:
                            // Connection broken. Mono returns -1 instead of throw an exception as ms.net does.
                            throw new IOException();

                        default:
                            // This could mean a number of things
                            //   We've gotten out of sync with the backend?
                            //   We need to implement this type?
                            //   Backend has gone insane?
                            throw new DataException("Backend sent unrecognized response type");
                    }
                }
            }
        }

        private enum BackEndMessageCode
        {
            IO_ERROR = -1, // Connection broken. Mono returns -1 instead of throwing an exception as ms.net does.

            CopyData = 'd',
            CopyDone = 'c',
            DataRow = 'D',
            BinaryRow = 'B', //Version 2 only

            BackendKeyData = 'K',
            CancelRequest = 'F',
            CompletedResponse = 'C',
            CopyDataRows = ' ',
            CopyInResponse = 'G',
            CopyOutResponse = 'H',
            CursorResponse = 'P', //Version 2 only
            EmptyQueryResponse = 'I',
            ErrorResponse = 'E',
            FunctionCall = 'F',
            FunctionCallResponse = 'V',

            AuthenticationRequest = 'R',

            NoticeResponse = 'N',
            NotificationResponse = 'A',
            ParameterStatus = 'S',
            PasswordPacket = ' ',
            ReadyForQuery = 'Z',
            RowDescription = 'T',
            SSLRequest = ' ',

            // extended query backend messages
            ParseComplete = '1',
            BindComplete = '2',
            PortalSuspended = 's',
            ParameterDescription = 't',
            NoData = 'n',
            CloseComplete = '3'
        }

        private enum AuthenticationRequestType
        {
            AuthenticationOk = 0,
            AuthenticationKerberosV4 = 1,
            AuthenticationKerberosV5 = 2,
            AuthenticationClearTextPassword = 3,
            AuthenticationCryptPassword = 4,
            AuthenticationMD5Password = 5,
            AuthenticationSCMCredential = 6,
            AuthenticationGSS = 7,
            AuthenticationGSSContinue = 8,
            AuthenticationSSPI = 9
        }

        protected IEnumerable<IServerResponseObject> ProcessBackendResponses_Ver_3(NpgsqlConnector context)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ProcessBackendResponses");

            using (new ContextResetter(context))
            {
                Stream stream = context.Stream;
                NpgsqlMediator mediator = context.Mediator;

                NpgsqlRowDescription lastRowDescription = null;

                List<NpgsqlError> errors = new List<NpgsqlError>();

                for (;;)
                {
                    // Check the first Byte of response.
                    BackEndMessageCode message = (BackEndMessageCode) stream.ReadByte();
                    switch (message)
                    {
                        case BackEndMessageCode.ErrorResponse:

                            NpgsqlError error = new NpgsqlError(context.BackendProtocolVersion, stream);
                            error.ErrorSql = mediator.SqlSent;

                            errors.Add(error);

                            NpgsqlEventLog.LogMsg(resman, "Log_ErrorResponse", LogLevel.Debug, error.Message);

                            // Return imediately if it is in the startup state or connected state as
                            // there is no more messages to consume.
                            // Possible error in the NpgsqlStartupState:
                            //        Invalid password.
                            // Possible error in the NpgsqlConnectedState:
                            //        No pg_hba.conf configured.

                            if (!context.RequireReadyForQuery)
                            {
                                throw new NpgsqlException(errors);
                            }

                            break;
                        case BackEndMessageCode.AuthenticationRequest:

                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "AuthenticationRequest");

                            // Get the length in case we're getting AuthenticationGSSContinue 
                            int authDataLength = PGUtil.ReadInt32(stream) - 8;

                            AuthenticationRequestType authType = (AuthenticationRequestType) PGUtil.ReadInt32(stream);
                            switch (authType)
                            {
                                case AuthenticationRequestType.AuthenticationOk:
                                    NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationOK", LogLevel.Debug);
                                    break;
                                case AuthenticationRequestType.AuthenticationClearTextPassword:
                                    NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationClearTextRequest", LogLevel.Debug);

                                    // Send the PasswordPacket.

                                    ChangeState(context, NpgsqlStartupState.Instance);
                                    context.Authenticate(context.Password);

                                    break;
                                case AuthenticationRequestType.AuthenticationMD5Password:
                                    NpgsqlEventLog.LogMsg(resman, "Log_AuthenticationMD5Request", LogLevel.Debug);
                                    // Now do the "MD5-Thing"
                                    // for this the Password has to be:
                                    // 1. md5-hashed with the username as salt
                                    // 2. md5-hashed again with the salt we get from the backend


                                    MD5 md5 = MD5.Create();


                                    // 1.
                                    byte[] passwd = ENCODING_UTF8.GetBytes(context.Password);
                                    byte[] saltUserName = ENCODING_UTF8.GetBytes(context.UserName);

                                    byte[] crypt_buf = new byte[passwd.Length + saltUserName.Length];

                                    passwd.CopyTo(crypt_buf, 0);
                                    saltUserName.CopyTo(crypt_buf, passwd.Length);


                                    StringBuilder sb = new StringBuilder();
                                    byte[] hashResult = md5.ComputeHash(crypt_buf);
                                    foreach (byte b in hashResult)
                                    {
                                        sb.Append(b.ToString("x2"));
                                    }


                                    String prehash = sb.ToString();

                                    byte[] prehashbytes = ENCODING_UTF8.GetBytes(prehash);
                                    crypt_buf = new byte[prehashbytes.Length + 4];


                                    stream.Read(crypt_buf, prehashbytes.Length, 4);
                                    // Send the PasswordPacket.
                                    ChangeState(context, NpgsqlStartupState.Instance);


                                    // 2.
                                    prehashbytes.CopyTo(crypt_buf, 0);

                                    sb = new StringBuilder("md5"); // This is needed as the backend expects md5 result starts with "md5"
                                    hashResult = md5.ComputeHash(crypt_buf);
                                    foreach (byte b in hashResult)
                                    {
                                        sb.Append(b.ToString("x2"));
                                    }

                                    context.Authenticate(sb.ToString());

                                    break;
#if WINDOWS && UNMANAGED

                                case AuthenticationRequestType.AuthenticationSSPI:
                                    {
                                        if (context.IntegratedSecurity)
                                        {
                                            // For SSPI we have to get the IP-Address (hostname doesn't work)
                                            string ipAddressString = ((IPEndPoint)context.Socket.RemoteEndPoint).Address.ToString();
                                            context.SSPI = new SSPIHandler(ipAddressString, "POSTGRES");
                                            ChangeState(context, NpgsqlStartupState.Instance);
                                            context.Authenticate(context.SSPI.Continue(null));
                                            break;
                                        }
                                        else
                                        {
                                            // TODO: correct exception
                                            throw new Exception();
                                        }
                                    }


                                case AuthenticationRequestType.AuthenticationGSSContinue:
                                    {
                                        byte[] authData = new byte[authDataLength];
                                        PGUtil.CheckedStreamRead(stream, authData, 0, authDataLength);
                                        context.Authenticate(context.SSPI.Continue(authData));
                                        break;
                                    }

#endif

                                default:
                                    // Only AuthenticationClearTextPassword and AuthenticationMD5Password supported for now.
                                    errors.Add(
                                        new NpgsqlError(context.BackendProtocolVersion,
                                                        String.Format(resman.GetString("Exception_AuthenticationMethodNotSupported"), authType)));
                                    throw new NpgsqlException(errors);
                            }
                            break;
                        case BackEndMessageCode.RowDescription:
                            yield return lastRowDescription = new NpgsqlRowDescriptionV3(stream, context.OidToNameMapping);
                            break;
                        case BackEndMessageCode.ParameterDescription:

                            // Do nothing,for instance,  just read...
                            int lenght = PGUtil.ReadInt32(stream);
                            int nb_param = PGUtil.ReadInt16(stream);
                            for (int i = 0; i < nb_param; i++)
                            {
                                int typeoid = PGUtil.ReadInt32(stream);
                            }

                            break;

                        case BackEndMessageCode.DataRow:
                            yield return new ForwardsOnlyRow(new StringRowReaderV3(lastRowDescription, stream));
                            break;

                        case BackEndMessageCode.ReadyForQuery:

                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ReadyForQuery");

                            // Possible status bytes returned:
                            //   I = Idle (no transaction active).
                            //   T = In transaction, ready for more.
                            //   E = Error in transaction, queries will fail until transaction aborted.
                            // Just eat the status byte, we have no use for it at this time.
                            PGUtil.ReadInt32(stream);
                            stream.ReadByte();

                            ChangeState(context, NpgsqlReadyState.Instance);

                            if (errors.Count != 0)
                            {
                                throw new NpgsqlException(errors);
                            }

                            yield break;

                        case BackEndMessageCode.BackendKeyData:

                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BackendKeyData");
                            // BackendKeyData message.
                            NpgsqlBackEndKeyData backend_keydata = new NpgsqlBackEndKeyData(context.BackendProtocolVersion, stream);
                            context.BackEndKeyData = backend_keydata;


                            // Wait for ReadForQuery message
                            break;

                        case BackEndMessageCode.NoticeResponse:
                            // Notices and errors are identical except that we
                            // just throw notices away completely ignored.
                            context.FireNotice(new NpgsqlError(context.BackendProtocolVersion, stream));
                            break;

                        case BackEndMessageCode.CompletedResponse:
                            PGUtil.ReadInt32(stream);
                            yield return new CompletedResponse(stream);
                            break;
                        case BackEndMessageCode.ParseComplete:
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParseComplete");
                            // Just read up the message length.
                            PGUtil.ReadInt32(stream);
                            yield break;
                        case BackEndMessageCode.BindComplete:
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BindComplete");
                            // Just read up the message length.
                            PGUtil.ReadInt32(stream);
                            yield break;
                        case BackEndMessageCode.EmptyQueryResponse:
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "EmptyQueryResponse");
                            PGUtil.ReadInt32(stream);
                            break;
                        case BackEndMessageCode.NotificationResponse:
                            // Eat the length
                            PGUtil.ReadInt32(stream);
                            context.FireNotification(new NpgsqlNotificationEventArgs(stream, true));
                            if (context.IsNotificationThreadRunning)
                            {
                                yield break;
                            }
                            break;
                        case BackEndMessageCode.ParameterStatus:
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParameterStatus");
                            NpgsqlParameterStatus parameterStatus = new NpgsqlParameterStatus(stream);

                            NpgsqlEventLog.LogMsg(resman, "Log_ParameterStatus", LogLevel.Debug, parameterStatus.Parameter,
                                                  parameterStatus.ParameterValue);

                            context.AddParameterStatus(parameterStatus);

                            if (parameterStatus.Parameter == "server_version")
                            {
                                // Deal with this here so that if there are 
                                // changes in a future backend version, we can handle it here in the
                                // protocol handler and leave everybody else put of it.
                                string versionString = parameterStatus.ParameterValue.Trim();
                                for (int idx = 0; idx != versionString.Length; ++idx)
                                {
                                    char c = parameterStatus.ParameterValue[idx];
                                    if (!char.IsDigit(c) && c != '.')
                                    {
                                        versionString = versionString.Substring(0, idx);
                                        break;
                                    }
                                }
                                context.ServerVersion = new Version(versionString);
                            }
                            break;
                        case BackEndMessageCode.NoData:
                            // This nodata message may be generated by prepare commands issued with queries which doesn't return rows
                            // for example insert, update or delete.
                            // Just eat the message.
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParameterStatus");
                            PGUtil.ReadInt32(stream);
                            break;

                        case BackEndMessageCode.CopyInResponse:
                            // Enter COPY sub protocol and start pushing data to server
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CopyInResponse");
                            ChangeState(context, NpgsqlCopyInState.Instance);
                            PGUtil.ReadInt32(stream); // length redundant
                            context.CurrentState.StartCopy(context, ReadCopyHeader(stream));
                            yield break;
                                // Either StartCopy called us again to finish the operation or control should be passed for user to feed copy data

                        case BackEndMessageCode.CopyOutResponse:
                            // Enter COPY sub protocol and start pulling data from server
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CopyOutResponse");
                            ChangeState(context, NpgsqlCopyOutState.Instance);
                            PGUtil.ReadInt32(stream); // length redundant
                            context.CurrentState.StartCopy(context, ReadCopyHeader(stream));
                            yield break;
                                // Either StartCopy called us again to finish the operation or control should be passed for user to feed copy data

                        case BackEndMessageCode.CopyData:
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CopyData");
                            Int32 len = PGUtil.ReadInt32(stream) - 4;
                            byte[] buf = new byte[len];
                            stream.Read(buf, 0, len);
                            context.Mediator.ReceivedCopyData = buf;
                            yield break; // read data from server one chunk at a time while staying in copy operation mode

                        case BackEndMessageCode.CopyDone:
                            NpgsqlEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CopyDone");
                            PGUtil.ReadInt32(stream); // CopyDone can not have content so this is always 4
                            // This will be followed by normal CommandComplete + ReadyForQuery so no op needed
                            break;

                        case BackEndMessageCode.IO_ERROR:
                            // Connection broken. Mono returns -1 instead of throwing an exception as ms.net does.
                            throw new IOException();

                        default:
                            // This could mean a number of things
                            //   We've gotten out of sync with the backend?
                            //   We need to implement this type?
                            //   Backend has gone insane?
                            // FIXME
                            // what exception should we really throw here?
                            throw new NotSupportedException(String.Format("Backend sent unrecognized response type: {0}", (Char) message));
                    }
                }
            }
        }


        private static NpgsqlCopyFormat ReadCopyHeader(Stream stream)
        {
            byte copyFormat = (byte) stream.ReadByte();
            Int16 numCopyFields = PGUtil.ReadInt16(stream);
            Int16[] copyFieldFormats = new Int16[numCopyFields];
            for (Int16 i = 0; i < numCopyFields; i++)
            {
                copyFieldFormats[i] = PGUtil.ReadInt16(stream);
            }
            return new NpgsqlCopyFormat(copyFormat, copyFieldFormats);
        }
    }

    /// <summary>
    /// Represents a completed response message.
    /// </summary>
    internal class CompletedResponse : IServerResponseObject
    {
        private readonly int? _rowsAffected;
        private readonly long? _lastInsertedOID;

        public CompletedResponse(Stream stream)
        {
            string[] tokens = PGUtil.ReadString(stream).Split();
            if (tokens.Length > 1)
            {
                int rowsAffected;
                if (int.TryParse(tokens[tokens.Length - 1], out rowsAffected))
                    _rowsAffected = rowsAffected;
                else
                    _rowsAffected = null;
                
            }
            _lastInsertedOID = (tokens.Length > 2 && tokens[0].Trim().ToUpperInvariant() == "INSERT")
                                   ? long.Parse(tokens[1])
                                   : (long?) null;
        }

        public long? LastInsertedOID
        {
            get { return _lastInsertedOID; }
        }

        public int? RowsAffected
        {
            get { return _rowsAffected; }
        }
    }

    /// <summary>
    /// For classes representing messages sent from the client to the server.
    /// </summary>
    internal abstract class ClientMessage
    {
        protected static readonly Encoding UTF8Encoding = Encoding.UTF8;
        public abstract void WriteToStream(Stream outputStream);
    }

    /// <summary>
    /// Marker interface which identifies a class which represents part of
    /// a response from the server.
    internal interface IServerResponseObject
    {
    }

    /// <summary>
    /// Marker interface which identifies a class which may take possession of a stream for the duration of
    /// it's lifetime (possibly temporarily giving that possession to another class for part of that time.
    /// 
    /// It inherits from IDisposable, since any such class must make sure it leaves the stream in a valid state.
    /// 
    /// The most important such class is that compiler-generated from ProcessBackendResponsesEnum. Of course
    /// we can't make that inherit from this interface, alas.
    /// </summary>
    internal interface IStreamOwner : IServerResponseObject, IDisposable
    {
    }
}