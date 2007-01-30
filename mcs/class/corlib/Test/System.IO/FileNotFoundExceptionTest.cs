//
// FileNotFoundExceptionTest.cs - Unit tests for
//	System.IO.FileNotFoundException
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
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.IO {
	[TestFixture]
	public class FileNotFoundExceptionTest {
		[Test]
		public void Constructor1 ()
		{
			FileNotFoundException fnf = new FileNotFoundException ();

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNull (fnf.FileName, "#2");
			Assert.IsNull (fnf.InnerException, "#3");
			Assert.IsNotNull (fnf.Message, "#4"); // Unable to find the specified file
			Assert.IsNull (fnf.FusionLog, "#5");
			Assert.IsTrue (fnf.ToString ().StartsWith (fnf.GetType().FullName), "#6");
		}

		[Test]
		public void Constructor2 ()
		{
			FileNotFoundException fnf = new FileNotFoundException ("message");

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNull (fnf.FileName, "#2");
			Assert.IsNull (fnf.InnerException, "#3");
			Assert.IsNotNull (fnf.Message, "#4");
			Assert.AreEqual ("message", fnf.Message, "#5");
			Assert.IsNull (fnf.FusionLog, "#6");
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ": message"),"#7");
#else
			Assert.AreEqual (fnf.GetType ().FullName + ": message",
				fnf.ToString (), "#7");
#endif
		}

		[Test]
		public void Constructor2_Message_Empty ()
		{
			FileNotFoundException fnf = new FileNotFoundException (string.Empty);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNull (fnf.FileName, "#2");
			Assert.IsNull (fnf.InnerException, "#3");
			Assert.IsNotNull (fnf.Message, "#4");
			Assert.AreEqual (string.Empty, fnf.Message, "#5");
			Assert.IsNull (fnf.FusionLog, "#6");
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ": "), "#7");
#else
			Assert.AreEqual (fnf.GetType ().FullName + ": ",
				fnf.ToString (), "#7");
#endif
		}

		[Test]
		public void Constructor2_Message_Null ()
		{
			FileNotFoundException fnf = new FileNotFoundException ((string) null);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNull (fnf.FileName, "#2");
			Assert.IsNull (fnf.InnerException, "#3");
#if NET_2_0
			Assert.IsNull (fnf.Message, "#4");
#else
			Assert.IsNotNull (fnf.Message, "#4"); // File or assembly name (null), or ...
#endif
			Assert.IsNull (fnf.FusionLog, "#5");
#if NET_2_0 && !TARGET_JVM
			Assert.AreEqual (fnf.GetType ().FullName + ": ",
				fnf.ToString (), "#6");
#else
			Assert.IsTrue (fnf.ToString ().StartsWith (fnf.GetType ().FullName), "#6");
#endif
		}

		[Test]
		public void Constructor3 ()
		{
			ArithmeticException ame = new ArithmeticException ("something");
			FileNotFoundException fnf = new FileNotFoundException ("message",
				ame);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNull (fnf.FileName, "#2");
			Assert.IsNotNull (fnf.InnerException, "#3");
			Assert.AreSame (ame, fnf.InnerException, "#4");
			Assert.IsNotNull (fnf.Message, "#5");
			Assert.AreEqual ("message", fnf.Message, "#6");
			Assert.IsNull (fnf.FusionLog, "#7");
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ": message ---> "
                + ame.GetType().FullName + ": something"), "#8");
#else
			Assert.AreEqual (fnf.GetType ().FullName + ": message ---> "
				+ ame.GetType ().FullName + ": something", fnf.ToString (), "#8");
#endif
		}

		[Test]
		public void Constructor3_Message_Empty ()
		{
			ArithmeticException ame = new ArithmeticException ("something");
			FileNotFoundException fnf = new FileNotFoundException (string.Empty, ame);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNull (fnf.FileName, "#2");
			Assert.IsNotNull (fnf.InnerException, "#3");
			Assert.AreSame (ame, fnf.InnerException, "#4");
			Assert.IsNotNull (fnf.Message, "#5");
			Assert.AreEqual (string.Empty, fnf.Message, "#6");
			Assert.IsNull (fnf.FusionLog, "#7");
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ":  ---> "
                + ame.GetType().FullName + ": something"), "#8");
#else
			Assert.AreEqual (fnf.GetType ().FullName + ":  ---> "
				+ ame.GetType ().FullName + ": something", fnf.ToString (), "#8");
#endif
		}

		[Test]
		public void Constructor3_Message_Null ()
		{
			ArithmeticException ame = new ArithmeticException ("something");
			FileNotFoundException fnf = new FileNotFoundException ((string) null, ame);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNull (fnf.FileName, "#2");
			Assert.IsNotNull (fnf.InnerException, "#3");
			Assert.AreSame (ame, fnf.InnerException, "#4");
#if NET_2_0
			Assert.IsNull (fnf.Message, "#5");
#else
			Assert.IsNotNull (fnf.Message, "#5"); // File or assembly name (null), or ...
#endif
			Assert.IsNull (fnf.FusionLog, "#6");
#if NET_2_0
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ":  ---> "
                + ame.GetType().FullName + ": something"), "#7");
