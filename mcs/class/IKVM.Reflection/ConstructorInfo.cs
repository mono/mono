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
using System.Diagnostics;

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

		public sealed override MethodBase __GetMethodOnTypeDefinition()
		{
			return new ConstructorInfoImpl((MethodInfo)GetMethodInfo().__GetMethodOnTypeDefinition());
		}

		public sealed override MemberTypes MemberType
		{
			get { return MemberTypes.Constructor; }
		}

		public sealed override int __MethodRVA
		{
			get { return GetMethodInfo().__MethodRVA; }
		}

		public sealed override bool ContainsGenericParameters
		{
			get { return GetMethodInfo().ContainsGenericParameters; }
		}

		public ParameterInfo __ReturnParameter
		{
			get { return new ParameterInfoWrapper(this, GetMethodInfo().ReturnParameter); }
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

		internal sealed override MemberInfo SetReflectedType(Type type)
		{
			return new ConstructorInfoWithReflectedType(type, this);
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

	sealed class ConstructorInfoWithReflectedType : ConstructorInfo
	{
		private readonly Type reflectedType;
		private readonly ConstructorInfo ctor;

		internal ConstructorInfoWithReflectedType(Type reflectedType, ConstructorInfo ctor)
		{
			Debug.Assert(reflectedType != ctor.DeclaringType);
			this.reflectedType = reflectedType;
			this.ctor = ctor;
		}

		public override bool Equals(object obj)
		{
			ConstructorInfoWithReflectedType other = obj as ConstructorInfoWithReflectedType;
			return other != null
				&& other.reflectedType == reflectedType
				&& other.ctor == ctor;
		}

		public override int GetHashCode()
		{
			return reflectedType.GetHashCode() ^ ctor.GetHashCode();
		}

		public override MethodBody GetMethodBody()
		{
			return ctor.GetMethodBody();
		}

		public override CallingConventions CallingConvention
		{
			get { return ctor.CallingConvention; }
		}

		public override MethodAttributes Attributes
		{
			get { return ctor.Attributes; }
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return ctor.GetMethodImplementationFlags();
		}

		internal override int ParameterCount
		{
			get { return ctor.ParameterCount; }
		}

		public override Type DeclaringType
		{
			get { return ctor.DeclaringType; }
		}

		public override Type ReflectedType
		{
			get { return reflectedType; }
		}

		public override string Name
		{
			get { return ctor.Name; }
		}

		public override string ToString()
		{
			return ctor.ToString();
		}

		public override Module Module
		{
			get { return ctor.Module; }
		}

		public override int MetadataToken
		{
			get { return ctor.MetadataToken; }
		}

		public override bool __IsMissing
		{
			get { return ctor.__IsMissing; }
		}

		internal override MethodInfo GetMethodInfo()
		{
			return ctor.GetMethodInfo();
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			return ctor.GetCustomAttributesData(attributeType);
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return ctor.GetMethodOnTypeDefinition();
		}

		internal override MethodSignature MethodSignature
		{
			get { return ctor.MethodSignature; }
		}

		internal override int ImportTo(Emit.ModuleBuilder module)
		{
			return ctor.ImportTo(module);
		}
	}
}
