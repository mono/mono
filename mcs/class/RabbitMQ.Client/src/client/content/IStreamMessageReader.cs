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

namespace RabbitMQ.Client.Content {
    ///<summary>Analyzes messages binary-compatible with QPid's
    ///"StreamMessage" wire encoding.</summary>
    public interface IStreamMessageReader: IMessageReader {
        ///<summary>Reads a bool from the message body.</summary>
        bool ReadBool();

        ///<summary>Reads an int from the message body.</summary>
        int ReadInt32();

        ///<summary>Reads a short from the message body.</summary>
        short ReadInt16();

        ///<summary>Reads a byte from the message body.</summary>
        byte ReadByte();

        ///<summary>Reads a char from the message body.</summary>
        char ReadChar();

        ///<summary>Reads a long from the message body.</summary>
        long ReadInt64();

        ///<summary>Reads a float from the message body.</summary>
        float ReadSingle();

        ///<summary>Reads a double from the message body.</summary>
        double ReadDouble();

        ///<summary>Reads a byte array from the message body. The body
        ///contains information about the size of the array to
        ///read.</summary>
        byte[] ReadBytes();

        ///<summary>Reads a string from the message body.</summary>
        string ReadString();

        ///<summary>Reads an object from the message body.</summary>
        object ReadObject();

        ///<summary>Reads objects from the message body until the
        ///end-of-stream is reached.</summary>
        object[] ReadObjects();
    }
}
