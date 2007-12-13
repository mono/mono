using System;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Xml;
using Mainsoft.Drawing.Configuration;

using imageio = javax.imageio;
using stream = javax.imageio.stream;
using awt = java.awt;
using image = java.awt.image;
using spi = javax.imageio.spi;
using dom = org.w3c.dom;

namespace Mainsoft.Drawing.Imaging {
	/// <summary>
	/// Summary description for ImageCodec.
	/// </summary>
	public class ImageCodec : IDisposable {

		#region Members

		imageio.ImageReader _nativeReader = null;
		imageio.ImageWriter _nativeWriter = null;
		stream.ImageInputStream _nativeStream = null;

		ImageFormat _imageFormat = null;

		int _currentFrame = 0;

		#endregion

		#region Constructros

		protected ImageCodec() {
		}

		static ImageCodec() {
		}

		#endregion

		#region Internal properties

		internal imageio.ImageReader NativeReader {
			get { return _nativeReader; }
			set { 
				_nativeReader = value; 
				if (value == null)
					return;
				_imageFormat = MimeTypesToImageFormat( value.getOriginatingProvider().getMIMETypes() );
			}
		}
		internal imageio.ImageWriter NativeWriter {
			get { return _nativeWriter; }
			set { 
				_nativeWriter = value; 
				if (value == null)
					return;
				_imageFormat = MimeTypesToImageFormat( value.getOriginatingProvider().getMIMETypes() );
			}
		}

		internal stream.ImageInputStream NativeStream {
			get { return _nativeStream; }
			set {
				_nativeStream = value;
				if (value == null)
					return;

				if (NativeReader != null)
					NativeReader.setInput( value );

				if (NativeWriter != null)
					NativeWriter.setOutput( value );
			}
		}

		#endregion

		#region ImageCodec factory methods

		public static ImageCodec CreateReader(stream.ImageInputStream inputStream) {
			java.util.Iterator iter = imageio.ImageIO.getImageReaders( inputStream );
			return CreateReader(iter);
		}

		public static ImageCodec CreateReader(ImageFormat imageFormat) {
			return CreateReader( ImageFormatToClsid( imageFormat ) );
		}

		public static ImageCodec CreateReader(Guid clsid) {
			ImageCodec codec = null;
			try {
				ImageCodecInfo codecInfo = FindDecoder(clsid);
				java.util.Iterator iter = imageio.ImageIO.getImageReadersByMIMEType( codecInfo.MimeType );
				codec = CreateReader(iter);
			}
			catch {}

			if (codec == null) {
				ImageFormat format = ClsidToImageFormat(clsid);
				string name = (format != null) ? format.ToString() : clsid.ToString();
				throw new NotSupportedException(String.Format("The '{0}' format decoder is not installed.", name));
			}

			return codec;
		}

		private static ImageCodec CreateReader(java.util.Iterator iter) {
			if ( !iter.hasNext() )
				return null;

			ImageCodec imageCodec = new ImageCodec();
			imageCodec.NativeReader = (imageio.ImageReader) iter.next();
			return imageCodec;
		}

		public static ImageCodec CreateWriter(ImageFormat imageFormat) {
			return CreateWriter( ImageFormatToClsid( imageFormat ) );
		}

		public static ImageCodec CreateWriter(Guid clsid) {
			ImageCodec codec = null;
			try {
				ImageCodecInfo codecInfo = FindEncoder(clsid);
				java.util.Iterator iter = imageio.ImageIO.getImageWritersByMIMEType( codecInfo.MimeType );
				codec = CreateWriter(iter);
			}
			catch {}

			if (codec == null) {
				ImageFormat format = ClsidToImageFormat(clsid);
				string name = (format != null) ? format.ToString() : clsid.ToString();
				throw new NotSupportedException(String.Format("The '{0}' format encoder is not installed.", name));
			}

			return codec;
		}

		private static ImageCodec CreateWriter(java.util.Iterator iter) {
			if ( !iter.hasNext() )
				return null;
			
			ImageCodec imageCodec = new ImageCodec();
			imageCodec.NativeWriter = (imageio.ImageWriter) iter.next();
			return imageCodec;
		}

		#endregion

		#region Codec enumerations

		internal static Hashtable Decoders {
			get {
				const string MYNAME = "System.Drawing.Imaging.ImageCodecInfo.decoders";
				Hashtable o = (Hashtable) AppDomain.CurrentDomain.GetData (MYNAME);
				if (o != null)
					return o;
				o = new ReaderSpiIterator().Iterate();
				AppDomain.CurrentDomain.SetData(MYNAME, o);
				return o;
			}
		}

