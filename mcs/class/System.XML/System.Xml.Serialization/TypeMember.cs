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
	}
}
