using System;
using System.Xml;
using System.Collections;

namespace System.Xml.Schema
{
	/// <summary>
	/// This class would store the infomation we need during compilation.
	/// </summary>
	internal class XmlSchemaInfo
	{
		internal XmlSchemaInfo()
		{
			IDCollection = new Hashtable();
		}

		internal string TargetNamespace = null;
		internal XmlSchemaDerivationMethod FinalDefault = XmlSchemaDerivationMethod.None;
		internal XmlSchemaDerivationMethod BlockDefault = XmlSchemaDerivationMethod.None;
		internal XmlSchemaForm ElementFormDefault = XmlSchemaForm.None;
		internal XmlSchemaForm AttributeFormDefault = XmlSchemaForm.None;
		internal Hashtable IDCollection;
		internal XmlSchemaObjectTable SchemaTypes ;
	}
}