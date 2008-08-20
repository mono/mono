//
// SyndicationContentTest.cs - NUnit Test Cases for Abstract Class SyncicationContent
//
// Author:
//      Stephen A Jazdzewski (Steve@Jazd.com)
//
// Copyright (C) 2007 Stephen A Jazdzewski
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
using System.Text;
using System.Xml;
using NUnit.Framework;
using System.ServiceModel.Syndication;

namespace MonoTests.System.ServiceModel.Syndication
{
	[TestFixture]
	public class TextSyndicationContentTest {
		static string title = "Lorem";
		static string lorem = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Integer vitae.";
		string [] text_types = new string [3];

		[SetUp]
		public void Setup ()
		{
			// Map Text Content Kind to expected return value from .Type
			text_types [(int) TextSyndicationContentKind.Html] = "html";
			text_types [(int) TextSyndicationContentKind.Plaintext] = "text";
			text_types [(int) TextSyndicationContentKind.XHtml] = "xhtml";
		}

		[Test]
		public void TextSimple ()
		{
			TextSyndicationContent stext = new TextSyndicationContent (lorem, TextSyndicationContentKind.Plaintext);

			Assert.AreEqual (typeof (TextSyndicationContent), stext.GetType (), "#TS1");
			Assert.AreEqual (typeof (string), stext.Text.GetType (), "#TS2");
			// check for content and type
			Assert.AreEqual (lorem, stext.Text.ToString (), "#TS3");
			Assert.AreEqual (text_types [(int) TextSyndicationContentKind.Plaintext], stext.Type.ToString (), "#TS4");

			stext = new TextSyndicationContent (null);

			// Be sure .Text is null
			try
			{
				Assert.AreEqual (typeof (string), stext.Text.GetType (), "#TS6");
				Assert.Fail ("#TS7 Expected an NullReferenceException to be thrown.");
			}
			catch (NullReferenceException) {}
		}

		static string validhtml = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" " +
			"\"http://www.w3.org/TR/html4/strict.dtd\">" +
			"<HTML><HEAD><TITLE>" + title + "/TITLE></HEAD>" +
			"<BODY><P>" + lorem + "</BODY></HTML>";

		[Test]
		public void HtmlSimple ()
		{
			TextSyndicationContent shtml = new TextSyndicationContent (validhtml, TextSyndicationContentKind.Html);

			Assert.AreEqual (typeof (TextSyndicationContent), shtml.GetType (), "#HS1");
			Assert.AreEqual (typeof (string), shtml.Text.GetType (), "#HS2");
			// check for content and type
			Assert.AreEqual (validhtml, shtml.Text.ToString (), "#HS3");
			Assert.AreEqual (text_types [(int) TextSyndicationContentKind.Html], shtml.Type, "#HS4");
		}

		static string validxhtml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
			"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" " +
			"\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" +
			"<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">" +
			"<head><title>" + title + "</title></head>" +
			"<body><p>" + lorem + "</p></body></html>";


		[Test]
		public void XHtmlSimple ()
		{
			TextSyndicationContent sxhtml = new TextSyndicationContent (validhtml, TextSyndicationContentKind.XHtml);

			Assert.AreEqual (typeof (TextSyndicationContent), sxhtml.GetType (), "#XS1");
			Assert.AreEqual (typeof (string), sxhtml.Text.GetType (), "#XS2");
			// check for content and type
			Assert.AreEqual (validhtml, sxhtml.Text.ToString (), "#XS3");
			Assert.AreEqual (text_types [(int) TextSyndicationContentKind.XHtml], sxhtml.Type, "#XS4");
		}

		[Test]
		public void Advanced ()
		{
			// Defaults to Plaintext
			TextSyndicationContent stext = new TextSyndicationContent (lorem);

			Assert.AreEqual (text_types [(int) TextSyndicationContentKind.Plaintext], stext.Type, "#A1");
		}
	}

	[TestFixture]
	public class UrlSyndicationContentTest {
		static string media_type = "Text/xml";
		static string url = "http://www.Jazd.com/rss";

		[Test]
		public void Simple ()
		{
			Uri suri = new Uri (url);
			UrlSyndicationContent surl = new UrlSyndicationContent (suri, media_type);

			Assert.AreEqual (typeof (UrlSyndicationContent), surl.GetType (), "#S1");
			// check for content and type
			Assert.AreEqual (url.ToLower (), surl.Url.ToString (), "#S2");
			Assert.AreEqual (media_type, surl.Type.ToString (), "#S3");
		}
	}

	[TestFixture]
	public class XmlSyndicationContentTest {
		static string type = "text/xml";
		static string tagname = "stuff";
		static string content = "Need some";
		static string documentname = "channel";

		// May end up being setup
		[Test]
		public void Simple ()
		{
			XmlDocument sdoc = new XmlDocument ();
			XmlElement selem = sdoc.CreateElement (tagname);
			selem.InnerText = content;

			StringBuilder syndication_string = new StringBuilder ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			XmlWriter syndication = XmlWriter.Create (syndication_string, settings);

			// Simple tests
			XmlSyndicationContent sxml = new XmlSyndicationContent (type, selem);

			Assert.AreEqual (typeof (XmlSyndicationContent), sxml.GetType (), "#S1");
			Assert.AreEqual (type, sxml.Type.ToString (), "#S2");

			// Check correct invalid argument rejection
			try
			{
				sxml.WriteTo (null, "", "");
				Assert.Fail ("#S3 Expected an ArgumentNullException to be thrown.");
			}
			catch (ArgumentNullException) { }
			try
			{
				sxml.WriteTo (syndication, "", "");
				Assert.Fail ("#S4 Expected an ArgumentException to be thrown.");
			}
			catch (ArgumentException) { }

			syndication.Close ();
		}

