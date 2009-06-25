//
// System.IO.StringWriter
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Add some testing for exceptions
//
// TODO: Some of the tests could be a bit more thorough
//

using NUnit.Framework;
using System.IO;
using System;

namespace MonoTests.System.IO {

[TestFixture]
public class StringReaderTest {
	[Test]
	public  void TestReadLine() {
		string testme = "a\nb\nc\n";
		StringReader sr = new StringReader (testme);
		string inputLine;
		int lines = 0;
		while ((inputLine = sr.ReadLine ()) != null)
			lines++;
		
		Assert.AreEqual (3, lines, "Incorrect number of lines");
	}

	[Test]
	public void TestPeekRead() {
		StringReader reader = new StringReader( "Test String" );

		char c = (char)reader.Peek();
		Assert.AreEqual ('T', c, "A1");

		char read = (char)reader.Read();

		Assert.AreEqual ('T', read, "A2");

		c = (char)reader.Peek();

		Assert.AreEqual ('e', c, "A3");
	}

	[Test]
	public void TestPeekAndReadAtEndOfString() {
		StringReader reader = new StringReader("x");

		char c = (char)reader.Peek();
		Assert.AreEqual ('x', c, "A1");

		c = (char)reader.Read();
		Assert.AreEqual ('x', c, "A2");

		int i = reader.Peek();
		Assert.AreEqual (-1, i, "A3");

		i = reader.Read();
		Assert.AreEqual (-1, i, "A4");

		i = reader.Peek();
		Assert.AreEqual (-1, i, "A5");
	}

	[Test]
	public void TestPeekAndReadEmptyString() {
		StringReader reader = new StringReader("");

		int i = reader.Peek();
		Assert.AreEqual (-1, i, "A1");

		i = reader.Read();
		Assert.AreEqual (-1, i, "A2");
	}

	[Test]
	public void TestRead() {
		StringReader reader = new StringReader( "Test String" );

		/* Read from start of string */
		char[] test = new char[5];

		int charsRead = reader.Read( test, 0, 5 );

		Assert.AreEqual (5, charsRead);
		Assert.AreEqual ("Test ", new String(test));

		/* Read to end of string */
		//reader = new StringReader( "Test String" );

		test = new char[6];
		charsRead = reader.Read( test, 0, 6 );
		Assert.AreEqual (6, charsRead);
		Assert.AreEqual ("String", new String( test ));

		/* Read past end of string */

		test = new char[6];
		reader = new StringReader( "Foo" );
		charsRead = reader.Read( test, 0, 6 );
		Assert.AreEqual (3, charsRead);
		Assert.AreEqual ("Foo\0\0\0", new String( test ));

		/* Check that a new invocation on the empty reader will return 0 */
		charsRead = reader.Read (test, 0, 6);
		Assert.AreEqual (0, charsRead);
		
	}

	[Test]
        public void TestReadEOL() {
                StringReader reader = new StringReader( "Line1\rLine2\r\nLine3\nLine4" );

                string test = reader.ReadLine();

                Assert.AreEqual ("Line1", test);

                test = reader.ReadLine();

                Assert.AreEqual ("Line2", test);

                test = reader.ReadLine();

                Assert.AreEqual ("Line3", test);

                test = reader.ReadLine();

                Assert.AreEqual ("Line4", test);
        }

	[Test]
        public void TestClose() {
        	
        	StringReader reader = new StringReader("reader");
        	reader.Close ();
        	
        	try {
        		reader.Read ();
        		Assert.Fail ();
        	} catch (Exception e) {
        		Assert.AreEqual (typeof (ObjectDisposedException), e.GetType (), "Close 1");
        	}
        	
        	try {
        		reader.Peek ();
        		Assert.Fail ();
        	} catch (Exception e) {
        		Assert.AreEqual (typeof (ObjectDisposedException), e.GetType (), "Close 2");        		             
        	}        	
        }
        
	[Test]
        public void TestExceptions() {
        	
        	StringReader reader;
        	
        	try {
	        	reader = new StringReader(null);
        		Assert.Fail ();
        	} catch (Exception e) {
        		Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "Exception 1");
        	}
        	
        	reader = new StringReader ("this is a test\nAnd nothing else");
		
		try {
			reader.Read (null, 0, 12);
			Assert.Fail ();
		} catch (Exception e) {
			Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "Exception 2");
		}        		
        }

	[Test]
	public void MoreEOL ()
	{
                TextReader tr = new StringReader ("There she was just a walking\n" +
						  "Down the street singin'\r" +
						  "Do wah diddy diddy dum diddy do");

		int i = 0;
		while (tr.ReadLine () != null)
			i++;

		Assert.AreEqual (3, i, "#01");
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_IndexOverflow () 
	{
		StringReader sr = new StringReader ("Mono");
		sr.Read (new char [4], Int32.MaxValue, 1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_CountOverflow () 
	{
		StringReader sr = new StringReader ("Mono");
		sr.Read (new char [4], 1, Int32.MaxValue);
	}

	[Test]
	public void Read_DoesntStopAtLineEndings ()
	{
		StringReader reader = new StringReader ("Line1\rLine2\r\nLine3\nLine4");
		Assert.AreEqual (reader.Read (new char[24], 0, 24), 24);
	}	

	[Test]
	public void MixedLineEnding ()
	{
		string foobar = "Foo\n\r\n\rBar";
		StringReader reader = new StringReader (foobar);
		int count = 0;
		while (reader.ReadLine () != null)
			count++;
		Assert.AreEqual (4, count);

	}
}

}
