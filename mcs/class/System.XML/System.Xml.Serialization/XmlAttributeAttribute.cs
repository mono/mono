//
// XmlAttributeAttribute.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Xml.Schema;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAttributeAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		 | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public class XmlAttributeAttribute : Attribute
	{
		private string attributeName;
		private string dataType;
		private Type type;
		private XmlSchemaForm form;
		private string ns;

		public XmlAttributeAttribute ()
		{
		}

		public XmlAttributeAttribute (string attributeName)
		{
			AttributeName = attributeName;
		}
		
		public XmlAttributeAttribute (Type type)
		{
			Type = type;
		}

		public XmlAttributeAttribute (string attributeName, Type type)
		{
			AttributeName = attributeName;
			Type = type;
		}

		public string AttributeName {
			get {
				return attributeName;
			}
			set {
				attributeName = value;
			}
		}
		public string DataType {
			get {
				return dataType;
			}
			set {
				dataType = value;
			}
		}
		public XmlSchemaForm Form {
			get {
				return form;
			}
			set {
				form = value;
			}
		}
		public string Namespace {
			get {
				return ns;
			}
			set {
				ns = value;
			}
		}

		public Type Type
		{
			get 
			{
				return type;
			}
			set 
			{
				type = value;
			}
		}

	}
}
