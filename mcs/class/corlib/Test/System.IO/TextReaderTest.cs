//
// TextWriterTest.cs
//
// Author: 
//	William Holmes <billholmes54@gmail.com>
//
//

using System;
using System.IO;
using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class TextReaderTest
	{
		[Test]
		public void TestNullReader ()
		{
			Assert.IsNotNull (TextReader.Null);

			string name = TextReader.Null.GetType ().FullName;
			Assert.AreEqual ("System.IO.TextReader+NullTextReader", name);

			string linetest = TextReader.Null.ReadLine ();
			Assert.IsNull (linetest, "We expect null");

			string readtoendtest = TextReader.Null.ReadToEnd ();
			Assert.AreEqual (string.Empty, readtoendtest, "Expect an empty string." );

			int count = TextReader.Null.Read ();
			Assert.AreEqual (-1, count);
		}
	}
}
