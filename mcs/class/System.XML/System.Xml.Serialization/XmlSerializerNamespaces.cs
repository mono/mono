//
// XmlSerializerNamespaces.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
// Modified: 
//		June 22, 2002 Ajay kumar Dwivedi (adwiv@yahoo.com)

using System;
using System.Xml;
using System.Collections;

namespace System.Xml.Serialization
{
	/// <summary>
	/// XmlSerializerNamespaces contains the unique mapping between a prefix and namespace.
	/// For a given prefix, we should have exactly one namespace.
	/// Ideally, for a given namespace there should be exactly one prefix.But 
	/// this is not enforced in MS implementation. We enforce both conditions.
	/// </summary>
	/// <remarks>
	/// XmlSerializerNamespaces can be used both during serialization and deserialization.
	/// During serialization, we need key to be the namespace whereas during deserialization,
	/// the key should be prefix. So we maintain both the mappings in two hashtables.
	/// Both the tables must always be synchronized.
	/// </remarks>
	public class XmlSerializerNamespaces
	{
		private Hashtable nameToNsMap;
		private Hashtable nsToNameMap;

		public XmlSerializerNamespaces ()
		{
			nameToNsMap = new Hashtable ();
			nsToNameMap = new Hashtable ();
		}

		public XmlSerializerNamespaces(XmlQualifiedName[] namespaces)
			: this()
		{
			foreach(XmlQualifiedName qname in namespaces)
			{
				//Remove the keys if value is present.
				if(nameToNsMap.ContainsKey(qname.Name))
				{
					nsToNameMap.Remove(nameToNsMap[qname.Name]);
					nameToNsMap.Remove(qname.Name);
				}
				if(nsToNameMap.ContainsKey(qname.Namespace))
				{
					nameToNsMap.Remove(nsToNameMap[qname.Namespace]);
					nsToNameMap.Remove(qname.Namespace);
				}
				nameToNsMap.Add(qname.Name, qname.Namespace);
				nsToNameMap.Add(qname.Namespace, qname.Name);
			}
		}

		public XmlSerializerNamespaces(XmlSerializerNamespaces namespaces)
			: this(namespaces.ToArray())
		{
		}

		public void Add (string prefix, string ns)
		{
			nameToNsMap.Add(prefix, ns);
			nsToNameMap.Add(ns, prefix);
		}

		public XmlQualifiedName[] ToArray ()
		{
			XmlQualifiedName[] array  = new XmlQualifiedName[Count];
			int i=0;
			foreach(string name in nameToNsMap.Keys)
			{
				array[i++] = new XmlQualifiedName(name, (string)nameToNsMap[name]);
			}
			return array;
		}

		public int Count 
		{
			get{ return nsToNameMap.Count; }
		}

	}
}
