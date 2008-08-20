//
// Mono.Security.Protocol.Ntlm.Type1Message - Negotiation
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
//
// References
// a.	NTLM Authentication Scheme for HTTP, Ronald Tschalär
//	http://www.innovation.ch/java/ntlm.html
// b.	The NTLM Authentication Protocol, Copyright © 2003 Eric Glass
//	http://davenport.sourceforge.net/ntlm.html
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.Protocol.Ntlm {

	public class Type1Message : MessageBase {

		private string _host;
		private string _domain;

		public Type1Message () : this (NtlmVersion.Version2)
		{
		}

		public Type1Message (NtlmVersion version) : base (1, version)
		{
			// default values
			_domain = Environment.UserDomainName;
			_host = Environment.MachineName;
			Flags = (NtlmFlags) 0xb203;
		}

		public Type1Message (byte[] message) : this (message, NtlmVersion.Version2)
		{
		}

		public Type1Message (byte[] message, NtlmVersion version) : base (1, version)
		{
			Decode (message);
		}

		// properties

		public string Domain {
			get { return _domain; }
			set { _domain = value; }
		}

		public string Host {
			get { return _host; }
			set { _host = value; }
		}

		// methods

		protected override void Decode (byte[] message) 
		{
			base.Decode (message);

			Flags = (NtlmFlags) BitConverterLE.ToUInt32 (message, 12);
			if (Version == NtlmVersion.Version1)
				return;

			int dom_len = BitConverterLE.ToUInt16 (message, 16);
			int dom_off = BitConverterLE.ToUInt16 (message, 20);
			_domain = Encoding.ASCII.GetString (message, dom_off, dom_len);

			int host_len = BitConverterLE.ToUInt16 (message, 24);
			int host_off = BitConverterLE.ToUInt16 (message, 28);
			_host = Encoding.ASCII.GetString (message, host_off, host_len);

			if (Version != NtlmVersion.Version3)
				return;
			OSVersion = CreateSubArray (message, 32, 8);
		}

		public override byte[] GetBytes () 
		{
			short dom_len = 0, host_len = 0;
			if (Version != NtlmVersion.Version1) {
				dom_len = (short) _domain.Length;
				host_len = (short) _host.Length;
			}

			int headSize = (Version == NtlmVersion.Version3 ? 40 : 32);

			byte[] data = PrepareMessage (headSize + dom_len + host_len);

			// v1 contains only the flags.
			if (Version == NtlmVersion.Version1)
				return data;

			data [12] = (byte) Flags;
			data [13] = (byte)((uint)Flags >> 8);
			data [14] = (byte)((uint)Flags >> 16);
			data [15] = (byte)((uint)Flags >> 24);

			int dom_off = (headSize + host_len);

			data [16] = (byte) dom_len;
			data [17] = (byte)(dom_len >> 8);
			data [18] = data [16];
			data [19] = data [17];
			data [20] = (byte) dom_off;
			data [21] = (byte)(dom_off >> 8);
			data [22] = (byte)(dom_off >> 16);
			data [23] = (byte)(dom_off >> 24);

			int host_off = headSize;
			data [24] = (byte) host_len;
			data [25] = (byte)(host_len >> 8);
			data [26] = data [24];
			data [27] = data [25];
			data [28] = (byte) host_off;
			data [29] = (byte)(host_off >> 8);
			data [30] = (byte)(host_off >> 16);
			data [31] = (byte)(host_off >> 24);

			if (Version == NtlmVersion.Version3)
				Buffer.BlockCopy (OSVersion, 0, data, 32, OSVersion.Length);

			byte[] host = Encoding.ASCII.GetBytes (_host.ToUpper (CultureInfo.InvariantCulture));
			Buffer.BlockCopy (host, 0, data, host_off, host.Length);

			byte[] domain = Encoding.ASCII.GetBytes (_domain.ToUpper (CultureInfo.InvariantCulture));
			Buffer.BlockCopy (domain, 0, data, dom_off, domain.Length);

			return data;
		}
	}
}
