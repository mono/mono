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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// NOT COMPLETE

using System;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Windows.Forms {
	[Editor("System.Drawing.Design.CursorEditor, System.Drawing.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
	[Serializable]
	[TypeConverter(typeof(CursorConverter))]
	public sealed class Cursor : IDisposable, ISerializable {
		#region	Internal Structs
		[StructLayout(LayoutKind.Sequential)]
		private  struct CursorDir {
			internal ushort		idReserved;	// Reserved
			internal ushort		idType;		// resource type (2 for cursors)
			internal ushort		idCount;	// how many cursors
			internal CursorEntry[]	idEntries;	// the entries for each cursor
		};
		
		[StructLayout(LayoutKind.Sequential)]
		private  struct CursorEntry {
			internal byte		width;		// Width of cursor
			internal byte		height;		// Height of cursor
			internal byte		colorCount;	// colors in cursor
			internal byte		reserved;	// Reserved
			internal ushort		xHotspot;	// Hotspot X
			internal ushort		yHotspot;	// Hotspot Y
			internal ushort		bitCount;	// Bits per pixel
			internal uint		sizeInBytes;	// size of (CursorInfoHeader + ANDBitmap + ORBitmap)
			internal uint		fileOffset;	// position in file 
		}; 

		[StructLayout(LayoutKind.Sequential)]
		private  struct CursorInfoHeader {
			internal uint		biSize; 
			internal int		biWidth; 
			internal int		biHeight; 
			internal ushort		biPlanes; 
			internal ushort		biBitCount; 
			internal uint		biCompression; 
			internal uint		biSizeImage; 
			internal int		biXPelsPerMeter; 
			internal int		biYPelsPerMeter; 
			internal uint		biClrUsed; 
			internal uint		biClrImportant; 
		};

		[StructLayout(LayoutKind.Sequential)]
		private struct CursorImage {
			internal CursorInfoHeader	cursorHeader;	// image header
			internal uint[]			cursorColors;	// colors table
			internal byte[]			cursorXOR;	// bits for XOR mask
			internal byte[]			cursorAND;	// bits for AND mask
		};
		#endregion	// Internal structs

		#region Local Variables
		private static Cursor	current;
		private CursorDir	cursor_dir;
		private CursorImage[]	cursor_data;
		private int		id;

		internal IntPtr		handle;
		private Size		size;
		private Bitmap		shape;
		private Bitmap		mask;
		private Bitmap		cursor;
		internal string		name;
		#endregion	// Local Variables

		#region Public Constructors
		private void CreateCursor(System.IO.Stream stream) {
			InitFromStream(stream);
			this.shape = ToBitmap(true, false);
			this.mask = ToBitmap(false, false);
			handle = XplatUI.DefineCursor(shape, mask, Color.FromArgb(255, 255, 255), Color.FromArgb(255, 255, 255), cursor_dir.idEntries[id].xHotspot, cursor_dir.idEntries[id].yHotspot);
			this.shape.Dispose();
			this.shape = null;
			this.mask.Dispose();
			this.mask = null;

			if (handle != IntPtr.Zero) {
				this.cursor = ToBitmap(true, true);
			}
		}

		private Cursor() {
		}

		~Cursor() {
			Dispose();
		}

		// This is supposed to take a Win32 handle
		public Cursor(IntPtr handle) {
			this.handle = handle;
		}

		public Cursor(System.IO.Stream stream) {
			CreateCursor(stream);
		}

		public Cursor(string fileName) : this (new FileStream (fileName, FileMode.Open)) {
		}

		public Cursor(Type type, string resource) {
			using (Stream s = type.Assembly.GetManifestResourceStream (type, resource)) {
				if (s == null) {
					throw new FileNotFoundException ("Resource name was not found: `" + resource + "'");
				}
				CreateCursor(s);
			}
		}
		#endregion	// Public Constructors

		#region Public Static Properties
		public static Rectangle Clip {
			get {
				IntPtr		handle;
				bool		confined;
				Rectangle	rect;
				Size		size;

				XplatUI.GrabInfo(out handle, out confined, out rect);
				if (handle != IntPtr.Zero) {
					return rect;
				}

				XplatUI.GetDisplaySize(out size);
				rect.X = 0;
				rect.Y = 0;
				rect.Width = size.Width;
				rect.Height = size.Height;
				return rect;
			}

			[MonoTODO("First need to add ability to set cursor clip rectangle to XplatUI drivers to implement this property")]
			set {
				;
			}
		}

		public static Cursor Current {
			get {
				return current;
			}

			set {
				if (current != value) {
					current = value;
					XplatUI.OverrideCursor(current.handle);
				}
			}
		}

		public static Point Position {
			get {
				int x;
				int y;

				XplatUI.GetCursorPos (IntPtr.Zero, out x, out y);
				return new Point (x, y);
			}

			set {
				XplatUI.SetCursorPos(IntPtr.Zero, value.X, value.Y);
			}
		}
		#endregion	// Public Static Properties

		#region Public Instance Properties
		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public Size Size {
			get {
				return size;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Static Methods
		public static void Hide() {
			XplatUI.ShowCursor(false);
		}

		public static void Show() {
			XplatUI.ShowCursor(false);
		}

		public static bool operator !=(Cursor left, Cursor right) {
			if ((object)left == (object)right) {
				return false;
			}

			if ((object)left == null || (object)right == null) {
				return true;
			}

			if (left.handle == right.handle) {
				return false;
			}
			return true;
		}


		public static bool operator ==(Cursor left, Cursor right) {
			if ((object)left == (object)right) {
				return true;
			}

			if ((object)left == null || (object)right == null) {
				return false;
			}

			if (left.handle == right.handle) {
				return true;
			}
			return false;
		}
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public IntPtr CopyHandle() {
			return handle;
		}

		public void Dispose() {
			if (this.cursor != null) {
				this.cursor.Dispose();
				this.cursor = null;
			}

			if (this.shape != null) {
				this.shape.Dispose();
				this.shape = null;
			}

			if (this.mask != null) {
				this.mask.Dispose();
				this.mask = null;
			}
		}

		public void Draw(Graphics g, Rectangle targetRect) {
			if (this.cursor != null) {
				g.DrawImage(this.cursor, targetRect);
			}
		}

		public void DrawStretched(Graphics g, Rectangle targetRect) {
			if (this.cursor != null) {
				g.DrawImage(this.cursor, targetRect, new Rectangle(0, 0, this.cursor.Width, this.cursor.Height), GraphicsUnit.Pixel);
			}
		}

		public override bool Equals(object obj) {
			if ( !(obj is Cursor)) {
				return false;
			}

			if (((Cursor)obj).handle == this.handle) {
				return true;
			}

			return false;
		}

		public override int GetHashCode() {
			return base.GetHashCode ();
		}

		public override string ToString() {
			if (name != null) {
				return "[Cursor:" + name + "]";
			}

			throw new FormatException("Cannot convert custom cursors to string.");
		}

		void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context) {
			MemoryStream	ms;
			BinaryWriter	wr;
			CursorImage	ci;

			ms = new MemoryStream();
			wr = new BinaryWriter(ms);
			ci = cursor_data[this.id];

			// Build the headers, first the CursorDir
			wr.Write((ushort)0);	// Reserved
			wr.Write((ushort)2);	// Resource type
			wr.Write((ushort)1);	// Count

			// Next the CursorEntry
			wr.Write((byte)cursor_dir.idEntries[this.id].width);
			wr.Write((byte)cursor_dir.idEntries[this.id].height);
			wr.Write((byte)cursor_dir.idEntries[this.id].colorCount);
			wr.Write((byte)cursor_dir.idEntries[this.id].reserved);
			wr.Write((ushort)cursor_dir.idEntries[this.id].xHotspot);
			wr.Write((ushort)cursor_dir.idEntries[this.id].yHotspot);
			wr.Write((uint)(40 + (ci.cursorColors.Length * 4) + ci.cursorXOR.Length + ci.cursorAND.Length));
			wr.Write((uint)(6 + 16));	// CursorDir + CursorEntry size

			// Then the CursorInfoHeader
			wr.Write(ci.cursorHeader.biSize);
			wr.Write(ci.cursorHeader.biWidth);
			wr.Write(ci.cursorHeader.biHeight);
			wr.Write(ci.cursorHeader.biPlanes);
			wr.Write(ci.cursorHeader.biBitCount);
			wr.Write(ci.cursorHeader.biCompression);
			wr.Write(ci.cursorHeader.biSizeImage);
			wr.Write(ci.cursorHeader.biXPelsPerMeter);
			wr.Write(ci.cursorHeader.biYPelsPerMeter);
			wr.Write(ci.cursorHeader.biClrUsed);
			wr.Write(ci.cursorHeader.biClrImportant);
			for (int i = 0; i < ci.cursorColors.Length; i++) {
				wr.Write(ci.cursorColors[i]);
			}
			wr.Write(ci.cursorXOR);
			wr.Write(ci.cursorAND);
			wr.Flush();

			si.AddValue ("CursorData", ms.ToArray());
		}
		#endregion	// Public Instance Methods

		#region Private Methods		  w
		private void InitFromStream(Stream stream) {
			ushort		entry_count;
			CursorEntry	ce;
			uint		largest;

			//read the cursor header
			if (stream == null || stream.Length == 0) {
				throw new System.ArgumentException ("The argument 'stream' must be a picture that can be used as a cursor", "stream");
			}
			
			BinaryReader reader = new BinaryReader (stream);
            
			cursor_dir = new CursorDir ();
			cursor_dir.idReserved = reader.ReadUInt16();
			if (cursor_dir.idReserved != 0) {
				throw new System.ArgumentException ("Invalid Argument", "stream");
			}
			
			cursor_dir.idType = reader.ReadUInt16();
			if (cursor_dir.idType != 2) { //must be 2
				throw new System.ArgumentException ("Invalid Argument", "stream");
			}

			entry_count = reader.ReadUInt16();
			cursor_dir.idCount = entry_count;
			cursor_dir.idEntries = new CursorEntry[entry_count];
			cursor_data = new CursorImage[entry_count];

			//now read in the CursorEntry structures
			for (int i=0; i < entry_count; i++){
				ce = new CursorEntry();

				ce.width = reader.ReadByte();
				ce.height = reader.ReadByte();
				ce.colorCount = reader.ReadByte();
				ce.reserved = reader.ReadByte();
				ce.xHotspot = reader.ReadUInt16();
				ce.yHotspot = reader.ReadUInt16();
				ce.sizeInBytes = reader.ReadUInt32();
				ce.fileOffset = reader.ReadUInt32();

				cursor_dir.idEntries[i] = ce;
			}

			// If we have more than one pick the largest cursor
			largest = 0;
			for (int j=0; j < entry_count; j++){
				if (cursor_dir.idEntries[j].sizeInBytes >= largest)	{
					largest = cursor_dir.idEntries[j].sizeInBytes;
					this.id = (ushort)j;
					this.size.Height = cursor_dir.idEntries[j].height;
					this.size.Width = cursor_dir.idEntries[j].width;
				}
			}

			//now read in the cursor data
			for (int j = 0; j < entry_count; j++) {
				CursorImage		curdata;
				CursorInfoHeader	cih;
				byte[]			buffer;
				BinaryReader		cih_reader;
				int			num_colors;
				int			cursor_height;
				int			bytes_per_line;
				int			xor_size;
				int			and_size;

				curdata = new CursorImage();
				cih = new CursorInfoHeader();
				
				stream.Seek (cursor_dir.idEntries[j].fileOffset, SeekOrigin.Begin);
				buffer = new byte [cursor_dir.idEntries[j].sizeInBytes];
				stream.Read (buffer, 0, buffer.Length);

				cih_reader = new BinaryReader(new MemoryStream(buffer));

				cih.biSize = cih_reader.ReadUInt32 ();
				if (cih.biSize != 40) {
					throw new System.ArgumentException ("Invalid cursor file", "stream");
				}
				cih.biWidth = cih_reader.ReadInt32 ();
				cih.biHeight = cih_reader.ReadInt32 ();
				cih.biPlanes = cih_reader.ReadUInt16 ();
				cih.biBitCount = cih_reader.ReadUInt16 ();
				cih.biCompression = cih_reader.ReadUInt32 ();
				cih.biSizeImage = cih_reader.ReadUInt32 ();
				cih.biXPelsPerMeter = cih_reader.ReadInt32 ();
				cih.biYPelsPerMeter = cih_reader.ReadInt32 ();
				cih.biClrUsed = cih_reader.ReadUInt32 ();
				cih.biClrImportant = cih_reader.ReadUInt32 ();

				curdata.cursorHeader = cih;

				//Read the number of colors used and corresponding memory occupied by
				//color table. Fill this memory chunk into rgbquad[]
				switch (cih.biBitCount){
					case 1: num_colors = 2; break;
					case 4: num_colors = 16; break;
					case 8: num_colors = 256; break;
					default: num_colors = 0; break;
				}
				
				curdata.cursorColors = new uint[num_colors];
				for (int i = 0; i < num_colors; i++) {
					curdata.cursorColors[i] = cih_reader.ReadUInt32 ();
				}

				//XOR mask is immediately after ColorTable and its size is 
				//icon height* no. of bytes per line
				
				//cursor height is half of BITMAPINFOHEADER.biHeight, since it contains
				//both XOR as well as AND mask bytes
				cursor_height = cih.biHeight/2;
				
				//bytes per line should should be uint aligned
				bytes_per_line = ((((cih.biWidth * cih.biPlanes * cih.biBitCount)+ 31)>>5)<<2);
				
				//Determine the XOR array Size
				xor_size = bytes_per_line * cursor_height;
				curdata.cursorXOR = new byte[xor_size];
				for (int i = 0; i < xor_size; i++) {
					curdata.cursorXOR[i] = cih_reader.ReadByte();
				}
				
				//Determine the AND array size
				and_size = (int)(cih_reader.BaseStream.Length - cih_reader.BaseStream.Position);
				curdata.cursorAND = new byte[and_size];
				for (int i = 0; i < and_size; i++) {
					curdata.cursorAND[i] = cih_reader.ReadByte();
				}
				
				cursor_data[j] = curdata;
				cih_reader.Close();
			}			

			reader.Close();
		}

		private Bitmap ToBitmap (bool xor, bool transparent) {
			Bitmap bmp;

			if (cursor_data != null) {
				MemoryStream		stream;
				BinaryWriter		writer;
				CursorImage		ci;
				uint			offset;
				uint			filesize;
				ushort			reserved12;
				CursorInfoHeader	cih;
				int			color_count;

				stream = new MemoryStream();
				writer = new BinaryWriter (stream);

				ci = cursor_data[this.id];

				try {
					// write bitmap file header
					writer.Write ('B');
					writer.Write ('M');

					// write the file size
					// file size = bitmapfileheader + bitmapinfo + colorpalette + image bits
					// sizeof bitmapfileheader = 14 bytes
					// sizeof bitmapinfo = 40 bytes
					if (xor) {
						offset = (uint)(14 + 40 + ci.cursorColors.Length * 4);
						filesize = (uint)(offset + ci.cursorXOR.Length);
					} else {
						offset = (uint)(14 + 40 + 8);	// AND mask is always monochrome
						filesize = (uint)(offset + ci.cursorAND.Length);
					}
					writer.Write(filesize);
					
					// write reserved words
					reserved12 = 0;
					writer.Write(reserved12);
					writer.Write(reserved12);

					// write offset
					writer.Write (offset);

					// write bitmapfile header
					cih = ci.cursorHeader;
					writer.Write(cih.biSize);
					writer.Write(cih.biWidth);
					writer.Write(cih.biHeight/2);
					writer.Write(cih.biPlanes);
					if (xor) {
						writer.Write(cih.biBitCount);
					} else {
						writer.Write((ushort)1);
					}
					writer.Write(cih.biCompression);
					if (xor) {
						writer.Write(ci.cursorXOR.Length);
					} else {
						writer.Write(ci.cursorAND.Length);
					}
					writer.Write(cih.biXPelsPerMeter);
					writer.Write(cih.biYPelsPerMeter);
					writer.Write(cih.biClrUsed);
					writer.Write(cih.biClrImportant);

					// write color table
					if (xor) {
						color_count = ci.cursorColors.Length;
						for (int j = 0; j < color_count; j++) {
							writer.Write (ci.cursorColors[j]);
						}
					} else {
						writer.Write((uint)0x00000000);
						writer.Write((uint)0x00ffffff);
					}

					// write image bits
					if (xor) {
						writer.Write(ci.cursorXOR);
					} else {
						writer.Write(ci.cursorAND);
					}
					writer.Flush();

					// create bitmap from stream and return
					bmp = new Bitmap(stream);

					if (transparent) {
						bmp = new Bitmap(bmp);	// This makes a 32bpp image out of an indexed one
						// Apply the mask to make properly transparent
						for (int y = 0; y < cih.biHeight/2; y++) {
							for (int x = 0; x < cih.biWidth / 8; x++) {
								for (int bit = 7; bit >= 0; bit--) {
									if (((ci.cursorAND[y * cih.biWidth / 8 +x] >> bit) & 1) != 0) {
										bmp.SetPixel(x*8 + 7-bit, cih.biHeight/2 - y - 1, Color.Transparent);
									}
								}
							}
						}
					}
				} catch (Exception e) {
					throw e;
				} finally {
					writer.Close();
				}
			} else {
				bmp = new Bitmap (32, 32);
			}

			return bmp;
		}

		#endregion	// Private Methods
	}
}
