//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapToken
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
	public sealed class SoapToken : ISoapXsd 
	{
		string _value;
		
		public SoapToken()
		{
		}
		
		public SoapToken (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "token"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapToken Parse (string value)
		{
			return new SoapToken (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
