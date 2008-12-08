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

using RabbitMQ.Client;

namespace RabbitMQ.Client.Impl
{
    /// <summary> Instances of RedirectException are thrown by
    /// ConnectionBase.Open when the broker returns a
    /// Connection.Redirect method. The ConnectionFactory catches the
    /// exception and arranges for the redirect to take
    /// place. </summary>
    public class RedirectException: Exception
    {
        public AmqpTcpEndpoint m_host;
        public AmqpTcpEndpoint[] m_knownHosts;

        ///<summary>The host we were redirected to. Try connecting to
        ///this first.</summary>
        public AmqpTcpEndpoint Host { get { return m_host; } }

        ///<summary>Other hosts the broker knows about. If connecting
        ///to Host fails, try some of these.</summary>
        public AmqpTcpEndpoint[] KnownHosts { get { return m_knownHosts; } }

        ///<summary>Uses AmqpTcpEndpoint.Parse and .ParseMultiple to
        ///convert the strings, and then passes them to the other
        ///overload of the constructor.</summary>
        public RedirectException(IProtocol protocol,
                                 string host,
                                 string knownHosts)
            : this(ParseHost(protocol, host),
                   AmqpTcpEndpoint.ParseMultiple(protocol, knownHosts))
        {}

        public RedirectException(AmqpTcpEndpoint host, AmqpTcpEndpoint[] knownHosts)
            : base(string.Format("The connection.open attempt was redirected to host '{0}'",
                                 host))
        {
            m_host = host;
            m_knownHosts = knownHosts;
        }

        ///<summary>Conservative extension to the spec, supporting
        ///multiple interfaces in the "host" field of the
        ///connection.redirect method.</summary>
        ///<remarks>
        /// We use ParseMultiple rather than Parse, because a single
        /// host may have multiple interfaces. The spec doesn't say
        /// what to do here, so this is a conservative extension (as
        /// in, if a broker only returns a single address, we handle
        /// that fine). We arbitrarily take the first element of the
        /// array.
        ///</remarks>
        public static AmqpTcpEndpoint ParseHost(IProtocol protocol, string host) {
            AmqpTcpEndpoint[] addresses = AmqpTcpEndpoint.ParseMultiple(protocol, host);
            if (addresses.Length == 0) {
                return AmqpTcpEndpoint.Parse(protocol, "");
                // ^^ effectively, a (kind of useless) default or null result
            } else {
                return addresses[0];
            }
        }
    }
}
