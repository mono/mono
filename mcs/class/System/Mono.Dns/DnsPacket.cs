//
// Mono.Dns.DnsPacket
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
using System.IO;

namespace Mono.Dns {
#if !NET_2_0
	public
#endif
	abstract class DnsPacket {
		protected byte [] packet;
		protected int position;
		protected DnsHeader header;

		protected DnsPacket ()
		{
			// Caller has to initialize packet, position and header
		}

		protected DnsPacket (int length)
			: this (new byte [length], length)
		{
		}

		protected DnsPacket (byte [] buffer, int length)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (length <= 0)
				throw new ArgumentOutOfRangeException("length", "Must be greater than zero.");

			packet = buffer;
			position = length;
			header = new DnsHeader(new ArraySegment<byte>(packet, 0, 12));
		}

		public byte [] Packet {
			get { return packet; }
		}

		public int Length {
			get { return position; }
		}

		public DnsHeader Header {
			get { return header; }
		}

		protected void WriteUInt16 (ushort v)
		{
			packet [position++] = (byte) ((v & 0x0ff00) >> 8);
			packet [position++] = (byte) (v & 0x0ff);
		}

		protected void WriteStringBytes (string str, int offset, int count)
		{
			for (int i = offset, c = 0; c < count; c++, i++)
				packet [position++] = (byte) str [i]; // Don't care about encoding.
		}

		protected void WriteLabel (string str, int offset, int count)
		{
			packet [position++] = (byte) count;
			WriteStringBytes (str, offset, count);
		}

		protected void WriteDnsName (string name)
		{
			if (!DnsUtil.IsValidDnsName (name))
				throw new ArgumentException ("Invalid DNS name");

			if (!String.IsNullOrEmpty (name)) {
				int len = name.Length;
				int label_start = 0;
				int label_len = 0;
				for (int i = 0; i < len; i++) {
					char c = name [i];
					if (c != '.') {
						label_len++;
					} else {
						if (i == 0)
							break; // "."
						WriteLabel (name, label_start, label_len);
						label_start += label_len + 1; // Skip the dot
						label_len = 0;
					}
				}
				if (label_len > 0)
					WriteLabel (name, label_start, label_len);
			}

			packet [position++] = 0;
		}

		protected internal string ReadName (ref int offset)
		{
			return DnsUtil.ReadName (packet, ref offset);
		}

		protected internal static string ReadName (byte [] buffer, ref int offset)
                {
			return DnsUtil.ReadName (buffer, ref offset);
                }

		protected internal ushort ReadUInt16 (ref int offset)
		{
			return (ushort)((packet[offset++] << 8) + packet[offset++]);
		}

		protected internal int ReadInt32 (ref int offset)
		{
			return (packet [offset++] << 24) + (packet [offset++] << 16) + (packet [offset++] << 8) + packet [offset++];
		}
	}
}

