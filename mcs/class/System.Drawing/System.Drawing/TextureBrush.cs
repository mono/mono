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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for TextureBrush.
	/// </summary>
	public sealed class TextureBrush : Brush
	{
		private Image image;

		internal TextureBrush (IntPtr ptr) : base (ptr)
		{
			// get image from IntPtr
			// image could be Bitmap or Metafile
			image = Image;
		}

		public TextureBrush (Image image) : this (image, WrapMode.Tile)
		{
		}

		public TextureBrush (Image image, Rectangle dstRect)
		{
			lock (this)
			{
				this.image = image;
				Status status = GDIPlus.GdipCreateTextureIAI (image.nativeObject, IntPtr.Zero, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
				GDIPlus.CheckStatus (status);
			}
		}

		public TextureBrush (Image image, RectangleF dstRect)
		{
			lock (this)
			{			
				this.image = image;
				Status status = GDIPlus.GdipCreateTextureIA (image.nativeObject, IntPtr.Zero, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
				GDIPlus.CheckStatus (status);
			}
		}

		public TextureBrush (Image image, WrapMode wrapMode)
		{
			lock (this)
			{			
				this.image = image;
				Status status = GDIPlus.GdipCreateTexture (image.nativeObject, wrapMode, out nativeObject);
				GDIPlus.CheckStatus (status);
			}
		}

		public TextureBrush (Image image, Rectangle dstRect, ImageAttributes imageAttr)
		{
			lock (this)
			{			
				this.image = image;
				Status status = GDIPlus.GdipCreateTextureIAI (image.nativeObject, imageAttr.NativeObject, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
				GDIPlus.CheckStatus (status);
			}
		}

		public TextureBrush (Image image, RectangleF dstRect, ImageAttributes imageAttr)
		{
			lock (this)
			{
				this.image = image;
				Status status = GDIPlus.GdipCreateTextureIA (image.nativeObject, imageAttr.NativeObject, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
				GDIPlus.CheckStatus (status);
			}
		}

		public TextureBrush (Image image, WrapMode wrapMode, Rectangle dstRect)
		{
			lock (this)
			{			
				this.image = image;
				Status status = GDIPlus.GdipCreateTexture2I (image.nativeObject, wrapMode, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
				GDIPlus.CheckStatus (status);
			}
		}

		public TextureBrush (Image image, WrapMode wrapMode, RectangleF dstRect)
		{
			lock (this)
			{			
				this.image = image;
				Status status = GDIPlus.GdipCreateTexture2 (image.nativeObject, wrapMode, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out nativeObject);
				GDIPlus.CheckStatus (status);
			}
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
				Matrix matrix = new Matrix ();
				Status status =	GDIPlus.GdipGetTextureTransform (nativeObject, matrix.nativeMatrix);
				GDIPlus.CheckStatus (status);

				return matrix;
			}
			set {
				Status status = GDIPlus.GdipSetTextureTransform (nativeObject, value.nativeMatrix);
				GDIPlus.CheckStatus (status);
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
			IntPtr clonePtr;
			Status status = GDIPlus.GdipCloneBrush (nativeObject, out clonePtr);
			GDIPlus.CheckStatus (status);

			TextureBrush clone = new TextureBrush (clonePtr);
			if (image != null)
				clone.image = (Image) image.Clone ();
			
			return clone;
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
