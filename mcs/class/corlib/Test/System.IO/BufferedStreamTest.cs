//
// System.IO.BufferedStream Unit Tests
//
// Authors: 
//	Ville Palo (vi64pa@kolumbus.fi)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ville Palo
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System.IO;
using System.Text;
using System;

namespace MonoTests.System.IO {

[TestFixture]
public class BufferedStreamTest {
	
	private MemoryStream mem;
	
        [SetUp]
        protected void SetUp ()
        {
		mem = new MemoryStream ();
        }

        [TearDown]
       	protected void TearDown ()
        {
		//Some tests might mess with mem, so let's check it first
		if (mem != null)
			mem.Close ();
        }


	[Test]
	public void Ctor ()
	{
		MemoryStream str = new MemoryStream ();
		str.Write (new byte [] {1, 2, 3, 4, 5, 6}, 0, 6);
		BufferedStream stream = new BufferedStream (str);
		
		Assert.AreEqual (true, stream.CanRead, "test#01");
		Assert.AreEqual (true, stream.CanSeek, "test#02");
		Assert.AreEqual (true, stream.CanWrite, "test#03");
		Assert.AreEqual (6, stream.Length, "test#04");
		Assert.AreEqual (6, stream.Position, "test#05");
		
		string path = Path.GetTempFileName ();
		if (File.Exists (path))
			File.Delete (path);
		
		FileStream file = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
		stream = new BufferedStream (file);		
		Assert.AreEqual (false, stream.CanRead, "test#06");
		Assert.AreEqual (true, stream.CanSeek, "test#07");
		Assert.AreEqual (true, stream.CanWrite, "test#08");
		Assert.AreEqual (0, stream.Length, "test#09");
		Assert.AreEqual (0, stream.Position, "test#10");		
		file.Close ();
		
		if (File.Exists (path))
			File.Delete (path);
		
		file = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
		stream = new BufferedStream (file, 12);		
		Assert.AreEqual (false, stream.CanRead, "test#11");
		Assert.AreEqual (true, stream.CanSeek, "test#12");
		Assert.AreEqual (true, stream.CanWrite, "test#13");
		Assert.AreEqual (0, stream.Length, "test#14");
		Assert.AreEqual (0, stream.Position, "test#15");		

		file.Close ();
		if (File.Exists (path))
			File.Delete (path);
	}

	/// <summary>
	/// Throws an exception if stream is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionStream () 
	{
		BufferedStream stream = new BufferedStream (null);
	}

	/// <summary>
	/// Throws an exception if stream is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionStream1 () 
	{
		BufferedStream stream = new BufferedStream (null, 12);
	}
	
	/// <summary>
	/// Throws an exception if stream is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void CtorOutOfRangeExceptionStream1 () 
	{
		MemoryStream str = new MemoryStream ();
		BufferedStream stream = new BufferedStream (str, -12);
	}


	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]
	public void CtorOutOfRangeException2 () 
	{
		MemoryStream str = new MemoryStream ();
		str.Close ();
		BufferedStream stream = new BufferedStream (str);		
	}
	
