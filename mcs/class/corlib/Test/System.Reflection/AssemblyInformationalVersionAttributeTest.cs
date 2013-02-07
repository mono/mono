// AssemblyInformationalVersionAttributeTest.cs
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
	/// Test Fixture for AssemblyInformationalVersionAttribute.
	/// </summary>
	[TestFixture]
	public class AssemblyInformationalVersionAttributeTest {

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
			Assert.AreEqual (attr.InformationalVersion,
							 "2.0.0.0", "#1");
		}

		[Test]
		public void TypeIdTest ()
		{
			Assert.AreEqual (
				attr.TypeId,
				typeof (AssemblyInformationalVersionAttribute), "#1"
				);

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
				attr.Match (new AssemblyInformationalVersionAttribute ("Descrptn")),
				false, "#1");
		}
	}
}

#endif