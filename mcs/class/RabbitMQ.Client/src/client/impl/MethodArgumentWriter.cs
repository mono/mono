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
using System.Text;
using System.Collections;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Util;

namespace RabbitMQ.Client.Impl
{
    public class MethodArgumentWriter
    {
        private NetworkBinaryWriter m_writer;
        private bool m_needBitFlush;
        private byte m_bitAccumulator;
        private int m_bitMask;
        
        public MethodArgumentWriter(NetworkBinaryWriter writer)
        {
            m_writer = writer;
            if (!m_writer.BaseStream.CanSeek)
            {
                //FIXME: Consider throwing System.IO.IOException
                // with message indicating that the specified writer does not support Seeking

                // Only really a problem if we try to write a table,
                // but complain anyway. See WireFormatting.WriteTable
                throw new NotSupportedException("Cannot write method arguments to non-positionable stream");
            }
            ResetBitAccumulator();
        }

        public NetworkBinaryWriter BaseWriter { get { return m_writer; } }

        private void ResetBitAccumulator()
        {
            m_needBitFlush = false;
            m_bitAccumulator = 0;
            m_bitMask = 1;
        }

        private void BitFlush()
        {
            if (m_needBitFlush)
            {
                m_writer.Write((byte)m_bitAccumulator);
                ResetBitAccumulator();
            }
        }

        public void WriteOctet(byte val)
        {
            BitFlush();
            WireFormatting.WriteOctet(m_writer, val);
        }

        public void WriteShortstr(string val)
        {
            BitFlush();
            WireFormatting.WriteShortstr(m_writer, val);
        }

        public void WriteLongstr(byte[] val)
        {
            BitFlush();
            WireFormatting.WriteLongstr(m_writer, val);
        }

        public void WriteShort(ushort val)
        {
            BitFlush();
            WireFormatting.WriteShort(m_writer, val);
        }

        public void WriteLong(uint val)
        {
            BitFlush();
            WireFormatting.WriteLong(m_writer, val);
        }

        public void WriteLonglong(ulong val)
        {
            BitFlush();
            WireFormatting.WriteLonglong(m_writer, val);
        }

        public void WriteBit(bool val)
        {
            if (m_bitMask > 0x80)
            {
                BitFlush();
            }
            if (val)
            {
                // The cast below is safe, because the combination of
                // the test against 0x80 above, and the action of
                // BitFlush(), causes m_bitMask never to exceed 0x80
                // at the point the following statement executes.
                m_bitAccumulator = (byte)(m_bitAccumulator | (byte)m_bitMask);
            }
            m_bitMask = m_bitMask << 1;
            m_needBitFlush = true;
        }

        public void WriteTable(IDictionary val)
        {
            BitFlush();
            WireFormatting.WriteTable(m_writer, val);
        }

        public void WriteTimestamp(AmqpTimestamp val)
        {
            BitFlush();
            WireFormatting.WriteTimestamp(m_writer, val);
        }

        // TODO: Consider using NotImplementedException (?)
        // This is a completely bizarre consequence of the way the
        // Message.Transfer method is marked up in the XML spec.
        public void WriteContent(byte[] val)
        {
            throw new NotSupportedException("WriteContent should not be called");
        }

        public void Flush()
        {
            BitFlush();
            m_writer.Flush();
        }
    }
}
