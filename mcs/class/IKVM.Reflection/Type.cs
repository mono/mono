/*
  Copyright (C) 2009-2011 Jeroen Frijters

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
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	interface IGenericContext
	{
		Type GetGenericTypeArgument(int index);
		Type GetGenericMethodArgument(int index);
	}

	interface IGenericBinder
	{
		Type BindTypeParameter(Type type);
		Type BindMethodParameter(Type type);
	}

	public abstract class Type : MemberInfo, IGenericContext, IGenericBinder
	{
		public static readonly Type[] EmptyTypes = Empty<Type>.Array;

		// prevent subclassing by outsiders
		internal Type()
		{
		}

		public static Binder DefaultBinder
		{
			get { return null; }
		}

		public sealed override MemberTypes MemberType
		{
			get { return IsNested ? MemberTypes.NestedType : MemberTypes.TypeInfo; }
		}

		public virtual string AssemblyQualifiedName
		{
			// NOTE the assembly name is not escaped here, only when used in a generic type instantiation
			get { return this.FullName + ", " + this.Assembly.FullName; }
		}

		public abstract Type BaseType
		{
			get;
		}

		public abstract TypeAttributes Attributes
		{
			get;
		}

		public virtual Type GetElementType()
		{
			return null;
		}

		internal virtual void CheckBaked()
		{
		}

		public virtual Type[] __GetDeclaredTypes()
		{
			return Type.EmptyTypes;
		}

		public virtual Type[] __GetDeclaredInterfaces()
		{
			return Type.EmptyTypes;
		}

		public virtual MethodBase[] __GetDeclaredMethods()
		{
			return Empty<MethodBase>.Array;
		}

		public virtual __MethodImplMap __GetMethodImplMap()
		{
			throw new NotSupportedException();
		}

		public virtual FieldInfo[] __GetDeclaredFields()
		{
			return Empty<FieldInfo>.Array;
		}

		public virtual EventInfo[] __GetDeclaredEvents()
		{
			return Empty<EventInfo>.Array;
		}

		public virtual PropertyInfo[] __GetDeclaredProperties()
		{
			return Empty<PropertyInfo>.Array;
		}

		public virtual Type[] __GetRequiredCustomModifiers()
		{
			return Type.EmptyTypes;
		}

		public virtual Type[] __GetOptionalCustomModifiers()
		{
			return Type.EmptyTypes;
		}

		public virtual bool HasElementType
		{
			get { return false; }
		}

		public virtual bool IsArray
		{
			get { return false; }
		}

		public virtual bool __IsVector
		{
			get { return false; }
		}

		public virtual bool IsByRef
		{
			get { return false; }
		}

		public virtual bool IsPointer
		{
			get { return false; }
		}

		public virtual bool IsValueType
		{
			get
			{
				Type baseType = this.BaseType;
				return baseType == this.Module.universe.System_Enum
					|| (baseType == this.Module.universe.System_ValueType && this != this.Module.universe.System_Enum);
			}
		}

		public virtual bool IsGenericParameter
		{
			get { return false; }
		}

		public virtual int GenericParameterPosition
		{
			get { throw new NotSupportedException(); }
		}

		public virtual MethodBase DeclaringMethod
		{
			get { return null; }
		}

		public virtual Type UnderlyingSystemType
		{
			get { return this; }
		}

		public override Type DeclaringType
		{
			get { return null; }
		}

		public virtual string __Name
		{
			get { throw new InvalidOperationException(); }
		}

		public virtual string __Namespace
		{
			get { throw new InvalidOperationException(); }
		}

		public abstract override string Name
		{
			get;
		}

		public virtual string Namespace
		{
			get
			{
				if (IsNested)
				{
					return DeclaringType.Namespace;
				}
				return __Namespace;
			}
		}

		internal virtual int GetModuleBuilderToken()
		{
			throw new InvalidOperationException();
		}

		public bool Equals(Type type)
		{
			return !ReferenceEquals(type, null) && ReferenceEquals(type.UnderlyingSystemType, this.UnderlyingSystemType);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Type);
		}

		public override int GetHashCode()
		{
			Type type = this.UnderlyingSystemType;
			return ReferenceEquals(type, this) ? base.GetHashCode() : type.GetHashCode();
		}

		public virtual Type[] GetGenericArguments()
		{
			return Type.EmptyTypes;
		}

		public virtual Type[][] __GetGenericArgumentsRequiredCustomModifiers()
		{
			return Empty<Type[]>.Array;
		}

		public virtual Type[][] __GetGenericArgumentsOptionalCustomModifiers()
		{
			return Empty<Type[]>.Array;
		}

		public virtual Type GetGenericTypeDefinition()
		{
			throw new InvalidOperationException();
		}

		public virtual StructLayoutAttribute StructLayoutAttribute
		{
			get { return null; }
		}

		public virtual bool IsGenericType
		{
			get { return false; }
		}

		public virtual bool IsGenericTypeDefinition
		{
			get { return false; }
		}

		public virtual bool ContainsGenericParameters
		{
			get
			{
				if (this.IsGenericParameter)
				{
					return true;
				}
				foreach (Type arg in this.GetGenericArguments())
				{
					if (arg.ContainsGenericParameters)
					{
						return true;
					}
				}
				return false;
			}
		}

		public virtual Type[] GetGenericParameterConstraints()
		{
			throw new InvalidOperationException();
		}

		public virtual GenericParameterAttributes GenericParameterAttributes
		{
			get { throw new InvalidOperationException(); }
		}

		public virtual int GetArrayRank()
		{
			throw new NotSupportedException();
		}

		// .NET 4.0 API
		public virtual Type GetEnumUnderlyingType()
		{
			if (!this.IsEnum)
			{
				throw new ArgumentException();
			}
			CheckBaked();
			return GetEnumUnderlyingTypeImpl();
		}

		internal Type GetEnumUnderlyingTypeImpl()
		{
			foreach (FieldInfo field in __GetDeclaredFields())
			{
				if (!field.IsStatic)
				{
					// the CLR assumes that an enum has only one instance field, so we can do the same
					return field.FieldType;
				}
			}
			throw new InvalidOperationException();
		}

		public override string ToString()
		{
			return FullName;
		}

		public abstract string FullName
		{
			get;
		}

		protected string GetFullName()
		{
			string ns = TypeNameParser.Escape(this.__Namespace);
			Type decl = this.DeclaringType;
			if (decl == null)
			{
				if (ns == null)
				{
					return this.Name;
				}
				else
				{
					return ns + "." + this.Name;
				}
			}
			else
			{
				if (ns == null)
				{
					return decl.FullName + "+" + this.Name;
				}
				else
				{
					return decl.FullName + "+" + ns + "." + this.Name;
				}
			}
		}

		internal virtual bool IsModulePseudoType
		{
			get { return false; }
		}

		internal virtual Type GetGenericTypeArgument(int index)
		{
			throw new InvalidOperationException();
		}

		public MemberInfo[] GetDefaultMembers()
		{
			Type defaultMemberAttribute = this.Module.universe.Import(typeof(System.Reflection.DefaultMemberAttribute));
			foreach (CustomAttributeData cad in CustomAttributeData.GetCustomAttributes(this))
			{
				if (cad.Constructor.DeclaringType.Equals(defaultMemberAttribute))
				{
					return GetMember((string)cad.ConstructorArguments[0].Value);
				}
			}
			return Empty<MemberInfo>.Array;
		}

		public MemberInfo[] GetMember(string name)
		{
			return GetMember(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}

		public MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
		{
			return GetMember(name, MemberTypes.All, bindingAttr);
		}

		public MemberInfo[] GetMembers()
		{
			return GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}

		public MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			List<MemberInfo> members = new List<MemberInfo>();
			members.AddRange(GetConstructors(bindingAttr));
			members.AddRange(GetMethods(bindingAttr));
			members.AddRange(GetFields(bindingAttr));
			members.AddRange(GetProperties(bindingAttr));
			members.AddRange(GetEvents(bindingAttr));
			members.AddRange(GetNestedTypes(bindingAttr));
			return members.ToArray();
		}

		public MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
		{
			MemberFilter filter = delegate(MemberInfo member, object filterCriteria) { return member.Name.Equals(filterCriteria); };
			return FindMembers(type, bindingAttr, filter, name);
		}

		private static void AddMembers(List<MemberInfo> list, MemberFilter filter, object filterCriteria, MemberInfo[] members)
		{
			foreach (MemberInfo member in members)
			{
				if (filter == null || filter(member, filterCriteria))
				{
					list.Add(member);
				}
			}
		}

		public MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria)
		{
			List<MemberInfo> members = new List<MemberInfo>();
			if ((memberType & MemberTypes.Constructor) != 0)
			{
				AddMembers(members, filter, filterCriteria, GetConstructors(bindingAttr));
			}
			if ((memberType & MemberTypes.Method) != 0)
			{
				AddMembers(members, filter, filterCriteria, GetMethods(bindingAttr));
			}
			if ((memberType & MemberTypes.Field) != 0)
			{
				AddMembers(members, filter, filterCriteria, GetFields(bindingAttr));
			}
			if ((memberType & MemberTypes.Property) != 0)
			{
				AddMembers(members, filter, filterCriteria, GetProperties(bindingAttr));
			}
			if ((memberType & MemberTypes.Event) != 0)
			{
				AddMembers(members, filter, filterCriteria, GetEvents(bindingAttr));
			}
			if ((memberType & MemberTypes.NestedType) != 0)
			{
				AddMembers(members, filter, filterCriteria, GetNestedTypes(bindingAttr));
			}
			return members.ToArray();
		}

		public EventInfo GetEvent(string name)
		{
			return GetEvent(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}

		public EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			foreach (EventInfo evt in GetEvents(bindingAttr))
			{
				if (evt.Name == name)
				{
					return evt;
				}
			}
			return null;
		}

		public EventInfo[] GetEvents()
		{
			return GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}

		public EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			List<EventInfo> list = new List<EventInfo>();
			Type type = this;
			while (type != null)
			{
				type.CheckBaked();
				foreach (EventInfo evt in type.__GetDeclaredEvents())
				{
					if (BindingFlagsMatch(evt.IsPublic, bindingAttr, BindingFlags.Public, BindingFlags.NonPublic)
						&& BindingFlagsMatch(evt.IsStatic, bindingAttr, BindingFlags.Static, BindingFlags.Instance))
					{
						list.Add(evt);
					}
				}
				if ((bindingAttr & BindingFlags.DeclaredOnly) == 0)
				{
					if ((bindingAttr & BindingFlags.FlattenHierarchy) == 0)
					{
						bindingAttr &= ~BindingFlags.Static;
					}
					type = type.BaseType;
				}
				else
				{
					break;
				}
			}
			return list.ToArray();
		}

		public FieldInfo GetField(string name)
		{
			return GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}

		public FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			foreach (FieldInfo field in GetFields(bindingAttr))
			{
				if (field.Name == name)
				{
					return field;
				}
			}
			return null;
		}

		public FieldInfo[] GetFields()
		{
			return GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
		}

		public FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			List<FieldInfo> list = new List<FieldInfo>();
			CheckBaked();
			foreach (FieldInfo field in __GetDeclaredFields())
			{
				if (BindingFlagsMatch(field.IsPublic, bindingAttr, BindingFlags.Public, BindingFlags.NonPublic)
					&& BindingFlagsMatch(field.IsStatic, bindingAttr, BindingFlags.Static, BindingFlags.Instance))
				{
					list.Add(field);
				}
			}
			if ((bindingAttr & BindingFlags.DeclaredOnly) == 0)
			{
				for (Type type = this.BaseType; type != null; type = type.BaseType)
				{
					type.CheckBaked();
					foreach (FieldInfo field in type.__GetDeclaredFields())
					{
						if ((field.Attributes & FieldAttributes.FieldAccessMask) > FieldAttributes.Private
							&& BindingFlagsMatch(field.IsStatic, bindingAttr, BindingFlags.Static | BindingFlags.FlattenHierarchy, BindingFlags.Instance))
						{
							list.Add(field);
						}
					}
				}
			}
			return list.ToArray();
		}

		public Type[] GetInterfaces()
		{
			List<Type> list = new List<Type>();
			for (Type type = this; type != null; type = type.BaseType)
			{
				AddInterfaces(list, type);
			}
			return list.ToArray();
		}

		private static void AddInterfaces(List<Type> list, Type type)
		{
			type.CheckBaked();
			foreach (Type iface in type.__GetDeclaredInterfaces())
			{
				if (!list.Contains(iface))
				{
					list.Add(iface);
					AddInterfaces(list, iface);
				}
			}
		}

		public MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			CheckBaked();
			List<MethodInfo> list = new List<MethodInfo>();
			foreach (MethodBase mb in __GetDeclaredMethods())
			{
				MethodInfo mi = mb as MethodInfo;
				if (mi != null
					&& BindingFlagsMatch(mi.IsPublic, bindingAttr, BindingFlags.Public, BindingFlags.NonPublic)
					&& BindingFlagsMatch(mi.IsStatic, bindingAttr, BindingFlags.Static, BindingFlags.Instance))
				{
					list.Add(mi);
				}
			}
			if ((bindingAttr & BindingFlags.DeclaredOnly) == 0)
			{
				for (Type type = this.BaseType; type != null; type = type.BaseType)
				{
					type.CheckBaked();
					foreach (MethodBase mb in type.__GetDeclaredMethods())
					{
						MethodInfo mi = mb as MethodInfo;
						if (mi != null
							&& (mi.Attributes & MethodAttributes.MemberAccessMask) > MethodAttributes.Private
							&& BindingFlagsMatch(mi.IsPublic, bindingAttr, BindingFlags.Public, BindingFlags.NonPublic)
							&& BindingFlagsMatch(mi.IsStatic, bindingAttr, BindingFlags.Static | BindingFlags.FlattenHierarchy, BindingFlags.Instance)
							&& !FindMethod(list, mi))
						{
							list.Add(mi);
						}
					}
				}
			}
			return list.ToArray();
		}

		private static bool FindMethod(List<MethodInfo> methods, MethodInfo method)
		{
			foreach (MethodInfo m in methods)
			{
				if (m.Name == method.Name && m.MethodSignature.Equals(method.MethodSignature))
				{
					return true;
				}
			}
			return false;
		}

		public MethodInfo[] GetMethods()
		{
			return GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
		}

		public MethodInfo GetMethod(string name)
		{
			return GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
		{
			MethodInfo found = null;
			foreach (MethodInfo method in GetMethods(bindingAttr))
			{
				if (method.Name == name)
				{
					if (found != null)
					{
						throw new AmbiguousMatchException();
					}
					found = method;
				}
			}
			return found;
		}

		public MethodInfo GetMethod(string name, Type[] types)
		{
			return GetMethod(name, types, null);
		}

		public MethodInfo GetMethod(string name, Type[] types, ParameterModifier[] modifiers)
		{
			return GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, types, modifiers);
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
		{
			MethodInfo found = null;
			foreach (MethodInfo method in GetMethods(bindingAttr))
			{
				if (method.Name == name && method.MethodSignature.MatchParameterTypes(types))
				{
					if (found != null)
					{
						throw new AmbiguousMatchException();
					}
					found = method;
				}
			}
			return found;
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			// FXBUG callConvention seems to be ignored
			return GetMethod(name, bindingAttr, binder, types, modifiers);
		}

		public ConstructorInfo[] GetConstructors()
		{
			return GetConstructors(BindingFlags.Public | BindingFlags.Instance);
		}

		public ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			CheckBaked();
			List<ConstructorInfo> list = new List<ConstructorInfo>();
			foreach (MethodBase mb in __GetDeclaredMethods())
			{
				ConstructorInfo constructor = mb as ConstructorInfo;
				if (constructor != null
					&& BindingFlagsMatch(constructor.IsPublic, bindingAttr, BindingFlags.Public, BindingFlags.NonPublic)
					&& BindingFlagsMatch(constructor.IsStatic, bindingAttr, BindingFlags.Static, BindingFlags.Instance))
				{
					list.Add(constructor);
				}
			}
			return list.ToArray();
		}

		public ConstructorInfo GetConstructor(Type[] types)
		{
			return GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Standard, types, null);
		}

		public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
		{
			foreach (ConstructorInfo constructor in GetConstructors(bindingAttr))
			{
				if (constructor.MethodSignature.MatchParameterTypes(types))
				{
					return constructor;
				}
			}
			return null;
		}

		public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, CallingConventions callingConvention, Type[] types, ParameterModifier[] modifiers)
		{
			// FXBUG callConvention seems to be ignored
			return GetConstructor(bindingAttr, binder, types, modifiers);
		}

		internal Type ResolveNestedType(TypeName typeName)
		{
			return FindNestedType(typeName) ?? Module.universe.GetMissingTypeOrThrow(Module, this, typeName);
		}

		// unlike the public API, this takes the namespace and name into account
		internal virtual Type FindNestedType(TypeName name)
		{
			foreach (Type type in __GetDeclaredTypes())
			{
				if (type.__Namespace == name.Namespace && type.__Name == name.Name)
				{
					return type;
				}
			}
			return null;
		}

		public Type GetNestedType(string name)
		{
			return GetNestedType(name, BindingFlags.Public);
		}

		public Type GetNestedType(string name, BindingFlags bindingAttr)
		{
			foreach (Type type in GetNestedTypes(bindingAttr))
			{
				// FXBUG the namespace is ignored
				if (type.__Name == name)
				{
					return type;
				}
			}
			return null;
		}

		public Type[] GetNestedTypes()
		{
			return GetNestedTypes(BindingFlags.Public);
		}

		public Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			CheckBaked();
			List<Type> list = new List<Type>();
			foreach (Type type in __GetDeclaredTypes())
			{
				if (BindingFlagsMatch(type.IsNestedPublic, bindingAttr, BindingFlags.Public, BindingFlags.NonPublic))
				{
					list.Add(type);
				}
			}
			return list.ToArray();
		}

		public PropertyInfo[] GetProperties()
		{
			return GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}

		public PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			List<PropertyInfo> list = new List<PropertyInfo>();
			Type type = this;
			while (type != null)
			{
				type.CheckBaked();
				foreach (PropertyInfo property in type.__GetDeclaredProperties())
				{
					if (BindingFlagsMatch(property.IsPublic, bindingAttr, BindingFlags.Public, BindingFlags.NonPublic)
						&& BindingFlagsMatch(property.IsStatic, bindingAttr, BindingFlags.Static, BindingFlags.Instance))
					{
						list.Add(property);
					}
				}
				if ((bindingAttr & BindingFlags.DeclaredOnly) == 0)
				{
					if ((bindingAttr & BindingFlags.FlattenHierarchy) == 0)
					{
						bindingAttr &= ~BindingFlags.Static;
					}
					type = type.BaseType;
				}
				else
				{
					break;
				}
			}
			return list.ToArray();
		}

		public PropertyInfo GetProperty(string name)
		{
			return GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}

		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
		{
			foreach (PropertyInfo prop in GetProperties(bindingAttr))
			{
				if (prop.Name == name)
				{
					return prop;
				}
			}
			return null;
		}

		public PropertyInfo GetProperty(string name, Type returnType)
		{
			PropertyInfo found = null;
			foreach (PropertyInfo prop in GetProperties())
			{
				if (prop.Name == name && prop.PropertyType.Equals(returnType))
				{
					if (found != null)
					{
						throw new AmbiguousMatchException();
					}
					found = prop;
				}
			}
			return found;
		}

		public PropertyInfo GetProperty(string name, Type[] types)
		{
			PropertyInfo found = null;
			foreach (PropertyInfo prop in GetProperties())
			{
				if (prop.Name == name && MatchParameterTypes(prop.GetIndexParameters(), types))
				{
					if (found != null)
					{
						throw new AmbiguousMatchException();
					}
					found = prop;
				}
			}
			return found;
		}

		private static bool MatchParameterTypes(ParameterInfo[] parameters, Type[] types)
		{
			if (parameters.Length == types.Length)
			{
				for (int i = 0; i < parameters.Length; i++)
				{
					if (!parameters[i].ParameterType.Equals(types[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public PropertyInfo GetProperty(string name, Type returnType, Type[] types)
		{
			return GetProperty(name, returnType, types, null);
		}

		public PropertyInfo GetProperty(string name, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			return GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, returnType, types, modifiers);
		}

		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			PropertyInfo found = null;
			foreach (PropertyInfo prop in GetProperties(bindingAttr))
			{
				if (prop.Name == name && prop.PropertyType.Equals(returnType) && MatchParameterTypes(prop.GetIndexParameters(), types))
				{
					if (found != null)
					{
						throw new AmbiguousMatchException();
					}
					found = prop;
				}
			}
			return found;
		}

		public Type GetInterface(string name)
		{
			return GetInterface(name, false);
		}

		public Type GetInterface(string name, bool ignoreCase)
		{
			if (ignoreCase)
			{
				throw new NotImplementedException();
			}
			foreach (Type type in GetInterfaces())
			{
				if (type.FullName == name)
				{
					return type;
				}
			}
			return null;
		}

		public Type[] FindInterfaces(TypeFilter filter, object filterCriteria)
		{
			List<Type> list = new List<Type>();
			foreach (Type type in GetInterfaces())
			{
				if (filter(type, filterCriteria))
				{
					list.Add(type);
				}
			}
			return list.ToArray();
		}

		public ConstructorInfo TypeInitializer
		{
			get { return GetConstructor(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null); }
		}

		public bool IsPrimitive
		{
			get
			{
				Universe u = this.Module.universe;
				return this == u.System_Boolean
					|| this == u.System_Byte
					|| this == u.System_SByte
					|| this == u.System_Int16
					|| this == u.System_UInt16
					|| this == u.System_Int32
					|| this == u.System_UInt32
					|| this == u.System_Int64
					|| this == u.System_UInt64
					|| this == u.System_IntPtr
					|| this == u.System_UIntPtr
					|| this == u.System_Char
					|| this == u.System_Double
					|| this == u.System_Single
					;
			}
		}

		public bool IsEnum
		{
			get { return this.BaseType == this.Module.universe.System_Enum; }
		}

		public bool IsSealed
		{
			get { return (Attributes & TypeAttributes.Sealed) != 0; }
		}

		public bool IsAbstract
		{
			get { return (Attributes & TypeAttributes.Abstract) != 0; }
		}

		private bool CheckVisibility(TypeAttributes access)
		{
			return (Attributes & TypeAttributes.VisibilityMask) == access;
		}

		public bool IsPublic
		{
			get { return CheckVisibility(TypeAttributes.Public); }
		}

		public bool IsNestedPublic
		{
			get { return CheckVisibility(TypeAttributes.NestedPublic); }
		}

		public bool IsNestedPrivate
		{
			get { return CheckVisibility(TypeAttributes.NestedPrivate); }
		}

		public bool IsNestedFamily
		{
			get { return CheckVisibility(TypeAttributes.NestedFamily); }
		}

		public bool IsNestedAssembly
		{
			get { return CheckVisibility(TypeAttributes.NestedAssembly); }
		}

		public bool IsNestedFamANDAssem
		{
			get { return CheckVisibility(TypeAttributes.NestedFamANDAssem); }
		}

		public bool IsNestedFamORAssem
		{
			get { return CheckVisibility(TypeAttributes.NestedFamORAssem); }
		}

		public bool IsNotPublic
		{
			get { return CheckVisibility(TypeAttributes.NotPublic); }
		}

		public bool IsImport
		{
			get { return (Attributes & TypeAttributes.Import) != 0; }
		}

		public bool IsCOMObject
		{
			get { return IsClass && IsImport; }
		}

		public bool IsContextful
		{
			get { return IsSubclassOf(this.Module.universe.Import(typeof(ContextBoundObject))); }
		}

		public bool IsMarshalByRef
		{
			get { return IsSubclassOf(this.Module.universe.Import(typeof(MarshalByRefObject))); }
		}

		public virtual bool IsVisible
		{
			get { return IsPublic || (IsNestedPublic && this.DeclaringType.IsVisible); }
		}

		public bool IsAnsiClass
		{
			get { return (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.AnsiClass; }
		}

		public bool IsUnicodeClass
		{
			get { return (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass; }
		}

		public bool IsAutoClass
		{
			get { return (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass; }
		}

		public bool IsAutoLayout
		{
			get { return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout; }
		}

		public bool IsLayoutSequential
		{
			get { return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout; }
		}

		public bool IsExplicitLayout
		{
			get { return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout; }
		}

		public bool IsSpecialName
		{
			get { return (Attributes & TypeAttributes.SpecialName) != 0; }
		}

		public bool IsSerializable
		{
			get { return (Attributes & TypeAttributes.Serializable) != 0; }
		}

		public bool IsClass
		{
			get { return !IsInterface && !IsValueType; }
		}

		public bool IsInterface
		{
			get { return (Attributes & TypeAttributes.Interface) != 0; }
		}

		public bool IsNested
		{
			// FXBUG we check the declaring type (like .NET) and this results
			// in IsNested returning true for a generic type parameter
			get { return this.DeclaringType != null; }
		}

		public bool __IsMissing
		{
			get { return this is MissingType; }
		}

		public virtual bool __ContainsMissingType
		{
			get
			{
				if (this.__IsMissing)
				{
					return true;
				}
				foreach (Type arg in this.GetGenericArguments())
				{
					if (arg.__ContainsMissingType)
					{
						return true;
					}
				}
				return false;
			}
		}

		public Type MakeArrayType()
		{
			return ArrayType.Make(this, null, null);
		}

		public Type __MakeArrayType(Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return ArrayType.Make(this, Util.Copy(requiredCustomModifiers), Util.Copy(optionalCustomModifiers));
		}

		public Type MakeArrayType(int rank)
		{
			return MultiArrayType.Make(this, rank, null, null);
		}

		public Type __MakeArrayType(int rank, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return MultiArrayType.Make(this, rank, Util.Copy(requiredCustomModifiers), Util.Copy(optionalCustomModifiers));
		}

		public Type MakeByRefType()
		{
			return ByRefType.Make(this, null, null);
		}

		public Type __MakeByRefType(Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return ByRefType.Make(this, Util.Copy(requiredCustomModifiers), Util.Copy(optionalCustomModifiers));
		}

		public Type MakePointerType()
		{
			return PointerType.Make(this, null, null);
		}

		public Type __MakePointerType(Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return PointerType.Make(this, Util.Copy(requiredCustomModifiers), Util.Copy(optionalCustomModifiers));
		}

		public Type MakeGenericType(params Type[] typeArguments)
		{
			return __MakeGenericType(typeArguments, null, null);
		}

		public Type __MakeGenericType(Type[] typeArguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			if (!this.IsGenericTypeDefinition)
			{
				throw new InvalidOperationException();
			}
			return GenericTypeInstance.Make(this, Util.Copy(typeArguments), Util.Copy(requiredCustomModifiers), Util.Copy(optionalCustomModifiers));
		}

		public static System.Type __GetSystemType(TypeCode typeCode)
		{
			switch (typeCode)
			{
				case TypeCode.Boolean:
					return typeof(System.Boolean);
				case TypeCode.Byte:
					return typeof(System.Byte);
				case TypeCode.Char:
					return typeof(System.Char);
				case TypeCode.DBNull:
					return typeof(System.DBNull);
				case TypeCode.DateTime:
					return typeof(System.DateTime);
				case TypeCode.Decimal:
					return typeof(System.Decimal);
				case TypeCode.Double:
					return typeof(System.Double);
				case TypeCode.Empty:
					return null;
				case TypeCode.Int16:
					return typeof(System.Int16);
				case TypeCode.Int32:
					return typeof(System.Int32);
				case TypeCode.Int64:
					return typeof(System.Int64);
				case TypeCode.Object:
					return typeof(System.Object);
				case TypeCode.SByte:
					return typeof(System.SByte);
				case TypeCode.Single:
					return typeof(System.Single);
				case TypeCode.String:
					return typeof(System.String);
				case TypeCode.UInt16:
					return typeof(System.UInt16);
				case TypeCode.UInt32:
					return typeof(System.UInt32);
				case TypeCode.UInt64:
					return typeof(System.UInt64);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static TypeCode GetTypeCode(Type type)
		{
			if (type == null)
			{
				return TypeCode.Empty;
			}
			if (type.IsEnum)
			{
				type = type.GetEnumUnderlyingType();
			}
			Universe u = type.Module.universe;
			if (type == u.System_Boolean)
			{
				return TypeCode.Boolean;
			}
			else if (type == u.System_Char)
			{
				return TypeCode.Char;
			}
			else if (type == u.System_SByte)
			{
				return TypeCode.SByte;
			}
			else if (type == u.System_Byte)
			{
				return TypeCode.Byte;
			}
			else if (type == u.System_Int16)
			{
				return TypeCode.Int16;
			}
			else if (type == u.System_UInt16)
			{
				return TypeCode.UInt16;
			}
			else if (type == u.System_Int32)
			{
				return TypeCode.Int32;
			}
			else if (type == u.System_UInt32)
			{
				return TypeCode.UInt32;
			}
			else if (type == u.System_Int64)
			{
				return TypeCode.Int64;
			}
			else if (type == u.System_UInt64)
			{
				return TypeCode.UInt64;
			}
			else if (type == u.System_Single)
			{
				return TypeCode.Single;
			}
			else if (type == u.System_Double)
			{
				return TypeCode.Double;
			}
			else if (type == u.System_DateTime)
			{
				return TypeCode.DateTime;
			}
			else if (type == u.System_DBNull)
			{
				return TypeCode.DBNull;
			}
			else if (type == u.System_Decimal)
			{
				return TypeCode.Decimal;
			}
			else if (type == u.System_String)
			{
				return TypeCode.String;
			}
			else
			{
				return TypeCode.Object;
			}
		}

		public Assembly Assembly
		{
			get { return Module.Assembly; }
		}

		// note that interface/delegate co- and contravariance is not considered
		public bool IsAssignableFrom(Type type)
		{
			if (this.Equals(type))
			{
				return true;
			}
			else if (type == null)
			{
				return false;
			}
			else if (this.IsArray && type.IsArray)
			{
				if (this.GetArrayRank() != type.GetArrayRank())
				{
					return false;
				}
				else if (this.__IsVector && !type.__IsVector)
				{
					return false;
				}
				Type e1 = this.GetElementType();
				Type e2 = type.GetElementType();
				return e1.IsValueType == e2.IsValueType && e1.IsAssignableFrom(e2);
			}
			else if (this.IsSealed)
			{
				return false;
			}
			else if (this.IsInterface)
			{
				return Array.IndexOf(type.GetInterfaces(), this) != -1;
			}
			else if (type.IsInterface)
			{
				return this == this.Module.universe.System_Object;
			}
			else if (type.IsPointer)
			{
				return this == this.Module.universe.System_Object || this == this.Module.universe.System_ValueType;
			}
			else
			{
				return type.IsSubclassOf(this);
			}
		}

		public bool IsSubclassOf(Type type)
		{
			Type thisType = this.BaseType;
			while (thisType != null)
			{
				if (thisType.Equals(type))
				{
					return true;
				}
				thisType = thisType.BaseType;
			}
			return false;
		}

		// This returns true if this type directly (i.e. not inherited from the base class) implements the interface.
		// Note that a complicating factor is that the interface itself can be implemented by an interface that extends it.
		private bool IsDirectlyImplementedInterface(Type interfaceType)
		{
			foreach (Type iface in __GetDeclaredInterfaces())
			{
				if (interfaceType.IsAssignableFrom(iface))
				{
					return true;
				}
			}
			return false;
		}

		public InterfaceMapping GetInterfaceMap(Type interfaceType)
		{
			CheckBaked();
			InterfaceMapping map = new InterfaceMapping();
			if (!IsDirectlyImplementedInterface(interfaceType))
			{
				Type baseType = this.BaseType;
				if (baseType == null)
				{
					throw new ArgumentException();
				}
				else
				{
					map = baseType.GetInterfaceMap(interfaceType);
				}
			}
			else
			{
				map.InterfaceMethods = interfaceType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				map.InterfaceType = interfaceType;
				map.TargetMethods = new MethodInfo[map.InterfaceMethods.Length];
				FillInExplicitInterfaceMethods(map.InterfaceMethods, map.TargetMethods);
				MethodInfo[] methods = GetMethods(BindingFlags.Instance | BindingFlags.Public);
				for (int i = 0; i < map.TargetMethods.Length; i++)
				{
					if (map.TargetMethods[i] == null)
					{
						// TODO use proper method resolution (also take into account that no implicit base class implementation is used across assembly boundaries)
						for (int j = 0; j < methods.Length; j++)
						{
							if (methods[j].Name == map.InterfaceMethods[i].Name
								&& methods[j].MethodSignature.Equals(map.InterfaceMethods[i].MethodSignature))
							{
								map.TargetMethods[i] = methods[j];
							}
						}
					}
				}
				for (Type baseType = this.BaseType; baseType != null && interfaceType.IsAssignableFrom(baseType); baseType = baseType.BaseType)
				{
					baseType.FillInExplicitInterfaceMethods(map.InterfaceMethods, map.TargetMethods);
				}
			}
			map.TargetType = this;
			return map;
		}

		internal void FillInExplicitInterfaceMethods(MethodInfo[] interfaceMethods, MethodInfo[] targetMethods)
		{
			__MethodImplMap impl = __GetMethodImplMap();
			for (int i = 0; i < impl.MethodDeclarations.Length; i++)
			{
				for (int j = 0; j < impl.MethodDeclarations[i].Length; j++)
				{
					int index = Array.IndexOf(interfaceMethods, impl.MethodDeclarations[i][j]);
					if (index != -1 && targetMethods[index] == null)
					{
						targetMethods[index] = impl.MethodBodies[i];
					}
				}
			}
		}

		Type IGenericContext.GetGenericTypeArgument(int index)
		{
			return GetGenericTypeArgument(index);
		}

		Type IGenericContext.GetGenericMethodArgument(int index)
		{
			throw new BadImageFormatException();
		}

		Type IGenericBinder.BindTypeParameter(Type type)
		{
			return GetGenericTypeArgument(type.GenericParameterPosition);
		}

		Type IGenericBinder.BindMethodParameter(Type type)
		{
			throw new BadImageFormatException();
		}

		internal virtual Type BindTypeParameters(IGenericBinder binder)
		{
			if (IsGenericTypeDefinition)
			{
				Type[] args = GetGenericArguments();
				Type.InplaceBindTypeParameters(binder, args);
				return GenericTypeInstance.Make(this, args, null, null);
			}
			else
			{
				return this;
			}
		}

		internal static void InplaceBindTypeParameters(IGenericBinder binder, Type[] types)
		{
			for (int i = 0; i < types.Length; i++)
			{
				types[i] = types[i].BindTypeParameters(binder);
			}
		}

		internal MethodBase FindMethod(string name, MethodSignature signature)
		{
			foreach (MethodBase method in __GetDeclaredMethods())
			{
				if (method.Name == name && method.MethodSignature.Equals(signature))
				{
					return method;
				}
			}
			return null;
		}

		internal FieldInfo FindField(string name, FieldSignature signature)
		{
			foreach (FieldInfo field in __GetDeclaredFields())
			{
				if (field.Name == name && field.FieldSignature.Equals(signature))
				{
					return field;
				}
			}
			return null;
		}

		internal bool IsAllowMultipleCustomAttribute
		{
			get
			{
				IList<CustomAttributeData> cad = GetCustomAttributesData(this.Module.universe.System_AttributeUsageAttribute);
				if (cad.Count == 1)
				{
					foreach (CustomAttributeNamedArgument arg in cad[0].NamedArguments)
					{
						if (arg.MemberInfo.Name == "AllowMultiple")
						{
							return (bool)arg.TypedValue.Value;
						}
					}
				}
				return false;
			}
		}

		internal bool IsPseudoCustomAttribute
		{
			get
			{
				Universe u = this.Module.universe;
				return this == u.System_NonSerializedAttribute
					|| this == u.System_SerializableAttribute
					|| this == u.System_Runtime_InteropServices_DllImportAttribute
					|| this == u.System_Runtime_InteropServices_FieldOffsetAttribute
					|| this == u.System_Runtime_InteropServices_InAttribute
					|| this == u.System_Runtime_InteropServices_MarshalAsAttribute
					|| this == u.System_Runtime_InteropServices_OutAttribute
					|| this == u.System_Runtime_InteropServices_StructLayoutAttribute
					|| this == u.System_Runtime_InteropServices_OptionalAttribute
					|| this == u.System_Runtime_InteropServices_PreserveSigAttribute
					|| this == u.System_Runtime_InteropServices_ComImportAttribute
					|| this == u.System_Runtime_CompilerServices_SpecialNameAttribute
					|| this == u.System_Runtime_CompilerServices_MethodImplAttribute
					;
			}
		}
	}

	abstract class ElementHolderType : Type
	{
		protected readonly Type elementType;
		private int token;
		private readonly Type[] requiredCustomModifiers;
		private readonly Type[] optionalCustomModifiers;

		protected ElementHolderType(Type elementType, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			this.elementType = elementType;
			this.requiredCustomModifiers = requiredCustomModifiers;
			this.optionalCustomModifiers = optionalCustomModifiers;
		}

		protected bool EqualsHelper(ElementHolderType other)
		{
			return other != null
				&& other.elementType.Equals(elementType)
				&& Util.ArrayEquals(other.requiredCustomModifiers, requiredCustomModifiers)
				&& Util.ArrayEquals(other.optionalCustomModifiers, optionalCustomModifiers);
		}

		public override Type[] __GetRequiredCustomModifiers()
		{
			return Util.Copy(requiredCustomModifiers);
		}

		public override Type[] __GetOptionalCustomModifiers()
		{
			return Util.Copy(optionalCustomModifiers);
		}

		public sealed override string Name
		{
			get { return elementType.Name + GetSuffix(); }
		}

		public sealed override string Namespace
		{
			get { return elementType.Namespace; }
		}

		public sealed override string FullName
		{
			get { return elementType.FullName + GetSuffix(); }
		}

		public sealed override string ToString()
		{
			return elementType.ToString() + GetSuffix();
		}

		public sealed override Type GetElementType()
		{
			return elementType;
		}

		public sealed override bool HasElementType
		{
			get { return true; }
		}

		public sealed override Module Module
		{
			get { return elementType.Module; }
		}

		internal sealed override int GetModuleBuilderToken()
		{
			if (token == 0)
			{
				token = ((ModuleBuilder)elementType.Module).ImportType(this);
			}
			return token;
		}

		public sealed override bool ContainsGenericParameters
		{
			get
			{
				Type type = elementType;
				while (type.HasElementType)
				{
					type = type.GetElementType();
				}
				return type.ContainsGenericParameters;
			}
		}

		public sealed override bool __ContainsMissingType
		{
			get
			{
				Type type = elementType;
				while (type.HasElementType)
				{
					type = type.GetElementType();
				}
				return type.__ContainsMissingType;
			}
		}

		internal sealed override Type BindTypeParameters(IGenericBinder binder)
		{
			Type type = elementType.BindTypeParameters(binder);
			Type[] req = BindArray(requiredCustomModifiers, binder);
			Type[] opt = BindArray(optionalCustomModifiers, binder);
			if (ReferenceEquals(type, elementType)
				&& ReferenceEquals(req, requiredCustomModifiers)
				&& ReferenceEquals(opt, optionalCustomModifiers))
			{
				return this;
			}
			return Wrap(type, req, opt);
		}

		internal override void CheckBaked()
		{
			elementType.CheckBaked();
		}

		private static Type[] BindArray(Type[] array, IGenericBinder binder)
		{
			if (array ==null || array.Length == 0)
			{
				return array;
			}
			Type[] result = array;
			for (int i = 0; i < array.Length; i++)
			{
				Type type = array[i].BindTypeParameters(binder);
				if (!ReferenceEquals(type, array[i]))
				{
					if (result == array)
					{
						result = (Type[])array.Clone();
					}
					result[i] = type;
				}
			}
			return result;
		}

		internal sealed override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			return CustomAttributeData.EmptyList;
		}

		protected abstract string GetSuffix();

		protected abstract Type Wrap(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers);
	}

	sealed class ArrayType : ElementHolderType
	{
		internal static Type Make(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return type.Module.CanonicalizeType(new ArrayType(type, requiredCustomModifiers, optionalCustomModifiers));
		}

		private ArrayType(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
			: base(type, requiredCustomModifiers, optionalCustomModifiers)
		{
		}

		public override Type BaseType
		{
			get { return elementType.Module.universe.System_Array; }
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			return new Type[] {
				this.Module.universe.Import(typeof(IList<>)).MakeGenericType(elementType),
				this.Module.universe.Import(typeof(ICollection<>)).MakeGenericType(elementType),
				this.Module.universe.Import(typeof(IEnumerable<>)).MakeGenericType(elementType)
			};
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			Type[] int32 = new Type[] { this.Module.universe.System_Int32 };
			List<MethodBase> list = new List<MethodBase>();
			list.Add(new BuiltinArrayMethod(this.Module, this, "Set", CallingConventions.Standard | CallingConventions.HasThis, this.Module.universe.System_Void, new Type[] { this.Module.universe.System_Int32, elementType }));
			list.Add(new BuiltinArrayMethod(this.Module, this, "Address", CallingConventions.Standard | CallingConventions.HasThis, elementType.MakeByRefType(), int32));
			list.Add(new BuiltinArrayMethod(this.Module, this, "Get", CallingConventions.Standard | CallingConventions.HasThis, elementType, int32));
			list.Add(new ConstructorInfoImpl(new BuiltinArrayMethod(this.Module, this, ".ctor", CallingConventions.Standard | CallingConventions.HasThis, this.Module.universe.System_Void, int32)));
			for (Type type = elementType; type.__IsVector; type = type.GetElementType())
			{
				Array.Resize(ref int32, int32.Length + 1);
				int32[int32.Length - 1] = int32[0];
				list.Add(new ConstructorInfoImpl(new BuiltinArrayMethod(this.Module, this, ".ctor", CallingConventions.Standard | CallingConventions.HasThis, this.Module.universe.System_Void, int32)));
			}
			return list.ToArray();
		}

		public override TypeAttributes Attributes
		{
			get { return TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable; }
		}

		public override bool IsArray
		{
			get { return true; }
		}

		public override bool __IsVector
		{
			get { return true; }
		}

		public override int GetArrayRank()
		{
			return 1;
		}

		public override bool Equals(object o)
		{
			return EqualsHelper(o as ArrayType);
		}

		public override int GetHashCode()
		{
			return elementType.GetHashCode() * 5;
		}

		protected override string GetSuffix()
		{
			return "[]";
		}

		protected override Type Wrap(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return Make(type, requiredCustomModifiers, optionalCustomModifiers);
		}
	}

	sealed class MultiArrayType : ElementHolderType
	{
		private readonly int rank;

		internal static Type Make(Type type, int rank, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return type.Module.CanonicalizeType(new MultiArrayType(type, rank, requiredCustomModifiers, optionalCustomModifiers));
		}

		private MultiArrayType(Type type, int rank, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
			: base(type, requiredCustomModifiers, optionalCustomModifiers)
		{
			this.rank = rank;
		}

		public override Type BaseType
		{
			get { return elementType.Module.universe.System_Array; }
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			Type int32 = this.Module.universe.System_Int32;
			Type[] setArgs = new Type[rank + 1];
			Type[] getArgs = new Type[rank];
			Type[] ctorArgs = new Type[rank * 2];
			for (int i = 0; i < rank; i++)
			{
				setArgs[i] = int32;
				getArgs[i] = int32;
				ctorArgs[i * 2 + 0] = int32;
				ctorArgs[i * 2 + 1] = int32;
			}
			setArgs[rank] = elementType;
			return new MethodBase[] {
				new ConstructorInfoImpl(new BuiltinArrayMethod(this.Module, this, ".ctor", CallingConventions.Standard | CallingConventions.HasThis, this.Module.universe.System_Void, getArgs)),
				new ConstructorInfoImpl(new BuiltinArrayMethod(this.Module, this, ".ctor", CallingConventions.Standard | CallingConventions.HasThis, this.Module.universe.System_Void, ctorArgs)),
				new BuiltinArrayMethod(this.Module, this, "Set", CallingConventions.Standard | CallingConventions.HasThis, this.Module.universe.System_Void, setArgs),
				new BuiltinArrayMethod(this.Module, this, "Address", CallingConventions.Standard | CallingConventions.HasThis, elementType.MakeByRefType(), getArgs),
				new BuiltinArrayMethod(this.Module, this, "Get", CallingConventions.Standard | CallingConventions.HasThis, elementType, getArgs),
			};
		}

		public override TypeAttributes Attributes
		{
			get { return TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable; }
		}

		public override bool IsArray
		{
			get { return true; }
		}

		public override int GetArrayRank()
		{
			return rank;
		}

		public override bool Equals(object o)
		{
			MultiArrayType at = o as MultiArrayType;
			return EqualsHelper(at) && at.rank == rank;
		}

		public override int GetHashCode()
		{
			return elementType.GetHashCode() * 9 + rank;
		}

		protected override string GetSuffix()
		{
			if (rank == 1)
			{
				return "[*]";
			}
			else
			{
				return "[" + new String(',', rank - 1) + "]";
			}
		}

		protected override Type Wrap(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return Make(type, rank, requiredCustomModifiers, optionalCustomModifiers);
		}
	}

	sealed class BuiltinArrayMethod : ArrayMethod
	{
		internal BuiltinArrayMethod(Module module, Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
			: base(module, arrayClass, methodName, callingConvention, returnType, parameterTypes)
		{
		}

		public override MethodAttributes Attributes
		{
			get { return this.Name == ".ctor" ? MethodAttributes.RTSpecialName | MethodAttributes.Public : MethodAttributes.Public; }
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return MethodImplAttributes.IL;
		}

		public override int MetadataToken
		{
			get { return 0x06000000; }
		}

		public override MethodBody GetMethodBody()
		{
			return null;
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] parameterInfos = new ParameterInfo[parameterTypes.Length];
			for (int i = 0; i < parameterInfos.Length; i++)
			{
				parameterInfos[i] = new ParameterInfoImpl(this, parameterTypes[i], i);
			}
			return parameterInfos;
		}

		public override ParameterInfo ReturnParameter
		{
			get { return new ParameterInfoImpl(this, this.ReturnType, -1); }
		}

		private sealed class ParameterInfoImpl : ParameterInfo
		{
			private readonly MethodInfo method;
			private readonly Type type;
			private readonly int pos;

			internal ParameterInfoImpl(MethodInfo method, Type type, int pos)
			{
				this.method = method;
				this.type = type;
				this.pos = pos;
			}

			public override Type ParameterType
			{
				get { return type; }
			}

			public override string Name
			{
				get { return null; }
			}

			public override ParameterAttributes Attributes
			{
				get { return ParameterAttributes.None; }
			}

			public override int Position
			{
				get { return pos; }
			}

			public override object RawDefaultValue
			{
				get { return null; }
			}

			public override Type[] GetOptionalCustomModifiers()
			{
				return Empty<Type>.Array;
			}

			public override Type[] GetRequiredCustomModifiers()
			{
				return Empty<Type>.Array;
			}

			public override MemberInfo Member
			{
				get { return method.IsConstructor ? (MethodBase)new ConstructorInfoImpl(method) : method; }
			}

			public override int MetadataToken
			{
				get { return 0x8000000; }
			}

			internal override Module Module
			{
				get { return method.Module; }
			}
		}
	}

	sealed class ByRefType : ElementHolderType
	{
		internal static Type Make(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return type.Module.CanonicalizeType(new ByRefType(type, requiredCustomModifiers, optionalCustomModifiers));
		}

		private ByRefType(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
			: base(type, requiredCustomModifiers, optionalCustomModifiers)
		{
		}

		public override bool Equals(object o)
		{
			return EqualsHelper(o as ByRefType);
		}

		public override int GetHashCode()
		{
			return elementType.GetHashCode() * 3;
		}

		public override Type BaseType
		{
			get { return null; }
		}

		public override TypeAttributes Attributes
		{
			get { return 0; }
		}

		public override bool IsByRef
		{
			get { return true; }
		}

		protected override string GetSuffix()
		{
			return "&";
		}

		protected override Type Wrap(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return Make(type, requiredCustomModifiers, optionalCustomModifiers);
		}
	}

	sealed class PointerType : ElementHolderType
	{
		internal static Type Make(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return type.Module.CanonicalizeType(new PointerType(type, requiredCustomModifiers, optionalCustomModifiers));
		}

		private PointerType(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
			: base(type, requiredCustomModifiers, optionalCustomModifiers)
		{
		}

		public override bool Equals(object o)
		{
			return EqualsHelper(o as PointerType);
		}

		public override int GetHashCode()
		{
			return elementType.GetHashCode() * 7;
		}

		public override Type BaseType
		{
			get { return null; }
		}

		public override TypeAttributes Attributes
		{
			get { return 0; }
		}

		public override bool IsPointer
		{
			get { return true; }
		}

		protected override string GetSuffix()
		{
			return "*";
		}

		protected override Type Wrap(Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return Make(type, requiredCustomModifiers, optionalCustomModifiers);
		}
	}

	sealed class GenericTypeInstance : Type
	{
		private readonly Type type;
		private readonly Type[] args;
		private readonly Type[][] requiredCustomModifiers;
		private readonly Type[][] optionalCustomModifiers;
		private Type baseType;
		private int token;

		internal static Type Make(Type type, Type[] typeArguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			bool identity = true;
			if (type is TypeBuilder || type is BakedType || type.__IsMissing)
			{
				// a TypeBuiler identity must be instantiated
				identity = false;
			}
			else
			{
				// we must not instantiate the identity instance, because typeof(Foo<>).MakeGenericType(typeof(Foo<>).GetGenericArguments()) == typeof(Foo<>)
				for (int i = 0; i < typeArguments.Length; i++)
				{
					if (typeArguments[i] != type.GetGenericTypeArgument(i)
						|| !IsEmpty(requiredCustomModifiers, i)
						|| !IsEmpty(optionalCustomModifiers, i))
					{
						identity = false;
						break;
					}
				}
			}
			if (identity)
			{
				return type;
			}
			else
			{
				return type.Module.CanonicalizeType(new GenericTypeInstance(type, typeArguments, requiredCustomModifiers, optionalCustomModifiers));
			}
		}

		private static bool IsEmpty(Type[][] mods, int i)
		{
			// we need to be extra careful, because mods doesn't not need to be in canonical format
			// (Signature.ReadGenericInst() calls Make() directly, without copying the modifier arrays)
			return mods == null || mods[i] == null || mods[i].Length == 0;
		}

		private GenericTypeInstance(Type type, Type[] args, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			this.type = type;
			this.args = args;
			this.requiredCustomModifiers = requiredCustomModifiers;
			this.optionalCustomModifiers = optionalCustomModifiers;
		}

		public override bool Equals(object o)
		{
			GenericTypeInstance gt = o as GenericTypeInstance;
			return gt != null && gt.type.Equals(type) && Util.ArrayEquals(gt.args, args)
				&& Util.ArrayEquals(gt.requiredCustomModifiers, requiredCustomModifiers)
				&& Util.ArrayEquals(gt.optionalCustomModifiers, optionalCustomModifiers);
		}

		public override int GetHashCode()
		{
			return type.GetHashCode() * 3 ^ Util.GetHashCode(args);
		}

		public override string AssemblyQualifiedName
		{
			get
			{
				string fn = FullName;
				return fn == null ? null : fn + ", " + type.Assembly.FullName;
			}
		}

		public override Type BaseType
		{
			get
			{
				if (baseType == null)
				{
					Type rawBaseType = type.BaseType;
					if (rawBaseType == null)
					{
						baseType = rawBaseType;
					}
					else
					{
						baseType = rawBaseType.BindTypeParameters(this);
					}
				}
				return baseType;
			}
		}

		public override bool IsValueType
		{
			get { return type.IsValueType; }
		}

		public override bool IsVisible
		{
			get
			{
				if (base.IsVisible)
				{
					foreach (Type arg in args)
					{
						if (!arg.IsVisible)
						{
							return false;
						}
					}
					return true;
				}
				return false;
			}
		}

		public override Type DeclaringType
		{
			get { return type.DeclaringType; }
		}

		public override TypeAttributes Attributes
		{
			get { return type.Attributes; }
		}

		internal override void CheckBaked()
		{
			type.CheckBaked();
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			FieldInfo[] fields = type.__GetDeclaredFields();
			for (int i = 0; i < fields.Length; i++)
			{
				fields[i] = fields[i].BindTypeParameters(this);
			}
			return fields;
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			Type[] interfaces = type.__GetDeclaredInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				interfaces[i] = interfaces[i].BindTypeParameters(this);
			}
			return interfaces;
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			MethodBase[] methods = type.__GetDeclaredMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				methods[i] = methods[i].BindTypeParameters(this);
			}
			return methods;
		}

		public override Type[] __GetDeclaredTypes()
		{
			return type.__GetDeclaredTypes();
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			EventInfo[] events = type.__GetDeclaredEvents();
			for (int i = 0; i < events.Length; i++)
			{
				events[i] = events[i].BindTypeParameters(this);
			}
			return events;
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			PropertyInfo[] properties = type.__GetDeclaredProperties();
			for (int i = 0; i < properties.Length; i++)
			{
				properties[i] = properties[i].BindTypeParameters(this);
			}
			return properties;
		}

		public override __MethodImplMap __GetMethodImplMap()
		{
			__MethodImplMap map = type.__GetMethodImplMap();
			map.TargetType = this;
			for (int i = 0; i < map.MethodBodies.Length; i++)
			{
				map.MethodBodies[i] = (MethodInfo)map.MethodBodies[i].BindTypeParameters(this);
				for (int j = 0; j < map.MethodDeclarations[i].Length; j++)
				{
					Type interfaceType = map.MethodDeclarations[i][j].DeclaringType;
					if (interfaceType.IsGenericType)
					{
						map.MethodDeclarations[i][j] = (MethodInfo)map.MethodDeclarations[i][j].BindTypeParameters(this);
					}
				}
			}
			return map;
		}

		public override string Namespace
		{
			get { return type.Namespace; }
		}

		public override Type UnderlyingSystemType
		{
			get { return this; }
		}

		public override string Name
		{
			get { return type.Name; }
		}

		public override string FullName
		{
			get
			{
				if (this.ContainsGenericParameters)
				{
					return null;
				}
				StringBuilder sb = new StringBuilder(this.type.FullName);
				sb.Append('[');
				foreach (Type type in args)
				{
					sb.Append('[').Append(type.AssemblyQualifiedName.Replace("]", "\\]")).Append(']');
				}
				sb.Append(']');
				return sb.ToString();
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(type.FullName);
			sb.Append('[');
			string sep = "";
			foreach (Type arg in args)
			{
				sb.Append(sep);
				sb.Append(arg);
				sep = ",";
			}
			sb.Append(']');
			return sb.ToString();
		}

		public override Module Module
		{
			get { return type.Module; }
		}

		public override bool IsGenericType
		{
			get { return true; }
		}

		public override Type GetGenericTypeDefinition()
		{
			return type;
		}

		public override Type[] GetGenericArguments()
		{
			return Util.Copy(args);
		}

		public override Type[][] __GetGenericArgumentsRequiredCustomModifiers()
		{
			return Util.Copy(requiredCustomModifiers ?? new Type[args.Length][]);
		}

		public override Type[][] __GetGenericArgumentsOptionalCustomModifiers()
		{
			return Util.Copy(optionalCustomModifiers ?? new Type[args.Length][]);
		}

		internal override Type GetGenericTypeArgument(int index)
		{
			return args[index];
		}

		public override bool ContainsGenericParameters
		{
			get
			{
				foreach (Type type in args)
				{
					if (type.ContainsGenericParameters)
					{
						return true;
					}
				}
				return false;
			}
		}

		public override bool __ContainsMissingType
		{
			get
			{
				foreach (Type type in args)
				{
					if (type.__ContainsMissingType)
					{
						return true;
					}
				}
				return this.type.__IsMissing;
			}
		}

		public override StructLayoutAttribute StructLayoutAttribute
		{
			get { return type.StructLayoutAttribute; }
		}

		internal override int GetModuleBuilderToken()
		{
			if (token == 0)
			{
				token = ((ModuleBuilder)type.Module).ImportType(this);
			}
			return token;
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			for (int i = 0; i < args.Length; i++)
			{
				Type xarg = args[i].BindTypeParameters(binder);
				if (!ReferenceEquals(xarg, args[i]))
				{
					Type[] xargs = new Type[args.Length];
					Array.Copy(args, xargs, i);
					xargs[i++] = xarg;
					for (; i < args.Length; i++)
					{
						xargs[i] = args[i].BindTypeParameters(binder);
					}
					return Make(type, xargs, null, null);
				}
			}
			return this;
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			return type.GetCustomAttributesData(attributeType);
		}
	}
}
