//
// System.Drawing.TextureBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002 Ximian, Inc
// (C) 2004 Novell, Inc.
//

using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for TextureBrush.
	/// </summary>
	public sealed class TextureBrush : Brush
	{
		Image image;
		Matrix matrix;

		internal TextureBrush (IntPtr ptr) : base (ptr)
		{
			// get image from IntPtr
			// image could be Bitmap or Metafile
			IntPtr img;
			Status status = GDIPlus.GdipGetTextureImage (nativeObject, out img);
			if (status != Status.Ok)
				throw GetException (status);
			image = new Bitmap (img);
			matrix = new Matrix ();
		}

		public TextureBrush (Image image) : this (image, WrapMode.Tile)
		{
		}

		public TextureBrush (Image image, Rectangle dstRect)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTextureIAI (image.nativeObject, IntPtr.Zero, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			if (status != Status.Ok)
				throw GetException (status);

			matrix = new Matrix ();
		}

		public TextureBrush (Image image, RectangleF dstRect)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTextureIA (image.nativeObject, IntPtr.Zero, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			if (status != Status.Ok)
				throw GetException (status);

			matrix = new Matrix ();
		}

		public TextureBrush (Image image, WrapMode wrapMode)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTexture (image.nativeObject, wrapMode, out nativeObject);
			if (status != Status.Ok)
				throw GetException (status);

			matrix = new Matrix ();
		}

		public TextureBrush (Image image, Rectangle dstRect, ImageAttributes imageAttr)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTextureIAI (image.nativeObject, imageAttr.NativeObject, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			if (status != Status.Ok)
				throw GetException (status);

			matrix = new Matrix ();
		}

		public TextureBrush (Image image, RectangleF dstRect, ImageAttributes imageAttr)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTextureIA (image.nativeObject, imageAttr.NativeObject, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			if (status != Status.Ok)
				throw GetException (status);

			matrix = new Matrix ();
		}

		public TextureBrush (Image image, WrapMode wrapMode, Rectangle dstRect)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTexture2I (image.nativeObject, wrapMode, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			if (status != Status.Ok)
				throw GetException (status);

			matrix = new Matrix ();
		}

		public TextureBrush (Image image, WrapMode wrapMode, RectangleF dstRect)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTexture2 (image.nativeObject, wrapMode, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			if (status != Status.Ok)
				throw GetException (status);

			matrix = new Matrix ();
		}

		// properties

		public Image Image {
			get {
				if (image == null) {
					IntPtr img;
					Status status = GDIPlus.GdipGetTextureImage (nativeObject, out img);
					if (status != Status.Ok)
						throw GetException (status);
					image = new Bitmap (img);
				}
				return image;
			}
		}

		public Matrix Transform {
			get {
				if (matrix == null) {
					IntPtr m;
					Status status =	GDIPlus.GdipGetTextureTransform (nativeObject, out m);
					if (status == Status.Ok)
						matrix = new Matrix (m);
					else
						throw GetException (status);
				}
				return matrix;
			}
			set {
				Status status = GDIPlus.GdipSetTextureTransform (nativeObject, value.nativeMatrix);
				if (status != Status.Ok)
					throw GetException (status);
			}
		}

		public WrapMode WrapMode {
			get {
				WrapMode mode = WrapMode.Tile;
				Status status = GDIPlus.GdipGetTextureWrapMode (nativeObject, out mode);
				if (status == Status.Ok)
					return mode;
				else
					throw GetException (status);
			}
			set {
				Status status = GDIPlus.GdipSetTextureWrapMode (nativeObject, value);
				if (status != Status.Ok)
					throw GetException (status);
			}
		}

		// public methods

		public override object Clone ()
		{
			return new TextureBrush (nativeObject);
		}

		Exception GetException (Status status)
		{
			String message;

			switch (status) {
				// TODO: Test and add more status code mappings here
				case Status.GenericError:
					message = String.Format ("Generic Error.");
					return new Exception (message);

				case Status.InvalidParameter:
					message = String.Format ("Invalid Parameter.");
					return new ArgumentException (message);

				case Status.OutOfMemory:
					message = String.Format ("Out of memory.");
					return new OutOfMemoryException (message);

				case Status.ObjectBusy:
					message = String.Format ("Object busy.");
					return new MemberAccessException (message);

				case Status.InsufficientBuffer:
					message = String.Format ("Insufficient buffer.");
					return new IO.InternalBufferOverflowException (message);

				case Status.PropertyNotSupported:
					message = String.Format ("Property not supported.");
					return new NotSupportedException (message);

				case Status.FileNotFound:
					message = String.Format ("File not found.");
					return new IO.FileNotFoundException (message);

				case Status.AccessDenied:
					message = String.Format ("Access denied.");
					return new UnauthorizedAccessException (message);

				case Status.UnknownImageFormat:
					message = String.Format ("Unknown image format.");
					return new NotSupportedException (message);

				case Status.NotImplemented:
					message = String.Format ("Feature not implemented.");
					return new NotImplementedException (message);

				default:
					return new Exception ("Unknown Error.");
			}
		}
	}
}
