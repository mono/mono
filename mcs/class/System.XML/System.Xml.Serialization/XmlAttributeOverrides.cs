//
// XmlAttributeOverrides.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System;
using System.Collections;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAttributeOverrides.
	/// </summary>
	public class XmlAttributeOverrides
	{
		
		private Hashtable overrides;

		public XmlAttributeOverrides ()
		{
			overrides = new Hashtable();
		}

		public XmlAttributes this [Type type] 
		{
			get { return this [type, string.Empty];	}
		}

		public XmlAttributes this [Type type, string member]
		{
			get 
			{
				return (XmlAttributes) overrides[GetKey(type,member)];
			}
		}

		public void Add (Type type, XmlAttributes attributes) 
		{
			Add(type, string.Empty, attributes);
		}

		public void Add (Type type, string member, XmlAttributes attributes) 
		{
			if(overrides[GetKey(type, member)] != null)
				throw new Exception("The attributes for the given type and Member already exist in the collection");
			
			overrides.Add(GetKey(type,member), attributes);
		}

		private TypeMember GetKey(Type type, string member)
		{
			return new TypeMember(type, member);
		}

		internal bool InternalEquals (XmlAttributeOverrides other)
		{
			if (other == null) return false;
			if (overrides.Count != other.overrides.Count) return false;
			
			foreach (DictionaryEntry entry in overrides)
			{
				XmlAttributes val = (XmlAttributes) other.overrides [entry.Key];
				if (val == null || !val.Equals ((XmlAttributes) entry.Value)) return false;
			}
			return true;
		}
	}
}
