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
public class StringWriterTest {
	[Test]
	public void TestConstructors() {
                StringBuilder sb = new StringBuilder("Test String");

                StringWriter writer = new StringWriter( sb );
                Assert.AreEqual (sb, writer.GetStringBuilder());
        }

	[Test]
        public void TestCultureInfoConstructor() {

		StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
		Assert.IsNotNull (writer.GetStringBuilder());
		
		Assert.AreEqual (String.Empty, writer.ToString());
		
		writer.Write( 'A' );
		Assert.AreEqual ("A", writer.ToString());
		
		writer.Write( " foo" );
		Assert.AreEqual ("A foo", writer.ToString());
		
		
		char[] testBuffer = "Test String".ToCharArray();
		
		writer.Write( testBuffer, 0, 4 );
		Assert.AreEqual ("A fooTest", writer.ToString());
		
		writer.Write( testBuffer, 5, 6 );
		Assert.AreEqual ("A fooTestString", writer.ToString());
		
		writer = new StringWriter(CultureInfo.InvariantCulture);
		writer.Write(null as string);
		Assert.AreEqual ("", writer.ToString());
        }

	[Test]
        public void TestWrite() {
                StringWriter writer = new StringWriter();

                Assert.AreEqual (String.Empty, writer.ToString());
                
                writer.Write( 'A' );
                Assert.AreEqual ("A", writer.ToString());

                writer.Write( " foo" );
                Assert.AreEqual ("A foo", writer.ToString());

                
                char[] testBuffer = "Test String".ToCharArray();

                writer.Write( testBuffer, 0, 4 );
                Assert.AreEqual ("A fooTest", writer.ToString());

                writer.Write( testBuffer, 5, 6 );
                Assert.AreEqual ("A fooTestString", writer.ToString());

		writer = new StringWriter ();
                writer.Write(null as string);
                Assert.AreEqual ("", writer.ToString());

        }

	[Test]
        public void TestNewLine() {
        	
        	StringWriter writer = new StringWriter();
        	
        	writer.NewLine = "\n\r";
        	Assert.AreEqual ("\n\r", writer.NewLine, "NewLine 1");
        	
        	writer.WriteLine ("first");
        	Assert.AreEqual ("first\n\r", writer.ToString(), "NewLine 2");
        	
        	writer.NewLine = "\n";
        	Assert.AreEqual ("first\n\r", writer.ToString(), "NewLine 3");
        	
        	writer.WriteLine ("second");
        	Assert.AreEqual ("first\n\rsecond\n", writer.ToString(), "NewLine 4");
        	
        }
        
	[Test]
        public void TestWriteLine() {
        	
        	StringWriter writer = new StringWriter();
        	writer.NewLine = "\n";
        	
        	writer.WriteLine ("first line");
        	writer.WriteLine ("second line");
        	        	
        	Assert.AreEqual ("first line\nsecond line\n", writer.ToString (), "WriteLine 1");
        	writer.Close ();
        }
        
	[Test]
        public void TestGetStringBuilder() {
        	
        	StringWriter writer = new StringWriter ();
        	writer.Write ("line");
		StringBuilder builder = writer.GetStringBuilder ();
        	builder.Append (12);
        	Assert.AreEqual ("line12", writer.ToString (), "GetStringBuilder 1");
        	writer.Write ("test");
        	Assert.AreEqual ("line12test", builder.ToString (), "GetStringBuilder 2");        	        	
        }
        
	[Test]
        public void TestClose() {
        	
        	StringWriter writer = new StringWriter ();
        	writer.Write ("mono");
        	writer.Close ();
        	
        	try {
        		writer.Write ("kicks ass");
        		Assert.Fail ("Close 1");
        	} catch (Exception e) {
        		Assert.AreEqual (typeof (ObjectDisposedException), e.GetType (), "Close 2");
        	}

        	Assert.AreEqual ("mono", writer.ToString (), "Close 3");
        	writer.Flush ();
        	StringBuilder builder = writer.GetStringBuilder ();
        	Assert.AreEqual ("mono", builder.ToString (), "Close 4");
        	
        	builder.Append (" kicks ass");
        	Assert.AreEqual ("mono kicks ass", writer.ToString (), "Close 5");
        }

	[Test]
        public void TestExceptions () {
        	
        	try {
        		StringWriter writer = new StringWriter (null as StringBuilder);
        		Assert.Fail ();
        	} catch (Exception e) {
        		Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "Exceptions 1");
        	}
        	{
       		StringWriter writer = new StringWriter (null as IFormatProvider);
        	}
        	try {
	        	StringWriter writer = new StringWriter (null as StringBuilder, null as IFormatProvider);
        		Assert.Fail ();
        	} catch (Exception e) {
        		Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "Exceptions 2");
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
		Assert.IsNotNull (writer.Encoding, "Disposed-Encoding");
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
		Assert.IsNotNull (writer.GetStringBuilder (), "Disposed-GetStringBuilder");
	}

	[Test]
	public void Disposed_ToString ()
	{
        	StringWriter writer = new StringWriter ();
		writer.Write ("Mono");
		writer.Close ();
		Assert.AreEqual ("Mono", writer.ToString (), "Disposed-ToString");
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