	[Test]	
	public void Close1 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		stream.Close ();
	}

	[Test]	
	public void Close2 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		Assert.AreEqual (false, stream.CanRead, "test#01");
		Assert.AreEqual (false, stream.CanSeek, "test#02");
		Assert.AreEqual (false, stream.CanWrite, "test#03");
	}

	[Test]	
	[ExpectedException(typeof(ObjectDisposedException))]	
	public void Close3 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		long l = stream.Position;
	}

	[Test]	
	[ExpectedException(typeof(ObjectDisposedException))]	
	public void Close4 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		long l = stream.Length;
	}
	
	[Test]	
	[ExpectedException(typeof(ObjectDisposedException))]	
	public void Close5 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		stream.WriteByte (1);
	}
	
	[Test]	
	[ExpectedException(typeof(ObjectDisposedException))]	
	public void Close6 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		stream.ReadByte ();
	}

	[Test]	
	[ExpectedException(typeof(NotSupportedException))]	
	public void Close7 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		mem.Close ();
		stream.WriteByte (1);
	}
	
	[Test]
	public void Read ()
	{
		mem.Write (new byte [] {0, 1, 2, 3, 4, 5, 6, 7}, 0, 8);
		BufferedStream stream = new BufferedStream (mem);

		byte [] bytes = new byte [10];
		stream.Read (bytes, 0, 3);
		Assert.AreEqual (0, bytes [0], "test#01");
		Assert.AreEqual (0, bytes [1], "test#02");
		Assert.AreEqual (0, bytes [2], "test#03");

		stream.Seek (0, SeekOrigin.Begin);
		stream.Read (bytes, 0, 3);
		Assert.AreEqual (0, bytes [0], "test#04");
		Assert.AreEqual (1, bytes [1], "test#05");
		Assert.AreEqual (2, bytes [2], "test#06");
		Assert.AreEqual (0, bytes [0], "test#07");		

		stream.Read (bytes, 5, 3);
		Assert.AreEqual (3, bytes [5], "test#08");
		Assert.AreEqual (4, bytes [6], "test#09");
		Assert.AreEqual (5, bytes [7], "test#10");
		Assert.AreEqual (0, bytes [8], "test#11");		

		stream.Read (bytes, 0, 10);
		Assert.AreEqual (3, bytes [5], "test#12");
		Assert.AreEqual (4, bytes [6], "test#13");
		Assert.AreEqual (5, bytes [7], "test#14");
		Assert.AreEqual (0, bytes [9], "test#15");				
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]		
	public void ReadException ()
	{
		mem.Write (new byte [] {0, 1, 2, 3, 4, 5, 6, 7}, 0, 8);
		BufferedStream stream = new BufferedStream (mem);

		byte [] bytes = new byte [10];
		stream.Read (bytes, 0, 30);		
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Read_Null () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Read (null, 0, 0);
	}

	[Test]
	[ExpectedException(typeof(NotSupportedException))]
	public void Read_CantRead () 
	{
		WriteOnlyStream wo = new WriteOnlyStream ();
		BufferedStream stream = new BufferedStream (wo);
		stream.Read (new byte [1], 0, 1);
	}

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void Read_OffsetNegative () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Read (new byte [1], -1, 1); 
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void Read_OffsetOverflow () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Read (new byte [1], Int32.MaxValue, 1); 
	}

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void Read_CountNegative () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Read (new byte [1], 1, -1); 
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void Read_CountOverflow () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Read (new byte [1], 1, Int32.MaxValue); 
	}
	
	[Test]
	public void ReadByte ()
	{
		mem.Write (new byte [] {0, 1, 2, 3, 4, 5, 6, 7}, 0, 8);
		BufferedStream stream = new BufferedStream (mem);

		Assert.AreEqual (-1, stream.ReadByte (), "test#01");
		Assert.AreEqual (-1, stream.ReadByte (), "test#02");
		Assert.AreEqual (-1, stream.ReadByte (), "test#03");

		stream.Seek (0, SeekOrigin.Begin);
		Assert.AreEqual (0, stream.ReadByte (), "test#04");
		Assert.AreEqual (1, stream.ReadByte (), "test#05");
		Assert.AreEqual (2, stream.ReadByte (), "test#06");
		Assert.AreEqual (3, stream.ReadByte (), "test#07");		
	}
	
	[Test]
	public void Write ()
	{
		BufferedStream stream = new BufferedStream (mem);
		
		stream.Write (new byte [] {0, 1, 2, 3, 4}, 0, 4);
		Assert.AreEqual (4, stream.Length, "test#01");
		byte [] bytes = mem.GetBuffer ();
		Assert.AreEqual (0, bytes [0], "test#02");
		Assert.AreEqual (1, bytes [1], "test#03");
		Assert.AreEqual (2, bytes [2], "test#04");
		Assert.AreEqual (3, bytes [3], "test#05");

		bytes = new byte [] {1, 4, 3};
		stream.Write (bytes, 0, 3);
		stream.Flush ();
		bytes = mem.GetBuffer ();		
		Assert.AreEqual (0, bytes [0], "test#06");
		Assert.AreEqual (1, bytes [1], "test#07");
		Assert.AreEqual (2, bytes [2], "test#08");
		Assert.AreEqual (3, bytes [3], "test#09");
		Assert.AreEqual (1, bytes [4], "test#10");
		Assert.AreEqual (4, bytes [5], "test#11");
		Assert.AreEqual (3, bytes [6], "test#10");
		Assert.AreEqual (0, bytes [7], "test#11");
		Assert.AreEqual (7, stream.Length, "test#12");
	}
		
	[Test]
	[ExpectedException(typeof (ArgumentException))]
	public void WriteException ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [] {0,1,2,3}, 0, 10);
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Write_Null () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (null, 0, 0);
	}

	[Test]
	[ExpectedException(typeof(NotSupportedException))]
	public void Write_CantWrite () 
	{
		ReadOnlyStream ro = new ReadOnlyStream ();
		BufferedStream stream = new BufferedStream (ro);
		stream.Write (new byte [1], 0, 1);
	}

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void Write_OffsetNegative () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [1], -1, 1); 
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void Write_OffsetOverflow () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [1], Int32.MaxValue, 1); 
	}

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void Write_CountNegative () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [1], 1, -1); 
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void Write_CountOverflow () 
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [1], 1, Int32.MaxValue); 
	}
	
	[Test]
	public void WriteByte ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.WriteByte (1);
		stream.WriteByte (2);
		stream.WriteByte (3);
		stream.Flush ();
		Assert.AreEqual (256, mem.GetBuffer ().Length, "test#01");
		Assert.AreEqual (3, stream.Length, "test#02");
		Assert.AreEqual (1, mem.GetBuffer () [0], "test#03");
		Assert.AreEqual (2, mem.GetBuffer () [1], "test#04");
		Assert.AreEqual (3, mem.GetBuffer () [2], "test#05");		
	}
	
	[Test]
	public void Flush ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.WriteByte (1);
		stream.WriteByte (2);
		
		byte [] bytes = mem.GetBuffer ();
		Assert.AreEqual (0, bytes.Length, "test#01");
		stream.Flush ();
		
		bytes = mem.GetBuffer ();
		Assert.AreEqual (256, bytes.Length, "test#02");
		Assert.AreEqual (1, bytes [0], "test#03");
		Assert.AreEqual (2, bytes [1], "test#04");
		mem.Close ();
		mem = new MemoryStream ();
		bytes = new byte [] {0, 1, 2, 3, 4, 5};
		stream = new BufferedStream (mem);
		stream.Write (bytes, 0, 2);
		Assert.AreEqual (2, stream.Length, "test#05");
		bytes = mem.GetBuffer ();
		Assert.AreEqual (256, bytes.Length, "test#06");

		Assert.AreEqual (0, bytes [0], "test#07");
		Assert.AreEqual (1, bytes [1], "test#08");
		
		stream.Write (bytes, 0, 2);
		
		bytes = mem.GetBuffer ();
		Assert.AreEqual (0, bytes [0], "test#09");
		Assert.AreEqual (1, bytes [1], "test#10");
		Assert.AreEqual (0, bytes [2], "test#11");
		Assert.AreEqual (0, bytes [3], "test#12");
		stream.Flush ();
		bytes = mem.GetBuffer ();
		Assert.AreEqual (0, bytes [2], "test#13");
		Assert.AreEqual (1, bytes [3], "test#14");
	}
	
	[Test]
	public void Seek ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [] {0, 1, 2, 3, 4, 5}, 0, 6);
		
		Assert.AreEqual (6, stream.Position, "test#01");
		
		stream.Seek (-5, SeekOrigin.End);		
		Assert.AreEqual (1, stream.Position, "test#02");
		
		stream.Seek (3, SeekOrigin.Current);
		Assert.AreEqual (4, stream.Position, "test#03");
		
		stream.Seek (300, SeekOrigin.Current);		
		Assert.AreEqual (304, stream.Position, "test#04");		
	}
	
	[Test]
	[ExpectedException(typeof (IOException))]
	public void SeekException ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Seek (-1, SeekOrigin.Begin);
	}
	
	[Test]
	public void SetLength ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [] {0,1,2,3,4,5}, 0, 6);
		
		Assert.AreEqual (6, stream.Length, "test#01");
		stream.SetLength (60);
		Assert.AreEqual (60, stream.Length, "test#02");
		
		stream.SetLength (2);
		Assert.AreEqual (2, stream.Length, "test#03");	
	}
	
	 [Test]
	 [ExpectedException(typeof(ArgumentOutOfRangeException))]
	 public void SetLengthException ()
	 {
		BufferedStream stream = new BufferedStream (mem);
		stream.SetLength (-1);	 	
	 }

	 [Test]
	 [ExpectedException(typeof(NotSupportedException))]
	 public void SetLengthException2 ()
	 {
		BufferedStream stream = new BufferedStream (mem);
	 	mem.Close ();
		stream.SetLength (1);	 	
	 }

	 [Test]
	 public void SetLengthException3 ()
	 {
		BufferedStream stream = new BufferedStream (mem);
	 	mem = null;
		// Strangely, this does not throw an exception on .NET 1.1
		stream.SetLength (1);	 	
	 }
	 
	 [Test]
	 public void Position ()
	 {
		mem = new MemoryStream ();
		BufferedStream stream = new BufferedStream (mem,5);
		stream.Write (new byte [] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15}, 0, 16);
	 	
		stream.Position = 0;
		Assert.AreEqual (0, stream.Position, "test#01");		
		Assert.AreEqual (0, stream.ReadByte (), "test#02");		
		
		stream.Position = 5;
		Assert.AreEqual (5, stream.Position, "test#01");		
		Assert.AreEqual (5, stream.ReadByte (), "test#02");		
		
		// Should not need to read from the underlying stream:
		stream.Position = 7;
		Assert.AreEqual (7, stream.Position, "test#01");		
		Assert.AreEqual (7, stream.ReadByte (), "test#02");		
		
		// Should not need to read from the underlying stream:
		stream.Position = 5;
		Assert.AreEqual (5, stream.Position, "test#01");		
		Assert.AreEqual (5, stream.ReadByte (), "test#02");		
				
		// Should not need to read from the underlying stream:
		stream.Position = 9;
		Assert.AreEqual (9, stream.Position, "test#01");
		Assert.AreEqual (9, stream.ReadByte (), "test#02");
		
		stream.Position = 10;
		Assert.AreEqual (10, stream.Position, "test#01");		
		Assert.AreEqual (10, stream.ReadByte (), "test#02");		
		
		stream.Position = 9;
		Assert.AreEqual (9, stream.Position, "test#01");		
		Assert.AreEqual (9, stream.ReadByte (), "test#02");		
	 }

	[Test]
	public void PositionAfterSetLength () 
	{
		BufferedStream stream = new BufferedStream (new MemoryStream ());
		stream.SetLength (32);
		stream.Position = 32;
		stream.SetLength (16);
		Assert.AreEqual (16, stream.Position, "Position==16");
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void SetLength_Disposed ()
	{
		BufferedStream stream = new BufferedStream (new MemoryStream ());
		stream.Close ();
		stream.SetLength (16);
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void Seek_ClosedMemoryStream ()
	{
		MemoryStream ms = new MemoryStream ();
		BufferedStream stream = new BufferedStream (ms);
		ms.Close ();
		stream.Seek (0, SeekOrigin.Begin);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void Seek_ClosedBufferedStream ()
	{
		BufferedStream stream = new BufferedStream (new MemoryStream ());
		stream.Close ();
		stream.Seek (0, SeekOrigin.Begin);
	}
}
}
