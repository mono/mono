// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007, 2008 LShift Ltd., Cohesive Financial
//   Technologies LLC., and Rabbit Technologies Ltd.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//   The contents of this file are subject to the Mozilla Public License
//   Version 1.1 (the "License"); you may not use this file except in
//   compliance with the License. You may obtain a copy of the License at
//   http://www.rabbitmq.com/mpl.html
//
//   Software distributed under the License is distributed on an "AS IS"
//   basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//   License for the specific language governing rights and limitations
//   under the License.
//
//   The Original Code is The RabbitMQ .NET Client.
//
//   The Initial Developers of the Original Code are LShift Ltd.,
//   Cohesive Financial Technologies LLC., and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd., Cohesive Financial Technologies
//   LLC., and Rabbit Technologies Ltd. are Copyright (C) 2007, 2008
//   LShift Ltd., Cohesive Financial Technologies LLC., and Rabbit
//   Technologies Ltd.;
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Util;

// We use spec version 0-9 for common constants such as frame types,
// error codes, and the frame end byte, since they don't vary *within
// the versions we support*. Obviously we may need to revisit this if
// that ever changes.
using CommonFraming = RabbitMQ.Client.Framing.v0_9;

namespace RabbitMQ.Client.Impl
{
    public abstract class ConnectionBase : IConnection
    {
        ///<summary>Heartbeat frame for transmission. Reusable across connections.</summary>
        public readonly Frame heartbeatFrame = new Frame(CommonFraming.Constants.FrameHeartbeat,
                                                         0,
                                                         new byte[0]);

        ///<summary>Timeout used while waiting for AMQP handshaking to
        ///complete (milliseconds)</summary>
        public static int HandshakeTimeout = 10000;

        ///<summary>Timeout used while waiting for a
        ///connection.close-ok reply to a connection close request
        ///(milliseconds)</summary>
        public static int ConnectionCloseTimeout = 10000;

        public ConnectionParameters m_parameters;
        public IFrameHandler m_frameHandler;
        public uint m_frameMax = 0;
        public ushort m_heartbeat = 0;
        public AmqpTcpEndpoint[] m_knownHosts = null;

        public MainSession m_session0;
        public ModelBase m_model0;

        public readonly SessionManager m_sessionManager;

        public volatile bool m_running = true;

        public readonly object m_eventLock = new object();
        public ConnectionShutdownEventHandler m_connectionShutdown;
        
        public volatile ShutdownEventArgs m_closeReason = null;
        public CallbackExceptionEventHandler m_callbackException;
        
        public ManualResetEvent m_appContinuation = new ManualResetEvent(false);
        public AutoResetEvent m_heartbeatRead = new AutoResetEvent(false);
        public AutoResetEvent m_heartbeatWrite = new AutoResetEvent(false);
        public volatile bool closed = false;

        public Guid id = Guid.NewGuid();

        public int m_missedHeartbeats = 0;
        
        public IList shutdownReport = ArrayList.Synchronized(new ArrayList());

        public ConnectionBase(ConnectionParameters parameters,
                              bool insist,
                              IFrameHandler frameHandler)
        {
            m_parameters = parameters;
            m_frameHandler = frameHandler;

            m_sessionManager = new SessionManager(this);
            m_session0 = new MainSession(this);
            m_session0.Handler = NotifyReceivedClose;
            m_model0 = (ModelBase)Protocol.CreateModel(m_session0);

            StartMainLoop();
            Open(insist);
            StartHeartbeatLoops();
        }

        public event ConnectionShutdownEventHandler ConnectionShutdown
        {
            add
            {
                bool ok = false;
                lock (m_eventLock)
                {
                    if (m_closeReason == null)
                    {
                        m_connectionShutdown += value;
                        ok = true;
                    }
                }
                if (!ok)
                {
                    value(this, m_closeReason);
                }
            }
            remove
            {
                lock (m_eventLock)
                {
                    m_connectionShutdown -= value;
                }
            }
        }

        public event CallbackExceptionEventHandler CallbackException
        {
            add
            {
                lock (m_eventLock)
                {
                    m_callbackException += value;
                }
            }
            remove
            {
                lock (m_eventLock)
                {
                    m_callbackException -= value;
                }
            }
        }

        public AmqpTcpEndpoint Endpoint
        {
            get
            {
                return m_frameHandler.Endpoint;
            }
        }

