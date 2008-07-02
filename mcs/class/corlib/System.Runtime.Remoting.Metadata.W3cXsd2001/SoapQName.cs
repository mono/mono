//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapQName.cs
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public sealed class SoapQName : ISoapXsd
	{
		string _name;
		string _key;
		string _namespace;
		
		public SoapQName ()
		{
		}

		public SoapQName (string value)
		{
			_name = value;
		}

		public SoapQName (string key, string name)
		{
			_key = key;
			_name = name;
		}

		public SoapQName (string key, string name, string namespaceValue)
		{
			_key = key;
			_name = name;
			_namespace = namespaceValue;
		}

		public string Key {
			get { return _key; } 
			set { _key = value; }
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
			if (_key == null || _key == "") return _name;
			else return _key + ":" + _name;
		}
	}
}
