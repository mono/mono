//
// Microsoft.TeamFoundation.VersionControl.Client.SyndicationItemTest
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
	public class SyndicationItemTest
	{
		[Test]
		public void SyndicationItem_EmptyConstructor()
		{
			SyndicationItem item = new SyndicationItem();
			Assert.IsNull(item.Id);
			Assert.IsNull(item.Title);
			Assert.IsNull(item.Summary);
			Assert.IsNull(item.Content);
			Assert.IsNull(item.SourceFeed);
			Assert.IsNull(item.Copyright);

			DateTime defDate = new DateTime(0);
			Assert.AreEqual(item.PublishDate, defDate);
			Assert.AreEqual(item.LastUpdatedTime, defDate);
		}
	}
}

