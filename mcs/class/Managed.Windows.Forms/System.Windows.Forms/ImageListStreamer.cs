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

		static int GetInt (Stream st)
		{
			byte [] bytes = new byte [4];
			st.Read (bytes, 0, 4);
			return (bytes [0] + (bytes [1] << 8) + (bytes [2] << 16) + (bytes [3] << 24));
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
			//WriteToFile (new MemoryStream (decoded.GetBuffer (), 0, (int) decoded.Length));
			decoded.Position = 4; // jumps over 'magic' and 'version', which are 16-bits each

			BinaryReader reader = new BinaryReader (decoded);
			ushort nimages = reader.ReadUInt16 ();
			ushort max_image = reader.ReadUInt16 ();	// cMaxImage
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

			decoded.Position = 28;
			if (decoded.ReadByte () != 'B') {
				return;
				/*
				Console.WriteLine ("FALLO en 1 {0}", decoded.Position - 1);
				WriteToFile (new MemoryStream (data));
				WriteToFile (decoded);
				throw new Exception ();
				*/
			}

			if (decoded.ReadByte () != 'M') {
				return;
				/*
				Console.WriteLine ("FALLO en 2 {0}", decoded.Position - 2);
				WriteToFile (new MemoryStream (data));
				WriteToFile (decoded);
				throw new Exception ();
				*/
			}
	
			int bmp_offset = 28;
			int bmp_size = GetInt (decoded);
			MemoryStream bmpms = new MemoryStream (decoded.GetBuffer (), bmp_offset, (int) decoded.Length - bmp_offset + 1);
			Bitmap bmp = null;
			try {
				bmp = new Bitmap (bmpms);
			} catch (Exception e) {
				throw;
				/*
				WriteToFile (new MemoryStream (data));
				WriteToFile (decoded);
				MemoryStream kk = new MemoryStream (decoded.GetBuffer (), bmp_offset, (int) decoded.Length - bmp_offset + 1);
				WriteToFile (kk);
				throw;
				*/
			}

			// the mask is set to the color at pixel 0, 0
			//TODO: do not keep back_color when got from pixel (0,0)?
			// do this per image?
			if (bkcolor == 0xFFFFFFFF) 
				back_color = bmp.GetPixel (0, 0);


			images = new Image [nimages];
			int w = cx;
			int h = cy;
			image_size = new Size (cx, cy);
			Rectangle dest_rect = new Rectangle (0, 0, w, h);
			for (int r = 0 ; r < nimages ; r++) {
				int col = r % grow;
				int row = r / grow;
				Rectangle area = new Rectangle (col * w, row * h, w, h);
				Bitmap b = new Bitmap (w, h);
				using (Graphics g = Graphics.FromImage (b)) {
					g.DrawImage (bmp, dest_rect, area, GraphicsUnit.Pixel);
				}
				b.MakeTransparent (back_color);
				images [r] = b;
			}
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
		public void GetObjectData (SerializationInfo info, StreamingContext context)
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
				g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), 0, 0, cols * ImageSize.Width, rows * ImageSize.Height);
				for (int i = 0; i < images.Length; i++) {
					g.DrawImage (images [i], (i % cols) * ImageSize.Width,
							(i / cols) * ImageSize.Height);
				}
			}

			MemoryStream tmp = new MemoryStream ();
			main.Save (tmp, ImageFormat.Bmp);
			tmp.WriteTo (stream);

			stream = GetRLEStream (stream, 4);
			info.AddValue ("Data", stream.ToArray (), typeof (byte []));
		}

		static MemoryStream GetDecodedStream (byte [] bytes, int offset, int size)
		{
			byte [] buffer = new byte [512];
			int position = 0;
			int count, data;
			MemoryStream result = new MemoryStream ();
			int stop = offset + size;
			while (offset + 1 < stop) {
				count = (int) bytes [offset++];
				data = (int) bytes [offset++];
				if ((512 - count) < position) {
					result.Write (buffer, 0, position);
					position = 0;
				}

				for (int i = 0; i < count; i++)
					buffer [position++] = (byte) data;
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

			if (current != -1) {
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

