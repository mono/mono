using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// This class would store the infomation we need during compilation.
	/// (maybe during validation too.. who knows)
	/// </summary>
	internal class XmlSchemaInfo
	{
		internal XmlSchemaInfo()
		{}

		internal string targetNS;
		internal XmlSchemaDerivationMethod finalDefault;
		internal XmlSchemaDerivationMethod blockDefault;
	}
}
