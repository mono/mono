// StreamWriterTest.cs - NUnit Test Cases for the SystemIO.StreamWriter class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class StreamWriterTest
	{
		class MockStream : Stream
		{
			bool canRead, canSeek, canWrite;
			public event Action OnFlush;
			public event Func<byte[], int, int, int> OnRead;
			public event Action<byte[], int, int> OnWrite;
			long length;

			public MockStream (bool canRead, bool canSeek, bool canWrite)
			{
				this.canRead = canRead;
				this.canSeek = canSeek;
				this.canWrite = canWrite;
			}

			public override bool CanRead {
				get {
					return canRead;
				}
			}

			public override bool CanSeek {
				get {
					return canSeek;
				}
			}

			public override bool CanWrite {
				get {
					return canWrite;
				}
			}

			public override void Flush ()
			{
				if (OnFlush != null)
					OnFlush ();
			}

			public override long Length {
				get {
					return length;
				}
			}

			public override long Position {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}

			public override int Read (byte[] buffer, int offset, int count)
			{
				if (OnRead != null)
					return OnRead (buffer, offset, count);

				return -1;
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				throw new NotImplementedException ();
			}

			public override void SetLength (long value)
			{
				this.length = value;
			}

			public override void Write (byte[] buffer, int offset, int count)
			{
				if (OnWrite != null)
					OnWrite (buffer, offset, count);
			}
		}

	private string _tmpFolder;
	private string _codeFileName;
	private string _thisCodeFileName;

	[SetUp]
	public void SetUp ()
	{
		_tmpFolder = Path.GetTempFileName ();
		if (File.Exists (_tmpFolder))
			File.Delete (_tmpFolder);

		_codeFileName = _tmpFolder + Path.DirectorySeparatorChar + "AFile.txt";
		_thisCodeFileName = _tmpFolder + Path.DirectorySeparatorChar + "StreamWriterTest.temp";

		if (Directory.Exists (_tmpFolder))
			Directory.Delete (_tmpFolder, true);
		Directory.CreateDirectory (_tmpFolder);

		if (!File.Exists (_thisCodeFileName)) 
			File.Create (_thisCodeFileName).Close ();
	}

	[TearDown]
	public void TearDown ()
	{
		if (Directory.Exists (_tmpFolder))
			Directory.Delete (_tmpFolder, true);
	}

	[Test] // .ctor (Stream)
	public void Constructor1 ()
	{
		FileStream f = new FileStream(_codeFileName, 
					      FileMode.Append, 
					      FileAccess.Write);
		StreamWriter r = new StreamWriter (f);
		Assert.IsFalse (r.AutoFlush, "#1");
		Assert.AreSame (f, r.BaseStream, "#2");
		Assert.IsNotNull (r.Encoding, "#3");
		Assert.AreEqual (typeof (UTF8Encoding), r.Encoding.GetType (), "#4");
		r.Close();
		f.Close();
	}

	[Test] // .ctor (Stream)
	public void Constructor1_Stream_NotWritable ()
	{
		FileStream f = new FileStream (_thisCodeFileName, FileMode.Open,
			FileAccess.Read);
		try {
			new StreamWriter (f);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Stream was not writable
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		} finally {
			f.Close ();
		}
	}

	[Test] // .ctor (Stream)
	public void Constructor1_Stream_Null ()
	{
		try {
			new StreamWriter((Stream) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("stream", ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String)
	public void Constructor2 ()
	{
		// TODO - Security/Auth exceptions
		using (StreamWriter r = new StreamWriter (_codeFileName)) {
			Assert.IsFalse (r.AutoFlush, "#1");
			Assert.IsNotNull (r.BaseStream, "#2");
			Assert.AreEqual (typeof (FileStream), r.BaseStream.GetType (), "#3");
			Assert.IsNotNull (r.Encoding, "#4");
			Assert.AreEqual (typeof (UTF8Encoding), r.Encoding.GetType (), "#5");
			r.Close ();
		}
	}

	[Test] // .ctor (String)
	public void Constructor2_Path_DirectoryNotFound ()
	{
		Directory.Delete (_tmpFolder, true);

		try {
			new StreamWriter (_codeFileName);
			Assert.Fail ("#1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsTrue (ex.Message.IndexOf (_tmpFolder) != -1, "#5");
		}
	}

	[Test] // .ctor (String)
	public void Constructor2_Path_Empty ()
	{
		try {
			new StreamWriter (string.Empty);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String)
	public void Constructor2_Path_IllegalChars ()
	{
		try {
			new StreamWriter ("!$what? what? Huh? !$*#" + Path.InvalidPathChars [0]);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String)
	public void Constructor2_Path_Null ()
	{
		try {
			new StreamWriter ((string) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("path", ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (Stream, Encoding)
	public void Constructor3 ()
	{
		FileStream f = new FileStream (_codeFileName,
					      FileMode.Append,
					      FileAccess.Write);
		StreamWriter r = new StreamWriter (f, Encoding.ASCII);
		Assert.IsFalse (r.AutoFlush, "#1");
		Assert.AreSame (f, r.BaseStream, "#2");
		Assert.IsNotNull (r.Encoding, "#3");
		Assert.AreEqual (typeof (ASCIIEncoding), r.Encoding.GetType (), "#4");
		r.Close ();
		f.Close ();
	}

	[Test] // .ctor (Stream, Encoding)
	public void Constructor3_Encoding_Null ()
	{
		MemoryStream m = new MemoryStream ();
		try {
			new StreamWriter (m, (Encoding) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("encoding", ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (Stream, Encoding)
	public void Constructor3_Stream_NotWritable ()
	{
		FileStream f = new FileStream (_thisCodeFileName, FileMode.Open,
			FileAccess.Read);
		try {
			new StreamWriter (f, Encoding.UTF8);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Stream was not writable
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		} finally {
			f.Close ();
		}
	}

	[Test] // .ctor (Stream, Encoding)
	public void Constructor3_Stream_Null ()
	{
		try {
			new StreamWriter ((Stream) null, Encoding.UTF8);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("stream", ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor4 ()
	{
		using (StreamWriter r = new StreamWriter (_codeFileName, false)) {
			Assert.IsFalse (r.AutoFlush, "#A1");
			Assert.IsNotNull (r.BaseStream, "#A2");
			Assert.AreEqual (typeof (FileStream), r.BaseStream.GetType (), "#A3");
			Assert.IsNotNull (r.Encoding, "#A4");
			Assert.AreEqual (typeof (UTF8Encoding), r.Encoding.GetType (), "#A5");
			r.Close();
		}

		using (StreamWriter r = new StreamWriter(_codeFileName, true)) {
			Assert.IsFalse (r.AutoFlush, "#B1");
			Assert.IsNotNull (r.BaseStream, "#B2");
			Assert.AreEqual (typeof (FileStream), r.BaseStream.GetType (), "#B3");
			Assert.IsNotNull (r.Encoding, "#B4");
			Assert.AreEqual (typeof (UTF8Encoding), r.Encoding.GetType (), "#B5");
			r.Close();
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor4_Path_DirectoryNotFound ()
	{
		Directory.Delete (_tmpFolder, true);

		try {
			new StreamWriter (_codeFileName, false);
			Assert.Fail ("#A1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsTrue (ex.Message.IndexOf (_tmpFolder) != -1, "#A5");
		}

		try {
			new StreamWriter (_codeFileName, true);
			Assert.Fail ("#B1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsTrue (ex.Message.IndexOf (_tmpFolder) != -1, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor4_Path_Empty ()
	{
		try {
			new StreamWriter (string.Empty, false);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			new StreamWriter (string.Empty, true);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor4_Path_InvalidChars ()
	{
		try {
			new StreamWriter ("!$what? what? Huh? !$*#" + Path.InvalidPathChars [0], false);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			new StreamWriter ("!$what? what? Huh? !$*#" + Path.InvalidPathChars [0], true);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean)
	public void Constructor4_Path_Null ()
	{
		try {
			new StreamWriter ((string) null, false);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("path", ex.ParamName, "#A5");
		}

		try {
			new StreamWriter ((string) null, true);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("path", ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (Stream, Encoding, Int32)
	public void Constructor5 ()
	{
		MemoryStream m;
		StreamWriter r;

		m = new MemoryStream ();
		r = new StreamWriter (m, Encoding.ASCII, 10);
		Assert.IsFalse (r.AutoFlush, "#A1");
		Assert.AreSame (m, r.BaseStream, "#A2");
		Assert.IsNotNull (r.Encoding, "#A3");
		Assert.AreEqual (typeof (ASCIIEncoding), r.Encoding.GetType (), "#A4");
		r.Close ();
		m.Close ();

		m = new MemoryStream ();
		r = new StreamWriter (m, Encoding.UTF8, 1);
		Assert.IsFalse (r.AutoFlush, "#B1");
		Assert.AreSame (m, r.BaseStream, "#B2");
		Assert.IsNotNull (r.Encoding, "#B3");
		Assert.AreEqual (typeof (UTF8Encoding), r.Encoding.GetType (), "#B4");
		r.Close ();
		m.Close ();
	}

	[Test] // .ctor (Stream, Encoding, Int32)
	public void Constructor5_BufferSize_NotPositive ()
	{
		MemoryStream m = new MemoryStream ();

		try {
			new StreamWriter (m, Encoding.UTF8, 0);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Positive number required
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("bufferSize", ex.ParamName, "#A5");
		}

		try {
			new StreamWriter (m, Encoding.UTF8, -1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Positive number required
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("bufferSize", ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (Stream, Encoding, Int32)
	public void Constructor5_Encoding_Null ()
	{
		MemoryStream m = new MemoryStream ();
		try {
			new StreamWriter (m, (Encoding) null, 10);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("encoding", ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (Stream, Encoding, Int32)
	public void Constructor5_Stream_NotWritable ()
	{
		FileStream f = new FileStream (_thisCodeFileName, FileMode.Open,
			FileAccess.Read);
		try {
			new StreamWriter (f, Encoding.UTF8, 10);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Stream was not writable
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		} finally {
			f.Close ();
		}
	}

	[Test] // .ctor (Stream, Encoding, Int32)
	public void Constructor5_Stream_Null ()
	{
		try {
			new StreamWriter ((Stream) null, Encoding.UTF8, 10);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("stream", ex.ParamName, "#5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding)
	public void Constructor6 ()
	{
		using (StreamWriter r = new StreamWriter (_codeFileName, false, Encoding.ASCII)) {
			Assert.IsFalse (r.AutoFlush, "#A1");
			Assert.IsNotNull (r.BaseStream, "#A2");
			Assert.AreEqual (typeof (FileStream), r.BaseStream.GetType (), "#A3");
			Assert.IsNotNull (r.Encoding, "#A4");
			Assert.AreEqual (typeof (ASCIIEncoding), r.Encoding.GetType (), "#A5");
			r.Close ();
		}

		using (StreamWriter r = new StreamWriter (_codeFileName, true, Encoding.UTF8)) {
			Assert.IsFalse (r.AutoFlush, "#B1");
			Assert.IsNotNull (r.BaseStream, "#B2");
			Assert.AreEqual (typeof (FileStream), r.BaseStream.GetType (), "#B3");
			Assert.IsNotNull (r.Encoding, "#B4");
			Assert.AreEqual (typeof (UTF8Encoding), r.Encoding.GetType (), "#B5");
			r.Close ();
		}
	}

	[Test] // .ctor (String, Boolean, Encoding)
	public void Constructor6_Encoding_Null ()
	{
		try {
			new StreamWriter (_codeFileName, false, (Encoding) null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("encoding", ex.ParamName, "#A5");
		}

		try {
			new StreamWriter (_codeFileName, true, (Encoding) null);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("encoding", ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding)
	public void Constructor6_Path_DirectoryNotFound ()
	{
		Directory.Delete (_tmpFolder, true);

		try {
			new StreamWriter (_codeFileName, false, Encoding.UTF8);
			Assert.Fail ("#A1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsTrue (ex.Message.IndexOf (_tmpFolder) != -1, "#A5");
		}

		try {
			new StreamWriter (_codeFileName, true, Encoding.UTF8);
			Assert.Fail ("#B1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsTrue (ex.Message.IndexOf (_tmpFolder) != -1, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding)
	public void Constructor6_Path_Empty ()
	{
		try {
			new StreamWriter (string.Empty, false, Encoding.UTF8);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			new StreamWriter (string.Empty, true, Encoding.UTF8);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding)
	public void Constructor6_Path_InvalidChars ()
	{
		try {
			new StreamWriter ("!$what? what? Huh? !$*#" +
				Path.InvalidPathChars [0], false,
				Encoding.UTF8);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			new StreamWriter ("!$what? what? Huh? !$*#" +
				Path.InvalidPathChars [0], true,
				Encoding.UTF8);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding)
	public void Constructor6_Path_Null ()
	{
		try {
			new StreamWriter ((string) null, false, Encoding.UTF8);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("path", ex.ParamName, "#A5");
		}

		try {
			new StreamWriter ((string) null, true, Encoding.UTF8);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("path", ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding, Int32)
	public void Constructor7 ()
	{
		using (StreamWriter r = new StreamWriter (_codeFileName, false, Encoding.ASCII, 10)) {
			Assert.IsFalse (r.AutoFlush, "#A1");
			Assert.IsNotNull (r.BaseStream, "#A2");
			Assert.AreEqual (typeof (FileStream), r.BaseStream.GetType (), "#A3");
			Assert.IsNotNull (r.Encoding, "#A4");
			Assert.AreEqual (typeof (ASCIIEncoding), r.Encoding.GetType (), "#A5");
			r.Close ();
		}

		using (StreamWriter r = new StreamWriter (_codeFileName, true, Encoding.UTF8, 1)) {
			Assert.IsFalse (r.AutoFlush, "#B1");
			Assert.IsNotNull (r.BaseStream, "#B2");
			Assert.AreEqual (typeof (FileStream), r.BaseStream.GetType (), "#B3");
			Assert.IsNotNull (r.Encoding, "#B4");
			Assert.AreEqual (typeof (UTF8Encoding), r.Encoding.GetType (), "#B5");
			r.Close ();
		}
	}

	[Test] // .ctor (String, Boolean, Encoding, Int32)
	public void Constructor7_BufferSize_NotPositive ()
	{
		try {
			new StreamWriter (_codeFileName, false, Encoding.UTF8, 0);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Positive number required
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("bufferSize", ex.ParamName, "#A5");
		}

		try {
			new StreamWriter (_codeFileName, false, Encoding.UTF8, -1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Positive number required
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("bufferSize", ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding, Int32)
	public void Constructor7_Encoding_Null ()
	{
		try {
			new StreamWriter (_codeFileName, false, (Encoding) null, 10);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("encoding", ex.ParamName, "#A5");
		}

		try {
			new StreamWriter (_codeFileName, true, (Encoding) null, 10);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("encoding", ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding, Int32)
	public void Constructor7_Path_DirectoryNotFound ()
	{
		Directory.Delete (_tmpFolder, true);

		try {
			new StreamWriter (_codeFileName, false, Encoding.UTF8, 10);
			Assert.Fail ("#A1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsTrue (ex.Message.IndexOf (_tmpFolder) != -1, "#A5");
		}

		try {
			new StreamWriter (_codeFileName, true, Encoding.UTF8, 10);
			Assert.Fail ("#B1");
		} catch (DirectoryNotFoundException ex) {
			// Could not find a part of the path '...'
			Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsTrue (ex.Message.IndexOf (_tmpFolder) != -1, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding, Int32)
	public void Constructor7_Path_Empty ()
	{
		try {
			new StreamWriter (string.Empty, false, Encoding.UTF8, 10);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			new StreamWriter (string.Empty, true, Encoding.UTF8, 10);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Empty path name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding, Int32)
	public void Constructor7_Path_InvalidChars ()
	{
		try {
			new StreamWriter ("!$what? what? Huh? !$*#" +
				Path.InvalidPathChars [0], false,
				Encoding.UTF8, 10);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			new StreamWriter ("!$what? what? Huh? !$*#" +
				Path.InvalidPathChars [0], true,
				Encoding.UTF8, 10);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Illegal characters in path
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}
	}

	[Test] // .ctor (String, Boolean, Encoding, Int32)
	public void Constructor7_Path_Null ()
	{
		try {
			new StreamWriter ((string) null, false, Encoding.UTF8, 10);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("path", ex.ParamName, "#A5");
		}

		try {
			new StreamWriter ((string) null, true, Encoding.UTF8, 10);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("path", ex.ParamName, "#B5");
		}
	}

	[Test]
	public void AutoFlush ()
	{
		MemoryStream m;
		StreamWriter w;

		m = new MemoryStream ();
		w = new StreamWriter (m);
		w.Write (1);
		w.Write (2);
		w.Write (3);
		w.Write (4);
		Assert.AreEqual (0, m.Length, "#A1");
		w.AutoFlush = true;
		Assert.IsTrue (w.AutoFlush, "#A2");
		Assert.AreEqual (4, m.Length, "#A3");
		w.Flush ();
		Assert.AreEqual (4, m.Length, "#A4");

		m = new MemoryStream ();
		w = new StreamWriter(m);
		w.AutoFlush = true;
		Assert.IsTrue (w.AutoFlush, "#B1");
		w.Write (1);
		w.Write (2);
		w.Write (3);
		w.Write (4);
		Assert.AreEqual (4, m.Length, "#B2");
		w.Flush ();
		Assert.AreEqual (4, m.Length, "#B3");
		w.AutoFlush = false;
		Assert.IsFalse (w.AutoFlush, "#B4");
		w.Write (4);
		Assert.AreEqual (4, m.Length, "#B5");
		w.Flush ();
		Assert.AreEqual (5, m.Length, "#B6");
	}

	[Test]
	public void AutoFlush_Disposed ()
	{
		StreamWriter w;
		
		w = new StreamWriter (new MemoryStream ());
		w.Close ();
		w.AutoFlush = false;
		Assert.IsFalse (w.AutoFlush, "#A1");
		try {
			w.AutoFlush = true;
			Assert.Fail ("#A2");
		} catch (ObjectDisposedException) {
		}
		Assert.IsTrue (w.AutoFlush, "#A3");

		w = new StreamWriter (new MemoryStream ());
		w.AutoFlush = true;
		w.Close ();
		Assert.IsTrue (w.AutoFlush, "#B1");
		try {
			w.AutoFlush = true;
			Assert.Fail ("#B2");
		} catch (ObjectDisposedException) {
		}
		Assert.IsTrue (w.AutoFlush, "#B3");
		w.AutoFlush = false;
		Assert.IsFalse (w.AutoFlush, "#B4");
	}

	[Test]
	public void Close ()
	{
		Encoding encoding = Encoding.ASCII;
		MemoryStream m = new MemoryStream ();
		StreamWriter w = new StreamWriter (m, encoding);
		w.Write (2);
		Assert.AreEqual (0, m.Length, "#1");
		w.Close ();
		Assert.IsFalse (m.CanWrite, "#2");
		Assert.AreEqual (50, m.GetBuffer () [0], "#3");
		Assert.IsNull (w.BaseStream, "#4");
		Assert.IsNull (w.Encoding, "#5");
	}

	[Test]
	public void Flush ()
	{
		MemoryStream m = new MemoryStream();
		StreamWriter w = new StreamWriter(m);
		w.Write(1);
		w.Write(2);
		w.Write(3);
		w.Write(4);
		Assert.AreEqual (0L, m.Length, "#1");
		w.Flush();
		Assert.AreEqual (4L, m.Length, "#2");
	}

	[Test]
	public void Flush_Disposed ()
	{
		StreamWriter w = new StreamWriter(new MemoryStream ());
		w.Close();
		try {
			w.Flush ();
			Assert.Fail ("#1");
		} catch (ObjectDisposedException) {
		}
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void WriteChar_Disposed () 
	{
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Close ();
		w.Write ('A');
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void WriteCharArray_Disposed () 
	{
		char[] c = new char [2] { 'a', 'b' };
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Close ();
		w.Write (c, 0, 2);
	}

	[Test]
	public void WriteCharArray_Null () 
	{
		char[] c = null;
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Write (c);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void WriteCharArray_IndexOverflow () 
	{
		char[] c = new char [2] { 'a', 'b' };
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Write (c, Int32.MaxValue, 2);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void WriteCharArray_CountOverflow () 
	{
		char[] c = new char [2] { 'a', 'b' };
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Write (c, 1, Int32.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void WriteString_Disposed () 
	{
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Close ();
		w.Write ("mono");
	}

	[Test]
	public void WriteString_Null () 
	{
		string s = null;
		StreamWriter w = new StreamWriter (new MemoryStream ());
		w.Write (s);
	}

	[Test]
	public void NoPreambleOnAppend ()
	{
		MemoryStream ms = new MemoryStream ();
		StreamWriter w = new StreamWriter (ms, Encoding.UTF8);
		w.Write ("a");
		w.Flush ();
		Assert.AreEqual (4, ms.Position, "#1");

		// Append 1 byte, should skip the preamble now.
		w.Write ("a");
		w.Flush ();
		w = new StreamWriter (ms, Encoding.UTF8);
		Assert.AreEqual (5, ms.Position, "#2");
	}

	[Test]
	[Category ("MultiThreaded")]
	public void FlushAsync ()
	{
		ManualResetEvent mre = new ManualResetEvent (false);
		var m = new MockStream(true, false, true);
		var w = new StreamWriter (m);
		w.Write(1);
		Assert.AreEqual (0L, m.Length, "#1");
		var t = w.WriteLineAsync ();
		Assert.IsTrue (t.Wait (1000), "#2");
		Assert.IsTrue (w.FlushAsync ().Wait (1000), "#3");
	}

	[Test]
	public void KeepOpenWithDispose ()
	{
		var ms = new MemoryStream ();
		using (StreamWriter writer = new StreamWriter (ms, new UTF8Encoding (false), 4096, true)) {
			writer.Write ('X');
		}

		Assert.AreEqual (1, ms.Length);
	}

	[Test]
	public void WriteAsync ()
	{
		var m = new MockStream(true, false, true);
		var w = new StreamWriter (m);

		var t = w.WriteAsync ("v");
		Assert.IsTrue (t.Wait (1000), "#1");

		t = w.WriteAsync ((string) null);
		Assert.IsTrue (t.Wait (1000), "#2");

		t = w.WriteLineAsync ("line");
		Assert.IsTrue (t.Wait (1000), "#3");

		t = w.WriteLineAsync ((string) null);
		Assert.IsTrue (t.Wait (1000), "#4");

		t = w.WriteLineAsync ('c');
		Assert.IsTrue (t.Wait (1000), "#5");
	}


	// TODO - Write - test errors, functionality tested in TestFlush.
}
}
