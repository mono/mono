//
// System.Web.Hosting.HostingEnvironmentTest 
// 
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
//
// Copyright (C) 2007 Gert Driesen
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
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;

using NUnit.Framework;

namespace MonoTests.System.Web.Caching
{
	[TestFixture]
	public class AggregateCacheDependencyTest
	{
		private string _tempFolder;

		[SetUp]
		public void SetUp ()
		{
			_tempFolder = Path.Combine (Path.GetTempPath (), 
				"MonoTests.System.Web.Caching.AggregateCacheDependencyTest");
			if (Directory.Exists (_tempFolder))
				Directory.Delete (_tempFolder, true);
			Directory.CreateDirectory (_tempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			if (Directory.Exists (_tempFolder))
				Directory.Delete (_tempFolder, true);
		}

		[Test] // bug #82419
		[Ignore ("The test is not written in reproducible way and hence keeps build looks as if it regressed incorrectly")]
		public void SingleDependency ()
		{
			string depFile = Path.Combine (_tempFolder, "dep.tmp");

			AggregateCacheDependency aggregate = new AggregateCacheDependency ();
			aggregate.Add (new CacheDependency (depFile));

			DateTime absoluteExpiration = DateTime.Now.AddSeconds (4);
			TimeSpan slidingExpiration = TimeSpan.Zero;
			CacheItemPriority priority = CacheItemPriority.Default;

			string original = "MONO";

			HttpRuntime.Cache.Insert ("key", original, aggregate,
				absoluteExpiration, slidingExpiration, priority,
				null);

			string cachedValue = HttpRuntime.Cache.Get ("key") as string;
			Assert.IsNotNull (cachedValue, "#A1");
			Assert.AreEqual ("MONO", cachedValue, "#A2");
			Assert.IsFalse (aggregate.HasChanged, "#A3");

			cachedValue = HttpRuntime.Cache.Get ("key") as string;
			Assert.IsNotNull (cachedValue, "#B1");
			Assert.AreEqual ("MONO", cachedValue, "#B2");
			Assert.IsFalse (aggregate.HasChanged, "#B3");

			File.WriteAllText (depFile, "OK", Encoding.UTF8);
			Thread.Sleep (500);

			cachedValue = HttpRuntime.Cache.Get ("key") as string;
			Assert.IsNull (cachedValue, "#C1");
			Assert.IsTrue (aggregate.HasChanged, "#C2");
		}

		[Test]
		[Ignore ("This test is racy, it fails from time to time.")]
		public void AbsoluteExpiration ()
		{
			string depFile = Path.Combine (_tempFolder, "dep.tmp");

			AggregateCacheDependency aggregate = new AggregateCacheDependency ();
			aggregate.Add (new CacheDependency (depFile));

			DateTime absoluteExpiration = DateTime.Now.AddMilliseconds (100);
			TimeSpan slidingExpiration = TimeSpan.Zero;
			CacheItemPriority priority = CacheItemPriority.Default;

			string original = "MONO";

			HttpRuntime.Cache.Insert ("key", original, aggregate,
				absoluteExpiration, slidingExpiration, priority,
				null);

			string cachedValue = HttpRuntime.Cache.Get ("key") as string;
			Assert.IsNotNull (cachedValue, "#A1");
			Assert.AreEqual ("MONO", cachedValue, "#A2");
			Assert.IsFalse (aggregate.HasChanged, "#A3");

			cachedValue = HttpRuntime.Cache.Get ("key") as string;
			Assert.IsNotNull (cachedValue, "#B1");
			Assert.AreEqual ("MONO", cachedValue, "#B2");
			Assert.IsFalse (aggregate.HasChanged, "#B3");

			Thread.Sleep (500);

			cachedValue = HttpRuntime.Cache.Get ("key") as string;
			Assert.IsNull (cachedValue, "#C1");
			Assert.IsFalse (aggregate.HasChanged, "#C2");
		}

		[Test]
		[Category ("NotWorking")]
		public void SlidingExpiration ()
		{
			string depFile = Path.Combine (_tempFolder, "dep.tmp");

			AggregateCacheDependency aggregate = new AggregateCacheDependency ();
			aggregate.Add (new CacheDependency (depFile));

			DateTime absoluteExpiration = Cache.NoAbsoluteExpiration;
			TimeSpan slidingExpiration = new TimeSpan (0, 0, 0, 0, 100);
			CacheItemPriority priority = CacheItemPriority.Default;

			string original = "MONO";

			HttpRuntime.Cache.Insert ("key", original, aggregate,
				absoluteExpiration, slidingExpiration, priority,
				null);

			string cachedValue = HttpRuntime.Cache.Get ("key") as string;
			Assert.IsNotNull (cachedValue, "#A1");
			Assert.AreEqual ("MONO", cachedValue, "#A2");
			Assert.IsFalse (aggregate.HasChanged, "#A3");

			Thread.Sleep (50);

			cachedValue = HttpRuntime.Cache.Get ("key") as string;
			Assert.IsNotNull (cachedValue, "#B1");
			Assert.AreEqual ("MONO", cachedValue, "#B2");
			Assert.IsFalse (aggregate.HasChanged, "#B3");

			Thread.Sleep (120);

			cachedValue = HttpRuntime.Cache.Get ("key") as string;
			Assert.IsNull (cachedValue, "#C1");
			Assert.IsFalse (aggregate.HasChanged, "#C2");
		}
	}
}
#endif
