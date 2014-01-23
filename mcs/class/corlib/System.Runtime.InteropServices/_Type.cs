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
		bool Equals (object other);

		bool Equals (Type o);

		Type[] FindInterfaces (TypeFilter filter, object filterCriteria);

		MemberInfo[] FindMembers (MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria);

		int GetArrayRank ();

		ConstructorInfo GetConstructor (Type[] types);

		ConstructorInfo GetConstructor (BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers);

		ConstructorInfo GetConstructor (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

		ConstructorInfo[] GetConstructors ();

		ConstructorInfo[] GetConstructors (BindingFlags bindingAttr);

		object[] GetCustomAttributes (bool inherit);

		object[] GetCustomAttributes (Type attributeType, bool inherit);

		MemberInfo[] GetDefaultMembers ();

		Type GetElementType ();

		EventInfo GetEvent (string name);

		EventInfo GetEvent (string name, BindingFlags bindingAttr);

		EventInfo[] GetEvents ();

		EventInfo[] GetEvents (BindingFlags bindingAttr);

		FieldInfo GetField (string name);

		FieldInfo GetField (string name, BindingFlags bindingAttr);

		FieldInfo[] GetFields ();

		FieldInfo[] GetFields (BindingFlags bindingAttr);

		int GetHashCode ();

		void GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);

		void GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo);

		void GetTypeInfoCount (out uint pcTInfo);

		void Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

		Type GetInterface (string name);

		Type GetInterface (string name, bool ignoreCase);

		InterfaceMapping GetInterfaceMap (Type interfaceType);

		Type[] GetInterfaces ();

		MemberInfo[] GetMember (string name);

		MemberInfo[] GetMember (string name, MemberTypes type, BindingFlags bindingAttr);

		MemberInfo[] GetMember (string name, BindingFlags bindingAttr);

		MemberInfo[] GetMembers ();

		MemberInfo[] GetMembers (BindingFlags bindingAttr);

		MethodInfo GetMethod (string name);

		MethodInfo GetMethod (string name, BindingFlags bindingAttr);

		MethodInfo GetMethod (string name, Type[] types);

		MethodInfo GetMethod (string name, Type[] types, ParameterModifier[] modifiers);

		MethodInfo GetMethod (string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers);

		MethodInfo GetMethod (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

		MethodInfo[] GetMethods ();

		MethodInfo[] GetMethods (BindingFlags bindingAttr);

		Type GetNestedType (string name);

		Type GetNestedType (string name, BindingFlags bindingAttr);

		Type[] GetNestedTypes ();

		Type[] GetNestedTypes (BindingFlags bindingAttr);

		PropertyInfo[] GetProperties ();

		PropertyInfo[] GetProperties (BindingFlags bindingAttr);

		PropertyInfo GetProperty (string name);

		PropertyInfo GetProperty (string name, BindingFlags bindingAttr);

		PropertyInfo GetProperty (string name, Type returnType);

		PropertyInfo GetProperty (string name, Type[] types);

		PropertyInfo GetProperty (string name, Type returnType, Type[] types);

		PropertyInfo GetProperty (string name, Type returnType, Type[] types, ParameterModifier[] modifiers);

		PropertyInfo GetProperty (string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers);

		Type GetType ();

		object InvokeMember (string name, BindingFlags invokeAttr, Binder binder, object target, object[] args);

		object InvokeMember (string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, CultureInfo culture);

		object InvokeMember (string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters);

		bool IsAssignableFrom (Type c);

		bool IsDefined (Type attributeType, bool inherit);

		bool IsInstanceOfType (object o);

		bool IsSubclassOf (Type c);

		string ToString ();

		Assembly Assembly {get;}

		string AssemblyQualifiedName {get;}

		TypeAttributes Attributes {get;}

		Type BaseType {get;}

		Type DeclaringType {get;}

		string FullName {get;}

		Guid GUID {get;}

		bool HasElementType {get;}

		bool IsAbstract {get;}

		bool IsAnsiClass {get;}

		bool IsArray {get;}

		bool IsAutoClass {get;}

		bool IsAutoLayout {get;}

		bool IsByRef {get;}

		bool IsClass {get;}

		bool IsCOMObject {get;}

		bool IsContextful {get;}

		bool IsEnum {get;}

		bool IsExplicitLayout {get;}

		bool IsImport {get;}

		bool IsInterface {get;}

		bool IsLayoutSequential {get;}

		bool IsMarshalByRef {get;}

		bool IsNestedAssembly {get;}

		bool IsNestedFamANDAssem {get;}

		bool IsNestedFamily {get;}

		bool IsNestedFamORAssem {get;}

		bool IsNestedPrivate {get;}

		bool IsNestedPublic {get;}

		bool IsNotPublic {get;}

		bool IsPointer {get;}

		bool IsPrimitive {get;}

		bool IsPublic {get;}

		bool IsSealed {get;}

		bool IsSerializable {get;}

		bool IsSpecialName {get;}

		bool IsUnicodeClass {get;}

		bool IsValueType {get;}

		MemberTypes MemberType {get;}

		Module Module {get;}

		string Name {get;}

		string Namespace {get;}

		Type ReflectedType {get;}

		RuntimeTypeHandle TypeHandle {get;}

		ConstructorInfo TypeInitializer {get;}

		Type UnderlyingSystemType {get;}
	}
}
