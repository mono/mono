//
// MonoTests.Microsoft.CSharp.CompilerErrorTest
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.
//

using System;
using Microsoft.CSharp;
using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp  {

	public class CompilerErrorTest : TestCase {

		public CompilerErrorTest () :
			base ("[MonoTests.Microsoft.CSharp.CompilerErrorTest]") {}

		public CompilerErrorTest (string name) : base (name)
		{
		}

		public static ITest Suite {
			get {
				return new TestSuite (typeof (CompilerErrorTest));
			}
		}

		protected override void SetUp () { }

		protected override void TearDown () { }

		public void TestDefaultCtor ()
		{
			CompilerError error = new CompilerError ();

			AssertEquals ("#A01", error.ErrorLevel, ErrorLevel.None);
			AssertEquals ("#A02", error.ErrorMessage, String.Empty);
			AssertEquals ("#A03", error.ErrorNumber, 0);
			AssertEquals ("#A04", error.SourceColumn, 0);
			AssertEquals ("#A05", error.SourceLine, 0);
			AssertEquals ("#A06", error.SourceFile, String.Empty);
		}

		public void TestGetSetErrorLevel ()
		{
			CompilerError error = new CompilerError ();

			error.ErrorLevel = ErrorLevel.None;
			AssertEquals ("#A07", error.ErrorLevel, ErrorLevel.None);
			error.ErrorLevel = ErrorLevel.Warning;
			AssertEquals ("#A08", error.ErrorLevel, ErrorLevel.Warning);
			error.ErrorLevel = ErrorLevel.Error;
			AssertEquals ("#A09", error.ErrorLevel, ErrorLevel.Error);
			error.ErrorLevel = ErrorLevel.FatalError;
			AssertEquals ("#A10", error.ErrorLevel, ErrorLevel.FatalError);
		}

		public void TestGetSetErrorMessage ()
		{
			CompilerError error = new CompilerError ();
			string[] message_array = { "Message 01", "Message 02", "Message 03" };
			
			foreach (string message in message_array) {
				error.ErrorMessage = message;
				AssertEquals ("#A11", error.ErrorMessage, message);
			}
		}

		public void TestGetSetErrorNumber ()
		{
			CompilerError error = new CompilerError ();
			Random rand=new Random(); 
			int number;
			

			for (int i=0; i<100; i++) {
				number = rand.Next ();
				error.ErrorNumber = number;
				AssertEquals ("#A12", error.ErrorNumber, number);
			}
		}

		public void TestGetSetSourceColumn ()
		{
			CompilerError error = new CompilerError ();
			Random rand = new Random(); 
			int column;

			for (int i=0; i<100; i++) {
				column = rand.Next ();
				error.SourceColumn = column;
				AssertEquals ("#A13", error.SourceColumn, column);
			}
		}

		public void TestGetSetSourceLine ()
		{
			CompilerError error = new CompilerError ();
			Random rand = new Random(); 
			int line;

			for (int i=0; i<100; i++) {
				line = rand.Next ();
				error.SourceLine = line;
				AssertEquals ("#A14", error.SourceLine, line);
			}
		}

		public void TestGetSetSourceFile ()
		{
			CompilerError error = new CompilerError ();
			string[] file_array = { "File 01", "File 02", "File 03" };
			
			foreach (string file in file_array) {
				error.SourceFile = file;
				AssertEquals ("#A15", error.SourceFile, file);
			}
		}

		public void TestToStringNoSource ()
		{
			CompilerError error = new CompilerError ();

			error.ErrorLevel = ErrorLevel.Warning;
			error.ErrorNumber = 101;
			error.ErrorMessage = "This is the error message";

			AssertEquals ("#A16", error.ToString (), 
				"warning CS101: This is the error message");
		}

		public void TestToStringSource ()
		{
			CompilerError error = new CompilerError ();
			
			error.ErrorLevel = ErrorLevel.Error;
			error.ErrorNumber = 101;
			error.ErrorMessage = "This is the error message";
			error.SourceFile = "SourceFile.cs";
			error.SourceLine = 10;
			error.SourceColumn = 15;

			AssertEquals ("#A17", error.ToString (), 
				"SourceFile.cs(10,15) error CS101: This is the error message");
		}

	}
}