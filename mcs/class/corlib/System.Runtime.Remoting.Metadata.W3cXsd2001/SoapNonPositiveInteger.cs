//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonPositiveInteger
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
	public sealed class SoapNonPositiveInteger : ISoapXsd
	{
		decimal _value;
		
		public SoapNonPositiveInteger()
		{
		}
		
		public SoapNonPositiveInteger(decimal value)
		{
			if (value > 0) 
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
		
		public static SoapNonPositiveInteger Parse (string value)
		{
			return new SoapNonPositiveInteger (decimal.Parse (value));
		}

		public override string ToString()
		{
			return _value.ToString ();
		}
	}
}
