// AssemblyAlgorithmIdAttributeTest.cs
//
// Author: Vineeth N <nvineeth@yahoo.com>
//
// (C) 2004 Ximian, Inc. http://www.ximian.com
//
using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Configuration.Assemblies;
using NUnit.Framework;

namespace MonoTests.System.Reflection {

	/// <summary>
	/// Test Fixture for AssemblyAlgorithmIdAttribute class
	/// </summary>
	[TestFixture]
	public class AssemblyAlgorithmIdAttributeTest : Assertion
	{
		private AssemblyBuilder dynAssembly;
		AssemblyName dynAsmName = new AssemblyName ();
		AssemblyAlgorithmIdAttribute attr;

		public AssemblyAlgorithmIdAttributeTest ()
		{
			//create a dynamic assembly with the required attribute
			//and check for the validity

			dynAsmName.Name = "TestAssembly";

			dynAssembly = Thread.GetDomain ().DefineDynamicAssembly (
				dynAsmName,AssemblyBuilderAccess.Run
				);

			// Set the required Attribute of the assembly.
			Type attribute = typeof (AssemblyAlgorithmIdAttribute);
			ConstructorInfo ctrInfo = attribute.GetConstructor (
				new Type [] { typeof (AssemblyHashAlgorithm) }
				);
			CustomAttributeBuilder attrBuilder =
				new CustomAttributeBuilder (
				ctrInfo,
				new object [1] { AssemblyHashAlgorithm.MD5 }
				);
			dynAssembly.SetCustomAttribute (attrBuilder);
			object [] attributes = dynAssembly.GetCustomAttributes (true);
			attr = attributes [0] as AssemblyAlgorithmIdAttribute;
		}
		
		[Test]
		public void AlgorithmIdTest()
		{
			AssertEquals ("#Testing AlgorithmId",
				attr.AlgorithmId,
				(uint) AssemblyHashAlgorithm.MD5);
		}

		[Test]
		public void TypeIdTest ()
		{
			AssertEquals ("#testing Typeid",
				attr.TypeId,
				typeof (AssemblyAlgorithmIdAttribute)
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
				attr.Match (new AssemblyAlgorithmIdAttribute (AssemblyHashAlgorithm.SHA1)),
				false);
		}
	}
}

