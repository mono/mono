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

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
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

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
			Assert.IsNotNull (bif.Message, "#4");
			Assert.AreEqual ("message", bif.Message, "#5");
			Assert.IsNull (bif.FusionLog, "#6");
#if TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName + ": message"), "#7");
#else
			Assert.AreEqual (bif.GetType ().FullName + ": message",
				bif.ToString (), "#7");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor2_Message_Empty ()
		{
			BadImageFormatException bif = new BadImageFormatException (string.Empty);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
			Assert.IsNotNull (bif.Message, "#4");
			Assert.AreEqual (string.Empty, bif.Message, "#5");
			Assert.IsNull (bif.FusionLog, "#6");
#if TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType().FullName + ": "), "#7");
#else
			Assert.AreEqual (bif.GetType ().FullName + ": ",
				bif.ToString (), "#7");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor2_Message_Null ()
		{
			BadImageFormatException bif = new BadImageFormatException ((string) null);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
#if NET_2_0
			Assert.IsNotNull (bif.Message, "#4"); // Could not load file or assembly '' ...
			Assert.IsTrue (bif.Message.IndexOf ("''") != -1, "#5");
#else
			Assert.IsNotNull (bif.Message, "#4"); // Format of the executable (.exe) or library ...
			Assert.IsFalse (bif.Message.IndexOf ("''") != -1, "#5");
#endif
			Assert.IsNull (bif.FusionLog, "#5");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName), "#6");
#if NET_2_0
			Assert.IsTrue (bif.ToString ().IndexOf ("''") != -1, "#7");
#else
			Assert.IsFalse (bif.ToString ().IndexOf ("''") != -1, "#7");
