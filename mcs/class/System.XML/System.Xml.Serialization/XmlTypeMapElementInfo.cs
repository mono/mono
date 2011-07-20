//
// XmlTypeMapElementInfo.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
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
		string _namespace = "";
		XmlSchemaForm _form;
		XmlTypeMapMember _member;
		object _choiceValue;
		bool _isNullable;
		int _nestingLevel;	// Only for array items
		XmlTypeMapping _mappedType;
		TypeData _type;
		bool _wrappedElement = true;
		int _explicitOrder = -1;
		
		public XmlTypeMapElementInfo (XmlTypeMapMember member, TypeData type)
		{
			_member = member;
			_type = type;
			if (type.IsValueType && type.IsNullable)
				_isNullable = true;
		}

		public TypeData TypeData
		{
			get { return _type; }
			set { _type = value; }
		}

		public object ChoiceValue
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

		public string DataTypeNamespace
		{
			get 
			{ 
				if (_mappedType == null) return XmlSchema.Namespace;
				else return _mappedType.XmlTypeNamespace;
			}
		}

		public string DataTypeName
		{
			get 
			{ 
				if (_mappedType == null) return TypeData.XmlType;
				else return _mappedType.XmlType;
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

		public bool MultiReferenceType
		{
			get 
			{ 
				if (_mappedType != null) return _mappedType.MultiReferenceType;
				else return false;
			}
		}

		public bool WrappedElement
		{
			get { return _wrappedElement; }
			set { _wrappedElement = value; }
		}

		public bool IsTextElement
		{
			get { return ElementName == "<text>"; }
			set {
				if (!value)
					throw new Exception ("INTERNAL ERROR; someone wrote unexpected code in sys.xml");
				ElementName = "<text>"; Namespace = string.Empty;
			}
		}

		public bool IsUnnamedAnyElement
		{
			get { return ElementName == string.Empty; }
			set {
				if (!value)
					throw new Exception ("INTERNAL ERROR; someone wrote unexpected code in sys.xml");
				ElementName = string.Empty; Namespace = string.Empty;
			}
		}

		public int ExplicitOrder
		{
			get { return _explicitOrder; }
			set { _explicitOrder = value; }
		}

		public override bool Equals (object other)
		{
			if (other == null)
				return false;
			XmlTypeMapElementInfo oinfo = (XmlTypeMapElementInfo)other;
			if (_elementName != oinfo._elementName) return false;
			if (_type.XmlType != oinfo._type.XmlType) return false;
			if (_namespace != oinfo._namespace) return false;
			if (_form != oinfo._form) return false;
			if (_type != oinfo._type) return false;
			if (_isNullable != oinfo._isNullable) return false;
			if (_choiceValue != null && !_choiceValue.Equals (oinfo._choiceValue)) return false;
			if (_choiceValue != oinfo._choiceValue) return false;
			return true;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}

	class XmlTypeMapElementInfoList: ArrayList
	{
		public int IndexOfElement (string name, string namspace)
		{
			for (int n=0; n<Count; n++) {
				XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) base [n];
				if (info.ElementName == name && info.Namespace == namspace)
					return n;
			}
			return -1;
		}
	}
}

