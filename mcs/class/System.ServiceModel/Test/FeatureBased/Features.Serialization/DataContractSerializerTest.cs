#if !MOBILE && !XAMMAC_4_5
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

		[Test]
		public void TestResolverCalledWritingObject ()
		{
			// object to be serialized
			var obj = new TestDcsResolverNs.TestDcsResolverType1 { ObjProp = new TestDcsResolverNs.TestDcsResolverType2 { TestProp = "TestVal" } };
			// dcs with custom resolver
			var dcs = new DataContractSerializer (typeof (TestDcsResolverNs.TestDcsResolverType1), null, int.MaxValue, false, false, null,
												  new TestDcsResolverNs.TestDcsResolver ());
			// serialize obj to a string
			var builder = new StringBuilder ();
			using (var writer = XmlWriter.Create (builder)) {
				dcs.WriteObject (writer, obj);
			}
			var output = builder.ToString ();
			// check that output contains both DummyNs and DummyName from custom resolver
			Assert.IsTrue (output.Contains (TestDcsResolverNs.TestDcsResolver.DummyNs), "TestDummyNs");
			Assert.IsTrue (output.Contains (TestDcsResolverNs.TestDcsResolver.DummyName), "TestDummyName");
		}

		[Test]
		public void TestWritingDerivedInObjectProperty ()
		{
			// NOTE: The mono implementation of DCS diverges from .net in that the mono implementation allows
			// serialization of any type if it is in a property of type Object. The .net implementation will
			// throw an exception if a property contains anything other than the declared type unless the
			// object type is specified as a known type or a custom data contract resolver is supplied.
			// If the mono implementation is brought in-line with the .net implementation, then this test
			// will fail. The test should be removed or ignored at that point. It is included here to
			// ensure that until then the current functionality is not unintentionally changed.

			// object to be serialized
			var obj = new TestDcsResolverNs.TestDcsResolverType1 { ObjProp = new TestDcsResolverNs.TestDcsResolverType2 { TestProp = "TestVal" } };
			var dcs = new DataContractSerializer (typeof (TestDcsResolverNs.TestDcsResolverType1), null, int.MaxValue, false, false, null, null);
			// serialize obj to a string
			var builder = new StringBuilder ();
			using (var writer = XmlWriter.Create (builder)) {
				Assert.DoesNotThrow (() => dcs.WriteObject (writer, obj), "TestWrite");
			}
		}
	}
}

namespace TestDcsResolverNs {
	[DataContract]
	public class TestDcsResolverType1 {
		[DataMember]
		public object ObjProp { get; set; }
	}

	[DataContract]
	public class TestDcsResolverType2 {
		[DataMember]
		public string TestProp { get; set; }
	}

	public class TestDcsResolver : DataContractResolver {
		public const string DummyName = "DummyName";
		public const string DummyNs = "DummyNs";

		public override bool TryResolveType (Type t, Type dt, DataContractResolver ktr, out XmlDictionaryString tn, out XmlDictionaryString tns)
		{
			// call known type resolver first
			if (ktr.TryResolveType (t, dt, null, out tn, out tns))
				return true;

			// set dummy name and namespace so it's easy to look
			// for in test
			var d = new XmlDictionary ();
			tn = d.Add (DummyName);
			tns = d.Add (DummyNs);

			return true;
		}

		public override Type ResolveName (string tn, string tns, Type dt, DataContractResolver ktr)
		{
			return ktr.ResolveName (tn, tns, dt, null);
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
#endif
