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
using System.IO;
using System.Collections;

using RabbitMQ.Util;

// We use spec version 0-9 for common constants such as frame types
// and the frame end byte, since they don't vary *within the versions
// we support*. Obviously we may need to revisit this if that ever
// changes.
using CommonFraming = RabbitMQ.Client.Framing.v0_9;
using System.Diagnostics;
using System.Net;

namespace RabbitMQ.Client.Impl {
    public class Command {
        private static readonly byte[] m_emptyByteArray = new byte[0];

        // EmptyContentBodyFrameSize, 8 = 1 + 2 + 4 + 1
        // - 1 byte of frame type
        // - 2 bytes of channel number
        // - 4 bytes of frame payload length
        // - 1 byte of payload trailer FrameEnd byte
        public const int EmptyContentBodyFrameSize = 8;

        static Command() {
            CheckEmptyContentBodyFrameSize();
        }

        public static void CheckEmptyContentBodyFrameSize() {
            Frame f = new Frame(CommonFraming.Constants.FrameBody, 0, m_emptyByteArray);
            MemoryStream stream = new MemoryStream();
            NetworkBinaryWriter writer = new NetworkBinaryWriter(stream);
            f.WriteTo(writer);
            long actualLength = stream.Length;

            if (EmptyContentBodyFrameSize != actualLength) {
                string message = 
                    string.Format("EmptyContentBodyFrameSize is incorrect - defined as {0} where the computed value is in fact {1}.",
                                  EmptyContentBodyFrameSize,
                                  actualLength);
                throw new ProtocolViolationException(message);
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        public MethodBase m_method;
        public ContentHeaderBase m_header;
        public byte[] m_body0;
        public ArrayList m_bodyN;

        public MethodBase Method { get { return m_method; } }
        public ContentHeaderBase Header { get { return m_header; } }
        public byte[] Body { get { return ConsolidateBody(); } }

        public Command(): this(null, null, null) {}

        public Command(MethodBase method): this(method, null, null) {}

        public Command(MethodBase method, ContentHeaderBase header, byte[] body) {
            m_method = method;
            m_header = header;
            m_body0 = body;
            m_bodyN = null;
        }

        public byte[] ConsolidateBody() {
            if (m_bodyN == null) {
                return (m_body0 == null) ? m_emptyByteArray : m_body0;
            } else {
                int totalSize = m_body0.Length;
                foreach (byte[] fragment in m_bodyN) {
                    totalSize += fragment.Length;
                }
                byte[] result = new byte[totalSize];
                Array.Copy(m_body0, 0, result, 0, m_body0.Length);
                int offset = m_body0.Length;
                foreach (byte[] fragment in m_bodyN) {
                    Array.Copy(fragment, 0, result, offset, fragment.Length);
                    offset += fragment.Length;
                }
                m_body0 = result;
                m_bodyN = null;
                return m_body0;
            }
        }

        public void AppendBodyFragment(byte[] fragment) {
            if (m_body0 == null) {
                m_body0 = fragment;
            } else {
                if (m_bodyN == null) {
                    m_bodyN = new ArrayList();
                }
                m_bodyN.Add(fragment);
            }
        }

        public void Transmit(int channelNumber, ConnectionBase connection) {
            Frame frame = new Frame(CommonFraming.Constants.FrameMethod, channelNumber);
            NetworkBinaryWriter writer = frame.GetWriter();
            writer.Write((ushort) m_method.ProtocolClassId);
            writer.Write((ushort) m_method.ProtocolMethodId);
            MethodArgumentWriter argWriter = new MethodArgumentWriter(writer);
            m_method.WriteArgumentsTo(argWriter);
            argWriter.Flush();
            connection.WriteFrame(frame);

            if (m_method.HasContent) {
                byte[] body = Body;

                frame = new Frame(CommonFraming.Constants.FrameHeader, channelNumber);
                writer = frame.GetWriter();
                writer.Write((ushort) m_header.ProtocolClassId);
                m_header.WriteTo(writer, (ulong) body.Length);
                connection.WriteFrame(frame);

                int frameMax = (int) Math.Min(int.MaxValue, connection.FrameMax);
                int bodyPayloadMax = (frameMax == 0)
                    ? body.Length
                    : frameMax - EmptyContentBodyFrameSize;
                for (int offset = 0; offset < body.Length; offset += bodyPayloadMax) {
                    int remaining = body.Length - offset;

                    frame = new Frame(CommonFraming.Constants.FrameBody, channelNumber);
                    writer = frame.GetWriter();
                    writer.Write(body, offset,
                                 (remaining < bodyPayloadMax) ? remaining : bodyPayloadMax);
                    connection.WriteFrame(frame);
                }
            }
        }
    }
}
