//
// XmlTypeMapMemberElement.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;

namespace System.Xml.Serialization
{
	// XmlTypeMapMemberElement
	// A member of a class that must be serialized as an XmlElement

	internal class XmlTypeMapMemberElement: XmlTypeMapMember
	{
		XmlTypeMapElementInfoList _elementInfo;
		string _choiceMember;

		public XmlTypeMapMemberElement()
		{
		}

		public XmlTypeMapElementInfoList ElementInfo
		{
			get { return _elementInfo; }
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
				string choiceValue = GetValue (ob, _choiceMember).ToString();
				foreach (XmlTypeMapElementInfo elem in _elementInfo)
					if (elem.ChoiceValue == choiceValue) return elem;
			}
			else
			{
				Type type = memberValue.GetType();
				foreach (XmlTypeMapElementInfo elem in _elementInfo)
					if (elem.TypeData.Type == type) return elem;
			}
			return null;
		}
	}

	// XmlTypeMapMemberList
	// A member of a class that must be serialized as a list

	internal class XmlTypeMapMemberList : XmlTypeMapMemberElement
	{
		XmlTypeMapping _listMap;
		string _elementName;
		string _namespace;

		public XmlTypeMapping ListTypeMapping
		{
			get { return _listMap; }
			set { _listMap = value; }
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
	}

	// XmlTypeMapMemberFlatList
	// A member of a class that must be serialized as a flat list

	internal class XmlTypeMapMemberFlatList : XmlTypeMapMemberElement
	{
		ListMap _listMap;
		int _flatArrayIndex;

		public ListMap ListMap
		{
			get { return _listMap; }
			set { _listMap = value; }
		}

		public int FlatArrayIndex
		{
			get { return _flatArrayIndex; }
			set { _flatArrayIndex = value; }
		}
	}
}
