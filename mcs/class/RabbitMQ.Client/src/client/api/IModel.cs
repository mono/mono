// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2010 LShift Ltd., Cohesive Financial
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
//   The Initial Developers of the Original Code are LShift Ltd,
//   Cohesive Financial Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created before 22-Nov-2008 00:00:00 GMT by LShift Ltd,
//   Cohesive Financial Technologies LLC, or Rabbit Technologies Ltd
//   are Copyright (C) 2007-2008 LShift Ltd, Cohesive Financial
//   Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd are Copyright (C) 2007-2010 LShift
//   Ltd. Portions created by Cohesive Financial Technologies LLC are
//   Copyright (C) 2007-2010 Cohesive Financial Technologies
//   LLC. Portions created by Rabbit Technologies Ltd are Copyright
//   (C) 2007-2010 Rabbit Technologies Ltd.
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Collections;
using RabbitMQ.Client.Apigen.Attributes;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Client
{
    ///<summary>Common AMQP model, spanning the union of the
    ///functionality offered by versions 0-8, 0-8qpid, and 0-9
    ///(without WIP) of AMQP.</summary>
    ///<remarks>
    /// Extends the IDisposable interface, so that the "using"
    /// statement can be used to scope the lifetime of a channel when
    /// appropriate.
    ///</remarks>
    public interface IModel: IDisposable
    {
        ///<summary>Notifies the destruction of the model.</summary>
        ///<remarks>
        /// If the model is already destroyed at the time an event
        /// handler is added to this event, the event handler will be
        /// fired immediately.
        ///</remarks>
        event ModelShutdownEventHandler ModelShutdown;

        ///<summary>Signalled when a Basic.Return command arrives from
        ///the broker.</summary>
        event BasicReturnEventHandler BasicReturn;

        ///<summary>Signalled when an exception occurs in a callback
        ///invoked by the model.</summary>
        ///<remarks>
        ///Examples of cases where this event will be signalled
        ///include exceptions thrown in IBasicConsumer methods, or
        ///exceptions thrown in ModelShutdownEventHandler delegates
        ///etc.
        ///</remarks>
        event CallbackExceptionEventHandler CallbackException;

        ///<summary>Returns null if the session is still in a state
        ///where it can be used, or the cause of its closure
        ///otherwise.</summary>
        ShutdownEventArgs CloseReason { get; }

        ///<summary>Returns true if the session is still in a state
        ///where it can be used. Identical to checking if CloseReason
        ///== null.</summary>
        bool IsOpen { get; }

        ///<summary>Construct a completely empty content header for
        ///use with the Basic content class.</summary>
        [AmqpContentHeaderFactory("basic")]
        IBasicProperties CreateBasicProperties();

        ///<summary>Construct a completely empty content header for
        ///use with the File content class.</summary>
        [AmqpContentHeaderFactory("file")]
        IFileProperties CreateFileProperties();

        ///<summary>Construct a completely empty content header for
        ///use with the Stream content class.</summary>
        [AmqpContentHeaderFactory("stream")]
        IStreamProperties CreateStreamProperties();

        ///<summary>(Spec method) Channel flow control functionality.</summary>
        ///<remarks>
        ///</remarks>
        [return: AmqpFieldMapping(null, "active")]
        void ChannelFlow(bool active);

        ///<summary>(Spec method) Declare an exchange.</summary>
        ///<remarks>
        ///The exchange is declared non-passive, non-autodelete, and
        ///non-internal, with no arguments. The "nowait" option is not
        ///exercised.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        void ExchangeDeclare(string exchange, string type, bool durable);

        ///<summary>(Spec method) Declare an exchange.</summary>
        ///<remarks>
        ///The exchange is declared non-passive, non-durable, non-autodelete, and
        ///non-internal, with no arguments. The "nowait" option is not
        ///exercised.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        void ExchangeDeclare(string exchange, string type);

        ///<summary>(Spec method) Declare an exchange.</summary>
        void ExchangeDeclare(string exchange,
                             string type,
                             bool passive,
                             bool durable,
                             bool autoDelete,
                             bool @internal,
                             [AmqpNowaitArgument(null)]
                             bool nowait,
                             IDictionary arguments);

        ///<summary>(Spec method) Delete an exchange.</summary>
        void ExchangeDelete(string exchange,
                            bool ifUnused,
                            [AmqpNowaitArgument(null)]
                            bool nowait);

        ///<summary>(Spec method) Declare a queue.</summary>
        ///<remarks>
        ///The queue is declared non-passive, non-durable,
        ///but exclusive and autodelete, with no arguments. The
        ///server autogenerates a name for the queue - the generated
        ///name is the return value of this method.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        string QueueDeclare();

        ///<summary>(Spec method) Declare a queue.</summary>
        ///<remarks>
        ///The queue is declared non-passive, non-durable,
        ///non-exclusive, and non-autodelete, with no arguments.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        string QueueDeclare(string queue);

        ///<summary>(Spec method) Declare a queue.</summary>
        ///<remarks>
        ///The queue is declared non-passive, non-exclusive, and
        ///non-autodelete, with no arguments.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        string QueueDeclare(string queue,
                            bool durable);

        ///<summary>(Spec method) Declare a queue.</summary>
        ///<remarks>
        ///Returns the name of the queue that was declared.
        ///</remarks>
        [return: AmqpFieldMapping(null, "queue")]
        string QueueDeclare(string queue,
                            bool passive,
                            bool durable,
                            bool exclusive,
                            bool autoDelete,
                            [AmqpNowaitArgument(null)]
                            bool nowait,
                            IDictionary arguments);

        ///<summary>(Spec method) Bind a queue to an exchange.</summary>
        void QueueBind(string queue,
                       string exchange,
                       string routingKey,
                       [AmqpNowaitArgument(null)]
                       bool nowait,
                       IDictionary arguments);

        ///<summary>(Spec method) Unbind a queue from an exchange.</summary>
        ///<remarks>
        ///Note: This operation is only supported when communicating
        ///using AMQP protocol version 0-9, or when communicating with
        ///a 0-8 broker that has been enhanced with the unofficial
        ///addition of a queue.unbind method.
        ///</remarks>
        [AmqpUnsupported("RabbitMQ.Client.Framing.v0_8qpid")]
        void QueueUnbind(string queue,
                         string exchange,
                         string routingKey,
                         IDictionary arguments);

        ///<summary>(Spec method) Purge a queue of messages.</summary>
        ///<remarks>
        ///Returns the number of messages purged. If nowait is
        ///specified, returns <code>uint.MaxValue</code>.
        ///</remarks>
        [return: AmqpFieldMapping(null, "messageCount")]
        uint QueuePurge(string queue,
                        [AmqpNowaitArgument(null, "0xFFFFFFFF")]
                        bool nowait);

        ///<summary>(Spec method) Delete a queue.</summary>
        ///<remarks>
        ///Returns the number of messages purged during queue
        ///deletion. If nowait is specified, returns
        ///<code>uint.MaxValue</code>.
        ///</remarks>
        [return: AmqpFieldMapping(null, "messageCount")]
        uint QueueDelete(string queue,
                         bool ifUnused,
                         bool ifEmpty,
                         [AmqpNowaitArgument(null, "0xFFFFFFFF")]
                         bool nowait);

        ///<summary>Start a Basic content-class consumer.</summary>
        ///<remarks>
        ///The consumer is started with noAck=false (i.e. BasicAck is required),
        ///an empty consumer tag (i.e. the server creates and returns a fresh consumer tag),
        ///noLocal=false and exclusive=false.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        string BasicConsume(string queue,
                            IDictionary filter,
                            IBasicConsumer consumer);

        ///<summary>Start a Basic content-class consumer.</summary>
        ///<remarks>
        ///The consumer is started with
        ///an empty consumer tag (i.e. the server creates and returns a fresh consumer tag),
        ///noLocal=false and exclusive=false.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        string BasicConsume(string queue,
                            bool noAck,
                            IDictionary filter,
                            IBasicConsumer consumer);

        ///<summary>Start a Basic content-class consumer.</summary>
        ///<remarks>
        ///The consumer is started with
        ///noLocal=false and exclusive=false.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        string BasicConsume(string queue,
                            bool noAck,
                            string consumerTag,
                            IDictionary filter,
                            IBasicConsumer consumer);

        ///<summary>Start a Basic content-class consumer.</summary>
        [AmqpMethodDoNotImplement(null)]
        string BasicConsume(string queue,
                            bool noAck,
                            string consumerTag,
                            bool noLocal,
                            bool exclusive,
                            IDictionary filter,
                            IBasicConsumer consumer);

        ///<summary>Delete a Basic content-class consumer.</summary>
        [AmqpMethodDoNotImplement(null)]
        void BasicCancel(string consumerTag);

        ///<summary>(Spec method) Configures QoS parameters of the Basic content-class.</summary>
        void BasicQos(uint prefetchSize,
                      ushort prefetchCount,
                      bool global);

        ///<summary>(Spec method) Convenience overload of BasicPublish.</summary>
        ///<remarks>
        ///The publication occurs with mandatory=false and immediate=false.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        void BasicPublish(PublicationAddress addr,
                          IBasicProperties basicProperties,
                          byte[] body);

        ///<summary>(Spec method) Convenience overload of BasicPublish.</summary>
        ///<remarks>
        ///The publication occurs with mandatory=false and immediate=false.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        void BasicPublish(string exchange,
                          string routingKey,
                          IBasicProperties basicProperties,
                          byte[] body);

        ///<summary>(Spec method) Publish a message using the Basic
        ///content-class.</summary>
        [AmqpMethodDoNotImplement(null)]
        void BasicPublish(string exchange,
                          string routingKey,
                          bool mandatory,
                          bool immediate,
                          IBasicProperties basicProperties,
                          byte[] body);

        ///<summary>(Spec method) Acknowledge one or more delivered message(s).</summary>
        void BasicAck(ulong deliveryTag,
                      bool multiple);

        ///<summary>(Spec method) Reject a delivered message.</summary>
        void BasicReject(ulong deliveryTag,
                         bool requeue);

        ///<summary>(Spec method)</summary>
        void BasicRecover(bool requeue);

        ///<summary>(Spec method) Retrieve an individual message, if
        ///one is available; returns null if the server answers that
        ///no messages are currently available. See also
        ///IModel.BasicAck.</summary>
        [AmqpMethodDoNotImplement(null)]
        BasicGetResult BasicGet(string queue,
                                bool noAck);

        ///<summary>(Spec method) Enable TX mode for this session.</summary>
        void TxSelect();

        ///<summary>(Spec method) Commit this session's active TX
        ///transaction.</summary>
        void TxCommit();

        ///<summary>(Spec method) Roll back this session's active TX
        ///transaction.</summary>
        void TxRollback();

        ///<summary>(Spec method) Enable DTX mode for this session.</summary>
        void DtxSelect();

        ///<summary>(Spec method)</summary>
        void DtxStart(string dtxIdentifier);

        ///<summary>Close this session.</summary>
        ///<remarks>
        ///If the session is already closed (or closing), then this
        ///method does nothing but wait for the in-progress close
        ///operation to complete. This method will not return to the
        ///caller until the shutdown is complete.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        void Close();
        
        ///<summary>Close this session.</summary>
        ///<remarks>
        ///The method behaves in the same way as Close(), with the only
        ///difference that the model is closed with the given model
        ///close code and message.
        ///<para>
        ///The close code (See under "Reply Codes" in the AMQP specification)
        ///</para>
        ///<para>
        ///A message indicating the reason for closing the model
        ///</para>
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        void Close(ushort replyCode, string replyText);
        
        ///<summary>Abort this session.</summary>
        ///<remarks>
        ///If the session is already closed (or closing), then this
        ///method does nothing but wait for the in-progress close
        ///operation to complete. This method will not return to the
        ///caller until the shutdown is complete.
        ///In comparison to normal Close() method, Abort() will not throw
        ///AlreadyClosedException or IOException during closing model.
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        void Abort();
        
        ///<summary>Abort this session.</summary>
        ///<remarks>
        ///The method behaves in the same way as Abort(), with the only
        ///difference that the model is closed with the given model
        ///close code and message.
        ///<para>
        ///The close code (See under "Reply Codes" in the AMQP specification)
        ///</para>
        ///<para>
        ///A message indicating the reason for closing the model
        ///</para>
        ///</remarks>
        [AmqpMethodDoNotImplement(null)]
        void Abort(ushort replyCode, string replyText);
    }

    ///<summary>Represents Basic.GetOk responses from the server.</summary>
    ///<remarks>
    /// Basic.Get either returns an instance of this class, or null if
    /// a Basic.GetEmpty was received.
    ///</remarks>
    public class BasicGetResult
    {
        private ulong m_deliveryTag;
        private bool m_redelivered;
        private string m_exchange;
        private string m_routingKey;
        private uint m_messageCount;
        private IBasicProperties m_basicProperties;
        private byte[] m_body;

        ///<summary>Sets the new instance's properties from the
        ///arguments passed in.</summary>
        public BasicGetResult(ulong deliveryTag,
                              bool redelivered,
                              string exchange,
                              string routingKey,
                              uint messageCount,
                              IBasicProperties basicProperties,
                              byte[] body)
        {
            m_deliveryTag = deliveryTag;
            m_redelivered = redelivered;
            m_exchange = exchange;
            m_routingKey = routingKey;
            m_messageCount = messageCount;
            m_basicProperties = basicProperties;
            m_body = body;
        }

        ///<summary>Retrieve the delivery tag for this message. See also IModel.BasicAck.</summary>
        public ulong DeliveryTag { get { return m_deliveryTag; } }
        ///<summary>Retrieve the redelivered flag for this message.</summary>
        public bool Redelivered { get { return m_redelivered; } }
        ///<summary>Retrieve the exchange this message was published to.</summary>
        public string Exchange { get { return m_exchange; } }
        ///<summary>Retrieve the routing key with which this message was published.</summary>
        public string RoutingKey { get { return m_routingKey; } }

        ///<summary>Retrieve the number of messages pending on the
        ///queue, excluding the message being delivered.</summary>
        ///<remarks>
        /// Note that this figure is indicative, not reliable, and can
        /// change arbitrarily as messages are added to the queue and
        /// removed by other clients.
        ///</remarks>
        public uint MessageCount { get { return m_messageCount; } }

        ///<summary>Retrieves the Basic-class content header properties for this message.</summary>
        public IBasicProperties BasicProperties { get { return m_basicProperties; } }
        ///<summary>Retrieves the body of this message.</summary>
        public byte[] Body { get { return m_body; } }
    }
}

