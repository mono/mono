// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaGroupBase.
	/// </summary>
	public abstract class XmlSchemaGroupBase : XmlSchemaParticle
	{
		protected XmlSchemaGroupBase()
		{
		}

		[XmlIgnore]
		public abstract XmlSchemaObjectCollection Items { get; }
	}
}
