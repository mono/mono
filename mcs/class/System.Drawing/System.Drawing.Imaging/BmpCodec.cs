//
// System.Drawing.Imaging.BMPCodec.cs
//
// Author: 
//    Alexandre Pigolkine (pigolkine@gmx.de)
//	  Jordi Mas i Hernàndez (jmas@softcatala.org>, 2004
//    BITMAPINFOHEADER,Decode functions implemented using code/ideas from
//    CxImage (c)  07/Aug/2001 <ing.davide.pizzolato@libero.it>
//
// (C) 2002/2003 Ximian, Inc.
//
// Useful documentation about bitmaps
//
//	http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/bitmaps_4v1h.asp
//	http://www.csdn.net/Dev/Format/windows/Bmp.html
//	http://www.fortunecity.com/skyscraper/windows/364/bmpffrmt.html
//
//	Header structure
//		BITMAPFILEHEADER
//		BITMAPINFOHEADER or BITMAPV4HEADER or BITMAPV5HEADER or BITMAPCOREHEADER 
//		RGBQUADS or RGBTRIPLE (optional)
//		Bitmap data
//
namespace System.Drawing.Imaging {

	using System;
	using System.IO;
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;
	

	internal struct BITMAPFILEHEADER {      // File info header
		public ushort bfType;      			// Specifies the type of file. This member must be BM.
		public uint bfSize;      			// Specifies the size of the file, in bytes.
		public ushort bfReserved1; 			// Reserved; must be set to zero.
		public ushort bfReserved2; 			// Reserved; must be set to zero.
		public uint bfOffBits;   			// Specifies the byte offset from the BITMAPFILEHEADER
		// structure to the actual bitmap data in the file.
		
		public void DumpHeader()
		{
			Console.WriteLine("bfType:" + bfType);
			Console.WriteLine("bfSize:" +  bfSize);
			Console.WriteLine("bfReserved1:" + bfReserved1);
			Console.WriteLine("bfReserved2:" + bfReserved2);
			Console.WriteLine("bfOffBits:" + bfOffBits);			
		}
	}

	internal enum BitmapFileType : ushort {
		BFT_ICON  = 0x4349,   /* 'IC' */
		BFT_BITMAP = 0x4d42,   /* 'BM' */
		BFT_CURSOR = 0x5450   /* 'PT' */
	}

	internal enum BitmapCompression : uint {
		BI_RGB        = 0,
		BI_RLE8       = 1,
		BI_RLE4       = 2,
		BI_BITFIELDS  = 3
	}
	
	[StructLayout(LayoutKind.Sequential)]
	internal struct CIEXYZ {
		int 	ciexyzX; 
		int	ciexyzY; 
		int	ciexyzZ; 
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct CIEXYZTRIPLE {			
		CIEXYZ	ciexyzRed; 
		CIEXYZ	ciexyzGreen; 
  		CIEXYZ	ciexyzBlue; 
	}
	
	/* OS/2 BMP Format */
	[StructLayout(LayoutKind.Sequential)]
	internal struct BITMAPCOREHEADER {
		internal int	bcSize; 
  		internal short	bcWidth; 
		internal short bcHeight; 
		internal short bcPlanes; 
		internal short bcBitCount; 
	}  
	
	/* Windows BMP formats */
	[StructLayout(LayoutKind.Sequential)]
	internal struct BITMAPINFOHEADER {
		internal int		biSize;
		internal int		biWidth;
		internal int		biHeight;
		internal short		biPlanes;
		internal short		biBitCount;
		internal int		biCompression;
		internal int		biSizeImage;
		internal int		biXPelsPerMeter;
		internal int		biYPelsPerMeter;
		internal int		biClrUsed;
		internal int		biClrImportant;
		
		/* V4 */				
		internal int		biRedMask; 
		internal int		biGreenMask; 
		internal int		biBlueMask; 
		internal int		biAlphaMask; 
		internal int		biCSType; 
		internal CIEXYZTRIPLE	biEndpoints; 
		internal int		biGammaRed; 
		internal int		biGammaGreen; 
		internal int		biGammaBlue; 
		
		/* V5 */		
		internal int		biIntent; 
		internal int		biProfileData; 
		internal int		biProfileSize; 
		internal int		biReserved; 	
		
		/* Variables not part of the struct*/
		internal bool 		upsidedown;
		internal bool		os2;
		internal byte[]		bmiColors;	
		
		

		static int WIDTHBYTES(int i) {
			return ((i+31)&(~31))/8;  /* ULONG aligned ! */
		}
			
		public int DibWidthBytesN(int n) {
			return WIDTHBYTES(biWidth * n);
		}
			
		public int DibWidthBytes() {
			return DibWidthBytesN(biBitCount);
		}
		
