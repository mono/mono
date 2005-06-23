// AssemblyInformationalVersionAttributeTest.cs
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
	/// Test Fixture for AssemblyInformationalVersionAttribute.
	/// </summary>
	[TestFixture]
	public class AssemblyInformationalVersionAttributeTest : Assertion {

		private AssemblyBuilder dynAssembly;
		AssemblyName dynAsmName = new AssemblyName ();
		AssemblyInformationalVersionAttribute attr;
		
		public AssemblyInformationalVersionAttributeTest ()
		{
			//create a dynamic assembly with the required attribute
			//and check for the validity

			dynAsmName.Name = "TestAssembly";

			dynAssembly = Thread.GetDomain ().DefineDynamicAssembly (
				dynAsmName,AssemblyBuilderAccess.Run
				);

			// Set the required Attribute of the assembly.
			Type attribute = typeof (AssemblyInformationalVersionAttribute);
			ConstructorInfo ctrInfo = attribute.GetConstructor (
				new Type [] { typeof (string) }
				);
			CustomAttributeBuilder attrBuilder =
				new CustomAttributeBuilder (ctrInfo, new object [1] { "2.0.0.0" });
			dynAssembly.SetCustomAttribute (attrBuilder);
			object [] attributes = dynAssembly.GetCustomAttributes (true);
			attr = attributes [0] as AssemblyInformationalVersionAttribute;
		}

		[Test]
		public void InformationalVersionTest ()
		{
			AssertEquals ("#Testing InformationalVersion",
				attr.InformationalVersion,
				"2.0.0.0");
		}

		[Test]
		public void TypeIdTest ()
		{
			AssertEquals ("#testing Typeid",
				attr.TypeId,
				typeof (AssemblyInformationalVersionAttribute)
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
				attr.Match (new AssemblyInformationalVersionAttribute ("Descrptn")),
				false);
		}
	}
}

