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
		internal object baseSchemaTypeInternal;
		internal XmlSchemaDatatype datatypeInternal;
		internal XmlSchemaDerivationMethod resolvedDerivedBy;
		private XmlSchemaDerivationMethod final;
		internal XmlSchemaDerivationMethod finalResolved;
		private bool isMixed;
		private string name;
		internal XmlQualifiedName QNameInternal;
		bool recursed;

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
		#endregion

		#region XmlIgnore
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
			get{ return  baseSchemaTypeInternal; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod DerivedBy 
		{
			get{ return resolvedDerivedBy; }
		}
		[XmlIgnore]
		public XmlSchemaDatatype Datatype 
		{
			get{ return datatypeInternal; }
		}
		[XmlIgnore]
		public virtual bool IsMixed 
		{  
			get{ return  isMixed; }
			set{ isMixed = value; } 
		}
		#endregion

		internal bool ValidateRecursionCheck ()
		{
			if (recursed)
				return (this != XmlSchemaComplexType.AnyType);
			recursed = true;
			XmlSchemaType baseType = this.BaseSchemaType as XmlSchemaType;
			bool result = false;
			if (baseType != null)
				result = baseType.ValidateRecursionCheck ();
			recursed = false;
			return result;
		}
	}
}
