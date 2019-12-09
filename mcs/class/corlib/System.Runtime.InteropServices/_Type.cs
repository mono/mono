//
// System.Runtime.InteropServices._Type interface
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

using System;
using System.Globalization;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[Guid ("BCA8B44D-AAD6-3A86-8AB7-03349F4F2DA2")]
#if !FULL_AOT_RUNTIME
	[TypeLibImportClass (typeof (Type))]
#endif
	[ComVisible (true)]
	public interface _Type
	{
		void GetTypeInfoCount(out uint pcTInfo);
		void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
		void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
		void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult,
			IntPtr pExcepInfo, IntPtr puArgErr);

		String ToString();
		bool Equals(Object other);
		int GetHashCode();
		Type GetType();

		MemberTypes MemberType { get; }
		String Name { get; }
		Type DeclaringType { get; }
		Type ReflectedType { get; }
		Object[] GetCustomAttributes(Type attributeType, bool inherit);
		Object[] GetCustomAttributes(bool inherit);
		bool IsDefined(Type attributeType, bool inherit);

		Guid GUID { get; }
		Module Module { get; }
		Assembly Assembly { get; }
		RuntimeTypeHandle TypeHandle { get; }
		String FullName { get; }
		String Namespace { get; }
		String AssemblyQualifiedName { get; }
		int GetArrayRank();
		Type BaseType { get; }

		ConstructorInfo[] GetConstructors(BindingFlags bindingAttr);
		Type GetInterface(String name, bool ignoreCase);
		Type[] GetInterfaces();
		Type[] FindInterfaces(TypeFilter filter,Object filterCriteria);
		EventInfo GetEvent(String name,BindingFlags bindingAttr);
		EventInfo[] GetEvents();
		EventInfo[] GetEvents(BindingFlags bindingAttr);
		Type[] GetNestedTypes(BindingFlags bindingAttr);
		Type GetNestedType(String name, BindingFlags bindingAttr);
		MemberInfo[] GetMember(String name, MemberTypes type, BindingFlags bindingAttr);
		MemberInfo[] GetDefaultMembers();
		MemberInfo[] FindMembers(MemberTypes memberType,BindingFlags bindingAttr,MemberFilter filter,Object filterCriteria);
		Type GetElementType();
		bool IsSubclassOf(Type c);
		bool IsInstanceOfType(Object o);
		bool IsAssignableFrom(Type c);
		InterfaceMapping GetInterfaceMap(Type interfaceType);
		MethodInfo GetMethod(String name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers);
		MethodInfo GetMethod(String name, BindingFlags bindingAttr);
		MethodInfo[] GetMethods(BindingFlags bindingAttr);
		FieldInfo GetField(String name, BindingFlags bindingAttr);
		FieldInfo[] GetFields(BindingFlags bindingAttr);
		PropertyInfo GetProperty(String name, BindingFlags bindingAttr);
		PropertyInfo GetProperty(String name,BindingFlags bindingAttr,Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers);
		PropertyInfo[] GetProperties(BindingFlags bindingAttr);
		MemberInfo[] GetMember(String name, BindingFlags bindingAttr);
		MemberInfo[] GetMembers(BindingFlags bindingAttr);
		Object InvokeMember(String name, BindingFlags invokeAttr, Binder binder, Object target, Object[] args, ParameterModifier[] modifiers,
			CultureInfo culture, String[] namedParameters);
		Type UnderlyingSystemType { get; }

		Object InvokeMember(String name,BindingFlags invokeAttr,Binder binder, Object target, Object[] args, CultureInfo culture);
		Object InvokeMember(String name,BindingFlags invokeAttr,Binder binder, Object target, Object[] args);
		ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);
		ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers);
		ConstructorInfo GetConstructor(Type[] types);
		ConstructorInfo[] GetConstructors();
		ConstructorInfo TypeInitializer{ get; }

		MethodInfo GetMethod(String name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);
		MethodInfo GetMethod(String name, Type[] types, ParameterModifier[] modifiers);
		MethodInfo GetMethod(String name, Type[] types);
		MethodInfo GetMethod(String name);
		MethodInfo[] GetMethods();
		FieldInfo GetField(String name);
		FieldInfo[] GetFields();
		Type GetInterface(String name);
		EventInfo GetEvent(String name);
		PropertyInfo GetProperty(String name, Type returnType, Type[] types,ParameterModifier[] modifiers);
		PropertyInfo GetProperty(String name, Type returnType, Type[] types);
		PropertyInfo GetProperty(String name, Type[] types);
		PropertyInfo GetProperty(String name, Type returnType);
		PropertyInfo GetProperty(String name);
		PropertyInfo[] GetProperties();
		Type[] GetNestedTypes();
		Type GetNestedType(String name);
		MemberInfo[] GetMember(String name);
		MemberInfo[] GetMembers();
		TypeAttributes Attributes { get; }
		bool IsNotPublic { get; }
		bool IsPublic { get; }
		bool IsNestedPublic { get; }
		bool IsNestedPrivate { get; }
		bool IsNestedFamily { get; }
		bool IsNestedAssembly { get; }
		bool IsNestedFamANDAssem { get; }
		bool IsNestedFamORAssem { get; }
		bool IsAutoLayout { get; }
		bool IsLayoutSequential { get; }
		bool IsExplicitLayout { get; }
		bool IsClass { get; }
		bool IsInterface { get; }
		bool IsValueType { get; }
		bool IsAbstract { get; }
		bool IsSealed { get; }
		bool IsEnum { get; }
		bool IsSpecialName { get; }
		bool IsImport { get; }
		bool IsSerializable { get; }
		bool IsAnsiClass { get; }
		bool IsUnicodeClass { get; }
		bool IsAutoClass { get; }
		bool IsArray { get; }
		bool IsByRef { get; }
		bool IsPointer { get; }
		bool IsPrimitive { get; }
		bool IsCOMObject { get; }
		bool HasElementType { get; }
		bool IsContextful { get; }
		bool IsMarshalByRef { get; }
		bool Equals(Type o);
	}
}
