//
// XmlTypeMapMemberElement.cs: 
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
using System.Collections;
using System.Globalization;

namespace System.Xml.Serialization
{
	// XmlTypeMapMemberElement
	// A member of a class that must be serialized as an XmlElement

	internal class XmlTypeMapMemberElement: XmlTypeMapMember
	{
		XmlTypeMapElementInfoList _elementInfo;
		string _choiceMember;
  		bool _isTextCollector;

		public XmlTypeMapMemberElement()
		{
		}

		public XmlTypeMapElementInfoList ElementInfo
		{
			get
			{
				if (_elementInfo == null) _elementInfo = new XmlTypeMapElementInfoList ();
				return _elementInfo;
			}
			set { _elementInfo = value; }
		}

		public string ChoiceMember
		{
			get { return _choiceMember; }
			set { _choiceMember = value; }
		}

		public XmlTypeMapElementInfo FindElement (object ob, object memberValue)
		{
			if (_elementInfo.Count == 1) 
				return (XmlTypeMapElementInfo) _elementInfo[0];
			else if (_choiceMember != null)
			{
				object value = GetValue (ob, _choiceMember);
				string choiceValue = value is IFormattable ? ((IFormattable) value).ToString (null, CultureInfo.InvariantCulture) : value.ToString();
				foreach (XmlTypeMapElementInfo elem in _elementInfo)
					if (elem.ChoiceValue == choiceValue) return elem;
			}
			else
			{
				if (memberValue == null)
					return (XmlTypeMapElementInfo) _elementInfo[0];
				Type type = memberValue.GetType();
				foreach (XmlTypeMapElementInfo elem in _elementInfo)
					if (elem.TypeData.Type == type) return elem;
			}
			return null;
		}

		public bool IsXmlTextCollector
		{
			get { return _isTextCollector; }
			set { _isTextCollector = value; }
		}
	}

	// XmlTypeMapMemberList
	// A member of a class that must be serialized as a list

	internal class XmlTypeMapMemberList : XmlTypeMapMemberElement
	{
		public XmlTypeMapping ListTypeMapping
		{
			get { return ((XmlTypeMapElementInfo) ElementInfo[0]).MappedType; }
		}

		public string ElementName
		{
			get { return ((XmlTypeMapElementInfo) ElementInfo[0]).ElementName; }
		}

		public string Namespace
		{
			get { return ((XmlTypeMapElementInfo) ElementInfo[0]).Namespace; }
		}
	}

	// XmlTypeMapMemberFlatList
	// A member of a class that must be serialized as a flat list

	internal class XmlTypeMapMemberExpandable : XmlTypeMapMemberElement
	{
		int _flatArrayIndex;

		public int FlatArrayIndex
		{
			get { return _flatArrayIndex; }
			set { _flatArrayIndex = value; }
		}
	}

	internal class XmlTypeMapMemberFlatList : XmlTypeMapMemberExpandable
	{
		ListMap _listMap;

		public ListMap ListMap
		{
			get { return _listMap; }
			set { _listMap = value; }
		}
	}

	internal class XmlTypeMapMemberAnyElement : XmlTypeMapMemberExpandable
	{
		public bool IsElementDefined (string name, string ns)
		{
			foreach (XmlTypeMapElementInfo elem in ElementInfo)
			{
				if (elem.IsUnnamedAnyElement)		// Default AnyElementAttribute
					return true;

				if (elem.ElementName == name && elem.Namespace == ns)
					return true;
			}
			return false;
		}

		public bool IsDefaultAny
		{
			get
			{
				foreach (XmlTypeMapElementInfo elem in ElementInfo)
				{
					if (elem.IsUnnamedAnyElement) 
						return true;
				}
				return false;
			}
		}
		
		public bool CanBeText
		{
			get
			{
				return (ElementInfo.Count > 0) && ((XmlTypeMapElementInfo)ElementInfo[0]).IsTextElement;
			}
		}
	}

	internal class XmlTypeMapMemberAnyAttribute: XmlTypeMapMember
	{
	}
}
