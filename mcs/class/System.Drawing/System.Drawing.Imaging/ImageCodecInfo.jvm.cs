//
// System.Drawing.Imaging.ImageCodecInfo.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
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

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using imageio = javax.imageio;
using spi = javax.imageio.spi;

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
		
		#region SpiIterators
		abstract class BaseSpiIterator {
			protected abstract java.util.Iterator GetIterator (string mimeType);
			protected abstract spi.ImageReaderWriterSpi GetNext (java.util.Iterator iter);

			#region ProcessOneCodec
			ImageCodecInfo ProcessOneCodec (Guid clsid, Guid formatID, string mimeType) {
				ImageCodecInfo ici = new ImageCodecInfo ();
				ici.Clsid = clsid;
				ici.FormatID = formatID;
				ici.MimeType = mimeType;
				java.util.Iterator iter = GetIterator (mimeType);
				while (iter.hasNext ()) {
					spi.ImageReaderWriterSpi rw = GetNext (iter);
					try {
						ici.CodecName = rw.getDescription (java.util.Locale.getDefault ());
						ici.DllName = null;
						foreach (string suffix in rw.getFileSuffixes ()) {
							if (ici.FilenameExtension != null)
								ici.FilenameExtension += ";";
							ici.FilenameExtension += "*."+suffix;
						}
						ici.Flags = ImageCodecFlags.Builtin|ImageCodecFlags.SupportBitmap;
						if (rw is spi.ImageReaderSpi) {
							ici.Flags |= ImageCodecFlags.Decoder;
							if ((rw as spi.ImageReaderSpi).getImageWriterSpiNames().Length != 0)
								ici.Flags |= ImageCodecFlags.Encoder;
						}
						if (rw is spi.ImageWriterSpi) {
							ici.Flags |= ImageCodecFlags.Encoder;
							if ((rw as spi.ImageWriterSpi).getImageReaderSpiNames().Length != 0)
								ici.Flags |= ImageCodecFlags.Decoder;
						}
						ici.FormatDescription = string.Join(";",
							rw.getFormatNames());
						ici.Version = (int)Convert.ToDouble(rw.getVersion ());
						break;
					}
					catch {
					}
				}
				return ici;
			}
			#endregion

			public Hashtable Iterate () {
				// TBD: Insert Exception handling here
				NameValueCollection nvc = (NameValueCollection) System.Configuration.ConfigurationSettings
					.GetConfig ("system.drawing/codecs");
				Hashtable codecs = new Hashtable (10);
			
				for (int i=0; i<nvc.Count; i++) {
					Guid clsid = new Guid (nvc.GetKey (i));
					ImageFormat format = ClsidToImageFormat (clsid);
					string [] codecMimeTypes = nvc[i].Split(',');
					for (int j = 0; j < codecMimeTypes.Length; j++) 
					{
						ImageCodecInfo codec = ProcessOneCodec (clsid, format.Guid, codecMimeTypes[j].Trim());
						if (codec.FilenameExtension != null)
							codecs [clsid] = codec;
					}
				}
				return codecs;
			}
		}

		class ReaderSpiIterator: BaseSpiIterator {
			protected override java.util.Iterator GetIterator(string mimeType) {
				return imageio.ImageIO.getImageReadersByMIMEType (mimeType);
			}
			protected override javax.imageio.spi.ImageReaderWriterSpi GetNext(java.util.Iterator iter) {
				imageio.ImageReader r = (imageio.ImageReader) iter.next ();
				return r.getOriginatingProvider ();
			}
		}

		class WriterSpiIterator: BaseSpiIterator {
			protected override java.util.Iterator GetIterator(string mimeType) {
				return imageio.ImageIO.getImageWritersByMIMEType (mimeType);
			}
			protected override javax.imageio.spi.ImageReaderWriterSpi GetNext(java.util.Iterator iter) {
				imageio.ImageWriter w = (imageio.ImageWriter) iter.next ();
				return w.getOriginatingProvider ();
			}
		}
		#endregion

		#region Clsid and FormatID
		static Guid BmpClsid = new Guid ("557cf400-1a04-11d3-9a73-0000f81ef32e");
		static Guid JpegClsid = new Guid ("557cf401-1a04-11d3-9a73-0000f81ef32e");
		static Guid GifClsid = new Guid ("557cf402-1a04-11d3-9a73-0000f81ef32e");
		static Guid EmfClsid = new Guid ("557cf403-1a04-11d3-9a73-0000f81ef32e");
		static Guid WmfClsid = new Guid ("557cf404-1a04-11d3-9a73-0000f81ef32e");
		static Guid TiffClsid = new Guid ("557cf405-1a04-11d3-9a73-0000f81ef32e");
		static Guid PngClsid = new Guid ("557cf406-1a04-11d3-9a73-0000f81ef32e");
		static Guid IconClsid = new Guid ("557cf407-1a04-11d3-9a73-0000f81ef32e");

		internal static ImageFormat ClsidToImageFormat (Guid clsid)
		{
			if (clsid.Equals (BmpClsid))
				return ImageFormat.Bmp;
			else if (clsid.Equals (JpegClsid))
				return ImageFormat.Jpeg;
			else if (clsid.Equals (GifClsid))
				return ImageFormat.Gif;
			else if (clsid.Equals (EmfClsid))
				return ImageFormat.Emf;
			else if (clsid.Equals (WmfClsid))
				return ImageFormat.Wmf;
			else if (clsid.Equals (TiffClsid))
				return ImageFormat.Tiff;
			else if (clsid.Equals (PngClsid))
				return ImageFormat.Png;
			else if (clsid.Equals (IconClsid))
				return ImageFormat.Icon;
			else
				return null;
		}

		internal static Guid ImageFormatToClsid (ImageFormat format)
		{
			if (format == null)
				return Guid.Empty;

			if (format.Guid.Equals (ImageFormat.Bmp.Guid))
				return BmpClsid;
			else if (format.Guid.Equals (ImageFormat.Jpeg.Guid))
				return JpegClsid;
			else if (format.Guid.Equals (ImageFormat.Gif))
				return GifClsid;
			else if (format.Guid.Equals (ImageFormat.Emf.Guid))
				return EmfClsid;
			else if (format.Guid.Equals (ImageFormat.Wmf.Guid))
				return WmfClsid;
			else if (format.Guid.Equals (ImageFormat.Tiff.Guid))
				return TiffClsid;
			else if (format.Guid.Equals (ImageFormat.Png.Guid))
                return PngClsid;
			else if (format.Guid.Equals (ImageFormat.Icon.Guid))
				return IconClsid;
			else
				return Guid.Empty;
		}
		#endregion

		#region Internals
		internal static Hashtable Decoders {
			get {
				const string MYNAME = "System.Drawing.Imaging.ImageCodecInfo.decoders";
				Hashtable o = (Hashtable) AppDomain.CurrentDomain.GetData (MYNAME);
				if (o != null)
					return o;
				o = new ReaderSpiIterator ().Iterate ();
				AppDomain.CurrentDomain.SetData (MYNAME, o);
				return o;
			}
		}

		internal static Hashtable Encoders {
			get {
				const string MYNAME = "System.Drawing.Imaging.ImageCodecInfo.encoders";
				Hashtable o = (Hashtable) AppDomain.CurrentDomain.GetData (MYNAME);
				if (o != null)
					return o;
				o = new WriterSpiIterator ().Iterate ();
				AppDomain.CurrentDomain.SetData (MYNAME, o);
				return o;
			}
		}

		internal static ImageCodecInfo FindEncoder (Guid clsid) {
			return (ImageCodecInfo) Encoders [clsid];
		}

		internal static ImageCodecInfo FindDecoder (Guid clsid) {
			return (ImageCodecInfo) Decoders [clsid];
		}

		internal static string ImageFormatToMimeType (ImageFormat format) {
			//FIXME: this code assumes that any encoder has corresponding decoder
			Guid clsid = ImageFormatToClsid (format);
			ImageCodecInfo codec = FindDecoder (clsid);
			if (codec == null)
				return null;
			return codec.MimeType;
		}
		#endregion

		public static ImageCodecInfo[] GetImageDecoders () 
		{
			Hashtable oldInfo = Decoders;
			ImageCodecInfo [] newInfo = new ImageCodecInfo [oldInfo.Count];
			int i=0;
			foreach (ImageCodecInfo codec in oldInfo.Values) {
				//newInfo [i++] = (ImageCodecInfo) codec.MemberwiseClone ();
				newInfo [i] = new ImageCodecInfo ();
				newInfo [i].clsid = codec.clsid;
				newInfo [i].formatID = codec.formatID;
				newInfo [i].codecName = codec.codecName;
				newInfo [i].dllName = codec.dllName;
				newInfo [i].flags = codec.flags;
				newInfo [i].filenameExtension = codec.filenameExtension;
				newInfo [i].formatDescription = codec.formatDescription;
				newInfo [i].mimeType = codec.mimeType;
				newInfo [i].signatureMasks = codec.signatureMasks;
				newInfo [i].signaturePatterns = codec.signaturePatterns;
				newInfo [i++].version = codec.version;
			}
			return newInfo;
		}
		
		private ImageCodecInfo () {
		}

		public static ImageCodecInfo[] GetImageEncoders () 
		{
			Hashtable oldInfo = Encoders;
			ImageCodecInfo [] newInfo = new ImageCodecInfo [oldInfo.Count];
			int i=0;
			foreach (ImageCodecInfo codec in oldInfo.Values) {
				//newInfo [i++] = (ImageCodecInfo) codec.MemberwiseClone ();
				newInfo [i] = new ImageCodecInfo ();
				newInfo [i].clsid = codec.clsid;
				newInfo [i].formatID = codec.formatID;
				newInfo [i].codecName = codec.codecName;
				newInfo [i].dllName = codec.dllName;
				newInfo [i].flags = codec.flags;
				newInfo [i].filenameExtension = codec.filenameExtension;
				newInfo [i].formatDescription = codec.formatDescription;
				newInfo [i].mimeType = codec.mimeType;
				newInfo [i].signatureMasks = codec.signatureMasks;
				newInfo [i].signaturePatterns = codec.signaturePatterns;
				newInfo [i++].version = codec.version;
			}
			return newInfo;
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