		[Test]
		public void XmlElementExtension ()
		{
			XmlDocument xdoc = new XmlDocument ();
			XmlElement xelem = xdoc.CreateElement (tagname);
			xelem.InnerText = content;

			// Same as Simple tests as setup for XmlElementExtension tests
			XmlSyndicationContent xxml = new XmlSyndicationContent (type, xelem);

			Assert.AreEqual (typeof (XmlSyndicationContent), xxml.GetType (), "#XE1");
			Assert.AreEqual (type, xxml.Type.ToString (), "#XE2");
			Assert.AreSame (xelem, xxml.Extension.Object, "#XE3");
			Assert.AreEqual (SyndicationElementExtensionKind.XmlElement, xxml.Extension.ExtensionKind, "#XE4");
			Assert.AreEqual (null, xxml.Extension.ObjectSerializer, "#XE5");

			// Create fake IO using stringbuilders
			StringBuilder element_string = new StringBuilder ();
			StringBuilder syndication_string = new StringBuilder ();

			XmlWriterSettings settings = new XmlWriterSettings ();

			XmlWriter element = XmlWriter.Create (element_string, settings);
			XmlWriter syndication = XmlWriter.Create (syndication_string, settings);

			// Make sure we get the input data back out
			xelem.WriteTo (element);
			xxml.WriteTo (syndication, documentname, "");

			element.Close ();
			syndication.Close ();

			// Pickout the 'channel' and 'stuff' tags from original and syndicated document
			XmlDocument syndoc = new XmlDocument();
			XmlDocument eledoc = new XmlDocument();

			syndoc.LoadXml (syndication_string.ToString ());
			XmlNodeList synresult = syndoc.GetElementsByTagName (tagname);
			XmlNodeList syntype = syndoc.GetElementsByTagName (documentname);

			eledoc.LoadXml (element_string.ToString ());
			XmlNodeList eleresult = eledoc.GetElementsByTagName (tagname);

			// Check document type
			Assert.AreEqual(type, syntype.Item (0).Attributes.GetNamedItem ("type").Value.ToString (),
					     "XE6");
			// Check content
			Assert.AreEqual (eleresult.Item (0).OuterXml.ToString (), synresult.Item (0).OuterXml.ToString (),
					      "XE7");
		}

		static Int32 author = 32765;
		static string comment = "No comment.";

		[Test]
		public void XmlSerializerExtension ()
		{
			DateTime date = new DateTime (2007, 5, 22);

			global::System.Xml.Serialization.XmlSerializer xs =
				new global::System.Xml.Serialization.XmlSerializer (typeof (Content));
			Content item = new Content ();
			string item_object = "Content";  // tag name for serialized object

			// fill object with some data
			item.author = author;
			item.comment = comment;
			item.date = date;

			XmlSyndicationContent se = new XmlSyndicationContent (type,item,xs);

			Assert.AreEqual (typeof (XmlSyndicationContent), se.GetType (), "#SE1");
			Assert.AreEqual (type, se.Type.ToString (), "#SE2");
			Assert.AreSame (item, se.Extension.Object, "#SE3");
			Assert.AreEqual (SyndicationElementExtensionKind.XmlSerializer, se.Extension.ExtensionKind, "#SE4");
			Assert.AreSame (xs, se.Extension.ObjectSerializer, "#SE5");

			// Create fake IO using stringbuilders
			StringBuilder object_string = new StringBuilder ();
			StringBuilder syndication_string = new StringBuilder ();

			XmlWriterSettings settings = new XmlWriterSettings ();

			XmlWriter serobj = XmlWriter.Create (object_string, settings);
			XmlWriter syndication = XmlWriter.Create (syndication_string, settings);

			xs.Serialize (serobj, item);
			se.WriteTo (syndication, documentname, "");

			serobj.Close ();
			syndication.Close ();

			// Pickout the 'Content' tag from original serialized object and syndicated document
			XmlDocument syndoc = new XmlDocument ();
			XmlDocument serdoc = new XmlDocument ();

			syndoc.LoadXml (syndication_string.ToString ());
			XmlNodeList synresult = syndoc.GetElementsByTagName (item_object);
			XmlNodeList syntype = syndoc.GetElementsByTagName (documentname);

			serdoc.LoadXml (object_string.ToString ());
			XmlNodeList serresult = serdoc.GetElementsByTagName (item_object);

			// Check document type
			Assert.AreEqual(type, syntype.Item (0).Attributes.GetNamedItem ("type").Value.ToString (),
					     "SE6");
			// Check content
			Assert.AreEqual (serresult.Item (0).OuterXml.ToString (), synresult.Item (0).OuterXml.ToString (),
					      "SE6");
		}

		[Test]
		// ToDo
		public void DataContractExtension ()
		{
		}
	}

	public class Content {
		public Int32 author;
		public string comment;
		public DateTime date;
	}
}
