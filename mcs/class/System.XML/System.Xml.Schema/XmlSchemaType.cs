//
// XmlSchemaType.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  atsushi@ximian.com
//
using System;
using System.Xml;
using System.ComponentModel;
using System.Xml.Serialization;
#if NET_1_2_in_the_future
using MS.Internal.Xml;
#endif

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaType.
	/// </summary>
	public class XmlSchemaType : XmlSchemaAnnotated
	{
		private XmlSchemaDerivationMethod final;
		private bool isMixed;
		private string name;
		bool recursed;

		internal XmlQualifiedName BaseSchemaTypeName;
		internal XmlSchemaType BaseXmlSchemaTypeInternal;
		internal XmlSchemaDatatype DatatypeInternal;
		internal XmlSchemaDerivationMethod resolvedDerivedBy;
		internal XmlSchemaDerivationMethod finalResolved;
		internal XmlQualifiedName QNameInternal;

		public XmlSchemaType ()
		{
			final = XmlSchemaDerivationMethod.None;
			QNameInternal = XmlQualifiedName.Empty;
		}

		#region Attributes
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name {
			get{ return name; }
			set{ name = value; }
		}

		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[System.Xml.Serialization.XmlAttribute("final")]
		public XmlSchemaDerivationMethod Final {
			get{ return  final; }
			set{ final = value; }
		}
		#endregion

		#region Post Compilation Schema Information
		[XmlIgnore]
		public XmlQualifiedName QualifiedName {
			get{ return QNameInternal; }
		}

		[XmlIgnore]
		public XmlSchemaDerivationMethod FinalResolved {
			get{ return finalResolved; }
		}

		[XmlIgnore]
#if NET_1_2
		[Obsolete ("This property is going away. Use BaseXmlSchemaType instead")]
#endif
		public object BaseSchemaType {
			get{
				if (BaseXmlSchemaType != null)
					return BaseXmlSchemaType;
				else if (this == XmlSchemaComplexType.AnyType)
					return null; // This property is designed so.
				else
					return Datatype;
			}
		}

		[XmlIgnore]
#if NET_1_2
		public XmlSchemaType BaseXmlSchemaType {
#else
		internal XmlSchemaType BaseXmlSchemaType {
#endif
			get { return  BaseXmlSchemaTypeInternal; }
		}

		[XmlIgnore]
		public XmlSchemaDerivationMethod DerivedBy {
			get{ return resolvedDerivedBy; }
		}

		[XmlIgnore]
		public XmlSchemaDatatype Datatype {
			get{ return DatatypeInternal; }
		}

		[XmlIgnore]
		public virtual bool IsMixed {  
			get{ return  isMixed; }
			set{ isMixed = value; } 
		}
		#endregion

#if NET_1_2
		public static XmlSchemaComplexType GetBuiltInComplexType (XmlQualifiedName qualifiedName)
#else
		internal static XmlSchemaComplexType GetBuiltInComplexType (XmlQualifiedName qualifiedName)
#endif
		{
			if (qualifiedName == XmlSchemaComplexType.AnyType.QualifiedName)
				return XmlSchemaComplexType.AnyType;

			return null;
		}

#if NET_1_2_in_the_future
		public static XmlSchemaSimpleType GetBuiltInSimpleType (XmlQualifiedName qualifiedName)
		{
			if (qualifiedName.Name == "anySimpleType" && qualifiedName.Namespace == XmlSchema.Namespace)
				return XmlSchemaSimpleType.AnySimpleType;

			return null;
		}

		public virtual IXmlInfosetReader Validate (IXmlInfosetReader reader, object schemas)
		{
			throw new NotImplementedException ();
		}

		public virtual IXmlInfosetWriter Validate (IXmlInfosetWriter reader, object schemas)
		{
			throw new NotImplementedException ();
		}
#endif

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
