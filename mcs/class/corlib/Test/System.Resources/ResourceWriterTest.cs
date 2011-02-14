//
// ResourceWriterTest.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResourceWriterTest
	{
		private string tempFolder = null;

		[SetUp]
		public void SetUp ()
		{
			tempFolder = Path.Combine (Path.GetTempPath (),
				"MonoTests.System.Resources.ResourceWriterTest");
			if (!Directory.Exists (tempFolder))
				Directory.CreateDirectory (tempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			if (Directory.Exists (tempFolder))
				Directory.Delete (tempFolder, true);
		}

		[Test] // ctor (Stream)
		public void Constructor0_Stream_NotWritable ()
		{
			MemoryStream ms = new MemoryStream (new byte [0], false);

			try {
				new ResourceWriter (ms);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Stream was not writable
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // ctor (Stream)
		public void Constructor0_Stream_Null ()
		{
			try {
				new ResourceWriter ((Stream) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("stream", ex.ParamName, "#6");
			}
		}

		[Test] // ctor (string)
		public void Constructor1_FileName_Null ()
		{
			try {
				new ResourceWriter ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // AddResource (string, byte [])
		public void AddResource0 ()
		{
			byte [] value = new byte [] { 5, 7 };

			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", value);
			writer.Generate ();

			try {
				writer.AddResource ("Address", new byte [] { 8, 12 });
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#B1");
			Assert.AreEqual ("Name", enumerator.Key, "#B3");
			Assert.AreEqual (value, enumerator.Value, "#B4");
			Assert.IsFalse (enumerator.MoveNext (), "#B5");

			writer.Close ();
		}

		[Test] // AddResource (string, byte [])
		public void AddResource0_Name_Null ()
		{
			byte [] value = new byte [] { 5, 7 };

			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);

			try {
				writer.AddResource ((string) null, value);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}

			writer.Close ();
		}

		[Test] // AddResource (string, byte [])
		public void AddResource0_Value_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", (byte []) null);
			writer.Generate ();

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#1");
			Assert.AreEqual ("Name", enumerator.Key, "#2");
			Assert.IsNull (enumerator.Value, "#3");
			Assert.IsFalse (enumerator.MoveNext (), "#4");

			writer.Close ();
		}

		[Test] // AddResource (string, object)
		public void AddResource1 ()
		{
			TimeSpan value = new TimeSpan (2, 5, 8);

			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Interval", value);
			writer.Generate ();

			try {
				writer.AddResource ("Start", value);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#B1");
			Assert.AreEqual ("Interval", enumerator.Key, "#B3");
			Assert.AreEqual (value, enumerator.Value, "#B4");
			Assert.IsFalse (enumerator.MoveNext (), "#B5");

			writer.Close ();
		}

		[Test] // AddResource (string, object)
		public void AddResource1_Name_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);

			try {
				writer.AddResource ((string) null, new object ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}

			writer.Close ();
		}

		[Test] // AddResource (string, object)
		public void AddResource1_Value_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", (object) null);
			writer.Generate ();

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#1");
			Assert.AreEqual ("Name", enumerator.Key, "#2");
			Assert.IsNull (enumerator.Value, "#3");
			Assert.IsFalse (enumerator.MoveNext (), "#4");

			writer.Close ();
		}

		[Test] // AddResource (string, string)
		public void AddResource2 ()
		{
			String value = "Some\0Value\tOr\rAnother";

			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Text", value);
			writer.Generate ();

			try {
				writer.AddResource ("Description", value);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#B1");
			Assert.AreEqual ("Text", enumerator.Key, "#B3");
			Assert.AreEqual (value, enumerator.Value, "#B4");
			Assert.IsFalse (enumerator.MoveNext (), "#B5");

			writer.Close ();
		}

		[Test] // AddResource (string, string)
		public void AddResource2_Name_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);

			try {
				writer.AddResource ((string) null, "abc");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}

			writer.Close ();
		}

		[Test] // AddResource (string, string)
		public void AddResource2_Value_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", (string) null);
			writer.Generate ();

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#1");
			Assert.AreEqual ("Name", enumerator.Key, "#2");
			Assert.IsNull (enumerator.Value, "#3");
			Assert.IsFalse (enumerator.MoveNext (), "#4");

			writer.Close ();
		}

		[Test]
		public void AddResource_Closed ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", "Miguel");
			writer.Close ();

			try {
				writer.AddResource ("Address", "US");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void AddResource_Name_Duplicate ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("FirstName", "Miguel");

			try {
				writer.AddResource ("FirstNaMe", "Chris");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Item has already been added. Key is dictionary:
				// 'FirstName'  Key being added: 'FirstNaMe'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}

			writer.AddResource ("Name", "Miguel");
			writer.Close ();
		}

#if NET_4_0
		// We are using a FileStream instead of a MemoryStream
		// to test that we support all kind of Stream instances,
		// and not only MemoryStream, as it used to be before 4.0.
		[Test]
		public void AddResource_Stream_Default ()
		{
			MemoryStream stream = new MemoryStream ();
			byte [] buff = Encoding.Unicode.GetBytes ("Miguel");
			stream.Write (buff, 0, buff.Length);
			stream.Position = 0;

			ResourceWriter rw = new ResourceWriter ("Test/resources/AddResource_Stream.resources");
			rw.AddResource ("Name", (object)stream);
			rw.Close ();

			ResourceReader rr = new ResourceReader ("Test/resources/AddResource_Stream.resources");
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();

			// Get the first element
			Assert.AreEqual (true, enumerator.MoveNext (), "#A0");

			DictionaryEntry de = enumerator.Entry;
			Assert.AreEqual ("Name", enumerator.Key, "#A1");
			Stream result_stream = de.Value as Stream;
			Assert.AreEqual (true, result_stream != null, "#A2");

			// Get the data and compare
			byte [] result_buff = new byte [result_stream.Length];
			result_stream.Read (result_buff, 0, result_buff.Length);
			string string_res = Encoding.Unicode.GetString (result_buff);
			Assert.AreEqual ("Miguel", string_res, "#A3");

			rr.Close ();
			stream.Close ();
		}

		[Test]
		public void AddResource_Stream_Errors ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter rw = new ResourceWriter (ms);

			ResourceStream stream = new ResourceStream ("Test");
			stream.SetCanSeek (false);

			// 
			// Seek not supported.
			// 
			try {
				rw.AddResource ("Name", stream);
				Assert.Fail ("#Exc1");
			} catch (ArgumentException) {
			}

			//
			// Even using the overload taking an object
			// seems to check for that
			//
			try {
				rw.AddResource ("Name", (object)stream);
				Assert.Fail ("#Exc2");
			} catch (ArgumentException) {
			}

			rw.Close ();
		}

		[Test]
		public void AddResource_Stream_Details ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter rw = new ResourceWriter (ms);

			ResourceStream stream = new ResourceStream ("MonoTest");

			// Set Position so we can test the ResourceWriter is resetting
			// it to 0 when generating.
			stream.Position = 2;
			rw.AddResource ("Name", stream);
			rw.Generate ();

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			string value = GetStringFromResource (rr, "Name");
			Assert.AreEqual ("MonoTest", value, "#A1");
			Assert.AreEqual (false, stream.IsDiposed, "#A2");

			// Test the second overload
			stream.Reset ();
			ms = new MemoryStream ();
			rw = new ResourceWriter (ms);
			rw.AddResource ("Name", stream, true);
			rw.Generate ();

			ms.Position = 0;
			rr = new ResourceReader (ms);
			value = GetStringFromResource (rr, "Name");
			Assert.AreEqual ("MonoTest", value, "#B1");
			Assert.AreEqual (true, stream.IsDiposed, "#B2");

			rr.Close ();
			rw.Close ();
			stream.Close ();
		}

		string GetStringFromResource (ResourceReader reader, string name)
		{
			Stream s = null;

			foreach (DictionaryEntry de in reader)
				if ((string)de.Key == name)
					s = (Stream)de.Value;

			if (s == null)
				return null;

			byte [] buff = new byte [s.Length];
			s.Read (buff, 0, buff.Length);
			return Encoding.Unicode.GetString (buff, 0, buff.Length);
		}

		class ResourceStream : Stream
		{
			bool can_seek;
			bool disposed;
			byte [] buff;
			int pos;

			public ResourceStream (string src)
			{
				buff = Encoding.Unicode.GetBytes (src);
				Reset ();
			}

			public void Reset ()
			{
				can_seek = true;
				pos = 0;
			}

			public override bool CanRead
			{
				get { 
					return true; 
				}
			}

			public override bool CanWrite
			{
				get { 
					throw new NotSupportedException (); 
				}
			}

			public override bool CanSeek
			{
				get { 
					return can_seek; 
				}
			}

			public void SetCanSeek (bool value)
			{
				can_seek = value;
			}

			public override long Position
			{
				get {
					return pos;
				}
				set {
					pos = (int)value;
				}
			}

			public override long Length {
				get { 
					return buff.Length; 
				}
			}

			public override void SetLength (long value)
			{
				throw new NotSupportedException ();
			}

			public override void Flush ()
			{
				// Nothing.
			}

			protected override void Dispose (bool disposing)
			{
				base.Dispose (disposing);
				disposed = true;
			}

			public bool IsDiposed {
				get { 
					return disposed; 
				}
			}

			// We are going to be returning bytes in blocks of three
			// Just to show a slightly anormal but correct behaviour.
			public override int Read (byte [] buffer, int offset, int count)
			{
				if (disposed)
					throw new ObjectDisposedException ("ResourcesStream");

				// Check if we are done.
				if (pos == buff.Length)
					return 0;

				if (buff.Length - pos < 3)
					count = buff.Length - pos;
				else
					count = 3;

				Buffer.BlockCopy (buff, pos, buffer, offset, count);
				pos += count;
				return count;
			}

			public override void Write (byte [] buffer, int offset, int count)
			{
				throw new NotSupportedException ();
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				throw new NotSupportedException ();
			}
		}
#endif

#if NET_2_0
		[Test]
		public void Bug81759 ()
		{
			MemoryStream ms = new MemoryStream ();
			using (ResourceReader xr = new ResourceReader (
				"Test/resources/bug81759.resources")) {
				ResourceWriter rw = new ResourceWriter (ms);
				foreach (DictionaryEntry de in xr)
					rw.AddResource ((string) de.Key, de.Value);
				rw.Close ();
			}
			ResourceReader rr = new ResourceReader (new MemoryStream (ms.ToArray ()));
			foreach (DictionaryEntry de in rr) {
				Assert.AreEqual ("imageList.ImageSize", de.Key as string, "#1");
				Assert.AreEqual ("Size", de.Value.GetType ().Name, "#2");
			}
		}
#endif

		[Test]
		public void Close ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", "Miguel");
			Assert.IsTrue (ms.CanWrite, "#A1");
			Assert.IsTrue (ms.GetBuffer ().Length == 0, "#A2");
			writer.Close ();
			Assert.IsFalse (ms.CanWrite, "#B1");
			Assert.IsFalse (ms.GetBuffer ().Length == 0, "#B2");
			writer.Close ();
		}

		[Test] // bug #339074
		public void Close_NoResources ()
		{
			string tempFile = Path.Combine (AppDomain.CurrentDomain.BaseDirectory,
				"test.resources");

			ResourceWriter writer = new ResourceWriter (tempFile);
			writer.Close ();

			using (FileStream fs = File.OpenRead (tempFile)) {
				Assert.IsFalse (fs.Length == 0, "#1");

				using (ResourceReader reader = new ResourceReader (fs)) {
					Assert.IsFalse (reader.GetEnumerator ().MoveNext (), "#2");
				}
			}
		}

		[Test]
		public void Generate ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", "Miguel");
			Assert.IsTrue (ms.Length == 0, "#A1");
			Assert.IsTrue (ms.CanWrite, "#A2");
			writer.Generate ();
			Assert.IsFalse (ms.Length == 0, "#B2");
			Assert.IsTrue (ms.CanWrite, "#B2");

			try {
				writer.Generate ();
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			writer.Close ();
		}

		[Test]
		public void Generate_Closed ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", "Miguel");
			writer.Close ();

			try {
				writer.Generate ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // bug #82566
		public void WriteEnum ()
		{
			MemoryStream ms = new MemoryStream ();

			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Targets", AttributeTargets.Assembly);
			writer.Generate ();

			ms.Position = 0;

			bool found = false;

			ResourceReader reader = new ResourceReader (ms);
			foreach (DictionaryEntry de in reader) {
				string name = de.Key as string;
				Assert.IsNotNull (name, "#1");
				Assert.AreEqual ("Targets", name, "#2");
				Assert.IsNotNull (de.Value, "#3");
				Assert.AreEqual (AttributeTargets.Assembly, de.Value, "#4");
				found = true;
			}

			Assert.IsTrue (found, "#5");

			writer.Dispose ();
		}

	}
}
