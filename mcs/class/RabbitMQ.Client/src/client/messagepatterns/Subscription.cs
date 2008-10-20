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

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;

namespace RabbitMQ.Client.MessagePatterns {
    ///<summary>Manages a subscription to a queue or exchange.</summary>
    ///<remarks>
    ///<para>
    /// This convenience class abstracts away from much of the detail
    /// involved in receiving messages from a queue or an exchange.
    ///</para>
    ///<para>
    /// Once created, the Subscription consumes from a queue (using a
    /// QueueingBasicConsumer). Received deliveries can be retrieved
    /// by calling Next(), or by using the Subscription as an
    /// IEnumerator in, for example, a foreach loop.
    ///</para>
    ///<para>
    /// See the documentation for Bind() and for the various overloads
    /// of the constructor for the various styles of binding and
    /// subscription that are available.
    ///</para>
    ///<para>
    /// Note that if the "noAck" option is enabled (which it is by
    /// default), then received deliveries are automatically acked
    /// within the server before they are even transmitted across the
    /// network to us. Calling Ack() on received events will always do
    /// the right thing: if "noAck" is enabled, nothing is done on an
    /// Ack() call, and if "noAck" is disabled, IModel.BasicAck() is
    /// called with the correct parameters.
    ///</para>
    ///</remarks>
    public class Subscription: IEnumerable, IEnumerator, IDisposable {
        protected IModel m_model;
        protected ushort m_ticket;

        ///<summary>Retrieve the IModel our subscription is carried by.</summary>
        public IModel Model { get { return m_model; } }
        ///<summary>Retrieve the ticket we use for all our server operations.</summary>
        public ushort Ticket { get { return m_ticket; } }

        protected string m_queueName;
        protected QueueingBasicConsumer m_consumer;
        protected string m_consumerTag;
        protected bool m_noAck;
        protected bool m_shouldDelete;

        ///<summary>Retrieve the queue name we have subscribed to. May
        ///be a server-generated name, depending on how the
        ///Subscription was constructed.</summary>
        public string QueueName { get { return m_queueName; } }
        ///<summary>Retrieve the IBasicConsumer that is receiving the
        ///messages from the server for us. Normally, you will not
        ///need to access this property - use Next() and friends
        ///instead.</summary>
        public IBasicConsumer Consumer { get { return m_consumer; } }
        ///<summary>Retrieve the consumer-tag that this subscription
        ///is using. Will usually be a server-generated
        ///name.</summary>
        public string ConsumerTag { get { return m_consumerTag; } }
        ///<summary>Returns true if we are in "noAck" mode, where
        ///calls to Ack() will be no-ops, and where the server acks
        ///messages before they are delivered to us. Returns false if
        ///we are in a mode where calls to Ack() are required, and
        ///where such calls will actually send an acknowledgement
        ///message across the network to the server.</summary>
        public bool NoAck { get { return m_noAck; } }

        protected BasicDeliverEventArgs m_latestEvent;

        ///<summary>Returns the most recent value returned by Next(),
        ///or null when either no values have been retrieved yet, or
        ///the most recent value has already been Ack()ed. See also
        ///the documentation for Ack().</summary>
        public BasicDeliverEventArgs LatestEvent { get { return m_latestEvent; } }

        ///<summary>Creates a new Subscription in "noAck" mode,
        ///consuming from a fresh, autodelete, anonymous queue. The
        ///name of the queue can be retrieved using the QueueName
        ///property of the Subscription. After creating the queue, the
        ///queue is bound to the named exchange, using Bind() with the
        ///given routingKey bind parameter.</summary>
        public Subscription(IModel model, ushort ticket,
                            string exchangeName, string exchangeType, string routingKey)
            : this(model, ticket)
        {
            Bind(exchangeName, exchangeType, routingKey);
        }

        ///<summary>Creates a new Subscription in "noAck" mode,
        ///consuming from a fresh, autodelete, anonymous queue. The
        ///name of the queue can be retrieved using the QueueName
        ///property of the Subscription.</summary>
        public Subscription(IModel model, ushort ticket)
            : this(model, ticket, null) {}

        ///<summary>Creates a new Subscription in "noAck" mode,
        ///consuming from a named queue. If the queueName parameter is
        ///null or the empty-string, creates a fresh, autodelete,
        ///anonymous queue; otherwise, the queue is declared using
        ///IModel.QueueDeclare() before IModel.BasicConsume() is
        ///called. After declaring the queue and starting the
        ///consumer, the queue is bound to the named exchange, using
        ///Bind() with the given routingKey bind parameter.</summary>
        public Subscription(IModel model, ushort ticket, string queueName,
                            string exchangeName, string exchangeType, string routingKey)
            : this(model, ticket, queueName)
        {
            Bind(exchangeName, exchangeType, routingKey);
        }

