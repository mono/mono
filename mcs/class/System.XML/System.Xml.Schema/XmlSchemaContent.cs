//
// System.Xml.Schema.XmlSchemaContent.cs
//
// Author:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  ginga@kit.hi-ho.ne.jp
//
using System;

namespace System.Xml.Schema
{
	public abstract class XmlSchemaContent : XmlSchemaAnnotated
	{
		protected XmlSchemaContent()
		{}

		internal object actualBaseSchemaType;

		internal abstract bool IsExtension { get; }

		internal abstract XmlQualifiedName GetBaseTypeName ();

		internal abstract XmlSchemaParticle GetParticle ();
	}
}
