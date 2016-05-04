namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.IO;
    using System.Net.Mime;

    
    static class CheckCommand
    {
        static AsyncCallback onReadLine = new AsyncCallback(OnReadLine);
        static AsyncCallback onWrite = new AsyncCallback(OnWrite);
        
        internal static IAsyncResult BeginSend(SmtpConnection conn, AsyncCallback callback, object state)
        {
            MultiAsyncResult multiResult = new MultiAsyncResult(conn, callback, state);
            multiResult.Enter();
            IAsyncResult writeResult = conn.BeginFlush(onWrite, multiResult);
            if (writeResult.CompletedSynchronously)
            {
                conn.EndFlush(writeResult);
                multiResult.Leave();
            }
            SmtpReplyReader reader = conn.Reader.GetNextReplyReader();
            multiResult.Enter();

            //this actually does a read on the stream.
            IAsyncResult result = reader.BeginReadLine(onReadLine, multiResult);
            if (result.CompletedSynchronously){
                LineInfo info = reader.EndReadLine(result);
                if (!(multiResult.Result is Exception))
                    multiResult.Result = info;
                multiResult.Leave();
            }
            multiResult.CompleteSequence();
            return multiResult;
        }


        internal static object EndSend(IAsyncResult result, out string response)
        {
            object commandResult = MultiAsyncResult.End(result);
            if (commandResult is Exception)
                throw (Exception)commandResult;

            LineInfo info = (LineInfo)commandResult;
            response = info.Line;
            return info.StatusCode;
        }


        static void OnReadLine(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult multiResult = (MultiAsyncResult)result.AsyncState;
                try
                {
                    SmtpConnection conn = (SmtpConnection)multiResult.Context;
                    LineInfo info = conn.Reader.CurrentReader.EndReadLine(result);
                    if (!(multiResult.Result is Exception))
                        multiResult.Result = info;
                    multiResult.Leave();
                }
                catch (Exception e)
                {
                    multiResult.Leave(e);
                }
            }
        }


        static void OnWrite(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult multiResult = (MultiAsyncResult)result.AsyncState;
                try
                {
                    SmtpConnection conn = (SmtpConnection)multiResult.Context;
                    conn.EndFlush(result);
                    multiResult.Leave();
                }
                catch (Exception e)
                {
                    multiResult.Leave(e);
                }
            }
        }
        
        internal static SmtpStatusCode Send(SmtpConnection conn, out string response)
        {
            conn.Flush();
            SmtpReplyReader reader = conn.Reader.GetNextReplyReader();
            LineInfo info = reader.ReadLine();
            response = info.Line;
            reader.Close();
            return info.StatusCode;
        }
    }

    static class ReadLinesCommand
    {
        static AsyncCallback onReadLines = new AsyncCallback(OnReadLines);
        static AsyncCallback onWrite = new AsyncCallback(OnWrite);

        internal static IAsyncResult BeginSend(SmtpConnection conn, AsyncCallback callback, object state)
        {
            MultiAsyncResult multiResult = new MultiAsyncResult(conn, callback, state);
            multiResult.Enter();
            IAsyncResult writeResult = conn.BeginFlush(onWrite, multiResult);
            if (writeResult.CompletedSynchronously)
            {
                conn.EndFlush(writeResult);
                multiResult.Leave();
            }
            SmtpReplyReader reader = conn.Reader.GetNextReplyReader();
            multiResult.Enter();
            IAsyncResult readLinesResult = reader.BeginReadLines(onReadLines, multiResult);
            if (readLinesResult.CompletedSynchronously)
            {
                LineInfo[] lines = conn.Reader.CurrentReader.EndReadLines(readLinesResult);
                if (!(multiResult.Result is Exception))
                    multiResult.Result = lines;
                multiResult.Leave();
            }
            multiResult.CompleteSequence();
            return multiResult;
        }

        internal static LineInfo[] EndSend(IAsyncResult result)
        {
            object commandResult = MultiAsyncResult.End(result);
            if (commandResult is Exception)
                throw (Exception)commandResult;
            return (LineInfo[])commandResult;
        }

        static void OnReadLines(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult multiResult = (MultiAsyncResult)result.AsyncState;
                try
                {
                    SmtpConnection conn = (SmtpConnection)multiResult.Context;
                    LineInfo[] lines = conn.Reader.CurrentReader.EndReadLines(result);
                    if (!(multiResult.Result is Exception))
                        multiResult.Result = lines;
                    multiResult.Leave();
                }
                catch (Exception e)
                {
                    multiResult.Leave(e);
                }
            }
        }

        static void OnWrite(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult multiResult = (MultiAsyncResult)result.AsyncState;
                try
                {
                    SmtpConnection conn = (SmtpConnection)multiResult.Context;
                    conn.EndFlush(result);
                    multiResult.Leave();
                }
                catch (Exception e)
                {
                    multiResult.Leave(e);
                }
            }
        }
        internal static LineInfo[] Send(SmtpConnection conn)
        {
            conn.Flush();
            return conn.Reader.GetNextReplyReader().ReadLines();
        }

    }

    static class AuthCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, string type, string message, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, type, message);
            return ReadLinesCommand.BeginSend(conn, callback, state);
        }

        internal static IAsyncResult BeginSend(SmtpConnection conn, string message, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, message);
            return ReadLinesCommand.BeginSend(conn, callback, state);
        }

        static LineInfo CheckResponse(LineInfo[] lines)
        {
            if (lines == null || lines.Length == 0)
            {
                throw new SmtpException(SR.GetString(SR.SmtpAuthResponseInvalid));
            }
            System.Diagnostics.Debug.Assert(lines.Length == 1, "Did not expect more than one line response for auth command");
            return lines[0];
        }

        internal static LineInfo EndSend(IAsyncResult result)
        {
            return CheckResponse(ReadLinesCommand.EndSend(result));
        }
        static void PrepareCommand(SmtpConnection conn, string type, string message)
        {
            conn.BufferBuilder.Append(SmtpCommands.Auth);
            conn.BufferBuilder.Append(type);
            conn.BufferBuilder.Append((byte)' ');
            conn.BufferBuilder.Append(message);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        static void PrepareCommand(SmtpConnection conn, string message)
        {
            conn.BufferBuilder.Append(message);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static LineInfo Send(SmtpConnection conn, string type, string message)
        {
            PrepareCommand(conn, type, message);
            return CheckResponse(ReadLinesCommand.Send(conn));
        }

        internal static LineInfo Send(SmtpConnection conn, string message)
        {
            PrepareCommand(conn, message);
            return CheckResponse(ReadLinesCommand.Send(conn));
        }

    }

    static class DataCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, AsyncCallback callback, object state)
        {
            PrepareCommand(conn);
            return CheckCommand.BeginSend(conn, callback, state);
        }

        static void CheckResponse(SmtpStatusCode statusCode, string serverResponse)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.StartMailInput:
                {
                    return;
                }
                case SmtpStatusCode.LocalErrorInProcessing:
                case SmtpStatusCode.TransactionFailed:
                default:
                {
                    if((int)statusCode < 400){
                        throw new SmtpException(SR.GetString(SR.net_webstatus_ServerProtocolViolation),serverResponse);
                    }

                    throw new SmtpException(statusCode,serverResponse,true);
                }
            }
        }

        internal static void EndSend(IAsyncResult result)
        {
            string response;
            SmtpStatusCode statusCode = (SmtpStatusCode)CheckCommand.EndSend(result, out response);
            CheckResponse(statusCode, response);
        }

        static void PrepareCommand(SmtpConnection conn)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString(SR.SmtpDataStreamOpen));
            }

            conn.BufferBuilder.Append(SmtpCommands.Data);
        }

        internal static void Send(SmtpConnection conn)
        {
            PrepareCommand(conn);
            string response;
            SmtpStatusCode statusCode = CheckCommand.Send(conn, out response);
            CheckResponse(statusCode, response);
        }
    }

    static class DataStopCommand
    {
        /*
        // Consider removing.
        internal static IAsyncResult BeginSend(SmtpConnection conn, AsyncCallback callback, object state)
        {
            PrepareCommand(conn);
            return CheckCommand.BeginSend(conn, callback, state);
        }
        */

        static void CheckResponse(SmtpStatusCode statusCode, string serverResponse)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.Ok:
                {
                    return;
                }
                case SmtpStatusCode.ExceededStorageAllocation:
                case SmtpStatusCode.TransactionFailed:
                case SmtpStatusCode.LocalErrorInProcessing:
                case SmtpStatusCode.InsufficientStorage:
                default:
                {
                    if((int)statusCode < 400){
                        throw new SmtpException(SR.GetString(SR.net_webstatus_ServerProtocolViolation), serverResponse);
                    }

                    throw new SmtpException(statusCode, serverResponse, true);
                }
            }
        }

        /*
        // Consider removing.
        internal static void EndSend(IAsyncResult result)
        {
            CheckResponse((SmtpStatusCode)CheckCommand.EndSend(result));
        }
        */

        static void PrepareCommand(SmtpConnection conn)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString(SR.SmtpDataStreamOpen));
            }

            conn.BufferBuilder.Append(SmtpCommands.DataStop);
        }
        internal static void Send(SmtpConnection conn)
        {
            PrepareCommand(conn);
            string response;
            SmtpStatusCode statusCode = CheckCommand.Send(conn, out response);
            CheckResponse(statusCode, response);
        }
    }

    static class EHelloCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, string domain, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, domain);
            return ReadLinesCommand.BeginSend(conn, callback, state);
        }

        static string[] CheckResponse(LineInfo[] lines)
        {
            if (lines == null || lines.Length == 0)
            {
                throw new SmtpException(SR.GetString(SR.SmtpEhloResponseInvalid));
            }
            if (lines[0].StatusCode != SmtpStatusCode.Ok)
            {
                if((int)lines[0].StatusCode < 400){
                    throw new SmtpException(SR.GetString(SR.net_webstatus_ServerProtocolViolation),lines[0].Line);
                }

                throw new SmtpException(lines[0].StatusCode, lines[0].Line, true);
            }
            string[] extensions = new string[lines.Length-1];
            for (int i = 1; i < lines.Length; i++)
            {
                extensions[i-1] = lines[i].Line;
            }
            return extensions;
        }

        internal static string[] EndSend(IAsyncResult result)
        {
            return CheckResponse(ReadLinesCommand.EndSend(result));
        }
        static void PrepareCommand(SmtpConnection conn, string domain)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString(SR.SmtpDataStreamOpen));
            }

            conn.BufferBuilder.Append(SmtpCommands.EHello);
            conn.BufferBuilder.Append(domain);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static string[] Send(SmtpConnection conn, string domain)
        {
            PrepareCommand(conn, domain);
            return CheckResponse(ReadLinesCommand.Send(conn));
        }

    }

    static class HelloCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, string domain, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, domain);
            return CheckCommand.BeginSend(conn, callback, state);
        }

        static void CheckResponse(SmtpStatusCode statusCode, string serverResponse)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.Ok:
                {
                    return;
                }
                default:
                {
                    if((int)statusCode < 400){
                        throw new SmtpException(SR.GetString(SR.net_webstatus_ServerProtocolViolation) ,serverResponse);
                    }

                    throw new SmtpException(statusCode, serverResponse, true);
                }
            }
        }

        internal static void EndSend(IAsyncResult result)
        {
            string response;
            SmtpStatusCode statusCode = (SmtpStatusCode)CheckCommand.EndSend(result, out response);
            CheckResponse(statusCode, response);
        }

        static void PrepareCommand(SmtpConnection conn, string domain)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString(SR.SmtpDataStreamOpen));
            }

            conn.BufferBuilder.Append(SmtpCommands.Hello);
            conn.BufferBuilder.Append(domain);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static void Send(SmtpConnection conn, string domain)
        {
            PrepareCommand(conn, domain);
            string response;
            SmtpStatusCode statusCode = CheckCommand.Send(conn, out response);
            CheckResponse(statusCode, response);
        }
    }

    static class StartTlsCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, AsyncCallback callback, object state)
        {
            PrepareCommand(conn);
            return CheckCommand.BeginSend(conn, callback, state);
        }

        static void CheckResponse(SmtpStatusCode statusCode, string response)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.ServiceReady:
                {
                    return;
                }

                case SmtpStatusCode.ClientNotPermitted:
                default:
                {
                    if((int)statusCode < 400){
                        throw new SmtpException(SR.GetString(SR.net_webstatus_ServerProtocolViolation),response);
                    }

                    throw new SmtpException(statusCode, response, true);
                }
            }
        }

        internal static void EndSend(IAsyncResult result)
        {
            string response;
            SmtpStatusCode statusCode = (SmtpStatusCode)CheckCommand.EndSend(result, out response);
            CheckResponse(statusCode, response);
        }

        static void PrepareCommand(SmtpConnection conn)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString(SR.SmtpDataStreamOpen));
            }

            conn.BufferBuilder.Append(SmtpCommands.StartTls);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static void Send(SmtpConnection conn)
        {
            PrepareCommand(conn);
            string response;
            SmtpStatusCode statusCode = CheckCommand.Send(conn, out response);
            CheckResponse(statusCode, response);
        }
    }

    static class MailCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, byte[] command, MailAddress from, 
            bool allowUnicode, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, command, from, allowUnicode);
            return CheckCommand.BeginSend(conn, callback, state);
        }

        static void CheckResponse(SmtpStatusCode statusCode, string response)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.Ok:
                {
                    return;
                }
                case SmtpStatusCode.ExceededStorageAllocation:
                case SmtpStatusCode.LocalErrorInProcessing:
                case SmtpStatusCode.InsufficientStorage:
                default:
                {
                    if((int)statusCode < 400){
                        throw new SmtpException(SR.GetString(SR.net_webstatus_ServerProtocolViolation),response);
                    }

                    throw new SmtpException(statusCode, response, true);
                }
            }
        }

        internal static void EndSend(IAsyncResult result)
        {
            string response;
            SmtpStatusCode statusCode = (SmtpStatusCode)CheckCommand.EndSend(result, out response);
            CheckResponse(statusCode, response);
        }

        static void PrepareCommand(SmtpConnection conn, byte[] command, MailAddress from, bool allowUnicode)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString(SR.SmtpDataStreamOpen));
            }
            conn.BufferBuilder.Append(command);
            string fromString = from.GetSmtpAddress(allowUnicode);
            conn.BufferBuilder.Append(fromString, allowUnicode);
            if (allowUnicode)
            {
                conn.BufferBuilder.Append(" BODY=8BITMIME SMTPUTF8");
            }
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static void Send(SmtpConnection conn, byte[] command, MailAddress from, bool allowUnicode)
        {
            PrepareCommand(conn, command, from, allowUnicode);
            string response;
            SmtpStatusCode statusCode = CheckCommand.Send(conn, out response);
            CheckResponse(statusCode, response);
        }
    }


    static class RecipientCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, string to, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, to);
            return CheckCommand.BeginSend(conn, callback, state);
        }


        static bool CheckResponse(SmtpStatusCode statusCode, string response)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.Ok:
                case SmtpStatusCode.UserNotLocalWillForward:
                {
                    return true;
                }
                case SmtpStatusCode.MailboxUnavailable:
                case SmtpStatusCode.UserNotLocalTryAlternatePath:
                case SmtpStatusCode.ExceededStorageAllocation:
                case SmtpStatusCode.MailboxNameNotAllowed:
                case SmtpStatusCode.MailboxBusy:
                case SmtpStatusCode.InsufficientStorage:
                {
                    return false;
                }
                default:
                {
                    if((int)statusCode < 400){
                        throw new SmtpException(SR.GetString(SR.net_webstatus_ServerProtocolViolation),response);
                    }

                    throw new SmtpException(statusCode, response, true);
                }
            }
        }
        
        internal static bool EndSend(IAsyncResult result, out string response)
        {
            SmtpStatusCode statusCode = (SmtpStatusCode)CheckCommand.EndSend(result, out response);
            return CheckResponse(statusCode, response);
        }
        
        
        static void PrepareCommand(SmtpConnection conn, string to)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString(SR.SmtpDataStreamOpen));
            }

            conn.BufferBuilder.Append(SmtpCommands.Recipient);
            conn.BufferBuilder.Append(to, true); // Unicode validation was done prior
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        
        internal static bool Send(SmtpConnection conn, string to, out string response)
        {
            PrepareCommand(conn, to);
            SmtpStatusCode statusCode = CheckCommand.Send(conn, out response);
            return CheckResponse(statusCode, response);
        }
    }


    internal static class SmtpCommands
    {
        internal readonly static byte[] Auth       = Encoding.ASCII.GetBytes("AUTH ");
        internal readonly static byte[] CRLF       = Encoding.ASCII.GetBytes("\r\n");
        internal readonly static byte[] Data       = Encoding.ASCII.GetBytes("DATA\r\n");
        internal readonly static byte[] DataStop   = Encoding.ASCII.GetBytes("\r\n.\r\n");
        internal readonly static byte[] EHello     = Encoding.ASCII.GetBytes("EHLO ");
        internal readonly static byte[] Expand     = Encoding.ASCII.GetBytes("EXPN ");
        internal readonly static byte[] Hello      = Encoding.ASCII.GetBytes("HELO ");
        internal readonly static byte[] Help       = Encoding.ASCII.GetBytes("HELP");
        internal readonly static byte[] Mail       = Encoding.ASCII.GetBytes("MAIL FROM:");
        internal readonly static byte[] Noop       = Encoding.ASCII.GetBytes("NOOP\r\n");
        internal readonly static byte[] Quit       = Encoding.ASCII.GetBytes("QUIT\r\n");
        internal readonly static byte[] Recipient  = Encoding.ASCII.GetBytes("RCPT TO:");
        internal readonly static byte[] Reset      = Encoding.ASCII.GetBytes("RSET\r\n");
        internal readonly static byte[] Send       = Encoding.ASCII.GetBytes("SEND FROM:");
        internal readonly static byte[] SendAndMail= Encoding.ASCII.GetBytes("SAML FROM:");
        internal readonly static byte[] SendOrMail = Encoding.ASCII.GetBytes("SOML FROM:");
        internal readonly static byte[] Turn       = Encoding.ASCII.GetBytes("TURN\r\n");
        internal readonly static byte[] Verify     = Encoding.ASCII.GetBytes("VRFY ");
        internal readonly static byte[] StartTls   = Encoding.ASCII.GetBytes("STARTTLS");
    }


    
    internal struct LineInfo
    {
        string line;
        SmtpStatusCode statusCode;

        internal LineInfo(SmtpStatusCode statusCode, string line)
        {
            this.statusCode = statusCode;
            this.line = line;
        }
        internal string Line
        {
            get
            {
                return line;
            }
        }
        internal SmtpStatusCode StatusCode
        {
            get
            {
                return statusCode;
            }
        }

    }

}
