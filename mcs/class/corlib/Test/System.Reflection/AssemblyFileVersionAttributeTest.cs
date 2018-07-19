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
	public class AssemblyFileVersionAttributeTest {

#if !MOBILE
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
			Assert.AreEqual (attr.Version,
							 "1.0.0.0", "#1");
		}

		[Test]
		public void TypeIdTest ()
		{
			Assert.AreEqual (
				attr.TypeId,
				typeof (AssemblyFileVersionAttribute), "#1"
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
				attr.Match (new AssemblyFileVersionAttribute ("Descrptn")),
				false, "#1");
		}
#endif
		[Test]
		public void CtorTest ()
		{
			var a = new AssemblyFileVersionAttribute ("1.2.3.4");
			Assert.AreEqual ("1.2.3.4", a.Version);
		}
	}
}

