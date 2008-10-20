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
using System.Net;
using System.Net.Sockets;
using System.Collections;

using RabbitMQ.Client.Impl;
using RabbitMQ.Client.Exceptions;

namespace RabbitMQ.Client
{
    ///<summary>Main entry point to the RabbitMQ .NET AMQP client
    ///API. Constructs IConnection instances.</summary>
    ///<remarks>
    ///<para>
    /// A simple example of connecting to a broker:
    ///</para>
    ///<example><code>
    ///     ConnectionFactory factory = new ConnectionFactory();
    ///     //
    ///     // The next three lines are optional:
    ///     factory.Parameters.UserName = ConnectionParameters.DefaultUser;
    ///     factory.Parameters.Password = ConnectionParameters.DefaultPass;
    ///     factory.Parameters.VirtualHost = ConnectionParameters.DefaultVHost;
    ///     //
    ///     IProtocol protocol = Protocols.DefaultProtocol;
    ///     IConnection conn = factory.CreateConnection(protocol, hostName, portNumber);
    ///     //
    ///     IModel ch = conn.CreateModel();
    ///     ushort ticket = ch.AccessRequest("/data");
    ///     //
    ///     // ... use ch's IModel methods ...
    ///     //
    ///     ch.Close(200, "Closing the channel");
    ///     conn.Close(200, "Closing the connection");
    ///</code></example>
    ///<para>
    /// Please see also the API overview and tutorial in the User Guide.
    ///</para>
    ///<para>
    /// Some of the static methods described below take, as a
    /// convenience, a System.Uri instance representing an AMQP server
    /// address. The use of Uri here is not standardised - Uri is
    /// simply a convenient container for internet-address-like
    /// components. In particular, the Uri "Scheme" property is
    /// ignored: only the "Host" and "Port" properties are extracted.
    ///</para>
    ///</remarks>
    public class ConnectionFactory
    {
        private ConnectionParameters m_parameters = new ConnectionParameters();
        ///<summary>Retrieve the parameters this factory uses to
        ///construct IConnection instances.</summary>
        public ConnectionParameters Parameters
        {
            get
            {
                return m_parameters;
            }
        }

        ///<summary>Constructs a ConnectionFactory with default values
        ///for Parameters.</summary>
        public ConnectionFactory()
        {
        }

        protected virtual IConnection FollowRedirectChain
            (int maxRedirects,
             IDictionary connectionAttempts,
             IDictionary connectionErrors,
             ref AmqpTcpEndpoint[] mostRecentKnownHosts,
             AmqpTcpEndpoint endpoint)
        {
            AmqpTcpEndpoint candidate = endpoint;
            try {
                while (true) {
                    int attemptCount =
                        connectionAttempts.Contains(candidate)
                        ? (int) connectionAttempts[candidate]
                        : 0;
                    connectionAttempts[candidate] = attemptCount + 1;
                    bool insist = attemptCount >= maxRedirects;

                    try {
                        IProtocol p = candidate.Protocol;
                        IFrameHandler fh = p.CreateFrameHandler(candidate);
                        // At this point, we may be able to create
                        // and fully open a successful connection,
                        // in which case we're done, and the
                        // connection should be returned.
                        return p.CreateConnection(m_parameters, insist, fh);
                    } catch (RedirectException re) {
                        if (insist) {
                            // We've been redirected, but we insisted that
                            // we shouldn't be redirected! Well-behaved
                            // brokers should never do this.
                            string message = string.Format("Server {0} ignored 'insist' flag, redirecting us to {1}",
                                                           candidate,
                                                           re.Host);
                            throw new ProtocolViolationException(message);
                        } else {
                            // We've been redirected. Follow this new link
                            // in the chain, by setting
                            // mostRecentKnownHosts (in case the chain
                            // runs out), and updating candidate for the
                            // next time round the loop.
                            connectionErrors[candidate] = re;
                            mostRecentKnownHosts = re.KnownHosts;
                            candidate = re.Host;
                        }
                    }
                }
            } catch (Exception e) {
                connectionErrors[candidate] = e;
                return null;
            }
        }

