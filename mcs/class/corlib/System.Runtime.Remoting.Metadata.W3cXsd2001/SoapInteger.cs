//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapInteger
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
	public sealed class SoapInteger : ISoapXsd
	{
		decimal _value;
		
		public SoapInteger ()
		{
		}
		
		public SoapInteger (decimal value)
		{
			_value = value;
		}
		
		public decimal Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "integer"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapInteger Parse (string value)
		{
			return new SoapInteger (decimal.Parse (value));
		}

		public override string ToString()
		{
			return _value.ToString ();
		}
	}
}
