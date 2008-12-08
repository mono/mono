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

namespace RabbitMQ.Client
{
    ///<summary>Useful default/base implementation of
    ///IBasicConsumer. Subclass and override HandleBasicDeliver in
    ///application code.</summary>
    ///<remarks>
    /// Note that the "Handle*" methods run in the connection's
    /// thread! Consider using QueueingBasicConsumer, which uses a
    /// SharedQueue instance to safely pass received messages across
    /// to user threads, or
    /// RabbitMQ.Client.MessagePatterns.Subscription, which manages
    /// resource declaration and binding in addition to providing a
    /// thread-safe interface.
    ///</remarks>
    public class DefaultBasicConsumer : IBasicConsumer
    {
        private IModel m_model = null;
        private string m_consumerTag = null;
        private bool m_running = false;
        private ShutdownEventArgs m_shutdownReason = null;

        ///<summary>Retrieve the IModel instance this consumer is
        ///registered with.</summary>
        public IModel Model
        {
            get { return m_model; }
            set { m_model = value; }
        }

        ///<summary>Retrieve the consumer tag this consumer is
        ///registered as; to be used when discussing this consumer
        ///with the server, for instance with
        ///IModel.BasicCancel().</summary>
        public string ConsumerTag
        {
            get { return m_consumerTag; }
            set { m_consumerTag = value; }
        }

        ///<summary>Returns true while the consumer is registered and
        ///expecting deliveries from the broker.</summary>
        public bool IsRunning
        {
            get { return m_running; }
        }

        ///<summary>If our IModel shuts down, this property will
        ///contain a description of the reason for the
        ///shutdown. Otherwise it will contain null. See
        ///ShutdownEventArgs.</summary>
        public ShutdownEventArgs ShutdownReason
        {
            get { return m_shutdownReason; }
        }

        ///<summary>Default constructor.</summary>
        public DefaultBasicConsumer() { }

        ///<summary>Constructor which sets the Model property to the
        ///given value.</summary>
        public DefaultBasicConsumer(IModel model)
        {
            m_model = model;
        }

        ///<summary>Default implementation - overridable in subclasses.</summary>
        ///<remarks>
        /// This default implementation simply sets the IsRunning
        /// property to false, and takes no further action.
        ///</remarks>
        public virtual void OnCancel()
        {
            m_running = false;
        }



        ///<summary>Default implementation - sets the ConsumerTag
        ///property and sets IsRunning to true.</summary>
        public virtual void HandleBasicConsumeOk(string consumerTag)
        {
            ConsumerTag = consumerTag;
            m_running = true;
        }

        ///<summary>Default implementation - calls OnCancel().</summary>
        public virtual void HandleBasicCancelOk(string consumerTag)
        {
            OnCancel();
        }

        ///<summary>Default implementation - sets ShutdownReason and
        ///calls OnCancel().</summary>
        public virtual void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            m_shutdownReason = reason;
            OnCancel();
        }

        ///<summary>Default implementation - override in subclasses.</summary>
        ///<remarks>
        /// Does nothing with the passed in information. Note that in
        /// particular, some delivered messages may require
        /// acknowledgement via IModel.BasicAck; the implementation of
        /// this method in this class does NOT acknowledge such
        /// messages.
        ///</remarks>
        public virtual void HandleBasicDeliver(string consumerTag,
                                               ulong deliveryTag,
                                               bool redelivered,
                                               string exchange,
                                               string routingKey,
                                               IBasicProperties properties,
                                               byte[] body)
        {
            // Nothing to do here.
        }
    }
}
