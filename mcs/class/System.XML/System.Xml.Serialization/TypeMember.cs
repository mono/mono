
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

namespace System.Xml.Serialization
{
	/// <summary>
	/// TypeMember is immutable class which is used as a key in a Hashtable.
	/// </summary>

	internal sealed class TypeMember
	{
		Type type;
		string member;
		internal TypeMember(Type type, string member)
		{
			this.type = type;
			this.member = member;
		}

		public override int GetHashCode()
		{
			return unchecked (type.GetHashCode() + member.GetHashCode());
		}

		public override bool Equals(object obj)
		{
			if(obj is TypeMember)
				return TypeMember.Equals(this,(TypeMember)obj);
			
			return false;
		}

		public static bool Equals(TypeMember tm1, TypeMember tm2)
		{
			if(Object.ReferenceEquals(tm1,tm2))
				return true;
			if(Object.ReferenceEquals(tm1,null) || Object.ReferenceEquals(tm2,null))
				return false;
			if(tm1.type == tm2.type && tm1.member == tm2.member)
				return true;
			return false;
		}

		public static bool operator==(TypeMember tm1, TypeMember tm2)
		{
			return TypeMember.Equals(tm1,tm2);
		}

		public static bool operator!=(TypeMember tm1, TypeMember tm2)
		{
			return !TypeMember.Equals(tm1,tm2);
		}
		
		public override string ToString ()
		{
			return type.ToString() + " " + member;
		}
	}
}
