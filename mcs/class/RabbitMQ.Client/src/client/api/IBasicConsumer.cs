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

namespace RabbitMQ.Client
{
    ///<summary>Consumer interface for Basic content-class. Used to
    ///receive messages from a queue by subscription.</summary>
    ///<remarks>
    ///<para>
    /// See IModel.BasicConsume, IModel.BasicCancel.
    ///</para>
    ///<para>
    /// Note that the "Handle*" methods run in the connection's
    /// thread! Consider using QueueingBasicConsumer, which uses a
    /// SharedQueue instance to safely pass received messages across
    /// to user threads.
    ///</para>
    ///</remarks>
    public interface IBasicConsumer
    {
        ///<summary>Retrieve the IModel this consumer is associated
        ///with, for use in acknowledging received messages, for
        ///instance.</summary>
        IModel Model { get; }

        ///<summary>Called upon successful registration of the
        ///consumer with the broker.</summary>
        void HandleBasicConsumeOk(string consumerTag);

        ///<summary>Called upon successful deregistration of the
        ///consumer from the broker.</summary>
        void HandleBasicCancelOk(string consumerTag);

        ///<summary>Called when the model shuts down.</summary>
        void HandleModelShutdown(IModel model, ShutdownEventArgs reason);

        ///<summary>Called each time a message arrives for this consumer.</summary>
        ///<remarks>
        ///Be aware that acknowledgement may be required. See IModel.BasicAck.
        ///</remarks>
        void HandleBasicDeliver(string consumerTag,
                                ulong deliveryTag,
                                bool redelivered,
                                string exchange,
                                string routingKey,
                                IBasicProperties properties,
                                byte[] body);
    }
}
