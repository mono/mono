//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapBase64Binary
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

using System;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
	public sealed class SoapBase64Binary : ISoapXsd
	{
		byte[] _value;
		
		public SoapBase64Binary ()
		{
		}

		public SoapBase64Binary (byte[] value)
		{
			_value = value;
		}

		public byte [] Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "base64Binary"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapBase64Binary Parse (string value)
		{
			return new SoapBase64Binary (Convert.FromBase64String (value));
		}

		public override string ToString()
		{
			return Convert.ToBase64String (_value);
		}
	}
}
