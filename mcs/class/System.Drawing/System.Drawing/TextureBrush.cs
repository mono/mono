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
			image = Image;
			matrix = Transform;
		}

		public TextureBrush (Image image) : this (image, WrapMode.Tile)
		{
		}

		public TextureBrush (Image image, Rectangle dstRect)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTextureIAI (image.nativeObject, IntPtr.Zero, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public TextureBrush (Image image, RectangleF dstRect)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTextureIA (image.nativeObject, IntPtr.Zero, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public TextureBrush (Image image, WrapMode wrapMode)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTexture (image.nativeObject, wrapMode, out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public TextureBrush (Image image, Rectangle dstRect, ImageAttributes imageAttr)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTextureIAI (image.nativeObject, imageAttr.NativeObject, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public TextureBrush (Image image, RectangleF dstRect, ImageAttributes imageAttr)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTextureIA (image.nativeObject, imageAttr.NativeObject, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public TextureBrush (Image image, WrapMode wrapMode, Rectangle dstRect)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTexture2I (image.nativeObject, wrapMode, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public TextureBrush (Image image, WrapMode wrapMode, RectangleF dstRect)
		{
			this.image = image;
			Status status = GDIPlus.GdipCreateTexture2 (image.nativeObject, wrapMode, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		// properties

		public Image Image {
			get {
				if (image == null) {
					IntPtr img;
					Status status = GDIPlus.GdipGetTextureImage (nativeObject, out img);
					GDIPlus.CheckStatus (status);
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
					GDIPlus.CheckStatus (status);
					matrix = new Matrix (m);
				}
				return matrix;
			}
			set {
				Status status = GDIPlus.GdipSetTextureTransform (nativeObject, value.nativeMatrix);
				GDIPlus.CheckStatus (status);
				matrix = value;
			}
		}

		public WrapMode WrapMode {
			get {
				WrapMode mode = WrapMode.Tile;
				Status status = GDIPlus.GdipGetTextureWrapMode (nativeObject, out mode);
				GDIPlus.CheckStatus (status);
				return mode;
			}
			set {
				Status status = GDIPlus.GdipSetTextureWrapMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		// public methods

		public override object Clone ()
		{
			return new TextureBrush (nativeObject);
		}

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			Status status = GDIPlus.GdipMultiplyTextureTransform (nativeObject, matrix.nativeMatrix, order);
			GDIPlus.CheckStatus (status);
		}

		public void ResetTransform ()
		{
			Status status = GDIPlus.GdipResetTextureTransform (nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void RotateTransform (float angle)
		{
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{
			Status status = GDIPlus.GdipRotateTextureTransform (nativeObject, angle, order);
			GDIPlus.CheckStatus (status);
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			Status status = GDIPlus.GdipScaleTextureTransform (nativeObject, sx, sy, order);
			GDIPlus.CheckStatus (status);
		}

		public void TranslateTransform (float dx, float dy)
		{
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{
			Status status = GDIPlus.GdipTranslateTextureTransform (nativeObject, dx, dy, order);
			GDIPlus.CheckStatus (status);
		}
	}
}
