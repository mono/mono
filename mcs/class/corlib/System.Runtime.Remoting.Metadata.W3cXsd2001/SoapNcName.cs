//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNcName
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
	public sealed class SoapNcName : ISoapXsd
	{
		string _value;
		
		public SoapNcName()
		{
		}
		
		public SoapNcName (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "NCName"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapNcName Parse (string value)
		{
			return new SoapNcName (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
