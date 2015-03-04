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
// Copyright (c) 2004-2010 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Reflection;

namespace System.Windows.Forms {
	[Editor("System.Drawing.Design.CursorEditor, " + Consts.AssemblySystem_Drawing_Design, typeof(System.Drawing.Design.UITypeEditor))]
	[Serializable]
	[TypeConverter(typeof(CursorConverter))]
	public sealed class Cursor : IDisposable, ISerializable {
		#region	Internal Structs
		[StructLayout (LayoutKind.Sequential)]
		private  struct CursorDir {
			internal ushort		idReserved;	// Reserved
			internal ushort		idType;		// resource type (2 for cursors)
			internal ushort		idCount;	// how many cursors
			internal CursorEntry[]	idEntries;	// the entries for each cursor
		};
		
		[StructLayout (LayoutKind.Sequential)]
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
		private StdCursor	std_cursor = (StdCursor) (-1);

		private object tag;

		#endregion	// Local Variables

		#region Public Constructors
		private void CreateCursor (Stream stream)
		{
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
	
		internal Cursor (StdCursor cursor) : this (XplatUI.DefineStdCursor (cursor))
		{
			std_cursor = cursor;
		}
		
		private Cursor(SerializationInfo info, StreamingContext context)
		{
		}

		private Cursor()
		{
		}

		~Cursor()
		{
			Dispose();
		}

		// This is supposed to take a Win32 handle
		public Cursor (IntPtr handle) 
		{
			this.handle = handle;
		}

		public Cursor (Stream stream)
		{
			CreateCursor(stream);
		}

		public Cursor (string fileName)
		{
			using (FileStream fs = File.OpenRead (fileName)) {
				CreateCursor (fs);
			}
		}

		public Cursor(Type type, string resource) {
			using (Stream s = type.Assembly.GetManifestResourceStream (type, resource)) {
				if (s != null) {
					CreateCursor (s);
					return;
				}
			}

			// Try a different way, previous failed
			using (Stream s = Assembly.GetExecutingAssembly ().GetManifestResourceStream (resource)) {
				if (s != null) {
					CreateCursor (s);
					return;
				}
			}
			throw new FileNotFoundException ("Resource name was not found: `" + resource + "'");
		}
		#endregion	// Public Constructors

		#region Public Static Properties
		public static Rectangle Clip {
			get {
				IntPtr		handle;
				bool		confined;
				Rectangle	rect;
				Size		size;

				XplatUI.GrabInfo (out handle, out confined, out rect);
				if (handle != IntPtr.Zero) {
					return rect;
				}

				XplatUI.GetDisplaySize (out size);
				rect.X = 0;
				rect.Y = 0;
				rect.Width = size.Width;
				rect.Height = size.Height;
				return rect;
			}

			[MonoTODO ("Stub, does nothing")]
			[MonoInternalNote ("First need to add ability to set cursor clip rectangle to XplatUI drivers to implement this property")]
			set {
				;
			}
		}

		public static Cursor Current {
			get {
				if (current != null) 
					return current;
				return Cursors.Default;
			}

			set {
				if (current == value)
					return;
				
				current = value;
				if (current == null){
					// FIXME - define and set empty cursor
					XplatUI.OverrideCursor(IntPtr.Zero);
				} else
					XplatUI.OverrideCursor(current.handle);
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

		[MonoTODO ("Implemented for Win32, X11 always returns 0,0")]
		public Point HotSpot {
			get {
				int cursor_w, cursor_h, hot_x, hot_y;
				XplatUI.GetCursorInfo (Handle, out cursor_w, out cursor_h, out hot_x, out hot_y);

				return new Point (hot_x, hot_y);
			}
		}

		public Size Size {
			get {
				return size;
			}
		}
		
		[Localizable (false)]
		[Bindable (true)]
		[TypeConverter (typeof (StringConverter))]
		[DefaultValue (null)]
		[MWFCategory ("Data")]
		public object Tag {
			get { return this.tag; }
			set { this.tag = value; }
		}

		#endregion	// Public Instance Properties

		#region Public Static Methods
		public static void Hide ()
		{
			XplatUI.ShowCursor(false);
		}

		public static void Show ()
		{
			XplatUI.ShowCursor(true);
		}

		public static bool operator != (Cursor left, Cursor right) {
			if ((object)left == (object)right)
				return false;

			if ((object)left == null || (object)right == null) 
				return true;

			if (left.handle == right.handle) 
				return false;
			return true;
		}


		public static bool operator ==(Cursor left, Cursor right)
		{
			if ((object)left == (object)right) 
				return true;

			if ((object)left == null || (object)right == null)
				return false;

			if (left.handle == right.handle)
				return true;

			return false;
		}
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public IntPtr CopyHandle() {
			return handle;
		}

		public void Dispose ()
		{
			if (cursor != null) {
				cursor.Dispose ();
				cursor = null;
			}

			if (shape != null) {
				shape.Dispose ();
				shape = null;
			}

			if (mask != null) {
				mask.Dispose ();
				mask = null;
			}

			GC.SuppressFinalize (this);
		}

		public void Draw (Graphics g, Rectangle targetRect)
		{
			if (cursor == null && std_cursor != (StdCursor) (-1)) 
				cursor = XplatUI.DefineStdCursorBitmap (std_cursor);

			if (cursor != null) {
				// Size of the targetRect is not considered at all
				g.DrawImage (cursor, targetRect.X, targetRect.Y);
			}
		}

		public void DrawStretched (Graphics g, Rectangle targetRect)
		{
			if (cursor == null && std_cursor != (StdCursor)(-1)) 
				cursor = XplatUI.DefineStdCursorBitmap (std_cursor);

			if (cursor != null) {
				g.DrawImage (cursor, targetRect, new Rectangle(0, 0, cursor.Width, cursor.Height), GraphicsUnit.Pixel);
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is Cursor)) 
				return false;

			if (((Cursor)obj).handle == handle)
				return true;

			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		public override string ToString()
		{
			if (name != null) {
				return "[Cursor:" + name + "]";
			}

			throw new FormatException("Cannot convert custom cursors to string.");
		}

		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext context)
		{
			MemoryStream	ms;
			BinaryWriter	wr;
			CursorImage	ci;

			ms = new MemoryStream ();
			wr = new BinaryWriter (ms);
			ci = cursor_data [this.id];

			// Build the headers, first the CursorDir
			wr.Write ((ushort) 0);	// Reserved
			wr.Write ((ushort) 2);	// Resource type
			wr.Write ((ushort) 1);	// Count

			// Next the CursorEntry
			wr.Write ((byte)cursor_dir.idEntries [this.id].width);
			wr.Write ((byte)cursor_dir.idEntries [this.id].height);
			wr.Write ((byte)cursor_dir.idEntries [this.id].colorCount);
			wr.Write ((byte)cursor_dir.idEntries [this.id].reserved);
			wr.Write ((ushort)cursor_dir.idEntries [this.id].xHotspot);
			wr.Write ((ushort)cursor_dir.idEntries [this.id].yHotspot);
			wr.Write ((uint)(40 + (ci.cursorColors.Length * 4) + ci.cursorXOR.Length + ci.cursorAND.Length));
			wr.Write ((uint)(6 + 16));	// CursorDir + CursorEntry size

			// Then the CursorInfoHeader
			wr.Write (ci.cursorHeader.biSize);
			wr.Write (ci.cursorHeader.biWidth);
			wr.Write (ci.cursorHeader.biHeight);
			wr.Write (ci.cursorHeader.biPlanes);
			wr.Write (ci.cursorHeader.biBitCount);
			wr.Write (ci.cursorHeader.biCompression);
			wr.Write (ci.cursorHeader.biSizeImage);
			wr.Write (ci.cursorHeader.biXPelsPerMeter);
			wr.Write (ci.cursorHeader.biYPelsPerMeter);
			wr.Write (ci.cursorHeader.biClrUsed);
			wr.Write (ci.cursorHeader.biClrImportant);
			
			for (int i = 0; i < ci.cursorColors.Length; i++) 
				wr.Write(ci.cursorColors[i]);

			wr.Write (ci.cursorXOR);
			wr.Write (ci.cursorAND);
			wr.Flush ();

			si.AddValue ("CursorData", ms.ToArray ());
		}
		#endregion	// Public Instance Methods

		#region Private Methods
		private void InitFromStream (Stream stream)
		{
			ushort		entry_count;
			CursorEntry	ce;
			uint		largest;

			//read the cursor header
			if (stream == null || stream.Length == 0) 
				throw new ArgumentException ("The argument 'stream' must be a picture that can be used as a cursor", "stream");
			
			BinaryReader reader = new BinaryReader (stream);
            
			cursor_dir = new CursorDir ();
			cursor_dir.idReserved = reader.ReadUInt16();
			cursor_dir.idType = reader.ReadUInt16();
			if (cursor_dir.idReserved != 0 || !(cursor_dir.idType == 2 || cursor_dir.idType == 1))
				throw new ArgumentException ("Invalid Argument, format error", "stream");

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
				if (cursor_dir.idType == 1) {
					ce.xHotspot = (ushort)(ce.width / 2);
					ce.yHotspot = (ushort)(ce.height / 2);
				}
				ce.sizeInBytes = reader.ReadUInt32();
				ce.fileOffset = reader.ReadUInt32();

				cursor_dir.idEntries[i] = ce;
			}

			// If we have more than one pick the largest cursor
			largest = 0;
			for (int j = 0; j < entry_count; j++){
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
					throw new ArgumentException ("Invalid cursor file", "stream");
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

		private Bitmap ToBitmap(bool xor, bool transparent)
		{
			CursorImage		ci;
			CursorInfoHeader	cih;
			int			ncolors;
			Bitmap			bmp;
			BitmapData		bits;
			ColorPalette		pal;
			int			biHeight;
			int			bytesPerLine;

			if (cursor_data == null)
				return new Bitmap(32, 32);

			ci = cursor_data[this.id];
			cih = ci.cursorHeader;
			biHeight = cih.biHeight / 2;

			if (!xor) {
				// The AND mask is 1bit - very straightforward
				bmp = new Bitmap(cih.biWidth, biHeight, PixelFormat.Format1bppIndexed);
				pal = bmp.Palette;
				pal.Entries[0] = Color.FromArgb(0, 0, 0);
				pal.Entries[1] = Color.FromArgb(unchecked((int)0xffffffffff));
				bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

				for (int y = 0; y < biHeight; y++) {
					Marshal.Copy(ci.cursorAND, bits.Stride * y, (IntPtr)(bits.Scan0.ToInt64() + bits.Stride * (biHeight - 1 - y)), bits.Stride);
				}

				bmp.UnlockBits(bits);
			} else {
				ncolors = (int)cih.biClrUsed;
				if (ncolors == 0) {
					if (cih.biBitCount < 24) {
						ncolors = (int)(1 << cih.biBitCount);
					}
				}

				switch(cih.biBitCount) {
				case 1: {	// Monochrome
					bmp = new Bitmap (cih.biWidth, biHeight, PixelFormat.Format1bppIndexed);
					break;
				}
					
				case 4: {	// 4bpp
					bmp = new Bitmap (cih.biWidth, biHeight, PixelFormat.Format4bppIndexed);
					break;
				}
					
				case 8: {	// 8bpp
					bmp = new Bitmap (cih.biWidth, biHeight, PixelFormat.Format8bppIndexed);
					break;
				}
					
				case 24:
				case 32: {	// 32bpp
					bmp = new Bitmap (cih.biWidth, biHeight, PixelFormat.Format32bppArgb);
					break;
				}
					
				default: 
					throw new Exception("Unexpected number of bits:" + cih.biBitCount.ToString());
				}
				
				if (cih.biBitCount < 24) {
					pal = bmp.Palette;				// Managed palette
					for (int i = 0; i < ci.cursorColors.Length; i++) 
						pal.Entries[i] = Color.FromArgb((int)ci.cursorColors[i] | unchecked((int)0xff000000));
					bmp.Palette = pal;
				}

				bytesPerLine = (int)((((cih.biWidth * cih.biBitCount) + 31) & ~31) >> 3);
				bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

				for (int y = 0; y < biHeight; y++) 
					Marshal.Copy(ci.cursorXOR, bytesPerLine * y, (IntPtr)(bits.Scan0.ToInt64() + bits.Stride * (biHeight - 1 - y)), bytesPerLine);
				
				bmp.UnlockBits(bits);
			}

			if (transparent) {
				bmp = new Bitmap(bmp);	// This makes a 32bpp image out of an indexed one
				// Apply the mask to make properly transparent
				for (int y = 0; y < biHeight; y++) {
					for (int x = 0; x < cih.biWidth / 8; x++) {
						for (int bit = 7; bit >= 0; bit--) {
							if (((ci.cursorAND[y * cih.biWidth / 8 +x] >> bit) & 1) != 0) 
								bmp.SetPixel(x*8 + 7-bit, biHeight - y - 1, Color.Transparent);
						}
					}
				}
			}

			return bmp;
		}
		#endregion	// Private Methods
	}
}
