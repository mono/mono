//
// DerivedType.cs - NUnit Test Cases for types derived from TypeBuilder
//
// Rodrigo Kumpera <rkumpera@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using NUnit.Framework;
using System.Runtime.CompilerServices;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace MonoTests.System.Reflection.Emit
{
#if NET_2_0
	[TestFixture]
	public class PointerTypeTest
	{
		AssemblyBuilder assembly;
		ModuleBuilder module;
		int typeCount;
		static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.TypeBuilderTest";

		string MakeName ()
		{
			return "internal__type"+ typeCount++;
		}

		[SetUp]
		protected void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;

			assembly =
				Thread.GetDomain ().DefineDynamicAssembly (
					assemblyName, AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());

			module = assembly.DefineDynamicModule ("module1");
			typeCount = 0;
		}

		[Test]
		public void PropertiesValue ()
		{
			TypeBuilder tb = module.DefineType ("ns.type", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();

			Assert.AreEqual (assembly, ptr.Assembly, "#1");
			Assert.AreEqual ("ns.type*, " + assembly.FullName, ptr.AssemblyQualifiedName, "#2");
			Assert.AreEqual ("ns.type*", ptr.FullName, "#4");
			Assert.AreEqual (module, ptr.Module, "#5");
			Assert.AreEqual ("ns", ptr.Namespace, "#6");
			Assert.AreEqual (ptr, ptr.UnderlyingSystemType, "#7");
			Assert.AreEqual ("type*", ptr.Name, "#8");

			try {
				object x = ptr.GUID;
				Assert.Fail ("#9");
			} catch (NotSupportedException) {}

			try {
				object x = ptr.TypeHandle;
				Assert.Fail ("#10");
			} catch (NotSupportedException) {}
		}	

		[Test]
		public void Methods ()
		{
			TypeBuilder tb = module.DefineType ("ns.type", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();

			try {
				ptr.GetInterface ("foo", true);
				Assert.Fail ("#1");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetInterfaces ();
				Assert.Fail ("#2");
			} catch (NotSupportedException) {

			}
	
			Assert.AreEqual (tb, ptr.GetElementType ());
	
			try {
				ptr.GetEvent ("foo", BindingFlags.Public);
				Assert.Fail ("#4");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetEvents (BindingFlags.Public);
				Assert.Fail ("#5");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetField ("foo", BindingFlags.Public);
				Assert.Fail ("#6");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetFields (BindingFlags.Public);
				Assert.Fail ("#7");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetMembers (BindingFlags.Public);
				Assert.Fail ("#8");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetMethod ("Sort");
				Assert.Fail ("#9");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetMethods (BindingFlags.Public);
				Assert.Fail ("#9");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetNestedType ("bla", BindingFlags.Public);
				Assert.Fail ("#10");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetNestedTypes (BindingFlags.Public);
				Assert.Fail ("#11");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetProperties (BindingFlags.Public);
				Assert.Fail ("#12");
			} catch (NotSupportedException) {

			}	
	
			try {
				ptr.GetProperty ("Length");
				Assert.Fail ("#13");
			} catch (NotSupportedException) {

			}
	
			try {
				ptr.GetConstructor (new Type[] { typeof (int) });
				Assert.Fail ("#14");
			} catch (NotSupportedException) {

			}
	
			TypeAttributes attr = TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Public;
			Assert.AreEqual (attr, ptr.Attributes, "#15");

			Assert.IsTrue (ptr.HasElementType, "#16");
			Assert.IsFalse (ptr.IsArray, "#17");
			Assert.IsFalse (ptr.IsByRef, "#18");
			Assert.IsFalse (ptr.IsCOMObject, "#19");
			Assert.IsTrue (ptr.IsPointer, "#20");
			Assert.IsFalse (ptr.IsPrimitive, "#21");

			try {
				ptr.GetConstructors (BindingFlags.Public);
				Assert.Fail ("#22");
			} catch (NotSupportedException) {

			}

			try {
				ptr.InvokeMember ("GetLength", BindingFlags.Public, null, null, null);
				Assert.Fail ("#23");
			} catch (NotSupportedException) {

			}

			try {
				ptr.GetArrayRank ();
				Assert.Fail ("#23");
			} catch (NotSupportedException) {

			}
		}

		[Test]
		public void GenericTypeMembers ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();

			try {
				arr.GetGenericArguments ();
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}

			try {
				arr.GetGenericParameterConstraints ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {}

			try {
				arr.GetGenericTypeDefinition ();
				Assert.Fail ("#3");
			} catch (NotSupportedException) {}
		
			Assert.IsFalse (arr.ContainsGenericParameters, "#4");
			try {
				var x = arr.GenericParameterAttributes;
				Assert.Fail ("#5");
			} catch (NotSupportedException) {}

			try {
				var x = arr.GenericParameterPosition;
				Assert.Fail ("#6");
			} catch (InvalidOperationException) {}

			Assert.IsFalse (arr.ContainsGenericParameters, "#7");

			Assert.IsFalse (arr.IsGenericParameter, "#8");
			Assert.IsFalse (arr.IsGenericType, "#9");
			Assert.IsFalse (arr.IsGenericTypeDefinition, "#10");
		}

		[Test]
		public void AttributeValues ()
		{
				TypeBuilder tb = module.DefineType (MakeName (), TypeAttributes.NotPublic);
				Assert.AreEqual (TypeAttributes.NotPublic, tb.Attributes, "#1");

				tb = module.DefineType (MakeName (), TypeAttributes.Public);
				Assert.AreEqual (TypeAttributes.Public, tb.Attributes, "#2");

				tb = module.DefineType (MakeName (), TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.Sealed);
				Assert.AreEqual (TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.Sealed, tb.Attributes, "#3");

				tb = module.DefineType (MakeName (), TypeAttributes.Public | TypeAttributes.Abstract);
				Assert.AreEqual (TypeAttributes.Public | TypeAttributes.Abstract, tb.Attributes, "$4");
		}

		[Test]
		public void AsParamType ()
		{

			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
	
			MethodBuilder mb = tb.DefineMethod ("Test", MethodAttributes.Public, typeof (void), new Type [1] { ptr });
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);
	
			Type res = tb.CreateType ();	
	
			object o = Activator.CreateInstance (res);
			//FIXME this crashes the runtime
			//res.GetMethod ("Test").Invoke (o, new object[1] { null });
		}

		[Test]
		public void AsLocalVariable ()
		{

			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
	
			MethodBuilder mb = tb.DefineMethod ("Test", MethodAttributes.Public, typeof (void), new Type [0]);
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.DeclareLocal (ptr);
			ilgen.Emit (OpCodes.Ret);
	
			Type res = tb.CreateType ();	
	
			object o = Activator.CreateInstance (res);
			res.GetMethod ("Test").Invoke (o, null);
		}

		[Test]
		public void TestEquals ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Type ptr2 = tb.MakePointerType ();
			Assert.IsTrue (ptr.Equals (ptr), "#2");
		}

		[Test]
		[Category ("NotWorking")] //two stage type creation makes this fail
		public void TestEquals2 ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Type ptr2 = tb.MakePointerType ();
			Assert.IsFalse (ptr.Equals (ptr2), "#1");
		}

		[Test]
		public void IsSubclassOf ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Assert.IsFalse (ptr.IsSubclassOf (tb), "#1");
			Assert.IsFalse (ptr.IsSubclassOf (typeof (object[])), "#2");
		}

		[Test]
		public void IsAssignableFrom ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Assert.IsFalse (ptr.IsAssignableFrom (tb), "#1");
			Assert.IsFalse (ptr.IsAssignableFrom (typeof (object[])), "#2");
		}

		[Test]
		[Category ("NotWorking")] //two stage type creation makes this fail
		public void IsAssignableFrom2 ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Assert.IsFalse (typeof (object[]).IsAssignableFrom (ptr), "#1");
			Assert.IsFalse (typeof (object).IsAssignableFrom (ptr), "#2");
		}


		[Test]
		public void GetInterfaceMap ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			try {
				ptr.GetInterfaceMap (typeof (IEnumerable));
				Assert.Fail ("#1");
			} catch (NotSupportedException) {

			}
		}

		[Test]
		public void IsInstanceOfType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Assert.IsFalse (ptr.IsInstanceOfType (tb), "#1");
			Assert.IsFalse (ptr.IsInstanceOfType (null), "#2");
			Assert.IsFalse (ptr.IsInstanceOfType (new object [1]), "#3");

			Type t = tb.CreateType ();
			object obj = Activator.CreateInstance (t);
			Assert.IsFalse (ptr.IsInstanceOfType (obj), "#4");
		}

		[Test]
		public void IsGenericTypeDefinition ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Assert.IsFalse (ptr.IsGenericTypeDefinition, "#1");
		}

		[Test]
		public void IsGenericType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Assert.IsFalse (ptr.IsGenericType, "#1");
		}

		[Test]
		public void MakeGenericType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			try {
				ptr.MakeGenericType (new Type[] { typeof (string) });
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void GenericParameterPosition ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			try {
				int pos = ptr.GenericParameterPosition;
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void GenericParameterAttributes ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			try {
				object attr = ptr.GenericParameterAttributes;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void GetGenericParameterConstraints ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			try {
				ptr.GetGenericParameterConstraints ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void MakeArrayType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Type res = ptr.MakeArrayType ();
			Assert.IsNotNull (res, "#1");
			Assert.IsTrue (res.IsArray, "#2");

			res = ptr.MakeArrayType (2);
			Assert.IsNotNull (res, "#3");
			Assert.IsTrue (res.IsArray, "#4");
		}

		[Test]
		public void MakeByRefType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Type res = ptr.MakeByRefType ();

			Assert.IsNotNull (res, "#1");
			Assert.IsTrue (res.IsByRef, "#2");
		}

		[Test]
		public void MakePointerType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			Type res = ptr.MakePointerType ();

			Assert.IsNotNull (res, "#1");
			Assert.IsTrue (res.IsPointer, "#2");
		}

		[Test]
		public void StructLayoutAttribute ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type ptr = tb.MakePointerType ();
			try {
				object x = ptr.StructLayoutAttribute;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void ByRefOfGenericTypeParameter ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("F")[0];
			Type ptr = gparam.MakePointerType ();

			try {
				ptr.GetGenericArguments ();
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}

			try {
				ptr.GetGenericParameterConstraints ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {}

			try {
				ptr.GetGenericTypeDefinition ();
				Assert.Fail ("#3");
			} catch (NotSupportedException) {}
		
			Assert.IsTrue (ptr.ContainsGenericParameters, "#4");
			try {
				var x = ptr.GenericParameterAttributes;
				Assert.Fail ("#5");
			} catch (NotSupportedException) {}

			try {
				var x = ptr.GenericParameterPosition;
				Assert.Fail ("#6");
			} catch (InvalidOperationException) {}


			Assert.IsFalse (ptr.IsGenericParameter, "#8");
			Assert.IsFalse (ptr.IsGenericType, "#9");
			Assert.IsFalse (ptr.IsGenericTypeDefinition, "#10");

#if NET_4_0
			Assert.AreEqual (TypeAttributes.Public, ptr.Attributes, "#11");
#else
			try {
				var x = ptr.Attributes; //This is because GenericTypeParameterBuilder doesn't support Attributes 
				Assert.Fail ("#11");
			} catch (NotSupportedException) {}
#endif

			Assert.IsTrue (ptr.HasElementType, "#12");
			Assert.IsTrue (ptr.IsPointer, "#13");

			Assert.AreEqual (assembly, ptr.Assembly, "#14");
			Assert.AreEqual (null, ptr.AssemblyQualifiedName, "#15");
			//XXX LAMEIMPL this passes on MS even thou it's pretty much very wrong. 
			Assert.AreEqual (typeof (Array), ptr.BaseType, "#16");
			Assert.AreEqual (null, ptr.FullName, "#17");
			Assert.AreEqual (module, ptr.Module, "#18");
			Assert.AreEqual (null, ptr.Namespace, "#19");
			Assert.AreEqual (ptr, ptr.UnderlyingSystemType, "#20");
			Assert.AreEqual ("F*", ptr.Name, "#21");

			Assert.AreEqual (gparam, ptr.GetElementType (), "#22");
		}
	}


	[TestFixture]
	public class ByrefTypeTest
	{
		AssemblyBuilder assembly;
		ModuleBuilder module;
		int typeCount;
		static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.TypeBuilderTest";

		string MakeName ()
		{
			return "internal__type"+ typeCount++;
		}

		[SetUp]
		protected void SetUp ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave);
		}

		protected void SetUp (AssemblyBuilderAccess mode)
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;

			assembly =
				Thread.GetDomain ().DefineDynamicAssembly (
					assemblyName, mode, Path.GetTempPath ());

			module = assembly.DefineDynamicModule ("module1");
			typeCount = 0;
		}

		[Test]
		public void PropertiesValue ()
		{
			TypeBuilder tb = module.DefineType ("ns.type", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();

			Assert.AreEqual (assembly, byref.Assembly, "#1");
			Assert.AreEqual ("ns.type&, " + assembly.FullName, byref.AssemblyQualifiedName, "#2");
			Assert.AreEqual ("ns.type&", byref.FullName, "#4");
			Assert.AreEqual (module, byref.Module, "#5");
			Assert.AreEqual ("ns", byref.Namespace, "#6");
			Assert.AreEqual (byref, byref.UnderlyingSystemType, "#7");
			Assert.AreEqual ("type&", byref.Name, "#8");

			try {
				object x = byref.GUID;
				Assert.Fail ("#9");
			} catch (NotSupportedException) {}

			try {
				object x = byref.TypeHandle;
				Assert.Fail ("#10");
			} catch (NotSupportedException) {}
		}	

		[Test]
		public void Methods ()
		{
			TypeBuilder tb = module.DefineType ("ns.type", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();

			try {
				byref.GetInterface ("foo", true);
				Assert.Fail ("#1");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetInterfaces ();
				Assert.Fail ("#2");
			} catch (NotSupportedException) {

			}
	
			Assert.AreEqual (tb, byref.GetElementType ());
	
			try {
				byref.GetEvent ("foo", BindingFlags.Public);
				Assert.Fail ("#4");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetEvents (BindingFlags.Public);
				Assert.Fail ("#5");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetField ("foo", BindingFlags.Public);
				Assert.Fail ("#6");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetFields (BindingFlags.Public);
				Assert.Fail ("#7");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetMembers (BindingFlags.Public);
				Assert.Fail ("#8");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetMethod ("Sort");
				Assert.Fail ("#9");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetMethods (BindingFlags.Public);
				Assert.Fail ("#9");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetNestedType ("bla", BindingFlags.Public);
				Assert.Fail ("#10");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetNestedTypes (BindingFlags.Public);
				Assert.Fail ("#11");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetProperties (BindingFlags.Public);
				Assert.Fail ("#12");
			} catch (NotSupportedException) {

			}	
	
			try {
				byref.GetProperty ("Length");
				Assert.Fail ("#13");
			} catch (NotSupportedException) {

			}
	
			try {
				byref.GetConstructor (new Type[] { typeof (int) });
				Assert.Fail ("#14");
			} catch (NotSupportedException) {

			}
	
			TypeAttributes attr = TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Public;
			Assert.AreEqual (attr, byref.Attributes, "#15");

			Assert.IsTrue (byref.HasElementType, "#16");
			Assert.IsFalse (byref.IsArray, "#17");
			Assert.IsTrue (byref.IsByRef, "#18");
			Assert.IsFalse (byref.IsCOMObject, "#19");
			Assert.IsFalse (byref.IsPointer, "#20");
			Assert.IsFalse (byref.IsPrimitive, "#21");

			try {
				byref.GetConstructors (BindingFlags.Public);
				Assert.Fail ("#22");
			} catch (NotSupportedException) {

			}

			try {
				byref.InvokeMember ("GetLength", BindingFlags.Public, null, null, null);
				Assert.Fail ("#23");
			} catch (NotSupportedException) {

			}

			try {
				byref.GetArrayRank ();
				Assert.Fail ("#23");
			} catch (NotSupportedException) {

			}
		}

		[Test]
		public void AttributeValues ()
		{
				TypeBuilder tb = module.DefineType (MakeName (), TypeAttributes.NotPublic);
				Assert.AreEqual (TypeAttributes.NotPublic, tb.Attributes, "#1");

				tb = module.DefineType (MakeName (), TypeAttributes.Public);
				Assert.AreEqual (TypeAttributes.Public, tb.Attributes, "#2");

				tb = module.DefineType (MakeName (), TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.Sealed);
				Assert.AreEqual (TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.Sealed, tb.Attributes, "#3");

				tb = module.DefineType (MakeName (), TypeAttributes.Public | TypeAttributes.Abstract);
				Assert.AreEqual (TypeAttributes.Public | TypeAttributes.Abstract, tb.Attributes, "$4");
		}

		[Test]
		public void AsParamType ()
		{

			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
	
			MethodBuilder mb = tb.DefineMethod ("Test", MethodAttributes.Public, typeof (void), new Type [1] { byref });
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);
	
			Type res = tb.CreateType ();	
	
			object o = Activator.CreateInstance (res);
			res.GetMethod ("Test").Invoke (o, new object[1] { null });
		}

		[Test]
		public void AsLocalVariable ()
		{

			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
	
			MethodBuilder mb = tb.DefineMethod ("Test", MethodAttributes.Public, typeof (void), new Type [0]);
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.DeclareLocal (byref);
			ilgen.Emit (OpCodes.Ret);
	
			Type res = tb.CreateType ();	
	
			object o = Activator.CreateInstance (res);
			res.GetMethod ("Test").Invoke (o, null);
		}

		[Test]
		public void TestEquals ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			Type byref2 = tb.MakeByRefType ();
			Assert.IsTrue (byref.Equals (byref), "#1");
		}

		[Test]
		[Category ("NotWorking")] //two stage type creation makes this fail
		public void TestEquals2 ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			Type byref2 = tb.MakeByRefType ();
			Assert.IsFalse (byref.Equals (byref2), "#1");
		}

		[Test]
		public void IsSubclassOf ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			Assert.IsFalse (byref.IsSubclassOf (tb), "#1");
			Assert.IsFalse (byref.IsSubclassOf (typeof (object[])), "#2");
		}

		[Test]
		public void IsAssignableFrom ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			Assert.IsFalse (byref.IsAssignableFrom (tb), "#1");
			Assert.IsFalse (byref.IsAssignableFrom (typeof (object[])), "#2");
		}

		[Test]
		[Category ("NotWorking")] //two stage type creation makes this fail
		public void IsAssignableFrom2 ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			Assert.IsFalse (typeof (object[]).IsAssignableFrom (byref), "#1");
			Assert.IsFalse (typeof (object).IsAssignableFrom (byref), "#2");
		}

		[Test]
		public void GetInterfaceMap ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			try {
				byref.GetInterfaceMap (typeof (IEnumerable));
				Assert.Fail ("#1");
			} catch (NotSupportedException) {

			}
		}

		[Test]
		public void IsInstanceOfType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			Assert.IsFalse (byref.IsInstanceOfType (tb), "#1");
			Assert.IsFalse (byref.IsInstanceOfType (null), "#2");
			Assert.IsFalse (byref.IsInstanceOfType (new object [1]), "#3");

			Type t = tb.CreateType ();
			object obj = Activator.CreateInstance (t);
			Assert.IsFalse (byref.IsInstanceOfType (obj), "#4");
		}

		[Test]
		public void IsGenericTypeDefinition ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			Assert.IsFalse (byref.IsGenericTypeDefinition, "#1");
		}

		[Test]
		public void IsGenericType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			Assert.IsFalse (byref.IsGenericType, "#1");
		}

		[Test]
		public void MakeGenericType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			try {
				byref.MakeGenericType (new Type[] { typeof (string) });
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void GenericParameterPosition ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			try {
				int pos = byref.GenericParameterPosition;
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void GenericParameterAttributes ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			try {
				object attr = byref.GenericParameterAttributes;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void GetGenericParameterConstraints ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			try {
				byref.GetGenericParameterConstraints ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void MakeArrayType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			try {
				byref.MakeArrayType ();
				Assert.Fail ("#1");
			} catch (ArgumentException) {};
			try {
				byref.MakeArrayType (2);
				Assert.Fail ("#2");
			} catch (ArgumentException) {};
		}

		[Test]
		public void MakeByRefType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			try {
				byref.MakeByRefType ();
				Assert.Fail ("#1");
			} catch (ArgumentException) {}
		}

		[Test]
		public void MakePointerType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			try {
				byref.MakePointerType ();
				Assert.Fail ("#1");
			} catch (ArgumentException) {}
		}

		[Test]
		public void StructLayoutAttribute ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type byref = tb.MakeByRefType ();
			try {
				object x = byref.StructLayoutAttribute;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		[Category ("NotDotNet")]
		// CompilerContext no longer supported
		[Category ("NotWorking")]
		public void ByRefOfAttriburesUnderCompilerContext ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);

			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("F")[0];
			Type byref = gparam.MakeByRefType ();

			tb = module.DefineType (MakeName (), TypeAttributes.Public);
			Assert.AreEqual (TypeAttributes.Public , byref.Attributes, "#1");

		}

		[Test]
		public void ByRefOfGenericTypeParameter ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("F")[0];
			Type byref = gparam.MakeByRefType ();

			try {
				byref.GetGenericArguments ();
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}

			try {
				byref.GetGenericParameterConstraints ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {}

			try {
				byref.GetGenericTypeDefinition ();
				Assert.Fail ("#3");
			} catch (NotSupportedException) {}
		
			Assert.IsTrue (byref.ContainsGenericParameters, "#4");
			try {
				var x = byref.GenericParameterAttributes;
				Assert.Fail ("#5");
			} catch (NotSupportedException) {}

			try {
				var x = byref.GenericParameterPosition;
				Assert.Fail ("#6");
			} catch (InvalidOperationException) {}


			Assert.IsFalse (byref.IsGenericParameter, "#8");
			Assert.IsFalse (byref.IsGenericType, "#9");
			Assert.IsFalse (byref.IsGenericTypeDefinition, "#10");


#if NET_4_0
			Assert.AreEqual (TypeAttributes.Public, byref.Attributes, "#11");
#else
			try {
				var x = byref.Attributes; //This is because GenericTypeParameterBuilder doesn't support Attributes 
				Assert.Fail ("#11");
			} catch (NotSupportedException) {}
#endif

			Assert.IsTrue (byref.HasElementType, "#12");
			Assert.IsTrue (byref.IsByRef, "#13");

			Assert.AreEqual (assembly, byref.Assembly, "#14");
			Assert.AreEqual (null, byref.AssemblyQualifiedName, "#15");
			//XXX LAMEIMPL this passes on MS even thou it's pretty much very wrong. 
			Assert.AreEqual (typeof (Array), byref.BaseType, "#16");
			Assert.AreEqual (null, byref.FullName, "#17");
			Assert.AreEqual (module, byref.Module, "#18");
			Assert.AreEqual (null, byref.Namespace, "#19");
			Assert.AreEqual (byref, byref.UnderlyingSystemType, "#20");
			Assert.AreEqual ("F&", byref.Name, "#21");

			Assert.AreEqual (gparam, byref.GetElementType (), "#22");
		}
	}

	[TestFixture]
	public class ArrayTypeTest
	{
		AssemblyBuilder assembly;
		ModuleBuilder module;
		int typeCount;
		static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.TypeBuilderTest";

		string MakeName ()
		{
			return "internal__type"+ typeCount++;
		}


		[SetUp]
		protected void SetUp ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave);
		}

		protected void SetUp (AssemblyBuilderAccess mode)
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;

			assembly =
				Thread.GetDomain ().DefineDynamicAssembly (
					assemblyName, mode, Path.GetTempPath ());

			module = assembly.DefineDynamicModule ("module1");
			typeCount = 0;
		}

		[Test]
		public void OneDimMultiDimentionArray ()
		{
			TypeBuilder tb = module.DefineType ("ns.type", TypeAttributes.Public);

			Type arr1 = tb.MakeArrayType ();
			Type arr2 = tb.MakeArrayType (1);
			Type arr3 = arr1.MakeArrayType (1);

			Assert.AreEqual ("type[]", arr1.Name, "#1");
			Assert.AreEqual ("type[*]", arr2.Name, "#2");
			Assert.AreEqual ("type[][*]", arr3.Name, "#3");

			var gparam = tb.DefineGenericParameters ("F")[0];
			Type arr4 = gparam.MakeArrayType ();
			Type arr5 = gparam.MakeArrayType (1);

			Assert.AreEqual ("F[]", arr4.Name, "#4");
			Assert.AreEqual ("F[*]", arr5.Name, "#5");

			var eb = module.DefineEnum ("enum", TypeAttributes.Public, tb);
			Type arr6 = eb.MakeArrayType ();
			Type arr7 = eb.MakeArrayType (1);

			Assert.AreEqual ("enum[]", arr6.Name, "#6");
			Assert.AreEqual ("enum[*]", arr7.Name, "#7");
		}

		[Test]
		public void PropertiesValue ()
		{
			TypeBuilder tb = module.DefineType ("ns.type", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();

			Assert.AreEqual (assembly, arr.Assembly, "#1");
			Assert.AreEqual ("ns.type[], " + assembly.FullName, arr.AssemblyQualifiedName, "#2");
			Assert.AreEqual (typeof (Array), arr.BaseType, "#3");
			Assert.AreEqual ("ns.type[]", arr.FullName, "#4");
			Assert.AreEqual (module, arr.Module, "#5");
			Assert.AreEqual ("ns", arr.Namespace, "#6");
			Assert.AreEqual (arr, arr.UnderlyingSystemType, "#7");
			Assert.AreEqual ("type[]", arr.Name, "#8");

			try {
				object x = arr.GUID;
				Assert.Fail ("#9");
			} catch (NotSupportedException) {}

			try {
				object x = arr.TypeHandle;
				Assert.Fail ("#10");
			} catch (NotSupportedException) {}
		}	

		[Test]
		public void Methods ()
		{
			TypeBuilder tb = module.DefineType ("ns.type", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();

			try {
				arr.GetInterface ("foo", true);
				Assert.Fail ("#1");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetInterfaces ();
				Assert.Fail ("#2");
			} catch (NotSupportedException) {

			}
	
			Assert.AreEqual (tb, arr.GetElementType ());
	
			try {
				arr.GetEvent ("foo", BindingFlags.Public);
				Assert.Fail ("#4");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetEvents (BindingFlags.Public);
				Assert.Fail ("#5");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetField ("foo", BindingFlags.Public);
				Assert.Fail ("#6");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetFields (BindingFlags.Public);
				Assert.Fail ("#7");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetMembers (BindingFlags.Public);
				Assert.Fail ("#8");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetMethod ("Sort");
				Assert.Fail ("#9");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetMethods (BindingFlags.Public);
				Assert.Fail ("#9");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetNestedType ("bla", BindingFlags.Public);
				Assert.Fail ("#10");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetNestedTypes (BindingFlags.Public);
				Assert.Fail ("#11");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetProperties (BindingFlags.Public);
				Assert.Fail ("#12");
			} catch (NotSupportedException) {

			}	
	
			try {
				arr.GetProperty ("Length");
				Assert.Fail ("#13");
			} catch (NotSupportedException) {

			}
	
			try {
				arr.GetConstructor (new Type[] { typeof (int) });
				Assert.Fail ("#14");
			} catch (NotSupportedException) {

			}
	
			TypeAttributes attr = TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Public;
			Assert.AreEqual (attr, arr.Attributes, "#15");

			Assert.IsTrue (arr.HasElementType, "#16");
			Assert.IsTrue (arr.IsArray, "#17");
			Assert.IsFalse (arr.IsByRef, "#18");
			Assert.IsFalse (arr.IsCOMObject, "#19");
			Assert.IsFalse (arr.IsPointer, "#20");
			Assert.IsFalse (arr.IsPrimitive, "#21");

			try {
				arr.GetConstructors (BindingFlags.Public);
				Assert.Fail ("#22");
			} catch (NotSupportedException) {

			}

			try {
				arr.InvokeMember ("GetLength", BindingFlags.Public, null, null, null);
				Assert.Fail ("#23");
			} catch (NotSupportedException) {

			}
		}

		[Test]
		public void AttributeValues ()
		{
			TypeBuilder tb = module.DefineType (MakeName (), TypeAttributes.NotPublic);
			Assert.AreEqual (TypeAttributes.NotPublic, tb.Attributes, "#1");

			tb = module.DefineType (MakeName (), TypeAttributes.Public);
			Assert.AreEqual (TypeAttributes.Public, tb.Attributes, "#2");

			tb = module.DefineType (MakeName (), TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.Sealed);
			Assert.AreEqual (TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.Sealed, tb.Attributes, "#3");

			tb = module.DefineType (MakeName (), TypeAttributes.Public | TypeAttributes.Abstract);
			Assert.AreEqual (TypeAttributes.Public | TypeAttributes.Abstract, tb.Attributes, "$4");
		}

		[Test]
		[Category ("NotDotNet")]
		// CompilerContext no longer supported
		[Category ("NotWorking")]
		public void AttributeValuesUnderCompilerContext ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);

			TypeAttributes arrayAttr = TypeAttributes.Sealed | TypeAttributes.Serializable;

			TypeBuilder tb = module.DefineType (MakeName (), TypeAttributes.NotPublic);
			Assert.AreEqual (TypeAttributes.NotPublic | arrayAttr, tb.MakeArrayType ().Attributes, "#1");

			tb = module.DefineType (MakeName (), TypeAttributes.Public);
			Assert.AreEqual (TypeAttributes.Public | arrayAttr, tb.MakeArrayType ().Attributes, "#2");

			tb = module.DefineType (MakeName (), TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.Sealed);
			Assert.AreEqual (TypeAttributes.Public | arrayAttr, tb.MakeArrayType ().Attributes, "#3");

			tb = module.DefineType (MakeName (), TypeAttributes.Public | TypeAttributes.Abstract);
			Assert.AreEqual (TypeAttributes.Public | arrayAttr, tb.MakeArrayType ().Attributes, "$4");
		}

		[Test]
		public void AsReturnType ()
		{

			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
	
			MethodBuilder mb = tb.DefineMethod ("Test", MethodAttributes.Public, arr, new Type [0]);
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
	
			Type res = tb.CreateType ();	
	
			object o = Activator.CreateInstance (res);
			res.GetMethod ("Test").Invoke (o, null);
		}

		[Test]
		public void AsParamType ()
		{

			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
	
			MethodBuilder mb = tb.DefineMethod ("Test", MethodAttributes.Public, typeof (void), new Type [1] { arr });
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);
	
			Type res = tb.CreateType ();	
	
			object o = Activator.CreateInstance (res);
			res.GetMethod ("Test").Invoke (o, new object[1] { null });
		}

		[Test]
		public void AsLocalVariable ()
		{

			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
	
			MethodBuilder mb = tb.DefineMethod ("Test", MethodAttributes.Public, arr, new Type [0]);
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.DeclareLocal (arr);
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Stloc_0);
			ilgen.Emit (OpCodes.Ldloc_0);
			ilgen.Emit (OpCodes.Ret);
	
			Type res = tb.CreateType ();	
	
			object o = Activator.CreateInstance (res);
			res.GetMethod ("Test").Invoke (o, null);
		}

		[Test]
		public void TestEquals ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Type arr2 = tb.MakeArrayType ();
			Assert.IsTrue (arr.Equals (arr), "#1");
		}

		[Test]
		[Category ("NotWorking")] //two stage type creation makes this fail
		public void TestEquals2 ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Type arr2 = tb.MakeArrayType ();
			Assert.IsFalse (arr.Equals (arr2), "#1");
		}

		[Test]
		public void IsSubclassOf ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Assert.IsFalse (arr.IsSubclassOf (tb), "#1");
			Assert.IsFalse (arr.IsSubclassOf (typeof (object[])), "#2");
		}

		[Test]
		public void IsAssignableFrom ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Assert.IsFalse (arr.IsAssignableFrom (tb), "#1");
			Assert.IsFalse (arr.IsAssignableFrom (typeof (object[])), "#2");
		}

		[Test]
		[Category ("NotWorking")] //two stage type creation makes this fail
		public void IsAssignableFrom2 ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Assert.IsFalse (typeof (object[]).IsAssignableFrom (arr), "#1");
			Assert.IsFalse (typeof (object).IsAssignableFrom (arr), "#2");
		}

		[Test]
		public void GetInterfaceMap ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			try {
				arr.GetInterfaceMap (typeof (IEnumerable));
				Assert.Fail ("#1");
			} catch (NotSupportedException) {

			}
		}

		[Test]
		public void IsInstanceOfType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Assert.IsFalse (arr.IsInstanceOfType (tb), "#1");
			Assert.IsFalse (arr.IsInstanceOfType (null), "#2");
			Assert.IsFalse (arr.IsInstanceOfType (new object [1]), "#3");

			Type t = tb.CreateType ();
			object obj = Array.CreateInstance (t, 10);
			Assert.IsFalse (arr.IsInstanceOfType (obj), "#4");
		}

		[Test]
		public void IsGenericTypeDefinition ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Assert.IsFalse (arr.IsGenericTypeDefinition, "#1");
		}

		[Test]
		public void IsGenericType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Assert.IsFalse (arr.IsGenericType, "#1");
		}

		[Test]
		public void MakeGenericType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			try {
				arr.MakeGenericType (new Type[] { typeof (string) });
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void GenericParameterPosition ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			try {
				int pos = arr.GenericParameterPosition;
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void GenericParameterAttributes ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			try {
				object attr = arr.GenericParameterAttributes;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void GetGenericParameterConstraints ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			try {
				arr.GetGenericParameterConstraints ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void MakeArrayType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Type res = arr.MakeArrayType ();
			Assert.IsNotNull (res, "#1");
			Assert.IsTrue (res.IsArray, "#2");

			res = arr.MakeArrayType (2);
			Assert.IsNotNull (res, "#3");
			Assert.IsTrue (res.IsArray, "#4");
		}

		[Test]
		public void MakeByRefType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Type res = arr.MakeByRefType ();

			Assert.IsNotNull (res, "#1");
			Assert.IsTrue (res.IsByRef, "#2");
		}

		[Test]
		public void MakePointerType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			Type res = arr.MakePointerType ();

			Assert.IsNotNull (res, "#1");
			Assert.IsTrue (res.IsPointer, "#2");
		}

		[Test]
		public void StructLayoutAttribute ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();
			try {
				object x = arr.StructLayoutAttribute;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void GetArrayRank ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);

			Assert.AreEqual (1, tb.MakeArrayType ().GetArrayRank (), "#1");
			Assert.AreEqual (2, tb.MakeArrayType (2).GetArrayRank (), "#2");
		}

		[Test]
		public void GenericTypeMembers ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			Type arr = tb.MakeArrayType ();

			try {
				arr.GetGenericArguments ();
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}

			try {
				arr.GetGenericParameterConstraints ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {}

			try {
				arr.GetGenericTypeDefinition ();
				Assert.Fail ("#3");
			} catch (NotSupportedException) {}
		
			Assert.IsFalse (arr.ContainsGenericParameters, "#4");
			try {
				var x = arr.GenericParameterAttributes;
				Assert.Fail ("#5");
			} catch (NotSupportedException) {}

			try {
				var x = arr.GenericParameterPosition;
				Assert.Fail ("#6");
			} catch (InvalidOperationException) {}

			Assert.IsFalse (arr.ContainsGenericParameters, "#7");

			Assert.IsFalse (arr.IsGenericParameter, "#8");
			Assert.IsFalse (arr.IsGenericType, "#9");
			Assert.IsFalse (arr.IsGenericTypeDefinition, "#10");
		}

		[Test]
		public void ArrayAsGenericArgumentOfNonSreType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);

			Type arr = tb.MakeArrayType ();
			Type inst = typeof (Foo<>).MakeGenericType (arr);
			
			MethodBuilder mb = tb.DefineMethod ("Main", MethodAttributes.Public | MethodAttributes.Static, typeof (object), Type.EmptyTypes);
	
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldtoken, inst);
			ilgen.Emit (OpCodes.Call, typeof (Type).GetMethod ("GetTypeFromHandle"));
			ilgen.Emit (OpCodes.Ret);
	
			Type res = tb.CreateType ();
			Type expected = typeof (Foo<>).MakeGenericType (res.MakeArrayType ());
	
			Assert.AreEqual (expected, res.GetMethod ("Main").Invoke (null, null), "#1");
			Assert.IsNotNull (Activator.CreateInstance (expected), "#2");
		}


		[Test]
		public void GenericTypeMembersOfGenericTypeParam ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("F")[0];
			Type arr = gparam.MakeArrayType ();

			try {
				arr.GetGenericArguments ();
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}

			try {
				arr.GetGenericParameterConstraints ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {}

			try {
				arr.GetGenericTypeDefinition ();
				Assert.Fail ("#3");
			} catch (NotSupportedException) {}
		
			Assert.IsTrue (arr.ContainsGenericParameters, "#4");
			try {
				var x = arr.GenericParameterAttributes;
				Assert.Fail ("#5");
			} catch (NotSupportedException) {}

			try {
				var x = arr.GenericParameterPosition;
				Assert.Fail ("#6");
			} catch (InvalidOperationException) {}


			Assert.IsFalse (arr.IsGenericParameter, "#8");
			Assert.IsFalse (arr.IsGenericType, "#9");
			Assert.IsFalse (arr.IsGenericTypeDefinition, "#10");

#if NET_4_0
			Assert.AreEqual (TypeAttributes.Public, arr.Attributes, "#11");
#else
			try {
				var x = arr.Attributes; //This is because GenericTypeParameterBuilder doesn't support Attributes 
				Assert.Fail ("#11");
			} catch (NotSupportedException) {}
#endif

			Assert.IsTrue (arr.HasElementType, "#12");
			Assert.IsTrue (arr.IsArray, "#13");

			Assert.AreEqual (assembly, arr.Assembly, "#14");
			Assert.AreEqual (null, arr.AssemblyQualifiedName, "#15");
			Assert.AreEqual (typeof (Array), arr.BaseType, "#16");
			Assert.AreEqual (null, arr.FullName, "#17");
			Assert.AreEqual (module, arr.Module, "#18");
			Assert.AreEqual (null, arr.Namespace, "#19");
			Assert.AreEqual (arr, arr.UnderlyingSystemType, "#20");
			Assert.AreEqual ("F[]", arr.Name, "#21");

			Assert.AreEqual (gparam, arr.GetElementType (), "#22");
		}

		[Test]
		public void GenericTypeMembersOfEnum ()
		{
			var eb = module.DefineEnum ("dd.enum", TypeAttributes.Public, typeof (int));
			Type arr = eb.MakeArrayType ();

			try {
				arr.GetGenericArguments ();
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}

			try {
				arr.GetGenericParameterConstraints ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {}

			try {
				arr.GetGenericTypeDefinition ();
				Assert.Fail ("#3");
			} catch (NotSupportedException) {}
		
			Assert.IsFalse (arr.ContainsGenericParameters, "#4");
			try {
				var x = arr.GenericParameterAttributes;
				Assert.Fail ("#5");
			} catch (NotSupportedException) {}

			try {
				var x = arr.GenericParameterPosition;
				Assert.Fail ("#6");
			} catch (InvalidOperationException) {}


			Assert.IsFalse (arr.IsGenericParameter, "#8");
			Assert.IsFalse (arr.IsGenericType, "#9");
			Assert.IsFalse (arr.IsGenericTypeDefinition, "#10");

			Assert.AreEqual (TypeAttributes.Public | TypeAttributes.Sealed, arr.Attributes, "#11");

			Assert.IsTrue (arr.HasElementType, "#12");
			Assert.IsTrue (arr.IsArray, "#13");

			Assert.AreEqual (assembly, arr.Assembly, "#14");
			Assert.AreEqual ("dd.enum[], MonoTests.System.Reflection.Emit.TypeBuilderTest, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", arr.AssemblyQualifiedName, "#15");
			Assert.AreEqual (typeof (Array), arr.BaseType, "#16");
			Assert.AreEqual ("dd.enum[]", arr.FullName, "#17");
			Assert.AreEqual (module, arr.Module, "#18");
			Assert.AreEqual ("dd", arr.Namespace, "#19");
			Assert.AreEqual (arr, arr.UnderlyingSystemType, "#20");
			Assert.AreEqual ("enum[]", arr.Name, "#21");

			Assert.AreEqual (eb, arr.GetElementType (), "#22");
		}

	}
#endif
}
