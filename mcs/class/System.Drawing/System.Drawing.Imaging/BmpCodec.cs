//
// System.Drawing.Imaging.BMPCodec.cs
//
// Author: 
//		Alexandre Pigolkine (pigolkine@gmx.de)
//	BITMAPINFOHEADER,Decode functions implemented using code/ideas from
//  CxImage (c)  07/Aug/2001 <ing.davide.pizzolato@libero.it>
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

		public void Initialize( InternalImageInfo info) {
			biSize = 40;
			biWidth = info.Size.Width;
			biHeight = info.Size.Height;
			biPlanes = 1;
			biBitCount = (short)System.Drawing.Image.GetPixelFormatSize(info.PixelFormat);
			biCompression = (int)BitmapCompression.BI_RGB;
			biSizeImage = (int)info.RawImageBytes.Length;
			biXPelsPerMeter = 0;
			biYPelsPerMeter = 0;
			biClrUsed = 0;
			biClrImportant = 0;
		}
	}

	internal class BMPCodec {
		
		internal BMPCodec() {
		}
		
		internal static ImageCodecInfo CodecInfo {
			get {
				ImageCodecInfo info = new ImageCodecInfo();
				info.Flags = ImageCodecFlags.Encoder | ImageCodecFlags.Decoder | ImageCodecFlags.Builtin | ImageCodecFlags.SupportBitmap;
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

		bool ReadFileHeader( Stream stream, out BITMAPFILEHEADER bmfh) {
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
		
		bool ReadInfoHeader( Stream stream, out BITMAPINFOHEADER_FLAT bmih) {
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
				if( bmih.biSize != 40) return false;
				
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
			catch( Exception e) {
				return false;
			}
			return true;
		}
		
		internal static void DecodeDelegate (Stream stream, InternalImageInfo info) {
			BMPCodec bmp = new BMPCodec();
			bmp.Decode (stream, info);
		}
		
		internal bool Decode( Stream stream, InternalImageInfo info) {
			if( stream.Length < 14 + 40/* sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER)*/)
				return false;
			long startPosition = stream.Position;
			
			BITMAPFILEHEADER	bmfh;
			BITMAPINFOHEADER_FLAT	bmih;
			if (!ReadFileHeader (stream, out bmfh)) return false;
			if (!ReadInfoHeader (stream, out bmih)) return false;
			Color[] colorEntries = new Color[bmih.DibNumColors()];
			int index = 0;
			for( int colorEntryIdx = 0; colorEntryIdx < colorEntries.Length; colorEntryIdx++) {
				// FIXME: is alpha can be used here
				colorEntries[colorEntryIdx] = Color.FromArgb(bmih.bmiColors[index+3], bmih.bmiColors[index+2], bmih.bmiColors[index+1], bmih.bmiColors[index]);
				index += 4;
				colorEntryIdx++;
			}
			info.Palette = new ColorPalette(1, colorEntries);
			info.Size = new Size(bmih.biWidth, bmih.biHeight);
			info.Stride = (int)bmih.DibWidthBytes();
			info.RawFormat = System.Drawing.Imaging.ImageFormat.Bmp;

			switch (bmih.biBitCount) {
				case 24:
				info.PixelFormat = PixelFormat.Format24bppRgb;
				if (bmfh.bfOffBits != 0L) stream.Seek (startPosition + bmfh.bfOffBits,SeekOrigin.Begin);
				if (bmih.biCompression == (uint)BitmapCompression.BI_RGB) {
					info.RawImageBytes = new byte[bmih.biSizeImage];
					stream.Read(info.RawImageBytes, 0, (int)bmih.biSizeImage);
				}
				else {
				}
				break;
				case 32:
				info.PixelFormat = PixelFormat.Format32bppArgb;
				if (bmfh.bfOffBits != 0L) stream.Seek (startPosition + bmfh.bfOffBits,SeekOrigin.Begin);
				if (bmih.biCompression == (uint)BitmapCompression.BI_RGB) {
					info.RawImageBytes = new byte[bmih.biSizeImage];
					stream.Read(info.RawImageBytes, 0, (int)bmih.biSizeImage);
				}
				else {
				}
				break;
				default:
					throw new NotImplementedException(String.Format("This format is not yet supported : {0} bpp", bmih.biBitCount));
				break;
			}
			return true;
		}

		internal static void EncodeDelegate (Stream stream, InternalImageInfo info) {
			BMPCodec bmp = new BMPCodec();
			bmp.Encode (stream, info);
		}
		
		internal bool Encode( Stream stream, InternalImageInfo info) {
			BITMAPFILEHEADER bmfh = new BITMAPFILEHEADER();
			bmfh.bfReserved1 = bmfh.bfReserved2 = 0;
			bmfh.bfType = (ushort)BitmapFileType.BFT_BITMAP;
			bmfh.bfOffBits = (uint)(14 + 40 + info.Palette.Entries.Length * 4);
			bmfh.bfSize = (uint)(bmfh.bfOffBits + info.RawImageBytes.Length);
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
			// FIXME: write palette here
			stream.Write(info.RawImageBytes, 0, info.RawImageBytes.Length);
			return true;
		}
	}
}
