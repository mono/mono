//
// System.Drawing.Imaging.ImageCodecInfo.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

namespace System.Drawing.Imaging {

	[ComVisible (false)]
	public sealed class ImageCodecInfo 
	{
		private Guid clsid;
		private string codecName;
		private string dllName;
		private string filenameExtension;
		private ImageCodecFlags flags;
		private string formatDescription;
		private Guid formatID;
		private string	mimeType;
		private byte[][] signatureMasks;
		private byte[][] signaturePatterns;
		private int version;
		
		internal ImageCodecInfo()
		{
			
		}

		// methods		
		public static ImageCodecInfo[] GetImageDecoders() 
		{			
			int decoderNums, arraySize, decoder_ptr, decoder_size;
			IntPtr decoders;
			ImageCodecInfo codecinfo = new ImageCodecInfo();
			ImageCodecInfo[] result;
			GdipImageCodecInfo gdipdecoder = new GdipImageCodecInfo();
			Status status;
			
			status = GDIPlus.GdipGetImageDecodersSize (out decoderNums, out arraySize);
			GDIPlus.CheckStatus (status);
			
			result =  new ImageCodecInfo [decoderNums];			
			
			if (decoderNums == 0)
				return result;			
			
			/* Get decoders list*/
			decoders = Marshal.AllocHGlobal (arraySize);						
			status = GDIPlus.GdipGetImageDecoders (decoderNums,  arraySize, decoders);
			GDIPlus.CheckStatus (status);
			
			decoder_size = Marshal.SizeOf (gdipdecoder);			
			decoder_ptr = decoders.ToInt32();
			
			for (int i = 0; i < decoderNums; i++, decoder_ptr += decoder_size)
			{
				gdipdecoder = (GdipImageCodecInfo) Marshal.PtrToStructure ((IntPtr)decoder_ptr, typeof (GdipImageCodecInfo));						
				result[i] = new ImageCodecInfo ();
				GdipImageCodecInfo.MarshalTo (gdipdecoder, result[i]);				
			}
			
			Marshal.FreeHGlobal (decoders);
			return result;
		}
		
		
		public static ImageCodecInfo[] GetImageEncoders() 
		{
			int encoderNums, arraySize, encoder_ptr, encoder_size;
			IntPtr encoders;
			ImageCodecInfo codecinfo = new ImageCodecInfo();
			ImageCodecInfo[] result;
			GdipImageCodecInfo gdipencoder = new GdipImageCodecInfo();
			Status status;
			
			status = GDIPlus.GdipGetImageEncodersSize (out encoderNums, out arraySize);
			GDIPlus.CheckStatus (status);
			
			result =  new ImageCodecInfo [encoderNums];			
			
			if (encoderNums == 0)
				return result;			
			
			/* Get encoders list*/
			encoders = Marshal.AllocHGlobal (arraySize);						
			status = GDIPlus.GdipGetImageEncoders (encoderNums,  arraySize, encoders);
			GDIPlus.CheckStatus (status);
			
			encoder_size = Marshal.SizeOf (gdipencoder);			
			encoder_ptr = encoders.ToInt32();
			
			for (int i = 0; i < encoderNums; i++, encoder_ptr += encoder_size)
			{
				gdipencoder = (GdipImageCodecInfo) Marshal.PtrToStructure ((IntPtr)encoder_ptr, typeof (GdipImageCodecInfo));						
				result[i] = new ImageCodecInfo ();
				GdipImageCodecInfo.MarshalTo (gdipencoder, result[i]);				
			}
			
			Marshal.FreeHGlobal (encoders);
			return result;
		}

		// properties
		
		public Guid Clsid 
		{
			get { return clsid; }
			set { clsid = value; }
		}

		
		public string CodecName 
		{
			get { return codecName; }
			set { codecName = value; }
		}

		
		public string DllName 
		{
			get { return dllName; }
			set { dllName = value; }
		}

		
		public string FilenameExtension 
		{
			get { return filenameExtension; }
			set { filenameExtension = value; }
		}

		
		public ImageCodecFlags Flags 
		{
			get { return flags; }
			set { flags = value; }
		}
		
		public string FormatDescription 
		{
			get { return formatDescription; }
			set { formatDescription = value; }
		}
		
		public Guid FormatID 
		{
			get { return formatID; }
			set { formatID = value; }
		}

		
		public string MimeType 
		{
			get { return mimeType; }
			set { mimeType = value; }
		}

		
		[CLSCompliant(false)]
		public byte[][] SignatureMasks 
		{
			get { return signatureMasks; }
			set { signatureMasks = value; }
		}

		[MonoTODO]
		[CLSCompliant(false)]
		public byte[][] SignaturePatterns 
		{
			get { return signaturePatterns; }
			set { signaturePatterns = value; }
		}
		
		public int Version 
		{
			get { return version; }
			set { version = value; }
		}

	}

}
