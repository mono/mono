// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaType.
	/// </summary>
	public class XmlSchemaType : XmlSchemaAnnotated
	{
		private object baseSchemaType;
		private XmlSchemaDatatype datatype;
		private XmlSchemaDerivationMethod derivedBy;
		private XmlSchemaDerivationMethod final;
		protected XmlSchemaDerivationMethod finalResolved;
		private bool isMixed;
		private string name;
		protected XmlQualifiedName qName;

		public XmlSchemaType()
		{
			final = XmlSchemaDerivationMethod.None;
		}

		#region Attributes
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return name; }
			set{ name = value; }
		}
		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute("final")]
		public XmlSchemaDerivationMethod Final 
		{
			get{ return  final; }
			set{ final = value; }
		}
		#endregion

		#region XmlIgnore
		[XmlIgnore]
		public object BaseSchemaType 
		{
			get{ return  baseSchemaType; }
		}
		[XmlIgnore]
		public XmlSchemaDatatype Datatype 
		{
			get{ return datatype; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod DerivedBy 
		{
			get{ return derivedBy; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod FinalResolved 
		{
			get{ return finalResolved; }
		}
		[XmlIgnore]
		public virtual bool IsMixed 
		{  
			get{ return  isMixed; }
			set{ isMixed = value; } 
		}
		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return qName; }
		}
		#endregion
	}
}