namespace RabbitMQ.Client.Impl
{
    ///<summary>Not part of the public API. Extension of IModel to
    ///include utilities and connection-setup routines needed by the
    ///implementation side.</summary>
    ///
    ///<remarks>This interface is used by the API autogeneration
    ///process. The AMQP XML specifications are read by the spec
    ///compilation tool, and after the basic method interface and
    ///implementation classes are generated, this interface is
    ///scanned, and a spec-version-specific implementation is
    ///autogenerated. Annotations are used on certain methods, return
    ///types, and parameters, to customise the details of the
    ///autogeneration process.</remarks>
    ///
    ///<see cref="RabbitMQ.Client.Impl.ModelBase"/>
    ///<see cref="RabbitMQ.Client.Framing.Impl.v0_9.Model"/>
    public interface IFullModel : RabbitMQ.Client.IModel
    {
        ///<summary>Used to send a Basic.Publish method. Called by the
        ///public publish method after potential null-reference issues
        ///have been rectified.</summary>
        [AmqpMethodMapping(null, "basic", "publish")]
        void _Private_BasicPublish(string exchange,
                                   string routingKey,
                                   bool mandatory,
                                   bool immediate,
                                   [AmqpContentHeaderMapping]
                                   IBasicProperties basicProperties,
                                   [AmqpContentBodyMapping]
                                   byte[] body);

