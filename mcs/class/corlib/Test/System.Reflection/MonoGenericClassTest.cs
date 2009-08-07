//
// MonoGenericClassTest.cs - NUnit Test Cases for MonoGenericClassTest
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
			Assert.AreEqual ("foo.type[[System.Double, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", inst.FullName, "#3");
			Assert.AreEqual ("foo.type[[System.Double, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], MonoTests.System.Reflection.Emit.MonoGenericClassTest, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", inst.AssemblyQualifiedName, "#4");
			Assert.AreEqual ("foo.type[System.Double,System.String]", inst.ToString (), "#5");
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
	}
#endif
}
