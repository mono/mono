//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapEntities
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
	public sealed class SoapEntities : ISoapXsd
	{
		string _value;
		
		public SoapEntities ()
		{
		}
		
		public SoapEntities (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "ENTITIES"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapEntities Parse (string value)
		{
			return new SoapEntities (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
