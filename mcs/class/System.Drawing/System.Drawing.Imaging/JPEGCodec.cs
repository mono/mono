//
// System.Drawing.Imaging.BMPCodec.cs
//
// Author: 
//		Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002/2003 Ximian, Inc.

#if DECLARE_CDECL_DELEGATES
namespace cdeclCallback {
	using System;
	internal class cdeclRedirector {
		internal delegate void MethodVoidIntPtr(IntPtr param);
		internal delegate int MethodIntIntPtr(IntPtr param);
		internal delegate void MethodVoidIntPtrInt(IntPtr param, int param1);
		internal delegate int MethodIntIntPtrInt(IntPtr param,int param1);
	}
}
#endif

namespace System.Drawing.Imaging
{
	using System;
	using System.IO;
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;
	using cdeclCallback;

	/// <summary>
	/// Summary description for JPEGCodec.
	/// </summary>
	internal class JPEGCodec
	{
		enum J_COLOR_SPACE : int {
			JCS_UNKNOWN		= 0,		/* error/unspecified */
			JCS_GRAYSCALE	= 1,		/* monochrome */
			JCS_RGB			= 2,		/* red/green/blue */
			JCS_YCbCr		= 3,		/* Y/Cb/Cr (also known as YUV) */
			JCS_CMYK		= 4,		/* C/M/Y/K */
			JCS_YCCK		= 5			/* Y/Cb/Cr/K */
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct jpeg_error_mgr_get {
			public IntPtr a1;
			public IntPtr a2;
			public IntPtr a3;
			public IntPtr a4;
			public IntPtr a5;
			public int msg_code;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=20)]
			public int[] array;
			public int trace_level;
			public int num_warnings;
			public int jpeg_message_table;
			public int last_jpeg_message;
			public int addon_message_table;
			public int first_addon_message;
			public int last_addon_message;
		};