        ///<summary>Creates a new Subscription in "noAck" mode,
        ///consuming from a named queue. If the queueName parameter is
        ///null or the empty-string, creates a fresh, autodelete,
        ///anonymous queue; otherwise, the queue is declared using
        ///IModel.QueueDeclare() before IModel.BasicConsume() is
        ///called.</summary>
        public Subscription(IModel model, ushort ticket, string queueName)
            : this(model, ticket, queueName, true) {}

        ///<summary>Creates a new Subscription, with full control over
        ///both "noAck" mode and the name of the queue (which, if null
        ///or the empty-string, will be a fresh autodelete queue, as
        ///for the other constructor overloads). After declaring the
        ///queue and starting the consumer, the queue is bound to the
        ///named exchange, using Bind() with the given routingKey bind
        ///parameter.</summary>
        public Subscription(IModel model, ushort ticket, string queueName, bool noAck,
                            string exchangeName, string exchangeType, string routingKey)
            : this(model, ticket, queueName, noAck)
        {
            Bind(exchangeName, exchangeType, routingKey);
        }

        ///<summary>Creates a new Subscription, with full control over
        ///both "noAck" mode and the name of the queue (which, if null
        ///or the empty-string, will be a fresh autodelete queue, as
        ///for the other constructor overloads).</summary>
        public Subscription(IModel model, ushort ticket, string queueName, bool noAck)
        {
            m_model = model;
            m_ticket = ticket;
            if (queueName == null || queueName.Equals("")) {
                m_queueName = m_model.QueueDeclare(ticket);
                m_shouldDelete = true;
            } else {
                m_queueName = m_model.QueueDeclare(ticket, queueName);
                m_shouldDelete = false;
            }
            m_consumer = new QueueingBasicConsumer(m_model);
            m_consumerTag = m_model.BasicConsume(m_ticket, m_queueName, m_noAck, null, m_consumer);
            m_latestEvent = null;
        }

        ///<summary>Closes this Subscription, cancelling the consumer
        ///record in the server. If an anonymous, autodelete queue
        ///(i.e., one with a server-generated name) was created during
        ///construction of the Subscription, this method also deletes
        ///the created queue (which is an optimisation: autodelete
        ///queues will be deleted when the IModel closes in any
        ///case).</summary>
        public void Close()
        {
            try {
                if (m_consumer != null) {
                    m_model.BasicCancel(m_consumerTag);
                }
                if (m_shouldDelete) {
                    m_shouldDelete = false;
                    // We set m_shouldDelete false before attempting
                    // the delete, because trying twice is worse than
                    // trying once and failing.
                    m_model.QueueDelete(m_ticket, m_queueName, false, false, false);
                }
            } catch (OperationInterruptedException) {
                // We don't mind, here.
            }
            m_consumer = null;
            m_consumerTag = null;
        }

        ///<summary>Causes the queue to which we have subscribed to be
        ///bound to an exchange. Uses IModel.ExchangeDeclare and
        ///IModel.QueueBind to (a) ensure the exchange exists, and (b)
        ///link the exchange to our queue.</summary>
        ///<remarks>
        ///<para>
        /// This method is called by some of the overloads of the
        /// Subscription constructor.
        ///</para>
        ///<para>
        /// Calling Bind() multiple times to bind to multiple
        /// exchanges, or to bind to a single exchange more than once
        /// with a different routingKey, is perfectly
        /// acceptable. Calling Bind() twice with exactly the same
        /// arguments is permitted and idempotent. For details, see
        /// the AMQP specification.
        ///</para>
        ///</remarks>
        public void Bind(string exchangeName, string exchangeType, string routingKey)
        {
            m_model.ExchangeDeclare(m_ticket, exchangeName, exchangeType);
            m_model.QueueBind(m_ticket, m_queueName, exchangeName, routingKey, false, null);
        }

        ///<summary>If LatestEvent is non-null, passes it to
        ///Ack(BasicDeliverEventArgs). Causes LatestEvent to become
        ///null.</summary>
        public void Ack()
        {
            if (m_latestEvent != null) {
                Ack(m_latestEvent);
            }
        }

        ///<summary>If we are not in "noAck" mode, calls
        ///IModel.BasicAck with the delivery-tag from the passed in
        ///event; otherwise, sends nothing to the server. In both
        ///cases, if the passed-in event is the same as LatestEvent
        ///(by pointer comparison), sets LatestEvent to
        ///null.</summary>
        ///<remarks>
        /// Make sure that this method is only called with events that
        /// originated from this Subscription - other usage will have
        /// unpredictable results.
        ///</remarks>
        public void Ack(BasicDeliverEventArgs evt)
        {
            if (evt == null) {
                return;
            }

            if (!m_noAck) {
                m_model.BasicAck(evt.DeliveryTag, false);
            }

            if (evt == m_latestEvent) {
                m_latestEvent = null;
            }
        }

