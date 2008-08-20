//
// System.ServiceModel.Syndication.SyndicationItem
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
	public class SyndicationItem
	{
		private string id;
		private TextSyndicationContent summary;
		private TextSyndicationContent title;
		private SyndicationContent content;
		private SyndicationFeed sourceFeed;
		private DateTime publishDate;
		private DateTime lastUpdatedTime;
		private TextSyndicationContent copyright;

		public SyndicationItem()
		{
			publishDate = new DateTime(0);
			lastUpdatedTime = new DateTime(0);
		}

		public void WriteTo(XmlWriter writer, SyndicationSerializer serializer)
		{
			serializer.WriteTo(writer, this);
		}

		public string Id
		{
			get { return id; }
			set { id = value; }
		}

		public SyndicationContent Content
		{
			get { return content; }
			set { content = value; }
		}

		public TextSyndicationContent Copyright
		{
			get { return copyright; }
			set { copyright = value; }
		}

		public DateTime LastUpdatedTime
		{
			get { return lastUpdatedTime; }
			set { lastUpdatedTime = value; }
		}

		public DateTime PublishDate
		{
			get { return publishDate; }
			set { publishDate = value; }
		}

		public SyndicationFeed SourceFeed
		{
			get { return sourceFeed; }
			set { sourceFeed = value; }
		}

		public TextSyndicationContent Summary {
			get { return summary; }
			set { summary = value; }
		}

		public TextSyndicationContent Title {
			get { return title; }
			set { title = value; }
		}

	}
}

