/*
  Copyright (C) 2009 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace IKVM.Reflection
{
// disable warnings that complain about us having == and != operators without also overriding Equals/GetHashCode,
// this is intentional because most subtypes use reference equality
#pragma warning disable 660, 661
	public abstract class MemberInfo : ICustomAttributeProvider
	{
		// prevent external subclasses
		internal MemberInfo()
		{
		}

		public abstract string Name { get; }
		public abstract Type DeclaringType { get; }
		public abstract MemberTypes MemberType { get; }

		public virtual Type ReflectedType
		{
			get { return DeclaringType; }
		}

		internal abstract MemberInfo SetReflectedType(Type type);

		public virtual int MetadataToken
		{
			get { throw new NotSupportedException(); }
		}

		public abstract Module Module
		{
			get;
		}

		public virtual bool __IsMissing
		{
			get { return false; }
		}

		public bool IsDefined(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit).Count != 0;
		}

		public IList<CustomAttributeData> __GetCustomAttributes(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit);
		}

		public IList<CustomAttributeData> GetCustomAttributesData()
		{
			return CustomAttributeData.GetCustomAttributes(this);
		}

		public IEnumerable<CustomAttributeData> CustomAttributes
		{
			get { return GetCustomAttributesData(); }
		}

		public static bool operator ==(MemberInfo m1, MemberInfo m2)
		{
			return ReferenceEquals(m1, m2) || (!ReferenceEquals(m1, null) && m1.Equals(m2));
		}

		public static bool operator !=(MemberInfo m1, MemberInfo m2)
		{
			return !(m1 == m2);
		}

		internal abstract int GetCurrentToken();

		internal abstract List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType);

		internal abstract bool IsBaked { get; }

		internal virtual bool BindingFlagsMatch(BindingFlags flags)
		{
			throw new InvalidOperationException();
		}

		internal virtual bool BindingFlagsMatchInherited(BindingFlags flags)
		{
			throw new InvalidOperationException();
		}

		protected static bool BindingFlagsMatch(bool state, BindingFlags flags, BindingFlags trueFlag, BindingFlags falseFlag)
		{
			return (state && (flags & trueFlag) == trueFlag)
				|| (!state && (flags & falseFlag) == falseFlag);
		}

		protected static T SetReflectedType<T>(T member, Type type)
			where T : MemberInfo
		{
			return member == null ? null : (T)member.SetReflectedType(type);
		}

		protected static T[] SetReflectedType<T>(T[] members, Type type)
			where T : MemberInfo
		{
			for (int i = 0; i < members.Length; i++)
			{
				members[i] = SetReflectedType(members[i], type);
			}
			return members;
		}
	}
}
