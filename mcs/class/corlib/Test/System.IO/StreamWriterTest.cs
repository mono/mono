// StreamWriterTest.cs - NUnit Test Cases for the SystemIO.StreamWriter class
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

[TestFixture]
public class StreamWriterTest
{
	static string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
	private string _codeFileName = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
	private string _thisCodeFileName = TempFolder + Path.DirectorySeparatorChar + "StreamWriterTest.temp";

	[SetUp]
	public void SetUp ()
	{
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
		Directory.CreateDirectory (TempFolder);

		if (!File.Exists (_thisCodeFileName)) 
			File.Create (_thisCodeFileName).Close ();
	}

	[TearDown]
	public void TearDown ()
	{
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
	}

	[Test] // .ctor (Stream)
	public void Constructor1 ()
	{
		FileStream f = new FileStream(_codeFileName, 
					      FileMode.Append, 
					      FileAccess.Write);
		StreamWriter r = new StreamWriter (f);
		Assert.IsFalse (r.AutoFlush, "#1");
		Assert.AreSame (f, r.BaseStream, "#2");
		Assert.IsNotNull (r.Encoding, "#3");
		Assert.AreEqual (typeof (UTF8Encoding), r.Encoding.GetType (), "#4");
		r.Close();
		f.Close();
	}

	[Test]
	public void Constructor1_Stream_NotWritable ()
	{
		FileStream f = new FileStream (_thisCodeFileName, FileMode.Open,
			FileAccess.Read);
		try {
			new StreamWriter (f);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Stream was not writable
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		} finally {
			f.Close ();
		}
	}

