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

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Client.MessagePatterns {
    ///<summary>Implements a simple RPC service, responding to
    ///requests received via a Subscription.</summary>
    ///<remarks>
    ///<para>
    /// This class interprets requests such as those sent by instances
    /// of SimpleRpcClient.
    ///</para>
    ///<para>
    /// The basic pattern for implementing a service is to subclass
    /// SimpleRpcServer, overriding HandleCall and HandleCast as
    /// appropriate, and then to create a Subscription object for
    /// receiving requests from clients, and start an instance of the
    /// SimpleRpcServer subclass with the Subscription.
    ///</para>
    ///<example><code>
    ///	string queueName = "ServiceRequestQueue"; // See also Subscription ctors
    ///	using (IConnection conn = new ConnectionFactory()
    ///	                                .CreateConnection(serverAddress)) {
    ///	    using (IModel ch = conn.CreateModel()) {
    ///	        ushort ticket = ch.AccessRequest("/data");
    ///	        Subscription sub = new Subscription(ch, ticket, queueName);
    ///	        new MySimpleRpcServerSubclass(sub).MainLoop();
    ///	    }
    ///	}
    ///</code></example>
    ///<para>
    /// Note that this class itself does not declare any resources
    /// (exchanges, queues or bindings). The Subscription we use for
    /// receiving RPC requests should have already declared all the
    /// resources we need. See the Subscription constructors and the
    /// Subscription.Bind method.
    ///</para>
    ///<para>
    /// If you are implementing a service that responds to
    /// "jms/stream-message"-formatted requests (as implemented by
    /// RabbitMQ.Client.Content.IStreamMessageReader), override
    /// HandleStreamMessageCall. Otherwise, override HandleSimpleCall
    /// or HandleCall as appropriate. Asynchronous, one-way requests
    /// are dealt with by HandleCast etc.
    ///</para>
    ///<para>
    /// Every time a request is successfully received and processed
    /// within the server's MainLoop, the request message is Ack()ed
    /// using Subscription.Ack before the next request is
    /// retrieved. This causes the Subscription object to take care of
    /// acknowledging receipt and processing of the request message.
    ///</para>
    ///<para>
    /// If transactional service is enabled, via SetTransactional(),
    /// then after every successful ProcessRequest, IModel.TxCommit is
    /// called. Making use of transactional service has effects on all
    /// parts of the application that share an IModel instance,
    /// completely changing the style of interaction with the AMQP
    /// server. For this reason, it is initially disabled, and must be
    /// explicitly enabled with a call to SetTransactional(). Please
    /// see the documentation for SetTransactional() for details.
    ///</para>
    ///<para>
    /// To stop a running RPC server, call Close(). This will in turn
    /// Close() the Subscription, which will cause MainLoop() to
    /// return to its caller.
    ///</para>
    ///<para>
    /// Unless overridden, ProcessRequest examines properties in the
    /// request content header, and uses them to dispatch to one of
    /// the Handle[...]() methods. See the documentation for
    /// ProcessRequest and each Handle[...] method for details.
    ///</para>
    ///</remarks>
    ///<see cref="SimpleRpcClient"/>
    public class SimpleRpcServer: IDisposable {
        protected Subscription m_subscription;
        private bool m_transactional;

        ///<summary>Returns true if we are in "transactional" mode, or
        ///false if we are not.</summary>
        public bool Transactional { get { return m_transactional; } }

        ///<summary>Create, but do not start, an instance that will
        ///receive requests via the given Subscription.</summary>
        ///<remarks>
	///<para>
	/// The instance is initially in non-transactional mode. See
	/// SetTransactional().
	///</para>
	///<para>
	/// Call MainLoop() to start the request-processing loop.
	///</para>
        ///</remarks>
        public SimpleRpcServer(Subscription subscription)
        {
            m_subscription = subscription;
            m_transactional = false;
        }

        ///<summary>Shut down the server, causing MainLoop() to return
        ///to its caller.</summary>
        ///<remarks>
	/// Acts by calling Close() on the server's Subscription object.
        ///</remarks>
        public void Close()
        {
            m_subscription.Close();
        }

	///<summary>Enables transactional mode.</summary>
	///<remarks>
	///<para>
	/// Once enabled, transactional mode is not only enabled for
	/// all users of the underlying IModel instance, but cannot be
	/// disabled without shutting down the entire IModel (which
	/// involves shutting down all the services depending on it,
	/// and should not be undertaken lightly).
	///</para>
	///<para>
	/// This method calls IModel.TxSelect, every time it is
	/// called. (TxSelect is idempotent, so this is harmless.)
	///</para>
	///</remarks>
	public void SetTransactional() {
	    m_subscription.Model.TxSelect();
	    m_transactional = true;
        }

        ///<summary>Enters the main loop of the RPC service.</summary>
        ///<remarks>
	///<para>
	/// Retrieves requests repeatedly from the service's
	/// subscription. Each request is passed to
	/// ProcessRequest. Once ProcessRequest returns, the request
	/// is acknowledged via Subscription.Ack(). If transactional
	/// mode is enabled, TxCommit is then called. Finally, the
	/// loop begins again.
	///</para>
	///<para>
	/// Runs until the subscription ends, which happens either as
	/// a result of disconnection, or of a call to Close().
	///</para>
        ///</remarks>
        public void MainLoop()
        {
            foreach (BasicDeliverEventArgs evt in m_subscription) {
                ProcessRequest(evt);
                m_subscription.Ack();
                if (m_transactional) {
                    m_subscription.Model.TxCommit();
                }
            }
        }

        ///<summary>Process a single request received from our
        ///subscription.</summary>
        ///<remarks>
	///<para>
	/// If the request's properties contain a non-null, non-empty
	/// CorrelationId string (see IBasicProperties), it is assumed
	/// to be a two-way call, requiring a response. The ReplyTo
	/// header property is used as the reply address (via
	/// PublicationAddress.Parse, unless that fails, in which case it
	/// is treated as a simple queue name), and the request is
	/// passed to HandleCall().
	///</para>
	///<para>
	/// If the CorrelationId is absent or empty, the request is
	/// treated as one-way asynchronous event, and is passed to
	/// HandleCast().
	///</para>
	///<para>
	/// Usually, overriding HandleCall(), HandleCast(), or one of
	/// their delegates is sufficient to implement a service, but
	/// in some cases overriding ProcessRequest() is
	/// required. Overriding ProcessRequest() gives the
	/// opportunity to implement schemes for detecting interaction
	/// patterns other than simple request/response or one-way
	/// communication.
	///</para>
        ///</remarks>
        public virtual void ProcessRequest(BasicDeliverEventArgs evt)
        {
            IBasicProperties properties = evt.BasicProperties;
            if (properties.ReplyTo != null && properties.ReplyTo != "") {
                // It's a request.

                PublicationAddress replyAddress = PublicationAddress.Parse(properties.ReplyTo);
                if (replyAddress == null) {
                    replyAddress = new PublicationAddress(ExchangeType.Direct,
                                                          "",
                                                          properties.ReplyTo);
                }

                IBasicProperties replyProperties;
                byte[] reply = HandleCall(evt.Redelivered,
                                          properties,
                                          evt.Body,
                                          out replyProperties);
                if (replyProperties == null) {
                    replyProperties = m_subscription.Model.CreateBasicProperties();
                }

                replyProperties.CorrelationId = properties.CorrelationId;
                m_subscription.Model.BasicPublish(m_subscription.Ticket,
                                                  replyAddress,
                                                  replyProperties,
                                                  reply);
            } else {
                // It's an asynchronous message.
                HandleCast(evt.Redelivered, properties, evt.Body);
            }
        }

        ///<summary>Called by HandleCall and HandleCast when a
        ///"jms/stream-message" request is received.</summary>
        ///<remarks>
	///<para>
	/// The args array contains the values decoded by HandleCall
	/// or HandleCast.
	///</para>
	///<para>
	/// The replyWriter parameter will be null if we were called
	/// from HandleCast, in which case a reply is not expected or
	/// possible, or non-null if we were called from
	/// HandleCall. Use the methods of replyWriter in this case to
	/// assemble your reply, which will be sent back to the remote
	/// caller.
	///</para>
	///<para>
	/// This default implementation does nothing, which
	/// effectively sends back an empty reply to any and all
	/// remote callers.
	///</para>
        ///</remarks>
        public virtual void HandleStreamMessageCall(IStreamMessageBuilder replyWriter,
                                                    bool isRedelivered,
                                                    IBasicProperties requestProperties,
                                                    object[] args)
        {
            // Override to do something with the request.
        }

        ///<summary>Called by ProcessRequest(), this is the most
        ///general method that handles RPC-style requests.</summary>
        ///<remarks>
	///<para>
	/// This method should map requestProperties and body to
	/// replyProperties and the returned byte array.
	///</para>
	///<para>
	/// The default implementation checks
	/// requestProperties.ContentType, and if it is
	/// "jms/stream-message" (i.e. the current value of
	/// StreamMessageBuilder.MimeType), parses it using
	/// StreamMessageReader and delegates to
	/// HandleStreamMessageCall before encoding and returning the
	/// reply. If the ContentType is any other value, the request
	/// is passed to HandleSimpleCall instead.
	///</para>
	///<para>
	/// The isRedelivered flag is true when the server knows for
	/// sure that it has tried to send this request previously
	/// (although not necessarily to this application). It is not
	/// a reliable indicator of previous receipt, however - the
	/// only claim it makes is that a delivery attempt was made,
	/// not that the attempt succeeded. Be careful if you choose
	/// to use the isRedelivered flag.
	///</para>
        ///</remarks>
        public virtual byte[] HandleCall(bool isRedelivered,
                                         IBasicProperties requestProperties,
                                         byte[] body,
                                         out IBasicProperties replyProperties)
        {
            if (requestProperties.ContentType == StreamMessageBuilder.MimeType) {
                IStreamMessageReader r = new StreamMessageReader(requestProperties, body);
                IStreamMessageBuilder w = new StreamMessageBuilder(m_subscription.Model);
                HandleStreamMessageCall(w,
                                        isRedelivered,
                                        requestProperties,
                                        r.ReadObjects());
                replyProperties = (IBasicProperties) w.GetContentHeader();
                return w.GetContentBody();
            } else {
                return HandleSimpleCall(isRedelivered,
                                        requestProperties,
                                        body,
                                        out replyProperties);
            }
        }

        ///<summary>Called by the default HandleCall() implementation
        ///as a fallback.</summary>
        ///<remarks>
	/// If the MIME ContentType of the request did not match any
	/// of the types specially recognised
	/// (e.g. "jms/stream-message"), this method is called instead
	/// with the raw bytes of the request. It should fill in
	/// replyProperties (or set it to null) and return a byte
	/// array to send back to the remote caller as a reply
	/// message.
        ///</remarks>
        public virtual byte[] HandleSimpleCall(bool isRedelivered,
                                               IBasicProperties requestProperties,
                                               byte[] body,
                                               out IBasicProperties replyProperties)
        {
            // Override to do something with the request.
            replyProperties = null;
            return null;
        }

        ///<summary>Called by ProcessRequest(), this is the most
        ///general method that handles asynchronous, one-way
        ///requests.</summary>
        ///<remarks>
	///<para>
	/// The default implementation checks
	/// requestProperties.ContentType, and if it is
	/// "jms/stream-message" (i.e. the current value of
	/// StreamMessageBuilder.MimeType), parses it using
	/// StreamMessageReader and delegates to
	/// HandleStreamMessageCall, passing in null as the
	/// replyWriter parameter to indicate that no reply is desired
	/// or possible. If the ContentType is any other value, the
	/// request is passed to HandleSimpleCast instead.
	///</para>
	///<para>
	/// The isRedelivered flag is true when the server knows for
	/// sure that it has tried to send this request previously
	/// (although not necessarily to this application). It is not
	/// a reliable indicator of previous receipt, however - the
	/// only claim it makes is that a delivery attempt was made,
	/// not that the attempt succeeded. Be careful if you choose
	/// to use the isRedelivered flag.
	///</para>
        ///</remarks>
        public virtual void HandleCast(bool isRedelivered,
                                       IBasicProperties requestProperties,
                                       byte[] body)
        {
            if (requestProperties.ContentType == StreamMessageBuilder.MimeType) {
                IStreamMessageReader r = new StreamMessageReader(requestProperties, body);
                HandleStreamMessageCall(null,
                                        isRedelivered,
                                        requestProperties,
                                        r.ReadObjects());
            } else {
                HandleSimpleCast(isRedelivered,
                                 requestProperties,
                                 body);
            }
        }

        ///<summary>Called by the default HandleCast() implementation
        ///as a fallback.</summary>
        ///<remarks>
	/// If the MIME ContentType of the request did not match any
	/// of the types specially recognised
	/// (e.g. "jms/stream-message"), this method is called instead
	/// with the raw bytes of the request.
        ///</remarks>
        public virtual void HandleSimpleCast(bool isRedelivered,
                                             IBasicProperties requestProperties,
                                             byte[] body)
        {
            // Override to do something with the request.
        }

        ///<summary>Implement the IDisposable interface, permitting
        ///SimpleRpcServer instances to be used in using
        ///statements.</summary>
        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
