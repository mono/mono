//
// System.Windows.Forms.ImageListStreamer.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
// (C) 2002 Ximian, Inc
//
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// </summary>
	[Serializable]
	public sealed class ImageListStreamer : ISerializable {
		private static Byte[] Signature = new Byte[] { 77 , 83 , 70 , 116 };
		private static bool   comCtrlInit = false;

		IntPtr himl;

		//Deserialization constructor.
		private ImageListStreamer (SerializationInfo info, StreamingContext context) {
			int i = 0;

			Byte[] data = ( Byte[] )info.GetValue( "Data", typeof( Byte[]) );
			if ( data != null && data.Length >= Signature.Length ) {
				// check the signature ( 'MSFt' )
				if ( data[0] == Signature[0] && data[1] == Signature[1] && data[2] == Signature[2] && data[3] == Signature[3] ) {
					// decompress data encoded with RLE

					// calulate size of array needed for decomressed data
					int RealByteCount = 0;
					for ( i = Signature.Length ; i < data.Length; i += 2 )
						RealByteCount += data[i];

					Console.WriteLine ( " RealByteCount = " + RealByteCount );
					if ( RealByteCount == 0)
						return;

					Byte[] decompressed = new Byte[ RealByteCount ];
					int j = 0;
					for ( i = Signature.Length ; i < data.Length; i += 2 ) {
						for ( int k = 0; k < data[i]; k++ )
							decompressed[ j++ ] = data[ i + 1 ];
					}

					BinaryReader reader = new BinaryReader ( new MemoryStream ( decompressed ) );

					IntPtr hbmMask = IntPtr.Zero;
					IntPtr hbmColor= IntPtr.Zero;

					try {
						// read image list header
						ushort usMagic   = reader.ReadUInt16 ( );
						ushort usVersion = reader.ReadUInt16 ( );
						ushort cCurImage = reader.ReadUInt16 ( );
						ushort cMaxImage = reader.ReadUInt16 ( );
						ushort cGrow     = reader.ReadUInt16 ( );
						ushort cx        = reader.ReadUInt16 ( );
						ushort cy        = reader.ReadUInt16 ( );
						uint   bkcolor   = reader.ReadUInt32 ( );
						ushort flags     = reader.ReadUInt16 ( );

						short[] ovls = new short[4];
						for ( i = 0 ; i < ovls.Length; i++)
							ovls[i] = reader.ReadInt16 ( );
#if DEBUG_OUTPUT
						Console.WriteLine( "usMagic = " + usMagic );
						Console.WriteLine( "usVersion = " + usVersion );
						Console.WriteLine( "cCurImage = " + cCurImage );
						Console.WriteLine( "cMaxImage = " + cMaxImage );
						Console.WriteLine( "cGrow = " + cGrow );
						Console.WriteLine( "cx = " + cx );
						Console.WriteLine( "cy = " + cy );
#endif
						// read image bitmap
						hbmColor = readBitmap( reader, (int)flags & ~(int)ImageListFlags.ILC_MASK, cx, cy );
						if ( hbmColor == IntPtr.Zero )
							return;
						
						if ( ( flags & ( ushort ) ImageListFlags.ILC_MASK ) == ( ushort )ImageListFlags.ILC_MASK ) {
							hbmMask = readBitmap( reader, 0, cx, cy );
							if ( hbmMask == IntPtr.Zero ) {
								Win32.DeleteObject ( hbmColor );
								return ;
							}
						}

						initCommonControlsLibrary ( );

						himl = Win32.ImageList_Create ( cx, cy, (uint)(flags & ~0x1000), 1, cGrow );

						if ( himl == IntPtr.Zero ) {
							Win32.DeleteObject ( hbmColor );
							Win32.DeleteObject ( hbmMask );
							return;
						}

						Win32.ImageList_Add ( himl, hbmColor, hbmMask );
						Win32.ImageList_SetImageCount ( himl, cCurImage );
						Win32.ImageList_SetBkColor ( himl, bkcolor );

						for ( i = 0; i < ovls.Length; i++ )
							Win32.ImageList_SetOverlayImage( himl, ovls[i], i + 1 );
					}
					catch ( SystemException ) {
						if ( hbmMask != IntPtr.Zero )
							Win32.DeleteObject ( hbmMask );
						if ( hbmColor != IntPtr.Zero )
							Win32.DeleteObject ( hbmColor );
					}
				}
			}
		}

		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context){
		}

		internal IntPtr Handle {
			get { return himl; }
		}

		static IntPtr readBitmap( BinaryReader reader, int ilcFlag, int cx, int cy )
		{
			IntPtr bmihc = IntPtr.Zero;
			IntPtr hbitmap = IntPtr.Zero;
			IntPtr nbits = IntPtr.Zero;
			IntPtr dc = IntPtr.Zero;
			bool result = false;

			try {
				BITMAPFILEHEADER bmfh = new BITMAPFILEHEADER ( );

				bmfh.bfType = reader.ReadUInt16 ( );
				bmfh.bfSize = reader.ReadUInt32 ( );
				bmfh.bfReserved1 = reader.ReadUInt16 ( );
				bmfh.bfReserved2 = reader.ReadUInt16 ( );
				bmfh.bfOffBits = reader.ReadUInt32 ( );

				if ( bmfh.bfType != ( ('M' << 8 ) | 'B' ) )
					return IntPtr.Zero;

				BITMAPINFOHEADER bmih = new BITMAPINFOHEADER ( );

				bmih.biSize = reader.ReadUInt32 ( );
				bmih.biWidth =  reader.ReadInt32 ( );
				bmih.biHeight= reader.ReadInt32 ( );
				bmih.biPlanes =  reader.ReadUInt16 ( );
				bmih.biBitCount  = reader.ReadUInt16 ( );
				bmih.biCompression = reader.ReadUInt32 ( );
				bmih.biSizeImage = reader.ReadUInt32 ( );
				bmih.biXPelsPerMeter = reader.ReadInt32 ( );
				bmih.biYPelsPerMeter = reader.ReadInt32 ( );
				bmih.biClrUsed = reader.ReadUInt32 ( );
				bmih.biClrImportant = reader.ReadUInt32 ( );

				if ( bmih.biSize != ( uint ) Marshal.SizeOf( typeof ( BITMAPINFOHEADER ) ) )
					return IntPtr.Zero;

				int bitsperpixel = bmih.biPlanes * bmih.biBitCount;
				int palspace = 0;

				if ( bitsperpixel <= 8 )
					palspace = ( 1 << bitsperpixel ) * Marshal.SizeOf( typeof ( RGBQUAD ) );

				int longsperline = ( ( bmih.biWidth * bitsperpixel + 31 ) & ~0x1f ) >>5 ;
				bmih.biSizeImage = ( uint )( longsperline * bmih.biHeight ) << 2;

				byte[] palette = null;
				if ( palspace > 0 ) {
					palette = new byte [ palspace ];
					int read = reader.Read ( palette, 0, palspace );
					if ( read != palspace )
						return IntPtr.Zero;
				}

				bmihc = Marshal.AllocHGlobal( ( int ) bmih.biSize + palspace );
				Marshal.StructureToPtr ( bmih, bmihc, false );
				if ( palette != null )
					Marshal.Copy ( palette, 0, ( IntPtr ) ( bmihc.ToInt32() + bmih.biSize ), palspace );


				int nwidth  = bmih.biWidth * ( bmih.biHeight / cy );
				int nheight = cy;

				dc = Win32.GetDC ( IntPtr.Zero );

				if (bitsperpixel == 1)
					hbitmap = Win32.CreateBitmap( nwidth, nheight, 1, 1, IntPtr.Zero );
				else
					hbitmap = Win32.CreateCompatibleBitmap( dc, nwidth, nheight );

				byte[] bits  = new byte[ bmih.biSizeImage ];
				reader.Read ( bits, 0, ( int ) bmih.biSizeImage );

				nbits = Marshal.AllocHGlobal( ( int ) bmih.biSizeImage );

				int bytesperline  = longsperline * 4;
				int nbytesperline = ( bmih.biHeight / cy ) * bytesperline;

				for (int i = 0; i < bmih.biHeight; i++ )
					Marshal.Copy ( bits, bytesperline * ( bmih.biHeight - 1 - i ),
						      ( IntPtr ) ( nbits.ToInt32 () +( ( bmih.biHeight - 1 - i ) % cy )* nbytesperline + ( i / cy ) * bytesperline ),
							bytesperline );	

				bmih.biWidth = nwidth;
				bmih.biHeight = nheight;
				Marshal.StructureToPtr ( bmih, bmihc, false );
				
				result = Win32.SetDIBits(dc, hbitmap, 0, ( uint ) nheight, nbits, bmihc, 0) != 0;
			}
			catch ( SystemException ) {
			}

			if ( bmihc != IntPtr.Zero )
				Marshal.FreeHGlobal ( bmihc );
			if ( nbits != IntPtr.Zero )
				Marshal.FreeHGlobal ( nbits );
			if ( dc != IntPtr.Zero )
				Win32.ReleaseDC ( IntPtr.Zero, dc );
			if ( !result && hbitmap != IntPtr.Zero ) 
				Win32.DeleteObject ( hbitmap );
				
			return hbitmap;
		}

		static internal void initCommonControlsLibrary ( ) {
			if ( !comCtrlInit ) {
				Win32.InitCommonControls ( );
				comCtrlInit = true;
			}
		}

	}
}
