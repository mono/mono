//
// XmlSerializerNamespaces.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System;
using System.Xml;
using System.Collections;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlSerializerNamespaces.
	/// </summary>
	public class XmlSerializerNamespaces
	{
		private Hashtable namespaces;
			
		public XmlSerializerNamespaces ()
		{
			namespaces = new Hashtable ();
		}
	
		public XmlSerializerNamespaces(XmlQualifiedName[] namespaces)
			: this()
		{
			foreach(XmlQualifiedName qname in namespaces) 
			{
				this.namespaces[qname.Name] = qname;
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
	
	}
}
