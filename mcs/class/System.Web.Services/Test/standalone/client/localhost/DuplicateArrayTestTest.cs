// Web service test for WSDL document:
// http://localhost:8080/DuplicateArrayTest.asmx?wsdl

using System;
using NUnit.Framework;
using DuplicateArrayTestTests.Soap;

namespace Localhost.DuplicateArrayTestTests
{
	[TestFixture]
	public class DuplicateArrayTestTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			DuplicateArrayTest s = new DuplicateArrayTest();

			string  title = "hello";
			string  url = "tmp";
			correctionsCorrection c = new correctionsCorrection();
			c.word = "bye";
			c.suggestions = new string[]{"end","test"};
			correctionsCorrection[] arr = new correctionsCorrection[2]{c,c};
			arr = s.SpellCheck(ref title,ref url,arr);
			
			Assert.AreEqual (2, arr.Length, "t1");
			
			for (int i =0; i< arr.Length ; i++)
			{
				c = arr[i];
				Assert.IsNotNull (c, "t2."+i);
				Assert.AreEqual ("bye", c.word, "t3."+i);
				Assert.IsNotNull (c.suggestions, "t4."+i);
				Assert.AreEqual (2, c.suggestions.Length, "t5."+i);
				Assert.AreEqual ("end", c.suggestions[0], "t6."+i);
				Assert.AreEqual ("test", c.suggestions[1], "t7."+i);
			}
			Assert.AreEqual ("hello", title, "t8");
			Assert.AreEqual ("tmp", url, "t9");
		}
	}
}
