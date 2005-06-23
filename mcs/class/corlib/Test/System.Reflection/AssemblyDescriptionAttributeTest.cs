// AssemblyDescriptionAttributeTest.cs
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
	/// Summary description for AssemblyDescriptionAttributeTest.
	/// </summary>
	[TestFixture]
	public class AssemblyDescriptionAttributeTest : Assertion
	{
		private AssemblyBuilder dynAssembly;
		AssemblyName dynAsmName = new AssemblyName ();
		AssemblyDescriptionAttribute attr;
		
		public AssemblyDescriptionAttributeTest ()
		{
			//create a dynamic assembly with the required attribute
			//and check for the validity

			dynAsmName.Name = "TestAssembly";

			dynAssembly = Thread.GetDomain ().DefineDynamicAssembly (
				dynAsmName,AssemblyBuilderAccess.Run
				);

			// Set the required Attribute of the assembly.
			Type attribute = typeof (AssemblyDescriptionAttribute);
			ConstructorInfo ctrInfo = attribute.GetConstructor (
				new Type [] { typeof (string) }
				);
			CustomAttributeBuilder attrBuilder =
				new CustomAttributeBuilder (ctrInfo, new object [1] { "Emitted Assembly" });
			dynAssembly.SetCustomAttribute (attrBuilder);
			object [] attributes = dynAssembly.GetCustomAttributes (true);
			attr = attributes [0] as AssemblyDescriptionAttribute;
		}

		[Test]
		public void DescriptionTest ()
		{
			AssertEquals ("#Testing Description",
				attr.Description,
				"Emitted Assembly");
		}

		[Test]
		public void TypeIdTest ()
		{
			AssertEquals ("#testing Typeid",
				attr.TypeId,
				typeof (AssemblyDescriptionAttribute)
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
				attr.Match (new AssemblyDescriptionAttribute ("Descrptn")),
				false);
		}
	}
}