        ///<summary>Used to send a Basic.Consume method. The public
        ///consume API calls this while also managing internal
        ///datastructures.</summary>
        [AmqpForceOneWay]
        [AmqpMethodMapping(null, "basic", "consume")]
        void _Private_BasicConsume(string queue,
                                   string consumerTag,
                                   bool noLocal,
                                   bool noAck,
                                   bool exclusive,
                                   bool nowait,
                                   [AmqpUnsupported("RabbitMQ.Client.Framing.v0_8")]
                                   [AmqpFieldMapping("RabbitMQ.Client.Framing.v0_8qpid",
                                                     "arguments")]
                                   IDictionary filter);

        ///<summary>Handle incoming Basic.ConsumeOk methods.</summary>
        void HandleBasicConsumeOk(string consumerTag);

        ///<summary>Used to send a Basic.Cancel method. The public
        ///consume API calls this while also managing internal
        ///datastructures.</summary>
        [AmqpForceOneWay]
        [AmqpMethodMapping(null, "basic", "cancel")]
        void _Private_BasicCancel(string consumerTag,
                                  bool nowait);

        ///<summary>Handle incoming Basic.CancelOk methods.</summary>
        void HandleBasicCancelOk(string consumerTag);

        ///<summary>Used to send a Channel.Open. Called during session
        ///initialisation.</summary>
        [AmqpMethodMapping(null, "channel", "open")]
        void _Private_ChannelOpen(string outOfBand);

