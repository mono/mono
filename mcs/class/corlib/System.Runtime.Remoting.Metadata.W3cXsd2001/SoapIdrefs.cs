//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapIdrefs
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
	public sealed class SoapIdrefs : ISoapXsd
	{
		string _value;
		
		public SoapIdrefs()
		{
		}
		
		public SoapIdrefs (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "IDREFS"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapIdrefs Parse (string value)
		{
			return new SoapIdrefs (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
