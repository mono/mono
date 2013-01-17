//
// ResXResourceReaderTest.cs: Unit Tests for ResXResourceReader.
//
// Authors:
//     Gert Driesen <drieseng@users.sourceforge.net>
//     Olivier Dufour <olivier.duff@gmail.com>
//     Gary Barnett <gary.barnett.mono@gmail.com>

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.Serialization;

using NUnit.Framework;
using System.Reflection;

namespace MonoTests.System.Resources {
	[TestFixture]
	public class ResXResourceReaderTest : MonoTests.System.Windows.Forms.TestHelper
	{
		private string _tempDirectory;
		private string _otherTempDirectory;

		[SetUp]
		protected override void SetUp ()
		{
			_tempDirectory = Path.Combine (Path.GetTempPath (), "ResXResourceReaderTest");
			_otherTempDirectory = Path.Combine (_tempDirectory, "in");
			if (!Directory.Exists (_otherTempDirectory)) {
				Directory.CreateDirectory (_otherTempDirectory);
			}
			base.SetUp ();
		}

		[TearDown]
		protected override void TearDown ()
		{
			if (Directory.Exists (_tempDirectory))
				Directory.Delete (_tempDirectory, true);
			base.TearDown ();
		}

		[Test] // ctor (Stream)
		public void Constructor1_Stream_InvalidContent ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.WriteByte (byte.MaxValue);
			ms.Position = 0;
			ResXResourceReader r = new ResXResourceReader (ms);
			try {
				r.GetEnumerator ();
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid ResX input
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.ParamName, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");
			}
		}

		[Test] // ctor (Stream)
		[Category ("NotDotNet")] // MS throws a NullReferenceException in GetEnumerator ()
		public void Constructor1_Stream_Null ()
		{
			try {
				new ResXResourceReader ((Stream) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("stream", ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String)
		public void Constructor2_FileName_DoesNotExist ()
		{
			ResXResourceReader r = new ResXResourceReader ((string) "definitelydoesnotexist.zzz");
			try {
				r.GetEnumerator ();
				Assert.Fail ("#1");
			} catch (FileNotFoundException ex) {
				Assert.AreEqual (typeof (FileNotFoundException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.FileName, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.InnerException, "#5");
			}
		}

		[Test] // ctor (TextReader)
		public void Constructor3_Reader_InvalidContent ()
		{
			StringReader sr = new StringReader ("</definitelyinvalid<");
			ResXResourceReader r = new ResXResourceReader (sr);
			try {
				r.GetEnumerator ();
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid ResX input
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.ParamName, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");
				Assert.AreEqual (typeof (XmlException), ex.InnerException.GetType (), "#6");
			}
		}

		[Test]
		public void Close_FileName ()
		{
			string fileName = Path.Combine (Path.Combine ("Test", "System.Resources"), "compat_1_1.resx");
			if (!File.Exists (fileName))
				fileName = String.Format ("..{0}System.Resources{0}compat_1_1.resx", Path.DirectorySeparatorChar);

			ResXResourceReader r1 = new ResXResourceReader (fileName);
			r1.GetEnumerator ();
			r1.Close ();
			r1.GetEnumerator ();

			ResXResourceReader r2 = new ResXResourceReader (fileName);
			r2.Close ();
			r2.GetEnumerator ();
			r2.Close ();
		}

		[Test]
		public void Close_Reader ()
		{
			string fileName = Path.Combine (Path.Combine ("Test", "System.Resources"), "compat_1_1.resx");
			if (!File.Exists (fileName))
				fileName = String.Format ("..{0}System.Resources{0}compat_1_1.resx", Path.DirectorySeparatorChar);

			using (StreamReader sr = new StreamReader (fileName)) {
				ResXResourceReader r = new ResXResourceReader (sr);
				Assert.IsFalse (sr.Peek () == -1, "#A1");
				r.GetEnumerator ();
				Assert.IsTrue (sr.Peek () == -1, "#A2");
				r.Close ();
				try {
					sr.Peek ();
					Assert.Fail ("#A3");
				} catch (ObjectDisposedException) {
				}
				r.GetEnumerator ();
			}

			using (StreamReader sr = new StreamReader (fileName)) {
				ResXResourceReader r = new ResXResourceReader (sr);
				r.Close ();
				try {
					sr.Peek ();
					Assert.Fail ("#B1");
				} catch (ObjectDisposedException) {
				}
				try {
					r.GetEnumerator ();
					Assert.Fail ("#B2");
				} catch (NullReferenceException) { // MS
				} catch (InvalidOperationException) { // Mono
				}
			}
		}

		[Test]
		public void Close_Stream ()
		{
			string fileName = Path.Combine (Path.Combine ("Test", "System.Resources"), "compat_1_1.resx");
			if (!File.Exists (fileName))
				fileName = String.Format ("..{0}System.Resources{0}compat_1_1.resx", Path.DirectorySeparatorChar);

			using (FileStream fs = File.OpenRead (fileName)) {
				ResXResourceReader r = new ResXResourceReader (fs);
				Assert.AreEqual (0, fs.Position, "#A1");
				r.GetEnumerator ();
				Assert.IsFalse (fs.Position == 0, "#A2");
				Assert.IsTrue (fs.CanRead, "#A3");
				r.Close ();
				Assert.IsTrue (fs.CanRead, "#A4");
				r.GetEnumerator ().MoveNext ();
			}

			using (FileStream fs = File.OpenRead (fileName)) {
				ResXResourceReader r = new ResXResourceReader (fs);
				r.Close ();
				Assert.AreEqual (0, fs.Position, "#B1");
				r.GetEnumerator ();
				Assert.IsFalse (fs.Position == 0, "#B2");
			}
		}

		[Test]
		public void ExternalFileReference_Icon ()
		{
			string refFile = Path.Combine (_tempDirectory, "32x32.ico");
			WriteEmbeddedResource ("32x32.ico", refFile);

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, refFile,
					typeof (Bitmap).AssemblyQualifiedName, string.Empty,
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				IDictionaryEnumerator enumerator = r.GetEnumerator ();
				enumerator.MoveNext ();
				Assert.IsNotNull (enumerator.Current, "#A1");
				Assert.AreEqual ("foo", enumerator.Key, "#A2");
				Bitmap bitmap = enumerator.Value as Bitmap;
				Assert.IsNotNull (bitmap, "#A3");
#if NET_2_0
				Assert.AreEqual (32, bitmap.Height, "#A4");
				Assert.AreEqual (32, bitmap.Width, "#A5");
#else
				Assert.AreEqual (96, bitmap.Height, "#A4");
				Assert.AreEqual (96, bitmap.Width, "#A5");
#endif
			}

			File.Delete (refFile);
			File.Delete (resxFile);

			refFile = Path.Combine (_tempDirectory, "32x32.ICO");
			WriteEmbeddedResource ("32x32.ico", refFile);

			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, refFile,
					typeof (Bitmap).AssemblyQualifiedName, string.Empty,
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				IDictionaryEnumerator enumerator = r.GetEnumerator ();
				enumerator.MoveNext ();
				Assert.IsNotNull (enumerator.Current, "#B1");
				Assert.AreEqual ("foo", enumerator.Key, "#B2");
				Bitmap bitmap = enumerator.Value as Bitmap;
				Assert.IsNotNull (bitmap, "#B3");
				Assert.AreEqual (96, bitmap.Height, "#B4");
				Assert.AreEqual (96, bitmap.Width, "#B5");
			}
		}

		[Test]
		public void ExternalFileReference_RelativePath ()
		{
			string refFile = Path.Combine (_otherTempDirectory, "string.txt");
			string relRefFile = Path.Combine ("in", "string.txt");
			using (StreamWriter sw = new StreamWriter (refFile, false, Encoding.UTF8)) {
				sw.Write ("hello");
			}

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, 
					"in" + Path.DirectorySeparatorChar + "string.txt", 
					typeof (StreamReader).AssemblyQualifiedName, string.Empty,
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#A1");
				} catch (ArgumentException ex) {
					// Invalid ResX input
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
					Assert.IsNotNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsNull (ex.ParamName, "#A5");

#if NET_2_0
					// Could not find a part of the path "<current dir>\in\string.txt".
					// Line 1, position 821
					XmlException inner = ex.InnerException as XmlException;
					Assert.IsNotNull (inner, "#A6");
					Assert.AreEqual (typeof (XmlException), inner.GetType (), "#A7");
					Assert.IsNotNull (inner.InnerException, "#A8");
					Assert.AreEqual (1, inner.LineNumber, "#A9");
//					Assert.AreEqual (821, inner.LinePosition, "#A10");
					Assert.IsNotNull (inner.Message, "#A11");
					Assert.IsTrue (inner.Message.IndexOf (Path.Combine (
						Directory.GetCurrentDirectory (), relRefFile)) != -1, "#A12");

					// Could not find a part of the path "<current dir>\in\string.txt"
					Exception inner2 = inner.InnerException;
					Assert.AreEqual (typeof (DirectoryNotFoundException), inner2.GetType (), "#A13");
					Assert.IsNull (inner2.InnerException, "#A14");
					Assert.IsNotNull (inner2.Message, "#A15");
					Assert.IsTrue (inner2.Message.IndexOf (Path.Combine (
						Directory.GetCurrentDirectory (), relRefFile)) != -1, "#A16");
#else
					// Could not find a part of the path "<current dir>\in\string.txt"
					Exception inner = ex.InnerException;
					Assert.AreEqual (typeof (DirectoryNotFoundException), inner.GetType (), "#A6");
					Assert.IsNull (inner.InnerException, "#A7");
					Assert.IsNotNull (inner.Message, "#A8");
					Assert.IsTrue (inner.Message.IndexOf (Path.Combine (
						Directory.GetCurrentDirectory (), relRefFile)) != -1, "#A9");
#endif
				}
#if NET_2_0
				Assert.IsNull (r.BasePath, "#A17");
#endif
			}

			string originalCurrentDir = Directory.GetCurrentDirectory ();
			Directory.SetCurrentDirectory (_tempDirectory);
			try {
				using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
					IDictionaryEnumerator enumerator = r.GetEnumerator ();
					enumerator.MoveNext ();
					Assert.IsNotNull (enumerator.Current, "#B1");
					Assert.AreEqual ("foo", enumerator.Key, "#B2");
					using (StreamReader sr = enumerator.Value as StreamReader) {
						Assert.IsNotNull (sr, "#B3");
						Assert.AreEqual ("hello", sr.ReadToEnd (), "#B4");
					}
#if NET_2_0
					Assert.IsNull (r.BasePath, "#B5");
#endif
				}
			} finally {
				// restore original current directory
				Directory.SetCurrentDirectory (originalCurrentDir);
			}

#if NET_2_0
			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				r.BasePath = _tempDirectory;
				IDictionaryEnumerator enumerator = r.GetEnumerator ();
				enumerator.MoveNext ();
				Assert.IsNotNull (enumerator.Current, "#C1");
				Assert.AreEqual ("foo", enumerator.Key, "#C2");
				using (StreamReader sr = enumerator.Value as StreamReader) {
					Assert.IsNotNull (sr, "#C3");
					Assert.AreEqual ("hello", sr.ReadToEnd (), "#C4");
				}
				Assert.AreEqual (_tempDirectory, r.BasePath, "#C5");
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				r.BasePath = Path.GetTempPath ();
				try {
					r.GetEnumerator ();
					Assert.Fail ("#D1");
				} catch (ArgumentException ex) {
					// Invalid ResX input
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
					Assert.IsNotNull (ex.InnerException, "#D3");
					Assert.IsNotNull (ex.Message, "#D4");
					Assert.IsNull (ex.ParamName, "#D5");

					// Could not find a part of the path "<temp path>\in\string.txt".
					// Line 1, position 821
					XmlException inner = ex.InnerException as XmlException;
					Assert.IsNotNull (inner, "#D6");
					Assert.AreEqual (typeof (XmlException), inner.GetType (), "#D7");
					Assert.IsNotNull (inner.InnerException, "#D8");
					Assert.AreEqual (1, inner.LineNumber, "#D9");
//					Assert.AreEqual (821, inner.LinePosition, "#D10");
					Assert.IsNotNull (inner.Message, "#D11");
					Assert.IsTrue (inner.Message.IndexOf (Path.Combine (
						Path.GetTempPath (), relRefFile)) != -1, "#D12");

					// Could not find a part of the path "<temp path>\in\string.txt"
					Exception inner2 = inner.InnerException as Exception;
					Assert.AreEqual (typeof (DirectoryNotFoundException), inner2.GetType (), "#D13");
					Assert.IsNull (inner2.InnerException, "#D14");
					Assert.IsNotNull (inner2.Message, "#D15");
					Assert.IsTrue (inner2.Message.IndexOf (Path.Combine (
						Path.GetTempPath (), relRefFile)) != -1, "#D16");
				}
				Assert.AreEqual (Path.GetTempPath (), r.BasePath, "#D17");
			}
