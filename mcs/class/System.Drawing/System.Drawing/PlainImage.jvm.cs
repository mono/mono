using System;
using System.Drawing.Imaging;
using System.Xml;

using Mainsoft.Drawing.Imaging;

using awt = java.awt;
using java.awt.image;
using imageio = javax.imageio;

namespace Mainsoft.Drawing.Imaging
{
	/// <summary>
	/// Summary description for PlainImage.
	/// </summary>
	public class PlainImage : ICloneable {

		#region Members

		awt.Image _nativeObject;
		awt.Image [] _thumbnails;
		FrameDimension _dimension;
		XmlDocument _metadata;
		imageio.metadata.IIOMetadata _nativeMetadata;
		ImageFormat _imageFormat;

		float _xResolution;
		float _yResolution;


		#endregion

		#region Constructors

		public PlainImage() {
		}

		public PlainImage(awt.Image image, awt.Image [] thumbnails, ImageFormat format, float xRes, float yRes, FrameDimension dimension) {
			_nativeObject = image;
			_thumbnails = thumbnails;
			_imageFormat = format;

			_xResolution = xRes;
			_yResolution = yRes;

			_dimension = dimension;
		}

		#endregion


		private awt.Image NativeObject {
			get { return _nativeObject; }
			set { _nativeObject = value; }
		}

		#region PlainImage properties

		public awt.Image NativeImage {
			get { return NativeObject; }
			set { NativeObject = value; }
		}

		internal imageio.metadata.IIOMetadata NativeMetadata {
			get { return _nativeMetadata; }
			set { _nativeMetadata = value; }
		}

		public XmlDocument Metadata {
			get { return _metadata; }
			set { _metadata = value; }
		}

		public ImageFormat ImageFormat {
			get { return _imageFormat; }
		}

		public FrameDimension Dimension {
			get { return _dimension; }
			set { _dimension = value; }
		}

		public awt.Image [] Thumbnails {
			get { return _thumbnails; }
		}

		public float HorizontalResolution {
			get { return _xResolution; }
			set { _xResolution = value; }
		}

		public float VerticalResolution {
			get { return _yResolution; }
			set { _yResolution = value; }
		}

		#endregion

		#region ICloneable members

		public object Clone() {
			return Clone(true);
		}

		public PlainImage Clone(bool cloneImage) {
			awt.Image img = NativeImage;
			awt.Image [] th = _thumbnails;

			if (cloneImage) {
				img = new BufferedImage(
					((BufferedImage)NativeObject).getColorModel(), 
					((BufferedImage)NativeObject).copyData(null), 
					((BufferedImage)NativeObject).isAlphaPremultiplied(), null);

				if (Thumbnails != null) {
					th = new java.awt.Image[ Thumbnails.Length ];
					for (int i=0; i < Thumbnails.Length; i++) {
						th[i] = new BufferedImage(
							((BufferedImage)Thumbnails[i]).getColorModel(), 
							((BufferedImage)Thumbnails[i]).copyData(null), 
							((BufferedImage)Thumbnails[i]).isAlphaPremultiplied(), null);
					}
				}
			}

			return new PlainImage( 
				img, 
				th, 
				ImageFormat, 
				HorizontalResolution, 
				VerticalResolution, 
				Dimension );
		}

		#endregion
	}
}
