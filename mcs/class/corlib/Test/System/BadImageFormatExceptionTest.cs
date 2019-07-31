//
// BadImageFormatExceptionTest.cs - Unit tests for
//	System.BadImageFormatException
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class BadImageFormatExceptionTest
	{
		[Test]
		public void Constructor1 ()
		{
			BadImageFormatException bif = new BadImageFormatException ();

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
			Assert.IsNotNull (bif.Message, "#4"); // Format of the executable (.exe) or library (.dll) is invalid
			Assert.IsNull (bif.FusionLog, "#5");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType().FullName), "#6");
		}

		[Test]
		public void Constructor2 ()
		{
			BadImageFormatException bif = new BadImageFormatException ("message");

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
			Assert.IsNotNull (bif.Message, "#4");
			Assert.AreEqual ("message", bif.Message, "#5");
			Assert.IsNull (bif.FusionLog, "#6");
			Assert.AreEqual (bif.GetType ().FullName + ": message",
				bif.ToString (), "#7");
		}

		[Test]
		public void Constructor2_Message_Empty ()
		{
			BadImageFormatException bif = new BadImageFormatException (string.Empty);

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
			Assert.IsNotNull (bif.Message, "#4");
			Assert.AreEqual (string.Empty, bif.Message, "#5");
			Assert.IsNull (bif.FusionLog, "#6");
			Assert.AreEqual (bif.GetType ().FullName + ": ",
				bif.ToString (), "#7");
		}

		[Test]
		public void Constructor2_Message_Null ()
		{
			BadImageFormatException bif = new BadImageFormatException ((string) null);

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
			Assert.IsNotNull (bif.Message, "#4");
			Assert.IsNull (bif.FusionLog, "#5");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName), "#6");
		}

		[Test]
		public void Constructor3 ()
		{
			ArithmeticException ame = new ArithmeticException ("something");
			BadImageFormatException bif = new BadImageFormatException ("message",
				ame);

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNotNull (bif.InnerException, "#3");
			Assert.AreSame (ame, bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual ("message", bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.AreEqual (bif.GetType ().FullName + ": message ---> "
				+ ame.GetType ().FullName + ": something", bif.ToString (), "#8");
		}

		[Test]
		public void Constructor3_Message_Empty ()
		{
			ArithmeticException ame = new ArithmeticException ("something");
			BadImageFormatException bif = new BadImageFormatException (string.Empty, ame);

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNotNull (bif.InnerException, "#3");
			Assert.AreSame (ame, bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual (string.Empty, bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.AreEqual (bif.GetType ().FullName + ":  ---> "
				+ ame.GetType ().FullName + ": something", bif.ToString (), "#8");
		}

		[Test]
		public void Constructor3_Message_Null ()
		{
			ArithmeticException ame = new ArithmeticException ("something");
			BadImageFormatException bif = new BadImageFormatException ((string) null, ame);

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNotNull (bif.InnerException, "#3");
			Assert.AreSame (ame, bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName), "#8");
			Assert.IsTrue (bif.ToString ().IndexOf ("---> " + ame.GetType ().FullName) != -1, "#9");
			Assert.IsFalse (bif.ToString ().IndexOf (Environment.NewLine) != -1, "#10");
		}

		[Test]
		public void Constructor3_InnerException_Null ()
		{
			BadImageFormatException bif = new BadImageFormatException ("message",
				(Exception) null);

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
			Assert.IsNotNull (bif.Message, "#4");
			Assert.AreEqual ("message", bif.Message, "#5");
			Assert.IsNull (bif.FusionLog, "#6");
			Assert.AreEqual (bif.GetType ().FullName + ": message",
				bif.ToString (), "#7");
		}

		[Test]
		public void Constructor4 ()
		{
			BadImageFormatException bif = new BadImageFormatException ("message",
				"file.txt");

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNotNull (bif.FileName, "#2");
			Assert.AreEqual ("file.txt", bif.FileName, "#3");
			Assert.IsNull (bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual ("message", bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName
				+ ": message" + Environment.NewLine), "#8");
			Assert.IsTrue (bif.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsFalse (bif.ToString ().IndexOf ("\"file.txt\"") != -1, "#9");
		}

		[Test]
		public void Constructor4_FileName_Empty ()
		{
			BadImageFormatException bif = new BadImageFormatException ("message",
				string.Empty);

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNotNull (bif.FileName, "#2");
			Assert.AreEqual (string.Empty, bif.FileName, "#3");
			Assert.IsNull (bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual ("message", bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.AreEqual (bif.GetType ().FullName + ": message",
				bif.ToString (), "#8");
		}

		[Test]
		public void Constructor4_FileName_Null ()
		{
			BadImageFormatException bif = new BadImageFormatException ("message",
				(string) null);

			Assert.IsNotNull (bif.Data, "#A1");
			Assert.IsNull (bif.FileName, "#A2");
			Assert.IsNull (bif.InnerException, "#A3");
			Assert.IsNotNull (bif.Message, "#A4");
			Assert.AreEqual ("message", bif.Message, "#A5");
			Assert.IsNull (bif.FusionLog, "#A6");
			Assert.AreEqual (bif.GetType ().FullName + ": message",
				bif.ToString (), "#A7");

			bif = new BadImageFormatException (string.Empty, (string) null);

			Assert.IsNotNull (bif.Data, "#B1");
			Assert.IsNull (bif.FileName, "#B2");
			Assert.IsNull (bif.InnerException, "#B3");
			Assert.IsNotNull (bif.Message, "#B4");
			Assert.AreEqual (string.Empty, bif.Message, "#B5");
			Assert.IsNull (bif.FusionLog, "#B6");
			Assert.AreEqual (bif.GetType ().FullName + ": ",
				bif.ToString (), "#B7");
		}

		[Test]
		public void Constructor4_FileNameAndMessage_Empty ()
		{
			BadImageFormatException bif = new BadImageFormatException (string.Empty,
				string.Empty);

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNotNull (bif.FileName, "#2");
			Assert.AreEqual (string.Empty, bif.FileName, "#3");
			Assert.IsNull (bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual (string.Empty, bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.AreEqual (bif.GetType ().FullName + ": ", bif.ToString (), "#8");
		}

		[Test]
		public void Constructor4_FileNameAndMessage_Null ()
		{
			BadImageFormatException bif = new BadImageFormatException ((string) null,
				(string) null);

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
			Assert.IsNotNull (bif.Message, "#4");
			Assert.IsNull (bif.FusionLog, "#5");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName
				+ ": "), "#6");
			Assert.IsFalse (bif.ToString ().IndexOf (Environment.NewLine) != -1, "#7");
		}

		[Test]
		public void Constructor4_Message_Empty ()
		{
			BadImageFormatException bif = null;
			
			bif = new BadImageFormatException (string.Empty, "file.txt");

			Assert.IsNotNull (bif.Data, "#1");
			Assert.IsNotNull (bif.FileName, "#2");
			Assert.AreEqual ("file.txt", bif.FileName, "#3");
			Assert.IsNull (bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual (string.Empty, bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName
				+ ": " + Environment.NewLine), "#8");
			Assert.IsTrue (bif.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsFalse (bif.ToString ().IndexOf ("\"file.txt\"") != -1, "#10");
		}

		[Test]
		public void Constructor4_Message_Null ()
		{
			BadImageFormatException bif = null;
			
			bif = new BadImageFormatException ((string) null, "file.txt");

			Assert.IsNotNull (bif.Data, "#A1");
			Assert.IsNotNull (bif.FileName, "#A2");
			Assert.AreEqual ("file.txt", bif.FileName, "#A3");
			Assert.IsNull (bif.InnerException, "#A4");
			Assert.IsNotNull (bif.Message, "#A5");
			Assert.IsNull (bif.FusionLog, "#A6");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName
				+ ": "), "#A7");
			Assert.IsTrue (bif.ToString ().IndexOf (Environment.NewLine) != -1, "#A8");
			Assert.IsTrue (bif.ToString ().IndexOf ("'file.txt'") != -1, "#A9");

			bif = new BadImageFormatException ((string) null, string.Empty);

			Assert.IsNotNull (bif.Data, "#B1");
			Assert.IsNotNull (bif.FileName, "#B2");
			Assert.AreEqual (string.Empty, bif.FileName, "#B3");
			Assert.IsNull (bif.InnerException, "#B4");
			// .NET 1.1: The format of the file 'file.txt' is invalid
			// .NET 2.0: Could not load file or assembly 'file.txt' or one of its ...
			// .NET Core: Format of the executable (.exe) or library (.dll) is invalid.
			Assert.IsNotNull (bif.Message, "#B5");
			Assert.IsNull (bif.FusionLog, "#B6");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName
				+ ": "), "#B7");
			Assert.IsFalse (bif.ToString ().IndexOf (Environment.NewLine) != -1, "#B8");
			Assert.IsTrue (bif.ToString ().IndexOf ("''") != -1, "#B9");
		}
	}
}
