//
// RelaxngDatatype.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
using System;
using System.Xml;

namespace Commons.Xml.Relaxng
{
	public abstract class RelaxngDatatype
	{
		public abstract string Name { get; }
		public abstract string NamespaceURI { get; }

		public abstract object Parse (string text, XmlReader reader);

		public virtual bool Compare (object o1, object o2)
		{
			return (o1 == o2);
		}

		public virtual bool CompareString (string s1, string s2, XmlReader reader)
		{
			return Compare (Parse (s1, reader), Parse (s2, reader));
		}

		public virtual bool IsValid (string text, XmlReader reader) 
		{
			try {
				Parse (text, reader);
			} catch (Exception) {
				return false;
			}
			return true;
		}
	}
}