        ///<summary>Used to send a Channel.CloseOk. Called during
        ///session shutdown.</summary>
        [AmqpMethodMapping(null, "channel", "close-ok")]
        void _Private_ChannelCloseOk();

        ///<summary>Used to send a Channel.Close. Called during
        ///session shutdown.</summary>
        [AmqpForceOneWay]
        [AmqpMethodMapping(null, "channel", "close")]
        void _Private_ChannelClose(ushort replyCode,
                                   string replyText,
                                   ushort classId,
                                   ushort methodId);

        ///<summary>Used to send a Basic.Get. Basic.Get is a special
        ///case, since it can result in a Basic.GetOk or a
        ///Basic.GetEmpty, so this level of manual control is
        ///required.</summary>
        [AmqpForceOneWay]
        [AmqpMethodMapping(null, "basic", "get")]
        void _Private_BasicGet(string queue,
                               bool noAck);

        ///<summary>Handle incoming Basic.GetOk methods. Routes the
        ///information to a waiting Basic.Get continuation.</summary>
        void HandleBasicGetOk(ulong deliveryTag,
                              bool redelivered,
                              string exchange,
                              string routingKey,
                              uint messageCount,
                              [AmqpContentHeaderMapping]
                              IBasicProperties basicProperties,
                              [AmqpContentBodyMapping]
                              byte[] body);

