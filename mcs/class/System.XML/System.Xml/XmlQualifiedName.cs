//
// System.Xml.XmlQualifiedName.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//		 
// (C) Ximian, Inc.
// 
// Modified: 
//		21st June 2002 : Ajay kumar Dwivedi (adwiv@yahoo.com)

using System;

namespace System.Xml
{
	public class XmlQualifiedName
	{
		// Constructors		
		public XmlQualifiedName ()
			: this (string.Empty, string.Empty)
		{
		}

		public XmlQualifiedName (string name)
			: this (name, string.Empty)
		{
		}

		public XmlQualifiedName (string name, string ns)
			: base ()
		{
			this.name = (name == null) ? "" : name;
			this.ns = (ns == null) ? "" : ns;
			this.hash = name.GetHashCode () ^ ns.GetHashCode ();
		}

		// Fields
		public static readonly XmlQualifiedName Empty = new XmlQualifiedName ();		
		private readonly string name;
		private readonly string ns;
		private readonly int hash;
		
		// Properties
		public bool IsEmpty
		{
			get {
				return name.Length == 0 && ns.Length == 0;
			}
		}

		public string Name
		{
			get { return name; }
		}

		public string Namespace
		{
			get { return ns; }
		}

		// Methods
		public override bool Equals (object other)
		{
			return this == (other as XmlQualifiedName);
		}

		public override int GetHashCode () 
		{ 
			return hash;
		}

		public override string ToString ()
		{
			if (ns == string.Empty)
				return name;
			else
			 	return ns + ":" + name;
		}

		public static string ToString (string name, string ns)
		{
			if (ns == string.Empty)
				return name;
			else				
				return ns + ":" + name;
		}

		// Operators
		public static bool operator == (XmlQualifiedName a, XmlQualifiedName b)
		{
			if((Object)a == (Object)b)
				return true;

			if((Object)a == null || (Object)b == null)
				return false;

			return (a.hash == b.hash) && (a.name == b.name) && (a.ns == b.ns);
		}

		public static bool operator != (XmlQualifiedName a, XmlQualifiedName b)
		{
			return !(a == b);
		}
	}
}
