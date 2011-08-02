//
// XmlSerializerNamespaces.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlSerializerNamespaces.
	/// </summary>
	public class XmlSerializerNamespaces
	{
#if MOONLIGHT
		private Dictionary<string,XmlQualifiedName> namespaces = new Dictionary<string,XmlQualifiedName> ();
#else
		private ListDictionary namespaces = new ListDictionary ();
#endif
		public XmlSerializerNamespaces ()
		{
		}

		public XmlSerializerNamespaces(XmlQualifiedName[] namespaces)
			: this()
		{
			foreach(XmlQualifiedName qname in namespaces) 
			{
				this.namespaces.Add (qname.Name, qname);
			}
		}
	
		public XmlSerializerNamespaces(XmlSerializerNamespaces namespaces)
			: this(namespaces.ToArray())
		{}
	
		public void Add (string prefix, string ns)
		{
			XmlQualifiedName qname = new XmlQualifiedName(prefix,ns);
			namespaces[qname.Name] = qname;  
		}

		public XmlQualifiedName[] ToArray ()
		{
			XmlQualifiedName[] array = new XmlQualifiedName[namespaces.Count];
			namespaces.Values.CopyTo(array,0);
			return array;
		}
	
		public int Count 
		{
			get{ return namespaces.Count; }
		}

		internal string GetPrefix(string Ns)
		{
			foreach(string prefix in namespaces.Keys)
			{
				if(Ns == ((XmlQualifiedName)namespaces[prefix]).Namespace) 
					return prefix;
			}
			return null;
		}
#if MOONLIGHT
		internal IEnumerable<XmlQualifiedName> GetNamespaces ()
		{
			return namespaces.Values;
		}
#else
		internal ListDictionary Namespaces
		{
			get {
				return namespaces;
			}
		}
#endif
	}
}
