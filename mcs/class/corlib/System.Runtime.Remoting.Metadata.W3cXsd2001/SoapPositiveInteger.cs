//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapPositiveInteger
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
	public sealed class SoapPositiveInteger : ISoapXsd
	{
		decimal _value;
		
		public SoapPositiveInteger()
		{
		}
		
		public SoapPositiveInteger (decimal value)
		{
			if (value <= 0) 
				throw SoapHelper.GetException (this, "invalid " + value);
			_value = value;
		}
		
		public decimal Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "nonPositiveInteger"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapPositiveInteger Parse (string value)
		{
			return new SoapPositiveInteger (decimal.Parse (value));
		}

		public override string ToString()
		{
			return _value.ToString ();
		}
	}
}
