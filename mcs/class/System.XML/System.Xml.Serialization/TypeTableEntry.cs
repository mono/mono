//
// System.Xml.Serialization.TypeTableEntry
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
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