        ///<summary>Explicit implementation of IConnection.Protocol.</summary>
        IProtocol IConnection.Protocol
        {
            get
            {
                return Endpoint.Protocol;
            }
        }

        ///<summary>Another overload of a Protocol property, useful
        ///for exposing a tighter type.</summary>
        public AbstractProtocolBase Protocol
        {
            get
            {
                return (AbstractProtocolBase)Endpoint.Protocol;
            }
        }

        public void WriteFrame(Frame f)
        {
            m_frameHandler.WriteFrame(f);
            m_heartbeatWrite.Set();
        }

        public ConnectionParameters Parameters
        {
            get
            {
                return m_parameters;
            }
        }

        public ushort ChannelMax
        {
            get
            {
                return m_sessionManager.ChannelMax;
            }
            set
            {
                m_sessionManager.ChannelMax = value;
            }
        }

        public uint FrameMax
        {
            get
            {
                return m_frameMax;
            }
            set
            {
                m_frameMax = value;
            }
        }

        public ushort Heartbeat
        {
            get
            {
                return m_heartbeat;
            }
            set
            {
                m_heartbeat = value;
                // Socket read timeout is twice the hearbeat
                // because when we hit the timeout socket is
                // in unusable state
                m_frameHandler.Timeout = value * 2 * 1000;
            }
        }

        public AmqpTcpEndpoint[] KnownHosts
        {
            get { return m_knownHosts; }
            set { m_knownHosts = value; }
        }

        public ShutdownEventArgs CloseReason
        {
            get
            {
                return m_closeReason;
            }
        }

        public bool IsOpen
        {
            get
            {
                return CloseReason == null;
            }
        }

        public bool AutoClose
        {
            get
            {
                return m_sessionManager.AutoClose;
            }
            set
            {
                m_sessionManager.AutoClose = value;
            }
        }

        public IModel CreateModel()
        {
            ISession session = CreateSession();
            IFullModel model = (IFullModel)Protocol.CreateModel(session);
            model._Private_ChannelOpen("");
            return model;
        }

        public ISession CreateSession()
        {
            return m_sessionManager.Create();
        }
        
        public ISession CreateSession(int channelNumber)
        {
            return m_sessionManager.Create(channelNumber);
        }

