//
// XmlArrayItemAttribute.cs: 
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
	/// Summary description for XmlArrayItemAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		 | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=true)]
	public class XmlArrayItemAttribute : Attribute
	{
		private string dataType;
		private string elementName;
		private XmlSchemaForm form;
		private string ns;
		private bool isNullable = true;
		private int nestingLevel;
		private Type type;

		public XmlArrayItemAttribute ()
		{
		}
		public XmlArrayItemAttribute (string elementName)
		{
			ElementName = elementName;
		}
		public XmlArrayItemAttribute (Type type)
		{
			Type = type;
		}
		public XmlArrayItemAttribute (string elementName, Type type)
		{
			ElementName = elementName;
			Type = type;
		}

		public string DataType {
			get { return dataType; }
			set { dataType = value; }
		}
		public string ElementName {
			get { return elementName; }
			set { elementName = value; }
		}
		public XmlSchemaForm Form {
			get { return form; }
			set { form = value; }
		}
		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}
		public bool IsNullable {
			get { return isNullable; } 
			set { isNullable = value; }
		}
		public Type Type {
			get { return type; }
			set { type = value; }
		}
		public int NestingLevel {
			get { return nestingLevel; }
			set { nestingLevel = value; }
		}
		
		internal bool InternalEquals (XmlArrayItemAttribute other)
		{
			if (other == null) return false;
			
			return (dataType == other.dataType &&
					elementName == other.elementName &&
					form == other.form &&
					ns == other.ns &&
					isNullable == other.isNullable &&
					nestingLevel == other.nestingLevel &&
					type == other.type);
		}
	}
}
