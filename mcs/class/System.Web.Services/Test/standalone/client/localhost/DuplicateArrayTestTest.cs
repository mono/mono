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
			
			AssertEquals ("t1", 2, arr.Length);
			
			for (int i =0; i< arr.Length ; i++)
			{
				c = arr[i];
				AssertNotNull ("t2."+i, c);
				AssertEquals ("t3."+i, "bye", c.word);
				AssertNotNull ("t4."+i, c.suggestions);
				AssertEquals ("t5."+i, 2, c.suggestions.Length);
				AssertEquals ("t6."+i, "end", c.suggestions[0]);
				AssertEquals ("t7."+i, "test", c.suggestions[1]);
			}
			AssertEquals ("t8", "hello", title);
			AssertEquals ("t9", "tmp", url);
		}
	}
}
