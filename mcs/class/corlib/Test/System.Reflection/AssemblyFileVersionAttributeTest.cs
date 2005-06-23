// AssemblyFileVersionAttributeTest.cs
//
// Author: Vineeth N <nvineeth@yahoo.com>
//
// (C) 2004 Ximian, Inc. http://www.ximian.com
//
using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;

namespace MonoTests.System.Reflection {

	/// <summary>
	/// Summary description for AssemblyFileVersionAttributeTest.
	/// </summary>
	[TestFixture]
	public class AssemblyFileVersionAttributeTest : Assertion {

		private AssemblyBuilder dynAssembly;
		AssemblyName dynAsmName = new AssemblyName ();
		AssemblyFileVersionAttribute attr;
		
		public AssemblyFileVersionAttributeTest ()
		{
			//create a dynamic assembly with the required attribute
			//and check for the validity

			dynAsmName.Name = "TestAssembly";

			dynAssembly = Thread.GetDomain ().DefineDynamicAssembly (
				dynAsmName,AssemblyBuilderAccess.Run
				);

			// Set the required Attribute of the assembly.
			Type attribute = typeof (AssemblyFileVersionAttribute);
			ConstructorInfo ctrInfo = attribute.GetConstructor (
				new Type [] { typeof(string) }
				);
			CustomAttributeBuilder attrBuilder =
				new CustomAttributeBuilder(ctrInfo, new object [1] { "1.0.0.0" });
			dynAssembly.SetCustomAttribute (attrBuilder);
			object [] attributes = dynAssembly.GetCustomAttributes (true);
			attr = attributes [0] as AssemblyFileVersionAttribute;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullExceptionTest()
		{
			string version = null;
			new AssemblyFileVersionAttribute (version);
		}
		
		[Test]
		public void FileVersionTest ()
		{
			AssertEquals ("#Testing FileVersion",
				attr.Version,
				"1.0.0.0");
		}

		[Test]
		public void TypeIdTest ()
		{
			AssertEquals ("#testing Typeid",
				attr.TypeId,
				typeof (AssemblyFileVersionAttribute)
				);
		}

		[Test]
		public void MatchTestForTrue ()
		{
			AssertEquals ("#testing Match method-- for true",
				attr.Match (attr),
				true);
		}

		[Test]
		public void MatchTestForFalse ()
		{
			AssertEquals ("#testing Match method-- for false",
				attr.Match (new AssemblyFileVersionAttribute ("Descrptn")),
				false);
		}
	}
}

