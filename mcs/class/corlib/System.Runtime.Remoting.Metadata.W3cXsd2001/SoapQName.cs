//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapQName
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
	public sealed class SoapQName : ISoapXsd
	{
		string _name;
		string _prefix;
		string _namespace;
		
		public SoapQName ()
		{
		}

		public SoapQName (string localName)
		{
			_name = localName;
		}

		public SoapQName (string prefix, string localName)
		{
			_prefix = prefix;
			_name = localName;
		}

		public SoapQName (string prefix, string localName, string namspace)
		{
			_prefix = prefix;
			_name = localName;
			_namespace = namspace;
		}

		public string Key {
			get { return _prefix; } 
			set { _prefix = value; }
		}

		public string Name {
			get { return _name; } 
			set { _name = value; }
		}

		public string Namespace {
			get { return _namespace; } 
			set { _namespace = value; }
		}

		public static string XsdType {
			get { return "QName"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapQName Parse (string value)
		{
			SoapQName res = new SoapQName ();
			int i = value.IndexOf (':');
			if (i != -1)
			{
				res.Key = value.Substring (0,i);
				res.Name = value.Substring (i+1);
			}
			else
				res.Name = value;
			return res;
		}

		public override string ToString()
		{
			if (_prefix == null || _prefix == "") return _name;
			else return _prefix + ":" + _name;
		}
	}
}
