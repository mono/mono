//
// XmlSchemaSet.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
//
#if NET_1_2

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security.Policy;
using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml.Schema
{
	public class XmlSchemaSet
	{
		public int Count {
			get { throw new NotImplementedException (); }
		}

		public XmlSchemaObjectTable GlobalAttributes {
			get { throw new NotImplementedException (); }
		}

		public XmlSchemaObjectTable GlobalElements {
			get { throw new NotImplementedException (); }
		}

		public XmlSchemaObjectTable GlobalTypes { 
			get { throw new NotImplementedException (); }
		}

		public bool IsCompiled { 
			get { throw new NotImplementedException (); }
		}

		public XmlNameTable NameTable { 
			get { throw new NotImplementedException (); }
		}

		public XmlResolver XmlResolver { 
			set { throw new NotImplementedException (); }
		}


		public XmlSchema Add (string targetNamespace, string url)
		{
			throw new NotImplementedException ();
		}

		public XmlSchema Add (string targetNamespace, XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		public void Add (XmlSchemaSet schemaSet)
		{
			throw new NotImplementedException ();
		}

		public XmlSchema Add (XmlSchema schema)
		{
			throw new NotImplementedException ();
		}

		public void Compile ()
		{
			throw new NotImplementedException ();
		}

		public bool Contains (string targetNamespace)
		{
			throw new NotImplementedException ();
		}

		public bool Contains (XmlSchema targetNamespace)
		{
			throw new NotImplementedException ();
		}

		public void CopyTo (XmlSchema[] array, int index)
		{
			throw new NotImplementedException ();
		}

		public XmlSchema Remove (XmlSchema schema)
		{
			throw new NotImplementedException ();
		}

		public ArrayList Schemas ()
		{
			throw new NotImplementedException ();
		}

		public ArrayList Schemas (string targetNamespace)
		{
			throw new NotImplementedException ();
		}

		public  XmlSchemaSet ()  
		{
			throw new NotImplementedException ();
		}

		public  XmlSchemaSet (XmlNameTable nameTable)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
