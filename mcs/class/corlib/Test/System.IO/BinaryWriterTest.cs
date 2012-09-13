//
// System.IO.StringWriter
//
// Authors: 
//	Ville Palo (vi64pa@kolumbus.fi)
//
// (C) 2003 Ville Palo
//

using NUnit.Framework;
using System.IO;
using System.Text;
using System;

namespace MonoTests.System.IO {

[TestFixture]
public class BinaryWriterTest {
	sealed class MyBinaryWriter : BinaryWriter
	{
		public MyBinaryWriter (Stream stream)
			: base (stream)
		{ }

		public void WriteLeb128 (int value)
		{
			base.Write7BitEncodedInt (value);
		}
	}
	
	string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
	
	[SetUp]
        protected void SetUp() {
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
		Directory.CreateDirectory (TempFolder);
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
	}

	[Test]
	public void Ctor ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);
		Assert.AreEqual (true, writer.BaseStream.CanRead, "test#01");
		Assert.AreEqual (true, writer.BaseStream.CanSeek, "test#02");
		Assert.AreEqual (true, writer.BaseStream.CanWrite, "test#03");
		
		writer = new BinaryWriter (stream, new ASCIIEncoding ());
		Assert.AreEqual (true, writer.BaseStream.CanRead, "test#04");
		Assert.AreEqual (true, writer.BaseStream.CanSeek, "test#05");
		Assert.AreEqual (true, writer.BaseStream.CanWrite, "test#06");			
		
	}

	/// <summary>
	/// Throws an exception if stream is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionStream () 
	{
		BinaryWriter reader = new BinaryWriter (null);
	}

	/// <summary>
	/// Throws an exception if encoding is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionStreamEncoding () 
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter reader = new BinaryWriter (stream, null);
	}
	
	/// <summary>
	/// Throws an exception if stream is closed
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CtorExceptionStreamClosed () 
	{
		MemoryStream stream = new MemoryStream ();
		stream.Close ();		
		BinaryWriter writer = new BinaryWriter (stream);
	}
	
	/// <summary>
	/// Throws an exception if stream does not support writing
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CtorArgumentExceptionStreamCannotWrite ()
	{
		string path = TempFolder + "/BinaryWriterTest.1";
		DeleteFile (path);
		FileStream stream = null;
		BinaryWriter reader = null;
			
		try {
			stream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Read);
			reader = new BinaryWriter (stream);
		} finally {
			if (reader != null)
				reader.Close ();
			if (stream != null)
				stream.Close ();
			DeleteFile (path);
		}
	}

	[Test]
	public void Encoding ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);
		writer.Write ("t*st");
		
		byte [] bytes = stream.GetBuffer ();
		Assert.AreEqual (4, bytes [0], "test#01");
		Assert.AreEqual (116, bytes [1], "test#02");
		Assert.AreEqual (42, bytes [2], "test#03");
		Assert.AreEqual (115, bytes [3], "test#04");
		Assert.AreEqual (116, bytes [4], "test#05");
		Assert.AreEqual (0, bytes [5], "test#06");
		
		stream = new MemoryStream ();
		writer = new BinaryWriter (stream, new UnicodeEncoding ());
		writer.Write ("t*st");
		
		bytes = stream.GetBuffer ();
		Assert.AreEqual (8, bytes [0], "test#07");
		Assert.AreEqual (116, bytes [1], "test#08");
		Assert.AreEqual (0, bytes [2], "test#09");
		Assert.AreEqual (42, bytes [3], "test#10");
		Assert.AreEqual (0, bytes [4], "test#11");
		Assert.AreEqual (115, bytes [5], "test#12");
		Assert.AreEqual (0, bytes [6], "test#13");
		Assert.AreEqual (116, bytes [7], "test#14");
		Assert.AreEqual (0, bytes [8], "test#15");

		stream = new MemoryStream ();
		writer = new BinaryWriter (stream, new UTF7Encoding ());
		writer.Write ("t*st");
		
		bytes = stream.GetBuffer ();
		Assert.AreEqual (8, bytes [0], "test#16");
		Assert.AreEqual (116, bytes [1], "test#17");
		Assert.AreEqual (43, bytes [2], "test#18");
		Assert.AreEqual (65, bytes [3], "test#19");
		Assert.AreEqual (67, bytes [4], "test#21");
		Assert.AreEqual (111, bytes [5], "test#22");
		Assert.AreEqual (45, bytes [6], "test#23");
		Assert.AreEqual (115, bytes [7], "test#24");
		Assert.AreEqual (116, bytes [8], "test#25");
		Assert.AreEqual (0, bytes [9], "test#26");
		Assert.AreEqual (0, bytes [10], "test#27");		

		stream = new MemoryStream ();
		writer = new BinaryWriter (stream, new ASCIIEncoding ());
		writer.Write ("t*st");
		bytes = stream.GetBuffer ();
		Assert.AreEqual (4, bytes [0], "test#28");
		Assert.AreEqual (116, bytes [1], "test#29");
		Assert.AreEqual (42, bytes [2], "test#30");
		Assert.AreEqual (115, bytes [3], "test#31");
		Assert.AreEqual (116, bytes [4], "test#32");
		Assert.AreEqual (0, bytes [5], "test#33");
	}
	
	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]
	public void Close1 ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream, new ASCIIEncoding ());
		writer.Close ();
		writer.Write ("Test");	
	}

	[Test]
	public void Close2 ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream, new ASCIIEncoding ());
		writer.Close ();
		writer.Flush ();
		stream.Flush ();
	}

	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]
	public void Close3 ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream, new ASCIIEncoding ());
		writer.Close ();
		writer.Seek (1, SeekOrigin.Begin);
	}
	
	[Test]
	public void Close4 ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream, new ASCIIEncoding ());
		writer.Close ();
		Assert.AreEqual (false, writer.BaseStream.CanRead, "test#01");
		Assert.AreEqual (false, writer.BaseStream.CanWrite, "test#01");
		Assert.AreEqual (false, writer.BaseStream.CanSeek, "test#01");		
	}
	
	[Test]
	public void Seek ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream, new ASCIIEncoding ());
		
		writer.Write ("Test");
		writer.Seek (2, SeekOrigin.Begin);
		writer.Write ("-");
		writer.Seek (400, SeekOrigin.Begin);
		writer.Write ("-");		
		writer.Seek (-394, SeekOrigin.End);
		writer.Write ("-");
		writer.Seek (-2, SeekOrigin.Current);
		writer.Write ("-");
		
		byte [] bytes = stream.GetBuffer ();
		Assert.AreEqual (512, bytes.Length, "test#01");
		Assert.AreEqual (4, bytes [0], "test#02");
		Assert.AreEqual (84, bytes [1], "test#03");
		Assert.AreEqual (1, bytes [2], "test#04");
		Assert.AreEqual (45, bytes [3], "test#05");
		Assert.AreEqual (116, bytes [4], "test#06");
		Assert.AreEqual (0, bytes [5], "test#07");
		Assert.AreEqual (0, bytes [6], "test#08");
		Assert.AreEqual (0, bytes [7], "test#09");
		Assert.AreEqual (1, bytes [8], "test#10");
		Assert.AreEqual (45, bytes [9], "test#11");
		Assert.AreEqual (0, bytes [10], "test#12");
		Assert.AreEqual (0, bytes [11], "test#13");
		Assert.AreEqual (0, bytes [12], "test#14");
		Assert.AreEqual (1, bytes [400], "test#15");				
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void SeekException ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);
		
		writer.Write ("Test");
		writer.Seek (-12, SeekOrigin.Begin);		
	}
	
	[Test]
	public void WriteCharArray ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);
		
		writer.Write (new char [] {'m', 'o', 'n', 'o', ':', ':'});
		writer.Write (new char [] {':', ':', 'O', 'N', 'O', 'M'});

		byte [] bytes = stream.GetBuffer ();
		
		Assert.AreEqual (256, bytes.Length, "test#01");
		Assert.AreEqual (109, bytes [0], "test#02");
		Assert.AreEqual (111, bytes [1], "test#03");
		Assert.AreEqual (110, bytes [2], "test#04");
		Assert.AreEqual (111, bytes [3], "test#05");
		Assert.AreEqual (58, bytes [4], "test#06");
		Assert.AreEqual (58, bytes [5], "test#07");
		Assert.AreEqual (58, bytes [6], "test#08");
		Assert.AreEqual (58, bytes [7], "test#09");
		Assert.AreEqual (79, bytes [8], "test#10");
		Assert.AreEqual (78, bytes [9], "test#11");
		Assert.AreEqual (79, bytes [10], "test#12");
		Assert.AreEqual (77, bytes [11], "test#13");
		Assert.AreEqual (0, bytes [12], "test#14");
		Assert.AreEqual (0, bytes [13], "test#15");		
	}
	
	[Test]
	public void WriteByteArray ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);		
		
		writer.Write (new byte [] {1, 2, 3, 4, 5, 6});
		writer.Write (new byte [] {6, 5, 4, 3, 2, 1});

		byte [] bytes = stream.GetBuffer ();		
		Assert.AreEqual (256, bytes.Length, "test#01");
		Assert.AreEqual (1, bytes [0], "test#02");
		Assert.AreEqual (2, bytes [1], "test#03");
		Assert.AreEqual (3, bytes [2], "test#04");
		Assert.AreEqual (4, bytes [3], "test#05");
		Assert.AreEqual (5, bytes [4], "test#06");
		Assert.AreEqual (6, bytes [5], "test#07");
		Assert.AreEqual (6, bytes [6], "test#08");
		Assert.AreEqual (5, bytes [7], "test#09");
		Assert.AreEqual (4, bytes [8], "test#10");
		Assert.AreEqual (3, bytes [9], "test#11");
		Assert.AreEqual (2, bytes [10], "test#12");
		Assert.AreEqual (1, bytes [11], "test#13");
		Assert.AreEqual (0, bytes [12], "test#14");
		Assert.AreEqual (0, bytes [13], "test#15");		
		
	}

	[Test]
	public void WriteInt ()
	{
		short s = 64;
		int i = 64646464;
		long l = 9999999999999;
		
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);		
		writer.Write (s);
		byte [] bytes;
		bytes = stream.GetBuffer ();
		Assert.AreEqual (256, bytes.Length, "test#01");
		Assert.AreEqual (64, bytes [0], "test#02");
		Assert.AreEqual (0, bytes [1], "test#03");
		
		writer.Write (i);
		bytes = stream.GetBuffer ();
		Assert.AreEqual (256, bytes.Length, "test#04");
		Assert.AreEqual (64, bytes [0], "test#05");
		Assert.AreEqual (0, bytes [1], "test#06");
		Assert.AreEqual (64, bytes [2], "test#07");
		Assert.AreEqual (109, bytes [3], "test#08");
		Assert.AreEqual (218, bytes [4], "test#09");
		Assert.AreEqual (3, bytes [5], "test#10");
		Assert.AreEqual (0, bytes [6], "test#11");

		writer.Write (l);
		bytes = stream.GetBuffer ();
		Assert.AreEqual (256, bytes.Length, "test#12");
		Assert.AreEqual (255, bytes [6], "test#13");
		Assert.AreEqual (159, bytes [7], "test#14");
		Assert.AreEqual (114, bytes [8], "test#15");
		Assert.AreEqual (78, bytes [9], "test#16");
		Assert.AreEqual (24, bytes [10], "test#17");
		Assert.AreEqual (9, bytes [11], "test#18");
		Assert.AreEqual (0, bytes [12], "test#19");
	}
	
	[Test]
	public void WriteDecimal ()
 	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);
	
		decimal d1 = 19932143214312.32M;
		decimal d2 = -8995034512332157M;
		
		writer.Write (d1);
		writer.Write (d2);
		byte [] bytes = stream.GetBuffer ();
		
		Assert.AreEqual (256, bytes.Length, "test#01");
		Assert.AreEqual (192, bytes [0], "test#02");
		Assert.AreEqual (18, bytes [1], "test#03");
		Assert.AreEqual (151, bytes [2], "test#04");
		Assert.AreEqual (95, bytes [3], "test#05");
		Assert.AreEqual (209, bytes [4], "test#06");
		Assert.AreEqual (20, bytes [5], "test#07");
		Assert.AreEqual (7, bytes [6], "test#08");
		Assert.AreEqual (0, bytes [7], "test#09");
		Assert.AreEqual (0, bytes [8], "test#10");
		Assert.AreEqual (0, bytes [9], "test#11");
		Assert.AreEqual (0, bytes [10], "test#12");
		Assert.AreEqual (0, bytes [11], "test#13");
		Assert.AreEqual (0, bytes [12], "test#14");
		Assert.AreEqual (0, bytes [13], "test#15");
		Assert.AreEqual (2, bytes [14], "test#16");
		Assert.AreEqual (0, bytes [15], "test#17");
		Assert.AreEqual (125, bytes [16], "test#18");
		Assert.AreEqual (149, bytes [17], "test#19");
		Assert.AreEqual (217, bytes [18], "test#20");
		Assert.AreEqual (172, bytes [19], "test#21");
		Assert.AreEqual (239, bytes [20], "test#22");
		Assert.AreEqual (244, bytes [21], "test#23");
		Assert.AreEqual (31, bytes [22], "test#24");
		Assert.AreEqual (0, bytes [23], "test#25");
		Assert.AreEqual (0, bytes [24], "test#26");
		Assert.AreEqual (0, bytes [25], "test#27");
		Assert.AreEqual (0, bytes [26], "test#28");		
		Assert.AreEqual (0, bytes [27], "test#29");
		Assert.AreEqual (0, bytes [28], "test#30");
		Assert.AreEqual (0, bytes [29], "test#31");
		Assert.AreEqual (0, bytes [30], "test#32");
		Assert.AreEqual (128, bytes [31], "test#33");
		Assert.AreEqual (0, bytes [32], "test#34");
		Assert.AreEqual (0, bytes [33], "test#35");
		Assert.AreEqual (0, bytes [34], "test#36");
		Assert.AreEqual (0, bytes [35], "test#37");
		Assert.AreEqual (0, bytes [36], "test#38");
		Assert.AreEqual (0, bytes [37], "test#39");
		Assert.AreEqual (0, bytes [38], "test#40");
		Assert.AreEqual (0, bytes [39], "test#41");
		Assert.AreEqual (0, bytes [40], "test#42");
		Assert.AreEqual (0, bytes [41], "test#43");
		Assert.AreEqual (0, bytes [42], "test#44");
		Assert.AreEqual (0, bytes [43], "test#45");
		Assert.AreEqual (0, bytes [44], "test#46");
		Assert.AreEqual (0, bytes [45], "test#47");
		Assert.AreEqual (0, bytes [46], "test#48");		
	}
	
	[Test]
	public void WriteFloat ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);
		float f1 = 1.543E+10F;
		float f2 = -9.6534E-6f;
		writer.Write (f1);
		writer.Write (f2);
		
		byte [] bytes = stream.GetBuffer ();
		Assert.AreEqual (256, bytes.Length, "test#01");
		Assert.AreEqual (199, bytes [0], "test#02");
		Assert.AreEqual (236, bytes [1], "test#03");
		Assert.AreEqual (101, bytes [2], "test#04");
		Assert.AreEqual (80, bytes [3], "test#05");
		Assert.AreEqual (10, bytes [4], "test#06");
		Assert.AreEqual (245, bytes [5], "test#07");
		Assert.AreEqual (33, bytes [6], "test#08");
		Assert.AreEqual (183, bytes [7], "test#09");
		Assert.AreEqual (0, bytes [8], "test#10");
		Assert.AreEqual (0, bytes [9], "test#11");		
	}

	[Test]
	public void WriteDouble ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);
		double d1 = 1.543E+100;
		double d2 = -9.6534E-129;
		writer.Write (d1);
		writer.Write (d2);
		
		byte [] bytes = stream.GetBuffer ();
		Assert.AreEqual (256, bytes.Length, "test#01");
		Assert.AreEqual (49, bytes [0], "test#02");
		Assert.AreEqual (69, bytes [1], "test#03");
		Assert.AreEqual (15, bytes [2], "test#04");
		Assert.AreEqual (157, bytes [3], "test#05");
		Assert.AreEqual (211, bytes [4], "test#06");
		Assert.AreEqual (55, bytes [5], "test#07");
		Assert.AreEqual (188, bytes [6], "test#08");
		Assert.AreEqual (84, bytes [7], "test#09");
		Assert.AreEqual (76, bytes [8], "test#10");
		Assert.AreEqual (59, bytes [9], "test#11");
		Assert.AreEqual (59, bytes [10], "test#12");
		Assert.AreEqual (60, bytes [11], "test#13");
		Assert.AreEqual (4, bytes [12], "test#14");
		Assert.AreEqual (196, bytes [13], "test#15");
		Assert.AreEqual (90, bytes [14], "test#16");
		Assert.AreEqual (165, bytes [15], "test#17");
		Assert.AreEqual (0, bytes [16], "test#18");
	}
	
	[Test]
	public void WriteByteAndChar ()
	{
		byte b1 = 12;
		byte b2 = 64;
		char c1 = '-';
		char c2 = 'M';
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);
		writer.Write (b1);
		writer.Write (c1);
		writer.Write (b2);
		writer.Write (c2);
		
		byte [] bytes = stream.GetBuffer ();
		Assert.AreEqual (256, bytes.Length, "test#01");
		Assert.AreEqual (12, bytes [0], "test#02");
		Assert.AreEqual (45, bytes [1], "test#03");
		Assert.AreEqual (64, bytes [2], "test#04");
		Assert.AreEqual (77, bytes [3], "test#05");
		Assert.AreEqual (0, bytes [4], "test#06");
	}
	
	[Test]
	public void WriteString ()
	{
		MemoryStream stream = new MemoryStream ();
		BinaryWriter writer = new BinaryWriter (stream);
		string s1 = "abc";
		string s2 = "DeF\n";
		writer.Write (s1);
		writer.Write (s2);

		byte [] bytes = stream.GetBuffer ();
		Assert.AreEqual (256, bytes.Length, "test#01");
		Assert.AreEqual (3, bytes [0], "test#02");
		Assert.AreEqual (97, bytes [1], "test#03");
		Assert.AreEqual (98, bytes [2], "test#04");
		Assert.AreEqual (99, bytes [3], "test#05");
		Assert.AreEqual (4, bytes [4], "test#06");
		Assert.AreEqual (68, bytes [5], "test#07");
		Assert.AreEqual (101, bytes [6], "test#08");
		Assert.AreEqual (70, bytes [7], "test#09");
		Assert.AreEqual (10, bytes [8], "test#10");
		Assert.AreEqual (0, bytes [9], "test#11");		
	}

	[Test]
	public void Write7BitEncodedIntTest ()
	{
		MemoryStream stream = new MemoryStream ();
		var writer = new MyBinaryWriter (stream);
		writer.WriteLeb128 (5);

		Assert.AreEqual (new byte[] { 5 }, stream.ToArray (), "#1");

		stream = new MemoryStream ();
		writer = new MyBinaryWriter (stream);
		writer.WriteLeb128 (int.MaxValue);

		Assert.AreEqual (new byte[] { 255, 255, 255, 255, 7 }, stream.ToArray (), "#2");

		stream = new MemoryStream ();
		writer = new MyBinaryWriter (stream);
		writer.WriteLeb128 (128);

		Assert.AreEqual (new byte[] { 128, 1 }, stream.ToArray (), "#3");

		stream = new MemoryStream ();
		writer = new MyBinaryWriter (stream);
		writer.WriteLeb128 (-1025);

		Assert.AreEqual (new byte[] { 255, 247, 255, 255, 15 }, stream.ToArray (), "#4");

		stream = new MemoryStream ();
		writer = new MyBinaryWriter (stream);
		writer.WriteLeb128 (int.MinValue);

		Assert.AreEqual (new byte[] { 128, 128, 128, 128, 8 }, stream.ToArray (), "#5");

		stream = new MemoryStream ();
		writer = new MyBinaryWriter (stream);
		writer.WriteLeb128 (-1);

		Assert.AreEqual (new byte[] { 255, 255, 255, 255, 15 }, stream.ToArray (), "#6");

		stream = new MemoryStream ();
		writer = new MyBinaryWriter (stream);
		writer.WriteLeb128 (0);

		Assert.AreEqual (new byte[] { 0 }, stream.ToArray (), "#7");
	}

	[Test]
	public void BaseStreamCallsFlush ()
	{
		FlushStream stream = new FlushStream ();
		BinaryWriter writer = new BinaryWriter (stream);
		Stream s = writer.BaseStream;
		Assert.IsTrue (stream.FlushCalled);
	}

	private void DeleteFile (string path)
	{
		if (File.Exists (path))
			File.Delete (path);
	}

	class FlushStream : Stream
	{
		public bool FlushCalled;

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
                        get { return true; }
                }

                public override bool CanWrite {
                        get { return true; }
                }

		public override long Length {
			get { return 0; }
		}

		public override long Position {
			get { return 0; }
			set { }
		}

		public override void Flush ()
		{
			FlushCalled = true;
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override int ReadByte ()
		{
			return -1;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return 0;
		}

		public override void SetLength (long value)
		{
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
		}

		public override void WriteByte (byte value)
		{
		}
	}


}

}

