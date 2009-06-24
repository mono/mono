// DictionaryEntryTest

using System;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Collections {

	[TestFixture]
	public class DictionaryEntryTest {
		[Test]
		public void Ctor () {

			DictionaryEntry d = new DictionaryEntry (1, "something");
			Assert.IsNotNull (d);
			Assert.AreEqual (1, d.Key, "#01");
			Assert.AreEqual ("something", d.Value, "#02");
		}

		[Test]
		public void Key () {
			DictionaryEntry d = new DictionaryEntry (1, "something");
			d.Key = 77.77;
			Assert.AreEqual (77.77, d.Key, "#03");
		}

		[Test]
		public void Value () {
			DictionaryEntry d = new DictionaryEntry (1, "something");
			d.Value = 'p';
			Assert.AreEqual ('p', d.Value, "#04");
		}

		[Test]
#if ONLY_1_1
		[ExpectedException (typeof (ArgumentNullException))]
#endif			
		public void NullKeyCtor ()
		{
			DictionaryEntry d = new DictionaryEntry (null, "bar");
		}

		[Test]
#if ONLY_1_1
		[ExpectedException (typeof (ArgumentNullException))]
#endif			
		public void NullKeySetter ()
		{
			DictionaryEntry d = new DictionaryEntry ("foo", "bar");
			d.Key = null;
		}
	}
}
