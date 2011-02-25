//
// TextWriterTest.cs
//
// Author: 
//	William Holmes <billholmes54@gmail.com>
//
//

using System;
using System.IO;
using System.Text;
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
		
		[Test]
		public void TestTextReaderReadLine() {
			using(var reader = new TestTextReader("abc\n\ndef\r\nghi")) {
				var line = reader.ReadLine();
				Assert.AreEqual("abc", line, "first line (\\n)");
				
				line = reader.ReadLine();
				Assert.AreEqual("", line, "second line (empty)");
				
				line = reader.ReadLine();
				Assert.AreEqual("def", line, "third line (\\r\\n)");
				
				line = reader.ReadLine();
				Assert.AreEqual("ghi", line, "fourth line (last)");
				
				line = reader.ReadLine();
				Assert.AreEqual(null, line, "eof");
			}
		}
		
		[Test]
		public void TestTextReaderReadToEnd() {
			using(var reader = new TestTextReader("abc\n\ndef\r\nghi")) {
				var contents = reader.ReadToEnd();
				Assert.AreEqual("abc\n\ndef\r\nghi", contents);
			}
		}
		
		private class TestTextReader : TextReader {
			TextReader reader;
			
			public TestTextReader(string text)
			{
				this.reader = new StringReader(text);
			}
		
			public override int Peek()
			{
				return reader.Peek();
			}
			
			public override int Read()
			{
				return reader.Read();
			}
		}
	}
}
