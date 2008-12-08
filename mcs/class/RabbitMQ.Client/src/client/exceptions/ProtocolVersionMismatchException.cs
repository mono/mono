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

namespace RabbitMQ.Client.Exceptions {
    ///<summary>Thrown to indicate that the peer does not support the
    ///wire protocol version we requested immediately after opening
    ///the TCP socket.</summary>
    public class ProtocolVersionMismatchException: System.Net.ProtocolViolationException {
        private readonly int m_clientMajor;
        private readonly int m_clientMinor;
        private readonly int m_serverMajor;
        private readonly int m_serverMinor;

        ///<summary>The client's AMQP specification major version.</summary>
        public int ClientMajor { get { return m_clientMajor; } }
        ///<summary>The client's AMQP specification minor version.</summary>
        public int ClientMinor { get { return m_clientMinor; } }
        ///<summary>The peer's AMQP specification major version.</summary>
        public int ServerMajor { get { return m_serverMajor; } }
        ///<summary>The peer's AMQP specification minor version.</summary>
        public int ServerMinor { get { return m_serverMinor; } }

        ///<summary>Fills the new instance's properties with the values passed in.</summary>
        public ProtocolVersionMismatchException(int clientMajor,
                                                int clientMinor,
                                                int serverMajor,
                                                int serverMinor)
            : base("AMQP server protocol negotiation failure: server version "+
                   serverMajor + "-" + serverMinor +
                   ", client version " + clientMajor + "-" + clientMinor)
        {
            m_clientMajor = clientMajor;
            m_clientMinor = clientMinor;
            m_serverMajor = serverMajor;
            m_serverMinor = serverMinor;
        }
    }
}
