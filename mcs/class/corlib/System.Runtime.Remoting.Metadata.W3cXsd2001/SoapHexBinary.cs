//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Text;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
	public sealed class SoapHexBinary : ISoapXsd
	{
		byte[] _value;
		
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
			StringBuilder sb = new StringBuilder ();
			foreach (byte b in _value)
				sb.Append (b.ToString ("X2"));
			return sb.ToString ();
		}
	}
}
