// created on 7/21/2001 at 2:36 PM

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Collections.Specialized {


	/// <summary>Microsoft NameValueCollection test.</summary>
	public class NameValueCollectionTest : TestCase {

		public NameValueCollectionTest() : base("MonoTests.System.Collections.Specialized.NameValueCollectionTest testsuite") {}
		public NameValueCollectionTest(String name) : base(name) {}

		public static ITest Suite {
			get {
				return new TestSuite(typeof(NameValueCollectionTest));
			}
		}
		
		public void TestToDo ()
		{
		}
	}
}

