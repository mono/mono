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

namespace System.Drawing {
	/// <summary>
	/// Summary description for TextureBrush.
	/// </summary>
	public sealed class TextureBrush : Brush {
		readonly awt.TexturePaint _nativeObject;
		RectangleF _sourceRectangle;
		Image _texture = null;
		WrapMode _wrapMode;

		protected override java.awt.Paint NativeObject {
			get {
				return _nativeObject;
			}
		}

		#region ctors

		public TextureBrush (Image image) : this (image, WrapMode.Tile) {
		}

		public TextureBrush (Image image, WrapMode wrapMode) : 
			this( image, wrapMode, new RectangleF(0, 0, image.Width, image.Height )){
		}

		public TextureBrush (Image image, Rectangle dstRect) : 
			this( image, WrapMode.Tile, dstRect ) {
		}

		public TextureBrush (Image image, RectangleF dstRect) : 
			this( image, WrapMode.Tile, dstRect ) {
		}

		[MonoTODO]
		public TextureBrush (Image image, Rectangle dstRect, ImageAttributes imageAttr) : this( image, dstRect ) {
			// TBD: Implement ImageAttributes
		}

		[MonoTODO]
		public TextureBrush (Image image, RectangleF dstRect, ImageAttributes imageAttr) : this( image, dstRect ) {
			// TBD: Implement ImageAttributes
		}

		public TextureBrush (Image image, WrapMode wrapMode, Rectangle dstRect) :
			this( image, wrapMode, new RectangleF(dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height )){
		}

		[MonoTODO]
		public TextureBrush (Image image, WrapMode wrapMode, RectangleF dstRect) {
			// TBD: check if not metafile
			_sourceRectangle = dstRect;
			_texture = (Image)((Bitmap)image).Clone(dstRect, image.PixelFormat);
			_wrapMode = wrapMode;

			if (wrapMode != Drawing2D.WrapMode.Tile)
				image = CreateWrappedImage(_texture, wrapMode);
			else
				image = _texture;

			_nativeObject = new awt.TexturePaint((image.BufferedImage)image.NativeObject.CurrentImage.NativeImage,
				new geom.Rectangle2D.Float(0, 0, image.Width, image.Height));
		}

		#endregion

		#region CreateWrappedImage

		private Image CreateWrappedImage(Image image, WrapMode wrapMode) {
			Image b = null;
			Graphics g = null;

			switch (wrapMode) {
				case Drawing2D.WrapMode.TileFlipX :
					b = new Bitmap(image.Width * 2, image.Height);
					g = Graphics.FromImage( b );
					g.DrawImage(image, new Matrix());
					g.DrawImage(image, new Matrix(-1, 0, 0, 1, image.Width * 2 - 1, 0));
					break;
				case Drawing2D.WrapMode.TileFlipY :
					b = new Bitmap(image.Width, image.Height * 2);
					g = Graphics.FromImage( b );
					g.DrawImage(image, new Matrix());
					g.DrawImage(image, new Matrix(1, 0, 0, -1, 0, image.Height * 2 - 1));
					break;
				case Drawing2D.WrapMode.TileFlipXY :
					b = new Bitmap(image.Width * 2, image.Height * 2);
					g = Graphics.FromImage( b );
					g.DrawImage(image, new Matrix());
					g.DrawImage(image, new Matrix(-1, 0, 0, 1, image.Width * 2 - 1, 0));
					g.DrawImage(image, new Matrix(1, 0, 0, -1, 0, image.Height * 2 - 1));
					g.DrawImage(image, new Matrix(-1, 0, 0, -1, image.Width * 2 - 1, image.Height * 2 - 1));
					break;
				case Drawing2D.WrapMode.Clamp :
					// TBD: Implement WrapMode.Clamp
					return image;
				default : 
					b = image;
					break;
			}

			return b;
		}

		#endregion

		#region properties

		public Image Image {
			get {
				return (Image)_texture.Clone();
			}
		}

		public Matrix Transform {
			get {					
				return BrushTransform;
			}
			set {
				BrushTransform = value;
			}
		}

		[MonoTODO]
		public WrapMode WrapMode {
			get {
				return _wrapMode;
			}
			set {
				_wrapMode = value;
			}
		}

		#endregion

		#region public methods

		public override object Clone () {
			TextureBrush copy = (TextureBrush)InternalClone();

			if (_texture != null)
				copy._texture = (Image)_texture.Clone();

			return copy;
		}

		public void MultiplyTransform (Matrix matrix) {
			base.BrushMultiplyTransform( matrix );
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order) {
			base.BrushMultiplyTransform( matrix, order );
		}

		public void ResetTransform () {
			base.BrushResetTransform();
		}

		public void RotateTransform (float angle) {
			base.BrushRotateTransform( angle );
		}

		public void RotateTransform (float angle, MatrixOrder order) {
			base.BrushRotateTransform( angle, order );
		}

		public void ScaleTransform (float sx, float sy) {
			base.BrushScaleTransform( sx, sy );
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order) {
			base.BrushScaleTransform( sx, sy, order );
		}

		public void TranslateTransform (float dx, float dy) {
			base.BrushTranslateTransform( dx, dy );
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order) {
			base.BrushTranslateTransform( dx, dy, order );
		}

		#endregion
	}
}
