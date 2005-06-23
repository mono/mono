// AssemblyCopyrightAttributeTest.cs
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
	/// Test Fixture for AssemblyCopyrightAttribute
	/// </summary>
	[TestFixture]
	public class AssemblyCopyrightAttributeTest : Assertion
	{
		private AssemblyBuilder dynAssembly;
		AssemblyName dynAsmName = new AssemblyName ();
		AssemblyCopyrightAttribute attr;

		public AssemblyCopyrightAttributeTest ()
		{
			//create a dynamic assembly with the required attribute
			//and check for the validity

			dynAsmName.Name = "TestAssembly";

			dynAssembly = Thread.GetDomain().DefineDynamicAssembly (
				dynAsmName,AssemblyBuilderAccess.Run
				);

			// Set the required Attribute of the assembly.
			Type attribute = typeof (AssemblyCopyrightAttribute);
			ConstructorInfo ctrInfo = attribute.GetConstructor (
				new Type []{ typeof (string) }
				);
			CustomAttributeBuilder attrBuilder =
				new CustomAttributeBuilder (ctrInfo, new object [1] {"Ximian"} );
			dynAssembly.SetCustomAttribute(attrBuilder);
			object [] attributes = dynAssembly.GetCustomAttributes (true);
			attr = attributes [0] as AssemblyCopyrightAttribute;
		}
		
		[Test]
		public void CopyrightTest ()
		{
			AssertEquals ("#Testing Copyright",
				attr.Copyright,
				"Ximian");
		}

		[Test]
		public void TypeIdTest ()
		{
			AssertEquals ("#testing Typeid",
				attr.TypeId,
				typeof (AssemblyCopyrightAttribute)
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
				attr.Match (new AssemblyCopyrightAttribute ("imian")),
				false);
		}
	}
}

