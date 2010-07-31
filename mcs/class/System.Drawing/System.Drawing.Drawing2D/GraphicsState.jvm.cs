//
// System.Drawing.Drawing2D.ExtendedGraphicsState.jvm.cs
//
// Author:
//   Vladimir Krasnov (vladimirk@mainsoft.com)
//
// Copyright (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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
using System.Drawing.Text;
using geom = java.awt.geom;
using awt = java.awt;

namespace System.Drawing.Drawing2D 
{
	/// <summary>
	/// Summary description for GraphicsState.
	/// </summary>
	public sealed class GraphicsState : MarshalByRefObject
	{
		readonly CompositingMode _compositingMode;
		readonly CompositingQuality _compositingQuality;
		readonly Region _clip;
		readonly awt.Shape _baseClip;
		readonly InterpolationMode _interpolationMode;
		readonly float _pageScale;
		readonly GraphicsUnit _pageUnit;
		readonly PixelOffsetMode _pixelOffsetMode;
		readonly Point _renderingOrigin;
		readonly SmoothingMode _smoothingMode;
		readonly int _textContrast;
		readonly TextRenderingHint _textRenderingHint;

		// additional transform in case that new container has one
		readonly Matrix _transform;
		readonly Matrix _baseTransform;

		GraphicsState _next = null;

		internal GraphicsState(Graphics graphics, bool resetState) 
			: this(graphics, Matrix.IdentityTransform, resetState) {}

		internal GraphicsState Next {
			get {
				return _next;
			}
			set {
				_next = value;
			}
		}

		internal GraphicsState(Graphics graphics, Matrix matrix, bool resetState)
		{
			_compositingMode = graphics.CompositingMode;
			_compositingQuality = graphics.CompositingQuality;
			_clip = graphics.ScaledClip;
			_baseClip = graphics.NativeObject.getClip();
			_interpolationMode = graphics.InterpolationMode;
			_pageScale = graphics.PageScale;
			_pageUnit = graphics.PageUnit;
			_pixelOffsetMode = graphics.PixelOffsetMode;
			
			// FIXME: render orign is not implemented yet
			//_renderingOrigin = new Point( g.RenderingOrigin.X, g.RenderingOrigin.Y );

			_smoothingMode = graphics.SmoothingMode;
			_transform = graphics.Transform;
			_baseTransform = graphics.BaseTransform;

			_textContrast = graphics.TextContrast;
			_textRenderingHint = graphics.TextRenderingHint;

			if (resetState)
				ResetState(graphics, matrix);
		}

		internal void RestoreState(Graphics graphics)
		{
			graphics.CompositingMode = _compositingMode;
			graphics.CompositingQuality = _compositingQuality;
			graphics.ScaledClip = _clip;
			graphics.InterpolationMode = _interpolationMode;
			graphics.PageScale = _pageScale;
			graphics.PageUnit = _pageUnit;
			graphics.PixelOffsetMode = _pixelOffsetMode;
	
			// FIXME: render orign is not implemented yet
			//graphics.RenderingOrigin = new Point( _renderingOrigin.X, _renderingOrigin.Y );

			graphics.SmoothingMode = _smoothingMode;
			graphics.Transform = _transform;
			graphics.BaseTransform = _baseTransform;
			graphics.TextContrast = _textContrast;
			graphics.TextRenderingHint = _textRenderingHint;

			// must be set after the base transform is restored
			graphics.NativeObject.setClip(_baseClip);
		}

		void ResetState(Graphics graphics, Matrix matrix)
		{
			//should be set before the base transform is changed
			if (_baseClip == null)
				graphics.IntersectScaledClipWithBase(graphics.VisibleShape);

			graphics.CompositingMode = CompositingMode.SourceOver;
			graphics.CompositingQuality = CompositingQuality.Default;
			graphics.ScaledClip = Region.InfiniteRegion;
			graphics.InterpolationMode = InterpolationMode.Bilinear;
			graphics.PageScale = 1.0f;
			graphics.PageUnit = GraphicsUnit.Display;
			graphics.PixelOffsetMode = PixelOffsetMode.Default;
			
			// FIXME: render orign is not implemented yet
			//graphics.RenderingOrigin = new Point(0, 0);

			graphics.SmoothingMode = SmoothingMode.None;
			graphics.ResetTransform();
			graphics.PrependBaseTransform(Graphics.GetFinalTransform(_transform.NativeObject, _pageUnit, _pageScale));
			graphics.PrependBaseTransform(matrix.NativeObject);
			graphics.TextContrast = 4;
			graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
		}

		internal void RestoreBaseClip(Graphics graphics) {
			graphics.NativeObject.setClip(_baseClip);
		}
	}
}
