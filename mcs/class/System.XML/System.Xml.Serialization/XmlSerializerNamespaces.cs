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
		private ArrayList xmlQualifiedNames;
		
		public XmlSerializerNamespaces ()
		{
			xmlQualifiedNames = new ArrayList ();
		}

		[MonoTODO]
		public XmlSerializerNamespaces(XmlQualifiedName[] namespaces)
		{
		}

		[MonoTODO]
		public XmlSerializerNamespaces(XmlSerializerNamespaces namespaces)
		{
		}

		public void Add (string prefix, string ns)
		{
			xmlQualifiedNames.Add (new XmlQualifiedName (prefix, ns) );
		}

		public XmlQualifiedName[] ToArray ()
		{
			return (XmlQualifiedName[])xmlQualifiedNames.ToArray ();
		}

		public int Count 
		{
			get{ return xmlQualifiedNames.Count; }
		}

	}
}