		internal static Hashtable Encoders {
			get {
				const string MYNAME = "System.Drawing.Imaging.ImageCodecInfo.encoders";
				Hashtable o = (Hashtable) AppDomain.CurrentDomain.GetData (MYNAME);
				if (o != null)
					return o;
				o = new WriterSpiIterator().Iterate();
				AppDomain.CurrentDomain.SetData(MYNAME, o);
				return o;
			}
		}

		internal static ImageCodecInfo FindEncoder (Guid clsid) {
			ImageCodecInfo codec = (ImageCodecInfo) Encoders[clsid];
			if (codec == null) {
				// .net saves in png if cannot find requested encoder. atc id 316563
				codec = (ImageCodecInfo) Encoders[ ImageCodec.PngClsid ];
			}
			return codec;
		}

		internal static ImageCodecInfo FindDecoder (Guid clsid) {
			ImageCodecInfo codec = (ImageCodecInfo) Decoders[clsid];
			if (codec == null) {
				ImageFormat format = ClsidToImageFormat(clsid);
				string name = (format != null) ? format.ToString() : clsid.ToString();
				throw new NotSupportedException(String.Format("The '{0}' format decoder is not installed.", name));
			}
			return codec;
		}

		#endregion

		#region SpiIterators

		abstract class BaseSpiIterator {
			protected abstract java.util.Iterator GetIterator (string mimeType);
			protected abstract spi.ImageReaderWriterSpi GetNext (java.util.Iterator iter);

			#region ProcessOneCodec
			private ImageCodecInfo ProcessOneCodec (Guid clsid, Guid formatID, string mimeType) {
				ImageCodecInfo ici = new ImageCodecInfo ();
				ici.Clsid = clsid;
				ici.FormatID = formatID;
				ici.MimeType = mimeType;
				java.util.Iterator iter = null;
				try {
					iter = GetIterator (mimeType);
				}
				catch(Exception) {
					return null;
				}
				while (iter.hasNext ()) {
					spi.ImageReaderWriterSpi rw = GetNext (iter);

					ici.CodecName = rw.getDescription (java.util.Locale.getDefault ());
					//ici.DllName = null;
					foreach (string suffix in rw.getFileSuffixes ()) {
						if (ici.FilenameExtension != null)
							ici.FilenameExtension += ";";
						ici.FilenameExtension += "*."+suffix;
					}
					ici.Flags = ImageCodecFlags.Builtin|ImageCodecFlags.SupportBitmap;
					if (rw is spi.ImageReaderSpi)
						ici.Flags |= ImageCodecFlags.Decoder;

					if (rw is spi.ImageWriterSpi)
						ici.Flags |= ImageCodecFlags.Encoder;

					ici.FormatDescription = string.Join(";",
						rw.getFormatNames());
					try {
						ici.Version = (int)Convert.ToDouble(rw.getVersion ());
					}
					catch (Exception) {
						ici.Version = 1;
					}
					break;
				}
				return ici;
			}
			#endregion

