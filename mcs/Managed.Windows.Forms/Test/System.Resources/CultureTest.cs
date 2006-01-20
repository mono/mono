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
        public class CultureTest
        {
                string fileName = Path.GetTempFileName ();

                [Test]
                public void TestWriter ()
                {
                        Thread.CurrentThread.CurrentCulture =
                                Thread.CurrentThread.CurrentUICulture = new CultureInfo ("de-DE");

                        ResXResourceWriter w = new ResXResourceWriter (fileName);
                        w.AddResource ("point", new Point (42, 43));
                        w.Generate ();
                        w.Close ();
                }

                [Test]
                public void TestReader ()
                {
                        Thread.CurrentThread.CurrentCulture =
                                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                        int count = 0;
                        ResXResourceReader r = new ResXResourceReader (fileName);
                        IDictionaryEnumerator e = r.GetEnumerator ();
                        while (e.MoveNext ()) {
                                if ((string)e.Key == "point") {
                                        Assert.AreEqual (typeof (Point), e.Value.GetType (), "#1");
                                        Point p = (Point) e.Value;
                                        Assert.AreEqual (42, p.X, "#2");
                                        Assert.AreEqual (43, p.Y, "#3");
                                        count++;
                                }
                        }
                        r.Close ();
                        File.Delete (fileName);
                        Assert.AreEqual (1, count, "#100");
                }
        }
}
