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

public class StringReaderTest : TestCase {
	public  void TestReadLine() {
		string testme = "a\nb\nc\n";
		StringReader sr = new StringReader (testme);
		string inputLine;
		int lines = 0;
		while ((inputLine = sr.ReadLine ()) != null)
			lines++;
		
		AssertEquals ("Incorrect number of lines", 3, lines);
	}

	public void TestPeekRead() {
		StringReader reader = new StringReader( "Test String" );

		char c = (char)reader.Peek();
		AssertEquals("A1", 'T', c );

		char read = (char)reader.Read();

		AssertEquals("A2", 'T', read );

		c = (char)reader.Peek();

		AssertEquals("A3", 'e', c );
	}

	public void TestPeekAndReadAtEndOfString() {
		StringReader reader = new StringReader("x");

		char c = (char)reader.Peek();
		AssertEquals("A1", 'x', c );

		c = (char)reader.Read();
		AssertEquals("A2", 'x', c);

		int i = reader.Peek();
		AssertEquals("A3", -1, i);

		i = reader.Read();
		AssertEquals("A4", -1, i);

		i = reader.Peek();
		AssertEquals("A5", -1, i);
	}

	public void TestPeekAndReadEmptyString() {
		StringReader reader = new StringReader("");

		int i = reader.Peek();
		AssertEquals("A1", -1, i);

		i = reader.Read();
		AssertEquals("A2", -1, i);
	}

	public void TestRead() {
		StringReader reader = new StringReader( "Test String" );

		/* Read from start of string */
		char[] test = new char[5];

		int charsRead = reader.Read( test, 0, 5 );

		AssertEquals( 5, charsRead );
		AssertEquals( "Test ", new String(test)  );

		/* Read to end of string */
		//reader = new StringReader( "Test String" );

		test = new char[6];
		charsRead = reader.Read( test, 0, 6 );
		AssertEquals( 6, charsRead);
		AssertEquals( "String", new String( test )  );

		/* Read past end of string */

		test = new char[6];
		reader = new StringReader( "Foo" );
		charsRead = reader.Read( test, 0, 6 );
		AssertEquals( 3, charsRead );
		AssertEquals(  "Foo\0\0\0", new String( test ) );

	}

        public void TestReadEOL() {
                StringReader reader = new StringReader( "Line1\rLine2\r\nLine3\nLine4" );

                string test = reader.ReadLine();

                AssertEquals( "Line1", test );

                test = reader.ReadLine();

                AssertEquals( "Line2", test );

                test = reader.ReadLine();

                AssertEquals( "Line3", test );

                test = reader.ReadLine();

                AssertEquals( "Line4", test );
        }

        public void TestClose() {
        	
        	StringReader reader = new StringReader("reader");
        	reader.Close ();
        	
        	try {
        		reader.Read ();
        		Fail();
        	} catch (Exception e) {
        		AssertEquals ("Close 1", typeof (ObjectDisposedException), e.GetType ());
        	}
        	
        	try {
        		reader.Peek ();
        		Fail ();
        	} catch (Exception e) {
        		AssertEquals ("Close 2", typeof (ObjectDisposedException), e.GetType ());        		             
        	}        	
        }
        
        public void TestExceptions() {
        	
        	StringReader reader;
        	
        	try {
	        	reader = new StringReader(null);
        		Fail ();
        	} catch (Exception e) {
        		AssertEquals ("Exception 1", typeof (ArgumentNullException), e.GetType ());
        	}
        	
        	reader = new StringReader ("this is a test\nAnd nothing else");
		
		try {
			reader.Read (null, 0, 12);
			Fail ();
		} catch (Exception e) {
			AssertEquals ("Exception 2", typeof (ArgumentNullException), e.GetType ());
		}        		
        }

}

}
