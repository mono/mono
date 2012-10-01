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
using System.Text;

namespace IKVM.Reflection
{
	public abstract class MethodInfo : MethodBase, IGenericContext, IGenericBinder
	{
		// prevent external subclasses
		internal MethodInfo()
		{
		}

		public sealed override MemberTypes MemberType
		{
			get { return MemberTypes.Method; }
		}

		public abstract Type ReturnType { get; }
		public abstract ParameterInfo ReturnParameter { get; }

		public virtual MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			throw new NotSupportedException(this.GetType().FullName);
		}

		public virtual MethodInfo GetGenericMethodDefinition()
		{
			throw new NotSupportedException(this.GetType().FullName);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(this.ReturnType.Name).Append(' ').Append(this.Name);
			string sep;
			if (this.IsGenericMethod)
			{
				sb.Append('[');
				sep = "";
				foreach (Type arg in GetGenericArguments())
				{
					sb.Append(sep).Append(arg);
					sep = ", ";
				}
				sb.Append(']');
			}
			sb.Append('(');
			sep = "";
			foreach (ParameterInfo arg in GetParameters())
			{
				sb.Append(sep).Append(arg.ParameterType);
				sep = ", ";
			}
			sb.Append(')');
			return sb.ToString();
		}

		internal bool IsNewSlot
		{
			get { return (this.Attributes & MethodAttributes.NewSlot) != 0; }
		}

		public MethodInfo GetBaseDefinition()
		{
			MethodInfo match = this;
			if (match.IsVirtual)
			{
				for (Type type = this.DeclaringType.BaseType; type != null && !match.IsNewSlot; type = type.BaseType)
				{
					MethodInfo method = type.FindMethod(this.Name, this.MethodSignature) as MethodInfo;
					if (method != null && method.IsVirtual)
					{
						match = method;
					}
				}
			}
			return match;
		}

		public virtual MethodInfo[] __GetMethodImpls()
		{
			throw new NotSupportedException();
		}

		public bool __TryGetImplMap(out ImplMapFlags mappingFlags, out string importName, out string importScope)
		{
			Module module = this.Module;
			foreach (int i in module.ImplMap.Filter(GetCurrentToken()))
			{
				mappingFlags = (ImplMapFlags)(ushort)module.ImplMap.records[i].MappingFlags;
				importName = module.GetString(module.ImplMap.records[i].ImportName);
				importScope = module.GetString(module.ModuleRef.records[(module.ImplMap.records[i].ImportScope & 0xFFFFFF) - 1]);
				return true;
			}
			mappingFlags = 0;
			importName = null;
			importScope = null;
			return false;
		}

		public ConstructorInfo __AsConstructorInfo()
		{
			return new ConstructorInfoImpl(this);
		}

		Type IGenericContext.GetGenericTypeArgument(int index)
		{
			return this.DeclaringType.GetGenericTypeArgument(index);
		}

		Type IGenericContext.GetGenericMethodArgument(int index)
		{
			return GetGenericMethodArgument(index);
		}

		internal virtual Type GetGenericMethodArgument(int index)
		{
			throw new InvalidOperationException();
		}

		internal virtual int GetGenericMethodArgumentCount()
		{
			throw new InvalidOperationException();
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return this;
		}

		Type IGenericBinder.BindTypeParameter(Type type)
		{
			return this.DeclaringType.GetGenericTypeArgument(type.GenericParameterPosition);
		}

		Type IGenericBinder.BindMethodParameter(Type type)
		{
			return GetGenericMethodArgument(type.GenericParameterPosition);
		}

		internal override MethodBase BindTypeParameters(Type type)
		{
			return new GenericMethodInstance(this.DeclaringType.BindTypeParameters(type), this, null);
		}

		// This method is used by ILGenerator and exists to allow ArrayMethod to override it,
		// because ArrayMethod doesn't have a working MethodAttributes property, so it needs
		// to base the result of this on the CallingConvention.
		internal virtual bool HasThis
		{
			get { return !IsStatic; }
		}

		internal sealed override MemberInfo SetReflectedType(Type type)
		{
			return new MethodInfoWithReflectedType(type, this);
		}

