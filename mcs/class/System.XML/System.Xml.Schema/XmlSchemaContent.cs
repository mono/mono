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
	/// <summary>
	/// Summary description for XmlSchemaContent.
	/// </summary>
	public abstract class XmlSchemaContent : XmlSchemaAnnotated
	{
		protected object actualBaseSchemaType;

		protected XmlSchemaContent()
		{}

		internal abstract bool IsExtension { get; }

		internal abstract XmlQualifiedName GetBaseTypeName ();

		internal abstract XmlSchemaParticle GetParticle ();
	}
}
