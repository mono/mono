// DictionaryEntryTest

using System;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Collections {

	[TestFixture]
	public class DictionaryEntryTest : Assertion {
		[Test]
		public void Ctor () {

			DictionaryEntry d = new DictionaryEntry (1, "something");
			AssertNotNull (d);
			AssertEquals ("#01", d.Key, 1);
			AssertEquals ("#02", d.Value, "something");
		}

		[Test]
		public void Key () {
			DictionaryEntry d = new DictionaryEntry (1, "something");
			d.Key = 77.77;
			AssertEquals ("#03", d.Key, 77.77);
		}

		[Test]
		public void Value () {
			DictionaryEntry d = new DictionaryEntry (1, "something");
			d.Value = 'p';
			AssertEquals ("#04", d.Value, 'p');
		}
	}
}
