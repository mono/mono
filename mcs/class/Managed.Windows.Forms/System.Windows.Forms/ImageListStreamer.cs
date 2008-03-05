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
// Copyright (c) 2002-2006 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
		readonly ImageList.ImageCollection imageCollection;
		Image [] images;
		Size image_size;
		Color back_color;

		internal ImageListStreamer (ImageList.ImageCollection imageCollection)
		{
			this.imageCollection = imageCollection;
		}

		private ImageListStreamer (SerializationInfo info, StreamingContext context)
		{
			byte [] data = (byte []) info.GetValue ("Data", typeof (byte []));
			if (data == null || data.Length <= 4) { // 4 is the signature
				return;
			}

			// check the signature ( 'MSFt' )
			if (data [0] != 77 || data [1] != 83 || data [2] != 70 || data [3] != 116) {
				return;
			}

			MemoryStream decoded = GetDecodedStream (data, 4, data.Length - 4);
			decoded.Position = 4; // jumps over 'magic' and 'version', which are 16-bits each

			BinaryReader reader = new BinaryReader (decoded);
			ushort nimages = reader.ReadUInt16 ();
			reader.ReadUInt16 ();	// cMaxImage
			ushort grow = reader.ReadUInt16 (); // cGrow
			ushort cx = reader.ReadUInt16 ();
			ushort cy = reader.ReadUInt16 ();
			uint bkcolor = reader.ReadUInt32 ();
			back_color = Color.FromArgb ((int) bkcolor);
			reader.ReadUInt16 ();	// flags

			short [] ovls = new short [4];
			for (int i = 0; i < 4; i++) {
				ovls[i] = reader.ReadInt16 ();
			}

			byte [] decoded_buffer = decoded.GetBuffer ();
			int bmp_offset = 28;
			// FileSize field from the bitmap file header
			int filesize = decoded_buffer [bmp_offset + 2] + (decoded_buffer [bmp_offset + 3] << 8) +
					(decoded_buffer [bmp_offset + 4] << 16) + (decoded_buffer [bmp_offset + 5] << 24);
			// ImageSize field from the info header (can be 0)
			int imagesize = decoded_buffer [bmp_offset + 34] + (decoded_buffer [bmp_offset + 35] << 8) +
					(decoded_buffer [bmp_offset + 36] << 16) + (decoded_buffer [bmp_offset + 37] << 24);

			int bmp_length = imagesize + filesize;
			MemoryStream bmpms = new MemoryStream (decoded_buffer, bmp_offset, bmp_length);
			Bitmap bmp = null;
			Bitmap mask = null;
			bmp = new Bitmap (bmpms);
			MemoryStream mask_stream = new MemoryStream (decoded_buffer,
							bmp_offset + bmp_length,
							(int) (decoded.Length - bmp_offset - bmp_length));

			if (mask_stream.Length > 0)
				mask = new Bitmap (mask_stream);

			if (bkcolor == 0xFFFFFFFF)
				back_color = bmp.GetPixel (0, 0);

			if (mask != null) {
				int width = bmp.Width;
				int height = bmp.Height;
				Bitmap newbmp = new Bitmap (bmp);
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						Color mcolor = mask.GetPixel (x, y);
						if (mcolor.B != 0) {
							newbmp.SetPixel (x, y, Color.Transparent);
						}
					}
				}
				bmp.Dispose ();
				bmp = newbmp;
				mask.Dispose ();
				mask = null;
			}
			images = new Image [nimages];
			image_size = new Size (cx, cy);
			Rectangle dest_rect = new Rectangle (0, 0, cx, cy);
			if (grow * bmp.Width > cx) // Some images store a wrong 'grow' factor
				grow = (ushort) (bmp.Width / cx);

			for (int r = 0 ; r < nimages ; r++) {
				int col = r % grow;
				int row = r / grow;
				Rectangle area = new Rectangle (col * cx, row * cy, cx, cy);
				Bitmap b = new Bitmap (cx, cy);
				using (Graphics g = Graphics.FromImage (b)) {
					g.DrawImage (bmp, dest_rect, area, GraphicsUnit.Pixel);
				}

				images [r] = b;
			}
			bmp.Dispose ();
		}

		/*
		static void WriteToFile (MemoryStream st)
		{
			st.Position = 0;
			FileStream fs = File.OpenWrite (Path.GetTempFileName ());
			Console.WriteLine ("Writing to {0}", fs.Name);
			st.WriteTo (fs);
			fs.Close ();
		}
		*/

		static byte [] header = new byte []{ 77, 83, 70, 116, 73, 76, 3, 0 };
		public void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			MemoryStream stream = new MemoryStream ();
			BinaryWriter writer = new BinaryWriter (stream);
			writer.Write (header);

			Image [] images = (imageCollection != null) ? imageCollection.ToArray () : this.images;
			int cols = 4;
			int rows = images.Length / cols;
			if (images.Length % cols > 0)
				++rows;

			writer.Write ((ushort) images.Length);
			writer.Write ((ushort) images.Length);
			writer.Write ((ushort) 0x4);
			writer.Write ((ushort) (images [0].Width));
			writer.Write ((ushort) (images [0].Height));
			writer.Write (0xFFFFFFFF); //BackColor.ToArgb ()); //FIXME: should set the right one here.
			writer.Write ((ushort) 0x1009);
			for (int i = 0; i < 4; i++)
				writer.Write ((short) -1);

			Bitmap main = new Bitmap (cols * ImageSize.Width, rows * ImageSize.Height);
			using (Graphics g = Graphics.FromImage (main)) {
				g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), 0, 0,
						main.Width, main.Height);
				for (int i = 0; i < images.Length; i++) {
					g.DrawImage (images [i], (i % cols) * ImageSize.Width,
							(i / cols) * ImageSize.Height);
				}
			}

			MemoryStream tmp = new MemoryStream ();
			main.Save (tmp, ImageFormat.Bmp);
			tmp.WriteTo (stream);

			Bitmap mask = Get1bppMask (main);
			main.Dispose ();
			main = null;

			tmp = new MemoryStream ();
			mask.Save (tmp, ImageFormat.Bmp);
			tmp.WriteTo (stream);
			mask.Dispose ();

			stream = GetRLEStream (stream, 4);
			si.AddValue ("Data", stream.ToArray (), typeof (byte []));
		}

		unsafe Bitmap Get1bppMask (Bitmap main)
		{
			Rectangle rect = new Rectangle (0, 0, main.Width, main.Height);
			Bitmap result = new Bitmap (main.Width, main.Height, PixelFormat.Format1bppIndexed);
			BitmapData dresult = result.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);

			int w = images [0].Width;
			int h = images [0].Height;
			byte *scan = (byte *) dresult.Scan0.ToPointer ();
			int stride = dresult.Stride;
			Bitmap current = null;
			for (int idx = 0; idx < images.Length; idx++) {
				current = (Bitmap) images [idx];
				// Hack for newly added images.
				// Probably has to be done somewhere else.
				Color c1 = current.GetPixel (0, 0);
				if (c1.A != 0 && c1 == back_color)
					current.MakeTransparent (back_color);
				//
			}

			int yidx = 0;
			int imgidx = 0;
			int localy = 0;
			int localx = 0;
			int factor_y = 0;
			int factor_x = 0;
			for (int y = 0; y < main.Height; y++) {
				if (localy == h) {
					localy = 0;
					factor_y += 4;
				}
				factor_x = 0;
				localx = 0;
				for (int x = 0; x < main.Width; x++) {
					if (localx == w) {
						localx = 0;
						factor_x++;
					}
					imgidx = factor_y + factor_x;
					if (imgidx >= images.Length)
						break;
					current = (Bitmap) images [imgidx];
					Color color = current.GetPixel (localx, localy);
					if (color.A == 0) {
						int ptridx = yidx + (x >> 3);
						scan [ptridx] |= (byte) (0x80 >> (x & 7));
					}
					localx++;
				}
				if (imgidx >= images.Length)
					break;
				yidx += stride;
				localy++;
			}
			result.UnlockBits (dresult);

			return result;
		}

		static MemoryStream GetDecodedStream (byte [] bytes, int offset, int size)
		{
			byte [] buffer = new byte [512];
			int position = 0;
			int count, data;
			MemoryStream result = new MemoryStream ();
			while (size > 0) {
				count = (int) bytes [offset++];
				data = (int) bytes [offset++];
				if ((512 - count) < position) {
					result.Write (buffer, 0, position);
					position = 0;
				}

				for (int i = 0; i < count; i++)
					buffer [position++] = (byte) data;
				size -= 2;
			}

			if (position > 0)
				result.Write (buffer, 0, position);

			result.Position = 0;
			return result;
		}

		//TODO: OptimizeMe
		static MemoryStream GetRLEStream (MemoryStream input, int start)
		{
			MemoryStream result = new MemoryStream ();
			byte [] ibuffer = input.GetBuffer ();
			result.Write (ibuffer, 0, start);
			input.Position = start;

			int prev = -1;
			int count = 0;
			int current;
			while ((current = input.ReadByte ()) != -1) {
				if (prev != current || count == 255) {
					if (prev != -1) {
						result.WriteByte ((byte) count);
						result.WriteByte ((byte) prev);
					}
					prev = current;
					count = 0;
				}
				count++;
			}

			if (count > 0) {
				result.WriteByte ((byte) count);
				result.WriteByte ((byte) current);
			}

			return result;
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

