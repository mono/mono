//
// System.IO.StringWriter
//
// Authors:
//	Marcin Szczepanski (marcins@zipworld.com.au)
//	Ben Maurer <bmaurer@users.sourceforge.net>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System.IO;
using System;
using System.Globalization;
using System.Text;

namespace MonoTests.System.IO {

[TestFixture]
public class StringWriterTest : Assertion {
	public void TestConstructors() {
                StringBuilder sb = new StringBuilder("Test String");

                StringWriter writer = new StringWriter( sb );
                AssertEquals( sb, writer.GetStringBuilder() );
        }

        public void TestCultureInfoConstructor() {

		StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
		AssertNotNull( writer.GetStringBuilder() );
		
		AssertEquals( String.Empty, writer.ToString() );
		
		writer.Write( 'A' );
		AssertEquals( "A", writer.ToString() );
		
		writer.Write( " foo" );
		AssertEquals( "A foo", writer.ToString() );
		
		
		char[] testBuffer = "Test String".ToCharArray();
		
		writer.Write( testBuffer, 0, 4 );
		AssertEquals( "A fooTest", writer.ToString() );
		
		writer.Write( testBuffer, 5, 6 );
		AssertEquals( "A fooTestString", writer.ToString() );
		
		writer = new StringWriter(CultureInfo.InvariantCulture);
		writer.Write(null as string);
		AssertEquals( "", writer.ToString() );
        }

        public void TestWrite() {
                StringWriter writer = new StringWriter();

                AssertEquals( String.Empty, writer.ToString() );
                
                writer.Write( 'A' );
                AssertEquals( "A", writer.ToString() );

                writer.Write( " foo" );
                AssertEquals( "A foo", writer.ToString() );

                
                char[] testBuffer = "Test String".ToCharArray();

                writer.Write( testBuffer, 0, 4 );
                AssertEquals( "A fooTest", writer.ToString() );

                writer.Write( testBuffer, 5, 6 );
                AssertEquals( "A fooTestString", writer.ToString() );

		writer = new StringWriter ();
                writer.Write(null as string);
                AssertEquals( "", writer.ToString() );

        }

        public void TestNewLine() {
        	
        	StringWriter writer = new StringWriter();
        	
        	writer.NewLine = "\n\r";
        	AssertEquals ("NewLine 1", "\n\r", writer.NewLine);
        	
        	writer.WriteLine ("first");
        	AssertEquals ("NewLine 2", "first\n\r", writer.ToString());
        	
        	writer.NewLine = "\n";
        	AssertEquals ("NewLine 3", "first\n\r", writer.ToString());
        	
        	writer.WriteLine ("second");
        	AssertEquals ("NewLine 4", "first\n\rsecond\n", writer.ToString());
        	
        }
        
        public void TestWriteLine() {
        	
        	StringWriter writer = new StringWriter();
        	writer.NewLine = "\n";
        	
        	writer.WriteLine ("first line");
        	writer.WriteLine ("second line");
        	        	
        	AssertEquals ("WriteLine 1", "first line\nsecond line\n", writer.ToString ());
        	writer.Close ();
        }
        
        public void TestGetStringBuilder() {
        	
        	StringWriter writer = new StringWriter ();
        	writer.Write ("line");
		StringBuilder builder = writer.GetStringBuilder ();
        	builder.Append (12);
        	AssertEquals ("GetStringBuilder 1", "line12", writer.ToString ());
        	writer.Write ("test");
        	AssertEquals ("GetStringBuilder 2", "line12test", builder.ToString ());        	        	
        }
        
        public void TestClose() {
        	
        	StringWriter writer = new StringWriter ();
        	writer.Write ("mono");
        	writer.Close ();
        	
        	try {
        		writer.Write ("kicks ass");
        		Fail ("Close 1");
        	} catch (Exception e) {
        		AssertEquals ("Close 2", typeof (ObjectDisposedException), e.GetType ());
        	}

        	AssertEquals ("Close 3", "mono", writer.ToString ());
        	writer.Flush ();
        	StringBuilder builder = writer.GetStringBuilder ();
        	AssertEquals ("Close 4", "mono", builder.ToString ());
        	
        	builder.Append (" kicks ass");
        	AssertEquals ("Close 5", "mono kicks ass", writer.ToString ());
        }

        public void TestExceptions () {
        	
        	try {
        		StringWriter writer = new StringWriter (null as StringBuilder);
        		Fail();
        	} catch (Exception e) {
        		AssertEquals ("Exceptions 1", typeof (ArgumentNullException), e.GetType ());
        	}
        	{
       		StringWriter writer = new StringWriter (null as IFormatProvider);
        	}
        	try {
	        	StringWriter writer = new StringWriter (null as StringBuilder, null as IFormatProvider);
        		Fail ();
        	} catch (Exception e) {
        		AssertEquals ("Exceptions 2", typeof (ArgumentNullException), e.GetType ());
        	}        	        	
        }

	[Test]
	// strangely this is accepted [ExpectedException (typeof (ArgumentNullException))]
	public void WriteString_Null ()
	{
        	StringWriter writer = new StringWriter ();
		writer.Write (null as String);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void WriteChars_Null ()
	{
        	StringWriter writer = new StringWriter ();
		writer.Write (null, 0, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void WriteChars_IndexNegative ()
	{
		char[] c = new char [2] { 'a', 'b' };
        	StringWriter writer = new StringWriter ();
		writer.Write (c, -1, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void WriteChars_CountNegative ()
	{
		char[] c = new char [2] { 'a', 'b' };
        	StringWriter writer = new StringWriter ();
		writer.Write (c, 0, -1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void WriteChars_IndexOverflow ()
	{
		char[] c = new char [2] { 'a', 'b' };
        	StringWriter writer = new StringWriter ();
		writer.Write (c, Int32.MaxValue, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void WriteChars_CountOverflow ()
	{
		char[] c = new char [2] { 'a', 'b' };
        	StringWriter writer = new StringWriter ();
		writer.Write (c, 0, Int32.MaxValue);
	}

	[Test]
	public void Disposed_Encoding ()
	{
        	StringWriter writer = new StringWriter ();
		writer.Close ();
		AssertNotNull ("Disposed-Encoding", writer.Encoding);
	}

	[Test]
	public void Disposed_DoubleClose ()
	{
        	StringWriter writer = new StringWriter ();
		writer.Close ();
		writer.Close ();
	}

	[Test]
	public void Disposed_GetStringBuilder ()
	{
        	StringWriter writer = new StringWriter ();
		writer.Write ("Mono");
		writer.Close ();
		AssertNotNull ("Disposed-GetStringBuilder", writer.GetStringBuilder ());
	}

	[Test]
	public void Disposed_ToString ()
	{
        	StringWriter writer = new StringWriter ();
		writer.Write ("Mono");
		writer.Close ();
		AssertEquals ("Disposed-ToString", "Mono", writer.ToString ());
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void Disposed_WriteChar ()
	{
        	StringWriter writer = new StringWriter ();
		writer.Close ();
		writer.Write ('c');
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void Disposed_WriteString ()
	{
        	StringWriter writer = new StringWriter ();
		writer.Close ();
		writer.Write ("mono");
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void Disposed_WriteChars ()
	{
		char[] c = new char [2] { 'a', 'b' };
        	StringWriter writer = new StringWriter ();
		writer.Close ();
		writer.Write (c, 0, 2);
	}
}

}
