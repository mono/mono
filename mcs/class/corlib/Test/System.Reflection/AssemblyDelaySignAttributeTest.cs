// AssemblyDelaySignAttributeTest.cs
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
	/// Summary description for AssemblyDelaySignAttributeTest.
	/// </summary>
	[TestFixture]
	public class AssemblyDelaySignAttributeTest
	{
		private AssemblyBuilder dynAssembly;
		AssemblyName dynAsmName = new AssemblyName ();
		AssemblyDelaySignAttribute attr;
		
		public AssemblyDelaySignAttributeTest ()
		{
			//create a dynamic assembly with the required attribute
			//and check for the validity

			dynAsmName.Name = "TestAssembly";

			dynAssembly = Thread.GetDomain ().DefineDynamicAssembly (
				dynAsmName,AssemblyBuilderAccess.Run
				);

			// Set the required Attribute of the assembly.
			Type attribute = typeof (AssemblyDelaySignAttribute);
			ConstructorInfo ctrInfo = attribute.GetConstructor (
				new Type [] { typeof (bool) }
				);
			CustomAttributeBuilder attrBuilder =
				new CustomAttributeBuilder (ctrInfo, new object [1] { false });
			dynAssembly.SetCustomAttribute (attrBuilder);
			object [] attributes = dynAssembly.GetCustomAttributes (true);
			attr = attributes [0] as AssemblyDelaySignAttribute;
		}
		
		[Test]
		public void DelaySignTest ()
		{
			Assert.AreEqual (
				attr.DelaySign,
				false, "#1");
		}

		[Test]
		public void TypeIdTest ()
		{
			Assert.AreEqual (
				attr.TypeId,
				typeof (AssemblyDelaySignAttribute)
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
				attr.Match (new AssemblyDelaySignAttribute (true)),
				false, "#1");
		}
	}
}

#endif
