/*
  Copyright (C) 2012 Jeroen Frijters

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
	public interface IReflectableType
	{
		TypeInfo GetTypeInfo();
	}

	public static class IntrospectionExtensions
	{
		// we target .NET 2.0 so we can't define an extension method
		public static TypeInfo GetTypeInfo(/*this*/ Type type)
		{
			return type.GetTypeInfo();
		}
	}

	public abstract class TypeInfo : Type, IReflectableType
	{
		private const BindingFlags Flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		internal TypeInfo()
		{
		}

		internal TypeInfo(Type underlyingType)
			: base(underlyingType)
		{
		}

		public IEnumerable<ConstructorInfo> DeclaredConstructors
		{
			get { return GetConstructors(Flags); }
		}

		public IEnumerable<EventInfo> DeclaredEvents
		{
			get { return GetEvents(Flags); }
		}

		public IEnumerable<FieldInfo> DeclaredFields
		{
			get { return GetFields(Flags); }
		}

		public IEnumerable<MemberInfo> DeclaredMembers
		{
			get { return GetMembers(Flags); }
		}

		public IEnumerable<MethodInfo> DeclaredMethods
		{
			get { return GetMethods(Flags); }
		}

		public IEnumerable<TypeInfo> DeclaredNestedTypes
		{
			get
			{
				Type[] types = GetNestedTypes(Flags);
				TypeInfo[] typeInfos = new TypeInfo[types.Length];
				for (int i = 0; i < types.Length; i++)
				{
					typeInfos[i] = types[i].GetTypeInfo();
				}
				return typeInfos;
			}
		}

		public IEnumerable<PropertyInfo> DeclaredProperties
		{
			get { return GetProperties(Flags); }
		}

		public Type[] GenericTypeParameters
		{
			get { return IsGenericTypeDefinition ? GetGenericArguments() : Type.EmptyTypes; }
		}

		public IEnumerable<Type> ImplementedInterfaces
		{
			get { return __GetDeclaredInterfaces(); }
		}

		public Type AsType()
		{
			return this;
		}

		public EventInfo GetDeclaredEvent(string name)
		{
			return GetEvent(name, Flags);
		}

		public FieldInfo GetDeclaredField(string name)
		{
			return GetField(name, Flags);
		}

		public MethodInfo GetDeclaredMethod(string name)
		{
			return GetMethod(name, Flags);
		}

		public IEnumerable<MethodInfo> GetDeclaredMethods(string name)
		{
			List<MethodInfo> methods = new List<MethodInfo>();
			foreach (MethodInfo method in GetMethods(Flags))
			{
				if (method.Name == name)
				{
					methods.Add(method);
				}
			}
			return methods;
		}

		public TypeInfo GetDeclaredNestedType(string name)
		{
			return GetNestedType(name, Flags).GetTypeInfo();
		}

		public PropertyInfo GetDeclaredProperty(string name)
		{
			return GetProperty(name, Flags);
		}

		public bool IsAssignableFrom(TypeInfo typeInfo)
		{
			return base.IsAssignableFrom(typeInfo);
		}
	}
}
