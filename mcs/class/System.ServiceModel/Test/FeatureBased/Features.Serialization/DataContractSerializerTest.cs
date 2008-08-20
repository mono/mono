using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using MonoTests.Features;
using MonoTests.Features.Contracts;
using NUnit.Framework;

namespace MonoTests.Features.Serialization
{
	[TestFixture]
    public class DataContractSerializerTest : TestFixtureBase<DataContractTesterContractClient, DataContractTester, MonoTests.Features.Contracts.IDataContractTesterContract>
	{		

		[Test]
		public void TestPrimitiveComplexType () {
			Proxy.MonoTests.Features.Client.ComplexPrimitiveClass n1 = GetNewDataInstance ();
			Proxy.MonoTests.Features.Client.ComplexPrimitiveClass n2 = GetNewDataInstance ();
			Proxy.MonoTests.Features.Client.ComplexPrimitiveClass result = ClientProxy.Add (n1, n2);
			Assert.IsTrue (result.byteMember == 2);
			Assert.IsTrue (result.sbyteMember == 2);
			Assert.IsTrue (result.shortMember == 2);
			Assert.IsTrue (result.ushortMember == 2);
			Assert.IsTrue (result.intMember == 2);
			Assert.IsTrue (result.uintMember == 2);
			Assert.IsTrue (result.longMember == 2);
			Assert.IsTrue (result.ulongMember == 2);
			Assert.IsTrue (result.doubleMember == 2);
			Assert.IsTrue (result.floatMember == 2);
		}

		[Test]
		public void TestPrimitiveComplexTypeByRef () {
			Proxy.MonoTests.Features.Client.ComplexPrimitiveClass n1 = GetNewDataInstance ();
			Proxy.MonoTests.Features.Client.ComplexPrimitiveClass n2 = GetNewDataInstance ();
			Proxy.MonoTests.Features.Client.ComplexPrimitiveClass result = null;
			result = ClientProxy.AddByRef (n1, n2);
			Assert.IsTrue (result.byteMember == 2);
			Assert.IsTrue (result.sbyteMember == 2);
			Assert.IsTrue (result.shortMember == 2);
			Assert.IsTrue (result.ushortMember == 2);
			Assert.IsTrue (result.intMember == 2);
			Assert.IsTrue (result.uintMember == 2);
			Assert.IsTrue (result.longMember == 2);
			Assert.IsTrue (result.ulongMember == 2);
			Assert.IsTrue (result.doubleMember == 2);
			Assert.IsTrue (result.floatMember == 2);
		}

		private Proxy.MonoTests.Features.Client.ComplexPrimitiveClass GetNewDataInstance ()
		{
			Proxy.MonoTests.Features.Client.ComplexPrimitiveClass inst = new Proxy.MonoTests.Features.Client.ComplexPrimitiveClass ();
			inst.byteMember = 1;
			inst.sbyteMember = 1;
			inst.intMember = 1;
			inst.uintMember = 1;
			inst.shortMember = 1;
			inst.ushortMember = 1;
			inst.longMember = 1;
			inst.ulongMember = 1;
			inst.doubleMember = 1;
			inst.floatMember = 1;
			return inst;
		}
	}
}
