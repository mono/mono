//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapEntity
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
	public sealed class SoapEntity : ISoapXsd
	{
		string _value;
		
		public SoapEntity ()
		{
		}
		
		public SoapEntity (string value)
		{
			_value = SoapHelper.Normalize (value);
		}
		
		public string Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "ENTITY"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapEntity Parse (string value)
		{
			return new SoapEntity (value);
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
