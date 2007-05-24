//
// ResXResourceReaderTest.cs: Unit Tests for ResXResourceReader.
//
// Authors:
//     Gert Driesen <drieseng@users.sourceforge.net>
//

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResXResourceReaderTest
	{
		private string _tempDirectory;
		private string _otherTempDirectory;

		[SetUp]
		public void SetUp ()
		{
			_tempDirectory = Path.Combine (Path.GetTempPath (), "ResXResourceReaderTest");
			_otherTempDirectory = Path.Combine (_tempDirectory, "in");
			if (!Directory.Exists (_otherTempDirectory)) {
				Directory.CreateDirectory (_otherTempDirectory);
			}
		}

		[TearDown]
		public void TearDown ()
		{
			if (Directory.Exists (_tempDirectory))
				Directory.Delete (_tempDirectory, true);
		}

		[Test]
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

		[Test]
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

		[Test]
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
					typeof (Bitmap).AssemblyQualifiedName, string.Empty));
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
					typeof (Bitmap).AssemblyQualifiedName, string.Empty));
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
					typeof (StreamReader).AssemblyQualifiedName, string.Empty));
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
					typeof (string).AssemblyQualifiedName, string.Empty));
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
					typeof (string).AssemblyQualifiedName, ";utf-7"));
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
					typeof (string).AssemblyQualifiedName, string.Empty));
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
					typeof (string).AssemblyQualifiedName, ";utf-8"));
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
			"	<data name=\"foo\" type=\"System.Resources.ResXFileRef, {2}\">" +
			"		<value>{3};{4}{5}</value>" +
			"	</data>" +
			"</root>";
	}
}
