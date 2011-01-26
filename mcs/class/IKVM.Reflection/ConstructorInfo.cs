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

namespace IKVM.Reflection
{
	public abstract class ConstructorInfo : MethodBase
	{
		// prevent external subclasses
		internal ConstructorInfo()
		{
		}

		public static readonly string ConstructorName = ".ctor";
		public static readonly string TypeConstructorName = ".cctor";

		internal abstract MethodInfo GetMethodInfo();

		internal override MethodBase BindTypeParameters(Type type)
		{
			return new ConstructorInfoImpl((MethodInfo)GetMethodInfo().BindTypeParameters(type));
		}

		public sealed override MemberTypes MemberType
		{
			get { return MemberTypes.Constructor; }
		}

		public override bool ContainsGenericParameters
		{
			get { return GetMethodInfo().ContainsGenericParameters; }
		}

		public sealed override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] parameters = GetMethodInfo().GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new ParameterInfoWrapper(this, parameters[i]);
			}
			return parameters;
		}

		private sealed class ParameterInfoWrapper : ParameterInfo
		{
			private readonly ConstructorInfo ctor;
			private readonly ParameterInfo forward;

			internal ParameterInfoWrapper(ConstructorInfo ctor, ParameterInfo forward)
			{
				this.ctor = ctor;
				this.forward = forward;
			}

			public override string Name
			{
				get { return forward.Name; }
			}

			public override Type ParameterType
			{
				get { return forward.ParameterType; }
			}

			public override ParameterAttributes Attributes
			{
				get { return forward.Attributes; }
			}

			public override int Position
			{
				get { return forward.Position; }
			}

			public override object RawDefaultValue
			{
				get { return forward.RawDefaultValue; }
			}

			public override Type[] GetOptionalCustomModifiers()
			{
				return forward.GetOptionalCustomModifiers();
			}

			public override Type[] GetRequiredCustomModifiers()
			{
				return forward.GetRequiredCustomModifiers();
			}

			public override MemberInfo Member
			{
				get { return ctor; }
			}

			public override int MetadataToken
			{
				get { return forward.MetadataToken; }
			}

			internal override Module Module
			{
				get { return ctor.Module; }
			}

			internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
			{
				return forward.GetCustomAttributesData(attributeType);
			}
		}
	}

	sealed class ConstructorInfoImpl : ConstructorInfo
	{
		private readonly MethodInfo method;

		internal ConstructorInfoImpl(MethodInfo method)
		{
			this.method = method;
		}

		public override bool Equals(object obj)
		{
			ConstructorInfoImpl other = obj as ConstructorInfoImpl;
			return other != null && other.method.Equals(method);
		}

		public override int GetHashCode()
		{
			return method.GetHashCode();
		}

		public override MethodBody GetMethodBody()
		{
			return method.GetMethodBody();
		}

		public override CallingConventions CallingConvention
		{
			get { return method.CallingConvention; }
		}

		public override MethodAttributes Attributes
		{
			get { return method.Attributes; }
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return method.GetMethodImplementationFlags();
		}

		internal override int ParameterCount
		{
			get { return method.ParameterCount; }
		}

		public override Type DeclaringType
		{
			get { return method.DeclaringType; }
		}

		public override string Name
		{
			get { return method.Name; }
		}

		public override string ToString()
		{
			return method.ToString();
		}

		public override Module Module
		{
			get { return method.Module; }
		}

		public override int MetadataToken
		{
			get { return method.MetadataToken; }
		}

		public override bool __IsMissing
		{
			get { return method.__IsMissing; }
		}

		internal override MethodInfo GetMethodInfo()
		{
			return method;
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			return method.GetCustomAttributesData(attributeType);
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return method.GetMethodOnTypeDefinition();
		}

		internal override MethodSignature MethodSignature
		{
			get { return method.MethodSignature; }
		}

		internal override int ImportTo(Emit.ModuleBuilder module)
		{
			return method.ImportTo(module);
		}
	}
}
