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
    ///<summary>Information about the reason why a particular model,
    ///session, or connection was destroyed.</summary>
    ///<remarks>
    ///The ClassId and Initiator properties should be
    ///used to determine the originator of the shutdown event.
    ///</remarks>
    public class ShutdownEventArgs : EventArgs
    {
        private readonly ShutdownInitiator m_initiator;
        private readonly ushort m_replyCode;
        private readonly string m_replyText;
        private readonly ushort m_classId;
        private readonly ushort m_methodId;
        private readonly object m_cause;

        ///<summary>Returns the source of the shutdown event: either
        ///the application, the library, or the remote peer.</summary>
        public ShutdownInitiator Initiator { get { return m_initiator; } }

        ///<summary>One of the standardised AMQP reason codes. See
        ///RabbitMQ.Client.Framing.*.Constants.</summary>
        public ushort ReplyCode { get { return m_replyCode; } }

        ///<summary>Informative human-readable reason text.</summary>
        public string ReplyText { get { return m_replyText; } }

        ///<summary>AMQP content-class ID, or 0 if none.</summary>
        public ushort ClassId { get { return m_classId; } }

        ///<summary>AMQP method ID within a content-class, or 0 if none.</summary>
        public ushort MethodId { get { return m_methodId; } }

        ///<summary>Object causing the shutdown, or null if none.</summary>
        public object Cause { get { return m_cause; } }

        ///<summary>Construct a ShutdownEventArgs with the given
        ///parameters, 0 for ClassId and MethodId, and a null
        ///Cause.</summary>
        public ShutdownEventArgs(ShutdownInitiator initiator,
                                 ushort replyCode,
                                 string replyText)
            : this(initiator,
                replyCode,
                replyText,
                null)
        { }

        ///<summary>Construct a ShutdownEventArgs with the given
        ///parameters and 0 for ClassId and MethodId.</summary>
        public ShutdownEventArgs(ShutdownInitiator initiator,
                                 ushort replyCode,
                                 string replyText,
                 object cause)
            : this(initiator,
                replyCode,
                replyText,
                0,
                0,
                cause)
        { }

        ///<summary>Construct a ShutdownEventArgs with the given
        ///parameters and a null cause.</summary>
        public ShutdownEventArgs(ShutdownInitiator initiator,
                                 ushort replyCode,
                                 string replyText,
                                 ushort classId,
                                 ushort methodId)
            : this(initiator,
                    replyCode,
                    replyText,
                    classId,
                    methodId,
                    null)
        { }

        ///<summary>Construct a ShutdownEventArgs with the given
        ///parameters.</summary>
        public ShutdownEventArgs(ShutdownInitiator initiator,
                                 ushort replyCode,
                                 string replyText,
                                 ushort classId,
                                 ushort methodId,
                                 object cause)
        {
            m_initiator = initiator;
            m_replyCode = replyCode;
            m_replyText = replyText;
            m_classId = classId;
            m_methodId = methodId;
            m_cause = cause;
        }

        ///<summary>Override ToString to be useful for debugging.</summary>
        public override string ToString()
        {
            return "AMQP close-reason, initiated by " + m_initiator +
                ", code=" + m_replyCode +
                ", text=\"" + m_replyText + "\"" +
                ", classId=" + m_classId +
                ", methodId=" + m_methodId +
                ", cause=" + m_cause;
        }
    }
}
