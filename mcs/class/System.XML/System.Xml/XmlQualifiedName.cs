//
// System.Xml.XmlQualifiedName.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

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
		[MonoTODO] public static readonly XmlQualifiedName Empty = new XmlQualifiedName ();
		
		private string name;
		private string namespace;
		
		public XmlQualifiedName (string name) {}

		// Properties
		public bool IsEmpty
		{
			if ((name == String.Empty) && (ns == String.Empty))
				return true;
			else
				return false;
		}

		public string Name
		{
			get { return name; }
		}

		public string Namespace
		{
			get { return namespace; }
		}

		// Methods
		public override bool Equals (object other)
		{
			if ((this.Name == other.Name) && (this.Namespace == other.Namespace))
				return true;
			else
				return false;
		}

		public override int GetHashCode ();

		public override string ToString ()
		{
			return ns + ":" + name;
		}

		public override string ToString (string name, string ns)
		{
			if (ns == null)
				return name;
			else
			 	return ns + ":" + name;
		}
	}
}
