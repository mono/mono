/*
  Copyright (C) 2009, 2010 Jeroen Frijters

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
	// this represents both generic method instantiations and non-generic methods on generic type instantations
	// (this means that it can be a generic method declaration as well as a generic method instance)
	sealed class GenericMethodInstance : MethodInfo
	{
		private readonly Type declaringType;
		private readonly MethodInfo method;
		private readonly Type[] methodArgs;
		private MethodSignature lazyMethodSignature;

		internal GenericMethodInstance(Type declaringType, MethodInfo method, Type[] methodArgs)
		{
			System.Diagnostics.Debug.Assert(!(method is GenericMethodInstance));
			this.declaringType = declaringType;
			this.method = method;
			this.methodArgs = methodArgs;
		}

		public override bool Equals(object obj)
		{
			GenericMethodInstance other = obj as GenericMethodInstance;
			return other != null
				&& other.method.Equals(method)
				&& other.declaringType.Equals(declaringType)
				&& Util.ArrayEquals(other.methodArgs, methodArgs);
		}

		public override int GetHashCode()
		{
			return declaringType.GetHashCode() * 33 ^ method.GetHashCode() ^ Util.GetHashCode(methodArgs);
		}

		public override Type ReturnType
		{
			get { return method.ReturnType.BindTypeParameters(this); }
		}

		public override ParameterInfo ReturnParameter
		{
			get { return new GenericParameterInfoImpl(this, method.ReturnParameter); }
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] parameters = method.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new GenericParameterInfoImpl(this, parameters[i]);
			}
			return parameters;
		}

		internal override int ParameterCount
		{
			get { return method.ParameterCount; }
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

		public override string Name
		{
			get { return method.Name; }
		}

		public override Type DeclaringType
		{
			get { return declaringType.IsModulePseudoType ? null : declaringType; }
		}

		public override Module Module
		{
			get { return method.Module; }
		}

		public override int MetadataToken
		{
			get { return method.MetadataToken; }
		}

		public override MethodBody GetMethodBody()
		{
			IKVM.Reflection.Reader.MethodDefImpl md = method as IKVM.Reflection.Reader.MethodDefImpl;
			if (md != null)
			{
				return md.GetMethodBody(this);
			}
			throw new NotSupportedException();
		}

		public override int __MethodRVA
		{
			get { return method.__MethodRVA; }
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			return new GenericMethodInstance(declaringType, method, typeArguments);
		}

		public override bool IsGenericMethod
		{
			get { return method.IsGenericMethod; }
		}

		public override bool IsGenericMethodDefinition
		{
			get { return method.IsGenericMethodDefinition && methodArgs == null; }
		}

		public override bool ContainsGenericParameters
		{
			get
			{
				if (declaringType.ContainsGenericParameters)
				{
					return true;
				}
				if (methodArgs != null)
				{
					foreach (Type type in methodArgs)
					{
						if (type.ContainsGenericParameters)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			if (this.IsGenericMethod)
			{
				if (this.IsGenericMethodDefinition)
				{
					return this;
				}
				else if (declaringType.IsConstructedGenericType)
				{
					return new GenericMethodInstance(declaringType, method, null);
				}
				else
				{
					return method;
				}
			}
			throw new InvalidOperationException();
		}

		public override MethodBase __GetMethodOnTypeDefinition()
		{
			return method;
		}

		public override Type[] GetGenericArguments()
		{
			if (methodArgs == null)
			{
				return method.GetGenericArguments();
			}
			else
			{
				return (Type[])methodArgs.Clone();
			}
		}

		internal override Type GetGenericMethodArgument(int index)
		{
			if (methodArgs == null)
			{
				return method.GetGenericMethodArgument(index);
			}
			else
			{
				return methodArgs[index];
			}
		}

		internal override int GetGenericMethodArgumentCount()
		{
			return method.GetGenericMethodArgumentCount();
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return method.GetMethodOnTypeDefinition();
		}

		internal override int ImportTo(Emit.ModuleBuilder module)
		{
			if (methodArgs == null)
			{
				return module.ImportMethodOrField(declaringType, method.Name, method.MethodSignature);
			}
			else
			{
				return module.ImportMethodSpec(declaringType, method, methodArgs);
			}
		}

		internal override MethodSignature MethodSignature
		{
			get { return lazyMethodSignature ?? (lazyMethodSignature = method.MethodSignature.Bind(declaringType, methodArgs)); }
		}

		internal override MethodBase BindTypeParameters(Type type)
		{
			System.Diagnostics.Debug.Assert(methodArgs == null);
			return new GenericMethodInstance(declaringType.BindTypeParameters(type), method, null);
		}

		internal override bool HasThis
		{
			get { return method.HasThis; }
		}

		public override MethodInfo[] __GetMethodImpls()
		{
			MethodInfo[] methods = method.__GetMethodImpls();
			for (int i = 0; i < methods.Length; i++)
			{
				methods[i] = (MethodInfo)methods[i].BindTypeParameters(declaringType);
			}
			return methods;
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

	sealed class GenericFieldInstance : FieldInfo
	{
		private readonly Type declaringType;
		private readonly FieldInfo field;

		internal GenericFieldInstance(Type declaringType, FieldInfo field)
		{
			this.declaringType = declaringType;
			this.field = field;
		}

		public override bool Equals(object obj)
		{
			GenericFieldInstance other = obj as GenericFieldInstance;
			return other != null && other.declaringType.Equals(declaringType) && other.field.Equals(field);
		}

		public override int GetHashCode()
		{
			return declaringType.GetHashCode() * 3 ^ field.GetHashCode();
		}

		public override FieldAttributes Attributes
		{
			get { return field.Attributes; }
		}

		public override string Name
		{
			get { return field.Name; }
		}

		public override Type DeclaringType
		{
			get { return declaringType; }
		}

		public override Module Module
		{
			get { return declaringType.Module; }
		}

		public override int MetadataToken
		{
			get { return field.MetadataToken; }
		}

		public override object GetRawConstantValue()
		{
			return field.GetRawConstantValue();
		}

		public override void __GetDataFromRVA(byte[] data, int offset, int length)
		{
			field.__GetDataFromRVA(data, offset, length);
		}

		public override int __FieldRVA
		{
			get { return field.__FieldRVA; }
		}

		public override bool __TryGetFieldOffset(out int offset)
		{
			return field.__TryGetFieldOffset(out offset);
		}

		public override FieldInfo __GetFieldOnTypeDefinition()
		{
			return field;
		}

		internal override FieldSignature FieldSignature
		{
			get { return field.FieldSignature.ExpandTypeParameters(declaringType); }
		}

		internal override int ImportTo(Emit.ModuleBuilder module)
		{
			return module.ImportMethodOrField(declaringType, field.Name, field.FieldSignature);
		}

		internal override FieldInfo BindTypeParameters(Type type)
		{
			return new GenericFieldInstance(declaringType.BindTypeParameters(type), field);
		}

		internal override int GetCurrentToken()
		{
			return field.GetCurrentToken();
		}

		internal override bool IsBaked
		{
			get { return field.IsBaked; }
		}
	}

	sealed class GenericParameterInfoImpl : ParameterInfo
	{
		private readonly GenericMethodInstance method;
		private readonly ParameterInfo parameterInfo;

		internal GenericParameterInfoImpl(GenericMethodInstance method, ParameterInfo parameterInfo)
		{
			this.method = method;
			this.parameterInfo = parameterInfo;
		}

		public override string Name
		{
			get { return parameterInfo.Name; }
		}

		public override Type ParameterType
		{
			get { return parameterInfo.ParameterType.BindTypeParameters(method); }
		}

		public override ParameterAttributes Attributes
		{
			get { return parameterInfo.Attributes; }
		}

		public override int Position
		{
			get { return parameterInfo.Position; }
		}

		public override object RawDefaultValue
		{
			get { return parameterInfo.RawDefaultValue; }
		}

		public override CustomModifiers __GetCustomModifiers()
		{
			return parameterInfo.__GetCustomModifiers().Bind(method);
		}

		public override bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
		{
			return parameterInfo.__TryGetFieldMarshal(out fieldMarshal);
		}

		public override MemberInfo Member
		{
			get { return method; }
		}

		public override int MetadataToken
		{
			get { return parameterInfo.MetadataToken; }
		}

		internal override Module Module
		{
			get { return method.Module; }
		}
	}

	sealed class GenericPropertyInfo : PropertyInfo
	{
		private readonly Type typeInstance;
		private readonly PropertyInfo property;

		internal GenericPropertyInfo(Type typeInstance, PropertyInfo property)
		{
			this.typeInstance = typeInstance;
			this.property = property;
		}

		public override bool Equals(object obj)
		{
			GenericPropertyInfo other = obj as GenericPropertyInfo;
			return other != null && other.typeInstance == typeInstance && other.property == property;
		}

		public override int GetHashCode()
		{
			return typeInstance.GetHashCode() * 537 + property.GetHashCode();
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

		private MethodInfo Wrap(MethodInfo method)
		{
			if (method == null)
			{
				return null;
			}
			return new GenericMethodInstance(typeInstance, method, null);
		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			return Wrap(property.GetGetMethod(nonPublic));
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			return Wrap(property.GetSetMethod(nonPublic));
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			MethodInfo[] accessors = property.GetAccessors(nonPublic);
			for (int i = 0; i < accessors.Length; i++)
			{
				accessors[i] = Wrap(accessors[i]);
			}
			return accessors;
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
			get { return property.PropertySignature.ExpandTypeParameters(typeInstance); }
		}

		public override string Name
		{
			get { return property.Name; }
		}

		public override Type DeclaringType
		{
			get { return typeInstance; }
		}

		public override Module Module
		{
			get { return typeInstance.Module; }
		}

		public override int MetadataToken
		{
			get { return property.MetadataToken; }
		}

		internal override PropertyInfo BindTypeParameters(Type type)
		{
			return new GenericPropertyInfo(typeInstance.BindTypeParameters(type), property);
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

	sealed class GenericEventInfo : EventInfo
	{
		private readonly Type typeInstance;
		private readonly EventInfo eventInfo;

		internal GenericEventInfo(Type typeInstance, EventInfo eventInfo)
		{
			this.typeInstance = typeInstance;
			this.eventInfo = eventInfo;
		}

		public override bool Equals(object obj)
		{
			GenericEventInfo other = obj as GenericEventInfo;
			return other != null && other.typeInstance == typeInstance && other.eventInfo == eventInfo;
		}

		public override int GetHashCode()
		{
			return typeInstance.GetHashCode() * 777 + eventInfo.GetHashCode();
		}

		public override EventAttributes Attributes
		{
			get { return eventInfo.Attributes; }
		}

		private MethodInfo Wrap(MethodInfo method)
		{
			if (method == null)
			{
				return null;
			}
			return new GenericMethodInstance(typeInstance, method, null);
		}

		public override MethodInfo GetAddMethod(bool nonPublic)
		{
			return Wrap(eventInfo.GetAddMethod(nonPublic));
		}

		public override MethodInfo GetRaiseMethod(bool nonPublic)
		{
			return Wrap(eventInfo.GetRaiseMethod(nonPublic));
		}

		public override MethodInfo GetRemoveMethod(bool nonPublic)
		{
			return Wrap(eventInfo.GetRemoveMethod(nonPublic));
		}

		public override MethodInfo[] GetOtherMethods(bool nonPublic)
		{
			MethodInfo[] others = eventInfo.GetOtherMethods(nonPublic);
			for (int i = 0; i < others.Length; i++)
			{
				others[i] = Wrap(others[i]);
			}
			return others;
		}

		public override MethodInfo[] __GetMethods()
		{
			MethodInfo[] others = eventInfo.__GetMethods();
			for (int i = 0; i < others.Length; i++)
			{
				others[i] = Wrap(others[i]);
			}
			return others;
		}

		public override Type EventHandlerType
		{
			get { return eventInfo.EventHandlerType.BindTypeParameters(typeInstance); }
		}

		public override string Name
		{
			get { return eventInfo.Name; }
		}

		public override Type DeclaringType
		{
			get { return typeInstance; }
		}

		public override Module Module
		{
			get { return eventInfo.Module; }
		}

		public override int MetadataToken
		{
			get { return eventInfo.MetadataToken; }
		}

		internal override EventInfo BindTypeParameters(Type type)
		{
			return new GenericEventInfo(typeInstance.BindTypeParameters(type), eventInfo);
		}

		internal override bool IsPublic
		{
			get { return eventInfo.IsPublic; }
		}

		internal override bool IsNonPrivate
		{
			get { return eventInfo.IsNonPrivate; }
		}

		internal override bool IsStatic
		{
			get { return eventInfo.IsStatic; }
		}

		internal override bool IsBaked
		{
			get { return eventInfo.IsBaked; }
		}

		internal override int GetCurrentToken()
		{
			return eventInfo.GetCurrentToken();
		}
	}
}
