//
// Mono.Net.Dns.DnsQuery
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo.mono@gmail.com)
//
// Copyright 2011 Gonzalo Paniagua Javier
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.Net.Dns {
	class DnsQuery : DnsPacket {
		public DnsQuery (string name, DnsQType qtype, DnsQClass qclass)
		{
			if (String.IsNullOrEmpty (name))
				throw new ArgumentNullException ("name");

			int length = DnsUtil.GetEncodedLength (name);
			if (length == -1)
				throw new ArgumentException ("Invalid DNS name", "name");

			length += 12 + 2 + 2; // Header + qtype + qclass
			packet = new byte [length];
			header = new DnsHeader (packet, 0);
			position = 12;
			WriteDnsName (name);
			WriteUInt16 ((ushort) qtype);
			WriteUInt16 ((ushort) qclass);
			Header.QuestionCount = 1;
			Header.IsQuery = true;
			Header.RecursionDesired = true;
		}
	}
}

