/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using NUnit.Framework;
using Directory = Lucene.Net.Store.Directory;
using FSDirectory = Lucene.Net.Store.FSDirectory;
using InputStream = Lucene.Net.Store.InputStream;
using OutputStream = Lucene.Net.Store.OutputStream;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using _TestHelper = Lucene.Net.Store.TestHelper;
namespace Lucene.Net.Index
{
	
	
	/// <author>  dmitrys@earthlink.net
	/// </author>
	/// <version>  $Id: TestCompoundFile.java,v 1.5 2004/03/29 22:48:06 cutting Exp $
	/// </version>
    [TestFixture]
    public class TestCompoundFile
	{
		/// <summary>Main for running test case by itself. </summary>
		[STAThread]
		public static void  Main(System.String[] args)
		{
            /*
			TestRunner.run(new NUnit.Framework.TestSuite(typeof(TestCompoundFile)));
			//        TestRunner.run (new TestCompoundFile("testSingleFile"));
			//        TestRunner.run (new TestCompoundFile("testTwoFiles"));
			//        TestRunner.run (new TestCompoundFile("testRandomFiles"));
			//        TestRunner.run (new TestCompoundFile("testClonedStreamsClosing"));
			//        TestRunner.run (new TestCompoundFile("testReadAfterClose"));
			//        TestRunner.run (new TestCompoundFile("testRandomAccess"));
			//        TestRunner.run (new TestCompoundFile("testRandomAccessClones"));
			//        TestRunner.run (new TestCompoundFile("testFileNotFound"));
			//        TestRunner.run (new TestCompoundFile("testReadPastEOF"));
			
			//        TestRunner.run (new TestCompoundFile("testIWCreate"));
            */
		}
		
		
		private Directory dir;
		
		[TestFixtureSetUp]
		public virtual void  SetUp()
		{
			//dir = new RAMDirectory();
			dir = FSDirectory.GetDirectory(new System.IO.FileInfo(SupportClass.AppSettings.Get("tempDir", "testIndex")), true);
		}
		
		
		/// <summary>Creates a file of the specified size with random data. </summary>
		private void  CreateRandomFile(Directory dir, System.String name, int size)
		{
			OutputStream os = dir.CreateFile(name);
			for (int i = 0; i < size; i++)
			{
				byte b = (byte) (((new System.Random()).NextDouble()) * 256);
				os.WriteByte(b);
			}
			os.Close();
		}
		
		/// <summary>Creates a file of the specified size with sequential data. The first
		/// byte is written as the start byte provided. All subsequent bytes are
		/// computed as start + offset where offset is the number of the byte.
		/// </summary>
		private void  CreateSequenceFile(Directory dir, System.String name, byte start, int size)
		{
			OutputStream os = dir.CreateFile(name);
			for (int i = 0; i < size; i++)
			{
				os.WriteByte(start);
				start++;
			}
			os.Close();
		}
		
		
		private void  AssertSameStreams(System.String msg, InputStream expected, InputStream test)
		{
			Assert.IsNotNull(expected, msg + " null expected");
			Assert.IsNotNull(test, msg + " null test");
			Assert.AreEqual(expected.Length(), test.Length(), msg + " length");
			Assert.AreEqual(expected.GetFilePointer(), test.GetFilePointer(), msg + " position");
			
			byte[] expectedBuffer = new byte[512];
			byte[] testBuffer = new byte[expectedBuffer.Length];
			
			long remainder = expected.Length() - expected.GetFilePointer();
			while (remainder > 0)
			{
				int readLen = (int) System.Math.Min(remainder, expectedBuffer.Length);
				expected.ReadBytes(expectedBuffer, 0, readLen);
				test.ReadBytes(testBuffer, 0, readLen);
				AssertEqualArrays(msg + ", remainder " + remainder, expectedBuffer, testBuffer, 0, readLen);
				remainder -= readLen;
			}
		}
		
		
		private void  AssertSameStreams(System.String msg, InputStream expected, InputStream actual, long seekTo)
		{
			if (seekTo >= 0 && seekTo < expected.Length())
			{
				expected.Seek(seekTo);
				actual.Seek(seekTo);
				AssertSameStreams(msg + ", seek(mid)", expected, actual);
			}
		}
		
		
		