        ///<summary>Handle incoming Basic.GetEmpty methods. Routes the
        ///information to a waiting Basic.Get continuation.</summary>
        ///<remarks>
        /// Note that the clusterId field is ignored, as in the
        /// specification it notes that it is "deprecated pending
        /// review".
        ///</remarks>
        void HandleBasicGetEmpty();

        ///<summary>Handle incoming Basic.Deliver methods. Dispatches
        ///to waiting consumers.</summary>
        void HandleBasicDeliver(string consumerTag,
                                ulong deliveryTag,
                                bool redelivered,
                                string exchange,
                                string routingKey,
                                [AmqpContentHeaderMapping]
                                IBasicProperties basicProperties,
                                [AmqpContentBodyMapping]
                                byte[] body);

        ///<summary>Handle incoming Basic.Return methods. Signals a
        ///BasicReturnEvent.</summary>
        void HandleBasicReturn(ushort replyCode,
                               string replyText,
                               string exchange,
                               string routingKey,
                               [AmqpContentHeaderMapping]
                               IBasicProperties basicProperties,
                               [AmqpContentBodyMapping]
                               byte[] body);
                               
        ///<summary>Used to send a Channel.FlowOk. Confirms that
        ///Channel.Flow from the broker was processed.</summary>
        [AmqpMethodMapping(null, "channel", "flow-ok")]
        void _Private_ChannelFlowOk();
        
