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
using System.IO;
using System.Net.Sockets;
using System.Text;

using RabbitMQ.Util;

namespace RabbitMQ.Client.Impl
{
    public class SocketFrameHandler_0_9 : IFrameHandler
    {
        public const int WSAEWOULDBLOCK = 10035; 
        // ^^ System.Net.Sockets.SocketError doesn't exist in .NET 1.1

        public AmqpTcpEndpoint m_endpoint;
        public TcpClient m_socket;
        public NetworkBinaryReader m_reader;
        public NetworkBinaryWriter m_writer;

        public SocketFrameHandler_0_9(AmqpTcpEndpoint endpoint)
        {
            m_endpoint = endpoint;
            m_socket = new TcpClient();
            m_socket.Connect(endpoint.HostName, endpoint.Port);
            // disable Nagle's algorithm, for more consistently low latency 
            m_socket.NoDelay = true;

            Stream netstream = m_socket.GetStream();
            if (endpoint.Ssl.Enabled) {
                try {
                    netstream = SslHelper.TcpUpgrade(netstream, endpoint.Ssl);
                } catch (Exception) {
                    Close();
                    throw;
                }
            }
            m_reader = new NetworkBinaryReader(new BufferedStream(netstream));
            m_writer = new NetworkBinaryWriter(new BufferedStream(netstream));
        }

        public AmqpTcpEndpoint Endpoint
        {
            get
            {
                return m_endpoint;
            }
        }

        public int Timeout
        {
            get
            {
                return m_socket.ReceiveTimeout;
            }
            set
            {
                m_socket.ReceiveTimeout = value;
            }
        }

        public void SendHeader()
        {
            lock (m_writer)
            {
                m_writer.Write(Encoding.ASCII.GetBytes("AMQP"));
                m_writer.Write((byte)1);
                m_writer.Write((byte)1);
                m_writer.Write((byte)m_endpoint.Protocol.MajorVersion);
                m_writer.Write((byte)m_endpoint.Protocol.MinorVersion);
                m_writer.Flush();
            }
        }

        public Frame ReadFrame()
        {
            lock (m_reader)
            {
                return Frame.ReadFrom(m_reader);
            }
        }

        public void WriteFrame(Frame frame)
        {
            lock (m_writer)
            {
                frame.WriteTo(m_writer);
                m_writer.Flush();
                //Console.WriteLine("OUTBOUND:");
                //DebugUtil.DumpProperties(frame, Console.Out, 2);
            }
        }

        public void Close()
        {
            m_socket.Close();
        }
    }
}
