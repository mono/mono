//
// ConfigurationErrorsExceptionTest.cs
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

#if NET_2_0

using System;
using System.Configuration;
using System.Configuration.Internal;
using System.IO;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Configuration
{
	[TestFixture]
	public class ConfigurationErrorsExceptionTest
	{
		private string foldername;

		[SetUp]
		public void SetUp ()
		{
			foldername = Path.Combine (Path.GetTempPath (),
				this.GetType ().FullName);
			if (!Directory.Exists (foldername))
				Directory.CreateDirectory (foldername);
		}

		[TearDown]
		public void TearDown ()
		{
			if (Directory.Exists (foldername))
				Directory.Delete (foldername, true);
		}

		[Test] // ctor ()
		public void Constructor1 ()
		{
			ConfigurationErrorsException cee = new ConfigurationErrorsException ();
			Assert.IsNotNull (cee.BareMessage, "#1");
			Assert.IsTrue (cee.BareMessage.IndexOf ("'" + typeof (ConfigurationErrorsException).FullName + "'") != -1, "#2:" + cee.BareMessage);
			Assert.IsNotNull (cee.Data, "#3");
			Assert.AreEqual (0, cee.Data.Count, "#4");
			Assert.IsNull (cee.Filename, "#5");
			Assert.IsNull (cee.InnerException, "#6");
			Assert.AreEqual (0, cee.Line, "#7");
			Assert.AreEqual (cee.BareMessage, cee.Message, "#8");
		}

		[Test] // ctor (String)
		public void Constructor2 ()
		{
			string msg;
			ConfigurationErrorsException cee;

			msg = "MSG";
			cee = new ConfigurationErrorsException (msg);
			Assert.AreSame (msg, cee.BareMessage, "#A1");
			Assert.IsNotNull (cee.Data, "#A2");
			Assert.AreEqual (0, cee.Data.Count, "#A3");
			Assert.IsNull (cee.Filename, "#A4");
			Assert.IsNull (cee.InnerException, "#A5");
			Assert.AreEqual (0, cee.Line, "#A6");
			Assert.AreSame (msg, cee.Message, "#A7");

			msg = null;
			cee = new ConfigurationErrorsException (msg);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#B1");
			Assert.IsNotNull (cee.Data, "#B2");
			Assert.AreEqual (0, cee.Data.Count, "#B3");
			Assert.IsNull (cee.Filename, "#B4");
			Assert.IsNull (cee.InnerException, "#B5");
			Assert.AreEqual (0, cee.Line, "#B6");
			Assert.AreEqual (cee.BareMessage, cee.Message, "#B7");
		}

		[Test] // ctor (String, Exception)
		public void Constructor3 ()
		{
			string msg;
			Exception inner;
			ConfigurationErrorsException cee;

			msg = "MSG";
			inner = new Exception ();
			cee = new ConfigurationErrorsException (msg, inner);
			Assert.AreSame (msg, cee.BareMessage, "#A1");
			Assert.IsNotNull (cee.Data, "#A2");
			Assert.AreEqual (0, cee.Data.Count, "#A3");
			Assert.IsNull (cee.Filename, "#A4");
			Assert.AreSame (inner, cee.InnerException, "#A5");
			Assert.AreEqual (0, cee.Line, "#A6");
			Assert.AreSame (msg, cee.Message, "#A7");

			msg = null;
			inner = null;
			cee = new ConfigurationErrorsException (msg, inner);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#B1");
			Assert.IsNotNull (cee.Data, "#B2");
			Assert.AreEqual (0, cee.Data.Count, "#B3");
			Assert.IsNull (cee.Filename, "#B4");
			Assert.AreSame (inner, cee.InnerException, "#B5");
			Assert.AreEqual (0, cee.Line, "#B6");
			Assert.AreEqual (cee.BareMessage, cee.Message, "#B7");
		}

		[Test] // ctor (String, XmlNode)
		public void Constructor4 ()
		{
			string msg;
			XmlNode node;
			ConfigurationErrorsException cee;

			string xmlfile = Path.Combine (foldername, "test.xml");
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("test"));
			doc.Save (xmlfile);

			msg = "MSG";
			node = new XmlDocument ();
			cee = new ConfigurationErrorsException (msg, node);
			Assert.AreSame (msg, cee.BareMessage, "#A1");
			Assert.IsNotNull (cee.Data, "#A2");
			Assert.AreEqual (0, cee.Data.Count, "#A3");
			Assert.IsNull (cee.Filename, "#A4");
			Assert.IsNull (cee.InnerException, "#A5");
			Assert.AreEqual (0, cee.Line, "#A6");
			Assert.AreSame (msg, cee.Message, "#A7");

			doc = new XmlDocument ();
			doc.Load (xmlfile);

			msg = "MSG";
			node = doc.DocumentElement;
			cee = new ConfigurationErrorsException (msg, node);
			Assert.AreSame (msg, cee.BareMessage, "#B1");
			Assert.IsNotNull (cee.Data, "#B2");
			Assert.AreEqual (0, cee.Data.Count, "#B3");
			Assert.IsNull (cee.Filename, "#B4");
			Assert.IsNull (cee.InnerException, "#B5");
			Assert.AreEqual (0, cee.Line, "#B6");
			Assert.AreSame (msg, cee.Message, "#B7");

			doc = new ConfigXmlDocument ();
			doc.Load (xmlfile);

			msg = "MSG";
			node = doc.DocumentElement;
			cee = new ConfigurationErrorsException (msg, node);
			Assert.AreSame (msg, cee.BareMessage, "#C1");
			Assert.IsNotNull (cee.Data, "#C2");
			Assert.AreEqual (0, cee.Data.Count, "#C3");
			Assert.AreEqual (xmlfile, cee.Filename, "#C4");
			Assert.IsNull (cee.InnerException, "#C5");
			Assert.AreEqual (1, cee.Line, "#C6");
			Assert.AreEqual (msg + " (" + xmlfile + " line 1)", cee.Message, "#C7");

			msg = null;
			node = null;
			cee = new ConfigurationErrorsException (msg, node);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#D1");
			Assert.IsNotNull (cee.Data, "#D2");
			Assert.AreEqual (0, cee.Data.Count, "#D3");
			Assert.IsNull (cee.Filename, "#D4");
			Assert.IsNull (cee.InnerException, "#D5");
			Assert.AreEqual (0, cee.Line, "#D6");
			Assert.AreEqual (cee.BareMessage, cee.Message, "#D7");
		}

		[Test] // ctor (String, Exception, XmlNode)
		public void Constructor6 ()
		{
			string msg;
			Exception inner;
			XmlNode node;
			ConfigurationErrorsException cee;

			string xmlfile = Path.Combine (foldername, "test.xml");
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("test"));
			doc.Save (xmlfile);

			msg = "MSG";
			inner = new Exception ();
			node = new XmlDocument ();
			cee = new ConfigurationErrorsException (msg, inner, node);
			Assert.AreSame (msg, cee.BareMessage, "#A1");
			Assert.IsNotNull (cee.Data, "#A2");
			Assert.AreEqual (0, cee.Data.Count, "#A3");
			Assert.IsNull (cee.Filename, "#A4");
			Assert.AreSame (inner, cee.InnerException, "#A5");
			Assert.AreEqual (0, cee.Line, "#A6");
			Assert.AreSame (msg, cee.Message, "#A7");

			doc = new XmlDocument ();
			doc.Load (xmlfile);

			msg = null;
			inner = null;
			node = doc.DocumentElement;
			cee = new ConfigurationErrorsException (msg, inner, node);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#B1");
			Assert.IsNotNull (cee.Data, "#B2");
			Assert.AreEqual (0, cee.Data.Count, "#B3");
			Assert.IsNull (cee.Filename, "#B4");
			Assert.AreSame (inner, cee.InnerException, "#B5");
			Assert.AreEqual (0, cee.Line, "#B6");
			Assert.AreEqual (cee.BareMessage, cee.Message, "#B7");

			doc = new ConfigXmlDocument ();
			doc.Load (xmlfile);

			msg = null;
			inner = null;
			node = doc.DocumentElement;
			cee = new ConfigurationErrorsException (msg, inner, node);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#C1");
			Assert.IsNotNull (cee.Data, "#C2");
			Assert.AreEqual (0, cee.Data.Count, "#C3");
			Assert.AreEqual (xmlfile, cee.Filename, "#C4");
			Assert.AreSame (inner, cee.InnerException, "#C5");
			Assert.AreEqual (1, cee.Line, "#C6");
			Assert.AreEqual (cee.BareMessage + " (" + xmlfile + " line 1)", cee.Message, "#C7");

			msg = null;
			inner = null;
			node = null;
			cee = new ConfigurationErrorsException (msg, inner, node);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#D1");
			Assert.IsNotNull (cee.Data, "#D2");
			Assert.AreEqual (0, cee.Data.Count, "#D3");
			Assert.IsNull (cee.Filename, "#D4");
			Assert.AreSame (inner, cee.InnerException, "#D5");
			Assert.AreEqual (0, cee.Line, "#D6");
			Assert.AreEqual (cee.BareMessage, cee.Message, "#D7");
		}

		[Test] // ctor (String, String, Int32)
		public void Constructor8 ()
		{
			string msg;
			string filename;
			int line;
			ConfigurationErrorsException cee;

			msg = "MSG";
			filename = "abc.txt";
			line = 7;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#A1");
			Assert.IsNotNull (cee.Data, "#A2");
			Assert.AreEqual (0, cee.Data.Count, "#A3");
			Assert.AreSame (filename, cee.Filename, "#A4");
			Assert.IsNull (cee.InnerException, "#A5");
			Assert.AreEqual (line, cee.Line, "#A6");
			Assert.AreEqual ("MSG (abc.txt line 7)", cee.Message, "#A7");

			msg = null;
			filename = null;
			line = 0;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#B1");
			Assert.IsNotNull (cee.Data, "#B3");
			Assert.AreEqual (0, cee.Data.Count, "#B4");
			Assert.AreSame (filename, cee.Filename, "#B5");
			Assert.IsNull (cee.InnerException, "#B6");
			Assert.AreEqual (0, cee.Line, "#B7");
			Assert.AreEqual (cee.BareMessage, cee.Message, "#B8");

			msg = null;
			filename = "abc.txt";
			line = 5;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#B1");
			Assert.IsNotNull (cee.Data, "#C2");
			Assert.AreEqual (0, cee.Data.Count, "#C3");
			Assert.AreSame (filename, cee.Filename, "#C4");
			Assert.IsNull (cee.InnerException, "#C5");
			Assert.AreEqual (5, cee.Line, "#C6");
			Assert.AreEqual (cee.BareMessage + " (abc.txt line 5)", cee.Message, "#C7");

			msg = "MSG";
			filename = null;
			line = 5;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#D1");
			Assert.IsNotNull (cee.Data, "#D2");
			Assert.AreEqual (0, cee.Data.Count, "#D3");
			Assert.AreSame (filename, cee.Filename, "#D4");
			Assert.IsNull (cee.InnerException, "#D5");
			Assert.AreEqual (5, cee.Line, "#D6");
			Assert.AreEqual (msg + " (line 5)", cee.Message, "#D7");

			msg = "MSG";
			filename = "abc.txt";
			line = 0;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#E1");
			Assert.IsNotNull (cee.Data, "#E2");
			Assert.AreEqual (0, cee.Data.Count, "#E3");
			Assert.AreSame (filename, cee.Filename, "#E4");
			Assert.IsNull (cee.InnerException, "#E5");
			Assert.AreEqual (0, cee.Line, "#E6");
			Assert.AreEqual (msg + " (abc.txt)", cee.Message, "#E7");

			msg = null;
			filename = null;
			line = 4;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#F1");
			Assert.IsNotNull (cee.Data, "#F2");
			Assert.AreEqual (0, cee.Data.Count, "#F3");
			Assert.AreSame (filename, cee.Filename, "#F4");
			Assert.IsNull (cee.InnerException, "#F5");
			Assert.AreEqual (4, cee.Line, "#F6");
			Assert.AreEqual (cee.BareMessage + " (line 4)", cee.Message, "#F7");

			msg = string.Empty;
			filename = string.Empty;
			line = 0;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#G1");
			Assert.IsNotNull (cee.Data, "#G2");
			Assert.AreEqual (0, cee.Data.Count, "#G3");
			Assert.AreSame (filename, cee.Filename, "#G4");
			Assert.IsNull (cee.InnerException, "#G5");
			Assert.AreEqual (0, cee.Line, "#G6");
			Assert.AreSame (msg, cee.Message, "#G7");

			msg = string.Empty;
			filename = "abc.txt";
			line = 6;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#H1");
			Assert.IsNotNull (cee.Data, "#H2");
			Assert.AreEqual (0, cee.Data.Count, "#H3");
			Assert.AreSame (filename, cee.Filename, "#H4");
			Assert.IsNull (cee.InnerException, "#H5");
			Assert.AreEqual (6, cee.Line, "#H6");
			Assert.AreEqual (msg + " (abc.txt line 6)", cee.Message, "#H7");

			msg = "MSG";
			filename = string.Empty;
			line = 6;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#I1");
			Assert.IsNotNull (cee.Data, "#I2");
			Assert.AreEqual (0, cee.Data.Count, "#I3");
			Assert.AreSame (filename, cee.Filename, "#I4");
			Assert.IsNull (cee.InnerException, "#I5");
			Assert.AreEqual (6, cee.Line, "#I6");
			Assert.AreEqual (msg + " (line 6)", cee.Message, "#I7");

			msg = string.Empty;
			filename = string.Empty;
			line = 4;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#J1");
			Assert.IsNotNull (cee.Data, "#J2");
			Assert.AreEqual (0, cee.Data.Count, "#J3");
			Assert.AreSame (filename, cee.Filename, "#J4");
			Assert.IsNull (cee.InnerException, "#J5");
			Assert.AreEqual (4, cee.Line, "#J6");
			Assert.AreEqual (msg + " (line 4)", cee.Message, "#J7");

			msg = "MSG";
			filename = string.Empty;
			line = 0;
			cee = new ConfigurationErrorsException (msg, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#K1");
			Assert.IsNotNull (cee.Data, "#K2");
			Assert.AreEqual (0, cee.Data.Count, "#K3");
			Assert.AreSame (filename, cee.Filename, "#K4");
			Assert.IsNull (cee.InnerException, "#K5");
			Assert.AreEqual (0, cee.Line, "#K6");
			Assert.AreEqual (msg, cee.Message, "#K7");
		}

		[Test] // ctor (String, Exception, String, Int32)
		public void Constructor9 ()
		{
			string msg;
			Exception inner;
			string filename;
			int line;
			ConfigurationErrorsException cee;

			msg = "MSG";
			inner = new Exception ();
			filename = "abc.txt";
			line = 7;
			cee = new ConfigurationErrorsException (msg, inner, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#A1");
			Assert.IsNotNull (cee.Data, "#A2");
			Assert.AreEqual (0, cee.Data.Count, "#A3");
			Assert.AreSame (filename, cee.Filename, "#A4");
			Assert.AreSame (inner, cee.InnerException, "#A5");
			Assert.AreEqual (line, cee.Line, "#A6");
			Assert.AreEqual (msg + " (abc.txt line 7)", cee.Message, "#A7");

			msg = null;
			inner = null;
			filename = null;
			line = 0;
			cee = new ConfigurationErrorsException (msg, inner, filename, line);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#B1");
			Assert.IsNotNull (cee.Data, "#B2");
			Assert.AreEqual (0, cee.Data.Count, "#B3");
			Assert.AreSame (filename, cee.Filename, "#B4");
			Assert.AreSame (inner, cee.InnerException, "#B5");
			Assert.AreEqual (0, cee.Line, "#B6");
			Assert.AreEqual (cee.BareMessage, cee.Message, "#B7");

			msg = null;
			inner = new Exception ();
			filename = null;
			line = 7;
			cee = new ConfigurationErrorsException (msg, inner, filename, line);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#C1");
			Assert.IsNotNull (cee.Data, "#C2");
			Assert.AreEqual (0, cee.Data.Count, "#C3");
			Assert.AreSame (filename, cee.Filename, "#C4");
			Assert.AreSame (inner, cee.InnerException, "#C5");
			Assert.AreEqual (line, cee.Line, "#C6");
			Assert.AreEqual (cee.BareMessage + " (line 7)", cee.Message, "#C7");

			msg = string.Empty;
			inner = new Exception ();
			filename = string.Empty;
			line = 7;
			cee = new ConfigurationErrorsException (msg, inner, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#D1");
			Assert.IsNotNull (cee.Data, "#D2");
			Assert.AreEqual (0, cee.Data.Count, "#D3");
			Assert.AreSame (filename, cee.Filename, "#D4");
			Assert.AreSame (inner, cee.InnerException, "#D5");
			Assert.AreEqual (line, cee.Line, "#D6");
			Assert.AreEqual (" (line 7)", cee.Message, "#D7");

			msg = string.Empty;
			inner = new Exception ();
			filename = "abc.txt";
			line = 7;
			cee = new ConfigurationErrorsException (msg, inner, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#E1");
			Assert.IsNotNull (cee.Data, "#E2");
			Assert.AreEqual (0, cee.Data.Count, "#E3");
			Assert.AreSame (filename, cee.Filename, "#E4");
			Assert.AreSame (inner, cee.InnerException, "#E5");
			Assert.AreEqual (line, cee.Line, "#E6");
			Assert.AreEqual (" (abc.txt line 7)", cee.Message, "#E7");

			msg = "MSG";
			inner = new Exception ();
			filename = null;
			line = 7;
			cee = new ConfigurationErrorsException (msg, inner, filename, line);
			Assert.AreSame (msg, cee.BareMessage, "#F1");
			Assert.IsNotNull (cee.Data, "#F2");
			Assert.AreEqual (0, cee.Data.Count, "#F3");
			Assert.AreSame (filename, cee.Filename, "#F4");
			Assert.AreSame (inner, cee.InnerException, "#F5");
			Assert.AreEqual (line, cee.Line, "#F6");
			Assert.AreEqual (cee.BareMessage + " (line 7)", cee.Message, "#F7");

			msg = null;
			inner = new Exception ();
			filename = "abc.txt";
			line = 7;
			cee = new ConfigurationErrorsException (msg, inner, filename, line);
			Assert.AreEqual (new ConfigurationErrorsException ().Message, cee.BareMessage, "#G1");
			Assert.IsNotNull (cee.Data, "#G2");
			Assert.AreEqual (0, cee.Data.Count, "#G3");
			Assert.AreSame (filename, cee.Filename, "#G4");
			Assert.AreSame (inner, cee.InnerException, "#G5");
			Assert.AreEqual (line, cee.Line, "#G6");
			Assert.AreEqual (cee.BareMessage + " (abc.txt line 7)", cee.Message, "#G7");
		}

		[Test] // GetFilename (XmlReader)
		public void GetFilename1_Reader_Null ()
		{
			XmlReader reader = null;
			string filename = ConfigurationErrorsException.GetFilename (reader);
			Assert.IsNull (filename);
		}

		[Test] // GetFilename (XmlNode)
		public void GetFilename2 ()
		{
			XmlNode node;
			string filename;

			string xmlfile = Path.Combine (foldername, "test.xml");
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("test"));
			doc.Save (xmlfile);

			node = new XmlDocument ();
			filename = ConfigurationErrorsException.GetFilename (node);
			Assert.IsNull (filename, "#1");

			doc = new XmlDocument ();
			doc.Load (xmlfile);

			node = doc.DocumentElement;
			filename = ConfigurationErrorsException.GetFilename (node);
			Assert.IsNull (filename, "#2");

			doc = new ConfigXmlDocument ();
			doc.Load (xmlfile);

			node = doc.DocumentElement;
			filename = ConfigurationErrorsException.GetFilename (node);
			Assert.AreEqual (xmlfile, filename, "#3");
		}

		[Test] // GetFilename (XmlNode)
		public void GetFilename2_Node_Null ()
		{
			XmlNode node = null;
			string filename = ConfigurationErrorsException.GetFilename (node);
			Assert.IsNull (filename);
		}

		[Test] // GetLineNumber (XmlReader)
		public void GetLineNumber1 ()
		{
			string xmlfile = Path.Combine (foldername, "test.xml");
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("test"));
			doc.Save (xmlfile);

			using (XmlReader reader = new XmlTextReader (xmlfile)) {
				int line = ConfigurationErrorsException.GetLineNumber (reader);
				Assert.AreEqual (0, line, "#1");
			}

			using (XmlErrorReader reader = new XmlErrorReader (xmlfile)) {
				int line = ConfigurationErrorsException.GetLineNumber (reader);
				Assert.AreEqual (666, line, "#1");
			}
		}

		[Test] // GetLineNumber (XmlReader)
		public void GetLineNumber1_Reader_Null ()
		{
			XmlReader reader = null;
			int line = ConfigurationErrorsException.GetLineNumber (reader);
			Assert.AreEqual (0, line);
		}

		[Test] // GetLineNumber (XmlNode)
		public void GetLineNumber2 ()
		{
			XmlNode node;
			int line;

			string xmlfile = Path.Combine (foldername, "test.xml");
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("test"));
			doc.Save (xmlfile);

			node = new XmlDocument ();
			line = ConfigurationErrorsException.GetLineNumber (node);
			Assert.AreEqual (0, line, "#1");

			doc = new XmlDocument ();
			doc.Load (xmlfile);

			node = doc.DocumentElement;
			line = ConfigurationErrorsException.GetLineNumber (node);
			Assert.AreEqual (0, line, "#2");

			doc = new ConfigXmlDocument ();
			doc.Load (xmlfile);

			node = doc.DocumentElement;
			line = ConfigurationErrorsException.GetLineNumber (node);
			Assert.AreEqual (1, line, "#3");
		}

		[Test] // GetLineNumber (XmlNode)
		public void GetLineNumber2_Node_Null ()
		{
			XmlNode node = null;
			int line = ConfigurationErrorsException.GetLineNumber (node);
			Assert.AreEqual (0, line);
		}

		class XmlErrorReader : XmlTextReader, IConfigErrorInfo
		{
			public XmlErrorReader (string filename) : base (filename)
			{
			}

			string IConfigErrorInfo.Filename {
				get { return "FILE"; }
			}

			int IConfigErrorInfo.LineNumber {
				get { return 666; }
			}
		}
	}
}

#endif
