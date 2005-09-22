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

using awt = java.awt;
using geom = java.awt.geom;
using image = java.awt.image;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for TextureBrush.
	/// </summary>
	public sealed class TextureBrush : Brush
	{
		awt.TexturePaint _nativeObject;

		protected override java.awt.Paint NativeObject {
			get {
				return _nativeObject;
			}
		}


		WrapMode _wrapMode;

		TextureBrush (awt.TexturePaint ptr)
		{
			_nativeObject = ptr;
		}

		public TextureBrush (Image image) : this (image, WrapMode.Tile)
		{
		}

		public TextureBrush (Image image, Rectangle dstRect)
		{
		}

		public TextureBrush (Image image, RectangleF dstRect)
		{
			// FIXME: check if not metafile
			_nativeObject = new awt.TexturePaint((image.BufferedImage)image.NativeObject.CurrentImage.NativeImage,
				new geom.Rectangle2D.Float((float)dstRect.X,(float)dstRect.Y,(float)dstRect.Width,
				(float)dstRect.Height));
		}

		public TextureBrush (Image image, WrapMode wrapMode)
		{
			// FIXME: check if not metafile
			// TBD: WRAP MODE
			_nativeObject = new awt.TexturePaint((image.BufferedImage)image.NativeObject.CurrentImage.NativeImage,
				new geom.Rectangle2D.Float(0,0,1,1));
		}

		public TextureBrush (Image image, Rectangle dstRect, ImageAttributes imageAttr)
		{
			throw new NotImplementedException();
		}

		public TextureBrush (Image image, RectangleF dstRect, ImageAttributes imageAttr)
		{
			throw new NotImplementedException();
		}

		public TextureBrush (Image image, WrapMode wrapMode, Rectangle dstRect)
		{
			// FIXME: check if not metafile
			// TBD:WRAP MODE
			_nativeObject = new awt.TexturePaint((image.BufferedImage)image.NativeObject.CurrentImage.NativeImage,
				new geom.Rectangle2D.Float ((float)dstRect.X,(float)dstRect.Y,(float)dstRect.Width,(float)dstRect.Height));
		}

		public TextureBrush (Image image, WrapMode wrapMode, RectangleF dstRect)
		{
			// FIXME: check if not metafile
			//TBD:WRAP MODE
			_nativeObject = new awt.TexturePaint((image.BufferedImage)image.NativeObject.CurrentImage.NativeImage,
				new geom.Rectangle2D.Float((float)dstRect.X,(float)dstRect.Y,(float)dstRect.Width,
				(float)dstRect.Height));
		}

		// properties
		public Image Image {
			get {
				return Image.ImageFromNativeImage(((awt.TexturePaint)NativeObject).getImage(),
					ImageFormat.Bmp);
			}
		}

		public Matrix Transform {
			get {					
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public WrapMode WrapMode {
			get {
				return _wrapMode;
			}
			set {
				_wrapMode = value;
			}
		}

		// public methods

		public override object Clone ()
		{
			throw new NotImplementedException();
		}

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			throw new NotImplementedException();
		}

		public void ResetTransform ()
		{
			throw new NotImplementedException();
		}

		public void RotateTransform (float angle)
		{
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{
			throw new NotImplementedException();
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			throw new NotImplementedException();
		}

		public void TranslateTransform (float dx, float dy)
		{
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{
			throw new NotImplementedException();
		}
	}
}
