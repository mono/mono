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
    ///<summary>Framework for constructing various types of AMQP
    ///Basic-class application messages.</summary>
    public class BasicMessageBuilder: IMessageBuilder {
        ///<summary>By default, new instances of BasicMessageBuilder and its
        ///subclasses will have this much initial buffer
        ///space.</summary>
        public const int DefaultAccumulatorSize = 1024;

        protected IBasicProperties m_properties;
        protected MemoryStream m_accumulator;

        protected NetworkBinaryWriter m_writer = null;

        ///<summary>Retrieve this instance's NetworkBinaryWriter writing to BodyStream.</summary>
        ///<remarks>
        /// If no NetworkBinaryWriter instance exists, one is created,
        /// pointing at the beginning of the accumulator. If one
        /// already exists, the existing instance is returned. The
        /// instance is not reset.
        ///</remarks>
        public NetworkBinaryWriter Writer {
            get {
                if (m_writer == null) {
                    m_writer = new NetworkBinaryWriter(m_accumulator);
                }
                return m_writer;
            }
        }

        ///<summary>Construct an instance ready for writing.</summary>
        ///<remarks>
        /// The DefaultAccumulatorSize is used for the initial accumulator buffer size.
        ///</remarks>
        public BasicMessageBuilder(IModel model): this(model, DefaultAccumulatorSize) {}

        ///<summary>Construct an instance ready for writing.</summary>
        public BasicMessageBuilder(IModel model, int initialAccumulatorSize) {
            m_properties = model.CreateBasicProperties();
            m_accumulator = new MemoryStream(initialAccumulatorSize);

            string contentType = GetDefaultContentType();
            if (contentType != null) {
                Properties.ContentType = contentType;
            }
        }

        ///<summary>Retrieve the IBasicProperties associated with this instance.</summary>
        public IBasicProperties Properties {
	    get {
		return m_properties;
	    }
	}

        ///<summary>Implement IMessageBuilder.Headers</summary>
	public IDictionary Headers {
	    get {
		if (Properties.Headers == null) {
		    Properties.Headers = new Hashtable();
		}
		return Properties.Headers;
	    }
	}

        ///<summary>Implement IMessageBuilder.BodyStream</summary>
        public Stream BodyStream {
	    get {
		return m_accumulator;
	    }
	}

        ///<summary>Implement
        ///IMessageBuilder.GetDefaultContentType(). Returns null;
        ///overridden in subclasses.</summary>
        public virtual string GetDefaultContentType() {
            return null;
        }

	///<summary>Implement IMessageBuilder.RawWrite</summary>
	public IMessageBuilder RawWrite(byte b) {
	    BodyStream.WriteByte(b);
	    return this;
	}

	///<summary>Implement IMessageBuilder.RawWrite</summary>
	public IMessageBuilder RawWrite(byte[] bytes) {
	    return RawWrite(bytes, 0, bytes.Length);
	}

	///<summary>Implement IMessageBuilder.RawWrite</summary>
	public IMessageBuilder RawWrite(byte[] bytes, int offset, int length) {
	    BodyStream.Write(bytes, offset, length);
	    return this;
	}

	///<summary>Implement IMessageBuilder.GetContentHeader</summary>
	public virtual IContentHeader GetContentHeader() {
	    return m_properties;
	}

	///<summary>Implement IMessageBuilder.GetContentBody</summary>
	public virtual byte[] GetContentBody() {
	    return m_accumulator.ToArray();
	}
    }
}
