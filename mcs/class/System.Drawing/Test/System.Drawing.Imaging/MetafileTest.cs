//
// Metafile class unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class MetafileTest {

		public const string Bitmap = "bitmaps/non-inverted.bmp";
		public const string WmfPlaceable = "bitmaps/telescope_01.wmf";
		public const string Emf = "bitmaps/milkmateya01.emf";

		// Get the input directory depending on the runtime
		static public string getInFile (string file)
		{
			string sRslt = Path.GetFullPath ("../System.Drawing/" + file);

			if (!File.Exists (sRslt))
				sRslt = "Test/System.Drawing/" + file;

			return sRslt;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Metafile_Stream_Null ()
		{
			new Metafile ((Stream)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Metafile_String_Null ()
		{
			new Metafile ((string) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Metafile_String_Empty ()
		{
			new Metafile (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ExternalException))]
		public void Metafile_String_FileDoesNotExists ()
		{
			string filename = getInFile ("telescope_02.wmf");
			new Metafile (filename);
		}

		[Test]
		public void Metafile_String ()
		{
			string filename = getInFile (WmfPlaceable);
			Metafile mf = new Metafile (filename);
			Metafile clone = (Metafile) mf.Clone ();
		}

		[Test]
		[ExpectedException (typeof (ExternalException))]
		public void GetMetafileHeader_Bitmap ()
		{
			new Metafile (getInFile (Bitmap));
		}

		static public void Check_MetaHeader_WmfPlaceable (MetaHeader mh)
		{
			Assert.AreEqual (9, mh.HeaderSize, "HeaderSize");
			Assert.AreEqual (98, mh.MaxRecord, "MaxRecord");
			Assert.AreEqual (3, mh.NoObjects, "NoObjects");
			Assert.AreEqual (0, mh.NoParameters, "NoParameters");
			Assert.AreEqual (1737, mh.Size, "Size");
			Assert.AreEqual (1, mh.Type, "Type");
			Assert.AreEqual (0x300, mh.Version, "Version");
		}

		public static void Check_MetafileHeader_WmfPlaceable (MetafileHeader header)
		{
			Assert.AreEqual (MetafileType.WmfPlaceable, header.Type, "Type");
			Assert.AreEqual (0x300, header.Version, "Version");
			// filesize - 22, which happens to be the size (22) of a PLACEABLEMETAHEADER struct
			Assert.AreEqual (3474, header.MetafileSize, "MetafileSize");

			Assert.AreEqual (-30, header.Bounds.X, "Bounds.X");
			Assert.AreEqual (-40, header.Bounds.Y, "Bounds.Y");
			Assert.AreEqual (3096, header.Bounds.Width, "Bounds.Width");
			Assert.AreEqual (4127, header.Bounds.Height, "Bounds.Height");
			Assert.AreEqual (606, header.DpiX, "DpiX");
			Assert.AreEqual (606, header.DpiY, "DpiY");
			Assert.AreEqual (0, header.EmfPlusHeaderSize, "EmfPlusHeaderSize");
			Assert.AreEqual (0, header.LogicalDpiX, "LogicalDpiX");
			Assert.AreEqual (0, header.LogicalDpiY, "LogicalDpiY");

			Assert.IsNotNull (header.WmfHeader, "WmfHeader");
			Check_MetaHeader_WmfPlaceable (header.WmfHeader);

			Assert.IsFalse (header.IsDisplay (), "IsDisplay");
			Assert.IsFalse (header.IsEmf (), "IsEmf");
			Assert.IsFalse (header.IsEmfOrEmfPlus (), "IsEmfOrEmfPlus");
			Assert.IsFalse (header.IsEmfPlus (), "IsEmfPlus");
			Assert.IsFalse (header.IsEmfPlusDual (), "IsEmfPlusDual");
			Assert.IsFalse (header.IsEmfPlusOnly (), "IsEmfPlusOnly");
			Assert.IsTrue (header.IsWmf (), "IsWmf");
			Assert.IsTrue (header.IsWmfPlaceable (), "IsWmfPlaceable");
		}

		[Test]
		public void GetMetafileHeader_WmfPlaceable ()
		{
			using (Metafile mf = new Metafile (getInFile (WmfPlaceable))) {
				MetafileHeader header1 = mf.GetMetafileHeader ();
				Check_MetafileHeader_WmfPlaceable (header1);

				MetafileHeader header2 = mf.GetMetafileHeader ();
				Assert.IsFalse (Object.ReferenceEquals (header1, header2), "Same object");
			}
		}

		[Test]
		public void GetMetafileHeader_FromFile_WmfPlaceable ()
		{
			using (Metafile mf = new Metafile (getInFile (WmfPlaceable))) {
				MetafileHeader header1 = mf.GetMetafileHeader ();
				Check_MetafileHeader_WmfPlaceable (header1);

				MetaHeader mh1 = header1.WmfHeader;
				Check_MetaHeader_WmfPlaceable (mh1);

				MetaHeader mh2 = mf.GetMetafileHeader ().WmfHeader;
				Assert.IsFalse (Object.ReferenceEquals (mh1, mh2), "Same object");
			}
		}

		[Test]
		public void GetMetafileHeader_FromFileStream_WmfPlaceable ()
		{
			using (FileStream fs = File.OpenRead (getInFile (WmfPlaceable))) {
				using (Metafile mf = new Metafile (fs)) {
					MetafileHeader header1 = mf.GetMetafileHeader ();
					Check_MetafileHeader_WmfPlaceable (header1);

					MetaHeader mh1 = header1.WmfHeader;
					Check_MetaHeader_WmfPlaceable (mh1);

					MetaHeader mh2 = mf.GetMetafileHeader ().WmfHeader;
					Assert.IsFalse (Object.ReferenceEquals (mh1, mh2), "Same object");
				}
			}
		}

		[Test]
		public void GetMetafileHeader_FromMemoryStream_WmfPlaceable ()
		{
			MemoryStream ms;
			string filename = getInFile (WmfPlaceable);
			using (FileStream fs = File.OpenRead (filename)) {
				byte[] data = new byte[fs.Length];
				fs.Read (data, 0, data.Length);
				ms = new MemoryStream (data);
			}
			using (Metafile mf = new Metafile (ms)) {
				MetafileHeader header1 = mf.GetMetafileHeader ();
				Check_MetafileHeader_WmfPlaceable (header1);

				MetaHeader mh1 = header1.WmfHeader;
				Check_MetaHeader_WmfPlaceable (mh1);

				MetaHeader mh2 = mf.GetMetafileHeader ().WmfHeader;
				Assert.IsFalse (Object.ReferenceEquals (mh1, mh2), "Same object");
			}
			ms.Close ();
		}

		public static void Check_MetafileHeader_Emf (MetafileHeader header)
		{
			Assert.AreEqual (MetafileType.Emf, header.Type, "Type");
			Assert.AreEqual (65536, header.Version, "Version");
			// extactly the filesize
			Assert.AreEqual (20456, header.MetafileSize, "MetafileSize");

			Assert.AreEqual (0, header.Bounds.X, "Bounds.X");
			Assert.AreEqual (0, header.Bounds.Y, "Bounds.Y");
#if false
			Assert.AreEqual (759, header.Bounds.Width, "Bounds.Width");
			Assert.AreEqual (1073, header.Bounds.Height, "Bounds.Height");
			Assert.AreEqual (96f, header.DpiX, 0.5f, "DpiX");
			Assert.AreEqual (96f, header.DpiY, 0.5f, "DpiY");
			Assert.AreEqual (6619188, header.EmfPlusHeaderSize, "EmfPlusHeaderSize");
			Assert.AreEqual (3670064, header.LogicalDpiX, "LogicalDpiX");
			Assert.AreEqual (3670064, header.LogicalDpiY, "LogicalDpiY");
#endif
			try {
				Assert.IsNotNull (header.WmfHeader, "WmfHeader");
				Assert.Fail ("WmfHeader didn't throw an ArgumentException");
			}
			catch (ArgumentException) {
			}
			catch (Exception e) {
				Assert.Fail ("WmfHeader didn't throw an ArgumentException but: {0}.", e.ToString ());
			}

			Assert.IsFalse (header.IsDisplay (), "IsDisplay");
			Assert.IsTrue (header.IsEmf (), "IsEmf");
			Assert.IsTrue (header.IsEmfOrEmfPlus (), "IsEmfOrEmfPlus");
			Assert.IsFalse (header.IsEmfPlus (), "IsEmfPlus");
			Assert.IsFalse (header.IsEmfPlusDual (), "IsEmfPlusDual");
			Assert.IsFalse (header.IsEmfPlusOnly (), "IsEmfPlusOnly");
			Assert.IsFalse (header.IsWmf (), "IsWmf");
			Assert.IsFalse (header.IsWmfPlaceable (), "IsWmfPlaceable");
		}

		[Test]
		public void GetMetafileHeader_FromFile_Emf ()
		{
			using (Metafile mf = new Metafile (getInFile (Emf))) {
				MetafileHeader header1 = mf.GetMetafileHeader ();
				Check_MetafileHeader_Emf (header1);
			}
		}

		[Test]
		public void GetMetafileHeader_FromFileStream_Emf ()
		{
			using (FileStream fs = File.OpenRead (getInFile (Emf))) {
				using (Metafile mf = new Metafile (fs)) {
					MetafileHeader header1 = mf.GetMetafileHeader ();
					Check_MetafileHeader_Emf (header1);
				}
			}
		}

		[Test]
		public void GetMetafileHeader_FromMemoryStream_Emf ()
		{
			MemoryStream ms;
			string filename = getInFile (Emf);
			using (FileStream fs = File.OpenRead (filename)) {
				byte[] data = new byte[fs.Length];
				fs.Read (data, 0, data.Length);
				ms = new MemoryStream (data);
			}
			using (Metafile mf = new Metafile (ms)) {
				MetafileHeader header1 = mf.GetMetafileHeader ();
				Check_MetafileHeader_Emf (header1);
			}
			ms.Close ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Static_GetMetafileHeader_Stream_Null ()
		{
			MetafileHeader header = Metafile.GetMetafileHeader ((Stream)null);
		}

		[Test]
		public void Static_GetMetafileHeader_Stream ()
		{
			string filename = getInFile (WmfPlaceable);
			using (FileStream fs = File.OpenRead (filename)) {
				MetafileHeader header = Metafile.GetMetafileHeader (fs);
				Check_MetafileHeader_WmfPlaceable (header);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Static_GetMetafileHeader_Filename_Null ()
		{
			MetafileHeader header = Metafile.GetMetafileHeader ((string) null);
		}

		[Test]
		public void Static_GetMetafileHeader_Filename ()
		{
			string filename = getInFile (WmfPlaceable);
			MetafileHeader header = Metafile.GetMetafileHeader (filename);
			Check_MetafileHeader_WmfPlaceable (header);
		}
	}

	[TestFixture]
	public class MetafileFulltrustTest {

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Static_GetMetafileHeader_IntPtr_Zero ()
		{
			Metafile.GetMetafileHeader (IntPtr.Zero);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Static_GetMetafileHeader_IntPtr ()
		{
			string filename = MetafileTest.getInFile (MetafileTest.WmfPlaceable);
			using (Metafile mf = new Metafile (filename)) {

				IntPtr hemf = mf.GetHenhmetafile ();
				Assert.IsTrue (hemf != IntPtr.Zero, "GetHenhmetafile");

				Metafile.GetMetafileHeader (hemf);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Metafile_IntPtrBool_Zero ()
		{
			new Metafile (IntPtr.Zero, false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Metafile_IntPtrEmfType_Zero ()
		{
			new Metafile (IntPtr.Zero, EmfType.EmfOnly);
		}

		private void CheckEmptyHeader (Metafile mf, EmfType type)
		{
			MetafileHeader mh = mf.GetMetafileHeader ();
			Assert.AreEqual (0, mh.Bounds.X, "Bounds.X");
			Assert.AreEqual (0, mh.Bounds.Y, "Bounds.Y");
			Assert.AreEqual (0, mh.Bounds.Width, "Bounds.Width");
			Assert.AreEqual (0, mh.Bounds.Height, "Bounds.Height");
			Assert.AreEqual (0, mh.MetafileSize, "MetafileSize");
			switch (type) {
			case EmfType.EmfOnly:
				Assert.AreEqual (MetafileType.Emf, mh.Type, "Type");
				break;
			case EmfType.EmfPlusDual:
				Assert.AreEqual (MetafileType.EmfPlusDual, mh.Type, "Type");
				break;
			case EmfType.EmfPlusOnly:
				Assert.AreEqual (MetafileType.EmfPlusOnly, mh.Type, "Type");
				break;
			default:
				Assert.Fail ("Unknown EmfType '{0}'", type);
				break;
			}
		}

		private void Metafile_IntPtrEmfType (EmfType type)
		{
			using (Bitmap bmp = new Bitmap (10, 10, PixelFormat.Format32bppArgb)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					IntPtr hdc = g.GetHdc ();
					try {
						Metafile mf = new Metafile (hdc, type);
						CheckEmptyHeader (mf, type);
					}
					finally {
						g.ReleaseHdc (hdc);
					}
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Metafile_IntPtrEmfType_Invalid ()
		{
			Metafile_IntPtrEmfType ((EmfType)Int32.MinValue);
		}

		[Test]
		public void Metafile_IntPtrEmfType_EmfOnly ()
		{
			Metafile_IntPtrEmfType (EmfType.EmfOnly);
		}

		[Test]
		public void Metafile_IntPtrEmfType_EmfPlusDual ()
		{
			Metafile_IntPtrEmfType (EmfType.EmfPlusDual);
		}

		[Test]
		public void Metafile_IntPtrEmfType_EmfPlusOnly ()
		{
			Metafile_IntPtrEmfType (EmfType.EmfPlusOnly);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Metafile_IntPtrRectangle_Zero ()
		{
			new Metafile (IntPtr.Zero, new Rectangle (1, 2, 3, 4));
		}

		[Test]
		public void Metafile_IntPtrRectangle_Empty ()
		{
			using (Bitmap bmp = new Bitmap (10, 10, PixelFormat.Format32bppArgb)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					IntPtr hdc = g.GetHdc ();
					try {
						Metafile mf = new Metafile (hdc, new Rectangle ());
						CheckEmptyHeader (mf, EmfType.EmfPlusDual);
					}
					finally {
						g.ReleaseHdc (hdc);
					}
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Metafile_IntPtrRectangleF_Zero ()
		{
			new Metafile (IntPtr.Zero, new RectangleF (1, 2, 3, 4));
		}

		[Test]
		public void Metafile_IntPtrRectangleF_Empty ()
		{
			using (Bitmap bmp = new Bitmap (10, 10, PixelFormat.Format32bppArgb)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					IntPtr hdc = g.GetHdc ();
					try {
						Metafile mf = new Metafile (hdc, new RectangleF ());
						CheckEmptyHeader (mf, EmfType.EmfPlusDual);
					}
					finally {
						g.ReleaseHdc (hdc);
					}
				}
			}
		}

		private void Metafile_StreamEmfType (Stream stream, EmfType type)
		{
			using (Bitmap bmp = new Bitmap (10, 10, PixelFormat.Format32bppArgb)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					IntPtr hdc = g.GetHdc ();
					try {
						Metafile mf = new Metafile (stream, hdc, type);
						CheckEmptyHeader (mf, type);
					}
					finally {
						g.ReleaseHdc (hdc);
					}
				}
			}
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Metafile_StreamIntPtrEmfType_Null ()
		{
			Metafile_StreamEmfType (null, EmfType.EmfOnly);
		}

		[Test]
		public void Metafile_StreamIntPtrEmfType_EmfOnly ()
		{
			using (MemoryStream ms = new MemoryStream ()) {
				Metafile_StreamEmfType (ms, EmfType.EmfOnly);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Metafile_StreamIntPtrEmfType_Invalid ()
		{
			using (MemoryStream ms = new MemoryStream ()) {
				Metafile_StreamEmfType (ms, (EmfType)Int32.MinValue);
			}
		}
	}
}
