//
// System.Drawing.Icon.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2002 Ximian, Inc
//
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing
{

	[Serializable]
	[ComVisible (false)]
	[Editor ("System.Drawing.Design.IconEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	[TypeConverter(typeof(IconConverter))]
	public sealed class Icon : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct IconDirEntry {
			internal byte	width;				// Width of icon
			internal byte	height;				// Height of icon
			internal byte	colorCount;			// colors in icon 
			internal byte	reserved;			// Reserved
			internal ushort planes;             // Color Planes
			internal ushort	bitCount;           // Bits per pixel
			internal uint	bytesInRes;         // bytes in resource
			internal uint	imageOffset;        // position in file 
		}; 

		[StructLayout(LayoutKind.Sequential)]
		internal struct IconDir {
			internal ushort			idReserved;   // Reserved
			internal ushort			idType;       // resource type (1 for icons)
			internal ushort			idCount;      // how many images?
			internal IconDirEntry []	idEntries;	  // the entries for each image
		};
		
		[StructLayout(LayoutKind.Sequential)]
		internal struct BitmapInfoHeader {
            internal uint	biSize; 
			internal int	biWidth; 
			internal int	biHeight; 
			internal ushort	biPlanes; 
			internal ushort	biBitCount; 
			internal uint	biCompression; 
			internal uint	biSizeImage; 
			internal int	biXPelsPerMeter; 
			internal int	biYPelsPerMeter; 
			internal uint	biClrUsed; 
			internal uint	biClrImportant; 
		};

		[StructLayout(LayoutKind.Sequential)]
		internal struct IconImage {
			internal BitmapInfoHeader	iconHeader;	//image header
			internal uint []			iconColors;	//colors table
			internal byte []			iconXOR;	// bits for XOR mask
			internal byte []			iconAND;	//bits for AND mask
		};	

		Size iconSize;
		IntPtr winHandle = IntPtr.Zero;
		IconDir	iconDir;
		ushort id;
		IconImage [] imageData;
			
		private Icon ()
		{
		}
		
		public Icon (Icon original, int width, int height) : this (original, new Size(width, height))
		{			
		}

		[MonoTODO ("Implement")]
		public Icon (Icon original, Size size)
		{
			iconSize = size;
			throw new NotImplementedException ();
		}

		public Icon (Stream stream) : this (stream, 32, 32) 
		{
		}

		public Icon (Stream stream, int width, int height)
		{
			//read the icon header
			if (stream == null || stream.Length == 0)
				throw new System.ArgumentException ("The argument 'stream' must be a picture that can be used as a Icon", "stream");
			
			BinaryReader reader = new BinaryReader (stream);
            			
			iconDir.idReserved = reader.ReadUInt16();
			if (iconDir.idReserved != 0) //must be 0
				throw new System.ArgumentException ("Invalid Argument", "stream");
			
			iconDir.idType = reader.ReadUInt16();
			if (iconDir.idType != 1) //must be 1
				throw new System.ArgumentException ("Invalid Argument", "stream");

			ushort dirEntryCount = reader.ReadUInt16();
			iconDir.idCount = dirEntryCount;
			iconDir.idEntries = new IconDirEntry [dirEntryCount];
			imageData = new IconImage [dirEntryCount];
			bool sizeObtained = false;
			//now read in the IconDirEntry structures
			for (int i=0; i<dirEntryCount; i++){
				IconDirEntry ide;
				ide.width = reader.ReadByte ();
				ide.height = reader.ReadByte ();
				ide.colorCount = reader.ReadByte ();
				ide.reserved = reader.ReadByte ();
				ide.planes = reader.ReadUInt16 ();
				ide.bitCount = reader.ReadUInt16 ();
				ide.bytesInRes = reader.ReadUInt32 ();
				ide.imageOffset = reader.ReadUInt32 ();
				iconDir.idEntries [i] = ide;
				//is this is the best fit??
				if (!sizeObtained)   
					if (ide.height>=height && ide.width>=width) {
						this.id = (ushort) i;
						sizeObtained = true;
						this.iconSize.Height = ide.height;
						this.iconSize.Width = ide.width;
					}
			}
			//if we havent found the best match, return the one with the
			//largest size. Is this approach correct??
			if (!sizeObtained){
				uint largestSize = 0;
				for (int j=0; j<dirEntryCount; j++){
					if (iconDir.idEntries [j].bytesInRes >= largestSize)	{
						largestSize = iconDir.idEntries [j].bytesInRes;
						this.id = (ushort) j;
						this.iconSize.Height = iconDir.idEntries [j].height;
						this.iconSize.Width = iconDir.idEntries [j].width;
					}
				}
			}
			
			//now read in the icon data
			for (int j = 0; j<dirEntryCount; j++)
			{
				IconImage iidata = new IconImage();
				BitmapInfoHeader bih = new BitmapInfoHeader();
				stream.Seek (iconDir.idEntries [j].imageOffset, SeekOrigin.Begin);
				byte [] buffer = new byte [iconDir.idEntries [j].bytesInRes];
				stream.Read (buffer, 0, buffer.Length);
				BinaryReader bihReader = new BinaryReader (new MemoryStream(buffer));
				bih.biSize = bihReader.ReadUInt32 ();
				bih.biWidth = bihReader.ReadInt32 ();
				bih.biHeight = bihReader.ReadInt32 ();
				bih.biPlanes = bihReader.ReadUInt16 ();
				bih.biBitCount = bihReader.ReadUInt16 ();
				bih.biCompression = bihReader.ReadUInt32 ();
				bih.biSizeImage = bihReader.ReadUInt32 ();
				bih.biXPelsPerMeter = bihReader.ReadInt32 ();
				bih.biYPelsPerMeter = bihReader.ReadInt32 ();
				bih.biClrUsed = bihReader.ReadUInt32 ();
				bih.biClrImportant = bihReader.ReadUInt32 ();

				iidata.iconHeader = bih;

				//Read the number of colors used and corresponding memory occupied by
				//color table. Fill this memory chunk into rgbquad[]
				int numColors;
				switch (bih.biBitCount){
					case 1: numColors = 2;
						break;
					case 4: numColors = 16;
						break;
					case 8: numColors = 256;
						break;
					default: numColors = 0;
						break;
				}
				
				
				iidata.iconColors = new uint [numColors];
				for (int i=0; i<numColors; i++)
					iidata.iconColors [i] = bihReader.ReadUInt32 ();
				//XOR mask is immediately after ColorTable and its size is 
				//icon height* no. of bytes per line
				
				//icon height is half of BITMAPINFOHEADER.biHeight, since it contains
				//both XOR as well as AND mask bytes
				int iconHeight = bih.biHeight/2;
				
				//bytes per line should should be uint aligned
				int numBytesPerLine = ((((bih.biWidth * bih.biPlanes * bih.biBitCount)+ 31)>>5)<<2);
				
				//Determine the XOR array Size
				int xorSize = numBytesPerLine * iconHeight;
				iidata.iconXOR = new byte [xorSize];
				for (int i=0; i<xorSize; i++)
					iidata.iconXOR[i] = bihReader.ReadByte();		
				
				//Determine the AND array size
				//For this i subtract the current position from the length.
				//ugly hack...
				int andSize = (int) (bihReader.BaseStream.Length - bihReader.BaseStream.Position);
				iidata.iconAND = new byte [andSize];
				for (int i=0; i<andSize; i++)
					iidata.iconAND[i] = bihReader.ReadByte();		
				
				imageData [j] = iidata;
			}			
		}

		public Icon (string fileName) : this (new FileStream (fileName, FileMode.Open))
		{			
		}

		[MonoTODO ("Implement")]
		public Icon (Type type, string resource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
        	private Icon (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			//FIXME What needs to be called to free memory pointed by handle
			if (winHandle!=IntPtr.Zero)
				winHandle = IntPtr.Zero;
		}

		[MonoTODO ("Implement")]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public static Icon FromHandle (IntPtr handle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public void Save (Stream outputStream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public Bitmap ToBitmap ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			//is this correct, this is what returned by .Net
			return "<Icon>";			
		}

		public IntPtr Handle {
			get { 
				return winHandle;
			}
		}

		public int Height {
			get {
				return iconSize.Height;
			}
		}

		public Size Size {
			get {
				return iconSize;
			}
		}

		public int Width {
			get {
				return iconSize.Width;
			}
		}

		~Icon ()
		{
			Dispose ();
		}
			
	}
}
