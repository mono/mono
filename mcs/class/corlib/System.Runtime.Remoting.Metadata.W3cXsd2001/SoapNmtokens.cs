//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNmtokens
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
	public sealed class SoapNmtokens : ISoapXsd
	{
		string _value;
		
		public SoapNmtokens()
		{
		}
		
		public SoapNmtokens (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "NMTOKENS"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapNmtokens Parse (string value)
		{
			return new SoapNmtokens (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