        ///<summary>Retrieves the next incoming delivery in our
        ///subscription queue.</summary>
	///<remarks>
	///<para>
	/// Returns null when the end of the stream is reached and on
        /// every subsequent call. End-of-stream can arise through the
        /// action of the Subscription.Close() method, or through the
        /// closure of the IModel or its underlying IConnection.
	///</para>
	///<para>
	/// Updates LatestEvent to the value returned.
	///</para>
	///<para>
	/// Does not acknowledge any deliveries at all (but in "noAck"
        /// mode, the server will have auto-acknowledged each event
        /// before it is even sent across the wire to us).
	///</para>
	///</remarks>
        public BasicDeliverEventArgs Next()
        {
            try {
                if (m_consumer == null) {
                    // Closed!
                    throw new InvalidOperationException();
                }
                m_latestEvent = (BasicDeliverEventArgs) m_consumer.Queue.Dequeue();
            } catch (EndOfStreamException) {
                m_latestEvent = null;
            }
	    return m_latestEvent;
        }

        ///<summary>Retrieves the next incoming delivery in our
        ///subscription queue, or times out after a specified number
        ///of milliseconds.</summary>

	///<remarks>
	///<para>
	/// Returns false only if the timeout expires before either a
	/// delivery appears or the end-of-stream is reached. If false
	/// is returned, the out parameter "result" is set to null,
	/// but LatestEvent is not updated.
	///</para>
	///<para>
	/// Returns true to indicate a delivery or the end-of-stream.
	///</para>
	///<para>
	/// If a delivery is already waiting in the queue, or one
	/// arrives before the timeout expires, it is removed from the
	/// queue and placed in the "result" out parameter. If the
	/// end-of-stream is detected before the timeout expires,
	/// "result" is set to null.
	///</para>
	///<para>
	/// Whenever this method returns true, it updates LatestEvent
	/// to the value placed in "result" before returning.
	///</para>
	///<para>
	/// End-of-stream can arise through the action of the
	/// Subscription.Close() method, or through the closure of the
	/// IModel or its underlying IConnection.
	///</para>
	///<para>
	/// This method does not acknowledge any deliveries at all
        /// (but in "noAck" mode, the server will have
        /// auto-acknowledged each event before it is even sent across
        /// the wire to us).
	///</para>
        ///<para>
	/// A timeout of -1 (i.e. System.Threading.Timeout.Infinite)
	/// will be interpreted as a command to wait for an
	/// indefinitely long period of time for an item or the end of
	/// the stream to become available. Usage of such a timeout is
	/// equivalent to calling Next() with no arguments (modulo
	/// predictable method signature differences).
        ///</para>
	///</remarks>
        public bool Next(int millisecondsTimeout, out BasicDeliverEventArgs result)
        {
            try {
                if (m_consumer == null) {
                    // Closed!
                    throw new InvalidOperationException();
                }
		object qValue;
		if (!m_consumer.Queue.Dequeue(millisecondsTimeout, out qValue)) {
		    result = null;
		    return false;
		}
                m_latestEvent = (BasicDeliverEventArgs) qValue;
            } catch (EndOfStreamException) {
                m_latestEvent = null;
            }
	    result = m_latestEvent;
	    return true;
        }

        ///<summary>Implementation of the IEnumerable interface, for
        ///permitting Subscription to be used in foreach
        ///loops.</summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        ///<summary>Implementation of the IEnumerator interface, for
        ///permitting Subscription to be used in foreach
        ///loops.</summary>
        ///<remarks>
        ///<para>
        /// As per the IEnumerator interface definition, throws
        /// InvalidOperationException if LatestEvent is null.
        ///</para>
        ///<para>
        /// Does not acknowledge any deliveries at all. Ack() must be
        /// called explicitly on received deliveries.
        ///</para>
        ///</remarks>
        object IEnumerator.Current {
            get {
                if (m_latestEvent == null) {
                    throw new InvalidOperationException();
                }
                return m_latestEvent;
            }
        }

        ///<summary>Implementation of the IEnumerator interface, for
        ///permitting Subscription to be used in foreach
        ///loops.</summary>
        ///<remarks>
        ///<para>
        /// Does not acknowledge any deliveries at all. Ack() must be
        /// called explicitly on received deliveries.
        ///</para>
        ///</remarks>
        bool IEnumerator.MoveNext()
        {
            return Next() != null;
        }

        ///<summary>Dummy implementation of the IEnumerator interface,
        ///for permitting Subscription to be used in foreach loops;
        ///Reset()ting a Subscription doesn't make sense, so this
        ///method always throws InvalidOperationException.</summary>
        void IEnumerator.Reset()
        {
            // It really doesn't make sense to try to reset a subscription.
            throw new InvalidOperationException("Subscription.Reset() does not make sense");
        }

        ///<summary>Implementation of the IDisposable interface,
        ///permitting Subscription to be used in using
        ///statements. Simply calls Close().</summary>
        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
