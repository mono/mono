// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeContent.
	/// </summary>
	public abstract class XmlSchemaSimpleTypeContent : XmlSchemaAnnotated
	{
		protected XmlSchemaSimpleTypeContent()
		{
		}

		internal object actualBaseSchemaType;

		internal object ActualBaseSchemaType {
			get { return actualBaseSchemaType; }
		}

		internal void ValidateActualType (XmlSchemaSimpleType baseType,
			XmlQualifiedName baseTypeName, ValidationEventHandler h, XmlSchema schema)
		{
			XmlSchemaSimpleType type = baseType;
			if (type == null)
				type = schema.SchemaTypes [baseTypeName] as XmlSchemaSimpleType;
			if (type != null) {
				errorCount += type.Validate (h, schema);
				actualBaseSchemaType = type;
			} else if (baseTypeName == XmlSchemaComplexType.AnyTypeName) {
//				actualBaseSchemaType = XmlSchemaComplexType.AnyType;
				actualBaseSchemaType = XmlSchemaSimpleType.AnySimpleType;
			} else if (baseTypeName.Namespace == XmlSchema.Namespace) {
				actualBaseSchemaType = XmlSchemaDatatype.FromName (baseTypeName);
				if (actualBaseSchemaType == null)
					error (h, "Invalid schema type name was specified: " + baseTypeName);
			}
			// otherwise, it might be missing sub components.
			else if (!schema.IsNamespaceAbsent (baseTypeName.Namespace))
				error (h, "Referenced base schema type " + baseTypeName + " was not found in the corresponding schema.");
		}

		internal virtual string Normalize (string s, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return s;
		}

	}
}
