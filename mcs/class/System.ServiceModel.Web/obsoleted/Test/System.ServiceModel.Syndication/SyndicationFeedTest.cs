//
// Microsoft.TeamFoundation.VersionControl.Client.SyndicationFeedTest
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
using System.ServiceModel.Syndication;

namespace System.ServiceModel.Syndication
{
	using NUnit.Framework;

	[TestFixture]
	public class SyndicationFeedTest
	{
		[Test]
		public void SyndicationFeed_EmptyConstructor()
		{
			SyndicationFeed feed = new SyndicationFeed();
			Assert.IsNull(feed.Id);
			Assert.IsNull(feed.Title);
			Assert.IsNull(feed.Description);
			Assert.IsNotNull(feed.LastUpdatedTime);
			Assert.IsNull(feed.Copyright);
			Assert.IsNull(feed.Generator);
			Assert.IsNull(feed.Language);
			Assert.IsNull(feed.ImageUrl);
		}

		[Test]
		public void SyndicationFeed_3ArgConstructor()
		{
			SyndicationFeed feed = new SyndicationFeed("title", "description", new Uri("http://www.go-mono.com"));
			Assert.IsNull(feed.Id);
			Assert.IsNotNull(feed.LastUpdatedTime);
			Assert.IsNull(feed.Copyright);
			Assert.IsNull(feed.Generator);
			Assert.IsNull(feed.Language);
			Assert.IsNull(feed.ImageUrl);

			Assert.AreEqual(feed.Title.Text, "title");
			Assert.AreEqual(feed.Description.Text, "description");
			Assert.AreEqual(feed.Links.Count, 1);
		}
	}
}