		public int DibSizeImage() {
			return biSizeImage == 0 ? DibWidthBytes() *  biHeight : biSizeImage;
		}
		
		public int DibNumColors() {
			return biClrUsed == 0 && biBitCount <= 8 ? (1 << biBitCount) : biClrUsed;
		}

		public void FixBitmapInfo(){
			if (biSizeImage == 0) 
				biSizeImage = DibSizeImage();
			if (biClrUsed == 0)
				biClrUsed = DibNumColors();
		}
		
		public void DumpHeader()
		{									
			Console.WriteLine("biSize " + biSize);
			Console.WriteLine("biWidth " + biWidth); 
			Console.WriteLine("biHeight " +  biHeight);
			Console.WriteLine("biPlanes " + biPlanes);
			Console.WriteLine("biBitCount " + biBitCount);
			Console.WriteLine("biCompression " + biCompression);
			Console.WriteLine("biSizeImage " + biSizeImage);
			Console.WriteLine("biXPelsPerMeter " + biXPelsPerMeter);
			Console.WriteLine("biYPelsPerMeter " + biYPelsPerMeter);
			Console.WriteLine("biClrUsed " + biClrUsed);
			Console.WriteLine("biClrImportant " + biClrImportant);
			Console.WriteLine("colors: " + DibNumColors());
		}
	    
		float HorizontalResolution {
			get {
				return (float)biXPelsPerMeter * 254.0F / 10000.0F;
			}
		}
		
		float VerticalResolution {
			get {
				return (float)biYPelsPerMeter * 254.0F / 10000.0F;
			}
		}

		public void Initialize (BitmapData info)
		{
			biSize = 40;
			biWidth = info.Width;
			biHeight = info.Height;
			biPlanes = 1;
			biBitCount = (short) System.Drawing.Image.GetPixelFormatSize (info.PixelFormat);
			biCompression = (int)BitmapCompression.BI_RGB;
			biSizeImage =  info.Height * info.Stride;
			biXPelsPerMeter = 0;
			biYPelsPerMeter = 0;
			biClrUsed = 0;
			biClrImportant = 0;			
		}
	}

	internal class BMPCodec {
		
		static int BITMAPINFOHEADER_SIZE  =  40;
		static int BITMAPINFOHEADER_V4_SIZE = 108;
		static int BITMAPINFOHEADER_V5_SIZE = 124;
		static int BITMAPFILEHEADER_SIZE = 14;		
		static int BITMAPCOREHEADER_SIZE = 12;
			
		internal BMPCodec() {
		}
		
		internal static ImageCodecInfo CodecInfo {
			get {
				ImageCodecInfo info = new ImageCodecInfo ();
				info.Flags = ImageCodecFlags.Encoder | ImageCodecFlags.Decoder |
					ImageCodecFlags.Builtin | ImageCodecFlags.SupportBitmap;

				info.FormatDescription = "BITMAP file format";
				info.FormatID = System.Drawing.Imaging.ImageFormat.Bmp.Guid;
				info.MimeType = "image/bmp";
				info.Version = 1;
				byte[][] signaturePatterns = new byte[1][];
				signaturePatterns[0] = new byte[2];
				signaturePatterns[0][0] = 0x42;
				signaturePatterns[0][1] = 0x4d;
				info.SignaturePatterns = signaturePatterns;
				byte[][] signatureMasks = new byte[1][];
				signatureMasks[0] = new byte[2];
				signatureMasks[0][0] = 0xff;
				signatureMasks[0][1] = 0xff;
				info.SignatureMasks = signatureMasks;
				info.decode += new ImageCodecInfo.DecodeFromStream(BMPCodec.DecodeDelegate);
				info.encode += new ImageCodecInfo.EncodeToStream(BMPCodec.EncodeDelegate);
				return info;
			}
		}

		bool ReadFileHeader (Stream stream, out BITMAPFILEHEADER bmfh) {
			bmfh = new BITMAPFILEHEADER();
			BinaryReader bs = new BinaryReader(stream);

			bmfh.bfType = bs.ReadUInt16();
			if(bmfh.bfType != (ushort)BitmapFileType.BFT_BITMAP) return false;
			bmfh.bfSize = bs.ReadUInt32();
			bmfh.bfReserved1 = bs.ReadUInt16();
			bmfh.bfReserved2 = bs.ReadUInt16();
			bmfh.bfOffBits = bs.ReadUInt32();
			
			//bmfh.DumpHeader ();
			return true;
		}
		
