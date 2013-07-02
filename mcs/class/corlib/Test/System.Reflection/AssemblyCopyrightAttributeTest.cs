// AssemblyCopyrightAttributeTest.cs
//
// Author: Vineeth N <nvineeth@yahoo.com>
//
// (C) 2004 Ximian, Inc. http://www.ximian.com
//

#if !MOBILE
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
	public class AssemblyCopyrightAttributeTest
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
			Assert.AreEqual (
				attr.Copyright,
				"Ximian", "#1");
		}

		[Test]
		public void TypeIdTest ()
		{
			Assert.AreEqual (
				attr.TypeId,
				typeof (AssemblyCopyrightAttribute)
				, "#1");
		}

		[Test]
		public void MatchTestForTrue ()
		{
			Assert.AreEqual (
				attr.Match (attr),
				true, "#1");
		}

		[Test]
		public void MatchTestForFalse ()
		{	
			Assert.AreEqual (
				attr.Match (new AssemblyCopyrightAttribute ("imian")),
				false, "#1");
		}
	}
}

#endif
