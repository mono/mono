// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2010 LShift Ltd., Cohesive Financial
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
//   Portions created by LShift Ltd are Copyright (C) 2007-2010 LShift
//   Ltd. Portions created by Cohesive Financial Technologies LLC are
//   Copyright (C) 2007-2010 Cohesive Financial Technologies
//   LLC. Portions created by Rabbit Technologies Ltd are Copyright
//   (C) 2007-2010 Rabbit Technologies Ltd.
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Net;
using System.Text;
using System.IO;

using RabbitMQ.Client;
using RabbitMQ.Util;

namespace RabbitMQ.Client.Content {
    ///<summary>Tags used in parsing and generating StreamWireFormatting message bodies.</summary>
    public enum StreamWireFormattingTag {
        Bool = 0x01,
        Byte = 0x02,
        Bytes = 0x03,
        Int16 = 0x04,
        Char = 0x05,
        Int32 = 0x06,
        Int64 = 0x07,
        Single = 0x08,
        Double = 0x09,
        String = 0x0A,
	Null = 0x0B
    };

    ///<summary>Internal support class for use in reading and writing
    ///information binary-compatible with QPid's "StreamMessage" wire
    ///encoding.</summary>
    public class StreamWireFormatting {
        public static bool ReadBool(NetworkBinaryReader reader) {
            object value = ReadNonnullObject("bool", reader);
            if (value is bool) {
                return (bool) value;
            }
            if (value is string) {
                return PrimitiveParser.ParseBool((string) value);
            }
            PrimitiveParser.InvalidConversion("bool", value);
            return false;
        }

        public static int ReadInt32(NetworkBinaryReader reader) {
            object value = ReadNonnullObject("int", reader);
            if (value is int || value is short || value is byte) {
                return (int) value;
            }
            if (value is string) {
                return PrimitiveParser.ParseInt((string) value);
            }
            PrimitiveParser.InvalidConversion("int", value);
            return 0;
        }

        public static short ReadInt16(NetworkBinaryReader reader) {
            object value = ReadNonnullObject("short", reader);
            if (value is short || value is byte) {
                return (short) value;
            }
            if (value is string) {
                return PrimitiveParser.ParseShort((string) value);
            }
            PrimitiveParser.InvalidConversion("short", value);
            return 0;
        }

        public static byte ReadByte(NetworkBinaryReader reader) {
            object value = ReadNonnullObject("byte", reader);
            if (value is byte) {
                return (byte) value;
            }
            if (value is string) {
                return PrimitiveParser.ParseByte((string) value);
            }
            PrimitiveParser.InvalidConversion("byte", value);
            return 0;
        }

        public static char ReadChar(NetworkBinaryReader reader) {
            object value = ReadNonnullObject("char", reader);
            if (value is char) {
                return (char) value;
            }
            PrimitiveParser.InvalidConversion("char", value);
            return (char) 0;
        }

        public static long ReadInt64(NetworkBinaryReader reader) {
            object value = ReadNonnullObject("long", reader);
            if (value is long || value is int || value is short || value is byte) {
                return (long) value;
            }
            if (value is string) {
                return PrimitiveParser.ParseLong((string) value);
            }
            PrimitiveParser.InvalidConversion("long", value);
            return 0;
        }

        public static float ReadSingle(NetworkBinaryReader reader) {
            object value = ReadNonnullObject("float", reader);
            if (value is float) {
                return (float) value;
            }
            if (value is string) {
                return PrimitiveParser.ParseFloat((string) value);
            }
            PrimitiveParser.InvalidConversion("float", value);
            return 0;
        }

        public static double ReadDouble(NetworkBinaryReader reader) {
            object value = ReadNonnullObject("double", reader);
            if (value is double || value is float) {
                return (double) value;
            }
            if (value is string) {
                return PrimitiveParser.ParseDouble((string) value);
            }
            PrimitiveParser.InvalidConversion("double", value);
            return 0;
        }

        public static byte[] ReadBytes(NetworkBinaryReader reader) {
            object value = ReadObject(reader);
            if (value == null) {
                return null;
            }
            if (value is byte[]) {
                return (byte[]) value;
            }
            PrimitiveParser.InvalidConversion("byte[]", value);
            return null;
        }

        public static string ReadString(NetworkBinaryReader reader) {
            object value = ReadObject(reader);
            if (value == null) {
                return null;
            }
            if (value is byte[]) {
                PrimitiveParser.InvalidConversion("string", value);
                return null;
            }
            return value.ToString();
        }

        ///<exception cref="ProtocolViolationException"/>
        public static object ReadNonnullObject(string target, NetworkBinaryReader reader) {
            object value = ReadObject(reader);
            if (value == null) {
                throw new ProtocolViolationException(string.Format("Null {0} value not permitted",
                                                                   target));
            }
            return value;
        }

