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
    public class WireFormatting
    {
        public static byte ReadOctet(NetworkBinaryReader reader)
        {
            return reader.ReadByte();
        }

        public static string ReadShortstr(NetworkBinaryReader reader)
        {
            int byteCount = reader.ReadByte();
            return Encoding.UTF8.GetString(reader.ReadBytes(byteCount));
        }

        public static byte[] ReadLongstr(NetworkBinaryReader reader)
        {
            uint byteCount = reader.ReadUInt32();
            if (byteCount > int.MaxValue)
            {
                throw new SyntaxError("Long string too long; " +
                                      "byte length=" + byteCount + ", max=" + int.MaxValue);
            }
            return reader.ReadBytes((int)byteCount);
        }

        public static ushort ReadShort(NetworkBinaryReader reader)
        {
            return reader.ReadUInt16();
        }

        public static uint ReadLong(NetworkBinaryReader reader)
        {
            return reader.ReadUInt32();
        }

        public static ulong ReadLonglong(NetworkBinaryReader reader)
        {
            return reader.ReadUInt64();
        }

        public static decimal AmqpToDecimal(byte scale, uint unsignedMantissa)
        {
            if (scale > 28)
            {
                throw new SyntaxError("Unrepresentable AMQP decimal table field: " +
                                      "scale=" + scale);
            }
            return new decimal((int)(unsignedMantissa & 0x7FFFFFFF),
                               0,
                               0,
                               ((unsignedMantissa & 0x80000000) == 0) ? false : true,
                               scale);
        }

        public static decimal ReadDecimal(NetworkBinaryReader reader)
        {
            byte scale = ReadOctet(reader);
            uint unsignedMantissa = ReadLong(reader);
            return AmqpToDecimal(scale, unsignedMantissa);
        }

        ///<summary>Reads an AMQP "table" definition from the reader.</summary>
        ///<remarks>
        /// Supports the AMQP 0-8/0-9 standard entry types S, I, D, T
        /// and F, as well as the QPid-0-8 specific b, d, f, l, s, t,
        /// x and V types.
        ///</remarks>
        public static IDictionary ReadTable(NetworkBinaryReader reader)
        {
            Hashtable table = new Hashtable();
            long tableLength = reader.ReadUInt32();

            Stream backingStream = reader.BaseStream;
            long startPosition = backingStream.Position;
            while ((backingStream.Position - startPosition) < tableLength)
            {
                string key = ReadShortstr(reader);
                object value = null;

                byte discriminator = reader.ReadByte();
                switch ((char)discriminator)
                {
                  case 'S':
                      value = ReadLongstr(reader);
                      break;
                  case 'I':
                      value = reader.ReadInt32();
                      break;
                  case 'D':
                      value = ReadDecimal(reader);
                      break;
                  case 'T':
                      value = ReadTimestamp(reader);
                      break;
                  case 'F':
                      value = ReadTable(reader);
                      break;

                  case 'b':
                      value = ReadOctet(reader);
                      break;
                  case 'd':
                      value = reader.ReadDouble();
                      break;
                  case 'f':
                      value = reader.ReadSingle();
                      break;
                  case 'l':
                      value = reader.ReadInt64();
                      break;
                  case 's':
                      value = reader.ReadInt16();
                      break;
                  case 't':
                      value = (ReadOctet(reader) != 0);
                      break;
                  case 'x':
                      value = new BinaryTableValue(ReadLongstr(reader));
                      break;
                  case 'V':
                      value = null;
                      break;

                  default:
                      throw new SyntaxError("Unrecognised type in table: " +
                                            (char) discriminator);
                }

                if (!table.ContainsKey(key))
                {
                    table[key] = value;
                }
            }

            return table;
        }

        public static AmqpTimestamp ReadTimestamp(NetworkBinaryReader reader)
        {
            ulong stamp = ReadLonglong(reader);
            // 0-9 is afaict silent on the signedness of the timestamp.
            // See also MethodArgumentWriter.WriteTimestamp and AmqpTimestamp itself
            return new AmqpTimestamp((long)stamp);
        }

        public static void WriteOctet(NetworkBinaryWriter writer, byte val)
        {
            writer.Write((byte)val);
        }

        public static void WriteShortstr(NetworkBinaryWriter writer, string val)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(val);
            int len = bytes.Length;
            if (len > 255)
            {
                throw new WireFormattingException("Short string too long; " +
                                                  "UTF-8 encoded length=" + len + ", max=255");
            }
            writer.Write((byte)len);
            writer.Write(bytes);
        }

        public static void WriteLongstr(NetworkBinaryWriter writer, byte[] val)
        {
            WriteLong(writer, (uint)val.Length);
            writer.Write(val);
        }

        public static void WriteShort(NetworkBinaryWriter writer, ushort val)
        {
            writer.Write((ushort)val);
        }

        public static void WriteLong(NetworkBinaryWriter writer, uint val)
        {
            writer.Write((uint)val);
        }

        public static void WriteLonglong(NetworkBinaryWriter writer, ulong val)
        {
            writer.Write((ulong)val);
        }

        public static void DecimalToAmqp(decimal value, out byte scale, out int mantissa)
        {
            // According to the documentation :-
            //  - word 0: low-order "mantissa"
            //  - word 1, word 2: medium- and high-order "mantissa"
            //  - word 3: mostly reserved; "exponent" and sign bit
            // In one way, this is broader than AMQP: the mantissa is larger.
            // In another way, smaller: the exponent ranges 0-28 inclusive.
            // We need to be careful about the range of word 0, too: we can
            // only take 31 bits worth of it, since the sign bit needs to
            // fit in there too.
            int[] bitRepresentation = decimal.GetBits(value);
            if (bitRepresentation[1] != 0 ||    // mantissa extends into middle word
                bitRepresentation[2] != 0 ||    // mantissa extends into top word
                bitRepresentation[0] < 0)       // mantissa extends beyond 31 bits
            {
                throw new WireFormattingException("Decimal overflow in AMQP encoding", value);
            }
            scale = (byte)((((uint)bitRepresentation[3]) >> 16) & 0xFF);
            mantissa = (int)((((uint)bitRepresentation[3]) & 0x80000000) |
                              (((uint)bitRepresentation[0]) & 0x7FFFFFFF));
        }

        public static void WriteDecimal(NetworkBinaryWriter writer, decimal value)
        {
            byte scale;
            int mantissa;
            DecimalToAmqp(value, out scale, out mantissa);
            WriteOctet(writer, scale);
            WriteLong(writer, (uint)mantissa);
        }

        ///<summary>Writes an AMQP "table" to the writer.</summary>
        ///<remarks>
        ///<para>
        /// In this method, we assume that the stream that backs our
        /// NetworkBinaryWriter is a positionable stream - which it is
        /// currently (see Frame.m_accumulator, Frame.GetWriter and
        /// Command.Transmit).
        ///</para>
        ///<para>
        /// Supports the AMQP 0-8/0-9 standard entry types S, I, D, T
        /// and F, as well as the QPid-0-8 specific b, d, f, l, s, t
        /// x and V types.
        ///</para>
        ///</remarks>
        public static void WriteTable(NetworkBinaryWriter writer, IDictionary val)
        {
            if (val == null)
            {
                writer.Write((uint)0);
            }
            else
            {
                Stream backingStream = writer.BaseStream;
                long patchPosition = backingStream.Position;
                writer.Write((uint)0); // length of table - will be backpatched

                foreach (DictionaryEntry entry in val)
                {
                    WriteShortstr(writer, (string)entry.Key);
                    object value = entry.Value;

                    if (value == null)
                    {
                        WriteOctet(writer, (byte)'V');
                    }
                    else if (value is string)
                    {
                        WriteOctet(writer, (byte)'S');
                        WriteLongstr(writer, Encoding.UTF8.GetBytes((string)value));
                    }
                    else if (value is byte[])
                    {
                        WriteOctet(writer, (byte)'S');
                        WriteLongstr(writer, (byte[])value);
                    }
                    else if (value is int)
                    {
                        WriteOctet(writer, (byte)'I');
                        writer.Write((int)value);
                    }
                    else if (value is decimal)
                    {
                        WriteOctet(writer, (byte)'D');
                        WriteDecimal(writer, (decimal)value);
                    }
                    else if (value is AmqpTimestamp)
                    {
                        WriteOctet(writer, (byte)'T');
                        WriteTimestamp(writer, (AmqpTimestamp)value);
                    }
                    else if (value is IDictionary)
                    {
                        WriteOctet(writer, (byte)'F');
                        WriteTable(writer, (IDictionary)value);
                    }
                    else if (value is byte)
                    {
                        WriteOctet(writer, (byte)'b');
                        WriteOctet(writer, (byte)value);
                    }
                    else if (value is double)
                    {
                        WriteOctet(writer, (byte)'d');
                        writer.Write((double)value);
                    }
                    else if (value is float)
                    {
                        WriteOctet(writer, (byte)'f');
                        writer.Write((float)value);
                    }
                    else if (value is long)
                    {
                        WriteOctet(writer, (byte)'l');
                        writer.Write((long)value);
                    }
                    else if (value is short)
                    {
                        WriteOctet(writer, (byte)'s');
                        writer.Write((short)value);
                    }
                    else if (value is bool)
                    {
                        WriteOctet(writer, (byte)'t');
                        WriteOctet(writer, (byte)(((bool)value) ? 1 : 0));
                    }
                    else if (value is BinaryTableValue)
                    {
                        WriteOctet(writer, (byte)'x');
                        WriteLongstr(writer, ((BinaryTableValue)value).Bytes);
                    }
                    else
                    {
                        throw new WireFormattingException("Value cannot appear as table value",
                                                          value);
                    }
                }

                // Now, backpatch the table length.
                long savedPosition = backingStream.Position;
                long tableLength = savedPosition - patchPosition - 4; // offset for length word
                backingStream.Seek(patchPosition, SeekOrigin.Begin);
                writer.Write((uint)tableLength);
                backingStream.Seek(savedPosition, SeekOrigin.Begin);
            }
        }

        public static void WriteTimestamp(NetworkBinaryWriter writer, AmqpTimestamp val)
        {
            // 0-9 is afaict silent on the signedness of the timestamp.
            // See also MethodArgumentReader.ReadTimestamp and AmqpTimestamp itself
            WriteLonglong(writer, (ulong)val.UnixTime);
        }
    }
}
