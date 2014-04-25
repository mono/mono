//
// monotouch.cs: iOS System.Reflection.Emit API needed to simplify mcs compilation
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Reflection.Emit
{
	public class ILGenerator
	{
		public void BeginCatchBlock (Type exceptionType)		
		{
			throw new NotSupportedException ();
		}

		public Label BeginExceptionBlock ()
		{
			throw new NotSupportedException ();
		}

		public void BeginExceptFilterBlock ()
		{
			throw new NotSupportedException ();
		}

		public void BeginFinallyBlock ()
		{
			throw new NotSupportedException ();
		}

		public LocalBuilder DeclareLocal (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public Label DefineLabel ()
		{
			throw new NotSupportedException ();
		}

		public void Emit (OpCode opcode)
		{
			throw new NotSupportedException ();
		}

		public void Emit (OpCode opcode, object args)
		{
			throw new NotSupportedException ();
		}

		public  void EmitCall (OpCode opcode, params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void EndExceptionBlock ()
		{
			throw new NotSupportedException ();
		}

		public void MarkLabel (Label loc)
		{
			throw new NotSupportedException ();
		}

		public int ILOffset { get; set; }
	}

	public class TypeBuilder : Type
	{
		#region implemented abstract members of MemberInfo

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override string Name {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		#region implemented abstract members of Type

		public override Type GetInterface (string name, bool ignoreCase)
		{
			throw new NotSupportedException ();
		}

		public override Type[] GetInterfaces ()
		{
			throw new NotSupportedException ();
		}

		public override Type GetElementType ()
		{
			throw new NotSupportedException ();
		}

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override EventInfo[] GetEvents (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override FieldInfo GetField (string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override Type GetNestedType (string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override Type[] GetNestedTypes (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool HasElementTypeImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsArrayImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsByRefImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsCOMObjectImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsPointerImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsPrimitiveImpl ()
		{
			throw new NotSupportedException ();
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override object InvokeMember (string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
		{
			throw new NotSupportedException ();
		}

		public override Assembly Assembly {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string AssemblyQualifiedName {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type BaseType {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string FullName {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Guid GUID {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Module Module {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string Namespace {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type UnderlyingSystemType {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		public void AddInterfaceImplementation (Type interfaceType)
		{
			throw new NotSupportedException ();
		}

		public void AddDeclarativeSecurity (params object[] args)
		{
			throw new NotSupportedException ();
		}	

		public void SetParent (object arg)
		{
			throw new NotSupportedException ();
		}

		public Type CreateType()
		{
			throw new NotSupportedException ();
		}

		public ConstructorBuilder DefineConstructor (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public MethodBuilder DefineMethod (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public TypeBuilder DefineNestedType (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public PropertyBuilder DefineProperty (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public EventBuilder DefineEvent (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public FieldBuilder DefineField (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters (params string[] names)
		{
			throw new NotSupportedException ();
		}

		public MethodBuilder DefineMethodOverride (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public static ConstructorInfo GetConstructor (Type type, ConstructorInfo constructor)
		{
			throw new NotSupportedException ();
		}

		public static FieldInfo GetField (Type type, FieldInfo field)
		{
			throw new NotSupportedException ();
		}

		public static MethodInfo GetMethod (Type type, MethodInfo method)
		{
			throw new NotSupportedException ();
		}

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}
	}

	public class MethodBuilder : MethodBase
	{
		#region implemented abstract members of MemberInfo

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override Type DeclaringType {
			get {
				throw new NotSupportedException ();
			}
		}

		public override MemberTypes MemberType {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string Name {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type ReflectedType {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		#region implemented abstract members of MethodBase

		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			throw new NotSupportedException ();
		}

		public override ParameterInfo[] GetParameters ()
		{
			throw new NotSupportedException ();
		}

		public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException ();
		}

		public override RuntimeMethodHandle MethodHandle {
			get {
				throw new NotSupportedException ();
			}
		}

		public override MethodAttributes Attributes {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		public void AddDeclarativeSecurity (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public ParameterBuilder DefineParameter (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters (params string[] names)
		{
			throw new NotSupportedException ();
		}

		public MethodToken GetToken()
		{
			throw new NotSupportedException ();
		}

		public ILGenerator GetILGenerator ()
		{
			throw new NotSupportedException ();
		}

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void SetImplementationFlags (MethodImplAttributes attributes)
		{
			throw new NotSupportedException ();
		}

		public void SetParameters (params Type[] parameterTypes)
		{
			throw new NotSupportedException ();
		}

		public void SetReturnType (object arg)
		{
			throw new NotSupportedException ();
		}
	}

	public class AssemblyBuilder : Assembly
	{
		public void AddResourceFile (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void DefineVersionInfoResource (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public ModuleBuilder DefineDynamicModule (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void DefineUnmanagedResource (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void Save (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void SetEntryPoint (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}
	}

	public class LocalBuilder : LocalVariableInfo
	{	
	}

	public class GenericTypeParameterBuilder : Type
	{
		#region implemented abstract members of MemberInfo

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override string Name {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		#region implemented abstract members of Type

		public override Type GetInterface (string name, bool ignoreCase)
		{
			throw new NotSupportedException ();
		}

		public override Type[] GetInterfaces ()
		{
			throw new NotSupportedException ();
		}

		public override Type GetElementType ()
		{
			throw new NotSupportedException ();
		}

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override EventInfo[] GetEvents (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override FieldInfo GetField (string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override Type GetNestedType (string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override Type[] GetNestedTypes (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool HasElementTypeImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsArrayImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsByRefImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsCOMObjectImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsPointerImpl ()
		{
			throw new NotSupportedException ();
		}

		protected override bool IsPrimitiveImpl ()
		{
			throw new NotSupportedException ();
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override object InvokeMember (string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
		{
			throw new NotSupportedException ();
		}

		public override Assembly Assembly {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string AssemblyQualifiedName {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type BaseType {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string FullName {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Guid GUID {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Module Module {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string Namespace {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type UnderlyingSystemType {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void SetGenericParameterAttributes (GenericParameterAttributes genericParameterAttributes)
		{
			throw new NotSupportedException ();
		}

		public void SetInterfaceConstraints (params Type[] interfaceConstraints)
		{
			throw new NotSupportedException ();
		}

		public void SetBaseTypeConstraint (Type baseTypeConstraint)
		{
			throw new NotSupportedException ();
		}		
	}

	public class ConstructorBuilder : MethodBase
	{
		#region implemented abstract members of MemberInfo

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override Type DeclaringType {
			get {
				throw new NotSupportedException ();
			}
		}

		public override MemberTypes MemberType {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string Name {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type ReflectedType {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		#region implemented abstract members of MethodBase

		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			throw new NotSupportedException ();
		}

		public override ParameterInfo[] GetParameters ()
		{
			throw new NotSupportedException ();
		}

		public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException ();
		}

		public override RuntimeMethodHandle MethodHandle {
			get {
				throw new NotSupportedException ();
			}
		}

		public override MethodAttributes Attributes {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		public void AddDeclarativeSecurity (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public ParameterBuilder DefineParameter (params object[] args)
		{
			throw new NotSupportedException ();
		}		

		public MethodToken GetToken()
		{
			throw new NotSupportedException ();
		}

		public ILGenerator GetILGenerator ()
		{
			throw new NotSupportedException ();
		}

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void SetImplementationFlags (MethodImplAttributes attributes)
		{
			throw new NotSupportedException ();
		}	
	}

	public class ModuleBuilder : Module
	{
		public void DefineManifestResource (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public TypeBuilder DefineType (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public MethodToken GetToken()
		{
			throw new NotSupportedException ();
		}

		public MethodInfo GetArrayMethod (params object[] args)
		{
			throw new NotSupportedException ();
		}

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}
	}

	public class PropertyBuilder : PropertyInfo
	{
		#region implemented abstract members of MemberInfo

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override Type DeclaringType {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string Name {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type ReflectedType {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		#region implemented abstract members of PropertyInfo

		public override MethodInfo[] GetAccessors (bool nonPublic)
		{
			throw new NotSupportedException ();
		}

		public override MethodInfo GetGetMethod (bool nonPublic)
		{
			throw new NotSupportedException ();
		}

		public override ParameterInfo[] GetIndexParameters ()
		{
			throw new NotSupportedException ();
		}

		public override MethodInfo GetSetMethod (bool nonPublic)
		{
			throw new NotSupportedException ();
		}

		public override object GetValue (object obj, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException ();
		}

		public override void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException ();
		}

		public override PropertyAttributes Attributes {
			get {
				throw new NotSupportedException ();
			}
		}

		public override bool CanRead {
			get {
				throw new NotSupportedException ();
			}
		}

		public override bool CanWrite {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type PropertyType {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		public MethodToken GetToken()
		{
			throw new NotSupportedException ();
		}

		public void SetGetMethod (object arg)
		{
			throw new NotSupportedException ();
		}

		public void SetSetMethod (object arg)
		{
			throw new NotSupportedException ();
		}

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}
	}

	public class FieldBuilder : FieldInfo
	{
		#region implemented abstract members of MemberInfo

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override Type DeclaringType {
			get {
				throw new NotSupportedException ();
			}
		}

		public override string Name {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type ReflectedType {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		#region implemented abstract members of FieldInfo

		public override object GetValue (object obj)
		{
			throw new NotSupportedException ();
		}

		public override void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException ();
		}

		public override FieldAttributes Attributes {
			get {
				throw new NotSupportedException ();
			}
		}

		public override RuntimeFieldHandle FieldHandle {
			get {
				throw new NotSupportedException ();
			}
		}

		public override Type FieldType {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		public void SetConstant (object arg)
		{
			throw new NotSupportedException ();
		}

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}
	}

	public class ParameterBuilder : ParameterInfo
	{
		public void SetConstant (object arg)
		{
			throw new NotSupportedException ();
		}

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}
	}

	public class EventBuilder
	{
		public void SetAddOnMethod (MethodBuilder mdBuilder)
		{
			throw new NotSupportedException ();
		}

		public void SetRemoveOnMethod (MethodBuilder mdBuilder)
		{
			throw new NotSupportedException ();
		}

		public void SetCustomAttribute (params object[] args)
		{
			throw new NotSupportedException ();
		}
	}

	public class CustomAttributeBuilder
	{
		public CustomAttributeBuilder (params object[] args)
		{
			throw new NotSupportedException ();
		}
	}
}