//
// System.Xml.XmlQualifiedName.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Xml
{
	public class XmlQualifiedName
	{
		// Constructors		
		public XmlQualifiedName ()
			: base ()
		{
		}

		public XmlQualifiedName (string name)
			: base ()
		{
			this.name = name;
		}

		public XmlQualifiedName (string name, string ns)
			: base ()
		{
			this.name = name;
			this.ns = ns;
		}

		// Fields
		public static readonly XmlQualifiedName Empty = new XmlQualifiedName ();		
		private string name;
		private string ns;
		
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
			if ((XmlQualifiedName) this == (XmlQualifiedName) other)
				return true;
			else
				return false;
		}

		[MonoTODO] public override int GetHashCode () { return 42; }

		public override string ToString ()
		{
			if (ns == null)
				return name;
			else
			 	return ns + ":" + name;
		}

		public static string ToString (string name, string ns)
		{
			if (ns == null)
				return name;
			else				
				return ns + ":" + name;
		}

		// Operators
		public static bool operator == (XmlQualifiedName a, XmlQualifiedName b)
		{
			if ((a.Name == b.Name) && (a.Namespace == b.Namespace))
				return true;
			else
				return false;
		}

		public static bool operator != (XmlQualifiedName a, XmlQualifiedName b)
		{
			if (!(a == b))
				return false;
			else
				return true;
		}
	}
}
