//
// System.Net.NetworkInformation.PhysicalAddress
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
#if NET_2_0
using System;
using System.Text;

namespace System.Net.NetworkInformation {
	public class PhysicalAddress {
		public static readonly PhysicalAddress None = new PhysicalAddress (new byte [0]);
		byte [] bytes;
		
		public PhysicalAddress (byte [] address)
		{
			this.bytes = address;
		}

		public static PhysicalAddress Parse (string address)
		{
			if (address == null)
				return None;

			if (address == "")
				throw new FormatException ("Invalid physical address.");

			// MS fails with IndexOutOfRange for something like: "00-0"
			int ndashes = 0;
			foreach (char c in address) {
				if (c == '-')
					ndashes++;
			}

			int len = address.Length;
			if (((len - 2) / 3) != ndashes)
				throw new FormatException ("Invalid physical address.");

			byte [] data = new byte [ndashes + 1];
			int idx = 0;
			for (int i = 0; i < len; i++) {
				byte b = (byte) (GetValue (address [i++]) << 8);
				b += GetValue (address [i++]);
				if (address [i] != '-')
					throw new FormatException ("Invalid physical address.");
				data [idx++] = b;
			}

			return new PhysicalAddress (data);
		}

		static byte GetValue (char c)
		{
			if (c >= 0 && c <= 9)
				return (byte) (c - '0');

			if (c >= 'a' && c <= 'f')
				return (byte) (c - 'a' + 10);

			if (c >= 'A' && c <= 'F')
				return (byte) (c - 'A' + 10);

			throw new FormatException ("Invalid physical address.");
		}

		public override bool Equals (object comparand)
		{
			PhysicalAddress other = comparand as PhysicalAddress;
			if (other == null)
				return false;

			// new byte [0] != new byte [0]
			return (bytes == other.bytes);
		}

		public override int GetHashCode ()
		{
			if (bytes == null)
				return 0;

			int a = 5;
			foreach (byte b in bytes)
				a  = (a << 3)  + b;

			return a;
		}

		public byte [] GetAddressBytes ()
		{
			return bytes;
		}

		public override string ToString ()
		{
			if (bytes == null)
				return "";

			StringBuilder sb = new StringBuilder ();
			foreach (byte b in bytes)
				sb.AppendFormat ("{0:2X}", (uint) b);
			return sb.ToString ();
		}
	}
}
#endif

