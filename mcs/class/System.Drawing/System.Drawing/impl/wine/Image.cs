//
// System.Drawing.Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Alexandre Pigolkine(pigolkine@gmx.de)
//
// Many methods are still commented. I'll care about them when all necessary
// classes are implemented.
//
namespace System.Drawing {

	namespace Win32Impl {

		using System;
		using System.Runtime.Remoting;
		using System.Runtime.Serialization;
		using System.Drawing.Imaging;
		using System.IO;

		//[Serializable]
		//[ComVisible(true)]

		internal abstract class Image : MarshalByRefObject, IImage /*, ICloneable, ISerializable */ 
		{

			internal IntPtr nativeObject_;
			internal System.Drawing.Win32Impl.Graphics selectedIntoGraphics_ = null;

			protected Size imageSize_;
			// constructor
			public Image () {}
    
			// public methods
			// static
			public static Image FromFile (string filename)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			public static Image FromFile (string filename, bool useEmbeddedColorManagement)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			public static Bitmap FromHbitmap (IntPtr hbitmap)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}

			public static Bitmap FromHbitmap (IntPtr hbitmap, IntPtr hpalette)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			//	public static int GetPixelFormatSize (PixelFormat pixfmt)
			//	{
			//		// Fixme: implement me
			//		throw new NotImplementedException ();
			//	}
			//
			//	public static bool IsAlphaPixelFormat (PixelFormat pixfmt)
			//	{
			//		// Fixme: implement me
			//		throw new NotImplementedException ();
			//	}
			//	
			//	public static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
			//	{
			//		// Fixme: implement me
			//		throw new NotImplementedException ();
			//	}
			//	
			//	public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
			//    	{
			//		// Fixme: implement me
			//		throw new NotImplementedException ();
			//	}

			// non-static
			public RectangleF GetBounds (ref GraphicsUnit pageUnit)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			//public EncoderParameters GetEncoderParameterList(Guid encoder);
			//public int GetFrameCount(FrameDimension dimension);
			//public PropertyItem GetPropertyItem(int propid);
			/*
			  public Image GetThumbnailImage(int thumbWidth, int thumbHeight,
			  Image.GetThumbnailImageAbort callback,
			  IntPtr callbackData);
			*/
	
			public void RemovePropertyItem (int propid)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			public void RotateFlip (RotateFlipType rotateFlipType)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}

			public void Save (string filename)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}

			public void Save(Stream stream, ImageFormat format)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
			
			public void Save(string filename, ImageFormat format)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
			
			//public void Save(Stream stream, ImageCodecInfo encoder,
			//                 EncoderParameters encoderParams);
			//public void Save(string filename, ImageCodecInfo encoder,
			//                 EncoderParameters encoderParams);
			//public void SaveAdd(EncoderParameters_ encoderParams);
			//public void SaveAdd(Image image, EncoderParameters_ encoderParams);
			//public int SelectActiveFrame(FrameDimension dimension, int frameIndex);
			//public void SetPropertyItem(PropertyItem propitem);

			// destructor
			~Image() {}

			// properties
			public int Flags 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			public Guid[] FrameDimensionsList 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			public int Height 
			{
				get 
				{
					return imageSize_.Height;
				}
			}
	
			public float HorizontalResolution 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			//	public ColorPalette Palette {
			//		get {
			//			throw new NotImplementedException ();
			//		}
			//		set {
			//			throw new NotImplementedException ();
			//		}
			//	}
	
			public SizeF PhysicalDimension 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			public PixelFormat PixelFormat {
				get {
					throw new NotImplementedException ();
				}
			}
	
			public int[] PropertyIdList 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			//public PropertyItem[] PropertyItems {get;}
			//public ImageFormat RawFormat {get;}

			public Size Size 
			{
				get 
				{
					return imageSize_;
				}
			}
	
			public float VerticalResolution 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			public int Width 
			{
				get 
				{
					return imageSize_.Width;
				}
			}
			[MonoTODO]
			public void Dispose ()
			{
				throw new NotImplementedException ();
			}

		}
	}
}
