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
	internal struct BITMAPINFOHEADER_FLAT {
		internal int      biSize;
		internal int      biWidth;
		internal int      biHeight;
		internal short    biPlanes;
		internal short    biBitCount;
		internal int      biCompression;
		internal int      biSizeImage;
		internal int      biXPelsPerMeter;
		internal int      biYPelsPerMeter;
		internal int      biClrUsed;
		internal int      biClrImportant;
		[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=1024)]
		internal byte[] bmiColors; 

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
			return biSizeImage == 0 ? DibWidthBytes() * biHeight : biSizeImage;
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
			biBitCount = (short)System.Drawing.Image.GetPixelFormatSize (info.PixelFormat);
			biCompression = (int)BitmapCompression.BI_RGB;
			biSizeImage = (int) info.Height * info.Width * Image.GetPixelFormatSize (info.PixelFormat) / 8;
			biXPelsPerMeter = 0;
			biYPelsPerMeter = 0;
			biClrUsed = 0;
			biClrImportant = 0;
			
		}
	}

	internal class BMPCodec {
		
		static  int BITMAPINFOHEADER_SIZE = 40;
			
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
			return true;
		}
		
		bool ReadInfoHeader (Stream stream, out BITMAPINFOHEADER_FLAT bmih) {
			bmih = new BITMAPINFOHEADER_FLAT();
			try {
				BinaryReader bs = new BinaryReader(stream);
				bmih.biSize = bs.ReadInt32();
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
				
				// Currently only BITMAPINFOHEADER
				if (bmih.biSize != BITMAPINFOHEADER_SIZE) return false;
				
				bmih.FixBitmapInfo();

				int numColors = bmih.DibNumColors();
				int index = 0;
				for (int i = 0; i < numColors; i++) {
					bmih.bmiColors[index++] = (byte)stream.ReadByte();
					bmih.bmiColors[index++] = (byte)stream.ReadByte();
					bmih.bmiColors[index++] = (byte)stream.ReadByte();
					bmih.bmiColors[index++] = (byte)stream.ReadByte();
				}
			}
			catch (Exception) {
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
			if (stream.Length < 14 + BITMAPINFOHEADER_SIZE/* sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER)*/)
				return false;
			long startPosition = stream.Position;
			
			BITMAPFILEHEADER	bmfh;
			BITMAPINFOHEADER_FLAT	bmih;
			
			if (!ReadFileHeader (stream, out bmfh))
				return false;
			if (!ReadInfoHeader (stream, out bmih))
				return false;
			
			Color[] colorEntries = new Color[bmih.DibNumColors()];
			int index = 0;
			for (int colorEntryIdx = 0; colorEntryIdx < colorEntries.Length; colorEntryIdx++) {
				// FIXME: is alpha can be used here
				colorEntries[colorEntryIdx] = Color.FromArgb(bmih.bmiColors[index+3], bmih.bmiColors[index+2], bmih.bmiColors[index+1], bmih.bmiColors[index]);
				index += 4;
				colorEntryIdx++;
			}
			image.Palette = new ColorPalette(1, colorEntries);
			image.SetRawFormat (System.Drawing.Imaging.ImageFormat.Bmp);
			info.Width = bmih.biWidth;
			info.Height = bmih.biHeight;
			info.Stride = (int)bmih.DibWidthBytes();

			switch (bmih.biBitCount) {
			case 24:
				Console.WriteLine ("BmpCodec: 24 bits bitmap", bmih.biSizeImage);
				info.PixelFormat = PixelFormat.Format24bppRgb;
				if (bmfh.bfOffBits != 0L)
					stream.Seek (startPosition + bmfh.bfOffBits,SeekOrigin.Begin);

				if (bmih.biCompression == (uint)BitmapCompression.BI_RGB) {
					
					IntPtr lfBuffer = Marshal.AllocHGlobal(bmih.biSizeImage);	
					byte[] bt = new byte[info.Stride];					
					int offset = (info.Height-1) * info.Stride;						
					int baseadr = lfBuffer.ToInt32();
										
					//	DIB are stored upside down. That means that the uppest row which 
					//	appears on the screen actually is the lowest row stored in the bitmap 					
					while(offset>=0){									
						stream.Read(bt, 0, info.Stride); 
						Marshal.Copy (bt, 0, (IntPtr)( baseadr + offset), info.Stride);						
						offset -= info.Stride;				
					}										
					
					Console.WriteLine ("BmpCodec: 24 bits bitmap", bmih.biSizeImage);
					info.Scan0 = lfBuffer;
					info.Allocated=true;		
			
				} else {
					//
					// FIXME
					// 
					Console.WriteLine ("BmpCodec: The {0} compression is not supported", bmih.biCompression);
				}
				break;
			case 32:
				info.PixelFormat = PixelFormat.Format32bppArgb;
				Console.WriteLine ("BmpCodec: 32 bits bitmap", bmih.biSizeImage);
				if (bmfh.bfOffBits != 0L)
					stream.Seek (startPosition + bmfh.bfOffBits,SeekOrigin.Begin);
				if (bmih.biCompression == (uint)BitmapCompression.BI_RGB) {
					
					IntPtr lfBuffer = Marshal.AllocHGlobal(bmih.biSizeImage);	
					byte[] bt = new byte[info.Stride];					
					int offset = (info.Height-1) * info.Stride;						
					int baseadr = lfBuffer.ToInt32();
										
					//	DIB are stored upside down. That means that the uppest row which 
					//	appears on the screen actually is the lowest row stored in the bitmap 					
					while(offset>=0){									
						stream.Read(bt, 0, info.Stride); 
						Marshal.Copy (bt, 0, (IntPtr)( baseadr + offset), info.Stride);						
						offset -= info.Stride;				
					}													
					
					info.Scan0 = lfBuffer;
					info.Allocated=true;						
					
				} else {
					//
					// FIXME
					// 
					Console.WriteLine ("BmpCodec: The {0} compression is not supported", bmih.biCompression);
				}
				break;
			default:
				throw new NotImplementedException(String.Format("This format is not yet supported : {0} bpp", bmih.biBitCount));
			}

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
			BITMAPFILEHEADER bmfh = new BITMAPFILEHEADER();
			bmfh.bfReserved1 = bmfh.bfReserved2 = 0;
			bmfh.bfType = (ushort)BitmapFileType.BFT_BITMAP;
			bmfh.bfOffBits = (uint)(14 + BITMAPINFOHEADER_SIZE + image.Palette.Entries.Length * 4);
			int line_size = info.Stride;
			bmfh.bfSize = (uint)(bmfh.bfOffBits + info.Height * line_size);
					     
			BinaryWriter bw = new BinaryWriter(stream);
			bw.Write(bmfh.bfType);
			bw.Write(bmfh.bfSize);
			bw.Write(bmfh.bfReserved1);
			bw.Write(bmfh.bfReserved2);
			bw.Write(bmfh.bfOffBits);

			BITMAPINFOHEADER_FLAT	bmih = new BITMAPINFOHEADER_FLAT();
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
			Console.WriteLine ("FIXME: BmpCodec: Write palette here");
			
			Console.WriteLine ("biWidth ->" + bmih.biWidth);
			Console.WriteLine ("Height->" + bmih.biHeight);			
			Console.WriteLine ("LineSize ->" + line_size);
			Console.WriteLine ("Address ->" + info.Scan0.ToInt32());
			Console.WriteLine ("Stride ->" + info.Stride);
			Console.WriteLine ("Planes ->" + bmih.biPlanes);						

			byte [] line_buffer = new byte [line_size];
			int stride = info.Stride;
			int offset = (info.Height-1) * stride;						
			int baseadr = info.Scan0.ToInt32();
			
			Console.WriteLine ("Offset ->" + offset);				
			
			//	DIB are stored upside down. That means that the uppest row which 
			//	appears on the screen actually is the lowest row stored in the 
			//	bitmap.							
			while(offset>=0){				
				//FIXME: not an optimal way to specify starting address
				//FIXME: Bitmaps are stored in DWORD alignments
				Marshal.Copy ((IntPtr)( baseadr + offset), line_buffer, 0, line_size);
				stream.Write(line_buffer, 0, line_size);
				offset -= stride;					
			}
			return true;
		}
	}
}
