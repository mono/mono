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

		public bool Contains(XmlArrayItemAttribute attribute)
		{
			return List.Contains(attribute);
		}

		public void CopyTo(XmlArrayItemAttribute[] array, int index)
		{
			List.CopyTo(array, index);
		}

		public int IndexOf(XmlArrayItemAttribute attribute)
		{
			return List.IndexOf(attribute);
		}

		public void Insert(int index, XmlArrayItemAttribute attribute)
		{
			List.Insert(index, attribute);
		}

		public void Remove(XmlArrayItemAttribute attribute)
		{
			List.Remove(attribute);
		}
		
		internal bool InternalEquals (XmlArrayItemAttributes other)
		{
			if (other == null) return false;
			
			if (Count != other.Count) return false;
			for (int n=0; n<Count; n++)
				if (!this[n].InternalEquals (other[n])) return false;
			return true;
		}
	}
}
