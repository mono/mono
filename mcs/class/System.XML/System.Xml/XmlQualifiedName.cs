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
		}

		// Fields
		public static readonly XmlQualifiedName Empty = new XmlQualifiedName ();		
		private readonly string name;
		private readonly string ns;
		
		// Properties
		public bool IsEmpty
		{
			get {
				if ((name == String.Empty) && (ns == String.Empty))
					return true;
				else
					return false;
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
			if(!(other is XmlQualifiedName))
				return false;

			if ((XmlQualifiedName) this == (XmlQualifiedName) other)
				return true;
			else
				return false;
		}

		public override int GetHashCode () 
		{ 
			return unchecked (name.GetHashCode () + ns.GetHashCode ());
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

			if ((a.Name == b.Name) && (a.Namespace == b.Namespace))
				return true;
			else
				return false;
		}

		public static bool operator != (XmlQualifiedName a, XmlQualifiedName b)
		{
			return !(a == b);
		}
	}
}
