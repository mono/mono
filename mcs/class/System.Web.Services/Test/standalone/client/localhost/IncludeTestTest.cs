// Web service test for WSDL document:
// http://localhost:8080/IncludeTest.asmx?wsdl

using System;
using NUnit.Framework;
using IncludeTestTests.Soap;

namespace IncludeTestTests
{
	[TestFixture]
	public class IncludeTestTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			IncludeTest test = new IncludeTest();
			object[] data = test.foo ();
			AssertNotNull (data);
			
			ComplexThing c1 = data[0] as ComplexThing;
			AssertNotNull (c1);
			AssertEquals ("abc",c1.name);
			
			ComplexThing c2 = data[1] as ComplexThing;
			AssertNotNull (c2);
			AssertEquals ("xyz",c2.name);
		}
	}
}
