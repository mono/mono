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
using RabbitMQ.Client.Impl;

namespace RabbitMQ.Client
{
    ///<summary>Represents a TCP-addressable AMQP peer, including the
    ///protocol variant to use, and a host name and port
    ///number.</summary>
    ///<para>
    /// Some of the constructors take, as a convenience, a System.Uri
    /// instance representing an AMQP server address. The use of Uri
    /// here is not standardised - Uri is simply a convenient
    /// container for internet-address-like components. In particular,
    /// the Uri "Scheme" property is ignored: only the "Host" and
    /// "Port" properties are extracted.
    ///</para>
    public class AmqpTcpEndpoint
    {
        ///<summary>Indicates that the default port for the protocol should be used</summary>
        public const int UseDefaultPort = -1;

        private IProtocol m_protocol;
        ///<summary>Retrieve or set the IProtocol of this AmqpTcpEndpoint.</summary>
        public IProtocol Protocol
        {
            get { return m_protocol; }
            set { m_protocol = value; }
        }

        private string m_hostName;
        ///<summary>Retrieve or set the hostname of this AmqpTcpEndpoint.</summary>
        public string HostName
        {
            get { return m_hostName; }
            set { m_hostName = value; }
        }

        private int m_port;
        ///<summary>Retrieve or set the port number of this
        ///AmqpTcpEndpoint. A port number of -1 causes the default
        ///port number for the IProtocol to be used.</summary>
        public int Port
        {
            get { return (m_port == UseDefaultPort) ? m_protocol.DefaultPort : m_port; }
            set { m_port = value; }
        }


        private SslOption m_ssl;
        ///<summary>Retrieve the SSL options for this AmqpTcpEndpoint.
        ///If not set, null is returned</summary>
        public SslOption Ssl
        {
            get { return m_ssl; }
            set { m_ssl = value; }
        }

        ///<summary>Construct an AmqpTcpEndpoint with the given
        ///IProtocol, hostname, port number and ssl option. If the port 
        ///number is -1, the default port number for the IProtocol 
        ///will be used.</summary>
        public AmqpTcpEndpoint(IProtocol protocol, string hostName, int portOrMinusOne, SslOption ssl)
        {
            m_protocol = protocol;
            m_hostName = hostName;
            m_port = portOrMinusOne;
            m_ssl = ssl;
        }

        ///<summary>Construct an AmqpTcpEndpoint with the given
        ///IProtocol, hostname, and port number. If the port number is
        ///-1, the default port number for the IProtocol will be
        ///used.</summary>
        public AmqpTcpEndpoint(IProtocol protocol, string hostName, int portOrMinusOne) :
            this(protocol, hostName, portOrMinusOne, new SslOption())
        {
        }

        ///<summary>Construct an AmqpTcpEndpoint with the given
        ///IProtocol and hostname, using the default port for the
        ///IProtocol.</summary>
        public AmqpTcpEndpoint(IProtocol protocol, string hostName) :
            this(protocol, hostName, -1)
        {
        }

        ///<summary>Construct an AmqpTcpEndpoint with the given
        ///IProtocol, "localhost" as the hostname, and using the
        ///default port for the IProtocol.</summary>
        public AmqpTcpEndpoint(IProtocol protocol) :
            this(protocol, "localhost", -1)
        {
        }

        ///<summary>Construct an AmqpTcpEndpoint with the given
        ///hostname and port number, using the IProtocol from
        ///Protocols.FromEnvironment(). If the port number is
        ///-1, the default port number for the IProtocol will be
        ///used.</summary>
        public AmqpTcpEndpoint(string hostName, int portOrMinusOne) :
            this(Protocols.FromEnvironment(), hostName, portOrMinusOne)
        {
        }

        ///<summary>Construct an AmqpTcpEndpoint with the given
        ///hostname, using the IProtocol from
        ///Protocols.FromEnvironment(), and the default port number of
        ///that IProtocol.</summary>
        public AmqpTcpEndpoint(string hostName) :
            this(Protocols.FromEnvironment(), hostName)
        {
        }