		private void  AssertSameSeekBehavior(System.String msg, InputStream expected, InputStream actual)
		{
			// seek to 0
			long point = 0;
			AssertSameStreams(msg + ", seek(0)", expected, actual, point);
			
			// seek to middle
			point = expected.Length() / 2L;
			AssertSameStreams(msg + ", seek(mid)", expected, actual, point);
			
			// seek to end - 2
			point = expected.Length() - 2;
			AssertSameStreams(msg + ", seek(end-2)", expected, actual, point);
			
			// seek to end - 1
			point = expected.Length() - 1;
			AssertSameStreams(msg + ", seek(end-1)", expected, actual, point);
			
			// seek to the end
			point = expected.Length();
			AssertSameStreams(msg + ", seek(end)", expected, actual, point);
			
			// seek past end
			point = expected.Length() + 1;
			AssertSameStreams(msg + ", seek(end+1)", expected, actual, point);
		}
		
		
		private void  AssertEqualArrays(System.String msg, byte[] expected, byte[] test, int start, int len)
		{
			Assert.IsNotNull(expected, msg + " null expected");
			Assert.IsNotNull(test, msg + " null test");
			
			for (int i = start; i < len; i++)
			{
				Assert.AreEqual(expected[i], test[i], msg + " " + i);
			}
		}
		
		
		// ===========================================================
		//  Tests of the basic CompoundFile functionality
		// ===========================================================
		
		
		/// <summary>This test creates compound file based on a single file.
		/// Files of different sizes are tested: 0, 1, 10, 100 bytes.
		/// </summary>
		[Test]
		public virtual void  TestSingleFile()
		{
			int[] data = new int[]{0, 1, 10, 100};
			for (int i = 0; i < data.Length; i++)
			{
				System.String name = "t" + data[i];
				CreateSequenceFile(dir, name, (byte) 0, data[i]);
				CompoundFileWriter csw = new CompoundFileWriter(dir, name + ".cfs");
				csw.AddFile(name);
				csw.Close();
				
				CompoundFileReader csr = new CompoundFileReader(dir, name + ".cfs");
				InputStream expected = dir.OpenFile(name);
				InputStream actual = csr.OpenFile(name);
				AssertSameStreams(name, expected, actual);
				AssertSameSeekBehavior(name, expected, actual);
				expected.Close();
				actual.Close();
				csr.Close();
			}
		}
		
		
		/// <summary>This test creates compound file based on two files.
		/// 
		/// </summary>
		[Test]
        public virtual void  TestTwoFiles()
		{
			CreateSequenceFile(dir, "d1", (byte) 0, 15);
			CreateSequenceFile(dir, "d2", (byte) 0, 114);
			
			CompoundFileWriter csw = new CompoundFileWriter(dir, "d.csf");
			csw.AddFile("d1");
			csw.AddFile("d2");
			csw.Close();
			
			CompoundFileReader csr = new CompoundFileReader(dir, "d.csf");
			InputStream expected = dir.OpenFile("d1");
			InputStream actual = csr.OpenFile("d1");
			AssertSameStreams("d1", expected, actual);
			AssertSameSeekBehavior("d1", expected, actual);
			expected.Close();
			actual.Close();
			
			expected = dir.OpenFile("d2");
			actual = csr.OpenFile("d2");
			AssertSameStreams("d2", expected, actual);
			AssertSameSeekBehavior("d2", expected, actual);
			expected.Close();
			actual.Close();
			csr.Close();
		}
		