#else
			Assert.AreEqual (fnf.GetType ().FullName + ":  ---> "
				+ ame.GetType ().FullName + ": something", fnf.ToString (), "#7");
#endif
#else
			Assert.IsTrue (fnf.ToString ().StartsWith (fnf.GetType ().FullName), "#7");
			Assert.IsFalse (fnf.ToString ().IndexOf (Environment.NewLine) != -1, "#9");
#endif
		}

		[Test]
		public void Constructor3_InnerException_Null ()
		{
			FileNotFoundException fnf = new FileNotFoundException ("message",
				(Exception) null);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNull (fnf.FileName, "#2");
			Assert.IsNull (fnf.InnerException, "#3");
			Assert.IsNotNull (fnf.Message, "#4");
			Assert.AreEqual ("message", fnf.Message, "#5");
			Assert.IsNull (fnf.FusionLog, "#6");
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ": message"), "#7");
#else
			Assert.AreEqual (fnf.GetType ().FullName + ": message",
				fnf.ToString (), "#7");
#endif
		}

		[Test]
		public void Constructor4 ()
		{
			FileNotFoundException fnf = new FileNotFoundException ("message",
				"file.txt");

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNotNull (fnf.FileName, "#2");
			Assert.AreEqual ("file.txt", fnf.FileName, "#3");
			Assert.IsNull (fnf.InnerException, "#4");
			Assert.IsNotNull (fnf.Message, "#5");
			Assert.AreEqual ("message", fnf.Message, "#6");
			Assert.IsNull (fnf.FusionLog, "#7");
			Assert.IsTrue (fnf.ToString ().StartsWith (fnf.GetType ().FullName
				+ ": message" + Environment.NewLine), "#8");
#if NET_2_0
			Assert.IsTrue (fnf.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsFalse (fnf.ToString ().IndexOf ("\"file.txt\"") != -1, "#9");
#else
			Assert.IsFalse (fnf.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsTrue (fnf.ToString ().IndexOf ("\"file.txt\"") != -1, "#10");
#endif
		}

		[Test]
		public void Constructor4_FileName_Empty ()
		{
			FileNotFoundException fnf = new FileNotFoundException ("message",
				string.Empty);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNotNull (fnf.FileName, "#2");
			Assert.AreEqual (string.Empty, fnf.FileName, "#3");
			Assert.IsNull (fnf.InnerException, "#4");
			Assert.IsNotNull (fnf.Message, "#5");
			Assert.AreEqual ("message", fnf.Message, "#6");
			Assert.IsNull (fnf.FusionLog, "#7");
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ": message"), "#8");
#else
			Assert.AreEqual (fnf.GetType ().FullName + ": message",
				fnf.ToString (), "#8");
#endif
		}

		[Test]
		public void Constructor4_FileName_Null ()
		{
			FileNotFoundException fnf = new FileNotFoundException ("message",
				(string) null);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#A1");
#endif
			Assert.IsNull (fnf.FileName, "#A2");
			Assert.IsNull (fnf.InnerException, "#A3");
			Assert.IsNotNull (fnf.Message, "#A4");
			Assert.AreEqual ("message", fnf.Message, "#A5");
			Assert.IsNull (fnf.FusionLog, "#A6");
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ": message"), "#A7");
#else
		
			Assert.AreEqual (fnf.GetType ().FullName + ": message",
				fnf.ToString (), "#A7");
#endif

			fnf = new FileNotFoundException (string.Empty, (string) null);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#B1");
#endif
			Assert.IsNull (fnf.FileName, "#B2");
			Assert.IsNull (fnf.InnerException, "#B3");
			Assert.IsNotNull (fnf.Message, "#B4");
			Assert.AreEqual (string.Empty, fnf.Message, "#B5");
			Assert.IsNull (fnf.FusionLog, "#B6");
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ": "), "#B7");
#else
			Assert.AreEqual (fnf.GetType ().FullName + ": ",
				fnf.ToString (), "#B7");
