//
// MonoGenericClassTest.cs - NUnit Test Cases for MonoGenericClassTest
//
// Rodrigo Kumpera <rkumpera@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace MonoTests.System.Reflection.Emit
{
#if NET_2_0
	[TestFixture]
	public class MonoGenericClassTest
	{
		AssemblyBuilder assembly;
		ModuleBuilder module;
		int typeCount;
		static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.MonoGenericClassTest";

		string MakeName ()
		{
			return "internal__type"+ typeCount++;
		}

		[SetUp]
		public void SetUp ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave);
		}
		
		void SetUp (AssemblyBuilderAccess access)
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;

			assembly =
				Thread.GetDomain ().DefineDynamicAssembly (
					assemblyName, access, Path.GetTempPath ());

			module = assembly.DefineDynamicModule ("module1");
			typeCount = 0;
		}


		[Test]
		public void TestNameMethods ()
		{
			TypeBuilder tb = module.DefineType ("foo.type");
			tb.DefineGenericParameters ("T", "K");

			Type inst = tb.MakeGenericType (typeof (double), typeof (string));

			Assert.AreEqual ("type", inst.Name, "#1");
			Assert.AreEqual ("foo", inst.Namespace, "#2");
#if NET_4_0
			Assert.AreEqual ("foo.type[[System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", inst.FullName, "#3");
			Assert.AreEqual ("foo.type[[System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], MonoTests.System.Reflection.Emit.MonoGenericClassTest, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", inst.AssemblyQualifiedName, "#4");
#elif NET_2_1
			Assert.AreEqual ("foo.type[[System.Double, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]", inst.FullName, "#3");
			Assert.AreEqual ("foo.type[[System.Double, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], MonoTests.System.Reflection.Emit.MonoGenericClassTest, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", inst.AssemblyQualifiedName, "#4");
#else
			Assert.AreEqual ("foo.type[[System.Double, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", inst.FullName, "#3");
			Assert.AreEqual ("foo.type[[System.Double, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], MonoTests.System.Reflection.Emit.MonoGenericClassTest, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", inst.AssemblyQualifiedName, "#4");
#endif
			Assert.AreEqual ("foo.type[System.Double,System.String]", inst.ToString (), "#5");
		}

		static void CheckInst (string prefix, Type inst, int a, int b)
		{
			var resA = inst.GetMethods (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			var resB = inst.GetMethods (BindingFlags.Public | BindingFlags.Instance);

			Assert.AreEqual (a, resA.Length, prefix + 1);
			Assert.AreEqual (b, resB.Length, prefix + 2);
		}

		[Test]
		[Category ("NotDotNet")]
		public void GetMethodsWorkWithFunkyInstantiations ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);
			TypeBuilder tb = module.DefineType ("Base", TypeAttributes.Public, typeof (object));

			var a = typeof (IList<>).GetGenericArguments () [0];
			var b = tb.DefineGenericParameters ("T") [0];

			CheckInst ("#A", typeof (Collection<>).MakeGenericType (new Type [] {a}), 12, 16);
			CheckInst ("#B", typeof (Collection<>).MakeGenericType (new Type[] { b }), 12, 16);

			var tb2 = module.DefineType ("Child", TypeAttributes.Public, typeof (Collection<>).MakeGenericType (tb.MakeGenericType (typeof (int))));
			tb2.DefineGenericParameters ("K");

			CheckInst ("#C", tb2.MakeGenericType (typeof (double)), 0, 16);
			
		}

		[Test]
		[Category ("NotDotNet")]
		public void GetEventMustWorkUnderCompilerContext ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);
			var tb = module.DefineType ("foo.type");
			tb.DefineGenericParameters ("T");

			var ginst = tb.MakeGenericType (typeof (double));
			
			try {
				ginst.GetEvent ("foo", BindingFlags.Public | BindingFlags.Instance);
			} catch (NotSupportedException) {
				Assert.Fail ("#1");
			}
		}

		[Test]
		public void MethodsThatRaiseNotSupported ()
		{
			var tb = module.DefineType ("foo.type");
			tb.DefineGenericParameters ("T");

			var ginst = tb.MakeGenericType (typeof (double));

			/*try { //FIXME this doesn't work yet
				ginst.GetElementType ();
				Assert.Fail ("#1");
			} catch (NotSupportedException) {  }*/
			try {
				ginst.GetInterface ("foo", true);
				Assert.Fail ("#2");
			} catch (NotSupportedException) {  }
			try {
				ginst.GetEvent ("foo", BindingFlags.Public | BindingFlags.Instance);
				Assert.Fail ("#3");
			} catch (NotSupportedException) {  }
			try {
				ginst.GetField ("foo", BindingFlags.Public | BindingFlags.Instance);
				Assert.Fail ("#4");
			} catch (NotSupportedException) {  }
			try {
				ginst.GetMembers (BindingFlags.Public | BindingFlags.Instance);
				Assert.Fail ("#5");
			} catch (NotSupportedException) {  }
			try {
				ginst.GetMethod ("Foo");
				Assert.Fail ("#6");
			} catch (NotSupportedException) {  }
			try {
				ginst.GetNestedType ("foo", BindingFlags.Public | BindingFlags.Instance);
				Assert.Fail ("#7");
			} catch (NotSupportedException) {  }
			try {
				ginst.GetProperty ("foo");
				Assert.Fail ("#8");
			} catch (NotSupportedException) {  }
			try {
				var x = ginst.TypeInitializer;
				Assert.Fail ("#9");
			} catch (NotSupportedException) {  }
			try {
				var x = ginst.InvokeMember ("foo", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, null, null);
				Assert.Fail ("#10");
			} catch (NotSupportedException) {  }
			try {
				ginst.IsDefined (typeof (int), true);
				Assert.Fail ("#11");
			} catch (NotSupportedException) {  }
			try {
				ginst.GetCustomAttributes (true);
				Assert.Fail ("#12");
			} catch (NotSupportedException) {  }
			try {
				ginst.GetCustomAttributes (typeof (int), true);
				Assert.Fail ("#13");
			} catch (NotSupportedException) {  }
		}

		[Test]
		public void ClassMustNotBeRegisteredAfterTypeBuilderIsFinished ()
		{
			TypeBuilder tb = module.DefineType ("foo.type");
			tb.DefineGenericParameters ("T");

			var c = tb.CreateType ();

			var sreInst = tb.MakeGenericType (typeof (int));
			var rtInst = c.MakeGenericType (typeof (int));

			Assert.AreNotSame (sreInst, rtInst, "#1");

			/*This must not throw*/
			rtInst.IsDefined (typeof (int), true);
		}
	}

#endif
}
