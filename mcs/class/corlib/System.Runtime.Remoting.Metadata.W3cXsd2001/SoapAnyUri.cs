//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapAnyUri
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
	public sealed class SoapAnyUri : ISoapXsd
	{
		string _value;
		
		public SoapAnyUri ()
		{
		}

		public SoapAnyUri (string value)
		{
			_value = value;
		}

		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "anyUri"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapAnyUri Parse (string value)
		{
			return new SoapAnyUri (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
