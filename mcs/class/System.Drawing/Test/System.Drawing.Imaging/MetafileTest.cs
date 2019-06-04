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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	public class MetafileTest {

		public static string Bitmap = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/non-inverted.bmp");
		public static string WmfPlaceable = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf");
		public static string Emf = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/milkmateya01.emf");

		[Test]
		public void Metafile_Stream_Null ()
		{
			Assert.Throws<ArgumentException> (() => new Metafile ((Stream)null));
		}

		[Test]
		public void Metafile_String_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new Metafile ((string) null));
		}

		[Test]
		public void Metafile_String_Empty ()
		{
			Assert.Throws<ArgumentException> (() => new Metafile (String.Empty));
		}

		[Test]
		public void Metafile_String_FileDoesNotExists ()
		{
			string filename = "non_existing.wmf";
			Assert.Throws<ExternalException> (() => new Metafile (filename));
		}

		[Test]
		public void Metafile_String ()
		{
			string filename = WmfPlaceable;
			Metafile mf = new Metafile (filename);
			Metafile clone = (Metafile) mf.Clone ();
		}

		[Test]
		public void GetMetafileHeader_Bitmap ()
		{
			Assert.Throws<ExternalException> (() => new Metafile (Bitmap));
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
			using (Metafile mf = new Metafile (WmfPlaceable)) {
				MetafileHeader header1 = mf.GetMetafileHeader ();
				Check_MetafileHeader_WmfPlaceable (header1);

				MetafileHeader header2 = mf.GetMetafileHeader ();
				Assert.IsFalse (Object.ReferenceEquals (header1, header2), "Same object");
			}
		}

		[Test]
		public void GetMetafileHeader_FromFile_WmfPlaceable ()
		{
			using (Metafile mf = new Metafile (WmfPlaceable)) {
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
			using (FileStream fs = File.OpenRead (WmfPlaceable)) {
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
			string filename = WmfPlaceable;
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
			using (Metafile mf = new Metafile (Emf)) {
				MetafileHeader header1 = mf.GetMetafileHeader ();
				Check_MetafileHeader_Emf (header1);
			}
		}

		[Test]
		public void GetMetafileHeader_FromFileStream_Emf ()
		{
			using (FileStream fs = File.OpenRead (Emf)) {
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
			string filename = Emf;
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
		public void Static_GetMetafileHeader_Stream_Null ()
		{
			Assert.Throws<NullReferenceException> (() => Metafile.GetMetafileHeader ((Stream)null));
		}

		[Test]
		public void Static_GetMetafileHeader_Stream ()
		{
			string filename = WmfPlaceable;
			using (FileStream fs = File.OpenRead (filename)) {
				MetafileHeader header = Metafile.GetMetafileHeader (fs);
				Check_MetafileHeader_WmfPlaceable (header);
			}
		}

		[Test]
		public void Static_GetMetafileHeader_Filename_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => Metafile.GetMetafileHeader ((string) null));
		}

		[Test]
		public void Static_GetMetafileHeader_Filename ()
		{
			string filename = WmfPlaceable;
			MetafileHeader header = Metafile.GetMetafileHeader (filename);
			Check_MetafileHeader_WmfPlaceable (header);
		}
	}

	[TestFixture]
	public class MetafileFulltrustTest {

		private Font test_font;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			try {
				test_font = new Font (FontFamily.GenericMonospace, 12);
			}
			catch (ArgumentException) {
			}
		}

		[Test]
		public void Static_GetMetafileHeader_IntPtr_Zero ()
		{
			Assert.Throws<ArgumentException> (() => Metafile.GetMetafileHeader (IntPtr.Zero));
		}

		[Test]
		public void Static_GetMetafileHeader_IntPtr ()
		{
			string filename = MetafileTest.WmfPlaceable;
			using (Metafile mf = new Metafile (filename)) {

				IntPtr hemf = mf.GetHenhmetafile ();
				Assert.IsTrue (hemf != IntPtr.Zero, "GetHenhmetafile");

				Assert.Throws<ArgumentException> (() => Metafile.GetMetafileHeader (hemf));
			}
		}

		[Test]
		public void Metafile_IntPtrBool_Zero ()
		{
			Assert.Throws<ArgumentException> (() => new Metafile (IntPtr.Zero, false));
		}

		[Test]
		public void Metafile_IntPtrEmfType_Zero ()
		{
			Assert.Throws<ArgumentException> (() => new Metafile (IntPtr.Zero, EmfType.EmfOnly));
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
		public void Metafile_IntPtrEmfType_Invalid ()
		{
			Assert.Throws<ArgumentException> (() => Metafile_IntPtrEmfType ((EmfType)Int32.MinValue));
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
		public void Metafile_IntPtrRectangle_Zero ()
		{
			Assert.Throws<ArgumentException> (() => new Metafile (IntPtr.Zero, new Rectangle (1, 2, 3, 4)));
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
		public void Metafile_IntPtrRectangleF_Zero ()
		{
			Assert.Throws<ArgumentException> (() => new Metafile (IntPtr.Zero, new RectangleF (1, 2, 3, 4)));
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
		public void Metafile_StreamIntPtrEmfType_Null ()
		{
			Assert.Throws<NullReferenceException> (() => Metafile_StreamEmfType (null, EmfType.EmfOnly));
		}

		[Test]
		public void Metafile_StreamIntPtrEmfType_EmfOnly ()
		{
			using (MemoryStream ms = new MemoryStream ()) {
				Metafile_StreamEmfType (ms, EmfType.EmfOnly);
			}
		}

		[Test]
		public void Metafile_StreamIntPtrEmfType_Invalid ()
		{
			using (MemoryStream ms = new MemoryStream ()) {
				Assert.Throws<ArgumentException> (() => Metafile_StreamEmfType (ms, (EmfType)Int32.MinValue));
			}
		}

		private void CreateFilename (EmfType type, bool single)
		{
			string name = String.Format ("{0}-{1}.emf", type, single ? "Single" : "Multiple");
			string filename = Path.Combine (Path.GetTempPath (), name);
			Metafile mf;
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppArgb)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					IntPtr hdc = g.GetHdc ();
					try {
						mf = new Metafile (filename, hdc, type);
						Assert.AreEqual (0, new FileInfo (filename).Length, "Empty");
					}
					finally {
						g.ReleaseHdc (hdc);
					}
				}
				long size = 0;
				using (Graphics g = Graphics.FromImage (mf)) {
					g.FillRectangle (Brushes.BlueViolet, 10, 10, 80, 80);
					size = new FileInfo (filename).Length;
					Assert.AreEqual (0, size, "Still-Empty");
				}
// FIXME / doesn't work on mono yet
//				size = new FileInfo (filename).Length;
//				Assert.IsTrue (size > 0, "Non-Empty/GraphicsDisposed");
				if (!single) {
					// can we append stuff ?
					using (Graphics g = Graphics.FromImage (mf)) {
						g.DrawRectangle (Pens.Azure, 10, 10, 80, 80);
						// happily no :)
					}
				}
				mf.Dispose ();
				Assert.AreEqual (size, new FileInfo (filename).Length, "Non-Empty/MetafileDisposed");
			}
		}

		[Test]
		public void CreateFilename_SingleGraphics_EmfOnly ()
		{
			CreateFilename (EmfType.EmfOnly, true);
		}

		[Test]
		public void CreateFilename_SingleGraphics_EmfPlusDual ()
		{
			CreateFilename (EmfType.EmfPlusDual, true);
		}

		[Test]
		public void CreateFilename_SingleGraphics_EmfPlusOnly ()
		{
			CreateFilename (EmfType.EmfPlusOnly, true);
		}

		[Test]
		public void CreateFilename_MultipleGraphics_EmfOnly ()
		{
			Assert.Throws<OutOfMemoryException> (() => CreateFilename (EmfType.EmfOnly, false));
		}

		[Test]
		public void CreateFilename_MultipleGraphics_EmfPlusDual ()
		{
			Assert.Throws<OutOfMemoryException> (() => CreateFilename (EmfType.EmfPlusDual, false));
		}

		[Test]
		public void CreateFilename_MultipleGraphics_EmfPlusOnly ()
		{
			Assert.Throws<OutOfMemoryException> (() => CreateFilename (EmfType.EmfPlusOnly, false));
		}

		[Test]
		public void Measure ()
		{
			if (test_font == null)
				Assert.Ignore ("No font family could be found.");

			Metafile mf;
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppArgb)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					IntPtr hdc = g.GetHdc ();
					try {
						mf = new Metafile (hdc, EmfType.EmfPlusOnly);
					}
					finally {
						g.ReleaseHdc (hdc);
					}
				}
				using (Graphics g = Graphics.FromImage (mf)) {
					string text = "this\nis a test";
					CharacterRange[] ranges = new CharacterRange[2];
					ranges[0] = new CharacterRange (0, 5);
					ranges[1] = new CharacterRange (5, 9);

					SizeF size = g.MeasureString (text, test_font);
					Assert.IsFalse (size.IsEmpty, "MeasureString");

					StringFormat sf = new StringFormat ();
					sf.FormatFlags = StringFormatFlags.NoClip;
					sf.SetMeasurableCharacterRanges (ranges);

					RectangleF rect = new RectangleF (0, 0, size.Width, size.Height);
					Region[] region = g.MeasureCharacterRanges (text, test_font, rect, sf);
					Assert.AreEqual (2, region.Length, "MeasureCharacterRanges");
				}
				mf.Dispose ();
			}
		}

		[Test]
		public void WorldTransforms ()
		{
			Metafile mf;
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppArgb)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					IntPtr hdc = g.GetHdc ();
					try {
						mf = new Metafile (hdc, EmfType.EmfPlusOnly);
					}
					finally {
						g.ReleaseHdc (hdc);
					}
				}
				using (Graphics g = Graphics.FromImage (mf)) {
					Assert.IsTrue (g.Transform.IsIdentity, "Initial/IsIdentity");
					g.ScaleTransform (2f, 0.5f);
					Assert.IsFalse (g.Transform.IsIdentity, "Scale/IsIdentity");
					g.RotateTransform (90);
					g.TranslateTransform (-2, 2);
					Matrix m = g.Transform;
					g.MultiplyTransform (m);
					// check
					float[] elements = g.Transform.Elements;
					Assert.AreEqual (-1f, elements[0], 0.00001f, "a0");
					Assert.AreEqual (0f, elements[1], 0.00001f, "a1");
					Assert.AreEqual (0f, elements[2], 0.00001f, "a2");
					Assert.AreEqual (-1f, elements[3], 0.00001f, "a3");
					Assert.AreEqual (-2f, elements[4], 0.00001f, "a4");
					Assert.AreEqual (-3f, elements[5], 0.00001f, "a5");

					g.Transform = m;
					elements = g.Transform.Elements;
					Assert.AreEqual (0f, elements[0], 0.00001f, "b0");
					Assert.AreEqual (0.5f, elements[1], 0.00001f, "b1");
					Assert.AreEqual (-2f, elements[2], 0.00001f, "b2");
					Assert.AreEqual (0f, elements[3], 0.00001f, "b3");
					Assert.AreEqual (-4f, elements[4], 0.00001f, "b4");
					Assert.AreEqual (-1f, elements[5], 0.00001f, "b5");

					g.ResetTransform ();
					Assert.IsTrue (g.Transform.IsIdentity, "Reset/IsIdentity");
				}
				mf.Dispose ();
			}
		}
	}
}