		/// <summary>This test creates a compound file based on a large number of files of
		/// various length. The file content is generated randomly. The sizes range
		/// from 0 to 1Mb. Some of the sizes are selected to test the buffering
		/// logic in the file reading code. For this the chunk variable is set to
		/// the length of the buffer used internally by the compound file logic.
		/// </summary>
		[Test]
        public virtual void  TestRandomFiles()
		{
			// Setup the test segment
			System.String segment = "test";
			int chunk = 1024; // internal buffer size used by the stream
			CreateRandomFile(dir, segment + ".zero", 0);
			CreateRandomFile(dir, segment + ".one", 1);
			CreateRandomFile(dir, segment + ".ten", 10);
			CreateRandomFile(dir, segment + ".hundred", 100);
			CreateRandomFile(dir, segment + ".big1", chunk);
			CreateRandomFile(dir, segment + ".big2", chunk - 1);
			CreateRandomFile(dir, segment + ".big3", chunk + 1);
			CreateRandomFile(dir, segment + ".big4", 3 * chunk);
			CreateRandomFile(dir, segment + ".big5", 3 * chunk - 1);
			CreateRandomFile(dir, segment + ".big6", 3 * chunk + 1);
			CreateRandomFile(dir, segment + ".big7", 1000 * chunk);
			
			// Setup extraneous files
			CreateRandomFile(dir, "onetwothree", 100);
			CreateRandomFile(dir, segment + ".notIn", 50);
			CreateRandomFile(dir, segment + ".notIn2", 51);
			
			// Now test
			CompoundFileWriter csw = new CompoundFileWriter(dir, "test.cfs");
			System.String[] data = new System.String[]{".zero", ".one", ".ten", ".hundred", ".big1", ".big2", ".big3", ".big4", ".big5", ".big6", ".big7"};
			for (int i = 0; i < data.Length; i++)
			{
				csw.AddFile(segment + data[i]);
			}
			csw.Close();
			
			CompoundFileReader csr = new CompoundFileReader(dir, "test.cfs");
			for (int i = 0; i < data.Length; i++)
			{
				InputStream check = dir.OpenFile(segment + data[i]);
				InputStream test = csr.OpenFile(segment + data[i]);
				AssertSameStreams(data[i], check, test);
				AssertSameSeekBehavior(data[i], check, test);
				test.Close();
				check.Close();
			}
			csr.Close();
		}
		
		
		/// <summary>Setup a larger compound file with a number of components, each of
		/// which is a sequential file (so that we can easily tell that we are
		/// reading in the right byte). The methods sets up 20 files - f0 to f19,
		/// the size of each file is 1000 bytes.
		/// </summary>
		private void  SetUp_2()
		{
			CompoundFileWriter cw = new CompoundFileWriter(dir, "f.comp");
			for (int i = 0; i < 20; i++)
			{
				CreateSequenceFile(dir, "f" + i, (byte) 0, 2000);
				cw.AddFile("f" + i);
			}
			cw.Close();
		}
		
		[Test]
		public virtual void  TestReadAfterClose()
		{
			Demo_FSInputStreamBug((FSDirectory) dir, "test");
		}
		
		private void  Demo_FSInputStreamBug(FSDirectory fsdir, System.String file)
		{
			// Setup the test file - we need more than 1024 bytes
			OutputStream os = fsdir.CreateFile(file);
			for (int i = 0; i < 2000; i++)
			{
				os.WriteByte((byte) i);
			}
			os.Close();
			
			InputStream in_Renamed = fsdir.OpenFile(file);
			
			// This read primes the buffer in InputStream
			byte b = in_Renamed.ReadByte();
			
			// Close the file
			in_Renamed.Close();
			
			// ERROR: this call should fail, but succeeds because the buffer
			// is still filled
			b = in_Renamed.ReadByte();
			
			// ERROR: this call should fail, but succeeds for some reason as well
			in_Renamed.Seek(1099);
			
            try
            {
                // OK: this call correctly fails. We are now past the 1024 internal
                // buffer, so an actual IO is attempted, which fails
                b = in_Renamed.ReadByte();
            }
            catch (System.IO.IOException e)
            {
            }
            catch (System.Exception)
            {
            }
		}
		
		
		internal static bool IsCSInputStream(InputStream is_Renamed)
		{
			return is_Renamed is CompoundFileReader.CSInputStream;
		}
		
