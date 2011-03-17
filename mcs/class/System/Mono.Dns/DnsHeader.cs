//
// Mono.Dns.DnsHeader
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
using System.Text;

namespace Mono.Dns {
#if !NET_2_0
	public
#endif
	class DnsHeader {
		public const int DnsHeaderLength = 12;
		ArraySegment<byte> bytes;

		public DnsHeader (byte [] bytes)
			: this (bytes, 0)
		{
		}

		public DnsHeader (byte [] bytes, int offset)
			: this (new ArraySegment<byte> (bytes, offset, DnsHeaderLength))
		{
		}

		public DnsHeader (ArraySegment<byte> segment)
		{
			if (segment.Count != DnsHeaderLength)
				throw new ArgumentException ("Count must be 12", "segment");

			bytes = segment;
		}

		public void Clear ()
		{
			for (int i = 0; i < DnsHeaderLength; i++)
				bytes.Array [i + bytes.Offset] = 0;
		}

		public ushort ID {
			get {
				return (ushort)(bytes.Array [bytes.Offset] * 256 + bytes.Array [bytes.Offset + 1]);
			}
			set {
				bytes.Array [bytes.Offset] = (byte) ((value & 0x0ff00) >> 8);
				bytes.Array [bytes.Offset + 1] = (byte) (value & 0x0ff);
			}
		}

		public bool IsQuery {
			get { return ((bytes.Array [2 + bytes.Offset] & 0x80) != 0); }
			set {
				if (!value) {
					bytes.Array [2 + bytes.Offset] |= 0x80;
				} else {
					bytes.Array [2 + bytes.Offset] &= 0x7f;
				}
			}
		}

		public DnsOpCode OpCode {
			get { return (DnsOpCode) ((bytes.Array [2 + bytes.Offset] & 0x78) >> 3); }
			set {
				if (!Enum.IsDefined (typeof (DnsOpCode), value))
					throw new ArgumentOutOfRangeException ("value", "Invalid DnsOpCode value");

				int v = (int) value;
				v <<= 3;
				int prev = (bytes.Array [2 + bytes.Offset] & 0x87);
				v |= prev;
				bytes.Array [2 + bytes.Offset] = (byte) v;
			}
		}

		public bool AuthoritativeAnswer {
			get { return (bytes.Array [2 + bytes.Offset] & 4) != 0; }
			set {
				if(value) {
					bytes.Array [2 + bytes.Offset] |= 4;
				} else {
					bytes.Array [2 + bytes.Offset] &= 0xFB;
				}
			}
		}

		public bool Truncation {
			get { return (bytes.Array [2 + bytes.Offset] & 2) != 0; }
			set {
				if(value) {
					bytes.Array [2 + bytes.Offset] |= 2;
				} else {
					bytes.Array [2 + bytes.Offset] &= 0xFD;
				}
			}
		}

		public bool RecursionDesired {
			get { return (bytes.Array [2 + bytes.Offset] & 1) != 0; }
			set {
				if(value) {
					bytes.Array [2 + bytes.Offset] |= 1;
				} else {
					bytes.Array [2 + bytes.Offset] &= 0xFE;
				}
			}
		}

		public bool RecursionAvailable {
			get { return (bytes.Array [3 + bytes.Offset] & 0x80) != 0; }
			set {
				if(value) {
					bytes.Array [3 + bytes.Offset] |= 0x80;
				} else {
					bytes.Array [3 + bytes.Offset] &= 0x7F;
				}
			}
		}

		// TODO: Add AuthenticData and Checking Disabled (bit 10 and 11 of Z)
		public int ZReserved {
			get { return (bytes.Array [3 + bytes.Offset] & 0x70) >> 4; }
			set {
				if(value < 0 || value > 7) {
					throw new ArgumentOutOfRangeException("value", "Must be between 0 and 7");
				}
				bytes.Array [3 + bytes.Offset] &= 0x8F;
				bytes.Array [3 + bytes.Offset] |= (byte) ((value << 4) & 0x70);
			}
		}

		public DnsRCode RCode {
			get { return (DnsRCode) (bytes.Array [3 + bytes.Offset] & 0x0f); }
			set {
				int v = (int)value;
				//Info: Values > 15 are encoded in other records (OPT, TSIG, TKEY)
				if (v < 0 || v > 15)
					throw new ArgumentOutOfRangeException("value", "Must be between 0 and 15");

				bytes.Array [3 + bytes.Offset] &= 0x0f;
				bytes.Array [3 + bytes.Offset] |= (byte) v;
			}
		}

		static ushort GetUInt16 (byte [] bytes, int offset)
		{
			return (ushort)(bytes [offset] * 256 + bytes [offset + 1]);
		}

		static void SetUInt16 (byte [] bytes, int offset, ushort val)
		{
			bytes [offset] = (byte) ((val & 0x0ff00) >> 8);
			bytes [offset + 1] = (byte) (val & 0x0ff);
		}

		public ushort QuestionCount {
			get { return GetUInt16 (bytes.Array, 4); }
			set { SetUInt16 (bytes.Array, 4, value); }
		}

		public ushort AnswerCount {
			get { return GetUInt16 (bytes.Array, 6); }
			set { SetUInt16 (bytes.Array, 6, value); }
		}

		public ushort AuthorityCount {
			get { return GetUInt16 (bytes.Array, 8); }
			set { SetUInt16 (bytes.Array, 8, value); }
		}

		public ushort AdditionalCount {
			get { return GetUInt16 (bytes.Array, 10); }
			set { SetUInt16 (bytes.Array, 10, value); }
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat ("ID: {0} QR: {1} Opcode: {2} AA: {3} TC: {4} RD: {5} RA: {6} \r\nRCode: {7} ",
					ID, IsQuery, OpCode, AuthoritativeAnswer, Truncation, RecursionDesired,
					RecursionAvailable, RCode);
			sb.AppendFormat ("Q: {0} A: {1} NS: {2} AR: {3}\r\n", QuestionCount, AnswerCount, AuthorityCount, AdditionalCount);
			return sb.ToString();
		}
	}
}

