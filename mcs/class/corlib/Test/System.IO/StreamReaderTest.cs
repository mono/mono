// StreamReaderTest.cs - NUnit Test Cases for the SystemIO.StreamReader class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace MonoTests.System.IO
{

public class StreamReaderTest : TestCase
{
 	protected override void SetUp() 
	{
	}

	protected override void TearDown() 
	{
	}

	private string _codeFileName = "resources" + Path.DirectorySeparatorChar + "AFile.txt";

	public void TestCtor1() {
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((Stream)null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			FileStream f = new FileStream(_codeFileName, FileMode.Open, FileAccess.Write);
			try {
				StreamReader r = new StreamReader(f);
				r.Close();
			} catch (ArgumentException) {
				errorThrown = true;
			}
			f.Close();
			Assert("no read error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			StreamReader r = new StreamReader(f);
			AssertNotNull("no stream reader", r);
			r.Close();
			f.Close();
		}
	}
	public void TestCtor2() {
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("");
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("empty string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((string)null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("nonexistentfile");
			} catch (FileNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("fileNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("nonexistentdir/file");
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("dirNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0]);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert("invalid filename error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			StreamReader r = new StreamReader(_codeFileName);
			AssertNotNull("no stream reader", r);
			r.Close();
		}
	}
	public void TestCtor3() {
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((Stream)null, false);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("null stream error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			FileStream f = new FileStream(_codeFileName, FileMode.Open, FileAccess.Write);
			try {
				StreamReader r = new StreamReader(f, false);
				r.Close();
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			f.Close();
			Assert("no read error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			StreamReader r = new StreamReader(f, false);
			AssertNotNull("no stream reader", r);
			r.Close();
			f.Close();
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((Stream)null, true);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			FileStream f = new FileStream(_codeFileName, FileMode.Open, FileAccess.Write);
			try {
				StreamReader r = new StreamReader(f, true);
				r.Close();
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			f.Close();
			Assert("no read error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			StreamReader r = new StreamReader(f, true);
			AssertNotNull("no stream reader", r);
			r.Close();
			f.Close();
		}
	}
	public void TestCtor4() {
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("", false);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("empty string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((string)null, false);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("nonexistentfile", false);
			} catch (FileNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("fileNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("nonexistentdir/file", false);
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("dirNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0], false);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert("invalid filename error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			StreamReader r = new StreamReader(_codeFileName, false);
			AssertNotNull("no stream reader", r);
			r.Close();
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("", true);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 6: " + e.ToString());
			}
			Assert("empty string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((string)null, true);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 7: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("nonexistentfile", true);
			} catch (FileNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 8: " + e.ToString());
			}
			Assert("fileNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("nonexistentdir/file", true);
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 9: " + e.ToString());
			}
			Assert("dirNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0], true);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 10: " + e.ToString());
			}
			Assert("invalid filename error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			StreamReader r = new StreamReader(_codeFileName, true);
			AssertNotNull("no stream reader", r);
			r.Close();
		}
	}

	// TODO - Ctor with Encoding
	
	public void TestBaseStream() {
		string progress = "beginning";
		try {
			Byte[] b = {};
			MemoryStream m = new MemoryStream(b);
			StreamReader r = new StreamReader(m);
			AssertEquals("wrong base stream ", m, r.BaseStream);
			progress = "Closing StreamReader";
			r.Close();
			progress = "Closing MemoryStream";
			m.Close();
		} catch (Exception e) {
			Fail ("At '" + progress + "' an unexpected exception was thrown: " + e.ToString());
		}
	}

	public void TestCurrentEncoding() {
		try {
			Byte[] b = {};
			MemoryStream m = new MemoryStream(b);
			StreamReader r = new StreamReader(m);
			AssertEquals("wrong encoding", 
				     Encoding.UTF8, r.CurrentEncoding);
		} catch (Exception e) {
			Fail ("Unexpected exception thrown: " + e.ToString());
		}
	}

	// TODO - Close - annoying spec - won't commit to any exceptions. How to test?
	// TODO - DiscardBufferedData - I have no clue how to test this function.

	public void TestPeek() {
		// FIXME - how to get an IO Exception?
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				MemoryStream m = new MemoryStream(b);
				StreamReader r = new StreamReader(m);
				m.Close();
				int nothing = r.Peek();
			} catch (ObjectDisposedException) {
				errorThrown = true;
			}
			Assert("nothing-to-peek-at error not thrown", errorThrown);
		}
		{
			Byte[] b = {1, 2, 3, 4, 5, 6};
			MemoryStream m = new MemoryStream(b);
			
			StreamReader r = new StreamReader(m);
			for (int i = 1; i <= 6; i++) {
				AssertEquals("peek incorrect", i, r.Peek());
				r.Read();
			}
			AssertEquals("should be none left", -1, r.Peek());
		}
	}

	public void TestRead() {
		// FIXME - how to get an IO Exception?
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				MemoryStream m = new MemoryStream(b);
				StreamReader r = new StreamReader(m);
				m.Close();
				int nothing = r.Read();
			} catch (ObjectDisposedException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("nothing-to-read error not thrown", errorThrown);
		}
		{
			Byte[] b = {1, 2, 3, 4, 5, 6};
			MemoryStream m = new MemoryStream(b);
			
			StreamReader r = new StreamReader(m);
			for (int i = 1; i <= 6; i++) {
				AssertEquals("read incorrect", i, r.Read());
			}
			AssertEquals("Should be none left", -1, r.Read());
		}

		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				StreamReader r = new StreamReader(new MemoryStream(b));
				r.Read(null, 0, 0);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("null buffer error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				StreamReader r = new StreamReader(new MemoryStream(b));
				Char[] c = new Char[1];
				r.Read(c, 0, 2);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("too-long range error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				StreamReader r = new StreamReader(new MemoryStream(b));
				Char[] c = new Char[1];
				r.Read(c, -1, 2);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("out of range error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				StreamReader r = new StreamReader(new MemoryStream(b));
				Char[] c = new Char[1];
				r.Read(c, 0, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert("out of range error not thrown", errorThrown);
		}
		{
			int ii = 1;
			try {
				Byte[] b = {(byte)'a', (byte)'b', (byte)'c', 
					    (byte)'d', (byte)'e', (byte)'f', 
					    (byte)'g'};
				MemoryStream m = new MemoryStream(b);
				ii++;
				StreamReader r = new StreamReader(m);
				ii++;

				char[] buffer = new Char[7];
				ii++;
				char[] target = {'g','d','e','f','b','c','a'};
				ii++;
				r.Read(buffer, 6, 1);
				ii++;
				r.Read(buffer, 4, 2);
				ii++;
				r.Read(buffer, 1, 3);
				ii++;
				r.Read(buffer, 0, 1);
				ii++;
				for (int i = 0; i < target.Length; i++) {
					AssertEquals("read no work", 
						     target[i], buffer[i]);
				i++;
				}
						    
			} catch (Exception e) {
				Fail ("Caught when ii=" + ii + ". e:" + e.ToString());
			}
		}
	}

	public void TestReadLine() {
		// TODO Out Of Memory Exc? IO Exc?
		Byte[] b = new Byte[8];
		b[0] = (byte)'a';
		b[1] = (byte)'\n';
		b[2] = (byte)'b';
		b[3] = (byte)'\n';
		b[4] = (byte)'c';
		b[5] = (byte)'\n';
		b[6] = (byte)'d';
		b[7] = (byte)'\n';
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		AssertEquals("line doesn't match", "a", r.ReadLine());
		AssertEquals("line doesn't match", "b", r.ReadLine());
		AssertEquals("line doesn't match", "c", r.ReadLine());
		AssertEquals("line doesn't match", "d", r.ReadLine());
		AssertEquals("line doesn't match", null, r.ReadLine());
	}

	public void TestReadToEnd() {
		// TODO Out Of Memory Exc? IO Exc?
		Byte[] b = new Byte[8];
		b[0] = (byte)'a';
		b[1] = (byte)'\n';
		b[2] = (byte)'b';
		b[3] = (byte)'\n';
		b[4] = (byte)'c';
		b[5] = (byte)'\n';
		b[6] = (byte)'d';
		b[7] = (byte)'\n';
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		AssertEquals("line doesn't match", "a\nb\nc\nd\n", r.ReadToEnd());
		AssertEquals("line doesn't match", "", r.ReadToEnd());
	}
}
}
