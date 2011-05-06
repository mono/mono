//
// XmlTypeMapMember.cs: 
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
using System.Reflection;

namespace System.Xml.Serialization
{
	// XmlTypeMapMember
	// A member of a class that must be serialized

	internal class XmlTypeMapMember
	{
		string _name;
		int _index;
		int _globalIndex = -1;
		int _specifiedGlobalIndex = -1;
		TypeData _typeData;
		MemberInfo _member;
		MemberInfo _specifiedMember;
		object _defaultValue = System.DBNull.Value;
		string documentation;
		int _flags;
		
		const int OPTIONAL = 1;
		const int RETURN_VALUE = 2;
		const int IGNORE = 4;

		public XmlTypeMapMember()
		{
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		
		public object DefaultValue
		{
			get { return _defaultValue; }
			set { _defaultValue = value; }
		}

		public string Documentation
		{
			set { documentation = value; }
			get { return documentation; }
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

		public static void SetValue (object ob, string name, object value)
		{
			MemberInfo[] mems = ob.GetType().GetMember (name, BindingFlags.Instance|BindingFlags.Public);
			if (mems[0] is PropertyInfo) ((PropertyInfo)mems[0]).SetValue (ob, value, null);
			else ((FieldInfo)mems[0]).SetValue (ob, value);
		}

		void InitMember (Type type)
		{
			MemberInfo[] mems = type.GetMember (_name, BindingFlags.Instance|BindingFlags.Public);
			_member = mems[0];
			
			mems = type.GetMember (_name + "Specified", BindingFlags.Instance|BindingFlags.Public);
			if (mems.Length > 0) _specifiedMember = mems[0];
			if (_specifiedMember is PropertyInfo && !((PropertyInfo) _specifiedMember).CanWrite)
				_specifiedMember = null;
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
		
		public int GlobalIndex
		{
			get { return _globalIndex; }
			set { _globalIndex = value; }
		}
		
		public int SpecifiedGlobalIndex
		{
			get { return _specifiedGlobalIndex; }
		}
		
		public bool IsOptionalValueType
		{
			get { return (_flags & OPTIONAL) != 0; }
			set { _flags = value ? (_flags | OPTIONAL) : (_flags & ~OPTIONAL); }
		}
		
		public bool IsReturnValue
		{
			get { return (_flags & RETURN_VALUE) != 0; }
			set { _flags = value ? (_flags | RETURN_VALUE) : (_flags & ~RETURN_VALUE); }
		}
		
		public bool Ignore
		{
			get { return (_flags & IGNORE) != 0; }
			set { _flags = value ? (_flags | IGNORE) : (_flags & ~IGNORE); }
		}
		
		public void CheckOptionalValueType (Type type)
		{
			// Used when reflecting a type
			if (_member == null) InitMember (type);
			IsOptionalValueType = (_specifiedMember != null);
		}
		
		public void CheckOptionalValueType (XmlReflectionMember[] members)
		{
			// Used when reflecting a list of members (e.g. web service parameters)
			for (int n=0; n<members.Length; n++) {
				XmlReflectionMember m = members [n];
				if (m.MemberName == Name + "Specified" && m.MemberType == typeof(bool) && m.XmlAttributes.XmlIgnore) {
					IsOptionalValueType = true;
					_specifiedGlobalIndex = n;
					break;
				}
			}
		}
		
		public bool GetValueSpecified (object ob)
		{
			if (_specifiedGlobalIndex != -1) {
				object[] array = (object[])ob;
				return _specifiedGlobalIndex < array.Length && (bool) array [_specifiedGlobalIndex];
			}
			else if (_specifiedMember is PropertyInfo)
				return (bool) ((PropertyInfo)_specifiedMember).GetValue (ob, null);
			else
				return (bool) ((FieldInfo)_specifiedMember).GetValue (ob);
		}

		public void SetValueSpecified (object ob, bool value)
		{
			if (_specifiedGlobalIndex != -1)
				((object[])ob) [_specifiedGlobalIndex] = value;
			else if (_specifiedMember is PropertyInfo)
				((PropertyInfo)_specifiedMember).SetValue (ob, value, null);
			else
				((FieldInfo)_specifiedMember).SetValue (ob, value);
		}
		
		public virtual bool RequiresNullable {
			get { return false; }
		}
	}
}
