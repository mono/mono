//
// XmlElementAttributes.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Collections;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlElementAttributes.
	/// </summary>
	public class XmlElementAttributes : CollectionBase
	{
		public XmlElementAttribute this [int index] {
			get {
				return (XmlElementAttribute)List [index];
			}
			set {
				List [index] = value;
			}	
		}

		public int Add (XmlElementAttribute attribute)
		{
			return List.Add (attribute);
		}

		public bool Contains(XmlElementAttribute attribute)
		{
			return List.Contains(attribute);	
		}

		public int IndexOf(XmlElementAttribute attribute)
		{
			return List.IndexOf(attribute);
		}

		public void Insert(int index, XmlElementAttribute attribute)
		{
			List.Insert(index, attribute);
		}

		public void Remove(XmlElementAttribute attribute)
		{
			List.Remove(attribute);
		}
	}
}