        protected virtual IConnection CreateConnection(int maxRedirects,
                                                       IDictionary connectionAttempts,
                                                       IDictionary connectionErrors,
                                                       params AmqpTcpEndpoint[] endpoints)
        {
            foreach (AmqpTcpEndpoint endpoint in endpoints)
            {
                AmqpTcpEndpoint[] mostRecentKnownHosts = new AmqpTcpEndpoint[0];
                // ^^ holds a list of known-hosts that came back with
                // a connection.redirect. If, once we reach the end of
                // a chain of redirects, we still haven't managed to
                // get a usable connection, we recurse on
                // mostRecentKnownHosts, trying each of those in
                // turn. Finally, if neither the initial
                // chain-of-redirects for the current endpoint, nor
                // the chains-of-redirects for each of the
                // mostRecentKnownHosts gives us a usable connection,
                // we give up on this particular endpoint, and
                // continue with the foreach loop, trying the
                // remainder of the array we were given.
                IConnection conn = FollowRedirectChain(maxRedirects,
                                                       connectionAttempts,
                                                       connectionErrors,
                                                       ref mostRecentKnownHosts,
                                                       endpoint);
                if (conn != null) {
                    return conn;
                }

                // Connection to this endpoint failed at some point
                // down the redirection chain - either the first
                // entry, or one of the re.Host values from subsequent
                // RedirectExceptions. We recurse into
                // mostRecentKnownHosts, to see if one of those is
                // suitable.
                if (mostRecentKnownHosts.Length > 0) {
                    // Only bother recursing if we know of some
                    // hosts. If we were to recurse with no endpoints
                    // in the array, we'd stomp on
                    // mostRecentException, which makes debugging
                    // connectivity problems needlessly more
                    // difficult.
                    conn = CreateConnection(maxRedirects,
                                            connectionAttempts,
                                            connectionErrors,
                                            mostRecentKnownHosts);
                    if (conn != null) {
                        return conn;
                    }
                }
            }
            return null;
        }

        ///<summary>Create a connection to the first available
        ///endpoint in the list provided. Up to a maximum of
        ///maxRedirects broker-originated redirects are permitted for
        ///each endpoint tried.</summary>
        public virtual IConnection CreateConnection(int maxRedirects,
                                                    params AmqpTcpEndpoint[] endpoints)
        {
            IDictionary connectionAttempts = new Hashtable();
            IDictionary connectionErrors = new Hashtable();
            IConnection conn = CreateConnection(maxRedirects,
                                                connectionAttempts,
                                                connectionErrors,
                                                endpoints);
            if (conn != null) {
                return conn;
            }
            throw new BrokerUnreachableException(connectionAttempts, connectionErrors);
        }

        ///<summary>Create a connection to the first available
        ///endpoint in the list provided. No broker-originated
        ///redirects are permitted.</summary>
        public virtual IConnection CreateConnection(params AmqpTcpEndpoint[] endpoints)
        {
            return CreateConnection(0, endpoints);
        }

        ///<summary>Create a connection to the endpoint specified.</summary>
        ///<exception cref="ArgumentException"/>
        public IConnection CreateConnection(IProtocol version,
                                            string hostName,
                                            int portNumber)
        {
            return CreateConnection(new AmqpTcpEndpoint[] {
                                        new AmqpTcpEndpoint(version, hostName, portNumber)
                                    });
        }

        ///<summary>Create a connection to the endpoint specified. The
        ///port used is the default for the protocol.</summary>
        ///<exception cref="ArgumentException"/>
        public IConnection CreateConnection(IProtocol version, string hostName)
        {
            return CreateConnection(version, hostName, -1);
        }

        ///<summary>Create a connection to the endpoint specified.</summary>
        ///<remarks>
        /// Please see the class overview documentation for
        /// information about the Uri format in use.
        ///</remarks>
        ///<exception cref="ArgumentException"/>
        public IConnection CreateConnection(IProtocol version, Uri uri)
        {
            return CreateConnection(version, uri.Host, uri.Port);
        }

        ///<summary>Create a connection to the endpoint specified,
        ///with the IProtocol from
        ///Protocols.FromEnvironment().</summary>
        ///<remarks>
        /// Please see the class overview documentation for
        /// information about the Uri format in use.
        ///</remarks>
        public IConnection CreateConnection(Uri uri)
        {
            return CreateConnection(Protocols.FromEnvironment(), uri.Host, uri.Port);
        }

        ///<summary>Create a connection to the host (and optional
        ///port) specified, with the IProtocol from
        ///Protocols.FromEnvironment(). The format of the address
        ///string is the same as that accepted by
        ///AmqpTcpEndpoint.Parse().</summary>
        public IConnection CreateConnection(string address) {
            return CreateConnection(AmqpTcpEndpoint.Parse(Protocols.FromEnvironment(), address));
        }
    }
}
