//
// ConfigurationExceptionTest.cs
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2008 Gert Driesen
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
using System.Configuration;
using System.IO;
using System.Xml;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Configuration
{
	[TestFixture]
	public class ConfigurationExceptionTest
	{
		private TempDirectory temp;
		private string foldername;

		[SetUp]
		public void SetUp ()
		{
			temp = new TempDirectory ();
			foldername = temp.Path;
		}

		[TearDown]
		public void TearDown ()
		{
			temp.Dispose ();
		}

		[Test] // ctor ()
		public void Constructor1 ()
		{
			ConfigurationException ce = new ConfigurationException ();
			Assert.IsNotNull (ce.BareMessage, "#1");
			Assert.IsTrue (ce.BareMessage.IndexOf ("'" + typeof (ConfigurationException).FullName + "'") != -1, "#2:" + ce.BareMessage);
			Assert.IsNotNull (ce.Data, "#3");
			Assert.AreEqual (0, ce.Data.Count, "#4");
			Assert.IsNull (ce.Filename, "#5");
			Assert.IsNull (ce.InnerException, "#6");
			Assert.AreEqual (0, ce.Line, "#7");
			Assert.AreEqual (ce.BareMessage, ce.Message, "#8");
		}

		[Test] // ctor (String)
		public void Constructor2 ()
		{
			string msg;
			ConfigurationException ce;

			msg = "MSG";
			ce = new ConfigurationException (msg);
			Assert.AreSame (msg, ce.BareMessage, "#A1");
			Assert.IsNotNull (ce.Data, "#A2");
			Assert.AreEqual (0, ce.Data.Count, "#A3");
			Assert.IsNull (ce.Filename, "#A4");
			Assert.IsNull (ce.InnerException, "#A5");
			Assert.AreEqual (0, ce.Line, "#A6");
			Assert.AreSame (msg, ce.Message, "#A7");

			msg = null;
			ce = new ConfigurationException (msg);

			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#B1");
			Assert.IsNotNull (ce.Data, "#B2");
			Assert.AreEqual (0, ce.Data.Count, "#B3");
			Assert.IsNull (ce.Filename, "#B4");
			Assert.IsNull (ce.InnerException, "#B5");
			Assert.AreEqual (0, ce.Line, "#B6");
			Assert.AreEqual (ce.BareMessage, ce.Message, "#B7");
		}

		[Test] // ctor (String, Exception)
		public void Constructor3 ()
		{
			string msg;
			Exception inner;
			ConfigurationException ce;

			msg = "MSG";
			inner = new Exception ();
			ce = new ConfigurationException (msg, inner);
			Assert.AreSame (msg, ce.BareMessage, "#A1");
			Assert.IsNotNull (ce.Data, "#A2");
			Assert.AreEqual (0, ce.Data.Count, "#A3");
			Assert.IsNull (ce.Filename, "#A4");
			Assert.AreSame (inner, ce.InnerException, "#A5");
			Assert.AreEqual (0, ce.Line, "#A6");
			Assert.AreSame (msg, ce.Message, "#A7");

			msg = null;
			inner = null;
			ce = new ConfigurationException (msg, inner);
			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#B1");
			Assert.IsNotNull (ce.Data, "#B2");
			Assert.AreEqual (0, ce.Data.Count, "#B3");
			Assert.IsNull (ce.Filename, "#B4");
			Assert.AreSame (inner, ce.InnerException, "#B5");
			Assert.AreEqual (0, ce.Line, "#B6");
			Assert.AreEqual (ce.BareMessage, ce.Message, "#B7");
		}

		[Test] // ctor (String, XmlNode)
		public void Constructor4 ()
		{
			string msg;
			XmlNode node;
			ConfigurationException ce;

			msg = "MSG";
			node = new XmlDocument ();
			ce = new ConfigurationException (msg, node);
			Assert.AreSame (msg, ce.BareMessage, "#A1");
			Assert.IsNotNull (ce.Data, "#A2");
			Assert.AreEqual (0, ce.Data.Count, "#A3");
			Assert.AreEqual (string.Empty, ce.Filename, "#A4");
			Assert.IsNull (ce.InnerException, "#A5");
			Assert.AreEqual (0, ce.Line, "#A6");
			Assert.AreSame (msg, ce.Message, "#A7");

			msg = null;
			node = null;
			ce = new ConfigurationException (msg, node);
			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#B1");
			Assert.IsNotNull (ce.Data, "#B2");
			Assert.AreEqual (0, ce.Data.Count, "#B3");
			Assert.AreEqual (string.Empty, ce.Filename, "#B4");
			Assert.IsNull (ce.InnerException, "#B5");
			Assert.AreEqual (0, ce.Line, "#B6");
			Assert.AreEqual (ce.BareMessage, ce.Message, "#B7");
		}

		[Test] // ctor (String, Exception, XmlNode)
		public void Constructor5 ()
		{
			string msg;
			Exception inner;
			XmlNode node;
			ConfigurationException ce;

			msg = "MSG";
			inner = new Exception ();
			node = new XmlDocument ();
			ce = new ConfigurationException (msg, inner, node);
			Assert.AreSame (msg, ce.BareMessage, "#A1");
			Assert.IsNotNull (ce.Data, "#A2");
			Assert.AreEqual (0, ce.Data.Count, "#A3");
			Assert.AreEqual (string.Empty, ce.Filename, "#A4");
			Assert.AreSame (inner, ce.InnerException, "#A5");
			Assert.AreEqual (0, ce.Line, "#A6");
			Assert.AreSame (msg, ce.Message, "#A7");

			msg = null;
			inner = null;
			node = null;
			ce = new ConfigurationException (msg, inner, node);
			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#B1");
			Assert.IsNotNull (ce.Data, "#B2");
			Assert.AreEqual (0, ce.Data.Count, "#B3");
			Assert.AreEqual (string.Empty, ce.Filename, "#B4");
			Assert.AreSame (inner, ce.InnerException, "#B5");
			Assert.AreEqual (0, ce.Line, "#B6");
			Assert.AreEqual (ce.BareMessage, ce.Message, "#B7");
		}

		[Test] // ctor (String, String, Int32)
		public void Constructor6 ()
		{
			string msg;
			string filename;
			int line;
			ConfigurationException ce;

			msg = "MSG";
			filename = "abc.txt";
			line = 7;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#A1");
			Assert.IsNotNull (ce.Data, "#A2");
			Assert.AreEqual (0, ce.Data.Count, "#A3");
			Assert.AreSame (filename, ce.Filename, "#A4");
			Assert.IsNull (ce.InnerException, "#A5");
			Assert.AreEqual (line, ce.Line, "#A6");
			Assert.AreEqual ("MSG (abc.txt line 7)", ce.Message, "#A7");

			msg = null;
			filename = null;
			line = 0;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#B1");
			Assert.IsNotNull (ce.Data, "#B3");
			Assert.AreEqual (0, ce.Data.Count, "#B4");
			Assert.AreSame (filename, ce.Filename, "#B5");
			Assert.IsNull (ce.InnerException, "#B6");
			Assert.AreEqual (0, ce.Line, "#B7");
			Assert.AreEqual (ce.BareMessage, ce.Message, "#B8");

			msg = null;
			filename = "abc.txt";
			line = 5;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#C1");
			Assert.IsNotNull (ce.Data, "#C2");
			Assert.AreEqual (0, ce.Data.Count, "#C3");
			Assert.AreSame (filename, ce.Filename, "#C4");
			Assert.IsNull (ce.InnerException, "#C5");
			Assert.AreEqual (5, ce.Line, "#C6");
			Assert.AreEqual (ce.BareMessage + " (abc.txt line 5)", ce.Message, "#C7");

			msg = "MSG";
			filename = null;
			line = 5;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#D1");
			Assert.IsNotNull (ce.Data, "#D2");
			Assert.AreEqual (0, ce.Data.Count, "#D3");
			Assert.AreSame (filename, ce.Filename, "#D4");
			Assert.IsNull (ce.InnerException, "#D5");
			Assert.AreEqual (5, ce.Line, "#D6");
			Assert.AreEqual (msg + " (line 5)", ce.Message, "#D7");

			msg = "MSG";
			filename = "abc.txt";
			line = 0;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#E1");
			Assert.IsNotNull (ce.Data, "#E2");
			Assert.AreEqual (0, ce.Data.Count, "#E3");
			Assert.AreSame (filename, ce.Filename, "#E4");
			Assert.IsNull (ce.InnerException, "#E5");
			Assert.AreEqual (0, ce.Line, "#E6");
			Assert.AreEqual (msg + " (abc.txt)", ce.Message, "#E7");

			msg = null;
			filename = null;
			line = 4;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#F1");
			Assert.IsNotNull (ce.Data, "#F2");
			Assert.AreEqual (0, ce.Data.Count, "#F3");
			Assert.AreSame (filename, ce.Filename, "#F4");
			Assert.IsNull (ce.InnerException, "#F5");
			Assert.AreEqual (4, ce.Line, "#F6");
			Assert.AreEqual (ce.BareMessage + " (line 4)", ce.Message, "#F7");

			msg = string.Empty;
			filename = string.Empty;
			line = 0;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#G1");
			Assert.IsNotNull (ce.Data, "#G2");
			Assert.AreEqual (0, ce.Data.Count, "#G3");
			Assert.AreSame (filename, ce.Filename, "#G4");
			Assert.IsNull (ce.InnerException, "#G5");
			Assert.AreEqual (0, ce.Line, "#G6");
			Assert.AreSame (msg, ce.Message, "#G7");

			msg = string.Empty;
			filename = "abc.txt";
			line = 6;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#H1");
			Assert.IsNotNull (ce.Data, "#H2");
			Assert.AreEqual (0, ce.Data.Count, "#H3");
			Assert.AreSame (filename, ce.Filename, "#H4");
			Assert.IsNull (ce.InnerException, "#H5");
			Assert.AreEqual (6, ce.Line, "#H6");
			Assert.AreEqual (msg + " (abc.txt line 6)", ce.Message, "#H7");

			msg = "MSG";
			filename = string.Empty;
			line = 6;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#I1");
			Assert.IsNotNull (ce.Data, "#I2");
			Assert.AreEqual (0, ce.Data.Count, "#I3");
			Assert.AreSame (filename, ce.Filename, "#I4");
			Assert.IsNull (ce.InnerException, "#I5");
			Assert.AreEqual (6, ce.Line, "#I6");
			Assert.AreEqual (msg + " (line 6)", ce.Message, "#I7");

			msg = string.Empty;
			filename = string.Empty;
			line = 4;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#J1");
			Assert.IsNotNull (ce.Data, "#J2");
			Assert.AreEqual (0, ce.Data.Count, "#J3");
			Assert.AreSame (filename, ce.Filename, "#J4");
			Assert.IsNull (ce.InnerException, "#J5");
			Assert.AreEqual (4, ce.Line, "#J6");
			Assert.AreEqual (msg + " (line 4)", ce.Message, "#J7");

			msg = "MSG";
			filename = string.Empty;
			line = 0;
			ce = new ConfigurationException (msg, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#K1");
			Assert.IsNotNull (ce.Data, "#K2");
			Assert.AreEqual (0, ce.Data.Count, "#K3");
			Assert.AreSame (filename, ce.Filename, "#K4");
			Assert.IsNull (ce.InnerException, "#K5");
			Assert.AreEqual (0, ce.Line, "#K6");
			Assert.AreEqual (msg, ce.Message, "#K7");
		}

		[Test] // ctor (String, Exception, String, Int32)
		public void Constructor7 ()
		{
			string msg;
			Exception inner;
			string filename;
			int line;
			ConfigurationException ce;

			msg = "MSG";
			inner = new Exception ();
			filename = "abc.txt";
			line = 7;
			ce = new ConfigurationException (msg, inner, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#A1");
			Assert.IsNotNull (ce.Data, "#A2");
			Assert.AreEqual (0, ce.Data.Count, "#A3");
			Assert.AreSame (filename, ce.Filename, "#A4");
			Assert.AreSame (inner, ce.InnerException, "#A5");
			Assert.AreEqual (line, ce.Line, "#A6");
			Assert.AreEqual (msg + " (abc.txt line 7)", ce.Message, "#A7");

			msg = null;
			inner = null;
			filename = null;
			line = 0;
			ce = new ConfigurationException (msg, inner, filename, line);
			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#B1");
			Assert.IsNotNull (ce.Data, "#B2");
			Assert.AreEqual (0, ce.Data.Count, "#B3");
			Assert.AreSame (filename, ce.Filename, "#B4");
			Assert.AreSame (inner, ce.InnerException, "#B5");
			Assert.AreEqual (0, ce.Line, "#B6");
			Assert.AreEqual (ce.BareMessage, ce.Message, "#B7");

			msg = null;
			inner = new Exception ();
			filename = null;
			line = 7;
			ce = new ConfigurationException (msg, inner, filename, line);
			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#C1");
			Assert.IsNotNull (ce.Data, "#C2");
			Assert.AreEqual (0, ce.Data.Count, "#C3");
			Assert.AreSame (filename, ce.Filename, "#C4");
			Assert.AreSame (inner, ce.InnerException, "#C5");
			Assert.AreEqual (line, ce.Line, "#C6");
			Assert.AreEqual (ce.BareMessage + " (line 7)", ce.Message, "#C7");

			msg = string.Empty;
			inner = new Exception ();
			filename = string.Empty;
			line = 7;
			ce = new ConfigurationException (msg, inner, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#D1");
			Assert.IsNotNull (ce.Data, "#D2");
			Assert.AreEqual (0, ce.Data.Count, "#D3");
			Assert.AreSame (filename, ce.Filename, "#D4");
			Assert.AreSame (inner, ce.InnerException, "#D5");
			Assert.AreEqual (line, ce.Line, "#D6");
			Assert.AreEqual (" (line 7)", ce.Message, "#D7");

			msg = string.Empty;
			inner = new Exception ();
			filename = "abc.txt";
			line = 7;
			ce = new ConfigurationException (msg, inner, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#E1");
			Assert.IsNotNull (ce.Data, "#E2");
			Assert.AreEqual (0, ce.Data.Count, "#E3");
			Assert.AreSame (filename, ce.Filename, "#E4");
			Assert.AreSame (inner, ce.InnerException, "#E5");
			Assert.AreEqual (line, ce.Line, "#E6");
			Assert.AreEqual (" (abc.txt line 7)", ce.Message, "#E7");

			msg = "MSG";
			inner = new Exception ();
			filename = null;
			line = 7;
			ce = new ConfigurationException (msg, inner, filename, line);
			Assert.AreSame (msg, ce.BareMessage, "#F1");
			Assert.IsNotNull (ce.Data, "#F2");
			Assert.AreEqual (0, ce.Data.Count, "#F3");
			Assert.AreSame (filename, ce.Filename, "#F4");
			Assert.AreSame (inner, ce.InnerException, "#F5");
			Assert.AreEqual (line, ce.Line, "#F6");
			Assert.AreEqual (ce.BareMessage + " (line 7)", ce.Message, "#F7");

			msg = null;
			inner = new Exception ();
			filename = "abc.txt";
			line = 7;
			ce = new ConfigurationException (msg, inner, filename, line);
			Assert.AreEqual (new ConfigurationException ().Message, ce.BareMessage, "#G1");
			Assert.IsNotNull (ce.Data, "#G2");
			Assert.AreEqual (0, ce.Data.Count, "#G3");
			Assert.AreSame (filename, ce.Filename, "#G4");
			Assert.AreSame (inner, ce.InnerException, "#G5");
			Assert.AreEqual (line, ce.Line, "#G6");
			Assert.AreEqual (ce.BareMessage + " (abc.txt line 7)", ce.Message, "#G7");
		}
	}
}
