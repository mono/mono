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

			public override Type[] GetOptionalCustomModifiers()
			{
				return property.PropertySignature.GetOptionalCustomModifiers(parameter);
			}

			public override Type[] GetRequiredCustomModifiers()
			{
				return property.PropertySignature.GetRequiredCustomModifiers(parameter);
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

		public ParameterInfo[] GetIndexParameters()
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

		public Type[] GetRequiredCustomModifiers()
		{
			return this.PropertySignature.GetRequiredCustomModifiers();
		}

		public Type[] GetOptionalCustomModifiers()
		{
			return this.PropertySignature.GetOptionalCustomModifiers();
		}

		public bool IsSpecialName
		{
			get { return (Attributes & PropertyAttributes.SpecialName) != 0; }
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
	}
}