        ///<summary>Construct an AmqpTcpEndpoint with a hostname of
        ///"localhost", using the IProtocol from
        ///Protocols.FromEnvironment(), and the default port number of
        ///that IProtocol.</summary>
        public AmqpTcpEndpoint() :
            this(Protocols.FromEnvironment())
        {
        }

        ///<summary>Construct an AmqpTcpEndpoint with the given
        ///IProtocol, Uri and ssl options.</summary>
        ///<remarks>
        /// Please see the class overview documentation for
        /// information about the Uri format in use.
        ///</remarks>
        public AmqpTcpEndpoint(IProtocol protocol, Uri uri, SslOption ssl) :
            this(protocol, uri.Host, uri.Port, ssl)
        {
        }

        ///<summary>Construct an AmqpTcpEndpoint with the given
        ///IProtocol and Uri.</summary>
        ///<remarks>
        /// Please see the class overview documentation for
        /// information about the Uri format in use.
        ///</remarks>
        public AmqpTcpEndpoint(IProtocol protocol, Uri uri) :
            this(protocol, uri.Host, uri.Port)
        {
        }

        ///<summary>Construct an AmqpTcpEndpoint with the given
        ///Uri, using the IProtocol from
        ///Protocols.FromEnvironment().</summary>
        ///<remarks>
        /// Please see the class overview documentation for
        /// information about the Uri format in use.
        ///</remarks>
        public AmqpTcpEndpoint(Uri uri) :
            this(Protocols.FromEnvironment(), uri)
        {
        }

        ///<summary>Returns a URI-like string of the form
        ///amqp-PROTOCOL://HOSTNAME:PORTNUMBER</summary>
        ///<remarks>
        /// This method is intended mainly for debugging and logging use.
        ///</remarks>
        public override string ToString()
        {
            return "amqp-" + Protocol + "://" + HostName + ":" + Port;
        }

        ///<summary>Compares this instance by value (protocol,
        ///hostname, port) against another instance</summary>
        public override bool Equals(object obj)
        {
            AmqpTcpEndpoint other = obj as AmqpTcpEndpoint;
            if (other == null) return false;
            if (other.Protocol != Protocol) return false;
            if (other.HostName != HostName) return false;
            if (other.Port != Port) return false;
            return true;
        }

        ///<summary>Implementation of hash code depending on protocol,
        ///hostname and port, to line up with the implementation of
        ///Equals()</summary>
        public override int GetHashCode()
        {
            return
                Protocol.GetHashCode() ^
                HostName.GetHashCode() ^
                Port;
        }

        ///<summary>Construct an instance from a protocol and an
        ///address in "hostname:port" format.</summary>
        ///<remarks>
        /// If the address string passed in contains ":", it is split
        /// into a hostname and a port-number part. Otherwise, the
        /// entire string is used as the hostname, and the port-number
        /// is set to -1 (meaning the default number for the protocol
        /// variant specified).
        ///</remarks>
        public static AmqpTcpEndpoint Parse(IProtocol protocol, string address) {
            int index = address.IndexOf(':');
            if (index == -1) {
                return new AmqpTcpEndpoint(protocol, address, -1);
            } else {
                string portStr = address.Substring(index + 1).Trim();
                int portNum = (portStr.Length == 0) ? -1 : int.Parse(portStr);
                return new AmqpTcpEndpoint(protocol,
                                           address.Substring(0, index),
                                           portNum);
            }
        }

        ///<summary>Splits the passed-in string on ",", and passes the
        ///substrings to AmqpTcpEndpoint.Parse()</summary>
        ///<remarks>
        /// Accepts a string of the form "hostname:port,
        /// hostname:port, ...", where the ":port" pieces are
        /// optional, and returns a corresponding array of
        /// AmqpTcpEndpoints.
        ///</remarks>
        public static AmqpTcpEndpoint[] ParseMultiple(IProtocol protocol, string addresses) {
            string[] partsArr = addresses.Split(new char[] { ',' });
            ArrayList results = new ArrayList();
            foreach (string partRaw in partsArr) {
                string part = partRaw.Trim();
                if (part.Length > 0) {
                    results.Add(Parse(protocol, part));
                }
            }
            return (AmqpTcpEndpoint[]) results.ToArray(typeof(AmqpTcpEndpoint));
        }
    }
}
