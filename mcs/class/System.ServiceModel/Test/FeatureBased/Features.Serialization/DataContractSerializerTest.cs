using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using MonoTests.Features;
using MonoTests.Features.Contracts;
using System.Runtime.Serialization;
using NUnit.Framework;
using System.Xml;
using System.IO;

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

		[Test]
		public void DefaultTypeMapTest ()
		{
			string t = "<Type1 xmlns=\"http://schemas.datacontract.org/2004/07/NS1.NS3\" "+
				"xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">"+
				"<AType xmlns:a=\"http://schemas.datacontract.org/2004/07/NS1.NS2\">"+
				"<a:Description>A description</a:Description>"+
				"<a:ID>10</a:ID>"+
				"</AType>"+
				"<ErrorMsg i:nil=\"true\"/>"+
				"<ResultCode>1</ResultCode>"+
				"</Type1>";
			var ser = new DataContractSerializer (typeof (NS1.NS3.Type1));
			var ret = (NS1.NS3.Type1) ser.ReadObject (XmlReader.Create (new StringReader (t)));
			Assert.IsNotNull (ret.AType, "#1");
			Assert.AreEqual (ret.AType.Description, "A description", "#2");
		}
	}
}

namespace NS1.NS2 {
	public class Type2 {
		public int ID { get; set; }
		public string Description { get; set; }
	}
}

namespace NS1.NS3 {
	public class Type1
	{
		public int ResultCode { get; set; }
		public string ErrorMsg {get; set; }
		public NS2.Type2 AType {get; set; }
	}
}
