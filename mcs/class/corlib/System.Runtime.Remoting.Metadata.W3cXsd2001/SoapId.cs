//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapId
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
	public sealed class SoapId : ISoapXsd
	{
		string _value;
		
		public SoapId()
		{
		}
		
		public SoapId (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "ID"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapId Parse (string value)
		{
			return new SoapId (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
