//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapName
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
	public sealed class SoapName : ISoapXsd
	{
		string _value;
		
		public SoapName ()
		{
		}
		
		public SoapName (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "Name"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapName Parse (string value)
		{
			return new SoapName (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
