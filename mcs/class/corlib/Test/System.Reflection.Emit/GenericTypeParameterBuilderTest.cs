//
// GenericTypeParameterBuilderTest.cs - NUnit Test Cases for GenericTypeParameterBuilder
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

using System.Collections.Generic;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class GenericTypeParameterBuilderTest
	{
		AssemblyBuilder assembly;
		ModuleBuilder module;
		int typeCount;
		static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.TypeBuilderTest";

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
			var gparam = tb.DefineGenericParameters ("A", "B")[1];

			Assert.AreEqual (assembly, gparam.Assembly, "#1");
			Assert.AreEqual (null, gparam.AssemblyQualifiedName, "#2");
			Assert.AreEqual (null, gparam.BaseType, "#3");
			Assert.AreEqual (null, gparam.FullName, "#4");
			Assert.AreEqual (module, gparam.Module, "#5");
			Assert.AreEqual (null, gparam.Namespace, "#6");
			Assert.AreEqual (gparam, gparam.UnderlyingSystemType, "#7");
			Assert.AreEqual ("B", gparam.Name, "#8");

			try {
				object x = gparam.GUID;
				Assert.Fail ("#9");
			} catch (NotSupportedException) {}

			try {
				object x = gparam.TypeHandle;
				Assert.Fail ("#10");
			} catch (NotSupportedException) {}

			try {
				object x = gparam.StructLayoutAttribute;
				Assert.Fail ("#11");
			} catch (NotSupportedException) {}

#if NET_4_0
			Assert.AreEqual (TypeAttributes.Public, gparam.Attributes, "#12");
#else
			try {
					var x = gparam.Attributes;
					Assert.Fail ("#12");
			} catch (NotSupportedException) {}
#endif
			Assert.IsFalse (gparam.HasElementType, "#13");
			Assert.IsFalse (gparam.IsArray, "#14");
			Assert.IsFalse (gparam.IsByRef, "#15");
			Assert.IsFalse (gparam.IsCOMObject, "#16");
			Assert.IsFalse (gparam.IsPointer, "#17");
			Assert.IsFalse (gparam.IsPrimitive, "#18");

		}	

		[Test]
		public void Methods ()
		{
			TypeBuilder tb = module.DefineType ("ns.type", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("A", "B")[1];

			try {
				gparam.GetInterface ("foo", true);
				Assert.Fail ("#1");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetInterfaces ();
				Assert.Fail ("#2");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetElementType ();
				Assert.Fail ("#3");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetEvent ("foo", BindingFlags.Public);
				Assert.Fail ("#4");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetEvents (BindingFlags.Public);
				Assert.Fail ("#5");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetField ("foo", BindingFlags.Public);
				Assert.Fail ("#6");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetFields (BindingFlags.Public);
				Assert.Fail ("#7");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetMembers (BindingFlags.Public);
				Assert.Fail ("#8");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetMethod ("Sort");
				Assert.Fail ("#9");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetMethods (BindingFlags.Public);
				Assert.Fail ("#9");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetNestedType ("bla", BindingFlags.Public);
				Assert.Fail ("#10");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetNestedTypes (BindingFlags.Public);
				Assert.Fail ("#11");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetProperties (BindingFlags.Public);
				Assert.Fail ("#12");
			} catch (NotSupportedException) {

			}	
	
			try {
				gparam.GetProperty ("Length");
				Assert.Fail ("#13");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetConstructor (new Type[] { typeof (int) });
				Assert.Fail ("#14");
			} catch (NotSupportedException) {

			}
	
			try {
				gparam.GetArrayRank ();
				Assert.Fail ("#15");
			} catch (NotSupportedException) {

			}

			try {
				gparam.GetConstructors (BindingFlags.Public);
				Assert.Fail ("#16");
			} catch (NotSupportedException) {}

			try {
				gparam.InvokeMember ("GetLength", BindingFlags.Public, null, null, null);
				Assert.Fail ("#17");
			} catch (NotSupportedException) {}

			try {
				gparam.IsSubclassOf (gparam);
				Assert.Fail ("#18");
			} catch (NotSupportedException) {}

			try {
				gparam.IsAssignableFrom (gparam);
				Assert.Fail ("#19");
			} catch (NotSupportedException) {}

			try {
				gparam.GetInterfaceMap (typeof (IEnumerable));
				Assert.Fail ("#20");
			} catch (NotSupportedException) {}

			try {
				gparam.IsInstanceOfType (new object());
				Assert.Fail ("#21");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void MakeGenericType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("A", "B")[1];
			try {
				gparam.MakeGenericType (new Type[] { typeof (string) });
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}
		}


		[Test]
		public void GenericParameterAttributes ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("A", "B")[1];
			try {
				object attr = gparam.GenericParameterAttributes;
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void MakeArrayType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("A", "B")[1];
			Type res = gparam.MakeArrayType ();
			Assert.IsNotNull (res, "#1");
			Assert.IsTrue (res.IsArray, "#2");

			res = gparam.MakeArrayType (2);
			Assert.IsNotNull (res, "#3");
			Assert.IsTrue (res.IsArray, "#4");
		}

		[Test]
		public void MakeByRefType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("A", "B")[1];
			Type res = gparam.MakeByRefType ();

			Assert.IsNotNull (res, "#1");
			Assert.IsTrue (res.IsByRef, "#2");
		}

		[Test]
		public void MakePointerType ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("A", "B")[1];
			Type res = gparam.MakePointerType ();

			Assert.IsNotNull (res, "#1");
			Assert.IsTrue (res.IsPointer, "#2");
		}

		[Test]
		public void SetBaseTypeConstraintWithNull ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("A", "B")[1];

			Assert.IsNull (gparam.BaseType, "#1");
			gparam.SetBaseTypeConstraint (null);
			Assert.AreEqual (typeof (object), gparam.BaseType, "#2");
		}

		[Test]
		public void GenericTypeMembers ()
		{
			TypeBuilder tb = module.DefineType ("dd.test", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("A", "B")[1];

			try {
				gparam.GetGenericArguments ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}

			try {
				gparam.GetGenericParameterConstraints ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {}

			try {
				gparam.GetGenericTypeDefinition ();
				Assert.Fail ("#3");
			} catch (InvalidOperationException) {}
		
			Assert.IsTrue (gparam.ContainsGenericParameters, "#4");
			try {
				var x = gparam.GenericParameterAttributes;
				Assert.Fail ("#5");
			} catch (NotSupportedException) {}

			Assert.AreEqual (1, gparam.GenericParameterPosition, "#6");

			Assert.IsTrue (gparam.ContainsGenericParameters, "#7");

			Assert.IsTrue (gparam.IsGenericParameter, "#8");
			Assert.IsFalse (gparam.IsGenericType, "#9");
			Assert.IsFalse (gparam.IsGenericTypeDefinition, "#10");
		}

#if !NET_4_0
		[Category ("NotDotNet")]
#endif
		[Test]
		// CompilerContext no longer supported
		[Category ("NotWorking")]
		public void GetAttributeFlagsImpl ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave  | (AssemblyBuilderAccess)0x800);
			TypeBuilder tb = module.DefineType ("ns.type", TypeAttributes.Public);
			var gparam = tb.DefineGenericParameters ("A", "B")[1];

			Assert.AreEqual (TypeAttributes.Public, gparam.Attributes, "#1");
		}
	}
}