#endif
		}

		[Test]
		public void Constructor4_FileNameAndMessage_Empty ()
		{
			FileNotFoundException fnf = new FileNotFoundException (string.Empty,
				string.Empty);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNotNull (fnf.FileName, "#2");
			Assert.AreEqual (string.Empty, fnf.FileName, "#3");
			Assert.IsNull (fnf.InnerException, "#4");
			Assert.IsNotNull (fnf.Message, "#5");
			Assert.AreEqual (string.Empty, fnf.Message, "#6");
			Assert.IsNull (fnf.FusionLog, "#7");
#if TARGET_JVM
            Assert.IsTrue(fnf.ToString().StartsWith(fnf.GetType().FullName + ": "), "#8");
#else
			Assert.AreEqual (fnf.GetType ().FullName + ": ", fnf.ToString (), "#8");
#endif
		}

		[Test]
		public void Constructor4_FileNameAndMessage_Null ()
		{
			FileNotFoundException fnf = new FileNotFoundException ((string) null,
				(string) null);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNull (fnf.FileName, "#2");
			Assert.IsNull (fnf.InnerException, "#3");
#if NET_2_0
			Assert.IsNull (fnf.Message, "#4");
#else
			Assert.IsNotNull (fnf.Message, "#4");
#endif
			Assert.IsNull (fnf.FusionLog, "#5");
			Assert.IsTrue (fnf.ToString ().StartsWith (fnf.GetType ().FullName
				+ ": "), "#6");
#if !TARGET_JVM
			Assert.IsFalse (fnf.ToString ().IndexOf (Environment.NewLine) != -1, "#7");
#endif
			Assert.IsFalse (fnf.ToString ().IndexOf ("''") != -1, "#8");
		}

		[Test]
		public void Constructor4_Message_Empty ()
		{
			FileNotFoundException fnf = null;
			
			fnf = new FileNotFoundException (string.Empty, "file.txt");

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#1");
#endif
			Assert.IsNotNull (fnf.FileName, "#2");
			Assert.AreEqual ("file.txt", fnf.FileName, "#3");
			Assert.IsNull (fnf.InnerException, "#4");
			Assert.IsNotNull (fnf.Message, "#5");
			Assert.AreEqual (string.Empty, fnf.Message, "#6");
			Assert.IsNull (fnf.FusionLog, "#7");
			Assert.IsTrue (fnf.ToString ().StartsWith (fnf.GetType ().FullName
				+ ": " + Environment.NewLine), "#8");
#if NET_2_0
			Assert.IsTrue (fnf.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsFalse (fnf.ToString ().IndexOf ("\"file.txt\"") != -1, "#10");
#else
			Assert.IsFalse (fnf.ToString ().IndexOf ("'file.txt'") != -1, "#9");
			Assert.IsTrue (fnf.ToString ().IndexOf ("\"file.txt\"") != -1, "#10");
#endif
		}

		[Test]
		public void Constructor4_Message_Null ()
		{
			FileNotFoundException fnf = null;
			
			fnf = new FileNotFoundException ((string) null, "file.txt");

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#A1");
#endif
			Assert.IsNotNull (fnf.FileName, "#A2");
			Assert.AreEqual ("file.txt", fnf.FileName, "#A3");
			Assert.IsNull (fnf.InnerException, "#A4");
			Assert.IsNotNull (fnf.Message, "#A5");
			Assert.IsNull (fnf.FusionLog, "#A6");
			Assert.IsTrue (fnf.ToString ().StartsWith (fnf.GetType ().FullName
				+ ": "), "#A7");
			Assert.IsTrue (fnf.ToString ().IndexOf (Environment.NewLine) != -1, "#A8");
#if NET_2_0
			Assert.IsTrue (fnf.ToString ().IndexOf ("'file.txt'") != -1, "#A9");
			Assert.IsFalse (fnf.ToString ().IndexOf ("\"file.txt\"") != -1, "#A10");
#else
			Assert.IsFalse (fnf.ToString ().IndexOf ("'file.txt'") != -1, "#A9");
			Assert.IsTrue (fnf.ToString ().IndexOf ("\"file.txt\"") != -1, "#A10");
#endif

			fnf = new FileNotFoundException ((string) null, string.Empty);

#if NET_2_0
			Assert.IsNotNull (fnf.Data, "#B1");
#endif
			Assert.IsNotNull (fnf.FileName, "#B2");
			Assert.AreEqual (string.Empty, fnf.FileName, "#B3");
			Assert.IsNull (fnf.InnerException, "#B4");
			// .NET 1.1: File or assembly name , or one of its dependencies ...
			// .NET 2.0: Could not load file or assembly '' or one of its ...
			Assert.IsNotNull (fnf.Message, "#B5");
			Assert.IsNull (fnf.FusionLog, "#B6");
			Assert.IsTrue (fnf.ToString ().StartsWith (fnf.GetType ().FullName
				+ ": "), "#B7");
#if !TARGET_JVM
			Assert.IsFalse (fnf.ToString ().IndexOf (Environment.NewLine) != -1, "#B8");
#endif
#if NET_2_0
			Assert.IsTrue (fnf.ToString ().IndexOf ("''") != -1, "#B9");
#else
			Assert.IsFalse (fnf.ToString ().IndexOf ("''") != -1, "#B9");
			Assert.IsFalse (fnf.ToString ().IndexOf ("\"\"") != -1, "#B10");
#endif
		}
	}
}