#endif
		}

		[Test]
		public void FileRef_String_UTF7 ()
		{
			string refFile = Path.Combine (_otherTempDirectory, "string.txt");
			using (StreamWriter sw = new StreamWriter (refFile, false, Encoding.UTF7)) {
				sw.Write ("\u0021\u0026\u002A\u003B");
			}

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, refFile, 
					typeof (string).AssemblyQualifiedName, string.Empty,
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
#if NET_2_0
				IDictionaryEnumerator enumerator = r.GetEnumerator ();
				enumerator.MoveNext ();
				Assert.IsNotNull (enumerator.Current, "#A1");
				Assert.AreEqual ("foo", enumerator.Key, "#A2");
				Assert.IsFalse ("\u0021\u0026\u002A\u003B" == (string) enumerator.Value, "#A3");
#else
				try {
					r.GetEnumerator ();
					Assert.Fail ("#A1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
					Assert.IsNotNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsNull (ex.ParamName, "#A5");

					Assert.AreEqual (typeof (MissingMethodException), ex.InnerException.GetType (), "#A6");
				}
#endif
			}

			resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, refFile,
					typeof (string).AssemblyQualifiedName, ";utf-7",
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
#if NET_2_0
				IDictionaryEnumerator enumerator = r.GetEnumerator ();
				enumerator.MoveNext ();
				Assert.IsNotNull (enumerator.Current, "#B1");
				Assert.AreEqual ("foo", enumerator.Key, "#B2");
				Assert.AreEqual ("\u0021\u0026\u002A\u003B", (string) enumerator.Value, "#B3");
#else
				try {
					r.GetEnumerator ();
					Assert.Fail ("#B1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsNull (ex.ParamName, "#B5");

					Assert.AreEqual (typeof (MissingMethodException), ex.InnerException.GetType (), "#B6");
				}
#endif
			}
		}

		[Test]
		public void FileRef_String_UTF8 ()
		{
			string refFile = Path.Combine (_otherTempDirectory, "string.txt");
			using (StreamWriter sw = new StreamWriter (refFile, false, Encoding.UTF8)) {
				sw.Write ("\u0041\u2262\u0391\u002E");
			}

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, refFile,
					typeof (string).AssemblyQualifiedName, string.Empty,
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
#if NET_2_0
				IDictionaryEnumerator enumerator = r.GetEnumerator ();
				enumerator.MoveNext ();
				Assert.IsNotNull (enumerator.Current, "#A1");
				Assert.AreEqual ("foo", enumerator.Key, "#A2");
				Assert.AreEqual ("\u0041\u2262\u0391\u002E", (string) enumerator.Value, "#A3");
#else
				try {
					r.GetEnumerator ();
					Assert.Fail ("#A1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
					Assert.IsNotNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsNull (ex.ParamName, "#A5");

					Assert.AreEqual (typeof (MissingMethodException), ex.InnerException.GetType (), "#A6");
				}
#endif
			}

			resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, refFile,
					typeof (string).AssemblyQualifiedName, ";utf-8",
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
#if NET_2_0
				IDictionaryEnumerator enumerator = r.GetEnumerator ();
				enumerator.MoveNext ();
				Assert.IsNotNull (enumerator.Current, "#B1");
				Assert.AreEqual ("foo", enumerator.Key, "#B2");
				Assert.AreEqual ("\u0041\u2262\u0391\u002E", (string) enumerator.Value, "#B3");
#else
				try {
					r.GetEnumerator ();
					Assert.Fail ("#B1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsNull (ex.ParamName, "#B5");

					Assert.AreEqual (typeof (MissingMethodException), ex.InnerException.GetType (), "#B6");
				}
#endif
			}
		}

		[Test]
		public void Namespaces ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<o:Document xmlns:x=\"http://www.mono-project.com\" xmlns:o=\"http://tempuri.org\">" +
				"	<o:Content>" +
				"		<x:DaTa name=\"name\">" +
				"			<o:value>de Icaza</o:value>" +
				"		</x:DaTa>" +
				"		<x:data name=\"firstName\">" +
				"			<x:value />" +
				"		</x:data>" +
				"		<o:data name=\"Address\" />" +
				"		<o:data name=\"city\">" +
				"			<x:value>Boston </x:value>" +
				"		</o:data>" +
				"		<o:data name=\"country\">" +
				"			 United States " +
				"		</o:data>" +
				"		<o:data name=\"\">" +
				"			BO    " +
				"		</o:data>" +
				"		<o:data name=\"country\">" +
				"			<x:value>Belgium</x:value>" +
				"		</o:data>" +
				"		<data name=\"zip\">" +
				"			<value><![CDATA[ <3510> ]]></value>" +
				"		</data>" +
				"	</o:Content>" +
				"	<o:Paragraph>" +
				"		<o:resheader name=\"resmimetype\">" +
				"			<o:value>{0}</o:value>" +
				"		</o:resheader>" +
				"		<x:resheader name=\"version\">" +
				"			<o:value>{1}</o:value>" +
				"		</x:resheader>" +
				"	</o:Paragraph>" +
				"	<x:Section>" +
				"		<o:resheader name=\"reader\">" +
				"			<x:value>System.Resources.ResXResourceReader, {2}</x:value>" +
				"		</o:resheader>" +
				"		<x:resheader name=\"writer\">" +
				"			<x:value>System.Resources.ResXResourceWriter, {2}</x:value>" +
				"		</x:resheader>" +
				"	</x:Section>" +
				"</o:Document>";

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					resXTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms));
			}

			// Stream
			using (FileStream fs = new FileStream (resxFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				using (ResXResourceReader r = new ResXResourceReader (fs)) {
					IDictionaryEnumerator enumerator = r.GetEnumerator ();
					int entries = 0;
					while (enumerator.MoveNext ()) {
						entries++;
						switch ((string) enumerator.Key) {
						case "":
							Assert.IsNotNull (enumerator.Value, "#A1");
							Assert.AreEqual ("BO", enumerator.Value, "#A2");
							break;
						case "Address":
							Assert.IsNotNull (enumerator.Value, "#B1");
#if NET_2_0
							Assert.AreEqual (string.Empty, enumerator.Value, "#B2");
#else
							Assert.AreEqual ("Boston ", enumerator.Value, "#B2");
#endif
							break;
						case "country":
							Assert.IsNotNull (enumerator.Value, "#C1");
#if NET_2_0
							Assert.AreEqual (string.Empty, enumerator.Value, "#C2");
#else
							Assert.AreEqual ("Belgium", enumerator.Value, "#C2");
#endif
							break;
						case "firstName":
#if NET_2_0
							Assert.IsNull (enumerator.Value, "#D");
#else
							Assert.IsNotNull (enumerator.Value, "#D1");
							Assert.AreEqual (string.Empty, enumerator.Value, "#D2");
#endif
							break;
						case "zip":
							Assert.IsNotNull (enumerator.Value, "#E1");
							Assert.AreEqual (" <3510> ", enumerator.Value, "#E2");
							break;
						default:
							Assert.Fail ("#F:" + enumerator.Key);
							break;
						}
					}
					Assert.AreEqual (5, entries, "#G");
				}
			}
		}

		[Test]
		public void ResHeader_Missing ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<data name=\"name\">" +
				"		<value>de Icaza</value>" +
				"	</data>" +
				"	<data name=\"firstName\">" +
				"		<value />" +
				"	</data>" +
				"	<data name=\"address\" />" +
				"</root>";

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (resXTemplate);
			}

			// Stream
			using (FileStream fs = new FileStream (resxFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				using (ResXResourceReader r = new ResXResourceReader (fs)) {
					try {
						r.GetEnumerator ();
						Assert.Fail ("#A1");
					} catch (ArgumentException ex) {
						//Invalid ResX input.  Could not find valid \"resheader\"
						// tags for the ResX reader & writer type names
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
						Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#A5");
						Assert.IsNull (ex.ParamName, "#A6");
					}
				}
			}

			// File
			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#B1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#B5");
					Assert.IsNull (ex.ParamName, "#B6");
				}
			}

			// TextReader
			using (StreamReader sr = new StreamReader (resxFile, Encoding.UTF8)) {
				using (ResXResourceReader r = new ResXResourceReader (sr)) {
					try {
						r.GetEnumerator ();
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						//Invalid ResX input.  Could not find valid \"resheader\"
						// tags for the ResX reader & writer type names
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNull (ex.InnerException, "#C3");
						Assert.IsNotNull (ex.Message, "#C4");
						Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#C5");
						Assert.IsNull (ex.ParamName, "#C6");
					}
				}
			}
		}

		[Test]
		public void ResHeader_ResMimeType ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resMIMEtype\">" +
				"		{0}" +
				"	</resheader>" +
				"	<resheader name=\"version\">" +
				"		<value>{1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reAder\">" +
				"		<value>System.Resources.ResXResourceReader, {2}</value>" +
				"	</resheader>" +
				"	<resheader name=\"wriTer\">" +
				"		<value>System.Resources.ResXResourceWriter, {2}</value>" +
				"	</resheader>" +
				"</root>";

			string resXContent = null;

			// <value> element, exact case
			resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType, "1.0", 
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				r.GetEnumerator ();
			}

			// <value> element, uppercase
			resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType.ToUpper (), "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#A1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#A5");
					Assert.IsNull (ex.ParamName, "#A6");
				}
			}

			// text, exact case
			resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType, "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				r.GetEnumerator ();
			}

			// text, uppercase
			resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType.ToUpper (), "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#B1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#B5");
					Assert.IsNull (ex.ParamName, "#B6");
				}
			}

			// CDATA, exact case
			resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, "<![CDATA[" +ResXResourceWriter.ResMimeType + "]]>",
				"1.0", Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				r.GetEnumerator ();
			}

			// CDATA, uppercase
			resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, "<![CDATA[" + ResXResourceWriter.ResMimeType.ToUpper () + "]]>",
				"1.0", Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#C1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#C5");
					Assert.IsNull (ex.ParamName, "#C6");
				}
			}

			// <whatever> element, exact case
			resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, "<whatever>" + ResXResourceWriter.ResMimeType + "</whatever>",
				"1.0", Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				r.GetEnumerator ();
			}

			// <whatever> element, uppercase
			resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, "<whatever>" + ResXResourceWriter.ResMimeType.ToUpper () + "</whatever>",
				"1.0", Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#D1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
					Assert.IsNull (ex.InnerException, "#D3");
					Assert.IsNotNull (ex.Message, "#D4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#D5");
					Assert.IsNull (ex.ParamName, "#D6");
				}
			}
		}

		[Test]
		public void ResHeader_ResMimeType_Empty ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\"></resheader>" +
				"</root>";

			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXTemplate))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#5");
					Assert.IsNull (ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void ResHeader_ResMimeType_Invalid ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		<value>{0}</value>" +
				"	</resheader>" +
				"	<resheader name=\"version\">" +
				"		<value>{1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reader\">" +
				"		<value>System.Resources.ResXResourceReader, {2}</value>" +
				"	</resheader>" +
				"	<resheader name=\"writer\">" +
				"		<value>System.Resources.ResXResourceWriter, {2}</value>" +
				"	</resheader>" +
				"	<data name=\"name\">" +
				"		<value>de Icaza</value>" +
				"	</data>" +
				"	<data name=\"firstName\">" +
				"		<value />" +
				"	</data>" +
				"	<data name=\"Address\" />" +
				"</root>";

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					resXTemplate, "notvalid", "1.0", Consts.AssemblySystem_Windows_Forms
					));
			}

			// Stream
			using (FileStream fs = new FileStream (resxFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				using (ResXResourceReader r = new ResXResourceReader (fs)) {
					try {
						r.GetEnumerator ();
						Assert.Fail ("#A1");
					} catch (ArgumentException ex) {
						//Invalid ResX input.  Could not find valid \"resheader\"
						// tags for the ResX reader & writer type names
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
						Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#A5");
						Assert.IsNull (ex.ParamName, "#A6");
					}
				}
			}

			// File
			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#B1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#B5");
					Assert.IsNull (ex.ParamName, "#B6");
				}
			}

			// TextReader
			using (StreamReader sr = new StreamReader (resxFile, Encoding.UTF8)) {
				using (ResXResourceReader r = new ResXResourceReader (sr)) {
					try {
						r.GetEnumerator ();
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						//Invalid ResX input.  Could not find valid \"resheader\"
						// tags for the ResX reader & writer type names
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNull (ex.InnerException, "#C3");
						Assert.IsNotNull (ex.Message, "#C4");
						Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#C5");
						Assert.IsNull (ex.ParamName, "#C6");
					}
				}
			}
		}

		[Test]
		public void ResHeader_ResMimeType_Null ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\" />" +
				"</root>";

			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXTemplate))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#5");
					Assert.IsNull (ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void ResHeader_Reader_Invalid ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		{0}" +
				"	</resheader>" +
				"	<resheader name=\"version\">" +
				"		<value>{1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reader\">" +
				"		<value>System.Resources.InvalidResXResourceReader, {2}</value>" +
				"	</resheader>" +
				"	<resheader name=\"writer\">" +
				"		<value>System.Resources.ResXResourceWriter, {2}</value>" +
				"	</resheader>" +
				"</root>";

			string resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType, "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#5");
					Assert.IsNull (ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void ResHeader_Reader_Missing ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		{0}" +
				"	</resheader>" +
				"	<resheader name=\"version\">" +
				"		<value>{1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"writer\">" +
				"		<value>System.Resources.ResXResourceWriter, {2}</value>" +
				"	</resheader>" +
				"</root>";

			string resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType, "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#5");
					Assert.IsNull (ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void ResHeader_Reader_Null ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		{0}" +
				"	</resheader>" +
				"	<resheader name=\"version\">" +
				"		<value>{1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reader\" />" +
				"	<resheader name=\"writer\">" +
				"		<value>System.Resources.ResXResourceWriter, {2}</value>" +
				"	</resheader>" +
				"</root>";

			string resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType, "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#5");
					Assert.IsNull (ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void ResHeader_Writer_Invalid ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		{0}" +
				"	</resheader>" +
				"	<resheader name=\"version\">" +
				"		<value>{1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reader\">" +
				"		<value>System.Resources.ResXResourceReader, {2}</value>" +
				"	</resheader>" +
				"	<resheader name=\"writer\">" +
				"		<value>System.Resources.InvalidResXResourceWriter, {2}</value>" +
				"	</resheader>" +
				"</root>";

			string resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType, "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#5");
					Assert.IsNull (ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void ResHeader_Writer_Missing ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		{0}" +
				"	</resheader>" +
				"	<resheader name=\"version\">" +
				"		<value>{1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reader\">" +
				"		<value>System.Resources.ResXResourceReader, {2}</value>" +
				"	</resheader>" +
				"</root>";

			string resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType, "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#5");
					Assert.IsNull (ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void ResHeader_Writer_Null ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		{0}" +
				"	</resheader>" +
				"	<resheader name=\"version\">" +
				"		<value>{1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reader\">" +
				"		<value>System.Resources.ResXResourceReader, {2}</value>" +
				"	</resheader>" +
				"	<resheader name=\"writer\" />" +
				"</root>";

			string resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType, "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				try {
					r.GetEnumerator ();
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					//Invalid ResX input.  Could not find valid \"resheader\"
					// tags for the ResX reader & writer type names
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf ("\"resheader\"") != -1, "#5");
					Assert.IsNull (ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void ResHeader_Unknown ()
		{
			const string resXTemplate =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		{0}" +
				"	</resheader>" +
				"	<resheader name=\"version\">" +	
				"		<value>{1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reader\">" +
				"		<value>  System.Resources.ResXResourceReader  , {2}</value>" +
				"	</resheader>" +
				"	<resheader name=\"writer\">" +
				"		<value>  System.Resources.ResXResourceWriter  , {2}</value>" +
				"	</resheader>" +
				"	<resheader name=\"UNKNOWN\">" +
				"		<value>whatever</value>" +
				"	</resheader>" +
				"</root>";

			string resXContent = string.Format (CultureInfo.InvariantCulture,
				resXTemplate, ResXResourceWriter.ResMimeType, "1.0",
				Consts.AssemblySystem_Windows_Forms);
			using (ResXResourceReader r = new ResXResourceReader (new StringReader (resXContent))) {
				r.GetEnumerator ();
			}
		}

		[Test]
		public void ResName_Null ()
		{
			const string resXContent =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		<value>invalid</value>" +
				"	</resheader>" +
				"	<data name=\"name\">" +
				"		<value>de Icaza</value>" +
				"	</data>" +
				"	<data>" +
				"		<value>whatever</value>" +
				"	</data>" +
				"</root>";

			using (StringReader sr = new StringReader (resXContent)) {
				using (ResXResourceReader r = new ResXResourceReader (sr)) {
					try {
						r.GetEnumerator ();
						Assert.Fail ("#1");
					} catch (ArgumentException ex) {
						// Invalid ResX input.
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
						Assert.IsNotNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.IsNull (ex.ParamName, "#5");

#if NET_2_0
						// Could not find a name for a resource. The resource
						// value was 'whatever'. Line 1, position 200
						XmlException inner = ex.InnerException as XmlException;
						Assert.IsNotNull (inner, "#6");
						Assert.AreEqual (typeof (XmlException), inner.GetType (), "#7");
						Assert.IsNotNull (inner.InnerException, "#8");
						Assert.AreEqual (1, inner.LineNumber, "#9");
//						Assert.AreEqual (200, inner.LinePosition, "#10");
						Assert.IsNotNull (inner.Message, "#11");
						Assert.IsTrue (inner.Message.IndexOf ("'whatever'") != -1, "#12");
						Assert.IsTrue (inner.Message.IndexOf (" 1") != -1, "#13");
						//Assert.IsTrue (inner.Message.IndexOf ("200") != -1, "#14");

						// Could not find a name for a resource. The resource
						// value was 'whatever'
						ArgumentException inner2 = inner.InnerException as ArgumentException;
						Assert.IsNotNull (inner2, "#15");
						Assert.AreEqual (typeof (ArgumentException), inner2.GetType (), "#16");
						Assert.IsNull (inner2.InnerException, "#17");
						Assert.IsNotNull (inner2.Message, "#18");
						Assert.IsTrue (inner2.Message.IndexOf ("'whatever'") != -1, "#19");
						Assert.IsNull (inner2.ParamName, "#20");
#else
						// Could not find a name for a resource. The resource
						// value was 'whatever'
						ArgumentException inner = ex.InnerException as ArgumentException;
						Assert.IsNotNull (inner, "#6");
						Assert.AreEqual (typeof (ArgumentException), inner.GetType (), "#7");
						Assert.IsNull (inner.InnerException, "#8");
						Assert.IsNotNull (inner.Message, "#9");
						Assert.IsTrue (inner.Message.IndexOf ("'whatever'") != -1, "#10");
						Assert.IsNull (inner.ParamName, "#11");
#endif
					}
 				}
			}
		}

        static string resXWithEmptyName =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="""">
	<value>a resource with no name</value>
  </data>
</root>";

		static string resxWithNullRef =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name=""NullRef"" type=""System.Resources.ResXNullRef, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"">
	<value></value>
  </data>
</root>";

		[Test]
		public void ResName_Empty ()
		{
			using (StringReader sr = new StringReader (resXWithEmptyName)) {
				using (ResXResourceReader r = new ResXResourceReader (sr)) {
					IDictionaryEnumerator enumerator = r.GetEnumerator ();
					enumerator.MoveNext ();
					Assert.AreEqual ("", enumerator.Key, "#A1");
					Assert.AreEqual ("a resource with no name", (string) enumerator.Value, "#A2");
				}
			}
		}

		[Test]
		public void ResXNullRef ()
		{
			using (StringReader sr = new StringReader (resxWithNullRef)) {
				using (ResXResourceReader r = new ResXResourceReader (sr)) {
					IDictionaryEnumerator enumerator = r.GetEnumerator ();
					enumerator.MoveNext ();
					Assert.AreEqual ("NullRef", enumerator.Key, "#A1");
					Assert.IsNull (enumerator.Value, "#A2");
				}
			}
		}

		[Test]
		public void ResValue ()
		{
			string resXContent = string.Format (CultureInfo.CurrentCulture,
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		<value>{0}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reader\">" +
				"		<value>System.Resources.ResXResourceReader, {1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"writer\">" +
				"		<value>System.Resources.ResXResourceWriter, {1}</value>" +
				"	</resheader>" +
				"	<data name=\"name1\">" +
				"		<value><![CDATA[ <value1> ]]></value>" +
				"	</data>" +
				"	<data name=\"name2\">" +
				"		<value>  <![CDATA[<value2>]]>  </value>" +
				"	</data>" +
				"	<data name=\"name3\">" +
				"		  <![CDATA[<value3>]]>  " +
				"	</data>" +
				"	<data name=\"name4\">" +
				"		<value> value4 </value>" +
				"	</data>" +
				"	<data name=\"name5\">" +
				"		test<value>value5</value>" +
				"	</data>" +
				"	<data name=\"name6\">" +
				"		test1<value>value6</value>test2" +
				"	</data>" +
				"	<data name=\"name7\">" +
				"		<value>value7a</value>" +
				"		<whatever>value7b</whatever>" +
				"	</data>" +
				"	<data name=\"name8\">" +
				"		<whatever>value8</whatever>" +
				"	</data>" +
				"	<data name=\"name9\">" +
				"		<whatever>value9a</whatever>" +
				"		<whatever>value9b</whatever>" +
				"	</data>" +
				"	<data name=\"name10\">" +
				"		test<whatever>value10</whatever>" +
				"	</data>" +
				"	<data name=\"name11\">" +
				"		test1<whatever>value11</whatever>test2" +
				"	</data>" +
				"	<data name=\"name12\">" +
				"		<value> test  <![CDATA[<value12>]]>  </value>" +
				"	</data>" +
				"	<data name=\"name13\">" +
				"		 test <![CDATA[<value13>]]>  " +
				"	</data>" +
				"	<data name=\"name14\" />" +
				"	<data name=\"name15\"></data>" +
				"	<data name=\"name16\">value16</data>" +
				"	<data name=\"name17\">value17</data>" +
				"	<data name=\"name18\">" +
				"		<value>value18</value>" +
				"		<data name=\"name19\">" +
				"			<value>value18</value>" +
				"		</data>" +
				"	</data>" +
				"</root>",
				ResXResourceWriter.ResMimeType, Consts.AssemblySystem_Windows_Forms);

			using (StringReader sr = new StringReader (resXContent)) {
				using (ResXResourceReader r = new ResXResourceReader (sr)) {

					IDictionaryEnumerator enumerator = r.GetEnumerator ();
					int entries = 0;
					while (enumerator.MoveNext ()) {
						entries++;
						switch ((string) enumerator.Key) {
						case "name1":
							Assert.IsNotNull (enumerator.Value, "#A1");
							Assert.AreEqual (" <value1> ", enumerator.Value, "#A2");
							break;
						case "name2":
							Assert.IsNotNull (enumerator.Value, "#B1");
							Assert.AreEqual ("<value2>", enumerator.Value, "#B2");
							break;
						case "name3":
							Assert.IsNotNull (enumerator.Value, "#C1");
							Assert.AreEqual ("<value3>", enumerator.Value, "#C2");
							break;
						case "name4":
							Assert.IsNotNull (enumerator.Value, "#D1");
							Assert.AreEqual (" value4 ", enumerator.Value, "#D2");
							break;
						case "name5":
							Assert.IsNotNull (enumerator.Value, "#E1");
#if NET_2_0
							Assert.AreEqual ("value5", enumerator.Value, "#E2");
#else
							Assert.AreEqual ("test", enumerator.Value, "#E2");
#endif
							break;
						case "name6":
							Assert.IsNotNull (enumerator.Value, "#F1");
#if NET_2_0
							Assert.AreEqual ("test2", enumerator.Value, "#F2");
#else
							Assert.AreEqual ("test1", enumerator.Value, "#F2");
#endif
							break;
						case "name7":
							Assert.IsNotNull (enumerator.Value, "#G1");
#if NET_2_0
							Assert.AreEqual (string.Empty, enumerator.Value, "#G2");
#else
							Assert.AreEqual ("value7a", enumerator.Value, "#G2");
#endif
							break;
						case "name8":
							Assert.IsNotNull (enumerator.Value, "#H1");
#if NET_2_0
							Assert.AreEqual (string.Empty, enumerator.Value, "#H2");
#else
							Assert.AreEqual ("value8", enumerator.Value, "#H2");
#endif
							break;
						case "name9":
							Assert.IsNotNull (enumerator.Value, "#I1");
#if NET_2_0
							Assert.AreEqual (string.Empty, enumerator.Value, "#I2");
#else
							Assert.AreEqual ("value9a", enumerator.Value, "#I2");
#endif
							break;
						case "name10":
							Assert.IsNotNull (enumerator.Value, "#J1");
#if NET_2_0
							Assert.AreEqual (string.Empty, enumerator.Value, "#J2");
#else
							Assert.AreEqual ("test", enumerator.Value, "#J2");
#endif
							break;
						case "name11":
							Assert.IsNotNull (enumerator.Value, "#K1");
#if NET_2_0
							Assert.AreEqual ("test2", enumerator.Value, "#K2");
#else
							Assert.AreEqual ("test1", enumerator.Value, "#K2");
#endif
							break;
						case "name12":
							Assert.IsNotNull (enumerator.Value, "#L1");
							Assert.AreEqual (" test  <value12>", enumerator.Value, "#L2");
							break;
						case "name13":
							Assert.IsNotNull (enumerator.Value, "#M1");
#if NET_2_0
							Assert.AreEqual ("<value13>", enumerator.Value, "#M2");
#else
							Assert.AreEqual ("test", enumerator.Value, "#M2");
#endif
							break;
						case "name14":
#if NET_2_0
							Assert.IsNull (enumerator.Value, "#N1");
#else
							Assert.IsNotNull (enumerator.Value, "#N1");
							Assert.AreEqual (string.Empty, enumerator.Value, "#N2");
#endif
							break;
#if NET_2_0
						case "name16":
							Assert.IsNotNull (enumerator.Value, "#O1");
							Assert.AreEqual ("value16", enumerator.Value, "#O2");
							break;
#endif
						case "name17":
							Assert.IsNotNull (enumerator.Value, "#P1");
							Assert.AreEqual ("value17", enumerator.Value, "#P2");
							break;
						case "name18":
							Assert.IsNotNull (enumerator.Value, "#Q1");
							Assert.AreEqual ("value18", enumerator.Value, "#Q2");
							break;
						default:
							Assert.Fail ("#Q:" + enumerator.Key);
							break;
						}
					}
#if NET_2_0
					Assert.AreEqual (17, entries, "#Q");
#else
					Assert.AreEqual (16, entries, "#Q");
#endif
				}
			}
		}

#if NET_2_0
		[Test]
		public void UseResXDataNodes ()
		{
			string refFile = Path.Combine (_tempDirectory, "32x32.ico");
			WriteEmbeddedResource ("32x32.ico", refFile);

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, refFile,
					typeof (Bitmap).AssemblyQualifiedName, string.Empty,
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				r.UseResXDataNodes = true;
				IDictionaryEnumerator enumerator = r.GetEnumerator ();

				int entries = 0;
				while (enumerator.MoveNext ()) {
					entries++;

					ResXDataNode node = enumerator.Value as ResXDataNode;

					switch ((string) enumerator.Key) {
					case "foo":
						Assert.AreEqual ("foo", node.Name, "#A1");
						Bitmap bitmap = node.GetValue (new AssemblyName[] {typeof (Bitmap).Assembly.GetName ()}) as Bitmap;
						Assert.IsNotNull (bitmap, "#A2");
						break;
					case "panel_label.Locked":
						Assert.AreEqual ("panel_label.Locked", node.Name, "#B1");
						Assert.AreEqual (true, node.GetValue (new AssemblyName[] {typeof (int).Assembly.GetName ()}), "#B2");
						break;
					default:
						Assert.Fail ("#C:" + enumerator.Key);
						break;
					}
				}
				Assert.AreEqual (2, entries, "#D");
			}
		}
		
		[Test]
		[Category ("NotWorking")]
		public void ResXDataNode_GetNodePosition ()
		{
			// This test relies on a hashtable's enumerator being ordered,
			// when the ordering is not guaranteed.
			string refFile = Path.Combine (_tempDirectory, "32x32.ico");
			WriteEmbeddedResource ("32x32.ico", refFile);

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, refFile,
					typeof (Bitmap).AssemblyQualifiedName, string.Empty,
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				r.UseResXDataNodes = true;
				IDictionaryEnumerator enumerator = r.GetEnumerator ();
				enumerator.MoveNext ();
				ResXDataNode node = enumerator.Value as ResXDataNode;
				Assert.IsNotNull (node, "#A1");
				Assert.AreEqual(new Point(1, 717), node.GetNodePosition (), "#A2");
			}
		}

		[Test]
		public void GetMetadataEnumerator ()
		{
			string refFile = Path.Combine (_tempDirectory, "32x32.ico");
			WriteEmbeddedResource ("32x32.ico", refFile);

			string resxFile = Path.Combine (_tempDirectory, "resources.resx");
			using (StreamWriter sw = new StreamWriter (resxFile, false, Encoding.UTF8)) {
				sw.Write (string.Format (CultureInfo.InvariantCulture,
					_resXFileRefTemplate, ResXResourceWriter.ResMimeType, "1.0",
					Consts.AssemblySystem_Windows_Forms, refFile,
					typeof (Bitmap).AssemblyQualifiedName, string.Empty,
					Consts.AssemblyCorlib));
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				IDictionaryEnumerator enumerator = r.GetMetadataEnumerator ();
				Assert.IsTrue (enumerator.MoveNext (), "#A1");
				Assert.IsNotNull (enumerator.Current, "#A2");
				Assert.AreEqual ("panel_label.Locked", enumerator.Key, "#A3");
				Assert.AreEqual(typeof(bool), enumerator.Value.GetType(), "#A4");
				Assert.IsTrue ((bool) enumerator.Value, "#A5");
				Assert.IsFalse (enumerator.MoveNext (), "#A6");
			}

			using (ResXResourceReader r = new ResXResourceReader (resxFile)) {
				r.UseResXDataNodes = true;
				IDictionaryEnumerator enumerator = r.GetMetadataEnumerator ();
				Assert.IsFalse (enumerator.MoveNext (), "#B1");
			}
		}
#endif

		[Test]
		public void TypeConversion ()
		{
			string resXContent = string.Format (CultureInfo.CurrentCulture,
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root>" +
				"	<resheader name=\"resmimetype\">" +
				"		<value>{0}</value>" +
				"	</resheader>" +
				"	<resheader name=\"reader\">" +
				"		<value>System.Resources.ResXResourceReader, {1}</value>" +
				"	</resheader>" +
				"	<resheader name=\"writer\">" +
				"		<value>System.Resources.ResXResourceWriter, {1}</value>" +
				"	</resheader>" +
				"	<data name=\"AnchorStyle\" type=\"System.Windows.Forms.AnchorStyles, {1}\">" +
				"		<value>Bottom, Left</value>" +
				"	</data>" +
				"	<data name=\"BackgroundImage\" type=\"{2}\" mimetype=\"application/x-microsoft.net.object.bytearray.base64\">" +
				"		<value>" +
				"			Qk12BQAAAAAAADYEAAAoAAAAEgAAABAAAAABAAgAAAAAAAAAAAAgHAAAIBwAAAABAAAAAQAAAAAA/wAA" +
				"			M/8AAGb/AACZ/wAAzP8AAP//ADMA/wAzM/8AM2b/ADOZ/wAzzP8AM///AGYA/wBmM/8AZmb/AGaZ/wBm" +
				"			zP8AZv//AJkA/wCZM/8AmWb/AJmZ/wCZzP8Amf//AMwA/wDMM/8AzGb/AMyZ/wDMzP8AzP//AP8A/wD/" +
				"			M/8A/2b/AP+Z/wD/zP8A////MwAA/zMAM/8zAGb/MwCZ/zMAzP8zAP//MzMA/zMzM/8zM2b/MzOZ/zMz" +
				"			zP8zM///M2YA/zNmM/8zZmb/M2aZ/zNmzP8zZv//M5kA/zOZM/8zmWb/M5mZ/zOZzP8zmf//M8wA/zPM" +
				"			M/8zzGb/M8yZ/zPMzP8zzP//M/8A/zP/M/8z/2b/M/+Z/zP/zP8z////ZgAA/2YAM/9mAGb/ZgCZ/2YA" +
				"			zP9mAP//ZjMA/2YzM/9mM2b/ZjOZ/2YzzP9mM///ZmYA/2ZmM/9mZmb/ZmaZ/2ZmzP9mZv//ZpkA/2aZ" +
				"			M/9mmWb/ZpmZ/2aZzP9mmf//ZswA/2bMM/9mzGb/ZsyZ/2bMzP9mzP//Zv8A/2b/M/9m/2b/Zv+Z/2b/" +
				"			zP9m////mQAA/5kAM/+ZAGb/mQCZ/5kAzP+ZAP//mTMA/5kzM/+ZM2b/mTOZ/5kzzP+ZM///mWYA/5lm" +
				"			M/+ZZmb/mWaZ/5lmzP+ZZv//mZkA/5mZM/+ZmWb/mZmZ/5mZzP+Zmf//mcwA/5nMM/+ZzGb/mcyZ/5nM" +
				"			zP+ZzP//mf8A/5n/M/+Z/2b/mf+Z/5n/zP+Z////zAAA/8wAM//MAGb/zACZ/8wAzP/MAP//zDMA/8wz" +
				"			M//MM2b/zDOZ/8wzzP/MM///zGYA/8xmM//MZmb/zGaZ/8xmzP/MZv//zJkA/8yZM//MmWb/zJmZ/8yZ" +
				"			zP/Mmf//zMwA/8zMM//MzGb/zMyZ/8zMzP/MzP//zP8A/8z/M//M/2b/zP+Z/8z/zP/M/////wAA//8A" +
				"			M///AGb//wCZ//8AzP//AP///zMA//8zM///M2b//zOZ//8zzP//M////2YA//9mM///Zmb//2aZ//9m" +
				"			zP//Zv///5kA//+ZM///mWb//5mZ//+ZzP//mf///8wA///MM///zGb//8yZ///MzP//zP////8A////" +
				"			M////2b///+Z////zP//////AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAA" +
				"			AP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAA" +
				"			AP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAA" +
				"			AAAAAAAAAAwMDAwAAAAAAAAAAAAAAAAAAAAAAAwMDAwAAAAAAAAAAAAAAAAAAAAAAAwMDAAAAAAAAAAA" +
				"			AAAAAAAAAAAAAAwMDAAAAAAAAAAAAAAAAAAAAAAADAwMAAAAAAAAAAAADAAAAAAAAAAMDA0AAAAAAAAA" +
				"			AAwMDQAAABMTExMTExMTNwAAAAAMDAwMDQAAABMTExMTExMAAAAANzc3Nzc3NwAAAD4+Pj4+AAAAAD4+" +
				"			Pj4+Pj4+PgAAAGJiYgAAAAAAAAAAYmJiAAAAAAAAAGIAAAAAAAAAAABiYmIAAAAAAAAAAAAAAAAAAAAA" +
				"			AGJiYgAAAAAAAAAAAAAAAAAAAAAAAGJiYgAAAAAAAAAAAAAAAAAAAAAAAGJiYgAAAAAAAAAAAAAAAAAA" +
				"			AAAAAGJiYgAAAAAAAAAAAAAA" +
				"		</value>" +
				"	</data>" +
				"	<data name=\"Buffer\" type=\"{3}\">" +
				"		<value>BQIH</value>" +
				"	</data>" +
				"	<data name=\"Data\" mimetype=\"application/x-microsoft.net.object.bytearray.base64\">" +
				"		<value>Random Thoughts</value>" +
				"	</data>" +
				/*s*/"	<data name=\"Foo\" type=\"System.Windows.Forms.Application, {1}\">" +
				"		<value>A B C</value>" +
				"	</data>" +
				"	<data name=\"Image\" type=\"{2}\">" +
				"		<value>Summer.jpg</value>" +
				"	</data>" +
				/*e*/"	<data name=\"Text\">" +
				"		<value>OK</value>" +
				"	</data>" +
				"	<data name=\"Unknown\" mimetype=\"application/xxx\">" +
				"		<value>MIA</value>" +
				"	</data>" +
				"	<data name=\"Wrong\" typeof=\"{2}\" mimetype=\"application/xxx\">" +
				"		<value>SuperUnknown</value>" +
				"	</data>" +
				/*s*/"	<data name=\"Xtra\" type=\"System.Windows.Forms.AnchorStyles, {1}\" mimetype=\"application/x-microsoft.net.object.bytearray.base64\">" +
				"		<value>LeftRight</value>" +
				"	</data>" +
				/*e*/"</root>",
				ResXResourceWriter.ResMimeType, Consts.AssemblySystem_Windows_Forms,
				typeof (Bitmap).AssemblyQualifiedName, typeof (byte []).AssemblyQualifiedName);

			using (StringReader sr = new StringReader (resXContent)) {
				using (ResXResourceReader r = new ResXResourceReader (sr)) {
					IDictionaryEnumerator enumerator = r.GetEnumerator ();
					int entries = 0;
					while (enumerator.MoveNext ()) {
						entries++;
						switch ((string) enumerator.Key) {
						case "AnchorStyle":
							Assert.IsNotNull (enumerator.Value, "#A1");
							Assert.AreEqual (AnchorStyles.Bottom | AnchorStyles.Left, enumerator.Value, "#A2");
							break;
						case "BackgroundImage":
							Assert.IsNotNull (enumerator.Value, "#B1");
							Assert.AreEqual (typeof (Bitmap), enumerator.Value.GetType (), "#B2");
							break;
						case "Buffer":
							Assert.IsNotNull (enumerator.Value, "#C1");
							Assert.AreEqual (new byte [] { 5, 2, 7 }, enumerator.Value, "#C2");
							break;
						case "Data":
#if NET_2_0
							Assert.IsNull (enumerator.Value, "#D1");
#else
							Assert.IsNotNull (enumerator.Value, "#D1");
							Assert.AreEqual ("Random Thoughts", enumerator.Value, "#D2");
#endif
							break;
						case "Foo":
#if NET_2_0
							Assert.IsNull (enumerator.Value, "#E1");
#else
							Assert.IsNotNull (enumerator.Value, "#E1");
							Assert.AreEqual ("A B C", enumerator.Value, "#E2");
#endif
							break;
						case "Image":
#if NET_2_0
							Assert.IsNull (enumerator.Value, "#F1");
#else
							Assert.IsNotNull (enumerator.Value, "#F1");
							Assert.AreEqual ("Summer.jpg", enumerator.Value, "#F2");
#endif
							break;
						case "Text":
							Assert.IsNotNull (enumerator.Value, "#G1");
							Assert.AreEqual ("OK", enumerator.Value, "#G2");
							break;
						case "Unknown":
#if NET_2_0
							Assert.IsNull (enumerator.Value, "#H1");
#else
							Assert.IsNotNull (enumerator.Value, "#H1");
							Assert.AreEqual ("MIA", enumerator.Value, "#H2");
#endif
							break;
						case "Wrong":
#if NET_2_0
							Assert.IsNull (enumerator.Value, "#I1");
#else
							Assert.IsNotNull (enumerator.Value, "#I1");
							Assert.AreEqual ("SuperUnknown", enumerator.Value, "#I2");
#endif
							break;
						case "Xtra":
#if NET_2_0
							Assert.IsNull (enumerator.Value, "#J1");
#else
							Assert.IsNotNull (enumerator.Value, "#J1");
							Assert.AreEqual ("LeftRight", enumerator.Value, "#J2");
#endif
							break;
						default:
							Assert.Fail ("#J:" + enumerator.Key);
							break;
						}
					}
					Assert.AreEqual (10, entries, "#G");
				}
			}
		}

		[Test, ExpectedException (typeof (SerializationException))]
		public void DeSerializationErrorBubbles ()
		{
			using (StringReader sr = new StringReader (serializedResXCorruped)) {
				using (ResXResourceReader r = new ResXResourceReader (sr)) {
					IDictionaryEnumerator enumerator = r.GetEnumerator ();
					// should throw exception
				}
			}
		}

		[Test, ExpectedException (typeof (TargetInvocationException))]
		public void FileRef_DeserializationFails ()
		{
			string corruptFile = Path.GetTempFileName ();
			ResXFileRef fileRef = new ResXFileRef (corruptFile, typeof (serializable).AssemblyQualifiedName);

			File.AppendAllText (corruptFile,"corrupt");

			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter (sb)) {
				using (ResXResourceWriter writer = new ResXResourceWriter (sw)) {
					writer.AddResource ("test", fileRef);
				}
			}

			using (StringReader sr = new StringReader (sb.ToString ())) {
				using (ResXResourceReader reader = new ResXResourceReader (sr)) {
					reader.GetEnumerator ();
				}
			}
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void FileRef_TypeCantBeResolved ()
		{
			string aFile = Path.GetTempFileName ();
			ResXFileRef fileRef = new ResXFileRef (aFile, "a.type.doesnt.exist");

			StringBuilder sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				using (ResXResourceWriter writer = new ResXResourceWriter (sw)) {
					writer.AddResource ("test", fileRef);
				}
			}

			using (StringReader sr = new StringReader (sb.ToString ())) {
				using (ResXResourceReader reader = new ResXResourceReader (sr)) {
					reader.GetEnumerator ();
				}
			}
		}

		[Test]
		public void TypeConverter_AssemblyNamesUsed ()
		{
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			StringReader sr = new StringReader (convertableResXWithoutAssemblyName);

			using (ResXResourceReader rr = new ResXResourceReader (sr, assemblyNames)) {
				IDictionaryEnumerator en = rr.GetEnumerator ();
				en.MoveNext ();

				object obj = ((DictionaryEntry) en.Current).Value;
				Assert.IsNotNull (obj, "#A1");
				Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName, "#A2");
			}

		}

		[Test]
		public void TypeConverter_ITRSUsed ()
		{
			ResXDataNode dn = new ResXDataNode ("test", 34L);

			StringBuilder sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				using (ResXResourceWriter writer = new ResXResourceWriter (sw)) {
					writer.AddResource (dn);
				}
			}

			using (StringReader sr = new StringReader (sb.ToString ())) {
				ResXResourceReader rr = new ResXResourceReader (sr, new ReturnIntITRS ());
	 			IDictionaryEnumerator en = rr.GetEnumerator ();
				en.MoveNext ();

				object o = ((DictionaryEntry) en.Current).Value;
				Assert.IsNotNull (o, "#A1");
				Assert.IsInstanceOfType (typeof (int), o,"#A2");
				Assert.AreEqual (34, o,"#A3");
				rr.Close ();
			}
		}

		[Test]
		public void Serializable_ITRSUsed ()
		{
			serializable ser = new serializable ("aaaaa", "bbbbb");
			ResXDataNode dn = new ResXDataNode ("test", ser);

			StringBuilder sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				using (ResXResourceWriter writer = new ResXResourceWriter (sw)) {
					writer.AddResource (dn);
				}
			}

			using (StringReader sr = new StringReader (sb.ToString ())) {
				ResXResourceReader rr = new ResXResourceReader (sr, new ReturnSerializableSubClassITRS ());
	 			
				IDictionaryEnumerator en = rr.GetEnumerator ();
				en.MoveNext ();

				object o = ((DictionaryEntry) en.Current).Value;
				Assert.IsNotNull (o, "#A1");
				Assert.IsInstanceOfType (typeof (serializableSubClass), o,"#A2");
				rr.Close ();
			}
		}

		static string convertableResXWithoutAssemblyName =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  
  <data name=""test"" type=""DummyAssembly.Convertable"">
	<value>im a name	im a value</value>
  </data>
</root>";

		static string serializedResXCorruped =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name=""test"" mimetype=""application/x-microsoft.net.object.binary.base64"">
	<value>
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
</value>
  </data>
</root>";

		private static void WriteEmbeddedResource (string name, string filename)
		{
			const int size = 512;
			byte [] buffer = new byte [size];
			int count = 0;

			Stream input = typeof (ResXResourceReaderTest).Assembly.
				GetManifestResourceStream (name);
			Stream output = File.Open (filename, FileMode.Create);

			try {
				while ((count = input.Read (buffer, 0, size)) > 0) {
					output.Write (buffer, 0, count);
				}
			} finally {
				output.Close ();
			}
		}

		private const string _resXFileRefTemplate =
			"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
			"<root>" +
			"	<resheader name=\"resmimetype\">" +
			"		<value>{0}</value>" +
			"	</resheader>" +
			"	<resheader name=\"version\">" +
			"		<value>{1}</value>" +
			"	</resheader>" +
			"	<resheader name=\"reader\">" +
			"		<value>System.Resources.ResXResourceReader, {2}</value>" +
			"	</resheader>" +
			"	<resheader name=\"writer\">" +
			"		<value>System.Resources.ResXResourceWriter, {2}</value>" +
			"	</resheader>" +
#if NET_2_0
			"	<metadata name=\"panel_label.Locked\" type=\"System.Boolean, {6}\">" +
			"		<value>True</value>" +
			" 	</metadata>" +
#endif
			"	<data name=\"foo\" type=\"System.Resources.ResXFileRef, {2}\">" +
			"		<value>{3};{4}{5}</value>" +
			"	</data>" +
			"</root>";
	}
}
