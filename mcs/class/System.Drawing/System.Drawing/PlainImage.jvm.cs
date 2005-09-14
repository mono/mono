using System;
using System.Drawing.Imaging;
using System.Xml;

using Mainsoft.Drawing.Imaging;

using awt = java.awt;
using java.awt.image;

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
		ImageFormat _imageFormat;

		float _xResolution;
		float _yResolution;


		#endregion

		#region Constructors

		public PlainImage() {
		}

		public PlainImage(awt.Image image, awt.Image [] thumbnails, ImageFormat format, float xRes, float yRes, FrameDimension dimension, XmlDocument metadata) {
			_nativeObject = image;
			_thumbnails = thumbnails;
			_imageFormat = format;

			_xResolution = xRes;
			_yResolution = yRes;

			_dimension = dimension;
			_metadata = metadata;
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
			
			awt.Image img = new BufferedImage(
				((BufferedImage)NativeObject).getColorModel(), 
				((BufferedImage)NativeObject).copyData(null), 
				((BufferedImage)NativeObject).isAlphaPremultiplied(), null);

			awt.Image [] th = null;
			if (Thumbnails != null) {
				th = new java.awt.Image[ Thumbnails.Length ];
				for (int i=0; i < Thumbnails.Length; i++) {
					th[i] = new BufferedImage(
						((BufferedImage)Thumbnails[i]).getColorModel(), 
						((BufferedImage)Thumbnails[i]).copyData(null), 
						((BufferedImage)Thumbnails[i]).isAlphaPremultiplied(), null);
				}
			}

			return new PlainImage( 
				img, 
				th, 
				ImageFormat, 
				HorizontalResolution, 
				VerticalResolution, 
				Dimension, 
				_metadata );
		}

		#endregion
	}
}