		internal sealed override List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType)
		{
			Module module = this.Module;
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			if ((this.Attributes & MethodAttributes.PinvokeImpl) != 0
				&& (attributeType == null || attributeType.IsAssignableFrom(module.universe.System_Runtime_InteropServices_DllImportAttribute)))
			{
				ImplMapFlags flags;
				string importName;
				string importScope;
				if (__TryGetImplMap(out flags, out importName, out importScope))
				{
					list.Add(CustomAttributeData.CreateDllImportPseudoCustomAttribute(module, flags, importName, importScope, GetMethodImplementationFlags()));
				}
			}
			if ((GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) != 0
				&& (attributeType == null || attributeType.IsAssignableFrom(module.universe.System_Runtime_InteropServices_PreserveSigAttribute)))
			{
				list.Add(CustomAttributeData.CreatePreserveSigPseudoCustomAttribute(module));
			}
			return list;
		}
	}

	sealed class MethodInfoWithReflectedType : MethodInfo
	{
		private readonly Type reflectedType;
		private readonly MethodInfo method;

		internal MethodInfoWithReflectedType(Type reflectedType, MethodInfo method)
		{
			Debug.Assert(reflectedType != method.DeclaringType);
			this.reflectedType = reflectedType;
			this.method = method;
		}

		public override bool Equals(object obj)
		{
			MethodInfoWithReflectedType other = obj as MethodInfoWithReflectedType;
			return other != null
				&& other.reflectedType == reflectedType
				&& other.method == method;
		}

		public override int GetHashCode()
		{
			return reflectedType.GetHashCode() ^ method.GetHashCode();
		}

		internal override MethodSignature MethodSignature
		{
			get { return method.MethodSignature; }
		}

		internal override int ParameterCount
		{
			get { return method.ParameterCount; }
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] parameters = method.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new ParameterInfoWrapper(this, parameters[i]);
			}
			return parameters;
		}

		public override MethodAttributes Attributes
		{
			get { return method.Attributes; }
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return method.GetMethodImplementationFlags();
		}

		public override MethodBody GetMethodBody()
		{
			return method.GetMethodBody();
		}

		public override CallingConventions CallingConvention
		{
			get { return method.CallingConvention; }
		}

		public override int __MethodRVA
		{
			get { return method.__MethodRVA; }
		}

		public override Type ReturnType
		{
			get { return method.ReturnType; }
		}

		public override ParameterInfo ReturnParameter
		{
			get { return new ParameterInfoWrapper(this, method.ReturnParameter); }
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			return SetReflectedType(method.MakeGenericMethod(typeArguments), reflectedType);
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			return method.GetGenericMethodDefinition();
		}

		public override string ToString()
		{
			return method.ToString();
		}

		public override MethodInfo[] __GetMethodImpls()
		{
			return method.__GetMethodImpls();
		}

		internal override Type GetGenericMethodArgument(int index)
		{
			return method.GetGenericMethodArgument(index);
		}

		internal override int GetGenericMethodArgumentCount()
		{
			return method.GetGenericMethodArgumentCount();
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return method.GetMethodOnTypeDefinition();
		}

		internal override bool HasThis
		{
			get { return method.HasThis; }
		}

		public override Module Module
		{
			get { return method.Module; }
		}

		public override Type DeclaringType
		{
			get { return method.DeclaringType; }
		}

		public override Type ReflectedType
		{
			get { return reflectedType; }
		}

		public override string Name
		{
			get { return method.Name; }
		}

		internal override int ImportTo(IKVM.Reflection.Emit.ModuleBuilder module)
		{
			return method.ImportTo(module);
		}

		public override MethodBase __GetMethodOnTypeDefinition()
		{
			return method.__GetMethodOnTypeDefinition();
		}

		public override bool __IsMissing
		{
			get { return method.__IsMissing; }
		}

		internal override MethodBase BindTypeParameters(Type type)
		{
			return method.BindTypeParameters(type);
		}

		public override bool ContainsGenericParameters
		{
			get { return method.ContainsGenericParameters; }
		}

		public override Type[] GetGenericArguments()
		{
			return method.GetGenericArguments();
		}

		public override bool IsGenericMethod
		{
			get { return method.IsGenericMethod; }
		}

		public override bool IsGenericMethodDefinition
		{
			get { return method.IsGenericMethodDefinition; }
		}

		public override int MetadataToken
		{
			get { return method.MetadataToken; }
		}

		internal override int GetCurrentToken()
		{
			return method.GetCurrentToken();
		}

		internal override bool IsBaked
		{
			get { return method.IsBaked; }
		}
	}
}
