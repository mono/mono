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

namespace RabbitMQ.Client.Content {
    ///<summary>Interface for constructing messages binary-compatible
    ///with QPid's "StreamMessage" wire encoding.</summary>
    public interface IStreamMessageBuilder: IMessageBuilder {
        ///<summary>Writes a bool value into the message body being assembled.</summary>
        IStreamMessageBuilder WriteBool(bool value);

        ///<summary>Writes an int value into the message body being assembled.</summary>
        IStreamMessageBuilder WriteInt32(int value);

        ///<summary>Writes a short value into the message body being assembled.</summary>
        IStreamMessageBuilder WriteInt16(short value);

        ///<summary>Writes a byte value into the message body being assembled.</summary>
        IStreamMessageBuilder WriteByte(byte value);

        ///<summary>Writes a char value into the message body being assembled.</summary>
        IStreamMessageBuilder WriteChar(char value);

        ///<summary>Writes a long value into the message body being assembled.</summary>
        IStreamMessageBuilder WriteInt64(long value);

        ///<summary>Writes a float value into the message body being assembled.</summary>
        IStreamMessageBuilder WriteSingle(float value);

        ///<summary>Writes a double value into the message body being assembled.</summary>
        IStreamMessageBuilder WriteDouble(double value);

        ///<summary>Writes a section of a byte array into the message body being assembled.</summary>
        IStreamMessageBuilder WriteBytes(byte[] source, int offset, int count);

        ///<summary>Writes a byte array into the message body being assembled.</summary>
        IStreamMessageBuilder WriteBytes(byte[] source);

        ///<summary>Writes a string value into the message body being assembled.</summary>
        IStreamMessageBuilder WriteString(string value);

        ///<summary>Writes an object value into the message body being assembled.</summary>
        ///<remarks>
        /// The only permitted types are bool, int, short, byte, char,
        /// long, float, double, byte[] and string.
        ///</remarks>
        IStreamMessageBuilder WriteObject(object value);

        ///<summary>Writes objects using WriteObject(), one after the
        ///other. No length indicator is written. See also
        ///IStreamMessageReader.ReadObjects().</summary>
        IStreamMessageBuilder WriteObjects(params object[] values);
    }
}
