//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public sealed class SoapHexBinary : ISoapXsd
	{
		byte[] _value;
		StringBuilder sb = new StringBuilder ();

		public SoapHexBinary ()
		{
		}
		
		public SoapHexBinary (byte[] value)
		{
		}
		
		public byte [] Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "hexBinary"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapHexBinary Parse (string s)
		{
			char [] chars = s.ToCharArray ();
			byte [] bytes = new byte [chars.Length / 2 + chars.Length % 2];
			FromBinHexString (chars, 0, chars.Length, bytes);
			return new SoapHexBinary (bytes);
		}

		internal static int FromBinHexString (char [] chars, int offset, int charLength, byte [] buffer)
		{
			int bufIndex = offset;
			for (int i = 0; i < charLength - 1; i += 2) {
				buffer [bufIndex] = (chars [i] > '9' ?
						(byte) (chars [i] - 'A' + 10) :
						(byte) (chars [i] - '0'));
				buffer [bufIndex] <<= 4;
				buffer [bufIndex] += chars [i + 1] > '9' ?
						(byte) (chars [i + 1] - 'A' + 10) : 
						(byte) (chars [i + 1] - '0');
				bufIndex++;
			}
			if (charLength %2 != 0)
				buffer [bufIndex++] = (byte)
					((chars [charLength - 1] > '9' ?
						(byte) (chars [charLength - 1] - 'A' + 10) :
						(byte) (chars [charLength - 1] - '0'))
					<< 4);

			return bufIndex - offset;
		}

		public override string ToString()
		{
			sb.Length = 0;
			foreach (byte b in _value)
				sb.Append (b.ToString ("X2"));
			return sb.ToString ();
		}
	}
}
