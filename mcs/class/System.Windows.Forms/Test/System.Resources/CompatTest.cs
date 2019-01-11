//
// CompatTest.cs: Compatibility unit tests for ResXResourceReader.
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

using MonoTests.Helpers;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class CompatTest : MonoTests.System.Windows.Forms.TestHelper
	{
		class Helper
		{
			public static void TestReader (string fileName)
			{
				ResXResourceReader r = new ResXResourceReader (fileName);
				Hashtable h = new Hashtable ();
				foreach (DictionaryEntry e in r) {
					h.Add (e.Key, e.Value);
				}
				r.Close ();

				Assert.AreEqual ("hola", (string) h ["String"], fileName + "#1");
				Assert.AreEqual ("hello", (string) h ["String2"], fileName + "#2");
				Assert.AreEqual (42, (int) h ["Int"], fileName + "#3");
				Assert.AreEqual (PlatformID.Win32NT, (PlatformID) h ["Enum"], fileName + "#4");
				Assert.AreEqual (43, ((Point) h ["Convertible"]).X, fileName + "#5");
				Assert.AreEqual (2, (byte) ((ArrayList) h ["Serializable"]) [1], fileName + "#6");
				Assert.AreEqual (13, ((byte []) h ["ByteArray"]) [1], fileName + "#7");
				Assert.AreEqual (16, ((byte []) h ["ByteArray2"]) [1], fileName + "#8");
				Assert.AreEqual (1013, ((int []) h ["IntArray"]) [1], fileName + "#9");
				Assert.AreEqual ("world", ((string []) h ["StringArray"]) [1], fileName + "#10");
				Assert.IsNull (h ["InvalidMimeType"], "#11");
				Assert.IsNotNull (h ["Image"], fileName + "#12");
				Assert.AreEqual (typeof (Bitmap), h ["Image"].GetType (), fileName + "#13");
			}
		}

		[Test]
		public void TestReader ()
		{
			Helper.TestReader (TestResourceHelper.GetFullPathOfResource ("Test/System.Resources/compat_1_1.resx"));
		}

		[Test]
		public void TestReader_2_0 ()
		{
			Helper.TestReader (TestResourceHelper.GetFullPathOfResource ("Test/System.Resources/compat_2_0.resx"));
		}
	}
}
