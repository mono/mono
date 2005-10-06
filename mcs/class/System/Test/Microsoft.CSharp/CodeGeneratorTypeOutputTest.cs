//
// CodeGeneratorTypeOutputTest.cs
//
// Author:
//	Gert Driesen <drieseng@users.sourceforge.net>
//
// (C)2005 Novell inc.
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
#if NET_2_0
using System.Collections.Generic;
#endif

using NUnit.Framework;

using Microsoft.CSharp;

namespace MonoTests.Microsoft.CSharp
{
	[TestFixture]
	public class CodeGeneratorTypeOutputTest
	{
		private ICodeGenerator gen;

		[SetUp]
		public void SetUp ()
		{
			gen = new CSharpCodeProvider ().CreateGenerator ();
		}

#if NET_2_0
		// This test fails on 2.0 profile, because our CodeTypeReference does not
		// parse basetype from right to left.
		[Test]
		[Category ("NotWorking")]
		public void GetTypeOutputFailure ()
		{
			Assert.AreEqual ("A[,,][,]", gen.GetTypeOutput (new CodeTypeReference ("A[,,][,]")), "#5");
			Assert.AreEqual ("A<B, D>[,]", gen.GetTypeOutput (new CodeTypeReference ("A[B,,D][,]")), "#6");
		}
#endif

		[Test]
		public void GetTypeOutput ()
		{
			Assert.AreEqual ("A", gen.GetTypeOutput (new CodeTypeReference ("A")), "#1");
			Assert.AreEqual ("A[]", gen.GetTypeOutput (new CodeTypeReference ("A[]")), "#2");
			Assert.AreEqual ("int[]", gen.GetTypeOutput (new CodeTypeReference (typeof (int).FullName, 1)), "#3");
			Assert.AreEqual ("int[,]", gen.GetTypeOutput (new CodeTypeReference (typeof (int).FullName, 2)), "#4");
#if NET_2_0
			Assert.AreEqual ("System.Nullable<int>", gen.GetTypeOutput (new CodeTypeReference (typeof (int?))), "#7");
			Assert.AreEqual ("System.Collections.Generic.Dictionary<int, string>", gen.GetTypeOutput (new CodeTypeReference (typeof (Dictionary<int, string>))), "#8");
#else
			Assert.AreEqual ("A[,,][,]", gen.GetTypeOutput (new CodeTypeReference ("A[,,][,]")), "#5");
			Assert.AreEqual ("A[B,,D][,]", gen.GetTypeOutput (new CodeTypeReference ("A[B,,D][,]")), "#6");
			Assert.AreEqual ("System.Nullable`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
				gen.GetTypeOutput (new CodeTypeReference ("System.Nullable`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")),
				"#9");
			Assert.AreEqual ("System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
				gen.GetTypeOutput (new CodeTypeReference ("System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")),
				"#10");
#endif
		}

