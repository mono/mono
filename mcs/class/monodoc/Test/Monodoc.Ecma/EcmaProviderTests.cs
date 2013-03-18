using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Monodoc;
using Monodoc.Ecma;
using Monodoc.Providers;
using Monodoc.Storage;

namespace MonoTests.Monodoc.Ecma
{

	[TestFixture]
	public class EcmaProviderTests
	{
		internal class NullHelpSource : HelpSource
		{
			public NullHelpSource()
			{
				base.Storage = new NullStorage();
			}
		}

		EcmaProvider ecma;
		HelpSource hs;
		const string baseDir = "../../class/monodoc/Test/monodoc_test/Ecma/MethodNames";

		[SetUp]
		public void Setup ()
		{
			ecma = new EcmaProvider ();
			//have to use NullHelpSource instead of Source, because Storage is protected member 
			hs = new NullHelpSource ();

			ecma.AddDirectory (baseDir);
			
			ecma.PopulateTree (hs.Tree);
		}

		[Test]
		public void TestEcmaClassA()
		{
			Node classA=hs.Tree.ChildNodes[0].ChildNodes[0];
			Assert.AreEqual ("ecma:0#ClassA/",classA.Element);
			Node classAMembers = classA.ChildNodes.FirstOrDefault (n => n.Element == "M");
			Assert.IsNotNull (classAMembers);
			Assert.AreEqual (1,classAMembers.ChildNodes.Count);
			//classAMembers.ChildNodes[0].
			Node classAOperators = classA.ChildNodes.FirstOrDefault (n => n.Element == "O");
			Assert.AreEqual (1,classAOperators.ChildNodes.Count);
		}

		[Test]
		public void TestEcmaClassB()
		{
			Node classB=hs.Tree.ChildNodes[0].ChildNodes[1];
			Assert.AreEqual ("ecma:1#ClassB/",classB.Element);
			Node classBOperators = classB.ChildNodes.FirstOrDefault (n => n.Element == "O");
			Assert.IsTrue (classBOperators == null || classBOperators.ChildNodes.Count == 0); 
			Node classBFields = classB.ChildNodes.FirstOrDefault (n => n.Element == "F");
			Assert.AreEqual (1,classBFields.ChildNodes.Count);
		}

	}
}
