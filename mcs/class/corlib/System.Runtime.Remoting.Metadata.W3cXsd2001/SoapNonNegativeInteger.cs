//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonNegativeInteger
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
	public sealed class SoapNonNegativeInteger : ISoapXsd
	{
		decimal _value;
		
		public SoapNonNegativeInteger()
		{
		}
		
		public SoapNonNegativeInteger(decimal value)
		{
			if (value < 0) 
				throw SoapHelper.GetException (this, "invalid " + value);
			_value = value;
		}
		
		public decimal Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "nonNegativeInteger"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapNonNegativeInteger Parse (string value)
		{
			return new SoapNonNegativeInteger (decimal.Parse (value));
		}

		public override string ToString()
		{
			return _value.ToString ();
		}
	}
}
