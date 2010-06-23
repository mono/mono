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
namespace RabbitMQ.Client
{
    ///<summary>Wrapper for a byte[]. May appear as values read from
    ///and written to AMQP field tables.</summary>
    ///<remarks>
    ///<para>
    /// The sole reason for the existence of this class is to permit
    /// encoding of byte[] as 'x' in AMQP field tables, an extension
    /// to the specification that is part of the tentative JMS mapping
    /// implemented by QPid.
    ///</para>
    ///<para>
    /// Instances of this object may be found as values held in
    /// IDictionary instances returned from
    /// RabbitMQ.Client.Impl.WireFormatting.ReadTable, e.g. as part of
    /// IBasicProperties.Headers tables. Likewise, instances may be
    /// set as values in an IDictionary table to be encoded by
    /// RabbitMQ.Client.Impl.WireFormatting.WriteTable.
    ///</para>
    ///<para>
    /// When an instance of this class is encoded/decoded, the type
    /// tag 'x' is used in the on-the-wire representation. The AMQP
    /// standard type tag 'S' is decoded to a raw byte[], and a raw
    /// byte[] is encoded as 'S'. Instances of System.String are
    /// converted to a UTF-8 binary representation, and then encoded
    /// using tag 'S'. In order to force the use of tag 'x', instances
    /// of this class must be used.
    ///</para>
    ///</remarks>
    public class BinaryTableValue
    {
        private byte[] m_bytes = null;

        ///<summary>The wrapped byte array, as decoded or as to be
        ///encoded.</summary>
        public byte[] Bytes {
            get { return m_bytes; }
            set { m_bytes = value; }
        }

        ///<summary>Constructs an instance with null for its Bytes
        ///property.</summary>
        public BinaryTableValue()
            : this(null)
        {}

        ///<summary>Constructs an instance with the passed-in value
        ///for its Bytes property.</summary>
        public BinaryTableValue(byte[] bytes)
        {
            m_bytes = bytes;
        }
    }
}
