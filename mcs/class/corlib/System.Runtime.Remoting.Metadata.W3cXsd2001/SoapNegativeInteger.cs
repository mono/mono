//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNegativeInteger
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
	public sealed class SoapNegativeInteger : ISoapXsd
	{
		decimal _value;
		
		public SoapNegativeInteger()
		{
		}
		
		public SoapNegativeInteger(decimal value)
		{
			if (value >= 0) 
				throw SoapHelper.GetException (this, "invalid " + value);
			_value = value;
		}
		
		public decimal Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "negativeInteger"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapNegativeInteger Parse (string value)
		{
			return new SoapNegativeInteger (decimal.Parse (value));
		}

		public override string ToString()
		{
			return _value.ToString ();
		}
	}
}
