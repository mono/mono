//
// Mono.Security.Protocol.Ntlm.NtlmTargetInformation
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2007 Novell, Inc. (http://www.novell.com)
//

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
using System.IO;
using System.Text;

namespace Mono.Security.Protocol.Ntlm {
	public class NtlmTargetInformation {
		string _server, _domain, _dns_host, _dns_domain;

		public string ServerName {
			get { return _server; }
			set { _server = value; }
		}

		public string DomainName {
			get { return _domain; }
			set { _domain = value; }
		}

		public string DnsHostName {
			get { return _dns_host; }
			set { _dns_host = value; }
		}

		public string DnsDomainName {
			get { return _dns_domain; }
			set { _dns_domain = value; }
		}

		public void Decode (byte [] bytes, int length, int offset)
		{
			int end = offset + length;
			for (int pos = offset; pos < end;) {
				short type = BitConverterLE.ToInt16 (bytes, pos); // reader.ReadInt16 ();
				short blen = BitConverterLE.ToInt16 (bytes, pos + 2); // reader.ReadInt16 ();
				string s = Encoding.Unicode.GetString (bytes, pos + 4, blen);
				pos += blen + 4;
				switch (type) {
				case 0: break; // terminator
				case 1: ServerName = s; break;
				case 2: DomainName = s; break;
				case 3: DnsHostName = s; break;
				case 4: DnsDomainName = s; break;
				default:
					throw new ArgumentException (String.Format ("Invalid SSPI message type 2 subblock type: {0}", type));
				}
				if (type == 0)
					break; // terminator subblock
			}
		}

		public byte [] ToBytes ()
		{
			MemoryStream ms = new MemoryStream ();
			BinaryWriter bw = new BinaryWriter (ms);

			WriteName (bw, 1, ServerName);
			WriteName (bw, 2, DomainName);
			WriteName (bw, 3, DnsHostName);
			WriteName (bw, 4, DnsDomainName);
			bw.Close ();
			return ms.ToArray ();
		}

		private void WriteName (BinaryWriter bw, short type, string value)
		{
			if (value == null)
				return;
			byte [] bytes = Encoding.Unicode.GetBytes (value);
			bw.Write (type);
			bw.Write ((short) bytes.Length);
			bw.Write (bytes);
		}
	}
}