		/* BITMAPINFOHEADER/BITMAPV4HEADER/BITMAPV5HEADER */
		bool ReadInfoHeader (Stream stream, out BITMAPINFOHEADER bmih) {
			bmih = new BITMAPINFOHEADER();
			try {
				BinaryReader bs = new BinaryReader(stream);
				bmih.biSize = bs.ReadInt32();
				
				if (bmih.biSize != BITMAPINFOHEADER_SIZE && bmih.biSize != BITMAPINFOHEADER_V4_SIZE &&
					bmih.biSize != BITMAPINFOHEADER_V5_SIZE && bmih.biSize != BITMAPCOREHEADER_SIZE) 					
					throw new Exception ("Invalid BITMAPINFOHEADER size");													
					
				if (bmih.biSize == BITMAPCOREHEADER_SIZE) { // OS/2 Format
					
					bmih.biWidth = bs.ReadInt16();
					bmih.biHeight = bs.ReadInt16();
					bmih.biPlanes = bs.ReadInt16();
					bmih.biBitCount = bs.ReadInt16();
					bmih.biCompression = 0;
					bmih.biSizeImage = 0;
					bmih.biXPelsPerMeter = 0;
					bmih.biYPelsPerMeter = 0;
					bmih.biClrUsed = 0;
					bmih.biClrImportant = 0;				
					bmih.os2 = true;
				}
				else {
				
					bmih.biWidth = bs.ReadInt32();
					bmih.biHeight = bs.ReadInt32();
					bmih.biPlanes = bs.ReadInt16();
					bmih.biBitCount = bs.ReadInt16();
					bmih.biCompression = bs.ReadInt32();
					bmih.biSizeImage = bs.ReadInt32();
					bmih.biXPelsPerMeter = bs.ReadInt32();
					bmih.biYPelsPerMeter = bs.ReadInt32();
					bmih.biClrUsed = bs.ReadInt32();
					bmih.biClrImportant = bs.ReadInt32();				
					bmih.os2 = false;
				}
				
				if (bmih.biHeight < 0) {
					bmih.upsidedown = false;
					bmih.biHeight =  -bmih.biHeight;
				}
				else
					bmih.upsidedown = true;
				
				//bmih.DumpHeader();							
				bmih.FixBitmapInfo();									
				
			}
			catch (Exception e) {
				Console.WriteLine("Exception: " + e.ToString());
				return false;
			}
			return true;
		}
		
		internal static void DecodeDelegate (Image image, Stream stream, BitmapData info)
		{
			BMPCodec bmp = new BMPCodec();
			bmp.Decode (image, stream, info);
		}
		
		internal bool Decode (Image image, Stream stream, BitmapData info)
		{			
			long startPosition = stream.Position;
			
			BITMAPFILEHEADER	bmfh;
			BITMAPINFOHEADER	bmih;
			
			if (!ReadFileHeader (stream, out bmfh))
				return false;			
			
			if (!ReadInfoHeader (stream, out bmih))
				return false;								
			
			if (bmih.biCompression != (uint)BitmapCompression.BI_RGB)
				throw new Exception ("BmpCodec: The compression is not supported");
				
			/* Read RGB palette*/
			Color[] colorEntries = new Color[bmih.DibNumColors()];
			int index = 0;
			byte r,g,b,a;
			
			if (bmih.os2){  // RGBTRIPLE
				for (int colorEntryIdx = 0; colorEntryIdx < colorEntries.Length; colorEntryIdx++, index += 3) {				
					b = (byte) stream.ReadByte();
					g = (byte) stream.ReadByte();
					r = (byte) stream.ReadByte();					
					colorEntries[colorEntryIdx] = Color.FromArgb(r,g,b);							
				}   								
			}
			else { // RGBSquads
				for (int colorEntryIdx = 0; colorEntryIdx < colorEntries.Length; colorEntryIdx++, index += 4) {				
					b = (byte) stream.ReadByte();
					g = (byte) stream.ReadByte();
					r = (byte) stream.ReadByte();
					a = (byte) stream.ReadByte();					
					colorEntries[colorEntryIdx] = Color.FromArgb(a, r,g,b);							
				}   								
			}
			
			image.Palette = new ColorPalette(0, colorEntries);
			image.SetRawFormat (System.Drawing.Imaging.ImageFormat.Bmp);
			info.Width = bmih.biWidth;
			info.Height = bmih.biHeight;
			info.Stride = (int)bmih.DibWidthBytes();
			
			//Console.WriteLine ("Stride ->" + info.Stride + " width " + info.Width * bmih.biBitCount);				

			switch (bmih.biBitCount) {
			case 24:
				info.PixelFormat = PixelFormat.Format24bppRgb;				
				break;
			case 32:
				info.PixelFormat = PixelFormat.Format32bppArgb;									
				break;				
			case 8:			
				info.PixelFormat = PixelFormat.Format8bppIndexed;				
				break;
			case 4:			
				info.PixelFormat = PixelFormat.Format4bppIndexed;				
				break;				
			
			default:
				throw new NotImplementedException(String.Format("This format is not yet supported : {0} bpp", bmih.biBitCount));
			}
			
			if (bmfh.bfOffBits != 0L)
				stream.Seek (startPosition + bmfh.bfOffBits,SeekOrigin.Begin);
					
			IntPtr lfBuffer = Marshal.AllocHGlobal(bmih.biSizeImage);	
			byte[] bt = new byte[info.Stride];					
			int offset = (info.Height-1) * info.Stride;						
			int baseadr = lfBuffer.ToInt32();
			
			//	If the height is positive the DIB are stored upside down. That means that the uppest row 
			//	which appears on the screen actually is the lowest row stored in the bitmap 					
			//	if it is negative, if it stored in the way arround.			
			if (bmih.upsidedown) {				
				offset = (info.Height-1) * info.Stride;														
				while(offset>=0){									
					stream.Read(bt, 0, info.Stride); 
					Marshal.Copy (bt, 0, (IntPtr)( baseadr + offset), info.Stride);						
					offset -= info.Stride;				
				}
			}										
			else {
				offset = 0;
				for (int lines = 0; lines < info.Height ; lines++){
					stream.Read(bt, 0, info.Stride); 
					Marshal.Copy (bt, 0, (IntPtr)( baseadr + offset), info.Stride);	
					offset += info.Stride;
				}										
			}			
			
			info.Scan0 = lfBuffer;
			info.Allocated = true;		
			return true;
		}

