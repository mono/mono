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

namespace RabbitMQ.Client.Content {
    ///<summary>Interface for analyzing application messages.</summary>
    ///<remarks>
    /// Subinterfaces provide specialized data-reading methods. This
    /// base interface deals with the lowest common denominator:
    /// bytes, with no special encodings for higher-level objects.
    ///</remarks>
    public interface IMessageReader {
	///<summary>Retrieves the content header properties of the
	///message being read.</summary>
	IDictionary Headers { get; }

	///<summary>Retrieve the message body, as a byte array.</summary>
	byte[] BodyBytes { get; }

	///<summary>Retrieve the Stream being used to read from the message body.</summary>
	Stream BodyStream { get; }

	///<summary>Read a single byte from the body stream, without
	///encoding or interpretation. Returns -1 for end-of-stream.</summary>
	int RawRead();

	///<summary>Read bytes from the body stream into a section of
	///an existing byte array, without encoding or
	///interpretation. Returns the number of bytes read from the
	///body and written into the target array, which may be less
	///than the number requested if the end-of-stream is
	///reached.</summary>
	int RawRead(byte[] target, int offset, int length);
    }
}