#endif
		}

		[Test]
		public void Constructor3 ()
		{
			ArithmeticException ame = new ArithmeticException ("something");
			BadImageFormatException bif = new BadImageFormatException ("message",
				ame);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNotNull (bif.InnerException, "#3");
			Assert.AreSame (ame, bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual ("message", bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
#if TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsTrue (bif.ToString ().IndexOf (ame.GetType ().FullName + ": something") != -1, "#8");
#else
			Assert.AreEqual (bif.GetType ().FullName + ": message ---> "
				+ ame.GetType ().FullName + ": something", bif.ToString (), "#8");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor3_Message_Empty ()
		{
			ArithmeticException ame = new ArithmeticException ("something");
			BadImageFormatException bif = new BadImageFormatException (string.Empty, ame);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNotNull (bif.InnerException, "#3");
			Assert.AreSame (ame, bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual (string.Empty, bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
#if TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsTrue (bif.ToString ().IndexOf (ame.GetType ().FullName + ": something") != -1, "#8");
#else
			Assert.AreEqual (bif.GetType ().FullName + ":  ---> "
				+ ame.GetType ().FullName + ": something", bif.ToString (), "#8");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor3_Message_Null ()
		{
			ArithmeticException ame = new ArithmeticException ("something");
			BadImageFormatException bif = new BadImageFormatException ((string) null, ame);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNotNull (bif.InnerException, "#3");
			Assert.AreSame (ame, bif.InnerException, "#4");
#if NET_2_0
			Assert.IsNotNull (bif.Message, "#5"); // Could not load file or assembly '' ...
			Assert.IsTrue (bif.Message.IndexOf ("''") != -1, "#6");
#else
			Assert.IsNotNull (bif.Message, "#5"); // Format of the executable (.exe) or library ...
			Assert.IsFalse (bif.Message.IndexOf ("''") != -1, "#6");
#endif
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName), "#8");
			Assert.IsTrue (bif.ToString ().IndexOf ("---> " + ame.GetType ().FullName) != -1, "#9");
#if !TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsFalse (bif.ToString ().IndexOf (Environment.NewLine) != -1, "#10");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor3_InnerException_Null ()
		{
			BadImageFormatException bif = new BadImageFormatException ("message",
				(Exception) null);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
			Assert.IsNotNull (bif.Message, "#4");
			Assert.AreEqual ("message", bif.Message, "#5");
			Assert.IsNull (bif.FusionLog, "#6");
#if TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType().FullName + ": message"), "#7");
#else
			Assert.AreEqual (bif.GetType ().FullName + ": message",
				bif.ToString (), "#7");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor4 ()
		{
			BadImageFormatException bif = new BadImageFormatException ("message",
				"file.txt");

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNotNull (bif.FileName, "#2");
			Assert.AreEqual ("file.txt", bif.FileName, "#3");
			Assert.IsNull (bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual ("message", bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName
				+ ": message" + Environment.NewLine), "#8");
#if NET_2_0
			Assert.IsTrue (bif.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsFalse (bif.ToString ().IndexOf ("\"file.txt\"") != -1, "#9");
#else
			Assert.IsFalse (bif.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsTrue (bif.ToString ().IndexOf ("\"file.txt\"") != -1, "#10");
#endif
		}

		[Test]
		public void Constructor4_FileName_Empty ()
		{
			BadImageFormatException bif = new BadImageFormatException ("message",
				string.Empty);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNotNull (bif.FileName, "#2");
			Assert.AreEqual (string.Empty, bif.FileName, "#3");
			Assert.IsNull (bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual ("message", bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
#if TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType().FullName + ": message"), "#8");
#else
			Assert.AreEqual (bif.GetType ().FullName + ": message",
				bif.ToString (), "#8");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor4_FileName_Null ()
		{
			BadImageFormatException bif = new BadImageFormatException ("message",
				(string) null);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#A1");
#endif
			Assert.IsNull (bif.FileName, "#A2");
			Assert.IsNull (bif.InnerException, "#A3");
			Assert.IsNotNull (bif.Message, "#A4");
			Assert.AreEqual ("message", bif.Message, "#A5");
			Assert.IsNull (bif.FusionLog, "#A6");
#if TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType().FullName + ": message"), "#A7");
#else
			Assert.AreEqual (bif.GetType ().FullName + ": message",
				bif.ToString (), "#A7");
#endif // TARGET_JVM

			bif = new BadImageFormatException (string.Empty, (string) null);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#B1");
#endif
			Assert.IsNull (bif.FileName, "#B2");
			Assert.IsNull (bif.InnerException, "#B3");
			Assert.IsNotNull (bif.Message, "#B4");
			Assert.AreEqual (string.Empty, bif.Message, "#B5");
			Assert.IsNull (bif.FusionLog, "#B6");
#if TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType().FullName + ": "), "#B7");
#else
			Assert.AreEqual (bif.GetType ().FullName + ": ",
				bif.ToString (), "#B7");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor4_FileNameAndMessage_Empty ()
		{
			BadImageFormatException bif = new BadImageFormatException (string.Empty,
				string.Empty);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNotNull (bif.FileName, "#2");
			Assert.AreEqual (string.Empty, bif.FileName, "#3");
			Assert.IsNull (bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual (string.Empty, bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
#if TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType().FullName + ": "), "#8");
#else
			Assert.AreEqual (bif.GetType ().FullName + ": ", bif.ToString (), "#8");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor4_FileNameAndMessage_Null ()
		{
			BadImageFormatException bif = new BadImageFormatException ((string) null,
				(string) null);

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNull (bif.FileName, "#2");
			Assert.IsNull (bif.InnerException, "#3");
#if NET_2_0
			Assert.IsNotNull (bif.Message, "#4"); // Could not load file or assembly '' ...
			Assert.IsTrue (bif.Message.IndexOf ("''") != -1, "#5");
#else
			Assert.IsNotNull (bif.Message, "#4"); // Format of the executable (.exe) or library ...
			Assert.IsFalse (bif.Message.IndexOf ("''") != -1, "#5");
#endif
			Assert.IsNull (bif.FusionLog, "#5");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName
				+ ": "), "#6");
#if !TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsFalse (bif.ToString ().IndexOf (Environment.NewLine) != -1, "#7");
#endif // TARGET_JVM
		}

		[Test]
		public void Constructor4_Message_Empty ()
		{
			BadImageFormatException bif = null;
			
			bif = new BadImageFormatException (string.Empty, "file.txt");

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#1");
#endif
			Assert.IsNotNull (bif.FileName, "#2");
			Assert.AreEqual ("file.txt", bif.FileName, "#3");
			Assert.IsNull (bif.InnerException, "#4");
			Assert.IsNotNull (bif.Message, "#5");
			Assert.AreEqual (string.Empty, bif.Message, "#6");
			Assert.IsNull (bif.FusionLog, "#7");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName
				+ ": " + Environment.NewLine), "#8");
#if NET_2_0
			Assert.IsTrue (bif.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsFalse (bif.ToString ().IndexOf ("\"file.txt\"") != -1, "#10");
#else
			Assert.IsFalse (bif.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsTrue (bif.ToString ().IndexOf ("\"file.txt\"") != -1, "#10");
#endif
		}

		[Test]
		public void Constructor4_Message_Null ()
		{
			BadImageFormatException bif = null;
			
			bif = new BadImageFormatException ((string) null, "file.txt");

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#A1");
#endif
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

#if NET_2_0
			Assert.IsNotNull (bif.Data, "#B1");
#endif
			Assert.IsNotNull (bif.FileName, "#B2");
			Assert.AreEqual (string.Empty, bif.FileName, "#B3");
			Assert.IsNull (bif.InnerException, "#B4");
			// .NET 1.1: The format of the file 'file.txt' is invalid
			// .NET 2.0: Could not load file or assembly 'file.txt' or one of its ...
			Assert.IsNotNull (bif.Message, "#B5");
			Assert.IsNull (bif.FusionLog, "#B6");
			Assert.IsTrue (bif.ToString ().StartsWith (bif.GetType ().FullName
				+ ": "), "#B7");
#if !TARGET_JVM // ToString always has a stack trace under TARGET_JVM
			Assert.IsFalse (bif.ToString ().IndexOf (Environment.NewLine) != -1, "#B8");
#endif // TARGET_JVM
			Assert.IsTrue (bif.ToString ().IndexOf ("''") != -1, "#B9");
		}
	}
}
