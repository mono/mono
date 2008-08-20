//
// System.ServiceModel.Syndication.SyndicationFeed
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
using System.Collections.ObjectModel;

namespace System.ServiceModel.Syndication
{
	public class SyndicationFeed
	{
		private string id;
		private TextSyndicationContent description;
		private TextSyndicationContent title;
		private Collection<SyndicationItem> items;
		private DateTime lastUpdatedTime;
		private TextSyndicationContent copyright;
		private string generator;
		private string language;
		private Uri imageUrl;
		private Collection<SyndicationLink> links;
		private Collection <SyndicationCategory> categories;
		private Collection <SyndicationPerson> contributors;

		public SyndicationFeed()
		{
			items = new Collection<SyndicationItem>();
			links = new Collection<SyndicationLink>();
			lastUpdatedTime = DateTime.Now.ToUniversalTime();
		}

		public SyndicationFeed(string title, string description,
													 Uri feedAlternateLink) : this()
		{
			this.title = new TextSyndicationContent(title);
			this.description = new TextSyndicationContent(description);
			links.Add(SyndicationLink.CreateAlternateLink(feedAlternateLink));
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

		public TextSyndicationContent Copyright
		{
			get { return copyright; }
			set { copyright = value; }
		}

		public string Generator
		{
			get { return generator; }
			set { generator = value; }
		}

		public string Language
		{
			get { return language; }
			set { language = value; }
		}

		public DateTime LastUpdatedTime
		{
			get { return lastUpdatedTime; }
			set { lastUpdatedTime = value; }
		}

		public Uri ImageUrl
		{
			get { return imageUrl; }
			set { imageUrl = value; }
		}

		public Collection<SyndicationItem> Items {
			get { return items; }
		}

		public Collection<SyndicationLink> Links {
			get { return links; }
		}

		public TextSyndicationContent Description {
			get { return description; }
			set { description = value; }
		}

		public TextSyndicationContent Title {
			get { return title; }
			set { title = value; }
		}

		public Collection <SyndicationPerson> Contributors {
			get { return contributors; }
		}

		public Collection <SyndicationCategory> Categories {
			get { return categories; }
		}
	}
}

