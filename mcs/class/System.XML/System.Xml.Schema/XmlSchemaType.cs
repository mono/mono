//
// XmlSchemaType.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  atsushi@ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Xml;
using System.ComponentModel;
using System.Xml.Serialization;
#if NET_2_0_in_the_future
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
#if NET_2_0
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
#if NET_2_0
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

#if NET_2_0
		[MonoTODO]
		public XmlTypeCode TypeCode {
			get { throw new NotImplementedException (); }
		}
#endif
		#endregion

#if NET_2_0
		public static XmlSchemaComplexType GetBuiltInComplexType (XmlQualifiedName qualifiedName)
#else
		internal static XmlSchemaComplexType GetBuiltInComplexType (XmlQualifiedName qualifiedName)
#endif
		{
			if (qualifiedName == XmlSchemaComplexType.AnyType.QualifiedName)
				return XmlSchemaComplexType.AnyType;

			return null;
		}

#if NET_2_0
		[MonoTODO]
		public static XmlSchemaSimpleType GetBuiltInSimpleType (XmlQualifiedName qualifiedName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsDerivedFrom (XmlSchemaType derivedType, XmlSchemaType baseType, XmlSchemaDerivationMethod except)
		{
			throw new NotImplementedException ();
		}
#endif

		internal bool ValidateRecursionCheck ()
		{
			if (recursed)
				return (this != XmlSchemaComplexType.AnyType);
			recursed = true;
			XmlSchemaType baseType = this.BaseXmlSchemaType as XmlSchemaType;
			bool result = false;
			if (baseType != null)
				result = baseType.ValidateRecursionCheck ();
			recursed = false;
			return result;
		}
	}
}