        ///<summary>Handle incoming Channel.Flow methods. Either
        ///stops or resumes sending the methods that have content.</summary>
        void HandleChannelFlow(bool active);

        ///<summary>Handle an incoming Channel.Close. Shuts down the
        ///session and model.</summary>
        void HandleChannelClose(ushort replyCode,
                                string replyText,
                                ushort classId,
                                ushort methodId);

        ///<summary>Handle an incoming Channel.CloseOk.</summary>
        void HandleChannelCloseOk();

        ///////////////////////////////////////////////////////////////////////////
        // Connection-related methods, for use in channel 0 during
        // connection startup/shutdown.

        ///<summary>Handle an incoming Connection.Start. Used during
        ///connection initialisation.</summary>
        void HandleConnectionStart(byte versionMajor,
                                   byte versionMinor,
                                   IDictionary serverProperties,
                                   byte[] mechanisms,
                                   byte[] locales);

        ///<summary>Sends a Connection.StartOk, and waits for a
        ///Connection.Tune. Used during connection
        ///initialisation.</summary>
        [return: AmqpMethodMapping(null, "connection", "tune")]
        ConnectionTuneDetails ConnectionStartOk(IDictionary clientProperties,
                                                string mechanism,
                                                byte[] response,
                                                string locale);

        ///<summary>Sends a Connection.TuneOk. Used during connection
        ///initialisation.</summary>
        void ConnectionTuneOk(ushort channelMax,
                              uint frameMax,
                              ushort heartbeat);


        ///<summary>Used to send a Connection.Open. Called during
        ///connection startup.</summary>
        [AmqpForceOneWay]
        [AmqpMethodMapping(null, "connection", "open")]
        void _Private_ConnectionOpen(string virtualHost,
                                     string capabilities,
                                     bool insist);

        ///<summary>Handle an incoming Connection.OpenOk.</summary>
        void HandleConnectionOpenOk(string knownHosts);

        ///<summary>Handle an incoming Connection.Redirect.</summary>
        void HandleConnectionRedirect(string host,
                                      string knownHosts);

        ///<summary>Used to send a Connection.Close. Called during
        ///connection shutdown.</summary>
        [AmqpMethodMapping(null, "connection", "close")]
        void _Private_ConnectionClose(ushort replyCode,
                                      string replyText,
                                      ushort classId,
                                      ushort methodId);

        ///<summary>Used to send a Connection.CloseOk. Called during
        ///connection shutdown.</summary>
        [AmqpMethodMapping(null, "connection", "close-ok")]
        void _Private_ConnectionCloseOk();

        ///<summary>Handle an incoming Connection.Close. Shuts down the
        ///connection and all sessions and models.</summary>
        void HandleConnectionClose(ushort replyCode,
                                   string replyText,
                                   ushort classId,
                                   ushort methodId);
    }

    ///<summary>Essential information from an incoming Connection.Tune
    ///method.</summary>
    public struct ConnectionTuneDetails
    {
        ///<summary>The peer's suggested channel-max parameter.</summary>
        public ushort m_channelMax;
        ///<summary>The peer's suggested frame-max parameter.</summary>
        public uint m_frameMax;
        ///<summary>The peer's suggested heartbeat parameter.</summary>
        public ushort m_heartbeat;
    }
}

namespace RabbitMQ.Client.Apigen.Attributes
{
    ///<summary>Base class for attributes for controlling the API
    ///autogeneration process.</summary>
    public abstract class AmqpApigenAttribute : Attribute
    {
        ///<summary>The specification namespace (i.e. version) that
        ///this attribute applies to, or null for all specification
        ///versions.</summary>
        public string m_namespaceName;

        public AmqpApigenAttribute(string namespaceName)
        {
            m_namespaceName = namespaceName;
        }
    }

