//
// XmlTypeMapMember.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Xml.Serialization
{
	// XmlTypeMapMember
	// A member of a class that must be serialized

	internal class XmlTypeMapMember
	{
		string _name;
		int _index;
		TypeData _typeData;

		public XmlTypeMapMember()
		{
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public object GetValue (object ob)
		{
			return GetValue (ob, _name);
		}

		public static object GetValue (object ob, string name)
		{
			MemberInfo[] mems = ob.GetType().GetMember (name, BindingFlags.Instance|BindingFlags.Public);
			if (mems[0] is PropertyInfo) return ((PropertyInfo)mems[0]).GetValue (ob, null);
			else return ((FieldInfo)mems[0]).GetValue (ob);
		}

		public void SetValue (object ob, object value)
		{
			MemberInfo[] mems = ob.GetType().GetMember (_name, BindingFlags.Instance|BindingFlags.Public);
			if (mems[0] is PropertyInfo) ((PropertyInfo)mems[0]).SetValue (ob, value, null);
			else ((FieldInfo)mems[0]).SetValue (ob, value);
		}

		public TypeData TypeData
		{
			get { return _typeData; }
			set { _typeData = value; }
		}

		public int Index
		{
			get { return _index; }
			set { _index = value; }
		}
	}
}
