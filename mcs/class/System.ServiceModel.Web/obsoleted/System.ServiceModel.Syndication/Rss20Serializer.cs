//
// System.ServiceModel.Syndication.Rss20Serializer
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
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class Rss20Serializer : SyndicationSerializer
	{
		internal override void WriteXml(XmlWriter writer, SyndicationFeed feed)
		{
			writer.WriteStartElement(FeedName, FeedNamespace);
			writer.WriteStartElement("channel", FeedNamespace);
			writer.WriteElementString("id", feed.Id);

			WriteXml(writer, feed.Title, "title");
			WriteXml(writer, feed.Description, "description");
			
			foreach (SyndicationItem item in feed.Items)
				{
					WriteTo(writer, item);
				}

			writer.WriteEndElement();
		}

		internal override void WriteXml(XmlWriter writer, SyndicationItem item)
		{
			writer.WriteStartElement(ItemName, ItemNamespace);
			writer.WriteElementString("guid", item.Id);
			WriteXml(writer, item.Title, "title");
			WriteXml(writer, item.Summary, "description");
			writer.WriteEndElement();
		}

		protected override string FeedName { 
			get { return "rss"; } 
		}

		protected override string FeedNamespace { 
			get { return ""; } 
		}

		protected override string ItemName { 
			get { return "item"; } 
		}

		protected override string ItemNamespace { 
			get { return ""; } 
		}

	}
}

