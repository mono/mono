// AssemblyDescriptionAttributeTest.cs
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
	/// Summary description for AssemblyDescriptionAttributeTest.
	/// </summary>
	[TestFixture]
	public class AssemblyDescriptionAttributeTest
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
			Assert.AreEqual (
				attr.Description,
				"Emitted Assembly", "#1");
		}

		[Test]
		public void TypeIdTest ()
		{
			Assert.AreEqual (
				attr.TypeId,
				typeof (AssemblyDescriptionAttribute), "#1"
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
				attr.Match (new AssemblyDescriptionAttribute ("Descrptn")),
				false, "#1");
		}
	}
}

#endif
