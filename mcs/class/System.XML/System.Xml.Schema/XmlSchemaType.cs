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
		internal object BaseSchemaTypeInternal;
		private XmlSchemaDatatype datatype;
		private XmlSchemaDerivationMethod derivedBy;
		private XmlSchemaDerivationMethod final;
		internal XmlSchemaDerivationMethod finalResolved;
		private bool isMixed;
		private string name;
		internal XmlQualifiedName QNameInternal;

		public XmlSchemaType()
		{
			final = XmlSchemaDerivationMethod.None;
			QNameInternal = XmlQualifiedName.Empty;
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
		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return QNameInternal; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod FinalResolved 
		{
			get{ return finalResolved; }
		}
		[XmlIgnore]
		public object BaseSchemaType 
		{
			get{ return  BaseSchemaTypeInternal; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod DerivedBy 
		{
			get{ return derivedBy; }
		}
		[XmlIgnore]
		public XmlSchemaDatatype Datatype 
		{
			get{ return datatype; }
		}
		#endregion

		#region XmlIgnore
		[XmlIgnore]
		public virtual bool IsMixed 
		{  
			get{ return  isMixed; }
			set{ isMixed = value; } 
		}
		#endregion

	}
}
