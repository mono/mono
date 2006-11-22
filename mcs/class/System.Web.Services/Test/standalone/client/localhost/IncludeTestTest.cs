// Web service test for WSDL document:
// http://localhost:8080/IncludeTest.asmx?wsdl

using System;
using NUnit.Framework;
using IncludeTestTests.Soap;

namespace Localhost.IncludeTestTests
{
	[TestFixture]
	public class IncludeTestTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			IncludeTest test = new IncludeTest();
			object[] data = test.foo ();
			Assert.IsNotNull (data);
			
			ComplexThing c1 = data[0] as ComplexThing;
			Assert.IsNotNull (c1);
			Assert.AreEqual ("abc",c1.name);
			
			ComplexThing c2 = data[1] as ComplexThing;
			Assert.IsNotNull (c2);
			Assert.AreEqual ("xyz",c2.name);
		}
	}
}
