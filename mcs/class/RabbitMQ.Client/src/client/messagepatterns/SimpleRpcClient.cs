// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2009 LShift Ltd., Cohesive Financial
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
//   Portions created by LShift Ltd are Copyright (C) 2007-2009 LShift
//   Ltd. Portions created by Cohesive Financial Technologies LLC are
//   Copyright (C) 2007-2009 Cohesive Financial Technologies
//   LLC. Portions created by Rabbit Technologies Ltd are Copyright
//   (C) 2007-2009 Rabbit Technologies Ltd.
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Threading;

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Client.MessagePatterns {
    ///<summary>Implements a simple RPC client.</summary>
    ///<remarks>
    ///<para>
    /// This class sends requests that can be processed by remote
    /// SimpleRpcServer instances.
    ///</para>
    ///<para>
    /// The basic pattern for accessing a remote service is to
    /// determine the exchange name and routing key needed for
    /// submissions of service requests, and to construct a
    /// SimpleRpcClient instance using that address. Once constructed,
    /// the various Call() and Cast() overloads can be used to send
    /// requests and receive the corresponding replies.
    ///</para>
    ///<example><code>
    ///	string queueName = "ServiceRequestQueue"; // See also Subscription ctors
    ///	using (IConnection conn = new ConnectionFactory()
    ///	                                .CreateConnection(serverAddress)) {
    ///	    using (IModel ch = conn.CreateModel()) {
    ///	        SimpleRpcClient client =
    ///	            new SimpleRpcClient(ch, queueName);
    ///	        client.TimeoutMilliseconds = 5000; // optional
    ///	
    ///	        /// ... make use of the various Call() overloads
    ///	    }
    ///	}
    ///</code></example>
    ///<para>
    /// Instances of this class do not themselves declare any
    /// resources (exchanges, queues or bindings). The Subscription we
    /// use for receiving RPC replies declares its own resources
    /// (usually a single queue), but if we are sending to an exchange
    /// other than one of the AMQP-standard mandated predefined
    /// exchanges, it is the user's responsibility to ensure that the
    /// exchange concerned exists (using IModel.ExchangeDeclare)
    /// before invoking Call() or Cast().
    ///</para>
    ///<para>
    /// This class implements only a few basic RPC message formats -
    /// to extend it with support for more formats, either subclass,
    /// or transcode the messages before transmission using the
    /// built-in byte[] format.
    ///</para>
    ///</remarks>
    ///<see cref="SimpleRpcServer"/>
    public class SimpleRpcClient: IDisposable {
	///<summary>This event is fired whenever Call() decides that a
	///timeout has occurred while waiting for a reply from the
	///service.</summary>
	///<remarks>
        /// See also OnTimedOut().
	///</remarks>
	public event EventHandler TimedOut;

	///<summary>This event is fired whenever Call() detects the
	///disconnection of the underlying Subscription while waiting
	///for a reply from the service.</summary>
	///<remarks>
        /// See also OnDisconnected(). Note that the sending of a
        /// request may result in OperationInterruptedException before
        /// the request is even sent.
	///</remarks>
	public event EventHandler Disconnected;

        protected IModel m_model;
        protected Subscription m_subscription;
        private PublicationAddress m_address;
        private int m_timeout;

        ///<summary>Retrieve the IModel this instance uses to communicate.</summary>
        public IModel Model { get { return m_model; } }

        ///<summary>Retrieve the Subscription that is used to receive
        ///RPC replies corresponding to Call() RPC requests. May be
        ///null.</summary>
        ///<remarks>
        ///<para>
        /// Upon construction, this property will be null. It is
        /// initialised by the protected virtual method
        /// EnsureSubscription upon the first call to Call(). Calls to
        /// Cast() do not initialise the subscription, since no
        /// replies are expected or possible when using Cast().
        ///</para>
        ///</remarks>
        public Subscription Subscription { get { return m_subscription; } }

        ///<summary>Retrieve or modify the address that will be used
        ///for the next Call() or Cast().</summary>
        ///<remarks>
        /// This address represents the service, i.e. the destination
        /// service requests should be published to. It can be changed
        /// at any time before a Call() or Cast() request is sent -
        /// the value at the time of the call is used by Call() and
        /// Cast().
        ///</remarks>
        public PublicationAddress Address {
            get { return m_address; }
            set { m_address = value; }
        }

	///<summary>Retrieve or modify the timeout (in milliseconds)
	///that will be used for the next Call().</summary>
	///<remarks>
        ///<para>
        /// This property defaults to
        /// System.Threading.Timeout.Infinite (i.e. -1). If it is set
        /// to any other value, Call() will only wait for the
        /// specified amount of time before returning indicating a
        /// timeout.
        ///</para>
        ///<para>
        /// See also TimedOut event and OnTimedOut().
        ///</para>
	///</remarks>
	public int TimeoutMilliseconds {
	    get { return m_timeout; }
	    set { m_timeout = value; }
	}

        ///<summary>Construct an instance with no configured
        ///Address. The Address property must be set before Call() or
        ///Cast() are called.</summary>
        public SimpleRpcClient(IModel model)
            : this(model, (PublicationAddress) null) {}

        ///<summary>Construct an instance that will deliver to the
        ///default exchange (""), with routing key equal to the passed
        ///in queueName, thereby delivering directly to a named queue
        ///on the AMQP server.</summary>
        public SimpleRpcClient(IModel model, string queueName)
            : this(model, new PublicationAddress(ExchangeType.Direct, "", queueName)) {}

        ///<summary>Construct an instance that will deliver to the
        ///named and typed exchange, with the given routing
        ///key.</summary>
        public SimpleRpcClient(IModel model, string exchange,
                               string exchangeType, string routingKey)
            : this(model, new PublicationAddress(exchangeType, exchange, routingKey)) {}

        ///<summary>Construct an instance that will deliver to the
        ///given address.</summary>
        public SimpleRpcClient(IModel model, PublicationAddress address)
        {
            m_model = model;
            m_address = address;
            m_subscription = null;
	    m_timeout = Timeout.Infinite;
        }

        ///<summary>Close the reply subscription associated with this instance, if any.</summary>
        ///<remarks>
        /// Simply delegates to calling Subscription.Close(). Clears
        /// the Subscription property, so that subsequent Call()s, if
        /// any, will re-initialize it to a fresh Subscription
        /// instance.
        ///</remarks>
        public void Close()
        {
            if (m_subscription != null) {
                m_subscription.Close();
                m_subscription = null;
            }
        }

        ///<summary>Should initialise m_subscription to be non-null
        ///and usable for fetching RPC replies from the service
        ///through the AMQP server.</summary>
        protected virtual void EnsureSubscription()
        {
            if (m_subscription == null) {
                m_subscription = new Subscription(m_model);
            }
        }

        ///<summary>Sends a "jms/stream-message"-encoded RPC request,
        ///and expects an RPC reply in the same format.</summary>
        ///<remarks>
        ///<para>
        /// The arguments passed in must be of types that are
        /// representable as JMS StreamMessage values, and so must the
        /// results returned from the service in its reply message.
        ///</para>
        ///<para>
        /// Calls OnTimedOut() and OnDisconnected() when a timeout or
        /// disconnection, respectively, is detected when waiting for
        /// our reply.
        ///</para>
        ///<para>
        /// Returns null if the request timed out or if we were
        /// disconnected before a reply arrived.
        ///</para>
        ///<para>
        /// The reply message, if any, is acknowledged to the AMQP
        /// server via Subscription.Ack().
        ///</para>
        ///</remarks>
        ///<see cref="IStreamMessageBuilder"/>
        ///<see cref="IStreamMessageReader"/>
        public virtual object[] Call(params object[] args)
        {
            IStreamMessageBuilder builder = new StreamMessageBuilder(m_model);
            builder.WriteObjects(args);
            IBasicProperties replyProperties;
            byte[] replyBody = Call((IBasicProperties) builder.GetContentHeader(),
                                    builder.GetContentBody(),
                                    out replyProperties);
            if (replyProperties == null) {
                return null;
            }
            if (replyProperties.ContentType != StreamMessageBuilder.MimeType) {
                throw new ProtocolViolationException
                    (string.Format("Expected reply of MIME type {0}; got {1}",
                                   StreamMessageBuilder.MimeType,
                                   replyProperties.ContentType));
            }
            IStreamMessageReader reader = new StreamMessageReader(replyProperties, replyBody);
            return reader.ReadObjects();
        }

        ///<summary>Sends a simple byte[] message, without any custom
        ///headers or properties.</summary>
        ///<remarks>
        ///<para>
        /// Delegates directly to Call(IBasicProperties, byte[]), and
        /// discards the properties of the received reply, returning
        /// only the body of the reply.
        ///</para>
        ///<para>
        /// Calls OnTimedOut() and OnDisconnected() when a timeout or
        /// disconnection, respectively, is detected when waiting for
        /// our reply.
        ///</para>
        ///<para>
        /// Returns null if the request timed out or if we were
        /// disconnected before a reply arrived.
        ///</para>
        ///<para>
        /// The reply message, if any, is acknowledged to the AMQP
        /// server via Subscription.Ack().
        ///</para>
        ///</remarks>
        public virtual byte[] Call(byte[] body)
        {
            BasicDeliverEventArgs reply = Call(null, body);
            return reply == null ? null : reply.Body;
        }

        ///<summary>Sends a byte[] message and IBasicProperties
        ///header, returning both the body and headers of the received
        ///reply.</summary>
        ///<remarks>
        ///<para>
        /// Sets the "replyProperties" outbound parameter to the
        /// properties of the received reply, and returns the byte[]
        /// body of the reply.
        ///</para>
        ///<para>
        /// Calls OnTimedOut() and OnDisconnected() when a timeout or
        /// disconnection, respectively, is detected when waiting for
        /// our reply.
        ///</para>
        ///<para>
        /// Both sets "replyProperties" to null and returns null when
        /// either the request timed out or we were disconnected
        /// before a reply arrived.
        ///</para>
        ///<para>
        /// The reply message, if any, is acknowledged to the AMQP
        /// server via Subscription.Ack().
        ///</para>
        ///</remarks>
        public virtual byte[] Call(IBasicProperties requestProperties,
                                   byte[] body,
                                   out IBasicProperties replyProperties)
        {
            BasicDeliverEventArgs reply = Call(requestProperties, body);
            if (reply == null) {
                replyProperties = null;
                return null;
            } else {
                replyProperties = reply.BasicProperties;
                return reply.Body;
            }
        }

        ///<summary>Sends a byte[]/IBasicProperties RPC request,
        ///returning full information about the delivered reply as a
        ///BasicDeliverEventArgs.</summary>
        ///<remarks>
        ///<para>
        /// This is the most general/lowest-level Call()-style method
        /// on SimpleRpcClient. It sets CorrelationId and ReplyTo on
        /// the request message's headers before transmitting the
        /// request to the service via the AMQP server. If the reply's
        /// CorrelationId does not match the request's CorrelationId,
        /// ProtocolViolationException will be thrown.
        ///</para>
        ///<para>
        /// Calls OnTimedOut() and OnDisconnected() when a timeout or
        /// disconnection, respectively, is detected when waiting for
        /// our reply.
        ///</para>
        ///<para>
        /// Returns null if the request timed out or if we were
        /// disconnected before a reply arrived.
        ///</para>
        ///<para>
        /// The reply message, if any, is acknowledged to the AMQP
        /// server via Subscription.Ack().
        ///</para>
        ///</remarks>
        ///<see cref="ProtocolViolationException"/>
        public virtual BasicDeliverEventArgs Call(IBasicProperties requestProperties, byte[] body)
        {
            EnsureSubscription();

            if (requestProperties == null) {
                requestProperties = m_model.CreateBasicProperties();
            }
            requestProperties.CorrelationId = Guid.NewGuid().ToString();
            requestProperties.ReplyTo = m_subscription.QueueName;

            Cast(requestProperties, body);
            return RetrieveReply(requestProperties.CorrelationId);
        }

        ///<summary>Retrieves the reply for the request with the given
        ///correlation ID from our internal Subscription.</summary>
        ///<remarks>
        /// Currently requires replies to arrive in the same order as
        /// the requests were sent out. Subclasses may override this
        /// to provide more sophisticated behaviour.
        ///</remarks>
        protected virtual BasicDeliverEventArgs RetrieveReply(string correlationId)
        {
            BasicDeliverEventArgs reply;
	    if (!m_subscription.Next(m_timeout, out reply)) {
		OnTimedOut();
		return null;
	    }

	    if (reply == null) {
		OnDisconnected();
		return null;
	    }

            if (reply.BasicProperties.CorrelationId != correlationId) {
                throw new ProtocolViolationException
                    (string.Format("Wrong CorrelationId in reply; expected {0}, got {1}",
                                   correlationId,
                                   reply.BasicProperties.CorrelationId));
            }

            m_subscription.Ack(reply);
            return reply;
        }

        ///<summary>Sends an asynchronous/one-way message to the
        ///service.</summary>
        public virtual void Cast(IBasicProperties requestProperties,
                                 byte[] body)
        {
            m_model.BasicPublish(Address,
                                 requestProperties,
                                 body);
        }

	///<summary>Signals that the configured timeout fired while
	///waiting for an RPC reply.</summary>
	///<remarks>
        /// Fires the TimedOut event.
	///</remarks>
	public virtual void OnTimedOut()
	{
	    if (TimedOut != null)
		TimedOut(this, null);
	}

	///<summary>Signals that the Subscription we use for receiving
	///our RPC replies was disconnected while we were
	///waiting.</summary>
	///<remarks>
        /// Fires the Disconnected event.
	///</remarks>
	public virtual void OnDisconnected()
	{
	    if (Disconnected != null)
		Disconnected(this, null);
	}

        ///<summary>Implement the IDisposable interface, permitting
        ///SimpleRpcClient instances to be used in using
        ///statements.</summary>
        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
