//
// System.Xml.Serialization.TypeTableEntry
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Collections;

namespace System.Xml.Serialization
{
	internal class TypeTableEntry
	{
		Hashtable memberTable;
		XmlAttributes xmlAttributes;
		ArrayList elementMembers;
		ArrayList attributeMembers;

		public TypeTableEntry (Hashtable memberTable,
			XmlAttributes xmlAttributes,
			ArrayList elementMembers,
			ArrayList attributeMembers)
		{
			this.memberTable = memberTable;
			this.xmlAttributes = xmlAttributes;
			this.elementMembers = elementMembers;
			this.attributeMembers = attributeMembers;
		}

		public Hashtable MemberTable {
			get { return memberTable; }
		}

		public XmlAttributes XmlAttributes {
			get { return xmlAttributes; }
			set { xmlAttributes = value; }
		}

		public ArrayList ElementMembers {
			get { return elementMembers; }
		}

		public ArrayList AttributeMembers {
			get { return attributeMembers; }
		}
	}

	internal class TypeTablePool
	{
		Hashtable pool = new Hashtable ();

		public TypeTablePool () {}

		public TypeTableEntry this [Type t] {
			get { return (TypeTableEntry) pool [t]; }
			set { pool [t] = value; }
		}

		public TypeTableEntry Get (Type t)
		{
			return this [t];
		}

		public void Add (Type t, TypeTableEntry tte)
		{
			pool.Add (t, tte);
		}

		public bool Contains (Type t)
		{
			return pool.Contains (t);
		}
	}
}

