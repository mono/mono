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
			Assert.IsFalse (arr.Equals (arr2), "#1");
			Assert.IsTrue (arr.Equals (arr), "#2");
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
			Assert.IsFalse (typeof (object[]).IsAssignableFrom (arr), "#3");
			Assert.IsFalse (typeof (object).IsAssignableFrom (arr), "#4");
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
			Type arr = tb.MakeArrayType ();

			Assert.AreEqual (1, tb.MakeArrayType ().GetArrayRank (), "#1");
			Assert.AreEqual (2, tb.MakeArrayType (2).GetArrayRank (), "#2");
		}
	}
#endif
}