		[StructLayout(LayoutKind.Sequential)]
		internal struct jpeg_error_mgr {
			public cdeclRedirector.MethodVoidIntPtr error_exit;
			public IntPtr a2;
			public IntPtr a3;
			public IntPtr a4;
			public IntPtr a5;
			public int msg_code;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=20)]
			public int[] array;
			public int trace_level;
			public int num_warnings;
			public int jpeg_message_table;
			public int last_jpeg_message;
			public int addon_message_table;
			public int first_addon_message;
			public int last_addon_message;
		};

		[StructLayout(LayoutKind.Sequential)]
			struct jpeg_source_mgr {
			public IntPtr next_input_byte; /* => next byte to read from buffer */
			public uint   bytes_in_buffer;	/* # of bytes remaining in buffer */

			//[MarshalAs(UnmanagedType.FunctionPtr)]
			public cdeclRedirector.MethodVoidIntPtr init_source;
			//[MarshalAs(UnmanagedType.FunctionPtr)]
			public cdeclRedirector.MethodIntIntPtr fill_input_buffer;
			//[MarshalAs(UnmanagedType.FunctionPtr)]
			public cdeclRedirector.MethodVoidIntPtrInt skip_input_data;
			//[MarshalAs(UnmanagedType.FunctionPtr)]
			public cdeclRedirector.MethodIntIntPtrInt resync_to_restart;
			//[MarshalAs(UnmanagedType.FunctionPtr)]
			public cdeclRedirector.MethodVoidIntPtr term_source;
		};
	
		class jpeg_decompress_struct {
			byte[] raw_struct;
			IntPtr raw_error_mgr = IntPtr.Zero;
			IntPtr raw_source_mgr = IntPtr.Zero;

			internal struct decompress_fields {
				internal int structure_size;
				internal int QUANTIZE_COLORS;
				internal int ACTUAL_NUMBER_OF_COLORS;
				internal int OUT_COLOR_SPACE;
				internal int OUTPUT_WIDTH;
				internal int OUTPUT_HEIGHT;
				internal int OUT_COLOR_COMPONENT;
				internal int OUTPUT_COMPONENTS;
				internal int OUTPUT_SCANLINE;
				internal int OUT_COLORMAP;
			};

			const int GNU_JPEG_DLL_WINDOWS = 0;
			const int LINUX_LIBJPEG = 1;
			const int KNOWN_JPEG_LINRARIES = 2;

			static bool offsets_initialized;
			static decompress_fields[] know_jpeg_libraries;
			static int current_library_index = LINUX_LIBJPEG;
			static void initialize_jpeg_decompress_structs() {
				if (!offsets_initialized) {
					know_jpeg_libraries = new decompress_fields[KNOWN_JPEG_LINRARIES];
					// GNU JPEG Windows version
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].structure_size = 432;
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].QUANTIZE_COLORS = 74;
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].ACTUAL_NUMBER_OF_COLORS = 112;
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].OUT_COLOR_SPACE = 44;
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].OUTPUT_WIDTH = 92;
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].OUTPUT_HEIGHT = 96;
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].OUT_COLOR_COMPONENT = 100;
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].OUTPUT_COMPONENTS = 104;
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].OUTPUT_SCANLINE = 120;
					know_jpeg_libraries[GNU_JPEG_DLL_WINDOWS].OUT_COLORMAP = 116;

					// libjpeg Linux version
					know_jpeg_libraries[LINUX_LIBJPEG].structure_size = 464;
					know_jpeg_libraries[LINUX_LIBJPEG].QUANTIZE_COLORS = 84;
					know_jpeg_libraries[LINUX_LIBJPEG].ACTUAL_NUMBER_OF_COLORS = 132;
					know_jpeg_libraries[LINUX_LIBJPEG].OUT_COLOR_SPACE = 44;
					know_jpeg_libraries[LINUX_LIBJPEG].OUTPUT_WIDTH = 112;
					know_jpeg_libraries[LINUX_LIBJPEG].OUTPUT_HEIGHT = 116;
					know_jpeg_libraries[LINUX_LIBJPEG].OUT_COLOR_COMPONENT = 120;
					know_jpeg_libraries[LINUX_LIBJPEG].OUTPUT_COMPONENTS = 124;
					know_jpeg_libraries[LINUX_LIBJPEG].OUTPUT_SCANLINE = 140;
					know_jpeg_libraries[LINUX_LIBJPEG].OUT_COLORMAP = 136;

					offsets_initialized = true;
				}
			}

			static jpeg_decompress_struct() {
				initialize_jpeg_decompress_structs();
			}

			public jpeg_decompress_struct() {
				raw_struct = new byte[know_jpeg_libraries[current_library_index].structure_size];
			}

			public void switch_to_struct_size(int size) {
				if (raw_struct.Length == size) return;

				bool structureFound = false;
				for( int i = 0; i < KNOWN_JPEG_LINRARIES; i++) {
					if( know_jpeg_libraries[i].structure_size == size) {
						current_library_index = i;
						raw_struct = new byte[know_jpeg_libraries[current_library_index].structure_size];
						structureFound = true;
						break;
					}
				}
				if (!structureFound) throw new Exception("JPEG Codec cannot work with existing libjpeg");
			}

			public byte[] raw_decompress_struct {
				get {
					return raw_struct;
				}
			}

			unsafe protected void copyToStruct( int value, int offset) {
				fixed( byte* pd = raw_struct) {
					*((int*)(pd + offset)) = value;
				}
			}

			unsafe protected int copyFromStruct( int offset) {
				int result = 0;
				fixed( byte* pd = raw_struct) {
					result = *((int*)(pd + offset));
				}
				return result;
			}

			public jpeg_error_mgr jpeg_error_mgr {
				set {
					raw_error_mgr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
					Marshal.StructureToPtr(value, raw_error_mgr, false);
					copyToStruct(raw_error_mgr.ToInt32(), 0);
				}
			}
		
			public jpeg_source_mgr jpeg_source_mgr {
				set {
					raw_source_mgr = Marshal.AllocHGlobal( Marshal.SizeOf(value));
					Marshal.StructureToPtr( value, raw_source_mgr, false);
					copyToStruct(raw_source_mgr.ToInt32(), 24);
				}
			}

			public int Stride {
				get {
					return OutputWidth * OutputComponents;
				}
			}

			public Color[] ColorMap {
				get {
					int actual_number_of_colors = copyFromStruct(know_jpeg_libraries[current_library_index].ACTUAL_NUMBER_OF_COLORS);
					IntPtr nativeMap = (IntPtr)copyFromStruct(know_jpeg_libraries[current_library_index].OUT_COLORMAP);
					Color[] map = new Color[actual_number_of_colors];
					if (nativeMap != IntPtr.Zero) {
						byte[] byteMap = new byte[OutColorComponents * actual_number_of_colors];
						Marshal.Copy( (IntPtr)Marshal.ReadInt32(nativeMap), byteMap, 0, byteMap.Length);
					}
					return map;
				}
			}

			public J_COLOR_SPACE OutColorSpace {
				get {
					return (J_COLOR_SPACE)copyFromStruct(know_jpeg_libraries[current_library_index].OUT_COLOR_SPACE);
				}
				set {
					copyToStruct((int)value,know_jpeg_libraries[current_library_index].OUT_COLOR_SPACE);
				}
			}

			public bool QuantizeColors {
				get {
					return raw_struct[know_jpeg_libraries[current_library_index].QUANTIZE_COLORS] != (byte)0 ? true : false;
				}
				set {
					raw_struct[know_jpeg_libraries[current_library_index].QUANTIZE_COLORS] = value ? (byte)1 : (byte)0;
				}
			}

			public int OutputWidth {
				get {
					return copyFromStruct(know_jpeg_libraries[current_library_index].OUTPUT_WIDTH);
				}
			}

			public int OutputHeight {
				get {
					return copyFromStruct(know_jpeg_libraries[current_library_index].OUTPUT_HEIGHT);
				}
			}

			public int OutColorComponents {
				get {
					return copyFromStruct(know_jpeg_libraries[current_library_index].OUT_COLOR_COMPONENT);
				}
			}

			public int OutputComponents {
				get {
					return copyFromStruct(know_jpeg_libraries[current_library_index].OUTPUT_COMPONENTS);
				}
			}

			public int OutputScanLine {
				get {
					return copyFromStruct(know_jpeg_libraries[current_library_index].OUTPUT_SCANLINE);
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
			internal struct JSAMPARRAY {
			// FIXME: This code is not working on Mono(** ERROR **: Invalid IL code at...). Report a bug and change it later.
			//const int MAX_SCAN_LINES = 10;
			//[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=MAX_SCAN_LINES)]
			//internal  IntPtr[] JSAMPLES;
			internal IntPtr JSAMPLE0;
			internal IntPtr JSAMPLE1;

			internal  JSAMPARRAY(int len) {
/*
				JSAMPLES = new IntPtr[MAX_SCAN_LINES];
				for (int i = 0; i < MAX_SCAN_LINES; i++) {
					JSAMPLES[i] = Marshal.AllocHGlobal(len);
				}
*/				
				JSAMPLE0 = Marshal.AllocHGlobal(len);
				JSAMPLE1 = Marshal.AllocHGlobal(len);
			}

			internal  void Dispose() {
/*
				for (int i = 0; i < MAX_SCAN_LINES; i++) {
					Marshal.FreeHGlobal(JSAMPLES[i]);
				}
*/				
				Marshal.FreeHGlobal(JSAMPLE0);
				Marshal.FreeHGlobal(JSAMPLE1);
			}
		}

		const string JPEGLibrary = "jpeg";
		[DllImport(JPEGLibrary, EntryPoint="jpeg_CreateDecompress", CallingConvention=CallingConvention.Cdecl)]
		internal static extern void jpeg_create_decompress(byte[] info, int version, int structure_size);
		[DllImport(JPEGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void jpeg_std_error(ref jpeg_error_mgr_get err_mgr);
		[DllImport(JPEGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern int jpeg_read_header(byte[] cinfo, int condition);
		[DllImport(JPEGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void jpeg_calc_output_dimensions(byte[] cinfo);
		[DllImport(JPEGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern int jpeg_start_decompress(byte[] cinfo);
		[DllImport(JPEGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern int jpeg_read_scanlines(byte[] cinfo, ref JSAMPARRAY buffer, int num);
		[DllImport(JPEGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern int jpeg_finish_decompress(byte[] cinfo);
		[DllImport(JPEGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void jpeg_destroy_decompress(byte[] cinfo);

		Stream fs;
		IntPtr buffer;
		int readSize = 4096;

		void init_source( IntPtr cinfo) {
			buffer = Marshal.AllocHGlobal(readSize);
		}

		int fill_input_buffer( IntPtr cinfo) {
			byte[] result = new byte[readSize];
			int readed = fs.Read(result, 0, readSize);
			Marshal.Copy(result, 0, buffer, readed);
			IntPtr srcAddr = (IntPtr)Marshal.ReadInt32(cinfo, 24);
			Marshal.WriteInt32(srcAddr, 0, buffer.ToInt32());
			Marshal.WriteInt32(srcAddr, 4, readed);
			return 1;
		}

		void skip_input_data( IntPtr cinfo, int num_bytes) {
			//byte[] result = new byte[num_bytes];
			//fs.Read(result, 0, num_bytes);
			fs.Seek(num_bytes, SeekOrigin.Current);
		}

		int resync_to_restart( IntPtr cinfo, int desired){
			return 0;
		}

		void term_source( IntPtr cinfo) {
			Marshal.AllocHGlobal(buffer);
			fs.Close();
		}

		class RetryInitializationException : Exception {
			int libraryStructureSize;
			public RetryInitializationException(int structureSize) {
				this.libraryStructureSize = structureSize;
			}
			public int LibraryStructureSize {
				get {
					return libraryStructureSize;
				}
			}
		}
		
		enum JPEGErrorCodes : int {
			JERR_BAD_STRUCT_SIZE = 21
		}

		void error_exit( IntPtr cinfo) {
			jpeg_error_mgr mgr = new jpeg_error_mgr();
			IntPtr err_raw = (IntPtr)Marshal.ReadInt32(cinfo, 0);
			mgr = (jpeg_error_mgr)Marshal.PtrToStructure(err_raw, mgr.GetType());
			if ( mgr.msg_code == (int)JPEGErrorCodes.JERR_BAD_STRUCT_SIZE) {
				throw new RetryInitializationException(mgr.array[0]);
			}
			throw new Exception();
		}

		internal JPEGCodec() {
		}

		internal static ImageCodecInfo CodecInfo {
			get {
				ImageCodecInfo info = new ImageCodecInfo();
				info.Flags = /*ImageCodecFlags.Encoder | */ ImageCodecFlags.Decoder | ImageCodecFlags.Builtin | ImageCodecFlags.SupportBitmap;
				info.FormatDescription = "JPEG file format";
				info.FormatID = System.Drawing.Imaging.ImageFormat.Jpeg.Guid;
				info.MimeType = "image/jpeg";
				info.Version = 1;
				byte[][] signaturePatterns = new byte[1][];
				signaturePatterns[0] = new byte[]{0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46, 0x00};
				info.SignaturePatterns = signaturePatterns;
				byte[][] signatureMasks = new byte[1][];
				signatureMasks[0] = new byte[]{0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff};
				info.SignatureMasks = signatureMasks;
				info.decode += new ImageCodecInfo.DecodeFromStream(JPEGCodec.DecodeDelegate);
				info.encode += new ImageCodecInfo.EncodeToStream(JPEGCodec.EncodeDelegate);
				return info;
			}
		}

		internal static void DecodeDelegate (Stream stream, InternalImageInfo info) {
			JPEGCodec jpeg = new JPEGCodec();
			jpeg.Decode (stream, info);
		}

		internal static void EncodeDelegate (Stream stream, InternalImageInfo info) {
			JPEGCodec jpeg = new JPEGCodec();
			jpeg.Encode (stream, info);
		}

		internal unsafe void switch_color_bytes( byte[] image) {
			fixed(byte* start = image) {
				byte *pb = start;
				byte t1;
				for( int ic = 0; ic < image.Length; ic +=3) {
					t1 = *pb;
					*(pb) = *(pb+2);
					*(pb+2) = t1;
					pb += 3;
				}
			}
		}

		internal bool Decode( Stream stream, InternalImageInfo info) {
			fs = stream;

			jpeg_error_mgr_get mgr = new jpeg_error_mgr_get();
			mgr.array = new int[20];
			jpeg_std_error( ref mgr);
			jpeg_error_mgr mgr_real = new jpeg_error_mgr();
			mgr_real.array = new int[20];
			mgr_real.error_exit = new cdeclCallback.cdeclRedirector.MethodVoidIntPtr(this.error_exit);
			mgr_real.msg_code = mgr.msg_code;
			mgr_real.a2 = mgr.a2;
			mgr_real.a3 = mgr.a3;
			mgr_real.a4 = mgr.a4;
			mgr_real.a5 = mgr.a5;
			mgr_real.trace_level = mgr.trace_level;
			mgr_real.num_warnings = mgr.num_warnings;
			mgr_real.last_jpeg_message = mgr.last_jpeg_message;
			mgr_real.first_addon_message = mgr.first_addon_message;
			mgr_real.last_addon_message = mgr.last_addon_message;
			mgr_real.jpeg_message_table = mgr.jpeg_message_table;
			
			jpeg_decompress_struct cinfo = new jpeg_decompress_struct();
			cinfo.jpeg_error_mgr = mgr_real;
			bool initializedOk = false;
			do {
				try {
					jpeg_create_decompress(cinfo.raw_decompress_struct, 62, cinfo.raw_decompress_struct.Length);
					initializedOk = true;
				}
				catch( RetryInitializationException ex) {
					initializedOk = false;
					cinfo.switch_to_struct_size(ex.LibraryStructureSize);
					cinfo.jpeg_error_mgr = mgr_real;
				}
			}while( !initializedOk);

			jpeg_source_mgr smgr = new jpeg_source_mgr();
			smgr.next_input_byte = IntPtr.Zero;
			smgr.bytes_in_buffer = 0;

			smgr.init_source = new cdeclRedirector.MethodVoidIntPtr(this.init_source);
			smgr.fill_input_buffer = new cdeclRedirector.MethodIntIntPtr(this.fill_input_buffer);
			smgr.skip_input_data = new cdeclRedirector.MethodVoidIntPtrInt(this.skip_input_data);
			smgr.resync_to_restart = new cdeclRedirector.MethodIntIntPtrInt(this.resync_to_restart);
			smgr.term_source = new cdeclRedirector.MethodVoidIntPtr(this.term_source);
			cinfo.jpeg_source_mgr = smgr;
			jpeg_read_header( cinfo.raw_decompress_struct, 1);

			jpeg_calc_output_dimensions(cinfo.raw_decompress_struct);
			jpeg_start_decompress(cinfo.raw_decompress_struct);

			int row_width = cinfo.Stride;
			while ((row_width & 3) != 0) row_width++;
			int pad_bytes = (row_width - cinfo.Stride);

			if (cinfo.OutColorSpace == J_COLOR_SPACE.JCS_RGB) {
				if (cinfo.QuantizeColors) {
					info.Format = PixelFormat.Format8bppIndexed;
				}
				else {
					info.Format = PixelFormat.Format24bppRgb;
				}
			}
			else {
				info.Format = PixelFormat.Format8bppIndexed;
			}
			info.Size = new Size(cinfo.OutputWidth,cinfo.OutputHeight);
			info.Stride = cinfo.Stride + pad_bytes;
			info.Palette = new ColorPalette(1, cinfo.ColorMap);
			info.RawImageBytes = new byte[(cinfo.OutputHeight) * (cinfo.Stride + pad_bytes)];

			JSAMPARRAY outbuf = new JSAMPARRAY(cinfo.Stride);
			int outputRow = 0;
			int outputIndex = info.RawImageBytes.Length - cinfo.Stride - pad_bytes;

			while (cinfo.OutputScanLine < cinfo.OutputHeight) {
				// FIXME: switch to the Length after fixing a run-time error
				int readed = jpeg_read_scanlines(cinfo.raw_decompress_struct, ref outbuf, 1 /*outbuf.JSAMPLES.Length*/);
				for (int i = 0; i < readed; i++) {
					// FIXME: switch to .JSAMPLES[i] after fixing a run-time error
					//Marshal.Copy(outbuf.JSAMPLES[i], info.RawImageBytes, outputIndex, cinfo.Stride);
					Marshal.Copy(outbuf.JSAMPLE0, info.RawImageBytes, outputIndex, cinfo.Stride);
					outputIndex -= cinfo.Stride + pad_bytes;
					outputRow++;
				}
			}
			// FIXME: analise count of color components here
			switch_color_bytes(info.RawImageBytes);
			jpeg_finish_decompress(cinfo.raw_decompress_struct);
			jpeg_destroy_decompress(cinfo.raw_decompress_struct);
			return true;
		}

		internal bool Encode( Stream stream, InternalImageInfo info) {
			return false;
		}
	}
}
