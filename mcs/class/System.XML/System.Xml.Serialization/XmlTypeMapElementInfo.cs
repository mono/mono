//
// XmlTypeMapElementInfo.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Xml.Schema;
using System.Collections;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlTypeMapElementInfo.
	/// </summary>
	internal class XmlTypeMapElementInfo
	{
		string _elementName;
		string _dataType;
		string _namespace = "";
		XmlSchemaForm _form;
		XmlTypeMapMember _member;
		string _choiceValue;
		bool _isNullable;
		int _nestingLevel;	// Only for array items
		XmlTypeMapping _mappedType;
		TypeData _type;

		public XmlTypeMapElementInfo (XmlTypeMapMember member, TypeData type)
		{
			_member = member;
			_type = type;
		}

		public TypeData TypeData
		{
			get { return _type; }
		}

		public string ChoiceValue
		{
			get { return _choiceValue; }
			set { _choiceValue = value; }
		}

		public string ElementName
		{
			get { return _elementName; }
			set { _elementName = value; }
		}

		public string Namespace
		{
			get { return _namespace; }
			set { _namespace = value; }
		}

		public string DataType
		{
			get { return _dataType; }
			set { _dataType = value; }
		}

		public string DataTypeNamespace
		{
			get 
			{ 
				if (_mappedType == null) return XmlSchema.Namespace;
				else return _mappedType.Namespace;
			}
		}

		public XmlSchemaForm Form 
		{
			get { return _form; }
			set { _form = value; }
		}

		public XmlTypeMapping MappedType
		{
			get { return _mappedType; }
			set { _mappedType = value; }
		}

		public bool IsNullable
		{
			get { return _isNullable; }
			set { _isNullable = value; }
		}

		internal bool IsPrimitive
		{
			get { return _mappedType == null; }
		}

		public XmlTypeMapMember Member
		{
			get { return _member; }
			set { _member = value; }
		}

		public int NestingLevel
		{
			get { return _nestingLevel; }
			set { _nestingLevel = value; }
		}

		internal bool MultiReferenceType
		{
			get 
			{ 
				if (_mappedType != null) return _mappedType.MultiReferenceType;
				else return false;
			}
		}

		public override bool Equals (object other)
		{
			XmlTypeMapElementInfo oinfo = (XmlTypeMapElementInfo)other;
			if (_elementName != oinfo._elementName) return false;
			if (_dataType != oinfo._dataType) return false;
			if (_namespace != oinfo._namespace) return false;
			if (_form != oinfo._form) return false;
			if (_choiceValue != oinfo._choiceValue) return false;
			if (_type.Type != oinfo._type.Type) return false;
			if (_isNullable != oinfo._isNullable) return false;
			if (_nestingLevel != oinfo._nestingLevel) return false;
			return true;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}

	class XmlTypeMapElementInfoList: ArrayList
	{
	}
}

