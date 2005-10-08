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
// Copyright (c) 2002-2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// Based on work done by:
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)

using System.IO;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;


namespace System.Windows.Forms {

	[Serializable]
	public sealed class ImageListStreamer : ISerializable {

		private static byte [] signature = new byte [] {77 , 83 , 70 , 116};

		private readonly ImageList.ImageCollection imageCollection;
		private Image [] images;
		private Size image_size;
		private Color back_color;

		internal ImageListStreamer (ImageList.ImageCollection imageCollection) {
			this.imageCollection = imageCollection;
		}
		
		private ImageListStreamer (SerializationInfo info, StreamingContext context) {

			byte [] data = (byte [])info.GetValue ("Data", typeof (byte []));
			if (data == null || data.Length <= signature.Length)
				return;
			// check the signature ( 'MSFt' )
			if (data [0] != signature [0] || data [1] != signature [1] ||
					data [2] != signature [2] ||  data [3] != signature [3])
				return;

			// calulate size of array needed for decomressed data
			int i = 0;
			int real_byte_count = 0;
			for (i = signature.Length; i < data.Length; i += 2)
				real_byte_count += data [i];

			if (real_byte_count == 0)
				return;
			
			int j = 0;
			byte [] decompressed = new byte [real_byte_count];

			for (i = signature.Length; i < data.Length; i += 2) {
				for (int k = 0; k < data [i]; k++)
					decompressed [j++] = data [i + 1];
			}

			MemoryStream stream = new MemoryStream (decompressed);
			BinaryReader reader = new BinaryReader (stream);

			try {
				// read image list header
				reader.ReadUInt16 ();	// usMagic
				reader.ReadUInt16 ();	// usVersion
				ushort cCurImage = reader.ReadUInt16 ();
				reader.ReadUInt16 ();	// cMaxImage
				reader.ReadUInt16 ();	// cGrow
				ushort cx	 = reader.ReadUInt16 ();
				ushort cy	 = reader.ReadUInt16 ();
				uint   bkcolor	 = reader.ReadUInt32 ();
				reader.ReadUInt16 ();	// flags

				short [] ovls = new short [4];
				for (i = 0; i < ovls.Length; i++) {
					ovls[i] = reader.ReadInt16 ();
				}

				image_size = new Size (cx, cy);
				back_color = Color.FromArgb ((int) bkcolor);
						
				MemoryStream start = new MemoryStream (decompressed,
						(int) stream.Position,
						(int) stream.Length - (int) stream.Position,
						false);

				Image image = Image.FromStream (start);

				// Holy calamity. This is what happens on MS
				// if the background colour is 0xFFFFFFFF (CLR_NONE)
				// the mask is set to the color at pixel 0, 0
				Bitmap bmp = image as Bitmap;
				if (bkcolor == 0xFFFFFFFF && bmp != null)
					back_color = bmp.GetPixel (0, 0);

				int step = image.Width / cx;
				images = new Image [cCurImage];

				Rectangle dest_rect = new Rectangle (0, 0, cx, cy);
				for (int r = 0 ; r < cCurImage ; r++) {
					Rectangle area = new Rectangle (
						(r % step) * cx,
						(r / step) * cy,
						cx, cy);
					Bitmap b = new Bitmap (cx, cy);
					using (Graphics g = Graphics.FromImage (b)) {
						g.DrawImage (image, dest_rect, area, 
								GraphicsUnit.Pixel);
					}
					b.MakeTransparent (back_color);
					images [r] = b;
				}

			} catch (Exception e) {

			}
		}

		[MonoTODO ("RLE is broken")]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			MemoryStream stream = new MemoryStream ();
			BinaryWriter writer = new BinaryWriter (stream);

			writer.Write (signature);
			writer.Write (GetStreamData ());
			
			info.AddValue ("Data", stream.ToArray (), typeof (byte []));
		}

		[MonoTODO ("Images should be written to the stream")]
		private byte [] GetStreamData ()
		{
			MemoryStream stream = new MemoryStream ();
			BinaryWriter writer = new BinaryWriter (stream);
			Image [] images = (imageCollection != null) ? imageCollection.ToArray () : this.images;

			int cols = 4;
			int rows = images.Length / cols;
			if (images.Length % cols > 0)
				++rows;

			Bitmap main = new Bitmap (cols * ImageSize.Width, rows * ImageSize.Height);
			using (Graphics g = Graphics.FromImage (main)) {
				g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), 0, 0, cols * ImageSize.Width, rows * ImageSize.Height);
				for (int i = 0; i < images.Length; i++) {
					g.DrawImage (images [i], (i % cols) * ImageSize.Width,
							(i / cols) * ImageSize.Height);
				}
			}

			writer.Write ((ushort) (('L' << 8) | 'I'));    // magic
			writer.Write ((ushort) 0x101);		       // version
			writer.Write ((ushort) images.Length);
			writer.Write ((ushort) images.Length);
			writer.Write ((ushort) (rows * cols));
			writer.Write ((ushort) 0x4);			// grow....not sure this should be hard coded
			writer.Write ((ushort) image_size.Width);
			writer.Write ((ushort) image_size.Height);
			writer.Write (BackColor.ToArgb ());
			writer.Write ((ushort) 0x1009);		       // flags

			for (int i = 0; i < 4; i++)
				writer.Write ((short) -1);  // ovls

			return RLEncodeData (stream.ToArray ());
		}

		// TODO: This is broken
		private byte [] RLEncodeData (byte [] data)
		{
			MemoryStream stream = new MemoryStream ();
			BinaryWriter writer = new BinaryWriter (stream);

			for (int i = 0; i < data.Length; i += 2) {
				int seq = 0;
				byte item  = data [i];
				while (data [i++] == item && i < data.Length)
					seq++;
				writer.Write ((byte) seq);
				writer.Write (item);
			}

			return stream.ToArray ();

		}

		internal Image [] Images {
			get { return images; }
		}

		internal Size ImageSize {
			get { return image_size; }
		}

		internal ColorDepth ColorDepth {
			get { return ColorDepth.Depth32Bit; }
		}

		internal Color BackColor {
			get { return back_color; }
		}
	}
}

