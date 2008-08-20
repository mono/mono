//
// System.ServiceModel.Syndication.Atom10Serializer
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
using System.Globalization;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class Atom10Serializer : SyndicationSerializer
	{
		internal override void WriteXml(XmlWriter writer, SyndicationFeed feed)
		{
			writer.WriteStartElement(FeedName, FeedNamespace);

			if (!String.IsNullOrEmpty(feed.Language))
					writer.WriteAttributeString("xml:lang", feed.Language);

			WriteXml(writer, feed.Title, "title", true);
			writer.WriteElementString("id", feed.Id);
			WriteXml(writer, feed.Copyright, "rights");

			string updated = feed.LastUpdatedTime.ToUniversalTime().ToString("s");
			writer.WriteElementString("updated", updated + "Z");

			if (feed.ImageUrl != null)
				writer.WriteElementString("logo", feed.ImageUrl.ToString());

			if (!String.IsNullOrEmpty(feed.Generator))
				writer.WriteElementString("generator", feed.Generator);

			WriteXml(writer, feed.Description, "summary");

			foreach (SyndicationItem item in feed.Items)
				{
					WriteTo(writer, item);
				}

			writer.WriteEndElement();
		}

		internal override void WriteXml(XmlWriter writer, SyndicationItem item)
		{
			writer.WriteStartElement(ItemName, ItemNamespace);
			writer.WriteElementString("id", item.Id);
			WriteXml(writer, item.Title, "title");
			WriteXml(writer, item.Summary, "summary");
			
			DateTime defDate = new DateTime(0);
			if (item.PublishDate != defDate)
				{
					string date = item.PublishDate.ToUniversalTime().ToString("s");
					writer.WriteElementString("published", date + "Z");
				}

			if (item.LastUpdatedTime != defDate)
				{
					string date = item.LastUpdatedTime.ToUniversalTime().ToString("s");
					writer.WriteElementString("updated", date + "Z");
				}

			WriteXml(writer, item.Content, "content");
			WriteXml(writer, item.Copyright, "rights");

			writer.WriteEndElement();
		}

		protected override string FeedName { 
			get { return "feed"; } 
		}

		protected override string FeedNamespace { 
			get { return "http://www.w3.org/2005/Atom"; } 
		}

		protected override string ItemName { 
			get { return "entry"; } 
		}

		protected override string ItemNamespace { 
			get { return "http://www.w3.org/2005/Atom"; } 
		}

	}
}

