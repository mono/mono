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
using System;
using System.Text;
using System.Globalization;

namespace System.Net.NetworkInformation {
	public class PhysicalAddress {
		public static readonly PhysicalAddress None = new PhysicalAddress (new byte [0]);
		private const int numberOfBytes = 6;
		byte [] bytes;
		
		public PhysicalAddress (byte [] address)
		{
			this.bytes = address;
		}

		internal static PhysicalAddress ParseEthernet (string address)
		{
			if (address == null)
				return None;

			string [] blocks = address.Split (':');
			byte [] bytes = new byte [blocks.Length];
			int i = 0;
			foreach (string b in blocks){
				bytes [i++] = Byte.Parse (b, NumberStyles.HexNumber);
			}
			return new PhysicalAddress (bytes);
		}
		
		public static PhysicalAddress Parse (string address)
		{
			if (address == null)
				return None;

			if (address == string.Empty)
				throw new FormatException("An invalid physical address was specified.");

			string[] addrSplit = address.Split('-');

			if (addrSplit.Length == 1) {
				if (address.Length != numberOfBytes * 2)
					throw new FormatException("An invalid physical address was specified.");

				addrSplit = new string[numberOfBytes];
				for (int index = 0; index < addrSplit.Length; index++) {
					addrSplit[index] = address.Substring(index * 2, 2);
				}
			}

			if (addrSplit.Length == numberOfBytes) {
				foreach (string str in addrSplit)
					if (str.Length > 2)
						throw new FormatException("An invalid physical address was specified.");
					else if (str.Length < 2)
						throw new IndexOutOfRangeException("An invalid physical address was specified.");
			}
			else
				throw new FormatException("An invalid physical address was specified.");

			byte[] data = new byte[numberOfBytes];
			for (int i = 0; i < numberOfBytes; i++) {
				byte b = (byte)(GetValue(addrSplit[i][0]) << 4);
				b += GetValue(addrSplit[i][1]);
				data[i] = b;
			}

			return new PhysicalAddress (data);
		}

		static byte GetValue (char c)
		{
			if (c >= '0' && c <= '9')
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

			if (bytes.Length != other.bytes.Length)
				return false;

			for (int index = 0; index < bytes.Length; index++)
				if (bytes[index] != other.bytes[index])
					return false;

			return true;
		}

		public override int GetHashCode ()
		{
			return (bytes[5] << 8) ^ (bytes[4]) ^ (bytes[3] << 24) ^ (bytes[2] << 16) ^ (bytes[1] << 8) ^ (bytes[0]);
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
				sb.AppendFormat("{0:X2}", b);
			return sb.ToString ();
		}
	}
}
