//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNotation
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
	public sealed class SoapNotation : ISoapXsd
	{
		string _value;
		
		public SoapNotation()
		{
		}
		
		public SoapNotation (string value)
		{
			_value = value;
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "NOTATION"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapNotation Parse (string value)
		{
			return new SoapNotation (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