        public bool SetCloseReason(ShutdownEventArgs reason)
        {
            lock (m_eventLock)
            {
                if (m_closeReason == null)
                {
                    m_closeReason = reason;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        public IList ShutdownReport
        {
            get
            {
                return shutdownReport;
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        ///<summary>API-side invocation of connection close.</summary>
        public void Close()
        {
            Close(200, "Goodbye", Timeout.Infinite);
        }
        
        ///<summary>API-side invocation of connection close with timeout.</summary>
        public void Close(int timeout)
        {
            Close(200, "Goodbye", timeout);
        }

        public void Close(ShutdownEventArgs reason)
        {
            Close(reason, false, Timeout.Infinite);
        }
        
        public void Close(ushort reasonCode, string reasonText, int timeout)
        {
            Close(new ShutdownEventArgs(ShutdownInitiator.Application, reasonCode, reasonText), false, timeout);
        }
        
        
        ///<summary>API-side invocation of connection abort.</summary>
        public void Abort()
        {
            Abort(Timeout.Infinite);
        }
        
        ///<summary>API-side invocation of connection abort with timeout.</summary>
        public void Abort(int timeout)
        {
            Abort(200, "Connection close forced", timeout);
        }
        
        public void Abort(ushort reasonCode, string reasonText, int timeout)
        {
            Abort(reasonCode, reasonText, ShutdownInitiator.Application, timeout);
        }
        
        public void Abort(ushort reasonCode, string reasonText,
                          ShutdownInitiator initiator, int timeout)
        {
            Close( new ShutdownEventArgs(initiator, reasonCode, reasonText),
                  true, timeout);
        }
        
        ///<summary>Try to close connection in a graceful way</summary>
        ///<remarks>
        ///<para>
        ///Shutdown reason contains code and text assigned when closing the connection,
        ///as well as the information about what initiated the close
        ///</para>
        ///<para>
        ///Abort flag, if true, signals to close the ongoing connection immediately 
        ///and do not report any errors if it was already closed.
        ///</para>
        ///<para>
        ///Timeout determines how much time internal close operations should be given
        ///to complete. Negative or Timeout.Infinite value mean infinity.
        ///</para>
        ///</remarks>
        public void Close(ShutdownEventArgs reason, bool abort, int timeout)
        {
            if (!SetCloseReason(reason))
                if (abort)
                {
                    if (!m_appContinuation.WaitOne(BlockingCell.validatedTimeout(timeout), true))
                        m_frameHandler.Close();
                    return;
                } else {
                    throw new AlreadyClosedException(m_closeReason);
                }
                                           
            OnShutdown();
            m_session0.SetSessionClosing(false);

            try
            {
                // Try to send connection close
                // Wait for CloseOk in the MainLoop
                m_session0.Transmit(ConnectionCloseWrapper(reason.ReplyCode,
                                                          reason.ReplyText));
            }
            catch (IOException ioe) {
                if (m_model0.CloseReason == null)
                {
                    if (!abort)
                        throw ioe;
                    else
                        LogCloseError("Couldn't close connection cleanly. " 
                                      + "Socket closed unexpectedly", ioe);
                }
            }
            finally
            {
                TerminateMainloop();
            }
            if (!m_appContinuation.WaitOne(BlockingCell.validatedTimeout(timeout),true))
                m_frameHandler.Close();
        }

        public delegate void ConnectionCloseDelegate(ushort replyCode,
                                                     string replyText,
                                                     ushort classId,
                                                     ushort methodId);

        public void InternalClose(ShutdownEventArgs reason)
        {
            if (!SetCloseReason(reason))
            {
                if (closed)
                    throw new AlreadyClosedException(m_closeReason);
                // We are quiescing, but still allow for server-close
            }
            
            OnShutdown();
            m_session0.SetSessionClosing(true);
            TerminateMainloop();
        }

        ///<remarks>
        /// May be called more than once. Should therefore be idempotent.
        ///</remarks>
        public void TerminateMainloop()
        {
            m_running = false;
        }

        public void StartMainLoop()
        {
            Thread mainloopThread = new Thread(new ThreadStart(MainLoop));
            mainloopThread.Name = "AMQP Connection " + Endpoint.ToString();
            mainloopThread.Start();
        }
        
        public void StartHeartbeatLoops()
        {
            if (Heartbeat != 0) {
                StartHeartbeatLoop(new ThreadStart(HeartbeatReadLoop), "Inbound");
                StartHeartbeatLoop(new ThreadStart(HeartbeatWriteLoop), "Outbound");
            }
        }
        
        public void StartHeartbeatLoop(ThreadStart loop, string name)
        {
            Thread heartbeatLoop = new Thread(loop);
            heartbeatLoop.Name = "AMQP Heartbeat " + name + " for Connection " + Endpoint.ToString();
            heartbeatLoop.Start();
        }
        
        public void HeartbeatWriteLoop()
        {
            try
            {
                while (!closed)
                {
                    if (!m_heartbeatWrite.WaitOne(Heartbeat * 1000, false))
                    {
                        WriteFrame(heartbeatFrame);
                    }
                }
            } catch (Exception e) {
                HandleMainLoopException(new ShutdownEventArgs(
                                                ShutdownInitiator.Library,
                                                0,
                                                "End of stream",
                                                e));
            }
            
            TerminateMainloop();
            FinishClose();
        }
        
        public void HeartbeatReadLoop()
        {
            while (!closed)
            {
                if (!m_heartbeatRead.WaitOne(Heartbeat * 1000, false))
                    m_missedHeartbeats++;
                else
                    m_missedHeartbeats = 0;
                    
                // Has to miss two full heartbeats to force socket close
                if (m_missedHeartbeats > 1)
                {
                    EndOfStreamException eose = new EndOfStreamException(
                                         "Heartbeat missing with heartbeat == " +
                                         m_heartbeat + " seconds");
                    HandleMainLoopException(new ShutdownEventArgs(
                                                          ShutdownInitiator.Library,
                                                          0,
                                                          "End of stream",
                                                          eose));
                    break;
                }
            }
            
            TerminateMainloop();
            FinishClose();
        }
        
        public void HandleHeartbeatFrame()
        {
            if (m_heartbeat == 0) {
                // Heartbeating not enabled for this connection.
                return;
            }
            
            m_heartbeatRead.Set();
        }

        public void MainLoop()
        {
            bool shutdownCleanly = false;
            try
            {
                while (m_running)
                {
                    try {
                        MainLoopIteration();
                    } catch (SoftProtocolException spe) {
                        QuiesceChannel(spe);
                    }
                }
                shutdownCleanly = true;
            }
            catch (EndOfStreamException eose)
            {
                // Possible heartbeat exception
                HandleMainLoopException(new ShutdownEventArgs(
                                                          ShutdownInitiator.Library,
                                                          0,
                                                          "End of stream",
                                                          eose));
            }
            catch (HardProtocolException hpe)
            {
                shutdownCleanly = HardProtocolExceptionHandler(hpe);
            }
            catch (Exception ex)
            {
                HandleMainLoopException(new ShutdownEventArgs(ShutdownInitiator.Library,
                                                          CommonFraming.Constants.InternalError,
                                                          "Unexpected Exception",
                                                          ex));
            }
            
            // If allowed for clean shutdown
            // Run limited version of the main loop
            if (shutdownCleanly)
            {
                ClosingLoop();
            }
            
            FinishClose();

            m_appContinuation.Set();
        }
        
        public void MainLoopIteration()
        {
            Frame frame = m_frameHandler.ReadFrame();

            // We have received an actual frame.
            if (frame.Type == CommonFraming.Constants.FrameHeartbeat) {
                // Ignore it: we've already just reset the heartbeat
                // counter.
                HandleHeartbeatFrame();
                return;
            }

            if (frame.Channel == 0) {
                // In theory, we could get non-connection.close-ok
                // frames here while we're quiescing (m_closeReason !=
                // null). In practice, there's a limited number of
                // things the server can ask of us on channel 0 -
                // essentially, just connection.close. That, combined
                // with the restrictions on pipelining, mean that
                // we're OK here to handle channel 0 traffic in a
                // quiescing situation, even though technically we
                // should be ignoring everything except
                // connection.close-ok.
                m_session0.HandleFrame(frame);
            } else {
                // If we're still m_running, but have a m_closeReason,
                // then we must be quiescing, which means any inbound
                // frames for non-zero channels (and any inbound
                // commands on channel zero that aren't
                // Connection.CloseOk) must be discarded.
                if (m_closeReason == null)
                {
                    // No close reason, not quiescing the
                    // connection. Handle the frame. (Of course, the
                    // Session itself may be quiescing this particular
                    // channel, but that's none of our concern.)
                    ISession session = m_sessionManager.Lookup(frame.Channel);
                    if (session == null) {
                        throw new ChannelErrorException(frame.Channel);
                    } else {
                        session.HandleFrame(frame);
                    }
                }
            }
        }
        
        // Only call at the end of the Mainloop or HeartbeatLoop
        public void FinishClose()
        {
            // Notify hearbeat loops that they can leave
            closed = true;
            m_heartbeatRead.Set();
            m_heartbeatWrite.Set();
        
            m_frameHandler.Close();                
            m_model0.SetCloseReason(m_closeReason);
            m_model0.FinishClose();
        }
            
        public bool HardProtocolExceptionHandler(HardProtocolException hpe)
        {
            if (SetCloseReason(hpe.ShutdownReason))
            {
                OnShutdown();
                m_session0.SetSessionClosing(false);
                try
                {
                    m_session0.Transmit(ConnectionCloseWrapper(
                                           hpe.ShutdownReason.ReplyCode,
                                           hpe.ShutdownReason.ReplyText));
                    return true;
                } catch (IOException ioe) {
                    LogCloseError("Broker closed socket unexpectedly", ioe);
                }

            } else
                LogCloseError("Hard Protocol Exception occured "
                              + "while closing the connection", hpe);
                
            return false;            
        }
        
        ///<remarks>
        /// Loop only used while quiescing. Use only to cleanly close connection
        ///</remarks>
        public void ClosingLoop()
        {
            m_frameHandler.Timeout = ConnectionCloseTimeout;
            DateTime startTimeout = DateTime.Now;
            try
            {
                // Wait for response/socket closure or timeout
                while (!closed)
                {
                    if ((DateTime.Now - startTimeout).TotalMilliseconds >= ConnectionCloseTimeout)
                    {
                        LogCloseError("Timeout, when waiting for server's response on close", null);
                        break;
                    }
                    MainLoopIteration();
                }
            }
            catch (EndOfStreamException eose)
            {
                if (m_model0.CloseReason == null)
                    LogCloseError("Connection didn't close cleanly. "
                                  + "Socket closed unexpectedly", eose);
            }
            catch (IOException ioe)
            {
                LogCloseError("Connection didn't close cleanly. "
                              + "Socket closed unexpectedly", ioe);
            }
            catch (Exception e)
            {
                LogCloseError("Unexpected exception while closing: ", e);
            }
        }
        
        public void NotifyReceivedClose()
        {
            closed = true;
            m_frameHandler.Close();
        }
        
        ///<summary>
        /// Sets the channel named in the SoftProtocolException into
        /// "quiescing mode", where we issue a channel.close and
        /// ignore everything up to the channel.close-ok reply that
        /// should eventually arrive.
        ///</summary>
        ///<remarks>
        ///<para>
        /// Since a well-behaved peer will not wait indefinitely before
        /// issuing the close-ok, we don't bother with a timeout here;
        /// compare this to the case of a connection.close-ok, where a
        /// timeout is necessary.
        ///</para>
        ///<para>
        /// We need to send the close method and politely wait for a
        /// reply before marking the channel as available for reuse.
        ///</para>
        ///<para>
        /// As soon as SoftProtocolException is detected, we should stop
        /// servicing ordinary application work, and should concentrate
        /// on bringing down the channel as quickly and gracefully as
        /// possible. The way this is done, as per the close-protocol,
        /// is to signal closure up the stack *before* sending the
        /// channel.close, by invoking ISession.Close. Once the upper
        /// layers have been signalled, we are free to do what we need
        /// to do to clean up and shut down the channel.
        ///</para>
        ///</remarks>
        public void QuiesceChannel(SoftProtocolException pe) {
            // First, construct the close request and QuiescingSession
            // that we'll use during the quiesce process.

            Command request;
            int replyClassId;
            int replyMethodId;
            Protocol.CreateChannelClose(pe.ReplyCode,
                                        pe.Message,
                                        out request,
                                        out replyClassId,
                                        out replyMethodId);

            ISession newSession = new QuiescingSession(this,
                                                       pe.Channel,
                                                       pe.ShutdownReason,
                                                       replyClassId,
                                                       replyMethodId);

            // Here we detach the session from the connection. It's
            // still alive: it just won't receive any further frames
            // from the mainloop (once we return to the mainloop, of
            // course). Instead, those frames will be directed at the
            // new QuiescingSession.
            ISession oldSession = m_sessionManager.Swap(pe.Channel, newSession);

            // Now we have all the information we need, and the event
            // flow of the *lower* layers is set up properly for
            // shutdown. Signal channel closure *up* the stack, toward
            // the model and application.
            oldSession.Close(pe.ShutdownReason);

            // The upper layers have been signalled. Now we can tell
            // our peer. The peer will respond through the lower
            // layers - specifically, through the QuiescingSession we
            // installed above.
            newSession.Transmit(request);
        }

        public void HandleMainLoopException(ShutdownEventArgs reason) {
            if (!SetCloseReason(reason))
            {
                LogCloseError("Unexpected Main Loop Exception while closing: "
                               + reason.ToString(), null);
                return;
            }
            
            OnShutdown();
            LogCloseError("Unexpected connection closure: " + reason.ToString(), null);
        }
        
        public void LogCloseError(String error, Exception ex)
        {
            shutdownReport.Add(new ShutdownReportEntry(error, ex));
        }
        
        public void PrettyPrintShutdownReport()
        {
            if (ShutdownReport.Count == 0)
            {
                Console.Error.WriteLine("No errors reported when closing connection {0}", this);
            } else {
                Console.Error.WriteLine("Log of errors while closing connection {0}:", this);
                foreach(ShutdownReportEntry entry in ShutdownReport)
                {
                    Console.Error.WriteLine(entry.ToString());
                }
            }
        }

        ///<summary>Broadcasts notification of the final shutdown of the connection.</summary>
        public void OnShutdown()
        {
            ConnectionShutdownEventHandler handler;
            ShutdownEventArgs reason;
            lock (m_eventLock)
            {
                handler = m_connectionShutdown;
                reason = m_closeReason;
                m_connectionShutdown = null;
            }
            if (handler != null)
            {
                foreach (ConnectionShutdownEventHandler h in handler.GetInvocationList()) {
                    try {
                        h(this, reason);
                    } catch (Exception e) {
                        CallbackExceptionEventArgs args = new CallbackExceptionEventArgs(e);
                        args.Detail["context"] = "OnShutdown";
                        OnCallbackException(args);
                    }
                }
            }
        }

        public void OnCallbackException(CallbackExceptionEventArgs args)
        {
            CallbackExceptionEventHandler handler;
            lock (m_eventLock) {
                handler = m_callbackException;
            }
            if (handler != null) {
                foreach (CallbackExceptionEventHandler h in handler.GetInvocationList()) {
                    try {
                        h(this, args);
                    } catch {
                        // Exception in
                        // Callback-exception-handler. That was the
                        // app's last chance. Swallow the exception.
                        // FIXME: proper logging
                    }
                }
            }
        }

        public IDictionary BuildClientPropertiesTable()
        {
            string version = this.GetType().Assembly.GetName().Version.ToString();
            //TODO: Get the rest of this data from the Assembly Attributes
            Hashtable table = new Hashtable();
            table["product"] = Encoding.UTF8.GetBytes("RabbitMQ");
            table["version"] = Encoding.UTF8.GetBytes(version);
            table["platform"] = Encoding.UTF8.GetBytes(".NET");
            table["copyright"] = Encoding.UTF8.GetBytes("Copyright (C) 2007-2008 LShift Ltd., " +
                                                        "Cohesive Financial Technologies LLC., " +
                                                        "and Rabbit Technologies Ltd.");
            table["information"] = Encoding.UTF8.GetBytes("Licensed under the MPL.  " +
                                                          "See http://www.rabbitmq.com/");
            return table;
        }
        
        public Command ConnectionCloseWrapper(ushort reasonCode, string reasonText)
        {
            Command request;
            int replyClassId, replyMethodId;
            Protocol.CreateConnectionClose(reasonCode,
                                           reasonText,
                                           out request,
                                           out replyClassId,
                                           out replyMethodId);
            return request;
        } 

        private static uint NegotiatedMaxValue(uint clientValue, uint serverValue)
        {
            return (clientValue == 0 || serverValue == 0) ?
                Math.Max(clientValue, serverValue) :
                Math.Min(clientValue, serverValue);
        }

        public void Open(bool insist)
        {
            BlockingCell connectionStartCell = new BlockingCell();
            m_model0.m_connectionStartCell = connectionStartCell;
            m_frameHandler.Timeout = HandshakeTimeout;
            m_frameHandler.SendHeader();

            ConnectionStartDetails connectionStart = (ConnectionStartDetails)
                connectionStartCell.Value;

            AmqpVersion serverVersion = new AmqpVersion(connectionStart.versionMajor,
                                                        connectionStart.versionMinor);
            if (!serverVersion.Equals(Protocol.Version))
            {
                TerminateMainloop();
                FinishClose();
                throw new ProtocolVersionMismatchException(Protocol.MajorVersion,
                                                           Protocol.MinorVersion,
                                                           serverVersion.Major,
                                                           serverVersion.Minor);
            }

            // FIXME: check that PLAIN is supported.
            // FIXME: parse out locales properly!
            ConnectionTuneDetails connectionTune =
                m_model0.ConnectionStartOk(BuildClientPropertiesTable(),
                                           "PLAIN",
                                           Encoding.UTF8.GetBytes("\0" + m_parameters.UserName +
                                                                  "\0" + m_parameters.Password),
                                           "en_US");

            ushort channelMax = (ushort) NegotiatedMaxValue(m_parameters.RequestedChannelMax,
                                                            connectionTune.channelMax);
            ChannelMax = channelMax;

            uint frameMax = NegotiatedMaxValue(m_parameters.RequestedFrameMax,
                                               connectionTune.frameMax);
            FrameMax = frameMax;

            ushort heartbeat = (ushort) NegotiatedMaxValue(m_parameters.RequestedHeartbeat,
                                                           connectionTune.heartbeat);
            Heartbeat = heartbeat;

            m_model0.ConnectionTuneOk(channelMax,
                                      frameMax,
                                      heartbeat);

            string knownHosts = m_model0.ConnectionOpen(m_parameters.VirtualHost,
                                                        "", // FIXME: make configurable?
                                                        insist);
            KnownHosts = AmqpTcpEndpoint.ParseMultiple(Protocol, knownHosts);
        }

        public override string ToString()
        {
            return string.Format("Connection({0},{1})", id, Endpoint);
        }
    }
}
