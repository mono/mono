//
// XmlAnyElementAttributes.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAnyElementAttributes.
	/// </summary>
	public class XmlAnyElementAttributes : CollectionBase
	{
		
		public XmlAnyElementAttribute this[int index] 
		{
			get 
			{
				return (XmlAnyElementAttribute)List[index];
			}
			set 
			{
				List[index] = value;
			}	
		}

		public int Add(XmlAnyElementAttribute attribute)
		{
			return List.Add(attribute);
		}

		public bool Contains(XmlAnyElementAttribute attribute)
		{
			return List.Contains(attribute);	
		}

		public int IndexOf(XmlAnyElementAttribute attribute)
		{
			return List.IndexOf(attribute);
		}

		public void Insert(int index, XmlAnyElementAttribute attribute)
		{
			List.Insert(index, attribute);
		}

		public void Remove(XmlAnyElementAttribute attribute)
		{
			List.Remove(attribute);
		}

		public void CopyTo(XmlAnyElementAttribute[] array,int index)
		{
			List.CopyTo(array, index);
		}
		
		internal bool InternalEquals (XmlAnyElementAttributes other)
		{
			if (other == null) return false;
			
			if (Count != other.Count) return false;
			for (int n=0; n<Count; n++)
				if (!this[n].InternalEquals (other[n])) return false;
			return true;
		}
	}

}
