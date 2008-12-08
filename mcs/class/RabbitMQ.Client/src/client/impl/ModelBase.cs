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
using System.Collections;
using System.Threading;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Util;

// We use spec version 0-9 for common constants such as frame types
// and the frame end byte, since they don't vary *within the versions
// we support*. Obviously we may need to revisit this if that ever
// changes.
using CommonFraming = RabbitMQ.Client.Framing.v0_9;
using System.Diagnostics;

namespace RabbitMQ.Client.Impl
{
    public abstract class ModelBase : IFullModel
    {
        private readonly object m_shutdownLock = new object();
        private ModelShutdownEventHandler m_modelShutdown;
        public ShutdownEventArgs m_closeReason = null;

        private readonly object m_eventLock = new object();
        private BasicReturnEventHandler m_basicReturn;
        private CallbackExceptionEventHandler m_callbackException;

        public event ModelShutdownEventHandler ModelShutdown
        {
            add
            {
                bool ok = false;
                lock (m_shutdownLock)
                {
                    if (m_closeReason == null)
                    {
                        m_modelShutdown += value;
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
                lock (m_shutdownLock)
                {
                    m_modelShutdown -= value;
                }
            }
        }

        public event BasicReturnEventHandler BasicReturn
        {
            add
            {
                lock (m_eventLock)
                {
                    m_basicReturn += value;
                }
            }
            remove
            {
                lock (m_eventLock)
                {
                    m_basicReturn -= value;
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

        public ISession m_session;

        public RpcContinuationQueue m_continuationQueue = new RpcContinuationQueue();

        ///<summary>Only used to kick-start a connection open
        ///sequence. See <see cref="ConnectionBase.Open"/> </summary>
        public BlockingCell m_connectionStartCell = null;

        public readonly IDictionary m_consumers = new Hashtable();

        public ModelBase(ISession session)
        {
            m_session = session;
            m_session.CommandReceived = new CommandHandler(HandleCommand);
            m_session.SessionShutdown += new SessionShutdownEventHandler(OnSessionShutdown);
        }

        public void HandleCommand(ISession session, Command cmd)
        {
            if (DispatchAsynchronous(cmd))
            {
                // Was asynchronous. Already processed. No need to process further.
            }
            else
            {
                m_continuationQueue.Next().HandleCommand(cmd);
            }
        }

        public void OnSessionShutdown(ISession session, ShutdownEventArgs reason)
        {
            SetCloseReason(reason);
            OnModelShutdown(reason);
        }

        public bool SetCloseReason(ShutdownEventArgs reason)
        {
            lock (m_shutdownLock)
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

        ///<summary>Broadcasts notification of the final shutdown of the model.</summary>
        ///<remarks>
        ///<para>
        ///Do not call anywhere other than at the end of OnSessionShutdown.
        ///</para>
        ///<para>
        ///Must not be called when m_closeReason == null, because
        ///otherwise there's a window when a new continuation could be
        ///being enqueued at the same time as we're broadcasting the
        ///shutdown event. See the definition of Enqueue() above.
        ///</para>
        ///</remarks>
        public virtual void OnModelShutdown(ShutdownEventArgs reason)
        {
            //Console.WriteLine("Model shutdown "+((Session)m_session).ChannelNumber+": "+reason);
            m_continuationQueue.HandleModelShutdown(reason);
            ModelShutdownEventHandler handler;
            lock (m_shutdownLock)
            {
                handler = m_modelShutdown;
                m_modelShutdown = null;
            }
            if (handler != null)
            {
                foreach (ModelShutdownEventHandler h in handler.GetInvocationList()) {
                    try {
                        h(this, reason);
                    } catch (Exception e) {
                        CallbackExceptionEventArgs args = new CallbackExceptionEventArgs(e);
                        args.Detail["context"] = "OnModelShutdown";
                        OnCallbackException(args);
                    }
                }
            }
        }

        public virtual void OnBasicReturn(BasicReturnEventArgs args)
        {
            BasicReturnEventHandler handler;
            lock (m_eventLock)
            {
                handler = m_basicReturn;
            }
            if (handler != null)
            {
                foreach (BasicReturnEventHandler h in handler.GetInvocationList()) {
                    try {
                        h(this, args);
                    } catch (Exception e) {
                        CallbackExceptionEventArgs exnArgs = new CallbackExceptionEventArgs(e);
                        exnArgs.Detail["context"] = "OnBasicReturn";
                        OnCallbackException(exnArgs);
                    }
                }
            }
        }

        public virtual void OnCallbackException(CallbackExceptionEventArgs args)
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
        
        public void Enqueue(IRpcContinuation k)
        {
            bool ok = false;
            lock (m_shutdownLock)
            {
                if (m_closeReason == null)
                {
                    m_continuationQueue.Enqueue(k);
                    ok = true;
                }
            }
            if (!ok)
            {
                k.HandleModelShutdown(m_closeReason);
            }
        }

        public void TransmitAndEnqueue(Command cmd, IRpcContinuation k)
        {
            Enqueue(k);
            try
            {
                m_session.Transmit(cmd);
            }
            catch (AlreadyClosedException)
            {
                // Ignored, since the continuation will be told about
                // the closure via an OperationInterruptedException because
                // of the shutdown event propagation.
            }
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

        public void ModelSend(MethodBase method, ContentHeaderBase header, byte[] body)
        {
            m_session.Transmit(new Command(method, header, body));
        }
        
        public MethodBase ModelRpc(MethodBase method, ContentHeaderBase header, byte[] body)
        {
            SimpleBlockingRpcContinuation k = new SimpleBlockingRpcContinuation();
            TransmitAndEnqueue(new Command(method, header, body), k);
            return k.GetReply().Method;
        }

        public abstract bool DispatchAsynchronous(Command cmd);
        
        public void HandleBasicDeliver(string consumerTag,
                                       ulong deliveryTag,
                                       bool redelivered,
                                       string exchange,
                                       string routingKey,
                                       IBasicProperties basicProperties,
                                       byte[] body)
        {
            IBasicConsumer consumer;
            lock (m_consumers)
            {
                consumer = (IBasicConsumer)m_consumers[consumerTag];
            }
            if (consumer == null)
            {
                // FIXME: what is an appropriate thing to do here?
                throw new NotSupportedException("FIXME unsolicited delivery for consumer tag " + consumerTag);
            }

            try {
                consumer.HandleBasicDeliver(consumerTag,
                                            deliveryTag,
                                            redelivered,
                                            exchange,
                                            routingKey,
                                            basicProperties,
                                            body);
            } catch (Exception e) {
                CallbackExceptionEventArgs args = new CallbackExceptionEventArgs(e);
                args.Detail["consumer"] = consumer;
                args.Detail["context"] = "HandleBasicDeliver";
                OnCallbackException(args);
            }
        }

        public void HandleBasicReturn(ushort replyCode,
                                      string replyText,
                                      string exchange,
                                      string routingKey,
                                      IBasicProperties basicProperties,
                                      byte[] body)
        {
            BasicReturnEventArgs e = new BasicReturnEventArgs();
            e.ReplyCode = replyCode;
            e.ReplyText = replyText;
            e.Exchange = exchange;
            e.RoutingKey = routingKey;
            e.BasicProperties = basicProperties;
            e.Body = body;
            OnBasicReturn(e);
        }

        public void HandleConnectionStart(byte versionMajor,
                                          byte versionMinor,
                                          IDictionary serverProperties,
                                          byte[] mechanisms,
                                          byte[] locales)
        {
            if (m_connectionStartCell == null)
            {
                ShutdownEventArgs reason =
                    new ShutdownEventArgs(ShutdownInitiator.Library,
                              CommonFraming.Constants.CommandInvalid,
                              "Unexpected Connection.Start");
                ((ConnectionBase)m_session.Connection).Close(reason);
            }
            ConnectionStartDetails details = new ConnectionStartDetails();
            details.versionMajor = versionMajor;
            details.versionMinor = versionMinor;
            details.serverProperties = serverProperties;
            details.mechanisms = mechanisms;
            details.locales = locales;
            m_connectionStartCell.Value = details;
            m_connectionStartCell = null;
        }

        public void HandleConnectionClose(ushort replyCode,
                                          string replyText,
                                          ushort classId,
                                          ushort methodId)
        {
            ShutdownEventArgs reason = new ShutdownEventArgs(ShutdownInitiator.Peer,
                                 replyCode,
                                 replyText,
                                 classId,
                                 methodId);
            try
            {
                ((ConnectionBase)m_session.Connection).InternalClose(reason);
                _Private_ConnectionCloseOk();
                 SetCloseReason((m_session.Connection).CloseReason);
            }
            catch (IOException)
            {
               // Ignored. We're only trying to be polite by sending
               // the close-ok, after all.
            }
            catch (AlreadyClosedException)
            {
               // Ignored. We're only trying to be polite by sending
               // the close-ok, after all.
            }
        }

        public void HandleChannelClose(ushort replyCode,
                                       string replyText,
                                       ushort classId,
                                       ushort methodId)
        {
            SetCloseReason(new ShutdownEventArgs(ShutdownInitiator.Peer,
                             replyCode,
                             replyText,
                             classId,
                             methodId));
            FinishClose();
            _Private_ChannelCloseOk();
        }

        public void FinishClose()
        {
            if (m_closeReason != null)
            {
                m_session.Close(m_closeReason);
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        public abstract IBasicProperties CreateBasicProperties();
        public abstract IFileProperties CreateFileProperties();
        public abstract IStreamProperties CreateStreamProperties();

        public abstract void ChannelFlow(bool active);
        
        public ushort AccessRequest(string realm)
        {
            return AccessRequest(realm, false, true, true, true, true);
        }
        
        public static AccessRequestConfig GetEnvironmentAccessRequestConfig()
        {
            string setting = Environment.GetEnvironmentVariable("AMQP_ACCESS_REQUEST");
            if (setting == null)
            {
                return AccessRequestConfig.UseDefault;
            }
            switch (setting)
            {
                case "ENABLE": return AccessRequestConfig.Enable;
                case "SUPPRESS": return AccessRequestConfig.Suppress;
                case "USE_DEFAULT": return AccessRequestConfig.UseDefault;
                default: {
                    string message = string.Format("Unsupported AMQP_ACCESS_REQUEST setting: {0}",
                                                   setting);
                    throw new NotSupportedException(message);
                }
            }
        }

        public static AccessRequestConfig Combine(AccessRequestConfig first,
                                                  AccessRequestConfig second)
        {
            return (first == AccessRequestConfig.UseDefault) ? second : first;
        }

        public ushort AccessRequest(string realm,
                        bool exclusive,
                        bool passive,
                        bool active,
                        bool write,
                        bool read)
        {
            bool send;
            switch (Combine(m_session.Connection.Parameters.AccessRequestConfig,
                            GetEnvironmentAccessRequestConfig()))
            {
                case AccessRequestConfig.Enable:
                    send = true;
                    break;
                case AccessRequestConfig.Suppress:
                    send = false;
                    break;
                case AccessRequestConfig.UseDefault:
                    send = !m_session.Connection.Protocol.DefaultSuppressAccessRequest;
                    break;
                default: {
                    send = false;

                    string message = string.Format("Illegal value for AccessRequestConfig: {0}",
                                                   (int) m_session.Connection.Parameters.AccessRequestConfig);
                    throw new ArgumentException(message);
                }
            }
            if (send)
            {
                return _Private_AccessRequest(realm, exclusive, passive, active, write, read);
            }
            else
            {
                return 0;
            }
        }

        // TODO: Consider changing the access modifier or name, _Private => protected (?)
        public abstract ushort _Private_AccessRequest(string realm,
                              bool exclusive,
                              bool passive,
                              bool active,
                              bool write,
                              bool read);

        public void ExchangeDeclare(ushort ticket, string exchange, string type, bool durable)
        {
            ExchangeDeclare(ticket, exchange, type, false, durable, false, false, false, null);
        }

        public void ExchangeDeclare(ushort ticket, string exchange, string type)
        {
            ExchangeDeclare(ticket, exchange, type, false, false, false, false, false, null);
        }

        public abstract void ExchangeDeclare(ushort ticket,
                                             string exchange,
                                             string type,
                                             bool passive,
                                             bool durable,
                                             bool autoDelete,
                                             bool @internal,
                                             bool nowait,
                                             IDictionary arguments);

        public abstract void ExchangeDelete(ushort ticket,
                                            string exchange,
                                            bool ifUnused,
                                            bool nowait);

        //TODO: Mark these as virtual, maybe the model has an optimized way
        //      of dealing with missing parameters.

        public string QueueDeclare(ushort ticket)
        {
            return QueueDeclare(ticket, "", false, false, true, true, false, null);
        }

        public string QueueDeclare(ushort ticket, string queue)
        {
            return QueueDeclare(ticket, queue, false);
        }

        public string QueueDeclare(ushort ticket, string queue, bool durable)
        {
            return QueueDeclare(ticket, queue, false, durable, false, false, false, null);
        }

        public abstract string QueueDeclare(ushort ticket,
                                            string queue,
                                            bool passive,
                                            bool durable,
                                            bool exclusive,
                                            bool autoDelete,
                                            bool nowait,
                                            IDictionary arguments);

        public abstract void QueueBind(ushort ticket,
                                       string queue,
                                       string exchange,
                                       string routingKey,
                                       bool nowait,
                                       IDictionary arguments);

        public abstract void QueueUnbind(ushort ticket,
                                         string queue,
                                         string exchange,
                                         string routingKey,
                                         IDictionary arguments);

        public abstract uint QueuePurge(ushort ticket,
                                        string queue,
                                        bool nowait);

        public abstract uint QueueDelete(ushort ticket,
                                         string queue,
                                         bool ifUnused,
                                         bool ifEmpty,
                                         bool nowait);

        public string BasicConsume(ushort ticket,
                                   string queue,
                                   IDictionary filter,
                                   IBasicConsumer consumer)
        {
            return BasicConsume(ticket, queue, false, filter, consumer);
        }

        public string BasicConsume(ushort ticket,
                                   string queue,
                                   bool noAck,
                                   IDictionary filter,
                                   IBasicConsumer consumer)
        {
            return BasicConsume(ticket, queue, noAck, "", filter, consumer);
        }

        public string BasicConsume(ushort ticket,
                                   string queue,
                                   bool noAck,
                                   string consumerTag,
                                   IDictionary filter,
                                   IBasicConsumer consumer)
        {
            return BasicConsume(ticket, queue, noAck, consumerTag, false, false, filter, consumer);
        }

        public class BasicConsumerRpcContinuation : SimpleBlockingRpcContinuation
        {
            public IBasicConsumer m_consumer;
            public string m_consumerTag;
            public BasicConsumerRpcContinuation() { }
        }

        public string BasicConsume(ushort ticket,
                                   string queue,
                                   bool noAck,
                                   string consumerTag,
                                   bool noLocal,
                                   bool exclusive,
                                   IDictionary filter,
                                   IBasicConsumer consumer)
        {
            ModelShutdown += new ModelShutdownEventHandler(consumer.HandleModelShutdown);

            BasicConsumerRpcContinuation k = new BasicConsumerRpcContinuation();
            k.m_consumer = consumer;

            Enqueue(k);
            // Non-nowait. We have an unconventional means of getting
            // the RPC response, but a response is still expected.
            try
            {
                _Private_BasicConsume(ticket, queue, consumerTag, noLocal, noAck, exclusive,
                    /*nowait:*/ false, filter);
            }
            catch (AlreadyClosedException)
            {
                // Ignored, since the continuation will be told about
                // the closure via an OperationInterruptedException because
                // of the shutdown event propagation.
            }
            k.GetReply();
            string actualConsumerTag = k.m_consumerTag;

            return actualConsumerTag;
        }

        public void HandleBasicConsumeOk(string consumerTag)
        {
            BasicConsumerRpcContinuation k =
                (BasicConsumerRpcContinuation)m_continuationQueue.Next();
            k.m_consumerTag = consumerTag;
            lock (m_consumers)
            {
                m_consumers[consumerTag] = k.m_consumer;
            }
            try {
                k.m_consumer.HandleBasicConsumeOk(consumerTag);
            } catch (Exception e) {
                // FIXME: should we propagate the exception to the
                // caller of BasicConsume?
                CallbackExceptionEventArgs args = new CallbackExceptionEventArgs(e);
                args.Detail["consumer"] = k.m_consumer;
                args.Detail["context"] = "HandleBasicConsumeOk";
                OnCallbackException(args);
            }
            k.HandleCommand(null); // release the continuation.
        }

        public void BasicCancel(string consumerTag)
        {
            BasicConsumerRpcContinuation k = new BasicConsumerRpcContinuation();
            k.m_consumerTag = consumerTag;

            Enqueue(k);

            try
            {
                _Private_BasicCancel(consumerTag, false);
            }
            catch (AlreadyClosedException)
            {
                // Ignored, since the continuation will be told about
                // the closure via an OperationInterruptedException because
                // of the shutdown event propagation.
            }

            k.GetReply();

            ModelShutdown -= new ModelShutdownEventHandler(k.m_consumer.HandleModelShutdown);
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
            BasicConsumerRpcContinuation k =
                (BasicConsumerRpcContinuation)m_continuationQueue.Next();

            Trace.Assert(k.m_consumerTag == consumerTag, string.Format(
                "Consumer tag mismatch during cancel: {0} != {1}",
                k.m_consumerTag,
                consumerTag
                ));
                
            lock (m_consumers)
            {
                k.m_consumer = (IBasicConsumer)m_consumers[consumerTag];
                m_consumers.Remove(consumerTag);
            }
            try {
                k.m_consumer.HandleBasicCancelOk(consumerTag);
            } catch (Exception e) {
                // FIXME: should we propagate the exception to the
                // caller of BasicCancel?
                CallbackExceptionEventArgs args = new CallbackExceptionEventArgs(e);
                args.Detail["consumer"] = k.m_consumer;
                args.Detail["context"] = "HandleBasicCancelOk";
                OnCallbackException(args);
            }
            k.HandleCommand(null); // release the continuation.
        }

        public class BasicGetRpcContinuation : SimpleBlockingRpcContinuation
        {
            public BasicGetResult m_result;
            public BasicGetRpcContinuation() { }
        }

        public BasicGetResult BasicGet(ushort ticket,
                                       string queue,
                                       bool noAck)
        {
            BasicGetRpcContinuation k = new BasicGetRpcContinuation();
            Enqueue(k);
            try
            {
                _Private_BasicGet(ticket, queue, noAck);
            }
            catch (AlreadyClosedException)
            {
                // Ignored, since the continuation will be told about
                // the closure via an OperationInterruptedException because
                // of the shutdown event propagation.
            }
            k.GetReply();
            return k.m_result;
        }

        public abstract void BasicQos(uint prefetchSize,
                                      ushort prefetchCount,
                                      bool global);

        public abstract void _Private_BasicConsume(ushort ticket,
                                                   string queue,
                                                   string consumerTag,
                                                   bool noLocal,
                                                   bool noAck,
                                                   bool exclusive,
                                                   bool nowait,
                                                   IDictionary filter);

        public abstract void _Private_BasicCancel(string consumerTag,
                                                  bool nowait);

        public void BasicPublish(ushort ticket,
                                 PublicationAddress addr,
                                 IBasicProperties basicProperties,
                                 byte[] body)
        {
            BasicPublish(ticket,
                         addr.ExchangeName,
                         addr.RoutingKey,
                         basicProperties,
                         body);
        }

        public void BasicPublish(ushort ticket,
                                 string exchange,
                                 string routingKey,
                                 IBasicProperties basicProperties,
                                 byte[] body)
        {
            BasicPublish(ticket,
                         exchange,
                         routingKey,
                         false,
                         false,
                         basicProperties,
                         body);
        }

        public void BasicPublish(ushort ticket,
                                 string exchange,
                                 string routingKey,
                                 bool mandatory,
                                 bool immediate,
                                 IBasicProperties basicProperties,
                                 byte[] body)
        {
            if (basicProperties == null)
            {
                basicProperties = CreateBasicProperties();
            }
            _Private_BasicPublish(ticket,
                                  exchange,
                                  routingKey,
                                  mandatory,
                                  immediate,
                                  basicProperties,
                                  body);
        }

        public abstract void _Private_BasicPublish(ushort ticket,
                                                   string exchange,
                                                   string routingKey,
                                                   bool mandatory,
                                                   bool immediate,
                                                   IBasicProperties basicProperties,
                                                   byte[] body);

        public abstract void BasicAck(ulong deliveryTag,
                                      bool multiple);

        public abstract void BasicReject(ulong deliveryTag,
                                         bool requeue);

        public abstract void BasicRecover(bool requeue);

        public abstract void TxSelect();
        public abstract void TxCommit();
        public abstract void TxRollback();

        public abstract void DtxSelect();
        public abstract void DtxStart(string dtxIdentifier);

        void IDisposable.Dispose()
        {
            Close(200, "");
        }

        public void Close(ushort replyCode, string replyText)
        {
            ShutdownContinuation k = new ShutdownContinuation();
            ModelShutdown += new ModelShutdownEventHandler(k.OnShutdown);

            if (SetCloseReason(new ShutdownEventArgs(ShutdownInitiator.Application,
                                 replyCode,
                                 replyText)))
            {
                _Private_ChannelClose(replyCode, replyText, 0, 0);
            }

            k.Wait();
        }

        public void HandleChannelCloseOk()
        {
            FinishClose();
        }

        public abstract void _Private_ChannelOpen(string outOfBand);

        public abstract void _Private_ChannelClose(ushort replyCode,
                                                   string replyText,
                                                   ushort classId,
                                                   ushort methodId);

        public abstract void _Private_ChannelCloseOk();

        public abstract void _Private_BasicGet(ushort ticket,
                                               string queue,
                                               bool noAck);

        public void HandleBasicGetOk(ulong deliveryTag,
                                     bool redelivered,
                                     string exchange,
                                     string routingKey,
                                     uint messageCount,
                                     IBasicProperties basicProperties,
                                     byte[] body)
        {
            BasicGetRpcContinuation k = (BasicGetRpcContinuation)m_continuationQueue.Next();
            k.m_result = new BasicGetResult(deliveryTag,
                                            redelivered,
                                            exchange,
                                            routingKey,
                                            messageCount,
                                            basicProperties,
                                            body);
            k.HandleCommand(null); // release the continuation.
        }

        public void HandleBasicGetEmpty()
        {
            BasicGetRpcContinuation k = (BasicGetRpcContinuation)m_continuationQueue.Next();
            k.m_result = null;
            k.HandleCommand(null); // release the continuation.
        }

        public abstract ConnectionTuneDetails ConnectionStartOk(IDictionary clientProperties,
                                                                string mechanism,
                                                                byte[] response,
                                                                string locale);

        public abstract void ConnectionTuneOk(ushort channelMax,
                                              uint frameMax,
                                              ushort heartbeat);

        public class ConnectionOpenContinuation : SimpleBlockingRpcContinuation
        {
            public bool m_redirect;
            public string m_host;
            public string m_knownHosts;
            public ConnectionOpenContinuation() { }
        }

        public string ConnectionOpen(string virtualHost,
                                     string capabilities,
                                     bool insist)
        {
            ConnectionOpenContinuation k = new ConnectionOpenContinuation();
            Enqueue(k);
            try {
                _Private_ConnectionOpen(virtualHost, capabilities, insist);
            }
            catch (AlreadyClosedException)
            {
                // Ignored, since the continuation will be told about
                // the closure via an OperationInterruptedException because
                // of the shutdown event propagation.
            }
            k.GetReply();
            if (k.m_redirect) {
                throw new RedirectException(m_session.Connection.Protocol,
                                            k.m_host,
                                            k.m_knownHosts);
            } else {
                return k.m_knownHosts;
            }
        }

        public abstract void _Private_ConnectionOpen(string virtualHost,
                                                     string capabilities,
                                                     bool insist);

        public void HandleConnectionOpenOk(string knownHosts)
        {
            ConnectionOpenContinuation k = (ConnectionOpenContinuation)m_continuationQueue.Next();
            k.m_redirect = false;
            k.m_host = null;
            k.m_knownHosts = knownHosts;
            k.HandleCommand(null); // release the continuation.
        }

        public void HandleConnectionRedirect(string host,
                                             string knownHosts)
        {
            ConnectionOpenContinuation k = (ConnectionOpenContinuation)m_continuationQueue.Next();
            k.m_redirect = true;
            k.m_host = host;
            k.m_knownHosts = knownHosts;
            k.HandleCommand(null); // release the continuation.
        }

        public abstract void _Private_ConnectionClose(ushort replyCode,
                                                      string replyText,
                                                      ushort classId,
                                                      ushort methodId);

        public abstract void _Private_ConnectionCloseOk();

        public override string ToString() {
            return m_session.ToString();
        }
    }
}
