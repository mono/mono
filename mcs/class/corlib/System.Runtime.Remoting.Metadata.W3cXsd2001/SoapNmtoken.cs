//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNmtoken
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
	public sealed class SoapNmtoken : ISoapXsd
	{
		string _value;
		
		public SoapNmtoken()
		{
		}
		
		public SoapNmtoken (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "NMTOKEN"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapNmtoken Parse (string value)
		{
			return new SoapNmtoken (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
