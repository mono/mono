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
using RabbitMQ.Client;
using RabbitMQ.Client.Impl;
using RabbitMQ.Util;

namespace RabbitMQ.Client.Framing.Impl.v0_9 {
    public abstract class ProtocolBase: AbstractProtocolBase {
	public override bool DefaultSuppressAccessRequest { get { return false; } }

        public override IFrameHandler CreateFrameHandler(AmqpTcpEndpoint endpoint) {
            return new SocketFrameHandler_0_9(endpoint);
        }

        public override IModel CreateModel(ISession session) {
            return new Model(session);
        }

        public override IConnection CreateConnection(ConnectionParameters parameters,
                                                     bool insist,
                                                     IFrameHandler frameHandler)
        {
            return new Connection(parameters, insist, frameHandler);
        }

        public override void CreateConnectionClose(ushort reasonCode,
                                                   string reasonText,
                                                   out Command request,
                                                   out int replyClassId,
                                                   out int replyMethodId)
        {
            request = new Command(new RabbitMQ.Client.Framing.Impl.v0_9.ConnectionClose(reasonCode,
                                                                                        reasonText,
                                                                                        0, 0));
            replyClassId = RabbitMQ.Client.Framing.Impl.v0_9.ConnectionCloseOk.ClassId;
            replyMethodId = RabbitMQ.Client.Framing.Impl.v0_9.ConnectionCloseOk.MethodId;
        }

        public override void CreateChannelClose(ushort reasonCode,
                                                string reasonText,
                                                out Command request,
                                                out int replyClassId,
                                                out int replyMethodId)
        {
            request = new Command(new RabbitMQ.Client.Framing.Impl.v0_9.ChannelClose(reasonCode,
                                                                                     reasonText,
                                                                                     0, 0)); 
            replyClassId = RabbitMQ.Client.Framing.Impl.v0_9.ChannelCloseOk.ClassId;
            replyMethodId = RabbitMQ.Client.Framing.Impl.v0_9.ChannelCloseOk.MethodId;                                                                                     
        }
    }
}
