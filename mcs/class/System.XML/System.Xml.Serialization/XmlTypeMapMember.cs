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
		MemberInfo _member;

		public XmlTypeMapMember()
		{
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public bool IsReadOnly (Type type)
		{
			if (_member == null) InitMember (type);
			return (_member is PropertyInfo) && !((PropertyInfo)_member).CanWrite;
		}

		public static object GetValue (object ob, string name)
		{
			MemberInfo[] mems = ob.GetType().GetMember (name, BindingFlags.Instance|BindingFlags.Public);
			if (mems[0] is PropertyInfo) return ((PropertyInfo)mems[0]).GetValue (ob, null);
			else return ((FieldInfo)mems[0]).GetValue (ob);
		}

		public object GetValue (object ob)
		{
			if (_member == null) InitMember (ob.GetType());
			if (_member is PropertyInfo) return ((PropertyInfo)_member).GetValue (ob, null);
			else return ((FieldInfo)_member).GetValue (ob);
		}

		public void SetValue (object ob, object value)
		{
			if (_member == null) InitMember (ob.GetType());
			if (_member is PropertyInfo) ((PropertyInfo)_member).SetValue (ob, value, null);
			else ((FieldInfo)_member).SetValue (ob, value);
		}

		void InitMember (Type type)
		{
			MemberInfo[] mems = type.GetMember (_name, BindingFlags.Instance|BindingFlags.Public);
			_member = mems[0];
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
