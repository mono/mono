//
// System.Drawing.Imaging.ImageCodecInfo.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto
// eMail: everaldo.canuto@bol.com.br
// Dennis Hayes (dennish@raytek.com)
// Alexandre Pigolkine (pigolkine@gmx.de)
//
using System;
using System.Collections;
using System.IO;

namespace System.Drawing.Imaging {

	//[ComVisible(false)]
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
		}

		internal delegate void DecodeFromStream( Stream stream, InternalImageInfo info);
		internal DecodeFromStream decode;

		internal delegate void EncodeToStream( Stream stream, InternalImageInfo info);
		internal EncodeToStream encode;

		// methods
		[MonoTODO]
		//[ComVisible(false)]
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
		//[ComVisible(false)]
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
		//[ComVisible(false)]
		public Guid Clsid {
			get { return clsid; }
			set { clsid = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public string CodecName {
			get { return codecName; }
			set { codecName = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public string DllName {
			get { return dllName; }
			set { dllName = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public string FilenameExtension {
			get { return filenameExtension; }
			set { filenameExtension = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public ImageCodecFlags Flags {
			get { return flags; }
			set { flags = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public string FormatDescription {
			get { return formatDescription; }
			set { formatDescription = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public Guid FormatID {
			get { return formatID; }
			set { formatID = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public string MimeType {
			get { return mimeType; }
			set { mimeType = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public byte[][] SignatureMasks {
			get { return signatureMasks; }
			set { signatureMasks = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public byte[][] SignaturePatterns {
			get { return signaturePatterns; }
			set { signaturePatterns = value; }
		}

		[MonoTODO]
		//[ComVisible(false)]
		public int Version {
			get { return version; }
			set { version = value; }
		}

	}

}