    ///<summary>Causes the API generator to ignore the attributed method.</summary>
    ///
    ///<remarks>Mostly used to declare convenience overloads of
    ///various AMQP methods in the IModel interface. The API
    ///autogeneration process should of course not attempt to produce
    ///an implementation of the convenience methods, as they will be
    ///implemented by hand with sensible defaults, delegating to the
    ///autogenerated variant of the method concerned.</remarks>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AmqpMethodDoNotImplementAttribute : AmqpApigenAttribute
    {
        public AmqpMethodDoNotImplementAttribute(string namespaceName)
            : base(namespaceName) { }
    }

    ///<summary>Causes the API generator to generate asynchronous
    ///receive code for the attributed method.</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AmqpAsynchronousHandlerAttribute : AmqpApigenAttribute
    {
        public AmqpAsynchronousHandlerAttribute(string namespaceName)
            : base(namespaceName) { }
    }

    ///<summary>Causes the API generator to generate
    ///exception-throwing code for, instead of simply ignoring, the
    ///attributed method.</summary>
    ///
    ///<see cref="AmqpMethodDoNotImplementAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AmqpUnsupportedAttribute : AmqpApigenAttribute
    {
        public AmqpUnsupportedAttribute(string namespaceName)
            : base(namespaceName) { }
    }

    ///<summary>Informs the API generator which AMQP method field to
    ///use for either a parameter in a request, or for a simple result
    ///in a reply.</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AmqpFieldMappingAttribute : AmqpApigenAttribute
    {
        public string m_fieldName;

        public AmqpFieldMappingAttribute(string namespaceName,
                                string fieldName)
            : base(namespaceName)
        {
            m_fieldName = fieldName;
        }
    }

    ///<summary>Informs the API generator which AMQP method to use for
    ///either a request (if applied to an IModel method) or a reply
    ///(if applied to an IModel method result).</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AmqpMethodMappingAttribute : AmqpApigenAttribute
    {
        public string m_className;
        public string m_methodName;

        public AmqpMethodMappingAttribute(string namespaceName,
                                 string className,
                                 string methodName)
            : base(namespaceName)
        {
            m_className = className;
            m_methodName = methodName;
        }
    }

    ///<summary>This attribute, if placed on a parameter in an IModel
    ///method, causes it to be interpreted as a "nowait" parameter for
    ///the purposes of autogenerated RPC reply continuation management
    ///and control.</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AmqpNowaitArgumentAttribute : AmqpApigenAttribute
    {
        public string m_replacementExpression;

        public AmqpNowaitArgumentAttribute(string namespaceName)
            : this(namespaceName, null) { }

        public AmqpNowaitArgumentAttribute(string namespaceName,
                                  string replacementExpression)
            : base(namespaceName)
        {
            m_replacementExpression = replacementExpression;
        }
    }

    ///<summary>This attribute, if placed on a method in IModel,
    ///causes the method to be interpreted as a factory method
    ///producing a protocol-specific implementation of a common
    ///content header interface.</summary>
    public class AmqpContentHeaderFactoryAttribute : Attribute
    {
        public string m_contentClass;

        public AmqpContentHeaderFactoryAttribute(string contentClass)
        {
            m_contentClass = contentClass;
        }
    }

    ///<summary>This attribute, if placed on a parameter in a
    ///content-carrying IModel method, causes it to be sent as part of
    ///the content header frame.</summary>
    public class AmqpContentHeaderMappingAttribute : Attribute
    {
        public AmqpContentHeaderMappingAttribute() { }
    }

    ///<summary>This attribute, if placed on a parameter in a
    ///content-carrying IModel method, causes it to be sent as part of
    ///the content body frame.</summary>
    public class AmqpContentBodyMappingAttribute : Attribute
    {
        public AmqpContentBodyMappingAttribute() { }
    }

    ///<summary>This attribute, placed on an IModel method, causes
    ///what would normally be an RPC, sent with ModelRpc, to be sent
    ///as if it were oneway, with ModelSend. The assumption that this
    ///is for a custom continuation (e.g. for BasicConsume/BasicCancel
    ///etc.)</summary>
    public class AmqpForceOneWayAttribute : Attribute
    {
        public AmqpForceOneWayAttribute() { }
    }
}
