//
// CultureTest.cs: Test cases for culture-invariant string convertions
//
// Authors:
//     Robert Jordan <robertj@gmx.net>
//

using System;
using System.Collections;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class CultureTest : MonoTests.System.Windows.Forms.TestHelper
	{
		string fileName = null;

		[SetUp]
		protected override void SetUp ()
		{
			fileName = Path.GetTempFileName ();
			base.SetUp ();
		}

		[TearDown]
		protected override void TearDown ()
		{
			File.Delete (fileName);
			base.TearDown ();
		}

		[Test]
		public void Test ()
		{
			Thread.CurrentThread.CurrentCulture =
					Thread.CurrentThread.CurrentUICulture = new CultureInfo ("de-DE");

			ResXResourceWriter w = new ResXResourceWriter (fileName);
			w.AddResource ("point", new Point (42, 43));
			w.Generate ();
			w.Close ();

			int count = 0;
			ResXResourceReader r = new ResXResourceReader (fileName);
			IDictionaryEnumerator e = r.GetEnumerator ();
			while (e.MoveNext ()) {
				if ((string) e.Key == "point") {
					Assert.AreEqual (typeof (Point), e.Value.GetType (), "#1");
					Point p = (Point) e.Value;
					Assert.AreEqual (42, p.X, "#2");
					Assert.AreEqual (43, p.Y, "#3");
					count++;
				}
			}
			r.Close ();
			Assert.AreEqual (1, count, "#100");
		}
	}
}
