// StreamReaderTest.cs - NUnit Test Cases for the SystemIO.StreamReader class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.IO
{
[TestFixture]
public class StreamReaderTest
{
	private string _tmpFolder;
	private string _codeFileName;
	private const string TestString = "Hello World!";

	[SetUp]
	public void SetUp ()
	{	
		_tmpFolder = Path.GetTempFileName ();
		if (File.Exists (_tmpFolder))
			File.Delete (_tmpFolder);
		_codeFileName = _tmpFolder + Path.DirectorySeparatorChar + "AFile.txt";

		if (!Directory.Exists (_tmpFolder))
			Directory.CreateDirectory (_tmpFolder);
		
		if (!File.Exists (_codeFileName))
			File.Create (_codeFileName).Close ();
	}

	[TearDown]
	public void TearDown ()
	{
		if (Directory.Exists (_tmpFolder))
			Directory.Delete (_tmpFolder, true);
	}


	[Test]
	public void TestCtor1() {
		{
			bool errorThrown = false;
			try {
				new StreamReader((Stream)null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "null string error not thrown");
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
			Assert.IsTrue (errorThrown, "no read error not thrown");
		}
		{
			// this is probably incestuous, but, oh well.
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			StreamReader r = new StreamReader(f);
			Assert.IsNotNull (r, "no stream reader");
			r.Close();
			f.Close();
		}
	}

	[Test]
	public void TestCtor2() {
		{
			bool errorThrown = false;
			try {
				new StreamReader("");
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "empty string error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader((string)null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "null string error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader("nonexistentfile");
			} catch (FileNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "fileNotFound error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader("nonexistentdir/file");
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "dirNotFound error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0]);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "invalid filename error not thrown");
		}
		{
			// this is probably incestuous, but, oh well.
			StreamReader r = new StreamReader(_codeFileName);
			Assert.IsNotNull (r, "no stream reader");
			r.Close();
		}
	}

	[Test]
	public void TestCtor3() {
		{
			bool errorThrown = false;
			try {
				new StreamReader((Stream)null, false);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "null stream error not thrown");
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
				Assert.Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			f.Close();
			Assert.IsTrue (errorThrown, "no read error not thrown");
		}
		{
			// this is probably incestuous, but, oh well.
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			StreamReader r = new StreamReader(f, false);
			Assert.IsNotNull (r, "no stream reader");
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
				Assert.Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "null string error not thrown");
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
				Assert.Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			f.Close();
			Assert.IsTrue (errorThrown, "no read error not thrown");
		}
		{
			// this is probably incestuous, but, oh well.
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			StreamReader r = new StreamReader(f, true);
			Assert.IsNotNull (r, "no stream reader");
			r.Close();
			f.Close();
		}
	}

	[Test]
	public void TestCtor4() {
		{
			bool errorThrown = false;
			try {
				new StreamReader("", false);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "empty string error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader((string)null, false);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "null string error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader(_tmpFolder + "/nonexistentfile", false);
			} catch (FileNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "fileNotFound error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader(_tmpFolder + "/nonexistentdir/file", false);
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "dirNotFound error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0], false);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "invalid filename error not thrown");
		}
		{
			// this is probably incestuous, but, oh well.
			StreamReader r = new StreamReader(_codeFileName, false);
			Assert.IsNotNull (r, "no stream reader");
			r.Close();
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader("", true);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 6: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "empty string error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader((string)null, true);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 7: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "null string error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader(_tmpFolder + "/nonexistentfile", true);
			} catch (FileNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 8: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "fileNotFound error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader(_tmpFolder + "/nonexistentdir/file", true);
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 9: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "dirNotFound error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0], true);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Assert.Fail ("Incorrect exception thrown at 10: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "invalid filename error not thrown");
		}
		{
			// this is probably incestuous, but, oh well.
			StreamReader r = new StreamReader(_codeFileName, true);
			Assert.IsNotNull (r, "no stream reader");
			r.Close();
		}
	}

	// TODO - Ctor with Encoding
	
	[Test]
	public void TestBaseStream() {
		Byte[] b = {};
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		Assert.AreSame (m, r.BaseStream, "wrong base stream ");
		r.Close();
		m.Close();
	}

	public void TestCurrentEncoding()
	{
		Byte[] b = {};
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		Assert.AreSame (Encoding.UTF8, r.CurrentEncoding, "wrong encoding");
	}

	// TODO - Close - annoying spec - won't commit to any exceptions. How to test?
	// TODO - DiscardBufferedData - I have no clue how to test this function.

	[Test]
	public void TestPeek() {
		// FIXME - how to get an IO Exception?
		Byte [] b;
		MemoryStream m;
		StreamReader r;

		try {
			b = new byte [0];
			m = new MemoryStream (b);
			r = new StreamReader(m);
			m.Close();
			int nothing = r.Peek();
			Assert.Fail ("#1");
		} catch (ObjectDisposedException) {
		}

		b = new byte [] {1, 2, 3, 4, 5, 6};
		m = new MemoryStream (b);
		r = new StreamReader(m);
		for (int i = 1; i <= 6; i++) {
			Assert.AreEqual (i, r.Peek(), "#2");
			r.Read();
		}
		Assert.AreEqual (-1, r.Peek(), "#3");
	}

	[Test]
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
				Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "nothing-to-read error not thrown");
		}
		{
			Byte[] b = {1, 2, 3, 4, 5, 6};
			MemoryStream m = new MemoryStream(b);
			
			StreamReader r = new StreamReader(m);
			for (int i = 1; i <= 6; i++) {
				Assert.AreEqual (i, r.Read (), "read incorrect");
			}
			Assert.AreEqual (-1, r.Read (), "Should be none left");
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
				Assert.Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "null buffer error not thrown");
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
				Assert.Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "too-long range error not thrown");
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
				Assert.Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "out of range error not thrown");
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
				Assert.Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert.IsTrue (errorThrown, "out of range error not thrown");
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
					Assert.AreEqual (target[i], buffer[i], "read no work");
				i++;
				}
			} catch (Exception e) {
				Assert.Fail ("Caught when ii=" + ii + ". e:" + e.ToString());
			}
		}
	}

	[Test]
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
		Assert.AreEqual ("a", r.ReadLine(), "#1");
		Assert.AreEqual ("b", r.ReadLine (), "#2");
		Assert.AreEqual ("c", r.ReadLine (), "#3");
		Assert.AreEqual ("d", r.ReadLine(), "#4");
		Assert.IsNull (r.ReadLine (), "#5");
	}

	[Test]
	public void ReadLine1() {
		Byte[] b = new Byte[10];
		b[0] = (byte)'a';
		b[1] = (byte)'\r';
		b[2] = (byte)'b';
		b[3] = (byte)'\n';
		b[4] = (byte)'c';
		b[5] = (byte)'\n';
		b[5] = (byte)'\r';
		b[6] = (byte)'d';
		b[7] = (byte)'\n';
		b[8] = (byte)'\r';
		b[9] = (byte)'\n';
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		Assert.AreEqual ("a", r.ReadLine (), "#1");
		Assert.AreEqual ("b", r.ReadLine (), "#2");
		Assert.AreEqual ("c", r.ReadLine (), "#3");
		Assert.AreEqual ("d", r.ReadLine (), "#4");
		Assert.AreEqual (string.Empty, r.ReadLine (), "#5");
		Assert.IsNull (r.ReadLine(), "#6");
	}

	[Test]
	public void ReadLine2() {
		Byte[] b = new Byte[10];
		b[0] = (byte)'\r';
		b[1] = (byte)'\r';
		b[2] = (byte)'\n';
		b[3] = (byte)'\n';
		b[4] = (byte)'c';
		b[5] = (byte)'\n';
		b[5] = (byte)'\r';
		b[6] = (byte)'d';
		b[7] = (byte)'\n';
		b[8] = (byte)'\r';
		b[9] = (byte)'\n';
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		Assert.AreEqual (string.Empty, r.ReadLine (), "#1");
		Assert.AreEqual (string.Empty, r.ReadLine (), "#2");
		Assert.AreEqual (string.Empty, r.ReadLine (), "#3");
		Assert.AreEqual ("c", r.ReadLine (), "#4");
		Assert.AreEqual ("d", r.ReadLine (), "#5");
		Assert.AreEqual (string.Empty, r.ReadLine (), "#6");
		Assert.IsNull (r.ReadLine (), "#7");
	}

	[Test]
	public void ReadLine3() {
		StringBuilder sb = new StringBuilder ();
		sb.Append (new string ('1', 32767));
		sb.Append ('\r');
		sb.Append ('\n');
		sb.Append ("Hola\n");
		byte [] bytes = Encoding.Default.GetBytes (sb.ToString ());
		MemoryStream m = new MemoryStream(bytes);
		StreamReader r = new StreamReader(m);
		Assert.AreEqual (new string ('1', 32767), r.ReadLine(), "#1");
		Assert.AreEqual ("Hola", r.ReadLine (), "#2");
		Assert.IsNull (r.ReadLine (), "#3");
	}

	[Test]
	public void ReadLine4() {
		StringBuilder sb = new StringBuilder ();
		sb.Append (new string ('1', 32767));
		sb.Append ('\r');
		sb.Append ('\n');
		sb.Append ("Hola\n");
		sb.Append (sb.ToString ());
		byte [] bytes = Encoding.Default.GetBytes (sb.ToString ());
		MemoryStream m = new MemoryStream(bytes);
		StreamReader r = new StreamReader(m);
		Assert.AreEqual (new string ('1', 32767), r.ReadLine (), "#1");
		Assert.AreEqual ("Hola", r.ReadLine (), "#2");
		Assert.AreEqual (new string ('1', 32767), r.ReadLine (), "#3");
		Assert.AreEqual ("Hola", r.ReadLine (), "#4");
		Assert.IsNull (r.ReadLine (), "#5");
	}

	[Test]
	public void ReadLine5() {
		StringBuilder sb = new StringBuilder ();
		sb.Append (new string ('1', 32768));
		sb.Append ('\r');
		sb.Append ('\n');
		sb.Append ("Hola\n");
		byte [] bytes = Encoding.Default.GetBytes (sb.ToString ());
		MemoryStream m = new MemoryStream(bytes);
		StreamReader r = new StreamReader(m);
		Assert.AreEqual (new string ('1', 32768), r.ReadLine (), "#1");
		Assert.AreEqual ("Hola", r.ReadLine (), "#2");
		Assert.IsNull (r.ReadLine (), "#3");
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
		Assert.AreEqual ("a\nb\nc\nd\n", r.ReadToEnd (), "#1");
		Assert.AreEqual (string.Empty, r.ReadToEnd (), "#2");
	}

	[Test]
	public void TestBaseStreamClosed ()
	{
		byte [] b = {};
		MemoryStream m = new MemoryStream (b);
		StreamReader r = new StreamReader (m);
		m.Close ();
		try {
			r.Peek ();
			Assert.Fail ();
		} catch (ObjectDisposedException) {
		}
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Contructor_Stream_NullEncoding () 
	{
		new StreamReader (new MemoryStream (), null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Contructor_Path_NullEncoding () 
	{
		new StreamReader (_codeFileName, null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Read_Null () 
	{
		StreamReader r = new StreamReader (new MemoryStream ());
		r.Read (null, 0, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_IndexOverflow () 
	{
		char[] array = new char [16];
		StreamReader r = new StreamReader (new MemoryStream (16));
		r.Read (array, 1, Int32.MaxValue);
	}	

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_CountOverflow () 
	{
		char[] array = new char [16];
		StreamReader r = new StreamReader (new MemoryStream (16));
		r.Read (array, Int32.MaxValue, 1);
	}

	[Test]
	public void Read_DoesntStopAtLineEndings ()
	{
		MemoryStream ms = new MemoryStream (Encoding.ASCII.GetBytes ("Line1\rLine2\r\nLine3\nLine4"));
		StreamReader reader = new StreamReader (ms);
		Assert.AreEqual (24, reader.Read (new char[24], 0, 24));
	}

	[Test]
	public void EncodingDetection()
	{
		if (!CheckEncodingDetected(Encoding.UTF8))
			Assert.Fail ("Failed to detect UTF8 encoded string");
		if (!CheckEncodingDetected(Encoding.Unicode))
			Assert.Fail ("Failed to detect UTF16LE encoded string");
		if (!CheckEncodingDetected(Encoding.BigEndianUnicode))
			Assert.Fail ("Failed to detect UTF16BE encoded string");
		if (!CheckEncodingDetected(Encoding.UTF32))
			Assert.Fail ("Failed to detect UTF32LE encoded string");
		if (!CheckEncodingDetected(new UTF32Encoding(true, true)))
			Assert.Fail ("Failed to detect UTF32BE encoded string");
	}

	// This is a special case, where the StreamReader has less than 4 bytes at 
	// encoding detection time, so it tries to check for Unicode encoding, instead of
	// waiting for more bytes to test against the UTF32 BOM.
	[Test]
	public void EncodingDetectionUnicode ()
	{
		byte [] bytes = new byte [3];
		bytes [0] = 0xff;
		bytes [1] = 0xfe;
		bytes [2] = 0;
		MemoryStream inStream = new MemoryStream (bytes);
		StreamReader reader = new StreamReader (inStream, Encoding.UTF8, true);

		// It should start with the encoding we used in the .ctor
		Assert.AreEqual (Encoding.UTF8, reader.CurrentEncoding, "#A1");

		reader.Read ();
		//reader.Read ();
		Assert.AreEqual (Encoding.Unicode, reader.CurrentEncoding, "#B1");

		reader.Close ();
	}

	private bool CheckEncodingDetected(Encoding encoding)
	{
		MemoryStream outStream = new MemoryStream();
		using (StreamWriter outWriter = new StreamWriter(outStream, encoding))
		{
			outWriter.Write(TestString);
		}
		byte[] testBytes = outStream.ToArray();

		StreamReader inReader = new StreamReader(new MemoryStream(testBytes, false));
		string decodedString = inReader.ReadToEnd();

		return decodedString == TestString;
	}
    
    [Test] // Bug445326
	[Category ("MobileNotWorking")]
	public void EndOfBufferIsCR ()
	{
		using (StreamReader reader = new StreamReader (TestResourceHelper.GetFullPathOfResource ("Test/resources/Fergie.GED"))) {
			string line;
			int count = 0;
			while ((line = reader.ReadLine ()) != null) {
				Assert.IsFalse (line.Length > 1000, "#1 " + count);
				count++;
			}
			Assert.AreEqual (16107, count, "#2");
		}
	}

	[Test]
	public void bug75526 ()
	{
		StreamReader sr = new StreamReader (new Bug75526Stream ());
		int len = sr.Read (new char [10], 0, 10);
		Assert.AreEqual (2, len);
	}

	class Bug75526Stream : MemoryStream
	{
		public override int Read (byte [] buffer, int offset, int count)
		{
			buffer [offset + 0] = (byte) 'a';
			buffer [offset + 1] = (byte) 'b';
			return 2;
		}
	}

	[Test]
	public void PeekWhileBlocking ()
	{
		StreamReader reader = new StreamReader (new MyStream ());
		int c = reader.Read ();
		Assert.IsFalse (reader.EndOfStream);
		string str = reader.ReadToEnd ();
		Assert.AreEqual ("bc", str);
	}

	[Test]
	public void EncodingChangedAuto ()
	{
		int testlines = 2048; // all data should larger than stream reader default buffer size
		string testdata = "test";
		MemoryStream ms = new MemoryStream();
		// write utf8 encoding data.
		using (StreamWriter sw = new StreamWriter (ms, Encoding.UTF8)) {
			for (int i = 0; i < testlines; i++)
				sw.WriteLine (testdata);
		}

		MemoryStream readms = new MemoryStream (ms.GetBuffer());
		using (StreamReader sr = new StreamReader(readms, Encoding.Unicode, true)) {
			for (int i = 0; i < testlines; i++) {
				string line = sr.ReadLine ();
				if (line != testdata)
					Assert.Fail ("Wrong line content");
			}
		}
	}

	[Test]
	public void NullStream ()
	{
		var buffer = new char[2];
		Assert.AreEqual (0, StreamReader.Null.ReadBlock (buffer, 0, buffer.Length));
	}

	[Test]
	public void ReadLineAsync ()
	{
		MemoryStream ms = new MemoryStream ();
		StreamWriter sw = new StreamWriter (ms, Encoding.UTF8);
		sw.WriteLine ("a");
		sw.WriteLine ("b");
		sw.Flush ();
		ms.Seek (0, SeekOrigin.Begin);

		Func<Task<string>> res = async () => {
			using (StreamReader reader = new StreamReader (ms)) {
				return await reader.ReadLineAsync () + await reader.ReadToEndAsync () + await reader.ReadToEndAsync ();
			}
		};

		var result = res ();
		Assert.IsTrue (result.Wait (3000), "#1");
		Assert.AreEqual ("ab" + Environment.NewLine, result.Result);
	}
}

class MyStream : Stream {
	int n;

	public override int Read (byte [] buffer, int offset, int size)
	{
		if (n == 0) {
			buffer [offset] = (byte) 'a';
			n++;
			return 1;
		} else if (n == 1) {
			buffer [offset] = (byte) 'b';
			buffer [offset + 1] = (byte) 'c';
			n++;
			return 2;
		}
		return 0;
	}

	public override bool CanRead {
		get { return true; }
	}

	public override bool CanSeek {
		get { return false; }
	}

	public override bool CanWrite {
		get { return false; }
	}

	public override long Length {
		get { return 0; }
	}

	public override long Position {
		get { return 0; }
			set { }
	}

	public override void Flush ()
	{
	}

	public override long Seek (long offset, SeekOrigin origin)
	{
		return 0;
	}

	public override void SetLength (long value)
	{
	}

	public override void Write (byte[] buffer, int offset, int count)
	{
	}
}

}
