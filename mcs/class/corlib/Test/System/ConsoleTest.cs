// ConsoleTest.cs - NUnit Test Cases for the System.Console class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace MonoTests.System
{
[TestFixture]
public class ConsoleTest
{
	public ConsoleTest() {}

	TextWriter _err;
	TextReader _in;
	TextWriter _out;

	[SetUp]
	public void SetUp() 
	{
		_err = Console.Error;
		_in = Console.In;
		_out = Console.Out;
	}

	[TearDown]
	public void TearDown() 
	{
		Console.SetError(_err);
		Console.SetIn(_in);
		Console.SetOut(_out);
	}

	[Test]
	public void TestError()
	{
		Assert.IsNotNull (Console.Error, "No error");
	}

	[Test]
	public void TestIn()
	{
		Assert.IsNotNull (Console.In, "No in");
	}

	[Test]
	public void TestOut()
	{
		Assert.IsNotNull (Console.Out, "No out");
	}

	[Test]
	public void TestOpenStandardError()
	{
		{
			Stream err = Console.OpenStandardError ();
			Assert.IsNotNull (err, "Can't open error #1");
		}
		{
			Stream err = Console.OpenStandardError (512);
			Assert.IsNotNull (err, "Can't open error #2");
		}
		// Spec says these are here, MS implementation says no.
		//{
		//bool errorThrown = false;
		//try {
		//Stream err = Console.OpenStandardError(-1);
		//} catch (ArgumentOutOfRangeException) {
		//errorThrown = true;
		//}
		//Assert("negative buffer error not thrown", 
		//errorThrown);
		//}
		//{
		//bool errorThrown = false;
		//try {
		//Stream err = Console.OpenStandardError(0);
		//} catch (ArgumentOutOfRangeException) {
		//errorThrown = true;
		//}
		//Assert("zero buffer error not thrown", errorThrown);
		//}
	}

	[Test]
	public void TestOpenStandardInput()
	{
		{
			Stream in1 = Console.OpenStandardInput ();
			Assert.IsNotNull (in1, "Can't open input #1");
		}
		{
			Stream in1 = Console.OpenStandardInput (512);
			Assert.IsNotNull (in1, "Can't open input #2");
		}
		// see commented-out tests in TestOpenStandardError
	}

	[Test]
	public void TestOpenStandardOutput()
	{
		{
			Stream out1 = Console.OpenStandardOutput ();
			Assert.IsNotNull(out1, "Can't open output #1");
		}
		{
			Stream out1 = Console.OpenStandardOutput(512);
			Assert.IsNotNull (out1, "Can't open output #2");
		}
		// see commented-out tests in TestOpenStandardError
	}

	[Test]
	public void TestRead()
	{
		String testStr = "This is a readline test";
		Stream s = new MemoryStream();
		TextWriter w = new StreamWriter(s);
		((StreamWriter)w).AutoFlush = true;
		TextReader r = new StreamReader(s);
		Console.SetIn(r);
		w.WriteLine(testStr);
		s.Position = 0;
		char val = (char) Console.Read();
		Assert.AreEqual ('T', val, "Wrong read");
	}

	[Test]
	public void TestReadLine()
	{
		String testStr = "This is a readline test";
		Stream s = new MemoryStream();
		TextWriter w = new StreamWriter(s);
		((StreamWriter)w).AutoFlush = true;
		TextReader r = new StreamReader(s);
		Console.SetIn(r);
		w.WriteLine(testStr);
		s.Position = 0;
		String line = Console.ReadLine();
		Assert.AreEqual (testStr, line, "Wrong line");
	}

	[Test]
	public void TestSetError()
	{
		{
			bool errorThrown = false;
			try {
				Console.SetError(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "null error error not thrown");
		}
		{
			String testStr = "This is a stderr test";
			Stream s = new MemoryStream();
			TextWriter w = new StreamWriter(s);
			((StreamWriter)w).AutoFlush = true;
			TextReader r = new StreamReader(s);
			Console.SetError(w);
			Console.Error.WriteLine(testStr);
			s.Position = 0;
			String line = r.ReadLine();
			Assert.AreEqual (testStr, line, "Wrong line");
		}
	}

	[Test]
	public void TestSetIn()
	{
		{
			bool errorThrown = false;
			try {
				Console.SetIn(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "null in error not thrown");
		}
		{
			String testStr = "This is a stdin test";
			Stream s = new MemoryStream();
			TextWriter w = new StreamWriter(s);
			((StreamWriter)w).AutoFlush = true;
			TextReader r = new StreamReader(s);
			Console.SetIn(r);
			w.WriteLine(testStr);
			s.Position = 0;
			String line = Console.In.ReadLine();
			Assert.AreEqual (testStr, line, "Wrong line");
		}
	}

	[Test]
	public void TestSetOut()
	{
		{
			bool errorThrown = false;
			try {
				Console.SetOut(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "null out error not thrown");
		}
		{
			String testStr = "This is a stdout test";
			Stream s = new MemoryStream();
			TextWriter w = new StreamWriter(s);
			((StreamWriter)w).AutoFlush = true;
			TextReader r = new StreamReader(s);
			Console.SetOut(w);
			Console.Out.WriteLine(testStr);
			s.Position = 0;
			String line = r.ReadLine();
			Assert.AreEqual (testStr, line, "Wrong line");
		}
	}
	
	[Test]
	public void TestWrite_Params()
	{
		Console.Write ("text {0}", (object[]) null);
	}

	[Test]
	public void TestWrite()
	{
		Stream s = new MemoryStream();
		TextWriter w = new StreamWriter(s);
		((StreamWriter)w).AutoFlush = true;
		TextReader r = new StreamReader(s);
		Console.SetOut(w);

		long endPos = 0;

		String testStr = "This is a stdout write test";
		Console.Write(testStr);
		s.Position = endPos;
		String line = r.ReadToEnd();
		Assert.AreEqual (testStr, line, "Wrong line");
		endPos = s.Position;

		Boolean[] booleans = {true, false};
		foreach (bool b in booleans) {
			Console.Write(b);
			s.Position = endPos;
			line = r.ReadToEnd();
			Assert.AreEqual (b.ToString(), line, "Wrong boolean");
			endPos = s.Position;
		}

		Char[] chars = {'a', ';', '?'};
		foreach (Char c in chars) {
			Console.Write(c);
			s.Position = endPos;
			line = r.ReadToEnd();
			Assert.AreEqual (c.ToString(), line, "Wrong char");
			endPos = s.Position;
		}

		// test writing a null value		
		string x = null;
		Console.Write (x);

		// TODO - Likewise for char[], decimal, double, int, long, object, single, uint32, uint64
		// TODO - write with format string
	}

	[Test]
	public void TestWriteLine()
	{
		Stream s = new MemoryStream();
		TextWriter w = new StreamWriter(s);
		((StreamWriter)w).AutoFlush = true;
		TextReader r = new StreamReader(s);
		Console.SetOut(w);

		long endPos = 0;

		String testStr = "This is a stdout writeline test";
		Console.WriteLine(testStr);
		s.Position = endPos;
		String line = r.ReadLine();
		Assert.AreEqual (testStr, line, "Wrong line");
		endPos = s.Position;

		Boolean[] booleans = {true, false};
		foreach (bool b in booleans) {
			Console.WriteLine(b);
			s.Position = endPos;
			line = r.ReadLine();
			Assert.AreEqual (b.ToString(), line, "Wrong boolean");
			endPos = s.Position;
		}

		Char[] chars = {'a', ';', '?'};
		foreach (Char c in chars) {
			Console.WriteLine(c);
			s.Position = endPos;
			line = r.ReadLine();
			Assert.AreEqual (c.ToString(), line, "Wrong char");
			endPos = s.Position;
		}

		// test writing a null value		
		string x = null;
		Console.WriteLine (x);

		// TODO - Likewise for char[], decimal, double, int, long, object, single, uint32, uint64
		// TODO - write with format string
	}
	
	[Test]
	public void TestWriteLine_Params()
	{
		Stream s = new MemoryStream();
		TextWriter w = new StreamWriter(s);
		((StreamWriter)w).AutoFlush = true;
		TextReader r = new StreamReader(s);
		Console.SetOut(w);

		Console.WriteLine ("text {0}", (object[]) null);
	}

#if !MOBILE
	// Bug 678357
	[Test]
	public void EncodingTest ()
	{
		Console.OutputEncoding = Encoding.ASCII;
		Assert.AreEqual (Console.OutputEncoding, Console.Out.Encoding);
		Console.OutputEncoding = Encoding.UTF8;
		Assert.AreEqual (Console.OutputEncoding, Console.Out.Encoding);
	}
#endif
}
}
