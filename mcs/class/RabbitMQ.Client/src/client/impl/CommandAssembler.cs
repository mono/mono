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
using System.Collections;

using RabbitMQ.Util;

// We use spec version 0-9 for common constants such as frame types
// and the frame end byte, since they don't vary *within the versions
// we support*. Obviously we may need to revisit this if that ever
// changes.
using CommonFraming = RabbitMQ.Client.Framing.v0_9;
using System.Diagnostics;

namespace RabbitMQ.Client.Impl
{
    public enum AssemblyState
    {
        ExpectingMethod,
        ExpectingContentHeader,
        ExpectingContentBody,
        Complete
    }

    public class CommandAssembler
    {
        public AbstractProtocolBase m_protocol;
        public AssemblyState m_state;
        public Command m_command;
        public ulong m_remainingBodyBytes;

        public CommandAssembler(AbstractProtocolBase protocol)
        {
            m_protocol = protocol;
            Reset();
        }

        private void Reset()
        {
            m_state = AssemblyState.ExpectingMethod;
            m_command = new Command();
            m_remainingBodyBytes = 0;
        }

        private Command CompletedCommand()
        {
            if (m_state == AssemblyState.Complete)
            {
                Command result = m_command;
                Reset();
                return result;
            }
            else
            {
                return null;
            }
        }

        private void UpdateContentBodyState()
        {
            m_state = (m_remainingBodyBytes > 0)
                ? AssemblyState.ExpectingContentBody
                : AssemblyState.Complete;
        }
        
        public Command HandleFrame(Frame f)
        {
            switch (m_state)
            {
                case AssemblyState.ExpectingMethod:
                    {
                        if (f.Type != CommonFraming.Constants.FrameMethod)
                        {
                            throw new UnexpectedFrameException(f);
                        }
                        m_command.m_method = m_protocol.DecodeMethodFrom(f.GetReader());
                        m_state = m_command.m_method.HasContent
                            ? AssemblyState.ExpectingContentHeader
                            : AssemblyState.Complete;
                        return CompletedCommand();
                    }
                case AssemblyState.ExpectingContentHeader:
                    {
                        if (f.Type != CommonFraming.Constants.FrameHeader)
                        {
                            throw new UnexpectedFrameException(f);
                        }
                        NetworkBinaryReader reader = f.GetReader();
                        m_command.m_header = m_protocol.DecodeContentHeaderFrom(reader);
                        m_remainingBodyBytes = m_command.m_header.ReadFrom(f.Channel, reader);
                        UpdateContentBodyState();
                        return CompletedCommand();
                    }
                case AssemblyState.ExpectingContentBody:
                    {
                        if (f.Type != CommonFraming.Constants.FrameBody)
                        {
                            throw new UnexpectedFrameException(f);
                        }
                        byte[] fragment = f.Payload;
                        m_command.AppendBodyFragment(fragment);
                        if ((ulong)fragment.Length > m_remainingBodyBytes)
                        {
                            throw new MalformedFrameException
                                (string.Format("Overlong content body received - {0} bytes remaining, {1} bytes received",
                                               m_remainingBodyBytes,
                                               fragment.Length));
                        }
                        m_remainingBodyBytes -= (ulong)fragment.Length;
                        UpdateContentBodyState();
                        return CompletedCommand();
                    }
                case AssemblyState.Complete:
                default:
                    Trace.Fail(string.Format(
                        "Received frame in invalid state {0}; {1}",
                        m_state, 
                        f));
                    return null;
            }
        }
    }
}
