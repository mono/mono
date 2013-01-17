//
// Mono.Net.Dns.DnsUtil
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

namespace Mono.Net.Dns {
	static class DnsUtil {
		// RFC 2181 - Section 11
		public static bool IsValidDnsName (string name)
		{
			if (name == null)
				return false;

			int len = name.Length;
			if (len > 255)
				return false;
			int part_length = 0;
			for (int i = 0; i < len; i++) {
				char c = name [i];
				if (c == '.') {
					if (i == 0 && len > 1)
						return false; // Can't begin with a dot unless it's "."
					if (i > 0 && part_length == 0)
						return false; // No ".." allowed
					part_length = 0;
					continue;
				}
				part_length++;
				if (part_length > 63)
					return false;
			}
			return true;
		}

		public static int GetEncodedLength (string name)
		{
			if (!IsValidDnsName (name))
				return -1;

			if (name == String.Empty)
				return 1;

			int len = name.Length;
			if (name [len - 1] == '.')
				return len + 1; // (length + label + ... + \0)
			return len + 2; // need 1 more for the second to last label length
		}

		public static int GetNameLength (byte [] buffer)
		{
			return GetNameLength (buffer, 0);
		}

		public static int GetNameLength (byte [] buffer, int offset)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			int i = 0;
			int len = 0;
			while (len < 256) {
				i = buffer [offset++];
				if (i == 0)
					return len > 0 ? --len : 0;
				int ptr = i & 0x0C0;
				if (ptr == 0x0C0) {
					i = ((ptr & 0x3F) << 8) + buffer[offset++];
					offset = i;
					continue;
				} else if (ptr >= 0x40) {
					return -2; // Invalid ptr
				}
				len += i + 1;
				offset += i;
			}
			return -1; // Invalid length
		}

		public static string ReadName (byte [] buffer, ref int offset)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			StringBuilder sb = new StringBuilder (32);
			int i = 0;
			bool no_ptr = true;
			int off = offset;
			while (sb.Length < 256) {
				i = buffer [off++];
				if (no_ptr) offset++;
				if (i == 0) {
					if (sb.Length > 0)
						sb.Length--;
					return sb.ToString ();
				}
				int ptr = i & 0x0C0;
				if (ptr == 0x0C0) {
					i = ((ptr & 0x3F) << 8) + buffer [off];
					if (no_ptr) offset++;
					no_ptr = false;
					off = i;
					continue;
				} else if (i >= 0x40) {
					return null; // Invalid ptr
				}

				for (int k = 0; k < i; k++)
					sb.Append ((char) buffer [off + k]);
				sb.Append ('.');
				off += i;
				if (no_ptr) offset += i;
			}
			return null;  // never reached
		}

	}
}

