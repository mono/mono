//
// XmlArrayItemAttributes.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Xml.Serialization;
using System.Collections;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlArrayItemAttributes.
	/// </summary>
	public class XmlArrayItemAttributes : CollectionBase
	{

		public XmlArrayItemAttribute this [int index] {
			get {
				return (XmlArrayItemAttribute)List [index];
			}
			set {
				List [index] = value;
			}	
		}

		public int Add (XmlArrayItemAttribute attribute)
		{
			return List.Add(attribute);
		}

	}
}