		[Test]
		public void Shortcuts () {
			Assert.AreEqual ("int", gen.GetTypeOutput (new CodeTypeReference (typeof(int))), "#1");
			Assert.AreEqual ("int", gen.GetTypeOutput (new CodeTypeReference ("systeM.inT32")), "#2");
			Assert.AreEqual ("long", gen.GetTypeOutput (new CodeTypeReference (typeof(long))), "#3");
			Assert.AreEqual ("long", gen.GetTypeOutput (new CodeTypeReference ("systeM.inT64")), "#4");
			Assert.AreEqual ("short", gen.GetTypeOutput (new CodeTypeReference (typeof(short))), "#5");
			Assert.AreEqual ("short", gen.GetTypeOutput (new CodeTypeReference ("systeM.inT16")), "#6");
			Assert.AreEqual ("bool", gen.GetTypeOutput (new CodeTypeReference (typeof(bool))), "#7");
			Assert.AreEqual ("bool", gen.GetTypeOutput (new CodeTypeReference ("systeM.BooLean")), "#8");
			Assert.AreEqual ("char", gen.GetTypeOutput (new CodeTypeReference (typeof(char))), "#9");
			Assert.AreEqual ("char", gen.GetTypeOutput (new CodeTypeReference ("systeM.cHar")), "#10");
			Assert.AreEqual ("string", gen.GetTypeOutput (new CodeTypeReference (typeof(string))), "#11");
			Assert.AreEqual ("string", gen.GetTypeOutput (new CodeTypeReference ("systeM.sTring")), "#12");
			Assert.AreEqual ("object", gen.GetTypeOutput (new CodeTypeReference (typeof(object))), "#13");
			Assert.AreEqual ("object", gen.GetTypeOutput (new CodeTypeReference ("systeM.oBject")), "#14");
			Assert.AreEqual ("void", gen.GetTypeOutput (new CodeTypeReference (typeof(void))), "#15");
			Assert.AreEqual ("void", gen.GetTypeOutput (new CodeTypeReference ("systeM.vOid")), "#16");
#if NET_2_0
			Assert.AreEqual ("byte", gen.GetTypeOutput (new CodeTypeReference (typeof(byte))), "#17");
			Assert.AreEqual ("byte", gen.GetTypeOutput (new CodeTypeReference ("systeM.bYte")), "#18");
			Assert.AreEqual ("sbyte", gen.GetTypeOutput (new CodeTypeReference (typeof(sbyte))), "#19");
			Assert.AreEqual ("sbyte", gen.GetTypeOutput (new CodeTypeReference ("systeM.sBYte")), "#20");
			Assert.AreEqual ("decimal", gen.GetTypeOutput (new CodeTypeReference (typeof(decimal))), "#21");
			Assert.AreEqual ("decimal", gen.GetTypeOutput (new CodeTypeReference ("systeM.dEcimal")), "#22");
			Assert.AreEqual ("double", gen.GetTypeOutput (new CodeTypeReference (typeof(double))), "#23");
			Assert.AreEqual ("double", gen.GetTypeOutput (new CodeTypeReference ("systeM.dOuble")), "#24");
			Assert.AreEqual ("float", gen.GetTypeOutput (new CodeTypeReference (typeof(float))), "#25");
			Assert.AreEqual ("float", gen.GetTypeOutput (new CodeTypeReference ("systeM.SiNgle")), "#26");
			Assert.AreEqual ("uint", gen.GetTypeOutput (new CodeTypeReference (typeof (uint))), "#27");
			Assert.AreEqual ("uint", gen.GetTypeOutput (new CodeTypeReference ("systeM.UinT32")), "#28");
			Assert.AreEqual ("ulong", gen.GetTypeOutput (new CodeTypeReference (typeof (ulong))), "#29");
			Assert.AreEqual ("ulong", gen.GetTypeOutput (new CodeTypeReference ("systeM.uinT64")), "#30");
			Assert.AreEqual ("ushort", gen.GetTypeOutput (new CodeTypeReference (typeof (ushort))), "#31");
			Assert.AreEqual ("ushort", gen.GetTypeOutput (new CodeTypeReference ("systeM.uinT16")), "#32");
#else
			Assert.AreEqual (typeof (byte).FullName, gen.GetTypeOutput (new CodeTypeReference (typeof (byte))), "#17");
			Assert.AreEqual ("systeM.bYte", gen.GetTypeOutput (new CodeTypeReference ("systeM.bYte")), "#18");
			Assert.AreEqual (typeof (sbyte).FullName, gen.GetTypeOutput (new CodeTypeReference (typeof (sbyte))), "#19");
			Assert.AreEqual ("systeM.sBYte", gen.GetTypeOutput (new CodeTypeReference ("systeM.sBYte")), "#20");
			Assert.AreEqual (typeof (decimal).FullName, gen.GetTypeOutput (new CodeTypeReference (typeof (decimal))), "#21");
			Assert.AreEqual ("systeM.dEcimal", gen.GetTypeOutput (new CodeTypeReference ("systeM.dEcimal")), "#22");
			Assert.AreEqual (typeof (double).FullName, gen.GetTypeOutput (new CodeTypeReference (typeof (double))), "#23");
			Assert.AreEqual ("systeM.dOuble", gen.GetTypeOutput (new CodeTypeReference ("systeM.dOuble")), "#24");
			Assert.AreEqual (typeof (float).FullName, gen.GetTypeOutput (new CodeTypeReference (typeof (float))), "#25");
			Assert.AreEqual ("systeM.SiNgle", gen.GetTypeOutput (new CodeTypeReference ("systeM.SiNgle")), "#26");
			Assert.AreEqual (typeof (uint).FullName, gen.GetTypeOutput (new CodeTypeReference (typeof (uint))), "#27");
			Assert.AreEqual ("systeM.UinT32", gen.GetTypeOutput (new CodeTypeReference ("systeM.UinT32")), "#28");
			Assert.AreEqual (typeof (ulong).FullName, gen.GetTypeOutput (new CodeTypeReference (typeof (ulong))), "#29");
			Assert.AreEqual ("systeM.uinT64", gen.GetTypeOutput (new CodeTypeReference ("systeM.uinT64")), "#30");
			Assert.AreEqual (typeof (ushort).FullName, gen.GetTypeOutput (new CodeTypeReference (typeof (ushort))), "#31");
			Assert.AreEqual ("systeM.uinT16", gen.GetTypeOutput (new CodeTypeReference ("systeM.uinT16")), "#32");
#endif
		}
	}
}
