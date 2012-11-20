/*
  Copyright (C) 2009-2012 Jeroen Frijters

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

namespace IKVM.Reflection
{
	public abstract class PropertyInfo : MemberInfo
	{
		// prevent external subclasses
		internal PropertyInfo()
		{
		}

		public sealed override MemberTypes MemberType
		{
			get { return MemberTypes.Property; }
		}

		public abstract PropertyAttributes Attributes { get; }
		public abstract bool CanRead { get; }
		public abstract bool CanWrite { get; }
		public abstract MethodInfo GetGetMethod(bool nonPublic);
		public abstract MethodInfo GetSetMethod(bool nonPublic);
		public abstract MethodInfo[] GetAccessors(bool nonPublic);
		public abstract object GetRawConstantValue();
		internal abstract bool IsPublic { get; }
		internal abstract bool IsNonPrivate { get; }
		internal abstract bool IsStatic { get; }
		internal abstract PropertySignature PropertySignature { get; }

		private sealed class ParameterInfoImpl : ParameterInfo
		{
			private readonly PropertyInfo property;
			private readonly int parameter;

			internal ParameterInfoImpl(PropertyInfo property, int parameter)
			{
				this.property = property;
				this.parameter = parameter;
			}

			public override string Name
			{
				get { return null; }
			}

			public override Type ParameterType
			{
				get { return property.PropertySignature.GetParameter(parameter); }
			}

			public override ParameterAttributes Attributes
			{
				get { return ParameterAttributes.None; }
			}

			public override int Position
			{
				get { return parameter; }
			}

			public override object RawDefaultValue
			{
				get { throw new InvalidOperationException(); }
			}

			public override CustomModifiers __GetCustomModifiers()
			{
				return property.PropertySignature.GetParameterCustomModifiers(parameter);
			}

			public override bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
			{
				fieldMarshal = new FieldMarshal();
				return false;
			}

			public override MemberInfo Member
			{
				get { return property; }
			}

			public override int MetadataToken
			{
				get { return 0x08000000; }
			}

			internal override Module Module
			{
				get { return property.Module; }
			}
		}

		public virtual ParameterInfo[] GetIndexParameters()
		{
			ParameterInfo[] parameters = new ParameterInfo[this.PropertySignature.ParameterCount];
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new ParameterInfoImpl(this, i);
			}
			return parameters;
		}

		public Type PropertyType
		{
			get { return this.PropertySignature.PropertyType; }
		}

		public CustomModifiers __GetCustomModifiers()
		{
			return this.PropertySignature.GetCustomModifiers();
		}

		public Type[] GetRequiredCustomModifiers()
		{
			return __GetCustomModifiers().GetRequired();
		}

		public Type[] GetOptionalCustomModifiers()
		{
			return __GetCustomModifiers().GetOptional();
		}

		public bool IsSpecialName
		{
			get { return (Attributes & PropertyAttributes.SpecialName) != 0; }
		}

		public MethodInfo GetMethod
		{
			get { return GetGetMethod(true); }
		}

		public MethodInfo SetMethod
		{
			get { return GetSetMethod(true); }
		}

		public MethodInfo GetGetMethod()
		{
			return GetGetMethod(false);
		}

		public MethodInfo GetSetMethod()
		{
			return GetSetMethod(false);
		}

		public MethodInfo[] GetAccessors()
		{
			return GetAccessors(false);
		}

		public CallingConventions __CallingConvention
		{
			get { return this.PropertySignature.CallingConvention; }
		}

		internal virtual PropertyInfo BindTypeParameters(Type type)
		{
			return new GenericPropertyInfo(this.DeclaringType.BindTypeParameters(type), this);
		}

		public override string ToString()
		{
			return this.DeclaringType.ToString() + " " + Name;
		}

		internal sealed override bool BindingFlagsMatch(BindingFlags flags)
		{
			return BindingFlagsMatch(IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic)
				&& BindingFlagsMatch(IsStatic, flags, BindingFlags.Static, BindingFlags.Instance);
		}

		internal sealed override bool BindingFlagsMatchInherited(BindingFlags flags)
		{
			return IsNonPrivate
				&& BindingFlagsMatch(IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic)
				&& BindingFlagsMatch(IsStatic, flags, BindingFlags.Static | BindingFlags.FlattenHierarchy, BindingFlags.Instance);
		}

		internal sealed override MemberInfo SetReflectedType(Type type)
		{
			return new PropertyInfoWithReflectedType(type, this);
		}

		internal sealed override List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType)
		{
			// properties don't have pseudo custom attributes
			return null;
		}
	}

	sealed class PropertyInfoWithReflectedType : PropertyInfo
	{
		private readonly Type reflectedType;
		private readonly PropertyInfo property;

		internal PropertyInfoWithReflectedType(Type reflectedType, PropertyInfo property)
		{
			this.reflectedType = reflectedType;
			this.property = property;
		}

		public override PropertyAttributes Attributes
		{
			get { return property.Attributes; }
		}

		public override bool CanRead
		{
			get { return property.CanRead; }
		}

		public override bool CanWrite
		{
			get { return property.CanWrite; }
		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			return SetReflectedType(property.GetGetMethod(nonPublic), reflectedType);
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			return SetReflectedType(property.GetSetMethod(nonPublic), reflectedType);
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			return SetReflectedType(property.GetAccessors(nonPublic), reflectedType);
		}

		public override object GetRawConstantValue()
		{
			return property.GetRawConstantValue();
		}

		internal override bool IsPublic
		{
			get { return property.IsPublic; }
		}

		internal override bool IsNonPrivate
		{
			get { return property.IsNonPrivate; }
		}

		internal override bool IsStatic
		{
			get { return property.IsStatic; }
		}

		internal override PropertySignature PropertySignature
		{
			get { return property.PropertySignature; }
		}

		public override ParameterInfo[] GetIndexParameters()
		{
			ParameterInfo[] parameters = property.GetIndexParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new ParameterInfoWrapper(this, parameters[i]);
			}
			return parameters;
		}

		internal override PropertyInfo BindTypeParameters(Type type)
		{
			return property.BindTypeParameters(type);
		}

		public override string ToString()
		{
			return property.ToString();
		}

		public override bool __IsMissing
		{
			get { return property.__IsMissing; }
		}

		public override Type DeclaringType
		{
			get { return property.DeclaringType; }
		}

		public override Type ReflectedType
		{
			get { return reflectedType; }
		}

		public override bool Equals(object obj)
		{
			PropertyInfoWithReflectedType other = obj as PropertyInfoWithReflectedType;
			return other != null
				&& other.reflectedType == reflectedType
				&& other.property == property;
		}

		public override int GetHashCode()
		{
			return reflectedType.GetHashCode() ^ property.GetHashCode();
		}

		public override int MetadataToken
		{
			get { return property.MetadataToken; }
		}

		public override Module Module
		{
			get { return property.Module; }
		}

		public override string Name
		{
			get { return property.Name; }
		}

		internal override bool IsBaked
		{
			get { return property.IsBaked; }
		}

		internal override int GetCurrentToken()
		{
			return property.GetCurrentToken();
		}
	}
}