		internal static void EncodeDelegate (Image image, Stream stream)
		{
			BMPCodec bmp = new BMPCodec();
			BitmapData info = ((Bitmap)image).LockBits (new Rectangle (new Point (0,0), image.Size),
									  ImageLockMode.ReadOnly, image.PixelFormat);
			bmp.Encode (image, stream, info);
			((Bitmap)image).UnlockBits (info);
		}
		
		internal bool Encode (Image image, Stream stream, BitmapData info)
		{
			Console.WriteLine ("***Encode! " + info.PixelFormat);			
			
			BITMAPFILEHEADER bmfh = new BITMAPFILEHEADER();
			bmfh.bfReserved1 = bmfh.bfReserved2 = 0;
			bmfh.bfType = (ushort)BitmapFileType.BFT_BITMAP;
			bmfh.bfOffBits = (uint)(BITMAPFILEHEADER_SIZE + BITMAPINFOHEADER_SIZE + image.Palette.Entries.Length * 4);
			int line_size = info.Stride;
			bmfh.bfSize = (uint)(bmfh.bfOffBits + info.Height * line_size);			
					     
			BinaryWriter bw = new BinaryWriter(stream);
			bw.Write(bmfh.bfType);
			bw.Write(bmfh.bfSize);
			bw.Write(bmfh.bfReserved1);
			bw.Write(bmfh.bfReserved2);
			bw.Write(bmfh.bfOffBits);
			//bmfh.DumpHeader();

			BITMAPINFOHEADER bmih = new BITMAPINFOHEADER();
			bmih.Initialize(info);
			bw.Write(bmih.biSize);
			bw.Write(bmih.biWidth);
			bw.Write(bmih.biHeight);
			bw.Write(bmih.biPlanes);
			bw.Write(bmih.biBitCount);
			bw.Write(bmih.biCompression);
			bw.Write(bmih.biSizeImage);
			bw.Write(bmih.biXPelsPerMeter);
			bw.Write(bmih.biYPelsPerMeter);
			bw.Write(bmih.biClrUsed);
			bw.Write(bmih.biClrImportant);
			//bmih.DumpHeader();
			
			// Write palette on disk on BGR
			Color[] colors  = image.Palette.Entries;											
			for (int i = 0; i < colors.Length; i++) {
				bw.Write(colors[i].B);
				bw.Write(colors[i].G);
				bw.Write(colors[i].R);
				bw.Write(colors[i].A);
			}				
			
			//Console.WriteLine("Colors written: " + image.Palette.Entries.Length);
			
			byte [] line_buffer = new byte [line_size];
			int stride = info.Stride;
			int offset = (info.Height-1) * stride;						
			int baseadr = info.Scan0.ToInt32();			
			
			
			//	We always store DIB upside down. That means that the uppest row which 
			//	appears on the screen actually is the lowest row stored in the bitmap			
			while(offset>=0){				
				Marshal.Copy ((IntPtr)( baseadr + offset), line_buffer, 0, line_size);
				stream.Write(line_buffer, 0, line_size);
				offset -= stride;					
			}
			
			return true;
		}
	}
}
