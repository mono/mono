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
		internal object actualBaseSchemaType;

		protected XmlSchemaContent()
		{}

		internal object ActualBaseSchemaType
		{
			get { return actualBaseSchemaType; }
		}

		internal abstract XmlQualifiedName GetBaseTypeName ();

		internal virtual XmlSchemaParticle GetParticle ()
		{
			return null; // default for simple types
		}
	}
}