// ConsoleTest.cs - NUnit Test Cases for the System.Console class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.IO;


namespace MonoTests.System
{

public class ConsoleTest : TestCase
{
	public ConsoleTest() {}

	TextWriter _err;
	TextReader _in;
	TextWriter _out;
	protected override void SetUp() 
	{
		_err = Console.Error;
		_in = Console.In;
		_out = Console.Out;
	}

	protected override void TearDown() 
	{
		Console.SetError(_err);
		Console.SetIn(_in);
		Console.SetOut(_out);
	}

	public void TestError() {
		AssertNotNull("No error", Console.Error);
	}

	public void TestIn() {
		AssertNotNull("No in", Console.In);
	}

	public void TestOut() {
		AssertNotNull("No out", Console.Out);
	}

	public void TestOpenStandardError() {
		{
			Stream err = Console.OpenStandardError();
			AssertNotNull("Can't open error", err);
		}
		{
			Stream err = Console.OpenStandardError(512);
			AssertNotNull("Can't open error", err);
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

	public void TestOpenStandardInput() {
		{
			Stream in1 = Console.OpenStandardInput();
			AssertNotNull("Can't open input", in1);
		}
		{
			Stream in1 = Console.OpenStandardInput(512);
			AssertNotNull("Can't open input", in1);
		}
		// see commented-out tests in TestOpenStandardError
	}
	
	public void TestOpenStandardOutput() {
		{
			Stream out1 = Console.OpenStandardOutput();
			AssertNotNull("Can't open output", out1);
		}
		{
			Stream out1 = Console.OpenStandardOutput(512);
			AssertNotNull("Can't open output", out1);
		}
		// see commented-out tests in TestOpenStandardError
	}
	
	public void TestRead() {
		String testStr = "This is a readline test";
		Stream s = new MemoryStream();
		TextWriter w = new StreamWriter(s);
		((StreamWriter)w).AutoFlush = true;
		TextReader r = new StreamReader(s);
		Console.SetIn(r);
		w.WriteLine(testStr);
		s.Position = 0;
		char val = (char) Console.Read();
		AssertEquals("Wrong read", 'T', val);
	}

	public void TestReadLine() {
		String testStr = "This is a readline test";
		Stream s = new MemoryStream();
		TextWriter w = new StreamWriter(s);
		((StreamWriter)w).AutoFlush = true;
		TextReader r = new StreamReader(s);
		Console.SetIn(r);
		w.WriteLine(testStr);
		s.Position = 0;
		String line = Console.ReadLine();
		AssertEquals("Wrong line", testStr, line);
	}

	public void TestSetError() {
		{
			bool errorThrown = false;
			try {
				Console.SetError(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("null error error not thrown", errorThrown);
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
			AssertEquals("Wrong line", testStr, line);
		}
	}

	public void TestSetIn() {
		{
			bool errorThrown = false;
			try {
				Console.SetIn(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("null in error not thrown", errorThrown);
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
			AssertEquals("Wrong line", testStr, line);
		}
	}

	public void TestSetOut() {
		{
			bool errorThrown = false;
			try {
				Console.SetOut(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("null out error not thrown", errorThrown);
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
			AssertEquals("Wrong line", testStr, line);
		}
	}

	public void TestWrite() {
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
		AssertEquals("Wrong line", testStr, line);
		endPos = s.Position;

		Boolean[] booleans = {true, false};
		foreach (bool b in booleans ) {
			Console.Write(b);
			s.Position = endPos;
			line = r.ReadToEnd();
			AssertEquals("Wrong boolean", b.ToString(), line);
			endPos = s.Position;
		}

		Char[] chars = {'a', ';', '?'};
		foreach (Char c in chars ) {
			Console.Write(c);
			s.Position = endPos;
			line = r.ReadToEnd();
			AssertEquals("Wrong char", c.ToString(), line);
			endPos = s.Position;
		}

		// test writing a null value		
		string x = null;
		Console.Write (x);

		// TODO - Likewise for char[], decimal, double, int, long, object, single, uint32, uint64
		// TODO - write with format string
	}
	public void TestWriteLine() {
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
		AssertEquals("Wrong line", testStr, line);
		endPos = s.Position;

		Boolean[] booleans = {true, false};
		foreach (bool b in booleans ) {
			Console.WriteLine(b);
			s.Position = endPos;
			line = r.ReadLine();
			AssertEquals("Wrong boolean", b.ToString(), line);
			endPos = s.Position;
		}

		Char[] chars = {'a', ';', '?'};
		foreach (Char c in chars ) {
			Console.WriteLine(c);
			s.Position = endPos;
			line = r.ReadLine();
			AssertEquals("Wrong char", c.ToString(), line);
			endPos = s.Position;
		}

		// test writing a null value		
		string x = null;
		Console.WriteLine (x);

		// TODO - Likewise for char[], decimal, double, int, long, object, single, uint32, uint64
		// TODO - write with format string
	}

}
}