        ///<exception cref="EndOfStreamException"/>
        ///<exception cref="ProtocolViolationException"/>
        public static object ReadObject(NetworkBinaryReader reader) {
            int typeTag = reader.ReadByte();
            switch (typeTag) {
	      case -1:
		  throw new EndOfStreamException("End of StreamMessage reached");

              case (int) StreamWireFormattingTag.Bool: {
                  byte value = reader.ReadByte();
                  switch (value) {
                    case 0x00: return false;
                    case 0x01: return true;
                    default: {
                        string message =
                            string.Format("Invalid boolean value in StreamMessage: {0}", value);
                        throw new ProtocolViolationException(message);
                    }
                  }
              }

              case (int) StreamWireFormattingTag.Byte:
                  return reader.ReadByte();

              case (int) StreamWireFormattingTag.Bytes: {
                  int length = reader.ReadInt32();
                  if (length == -1) {
                      return null;
                  } else {
                      return reader.ReadBytes(length);
                  }
              }

              case (int) StreamWireFormattingTag.Int16:
                  return reader.ReadInt16();

              case (int) StreamWireFormattingTag.Char:
                  return (char) reader.ReadUInt16();

              case (int) StreamWireFormattingTag.Int32:
                  return reader.ReadInt32();

              case (int) StreamWireFormattingTag.Int64:
                  return reader.ReadInt64();

              case (int) StreamWireFormattingTag.Single:
                  return reader.ReadSingle();

              case (int) StreamWireFormattingTag.Double:
                  return reader.ReadDouble();

              case (int) StreamWireFormattingTag.String:
                  return ReadUntypedString(reader);

              case (int) StreamWireFormattingTag.Null:
                  return null;

              default: {
                  string message = string.Format("Invalid type tag in StreamMessage: {0}",
                                                 typeTag);
                  throw new ProtocolViolationException(message);
              }
            }
        }

        public static string ReadUntypedString(NetworkBinaryReader reader) {
            BinaryWriter buffer = NetworkBinaryWriter.TemporaryBinaryWriter(256);
            while (true) {
                byte b = reader.ReadByte();
                if (b == 0) {
                    return Encoding.UTF8.GetString(NetworkBinaryWriter.TemporaryContents(buffer));
                } else {
                    buffer.Write(b);
                }
            }
        }

        public static void WriteBool(NetworkBinaryWriter writer, bool value) {
            writer.Write((byte) StreamWireFormattingTag.Bool);
            writer.Write(value ? (byte) 0x01 : (byte) 0x00);
        }

        public static void WriteInt32(NetworkBinaryWriter writer, int value) {
            writer.Write((byte) StreamWireFormattingTag.Int32);
            writer.Write(value);
        }

        public static void WriteInt16(NetworkBinaryWriter writer, short value) {
            writer.Write((byte) StreamWireFormattingTag.Int16);
            writer.Write(value);
        }

        public static void WriteByte(NetworkBinaryWriter writer, byte value) {
            writer.Write((byte) StreamWireFormattingTag.Byte);
            writer.Write(value);
        }

        public static void WriteChar(NetworkBinaryWriter writer, char value) {
            writer.Write((byte) StreamWireFormattingTag.Char);
            writer.Write((ushort) value);
        }

        public static void WriteInt64(NetworkBinaryWriter writer, long value) {
            writer.Write((byte) StreamWireFormattingTag.Int64);
            writer.Write(value);
        }

        public static void WriteSingle(NetworkBinaryWriter writer, float value) {
            writer.Write((byte) StreamWireFormattingTag.Single);
            writer.Write(value);
        }

        public static void WriteDouble(NetworkBinaryWriter writer, double value) {
            writer.Write((byte) StreamWireFormattingTag.Double);
            writer.Write(value);
        }

        public static void WriteBytes(NetworkBinaryWriter writer,
                                      byte[] value,
                                      int offset,
                                      int length)
        {
            writer.Write((byte) StreamWireFormattingTag.Bytes);
            writer.Write(length);
            writer.Write(value, offset, length);
        }

        public static void WriteBytes(NetworkBinaryWriter writer, byte[] value) {
            WriteBytes(writer, value, 0, value.Length);
        }

        public static void WriteString(NetworkBinaryWriter writer, string value) {
            writer.Write((byte) StreamWireFormattingTag.String);
            WriteUntypedString(writer, value);
        }

        ///<exception cref="ProtocolViolationException"/>
        public static void WriteObject(NetworkBinaryWriter writer, object value) {
            if (value is bool) { WriteBool(writer, (bool) value); }
            else if (value is int) { WriteInt32(writer, (int) value); }
            else if (value is short) { WriteInt16(writer, (short) value); }
            else if (value is byte) { WriteByte(writer, (byte) value); }
            else if (value is char) { WriteChar(writer, (char) value); }
            else if (value is long) { WriteInt64(writer, (long) value); }
            else if (value is float) { WriteSingle(writer, (float) value); }
            else if (value is double) { WriteDouble(writer, (double) value); }
            else if (value is byte[]) { WriteBytes(writer, (byte[]) value); }
	    else if (value is BinaryTableValue) { WriteBytes(writer,
							     ((BinaryTableValue) value).Bytes); }
            else if (value is string) { WriteString(writer, (string) value); }
            else {
                string message = string.Format("Invalid object in StreamMessage.WriteObject: {0}",
                                               value);
                throw new ProtocolViolationException(message);
            }
        }

        public static void WriteUntypedString(NetworkBinaryWriter writer, string value) {
            writer.Write(Encoding.UTF8.GetBytes(value));
            writer.Write((byte) 0);
        }
    }
}
