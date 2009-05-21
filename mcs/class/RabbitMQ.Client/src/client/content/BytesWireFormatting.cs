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
using System.Text;
using System.IO;

using RabbitMQ.Client;
using RabbitMQ.Util;

namespace RabbitMQ.Client.Content {
    ///<summary>Internal support class for use in reading and writing
    ///information binary-compatible with QPid's "BytesMessage" wire
    ///encoding.</summary>
    public class BytesWireFormatting {
        public static int ReadInt32(NetworkBinaryReader reader) {
            return reader.ReadInt32();
        }

        public static short ReadInt16(NetworkBinaryReader reader) {
            return reader.ReadInt16();
        }

        public static byte ReadByte(NetworkBinaryReader reader) {
            return reader.ReadByte();
        }

        public static char ReadChar(NetworkBinaryReader reader) {
            return (char) reader.ReadUInt16();
        }

        public static long ReadInt64(NetworkBinaryReader reader) {
            return reader.ReadInt64();
        }

        public static float ReadSingle(NetworkBinaryReader reader) {
            return reader.ReadSingle();
        }

        public static double ReadDouble(NetworkBinaryReader reader) {
            return reader.ReadDouble();
        }

        public static int Read(NetworkBinaryReader reader, byte[] target, int offset, int count) {
            return reader.Read(target, offset, count);
        }

        public static byte[] ReadBytes(NetworkBinaryReader reader, int count) {
            return reader.ReadBytes(count);
        }

        public static string ReadString(NetworkBinaryReader reader) {
            ushort length = reader.ReadUInt16();
            byte[] bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }


        public static void WriteInt32(NetworkBinaryWriter writer, int value) {
            writer.Write(value);
        }

        public static void WriteInt16(NetworkBinaryWriter writer, short value) {
            writer.Write(value);
        }

        public static void WriteByte(NetworkBinaryWriter writer, byte value) {
            writer.Write(value);
        }

        public static void WriteChar(NetworkBinaryWriter writer, char value) {
            writer.Write((ushort) value);
        }

        public static void WriteInt64(NetworkBinaryWriter writer, long value) {
            writer.Write(value);
        }

        public static void WriteSingle(NetworkBinaryWriter writer, float value) {
            writer.Write(value);
        }

        public static void WriteDouble(NetworkBinaryWriter writer, double value) {
            writer.Write(value);
        }

        public static void Write(NetworkBinaryWriter writer, byte[] source, int offset, int count)
        {
            writer.Write(source, offset, count);
        }

        public static void WriteBytes(NetworkBinaryWriter writer, byte[] source) {
            Write(writer, source, 0, source.Length);
        }

        public static void WriteString(NetworkBinaryWriter writer, string value) {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            writer.Write((ushort) bytes.Length);
            writer.Write(bytes);
        }
    }
}
