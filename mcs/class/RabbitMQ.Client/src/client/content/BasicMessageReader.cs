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

using RabbitMQ.Client;
using RabbitMQ.Util;

namespace RabbitMQ.Client.Content {
    ///<summary>Framework for analyzing various types of AMQP
    ///Basic-class application messages.</summary>
    public class BasicMessageReader: IMessageReader {
        protected IBasicProperties m_properties;
        protected byte[] m_body;

	protected MemoryStream m_stream = null;
        protected NetworkBinaryReader m_reader = null;

        ///<summary>Retrieve this instance's NetworkBinaryReader reading from BodyBytes.</summary>
        ///<remarks>
        /// If no NetworkBinaryReader instance exists, one is created,
        /// pointing at the beginning of the body. If one already
        /// exists, the existing instance is returned. The instance is
        /// not reset.
        ///</remarks>
        public NetworkBinaryReader Reader {
            get {
                if (m_reader == null) {
                    m_reader = new NetworkBinaryReader(BodyStream);
                }
                return m_reader;
            }
        }

        ///<summary>Construct an instance ready for reading.</summary>
        public BasicMessageReader(IBasicProperties properties, byte[] body) {
            m_properties = properties;
            m_body = body;
        }

	///<summary>Implement IMessageReader.Headers</summary>
	public IDictionary Headers {
	    get {
		if (Properties.Headers == null) {
		    Properties.Headers = new Hashtable();
		}
		return Properties.Headers;
	    }
	}

        ///<summary>Retrieve the IBasicProperties associated with this instance.</summary>
        public IBasicProperties Properties {
	    get {
		return m_properties;
	    }
	}

	///<summary>Implement IMessageReader.BodyBytes</summary>
        public byte[] BodyBytes {
            get {
                return m_body;
            }
        }

	///<summary>Implement IMessageReader.BodyStream</summary>
	public Stream BodyStream {
	    get {
		if (m_stream == null) {
		    m_stream = new MemoryStream(m_body);
		}
		return m_stream;
	    }
	}

	///<summary>Implement IMessageReader.RawRead</summary>
	public int RawRead() {
	    return BodyStream.ReadByte();
	}

	///<summary>Implement IMessageReader.RawRead</summary>
	public int RawRead(byte[] target, int offset, int length) {
	    return BodyStream.Read(target, offset, length);
	}
    }
}
