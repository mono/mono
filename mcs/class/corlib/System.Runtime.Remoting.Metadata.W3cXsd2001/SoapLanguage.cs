//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapLanguage
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
	public sealed class SoapLanguage : ISoapXsd
	{
		string _value;
		
		public SoapLanguage ()
		{
		}
		
		public SoapLanguage (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "language"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapLanguage Parse (string value)
		{
			return new SoapLanguage (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