			internal Hashtable Iterate () {
				// TBD: Insert Exception handling here
				NameValueCollection nvc = (NameValueCollection) System.Configuration.ConfigurationSettings
					.GetConfig ("mainsoft.drawing/codecs");
				Hashtable codecs = new Hashtable (10);
			
				for (int i=0; i<nvc.Count; i++) {
					Guid clsid = new Guid (nvc.GetKey (i));
					ImageFormat format = ClsidToImageFormat (clsid);
					ImageCodecInfo codec = ProcessOneCodec (clsid, format.Guid, nvc[i]);
					if ((codec != null) && (codec.FilenameExtension != null))
						codecs [clsid] = codec;
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

		private static ImageFormat MimeTypesToImageFormat (string [] mimeTypes) {
			foreach (ImageCodecInfo codec in Decoders.Values)
				for (int i=0; i<mimeTypes.Length; i++)
					if (codec.MimeType == mimeTypes [i])
						return ClsidToImageFormat (codec.Clsid);
			return null;
		}

		internal static ImageFormat ClsidToImageFormat (Guid clsid) {
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

		internal static Guid ImageFormatToClsid (ImageFormat format) {
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

		private FrameDimension FormatFrameDimesion {
			get {
				if (ImageFormat == null)
					return FrameDimension.Page;

				if (ImageFormat.Guid.Equals (ImageFormat.Bmp.Guid))
					return FrameDimension.Page;
				else if (ImageFormat.Guid.Equals (ImageFormat.Jpeg.Guid))
					return FrameDimension.Page;
				else if (ImageFormat.Guid.Equals (ImageFormat.Gif))
					return FrameDimension.Time;
				else if (ImageFormat.Guid.Equals (ImageFormat.Emf.Guid))
					return FrameDimension.Page;
				else if (ImageFormat.Guid.Equals (ImageFormat.Wmf.Guid))
					return FrameDimension.Page;
				else if (ImageFormat.Guid.Equals (ImageFormat.Tiff.Guid))
					return FrameDimension.Page;
				else if (ImageFormat.Guid.Equals (ImageFormat.Png.Guid))
					return FrameDimension.Page;
				else if (ImageFormat.Guid.Equals (ImageFormat.Icon.Guid))
					return FrameDimension.Resolution;
				else
					return FrameDimension.Page;
			}
		}

		#endregion
		
		#region Image read/write methods

		internal PlainImage ReadPlainImage() {
			awt.Image img = ReadImage( _currentFrame );
			if (img == null)
				return null;

			// its possible to fail to load thumbnails and metadata, but image is ok.
			awt.Image [] th = null;
#if THUMBNAIL_SUPPORTED
			try {
				th = ReadThumbnails( _currentFrame );
			}
			catch (Exception) {}
#endif
			
			XmlDocument md = null;
			imageio.metadata.IIOMetadata nativeMd = null;
			try {
				nativeMd = ReadImageMetadata( _currentFrame );
				md = ConvertImageMetadata( nativeMd );
			}
			catch (Exception) {}

			float [] resolution = GetResolution( md );

			PlainImage pi = new PlainImage( img, th, ImageFormat, resolution[0], resolution[1], FormatFrameDimesion );
			pi.NativeMetadata = nativeMd;
			return pi;
		}

		internal PlainImage ReadNextPlainImage() {
			_currentFrame++;
			return ReadPlainImage();
		}

		private awt.Image ReadImage(int frame) {
			if (NativeStream == null)
				throw new Exception("Input stream not specified");

			try {
				return NativeReader.read (frame);
			}
			catch (java.lang.IndexOutOfBoundsException) {
				return null;
			}
			catch (java.io.IOException ex) {
				throw new System.IO.IOException(ex.Message, ex);
			}
		}

#if THUMBNAIL_SUPPORTED
		private awt.Image [] ReadThumbnails(int frameIndex) {
			awt.Image [] thArray = null;

			try {
				if (NativeReader.readerSupportsThumbnails()) {
					int tmbNumber = NativeReader.getNumThumbnails(frameIndex);

					if (tmbNumber > 0) {
						thArray = new awt.Image[ tmbNumber ];

						for (int i = 0; i < tmbNumber; i++) {
							thArray[i] = NativeReader.readThumbnail(frameIndex, i);
						}
					}
				}
				return thArray;
			}
			catch (java.io.IOException ex) {
				throw new System.IO.IOException(ex.Message, ex);
			}
		}
#endif
		internal void WritePlainImage(PlainImageCollection pic) {
			if ((pic == null) || (pic.Count == 0))
				return;

			if (pic.Count == 1) {
				WritePlainImage( pic[0] );
				return;
			}

			try {
				if (NativeWriter.canWriteSequence ()) {
					NativeWriter.prepareWriteSequence (null);
					for (int i=0; i < pic.Count; i++) {
						imageio.IIOImage iio = GetIIOImageContainer( pic[i] );
						NativeWriter.writeToSequence (iio, null);
					}
					NativeWriter.endWriteSequence ();
				}
				else
					WritePlainImage( pic[0] );
			}
			catch (java.io.IOException ex) {
				throw new System.IO.IOException(ex.Message, ex);
			}
		}

		internal void WritePlainImage(PlainImage pi) {
			try {
				imageio.IIOImage iio = GetIIOImageContainer( pi );
				WriteImage( iio );
			}
			catch (java.io.IOException ex) {
				throw new System.IO.IOException(ex.Message, ex);
			}
		}

		private void WriteImage(imageio.IIOImage iio) {
			if (NativeStream == null)
				throw new Exception("Output stream not specified");

			NativeWriter.write( iio );
		}
		
		private imageio.IIOImage GetIIOImageContainer(PlainImage pi) {
			java.util.ArrayList al = null;
			
			// prepare thumbnails list
			if (pi.Thumbnails != null) {
				al = new java.util.ArrayList( pi.Thumbnails.Length );
				for (int i=0; i < pi.Thumbnails.Length; i++)
					al.add(pi.Thumbnails[i]);
			}

			// prepare IIOImage container
			if (pi.NativeImage is image.BufferedImage) {
				imageio.IIOImage iio = new javax.imageio.IIOImage(
					(image.BufferedImage)pi.NativeImage, al, null /*pi.NativeMetadata*/);
				return iio;
			}
			else
				// TBD: This codec is for raster formats only
				throw new NotSupportedException("Only raster formats are supported");
		}


		private imageio.metadata.IIOMetadata ReadImageMetadata(int frameIndex) {
			if (NativeStream == null)
				throw new Exception("Input stream not specified");

			try {
				imageio.metadata.IIOMetadata md = NativeReader.getImageMetadata( frameIndex );
				return md;
			}
			catch (java.io.IOException ex) {
				throw new System.IO.IOException(ex.Message, ex);
			}
		}

		#endregion

		#region Extra properties

		public ImageFormat ImageFormat {
			get { return _imageFormat; }
		}

		#endregion

		#region Metadata parse

		private float [] GetResolution(XmlDocument metaData) {
			if (metaData == null)
				return new float[]{0, 0};

			ResolutionConfigurationCollection rcc = 
				(ResolutionConfigurationCollection)
				ConfigurationSettings.GetConfig ("mainsoft.drawing/codecsmetadata");

			if (rcc == null)
				throw new ConfigurationException("Configuration section codecsmetadata not found");

			ResolutionConfiguration rc = rcc[ ImageFormat.ToString() ];

			if (rc == null)
				return new float[]{0, 0};

			// Horizontal resolution
			string xResPath = rc.XResPath;
			string xRes;

			if (xResPath == string.Empty)
				xRes = rc.XResDefault;
			else
				xRes = GetValueFromMetadata(metaData, xResPath);

			if ((xRes == null) || (xRes == string.Empty))
				xRes = rc.XResDefault;

			// Vertical resolution
			string yResPath = rc.YResPath;
			string yRes;

			if (yResPath == string.Empty)
				yRes = rc.YResDefault;
			else
				yRes = GetValueFromMetadata(metaData, yResPath);

			if ((yRes == null) || (yRes == string.Empty))
				yRes = rc.YResDefault;

			// Resolution units
			string resUnitsPath = rc.UnitsTypePath;
			string resUnitsType;

			if (resUnitsPath == string.Empty)
				resUnitsType = rc.UnitsTypeDefault;
			else
				resUnitsType = GetValueFromMetadata(metaData, resUnitsPath);

			if (resUnitsType == null)
				resUnitsType = rc.UnitsTypeDefault;

			// Unit scale
			string unitScale = rc.UnitsScale[resUnitsType].ToString();

			// Adjust resolution to its units
			float [] res = new float[2];
			res[0] = ParseFloatValue(xRes) * ParseFloatValue(unitScale);
			res[1] = ParseFloatValue(yRes) * ParseFloatValue(unitScale);

			return res;
		}

		private string GetValueFromMetadata(XmlDocument metaData, string path) {
			XmlNode n = metaData.SelectSingleNode(path);
			if (n == null)
				return null;

			return n.InnerText;
		}

		private XmlDocument ConvertImageMetadata(imageio.metadata.IIOMetadata metaData) {
			string [] formatNames = metaData.getMetadataFormatNames();
			dom.Element rootNode = (dom.Element) metaData.getAsTree(formatNames[0]);

			XmlDocument _metadataDocument = new XmlDocument();
			XmlConvert(rootNode, _metadataDocument);

			return _metadataDocument;
		}

		private void XmlConvert(dom.Node jNode, XmlNode nNode) {
			XmlDocument document = nNode.OwnerDocument;
			if (document == null)
				document = (XmlDocument)nNode;

			XmlNode n = null;
			switch (jNode.getNodeType()) {
				case 1 :
					n = document.CreateNode(XmlNodeType.Element, jNode.getNodeName(), jNode.getNamespaceURI());
					break;

				case 4 :
					n = document.CreateNode(XmlNodeType.CDATA, jNode.getNodeName(), jNode.getNamespaceURI());
					break;

				default:
					return;
			}
			//set value
			n.InnerText = jNode.getNodeValue();
			nNode.AppendChild( n );

			//copy attributes
			org.w3c.dom.NamedNodeMap nm = jNode.getAttributes();
			for (int i=0; i<nm.getLength(); i++) {
				XmlAttribute a = document.CreateAttribute( nm.item(i).getNodeName() );
				a.Value = nm.item(i).getNodeValue();
				n.Attributes.Append( a );
			}

			//copy childs
			org.w3c.dom.NodeList nl = jNode.getChildNodes();
			for (int i=0; i<nl.getLength(); i++) {
				XmlConvert(nl.item(i), n);
			}
		}

		protected virtual float ParseFloatValue(string strValue) {
			try {
				if ((strValue != null) && (strValue != "")) {
					int dividerPos = strValue.IndexOf("/");
				
					if (dividerPos < 0) {
						return float.Parse(strValue);
					} 
					else {
						return float.Parse(strValue.Substring( 0, dividerPos )) /
							float.Parse(strValue.Substring( dividerPos + 1 ));
					}
				}
				return float.NaN;
			}
			catch (Exception) {
				return float.NaN;
			}
		}

		#endregion

		#region IDisposable members

		public void Dispose() {
			if (NativeReader != null) {
				NativeReader.dispose();
				NativeReader = null;
			}

			if (NativeWriter != null) {
				NativeWriter.dispose();
				NativeWriter = null;
			}
		}

		#endregion

	}
}
