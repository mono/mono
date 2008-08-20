//
// Microsoft.TeamFoundation.VersionControl.Client.Rss20SerializerTest
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//

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
using System.Net;
using System.Xml;
using System.ServiceModel.Syndication;

namespace System.ServiceModel.Syndication
{
	using NUnit.Framework;

	[TestFixture]
	public class Rss20SerializerTest : Rss20Serializer
	{
		string FileToString(string path)
		{
			string x;

			using (StreamReader sr = new StreamReader(path))
				{
					x = sr.ReadToEnd();
				}

			return x;
		}

		string FeedToString(string id)
		{
			return FileToString(String.Format("Test/{0}.rss.xml", id));
		}

		string SyndicationFeedToString(SyndicationFeed f)
		{
			// try to keep the time here stable so it doesn't trip up the string compare
			f.LastUpdatedTime = FeedLib.FixedChangedDate;

			StringWriter strWriter = new StringWriter();
			using (XmlTextWriter writer = new XmlTextWriter(strWriter))
			{
				writer.Formatting = Formatting.Indented;
				Rss20Serializer serializer = new Rss20Serializer();
				serializer.WriteTo(writer, f);
			}

			return strWriter.ToString();
		}

		[Test]
		public void Serializer_Properties()
		{
			Assert.AreEqual("rss", FeedName);
			Assert.AreEqual("", FeedNamespace);
			Assert.AreEqual("item", ItemName);
			Assert.AreEqual("", ItemNamespace);
		}

		[Test]
		public void Serialize_EmptyFeed()
		{
			string expected = FeedToString("EmptyFeed");
			string actual = SyndicationFeedToString(FeedLib.EmptyFeed);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Serialize_FeedNoItems()
		{
			string expected = FeedToString("FeedNoItems");
			string actual = SyndicationFeedToString(FeedLib.FeedNoItems);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Serialize_FeedWithItems()
		{
			string expected = FeedToString("FeedWithItems");
			string actual = SyndicationFeedToString(FeedLib.FeedWithItems);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Serialize_FeedNoItemsSimpleProps()
		{
			string expected = FeedToString("FeedNoItemsSimpleProps");
			string actual = SyndicationFeedToString(FeedLib.FeedNoItemsSimpleProps);
			Assert.AreEqual(expected, actual);
		}

	}
}

