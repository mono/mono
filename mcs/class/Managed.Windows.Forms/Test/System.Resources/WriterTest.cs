//
// WriterTest.cs: Unit Tests for ResXResourceWriter.
//
// Authors:
//     Robert Jordan <robertj@gmx.net>
//

using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Resources;
using NUnit.Framework;

namespace MonoTests.System.Resources
{
        [TestFixture]
        public class WriterTest
        {
                string fileName = Path.GetTempFileName ();

                [Test]
                public void TestWriter ()
                {
                        ResXResourceWriter w = new ResXResourceWriter (fileName);
                        w.AddResource ("String", "hola");
                        w.AddResource ("String2", (object) "hello");
                        w.AddResource ("Int", 42);
                        w.AddResource ("Enum", PlatformID.Win32NT);
                        w.AddResource ("Convertible", new Point (43, 45));
                        w.AddResource ("Serializable", new ArrayList(new byte[] {1, 2, 3, 4}));
                        w.AddResource ("ByteArray", new byte[] {12, 13, 14});
                        w.AddResource ("ByteArray2", (object) new byte[] {15, 16, 17});
                        w.AddResource ("IntArray", new int[] {1012, 1013, 1014});
                        w.AddResource ("StringArray", new string[] {"hello", "world"});
                        w.Generate ();
                        w.Close ();

                        ResXResourceReader r = new ResXResourceReader (fileName);
                        Hashtable h = new Hashtable();
                        foreach (DictionaryEntry e in r) {
                                h.Add (e.Key, e.Value);
                        }
                        r.Close ();

                        Assert.AreEqual ("hola", (string) h["String"], "#1");
                        Assert.AreEqual ("hello", (string) h["String2"], "#2");
                        Assert.AreEqual (42, (int) h["Int"], "#3");
                        Assert.AreEqual (PlatformID.Win32NT, (PlatformID) h["Enum"], "#4");
                        Assert.AreEqual (43, ((Point) h["Convertible"]).X, "#5");
                        Assert.AreEqual (2, (byte) ((ArrayList) h["Serializable"])[1], "#6");
                        Assert.AreEqual (13, ((byte[]) h["ByteArray"])[1], "#7");
                        Assert.AreEqual (16, ((byte[]) h["ByteArray2"])[1], "#8");
                        Assert.AreEqual (1013, ((int[]) h["IntArray"])[1], "#9");
                        Assert.AreEqual ("world", ((string[]) h["StringArray"])[1], "#10");

                        File.Delete (fileName);
                }
        }
}
