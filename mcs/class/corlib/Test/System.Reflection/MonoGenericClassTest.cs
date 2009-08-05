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
	}
#endif
}
