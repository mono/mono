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
using System.Collections.Generic;

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

		// This test fails on 2.0 profile, because our CodeTypeReference does not
		// parse basetype from right to left.
		[Test]
		[Category ("NotWorking")]
		public void GetTypeOutputFailure ()
		{
			Assert.AreEqual ("A[,,][,]", gen.GetTypeOutput (new CodeTypeReference ("A[,,][,]")), "#5");
			Assert.AreEqual ("A<B, D>[,]", gen.GetTypeOutput (new CodeTypeReference ("A[B,,D][,]")), "#6");
		}

		[Test]
		public void GetTypeOutput ()
		{
			Assert.AreEqual ("A", gen.GetTypeOutput (new CodeTypeReference ("A")), "#1");
			Assert.AreEqual ("A[]", gen.GetTypeOutput (new CodeTypeReference ("A[]")), "#2");
			Assert.AreEqual ("int[]", gen.GetTypeOutput (new CodeTypeReference (typeof (int).FullName, 1)), "#3");
			Assert.AreEqual ("int[,]", gen.GetTypeOutput (new CodeTypeReference (typeof (int).FullName, 2)), "#4");
			Assert.AreEqual ("System.Nullable<int>", gen.GetTypeOutput (new CodeTypeReference (typeof (int?))), "#7");
			Assert.AreEqual ("System.Collections.Generic.Dictionary<int, string>", gen.GetTypeOutput (new CodeTypeReference (typeof (Dictionary<int, string>))), "#8");
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
		}
	}
}