	[Test] // .ctor (Stream)
	public void Constructor1_Stream_Null ()
	{
		try {
			new StreamWriter((Stream) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("stream", ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String)
	public void Constructor2 ()
	{
		// TODO - Security/Auth exceptions
		using (StreamWriter r = new StreamWriter (_codeFileName)) {
			Assert.IsFalse (r.AutoFlush, "#1");
			Assert.IsNotNull (r.BaseStream, "#2");
			Assert.AreEqual (typeof (FileStream), r.BaseStream.GetType (), "#3");
			Assert.IsNotNull (r.Encoding, "#4");
			Assert.AreEqual (typeof (UTF8Encoding), r.Encoding.GetType (), "#5");
			r.Close ();
		}
	}

	[Test] // .ctor (String)
	public void Constructor2_Path_DirectoryNotFound ()
	{
		Directory.Delete (TempFolder, true);

		try {
			new StreamWriter (_codeFileName);
			Assert.Fail ("#1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsTrue (ex.Message.IndexOf (TempFolder) != -1, "#5");
		}
	}

	[Test] // .ctor (String)
	public void Constructor2_Path_Empty ()
	{
		try {
			new StreamWriter (string.Empty);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String)
	public void Constructor2_Path_IllegalChars ()
	{
		try {
			new StreamWriter ("!$what? what? Huh? !$*#" + Path.InvalidPathChars [0]);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String)
	public void Constructor2_Path_Null ()
	{
		try {
			new StreamWriter ((string) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("path", ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String)
	public void Constructor2_Path_Whitespace ()
	{
		try {
			new StreamWriter (" \r\n ");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor3 ()
	{
		using (StreamWriter r = new StreamWriter (_codeFileName, false)) {
			r.Close();
		}

		using (StreamWriter r = new StreamWriter(_codeFileName, true)) {
			r.Close();
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor3_Path_DirectoryNotFound ()
	{
		Directory.Delete (TempFolder, true);

		try {
			new StreamWriter (_codeFileName, false);
			Assert.Fail ("#A1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsTrue (ex.Message.IndexOf (TempFolder) != -1, "#A5");
		}

		try {
			new StreamWriter (_codeFileName, true);
			Assert.Fail ("#B1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsTrue (ex.Message.IndexOf (TempFolder) != -1, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor3_Path_Empty ()
	{
		try {
			new StreamWriter (string.Empty, false);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			new StreamWriter (string.Empty, true);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor3_Path_InvalidChars ()
	{
		try {
			new StreamWriter ("!$what? what? Huh? !$*#" + Path.InvalidPathChars [0], false);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			new StreamWriter ("!$what? what? Huh? !$*#" + Path.InvalidPathChars [0], true);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor3_Path_Null ()
	{
		try {
			new StreamWriter ((string) null, false);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("path", ex.ParamName, "#A5");
		}

		try {
			new StreamWriter ((string) null, true);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("path", ex.ParamName, "#B5");
		}
	}

	// TODO - ctors with Encoding

	[Test]
	public void AutoFlush ()
	{
		MemoryStream m;
		StreamWriter w;

		m = new MemoryStream ();
		w = new StreamWriter (m);
		w.Write (1);
		w.Write (2);
		w.Write (3);
		w.Write (4);
		Assert.AreEqual (0, m.Length, "#A1");
		w.AutoFlush = true;
		Assert.IsTrue (w.AutoFlush, "#A2");
		Assert.AreEqual (4, m.Length, "#A3");
		w.Flush ();
		Assert.AreEqual (4, m.Length, "#A4");

		m = new MemoryStream ();
		w = new StreamWriter(m);
		w.AutoFlush = true;
		Assert.IsTrue (w.AutoFlush, "#B1");
		w.Write (1);
		w.Write (2);
		w.Write (3);
		w.Write (4);
		Assert.AreEqual (4, m.Length, "#B2");
		w.Flush ();
		Assert.AreEqual (4, m.Length, "#B3");
		w.AutoFlush = false;
		Assert.IsFalse (w.AutoFlush, "#B4");
		w.Write (4);
		Assert.AreEqual (4, m.Length, "#B5");
		w.Flush ();
		Assert.AreEqual (5, m.Length, "#B6");
	}

	[Test]
	[Category ("NotWorking")]
	public void AutoFlush_Disposed ()
	{
		StreamWriter w;
		
		w = new StreamWriter (new MemoryStream ());
		w.Close ();
		w.AutoFlush = false;
		Assert.IsFalse (w.AutoFlush, "#A1");
		try {
			w.AutoFlush = true;
			Assert.Fail ("#A2");
		} catch (ObjectDisposedException) {
		}
		Assert.IsTrue (w.AutoFlush, "#A3");

		w = new StreamWriter (new MemoryStream ());
		w.AutoFlush = true;
		w.Close ();
		Assert.IsTrue (w.AutoFlush, "#B1");
		try {
			w.AutoFlush = true;
			Assert.Fail ("#B2");
		} catch (ObjectDisposedException) {
		}
		Assert.IsTrue (w.AutoFlush, "#B3");
		w.AutoFlush = false;
		Assert.IsFalse (w.AutoFlush, "#B4");
	}

	[Test]
	public void Close ()
	{
		Encoding encoding = Encoding.ASCII;
		MemoryStream m = new MemoryStream ();
		StreamWriter w = new StreamWriter (m, encoding);
		w.Write (2);
		Assert.AreEqual (0, m.Length, "#1");
		w.Close ();
		Assert.IsFalse (m.CanWrite, "#2");
		Assert.AreEqual (50, m.GetBuffer () [0], "#3");
		Assert.IsNull (w.BaseStream, "#4");
		Assert.IsNull (w.Encoding, "#5");
	}

	[Test]
	public void Flush ()
	{
		MemoryStream m = new MemoryStream();
		StreamWriter w = new StreamWriter(m);
		w.Write(1);
		w.Write(2);
		w.Write(3);
		w.Write(4);
		Assert.AreEqual (0L, m.Length, "#1");
		w.Flush();
		Assert.AreEqual (4L, m.Length, "#2");
	}

	[Test]
	public void Flush_Disposed ()
	{
		StreamWriter w = new StreamWriter(new MemoryStream ());
		w.Close();
		try {
			w.Flush ();
			Assert.Fail ("#1");
		} catch (ObjectDisposedException) {
		}
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void WriteChar_Disposed () 
	{
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Close ();
		w.Write ('A');
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void WriteCharArray_Disposed () 
	{
		char[] c = new char [2] { 'a', 'b' };
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Close ();
		w.Write (c, 0, 2);
	}

	[Test]
	public void WriteCharArray_Null () 
	{
		char[] c = null;
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Write (c);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void WriteCharArray_IndexOverflow () 
	{
		char[] c = new char [2] { 'a', 'b' };
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Write (c, Int32.MaxValue, 2);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void WriteCharArray_CountOverflow () 
	{
		char[] c = new char [2] { 'a', 'b' };
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Write (c, 1, Int32.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void WriteString_Disposed () 
	{
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Close ();
		w.Write ("mono");
	}

	[Test]
	public void WriteString_Null () 
	{
		string s = null;
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Write (s);
	}

	[Test]
	public void NoPreambleOnAppend ()
	{
		MemoryStream ms = new MemoryStream ();
		StreamWriter w = new StreamWriter (ms, Encoding.UTF8);
		w.Write ("a");
		w.Flush ();
		Assert.AreEqual (4, ms.Position, "#1");

		// Append 1 byte, should skip the preamble now.
		w.Write ("a");
		w.Flush ();
		w = new StreamWriter (ms, Encoding.UTF8);
		Assert.AreEqual (5, ms.Position, "#2");
	}
	
	// TODO - Write - test errors, functionality tested in TestFlush.
}
}
