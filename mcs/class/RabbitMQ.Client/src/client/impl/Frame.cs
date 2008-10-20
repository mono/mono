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

using RabbitMQ.Util;
using RabbitMQ.Client.Exceptions;

// We use spec version 0-9 for common constants such as frame types,
// error codes, and the frame end byte, since they don't vary *within
// the versions we support*. Obviously we may need to revisit this if
// that ever changes.
using CommonFraming = RabbitMQ.Client.Framing.v0_9;

namespace RabbitMQ.Client.Impl
{
    public class Frame
    {
        public int m_type;
        public int m_channel;
        public byte[] m_payload;

        public int Type { get { return m_type; } }
        public int Channel { get { return m_channel; } }
        public byte[] Payload { get { return m_payload; } }

        public MemoryStream m_accumulator;

        public Frame() { }

        public Frame(int type, int channel)
        {
            m_type = type;
            m_channel = channel;
            m_payload = null;
            m_accumulator = new MemoryStream();
        }

        public Frame(int type, int channel, byte[] payload)
        {
            m_type = type;
            m_channel = channel;
            m_payload = payload;
            m_accumulator = null;
        }
        
        public static Frame ReadFrom(NetworkBinaryReader reader)
        {
            int type;
            int channel;

            type = reader.ReadByte();

            if (type == 'A')
            {
                // Probably an AMQP protocol header, otherwise meaningless
                ProcessProtocolHeader(reader);
            }

            channel = reader.ReadUInt16();
            int payloadSize = reader.ReadInt32(); // FIXME - throw exn on unreasonable value
            byte[] payload = reader.ReadBytes(payloadSize);
            if (payload.Length != payloadSize)
            {
                // Early EOF.
                throw new MalformedFrameException("Short frame - expected " +
                                                  payloadSize + " bytes, got " +
                                                  payload.Length + " bytes");
            }

            int frameEndMarker = reader.ReadByte();
            if (frameEndMarker != CommonFraming.Constants.FrameEnd)
            {
                throw new MalformedFrameException("Bad frame end marker: " + frameEndMarker);
            }

            return new Frame(type, channel, payload);
        }

        public static void ProcessProtocolHeader(NetworkBinaryReader reader)
        {
            try
            {
                byte b1 = reader.ReadByte();
                byte b2 = reader.ReadByte();
                byte b3 = reader.ReadByte();
                if (b1 != 'M' || b2 != 'Q' || b3 != 'P')
                {
                    throw new MalformedFrameException("Invalid AMQP protocol header from server");
                }

                int transportHigh = reader.ReadByte();
                int transportLow = reader.ReadByte();
                int serverMajor = reader.ReadByte();
                int serverMinor = reader.ReadByte();
                throw new PacketNotRecognizedException(transportHigh,
                                                       transportLow,
                                                       serverMajor,
                                                       serverMinor);
            }
            catch (EndOfStreamException)
            {
                // Ideally we'd wrap the EndOfStreamException in the
                // MalformedFrameException, but unfortunately the
                // design of MalformedFrameException's superclass,
                // ProtocolViolationException, doesn't permit
                // this. Fortunately, the call stack in the
                // EndOfStreamException is largely irrelevant at this
                // point, so can safely be ignored.
                throw new MalformedFrameException("Invalid AMQP protocol header from server");
            }
        }

        public void FinishWriting()
        {
            if (m_accumulator != null)
            {
                m_payload = m_accumulator.ToArray();
                m_accumulator = null;
            }
        }

        public void WriteTo(NetworkBinaryWriter writer)
        {
            FinishWriting();
            writer.Write((byte)m_type);
            writer.Write((ushort)m_channel);
            writer.Write((uint)m_payload.Length);
            writer.Write((byte[])m_payload);
            writer.Write((byte)CommonFraming.Constants.FrameEnd);
        }

        public NetworkBinaryReader GetReader()
        {
            return new NetworkBinaryReader(new MemoryStream(m_payload));
        }

        public NetworkBinaryWriter GetWriter()
        {
            return new NetworkBinaryWriter(m_accumulator);
        }

        public override string ToString()
        {
            return base.ToString() + string.Format("(type={0}, channel={1}, {2} bytes of payload)",
                                                   Type,
                                                   Channel,
                                                   Payload == null
                                                     ? "(null)"
                                                     : Payload.Length.ToString());
        }
    }
}
