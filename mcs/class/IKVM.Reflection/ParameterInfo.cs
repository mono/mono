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
using System.Collections.Generic;

namespace IKVM.Reflection
{
	public abstract class ParameterInfo : ICustomAttributeProvider
	{
		// prevent external subclasses
		internal ParameterInfo()
		{
		}

		public sealed override bool Equals(object obj)
		{
			ParameterInfo other = obj as ParameterInfo;
			return other != null && other.Member == this.Member && other.Position == this.Position;
		}

		public sealed override int GetHashCode()
		{
			return this.Member.GetHashCode() * 1777 + this.Position;
		}

		public static bool operator ==(ParameterInfo p1, ParameterInfo p2)
		{
			return ReferenceEquals(p1, p2) || (!ReferenceEquals(p1, null) && p1.Equals(p2));
		}

		public static bool operator !=(ParameterInfo p1, ParameterInfo p2)
		{
			return !(p1 == p2);
		}

		public abstract string Name { get; }
		public abstract Type ParameterType { get; }
		public abstract ParameterAttributes Attributes { get; }
		public abstract int Position { get; }
		public abstract object RawDefaultValue { get; }
		public abstract Type[] GetOptionalCustomModifiers();
		public abstract Type[] GetRequiredCustomModifiers();
		public abstract MemberInfo Member { get; }
		public abstract int MetadataToken { get; }
		internal abstract Module Module { get; }

		public bool IsIn
		{
			get { return (Attributes & ParameterAttributes.In) != 0; }
		}

		public bool IsOut
		{
			get { return (Attributes & ParameterAttributes.Out) != 0; }
		}

		public bool IsLcid
		{
			get { return (Attributes & ParameterAttributes.Lcid) != 0; }
		}

		public bool IsRetval
		{
			get { return (Attributes & ParameterAttributes.Retval) != 0; }
		}

		public bool IsOptional
		{
			get { return (Attributes & ParameterAttributes.Optional) != 0; }
		}

		public bool IsDefined(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit).Count != 0;
		}

		public IList<CustomAttributeData> __GetCustomAttributes(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit);
		}

		internal virtual IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			return this.Module.GetCustomAttributes(this.MetadataToken, attributeType);
		}
	}
}