		internal static bool IsCSInputStreamOpen(InputStream is_Renamed)
		{
            try
            {
                if (IsCSInputStream(is_Renamed))
                {
                    CompoundFileReader.CSInputStream cis = (CompoundFileReader.CSInputStream) is_Renamed;
				
                    return _TestHelper.IsFSInputStreamOpen(cis.base_Renamed);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
		}
		
		[Test]
		public virtual void  TestClonedStreamsClosing()
		{
			SetUp_2();
			CompoundFileReader cr = new CompoundFileReader(dir, "f.comp");
			
			// basic clone
			InputStream expected = dir.OpenFile("f11");
			Assert.IsTrue(_TestHelper.IsFSInputStreamOpen(expected));
			
			InputStream one = cr.OpenFile("f11");
			Assert.IsTrue(IsCSInputStreamOpen(one));
			
			InputStream two = (InputStream) one.Clone();
			Assert.IsTrue(IsCSInputStreamOpen(two));
			
			AssertSameStreams("basic clone one", expected, one);
			expected.Seek(0);
			AssertSameStreams("basic clone two", expected, two);
			
			// Now close the first stream
			one.Close();
			Assert.IsTrue(IsCSInputStreamOpen(one), "Only close when cr is closed");
			
			// The following should really fail since we couldn't expect to
			// access a file once close has been called on it (regardless of
			// buffering and/or clone magic)
			expected.Seek(0);
			two.Seek(0);
			AssertSameStreams("basic clone two/2", expected, two);
			
			
			// Now close the compound reader
			cr.Close();
			Assert.IsFalse(IsCSInputStreamOpen(one), "Now closed one");
			Assert.IsFalse(IsCSInputStreamOpen(two), "Now closed two");
			
			// The following may also fail since the compound stream is closed
			expected.Seek(0);
			two.Seek(0);
			//AssertSameStreams("basic clone two/3", expected, two);
			
			
			// Now close the second clone
			two.Close();
			expected.Seek(0);
			two.Seek(0);
			//AssertSameStreams("basic clone two/4", expected, two);
			
			expected.Close();
		}
		
		
		/// <summary>This test opens two files from a compound stream and verifies that
		/// their file positions are independent of each other.
		/// </summary>
		[Test]
        public virtual void  TestRandomAccess()
		{
			SetUp_2();
			CompoundFileReader cr = new CompoundFileReader(dir, "f.comp");
			
			// Open two files
			InputStream e1 = dir.OpenFile("f11");
			InputStream e2 = dir.OpenFile("f3");
			
			InputStream a1 = cr.OpenFile("f11");
			InputStream a2 = dir.OpenFile("f3");
			
			// Seek the first pair
			e1.Seek(100);
			a1.Seek(100);
			Assert.AreEqual(100, e1.GetFilePointer());
			Assert.AreEqual(100, a1.GetFilePointer());
			byte be1 = e1.ReadByte();
			byte ba1 = a1.ReadByte();
			Assert.AreEqual(be1, ba1);
			
			// Now seek the second pair
			e2.Seek(1027);
			a2.Seek(1027);
			Assert.AreEqual(1027, e2.GetFilePointer());
			Assert.AreEqual(1027, a2.GetFilePointer());
			byte be2 = e2.ReadByte();
			byte ba2 = a2.ReadByte();
			Assert.AreEqual(be2, ba2);
			
			// Now make sure the first one didn't move
			Assert.AreEqual(101, e1.GetFilePointer());
			Assert.AreEqual(101, a1.GetFilePointer());
			be1 = e1.ReadByte();
			ba1 = a1.ReadByte();
			Assert.AreEqual(be1, ba1);
			
			// Now more the first one again, past the buffer length
			e1.Seek(1910);
			a1.Seek(1910);
			Assert.AreEqual(1910, e1.GetFilePointer());
			Assert.AreEqual(1910, a1.GetFilePointer());
			be1 = e1.ReadByte();
			ba1 = a1.ReadByte();
			Assert.AreEqual(be1, ba1);
			
			// Now make sure the second set didn't move
			Assert.AreEqual(1028, e2.GetFilePointer());
			Assert.AreEqual(1028, a2.GetFilePointer());
			be2 = e2.ReadByte();
			ba2 = a2.ReadByte();
			Assert.AreEqual(be2, ba2);
			
			// Move the second set back, again cross the buffer size
			e2.Seek(17);
			a2.Seek(17);
			Assert.AreEqual(17, e2.GetFilePointer());
			Assert.AreEqual(17, a2.GetFilePointer());
			be2 = e2.ReadByte();
			ba2 = a2.ReadByte();
			Assert.AreEqual(be2, ba2);
			
			// Finally, make sure the first set didn't move
			// Now make sure the first one didn't move
			Assert.AreEqual(1911, e1.GetFilePointer());
			Assert.AreEqual(1911, a1.GetFilePointer());
			be1 = e1.ReadByte();
			ba1 = a1.ReadByte();
			Assert.AreEqual(be1, ba1);
			
			e1.Close();
			e2.Close();
			a1.Close();
			a2.Close();
			cr.Close();
		}
		
		/// <summary>This test opens two files from a compound stream and verifies that
		/// their file positions are independent of each other.
		/// </summary>
		[Test]
        public virtual void  TestRandomAccessClones()
		{
			SetUp_2();
			CompoundFileReader cr = new CompoundFileReader(dir, "f.comp");
			
			// Open two files
			InputStream e1 = cr.OpenFile("f11");
			InputStream e2 = cr.OpenFile("f3");
			
			InputStream a1 = (InputStream) e1.Clone();
			InputStream a2 = (InputStream) e2.Clone();
			
			// Seek the first pair
			e1.Seek(100);
			a1.Seek(100);
			Assert.AreEqual(100, e1.GetFilePointer());
			Assert.AreEqual(100, a1.GetFilePointer());
			byte be1 = e1.ReadByte();
			byte ba1 = a1.ReadByte();
			Assert.AreEqual(be1, ba1);
			
			// Now seek the second pair
			e2.Seek(1027);
			a2.Seek(1027);
			Assert.AreEqual(1027, e2.GetFilePointer());
			Assert.AreEqual(1027, a2.GetFilePointer());
			byte be2 = e2.ReadByte();
			byte ba2 = a2.ReadByte();
			Assert.AreEqual(be2, ba2);
			
			// Now make sure the first one didn't move
			Assert.AreEqual(101, e1.GetFilePointer());
			Assert.AreEqual(101, a1.GetFilePointer());
			be1 = e1.ReadByte();
			ba1 = a1.ReadByte();
			Assert.AreEqual(be1, ba1);
			
			// Now more the first one again, past the buffer length
			e1.Seek(1910);
			a1.Seek(1910);
			Assert.AreEqual(1910, e1.GetFilePointer());
			Assert.AreEqual(1910, a1.GetFilePointer());
			be1 = e1.ReadByte();
			ba1 = a1.ReadByte();
			Assert.AreEqual(be1, ba1);
			
			// Now make sure the second set didn't move
			Assert.AreEqual(1028, e2.GetFilePointer());
			Assert.AreEqual(1028, a2.GetFilePointer());
			be2 = e2.ReadByte();
			ba2 = a2.ReadByte();
			Assert.AreEqual(be2, ba2);
			
			// Move the second set back, again cross the buffer size
			e2.Seek(17);
			a2.Seek(17);
			Assert.AreEqual(17, e2.GetFilePointer());
			Assert.AreEqual(17, a2.GetFilePointer());
			be2 = e2.ReadByte();
			ba2 = a2.ReadByte();
			Assert.AreEqual(be2, ba2);
			
			// Finally, make sure the first set didn't move
			// Now make sure the first one didn't move
			Assert.AreEqual(1911, e1.GetFilePointer());
			Assert.AreEqual(1911, a1.GetFilePointer());
			be1 = e1.ReadByte();
			ba1 = a1.ReadByte();
			Assert.AreEqual(be1, ba1);
			
			e1.Close();
			e2.Close();
			a1.Close();
			a2.Close();
			cr.Close();
		}
		
		[Test]
		public virtual void  TestFileNotFound()
		{
			SetUp_2();
			CompoundFileReader cr = new CompoundFileReader(dir, "f.comp");
			
			// Open two files
			try
			{
				InputStream e1 = cr.OpenFile("bogus");
				Assert.Fail("File not found");
			}
			catch (System.IO.IOException e)
			{
				/* success */
				//System.out.println("SUCCESS: File Not Found: " + e);
			}
			
			cr.Close();
		}
		
		[Test]
		public virtual void  TestReadPastEOF()
		{
			SetUp_2();
			CompoundFileReader cr = new CompoundFileReader(dir, "f.comp");
			InputStream is_Renamed = cr.OpenFile("f2");
			is_Renamed.Seek(is_Renamed.Length() - 10);
			byte[] b = new byte[100];
			is_Renamed.ReadBytes(b, 0, 10);
			
			try
			{
				byte test = is_Renamed.ReadByte();
				Assert.Fail("Single byte read past end of file");
			}
			catch (System.IO.IOException e)
			{
				/* success */
				//System.out.println("SUCCESS: single byte read past end of file: " + e);
			}
			
			is_Renamed.Seek(is_Renamed.Length() - 10);
			try
			{
				is_Renamed.ReadBytes(b, 0, 50);
				Assert.Fail("Block read past end of file");
			}
			catch (System.IO.IOException e)
			{
				/* success */
				//System.out.println("SUCCESS: block read past end of file: " + e);
			}
			
			is_Renamed.Close();
			cr.Close();
		}
	}
}