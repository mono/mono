//
// System.Drawing.Imaging.ImageCodecInfo.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

namespace System.Drawing.Imaging {

	[ComVisible (false)]
	public sealed class ImageCodecInfo {

		Guid	clsid;
		string  codecName;
		string  dllName;
		string  filenameExtension;
		ImageCodecFlags flags;
		string  formatDescription;
		Guid	formatID;
		string	mimeType;
		byte[][] signatureMasks;
		byte[][] signaturePatterns;
		int		version;

		static ArrayList allCodecs = new ArrayList();

		static ImageCodecInfo() {
			allCodecs.Add(BMPCodec.CodecInfo);
			allCodecs.Add(JPEGCodec.CodecInfo);
			allCodecs.Add(PNGCodec.CodecInfo);
		}

		internal ImageCodecInfo()
		{
		}

		internal delegate void DecodeFromStream (Image image, Stream stream, BitmapData info);
		internal DecodeFromStream decode;

		internal delegate void EncodeToStream (Image image, Stream stream);
		internal EncodeToStream encode;

		// methods
		[MonoTODO]
		public static ImageCodecInfo[] GetImageDecoders() {
			ArrayList decoders = new ArrayList();
			foreach( ImageCodecInfo info in allCodecs) {
				if( (info.Flags & ImageCodecFlags.Decoder) != 0) {
					decoders.Add( info);
				}
			}
			ImageCodecInfo[] result = new ImageCodecInfo[decoders.Count];
			decoders.CopyTo( result, 0);
			return result;
		}
		
		[MonoTODO]
		public static ImageCodecInfo[] GetImageEncoders() {
			ArrayList encoders = new ArrayList();
			foreach( ImageCodecInfo info in allCodecs) {
				if( (info.Flags & ImageCodecFlags.Encoder) != 0) {
					encoders.Add( info);
				}
			}
			ImageCodecInfo[] result = new ImageCodecInfo[encoders.Count];
			encoders.CopyTo( result, 0);
			return result;
		}

		// properties
		[MonoTODO]
		public Guid Clsid {
			get { return clsid; }
			set { clsid = value; }
		}

		[MonoTODO]
		public string CodecName {
			get { return codecName; }
			set { codecName = value; }
		}

		[MonoTODO]
		public string DllName {
			get { return dllName; }
			set { dllName = value; }
		}

		[MonoTODO]
		public string FilenameExtension {
			get { return filenameExtension; }
			set { filenameExtension = value; }
		}

		[MonoTODO]
		public ImageCodecFlags Flags {
			get { return flags; }
			set { flags = value; }
		}

		[MonoTODO]
		public string FormatDescription {
			get { return formatDescription; }
			set { formatDescription = value; }
		}

		[MonoTODO]
		public Guid FormatID {
			get { return formatID; }
			set { formatID = value; }
		}

		[MonoTODO]
		public string MimeType {
			get { return mimeType; }
			set { mimeType = value; }
		}

		[MonoTODO]
		[CLSCompliant(false)]
		public byte[][] SignatureMasks {
			get { return signatureMasks; }
			set { signatureMasks = value; }
		}

		[MonoTODO]
		[CLSCompliant(false)]
		public byte[][] SignaturePatterns {
			get { return signaturePatterns; }
			set { signaturePatterns = value; }
		}

		[MonoTODO]
		public int Version {
			get { return version; }
			set { version = value; }
		}

	}

}
