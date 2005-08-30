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

namespace System.Drawing.Drawing2D 
{
	/// <summary>
	/// Summary description for GraphicsState.
	/// </summary>
	public sealed class GraphicsState : MarshalByRefObject
	{
		// Constructor
		internal GraphicsState()
		{
		}

		internal GraphicsState(Graphics graphics, bool resetState) : base()
		{
			this.SaveState(graphics);
			if (resetState)
				this.ResetState(graphics);
		}

		private CompositingMode _compositingMode;
		private CompositingQuality _compositingQuality;
		private Region _clip;
		private InterpolationMode _interpolationMode;
		private float _pageScale = 0;
		private GraphicsUnit _pageUnit;
		private PixelOffsetMode _pixelOffsetMode;
		private Point _renderingOrigin;
		private SmoothingMode _smoothingMode;
		private Matrix _transform = null;
		private int _textContrast;
		private TextRenderingHint _textRenderingHint;

		// additional transform in case that new container has one
		private Matrix _containerTransform;

		internal Matrix ContainerTransfrom
		{
			get {return _containerTransform;}
			set {_containerTransform = value;}
		}

		private void SaveState(Graphics g)
		{
			if (g != null)
			{
				try
				{
					_compositingMode = g.CompositingMode;
					_compositingQuality = g.CompositingQuality;
					_clip = g.Clip.Clone();
					_interpolationMode = g.InterpolationMode;
					_pageScale = g.PageScale;
					_pageUnit = g.PageUnit;
					_pixelOffsetMode = g.PixelOffsetMode;
			
					// FIXME: render orign is not implemented yet
					//_renderingOrigin = new Point( g.RenderingOrigin.X, g.RenderingOrigin.Y );

					_smoothingMode = g.SmoothingMode;
					_transform = g.Transform.Clone();
					_textContrast = g.TextContrast;
					_textRenderingHint = g.TextRenderingHint;
				}
				finally
				{
				}
			}
		}

		internal void RestoreState(Graphics g)
		{
			if (g != null)
			{
				try
				{
					g.CompositingMode = _compositingMode;
					g.CompositingQuality = _compositingQuality;
					g.Clip = _clip;
					g.InterpolationMode = _interpolationMode;
					g.PageScale = _pageScale;
					g.PageUnit = _pageUnit;
					g.PixelOffsetMode = _pixelOffsetMode;
			
					// FIXME: render orign is not implemented yet
					//g.RenderingOrigin = new Point( _renderingOrigin.X, _renderingOrigin.Y );

					g.SmoothingMode = _smoothingMode;
					g.Transform = _transform;
					g.TextContrast = _textContrast;
					g.TextRenderingHint = _textRenderingHint;
				}
				finally
				{
				}
			}
		}

		private void ResetState(Graphics g)
		{
			if (g != null)
			{
				try
				{
					g.CompositingMode = CompositingMode.SourceOver;
					g.CompositingQuality = CompositingQuality.Default;
					g.Clip = Region.InfiniteRegion;
					g.InterpolationMode = InterpolationMode.Bilinear;
					g.PageScale = 1.0f;
					g.PageUnit = GraphicsUnit.Display;
					g.PixelOffsetMode = PixelOffsetMode.Default;
					
					// FIXME: render orign is not implemented yet
					//g.RenderingOrigin = new Point(0, 0);

					g.SmoothingMode = SmoothingMode.None;
					g.ResetTransform();
					g.TextContrast = 4;
					g.TextRenderingHint = TextRenderingHint.SystemDefault;
				}
				finally
				{
				}
			}
		}
	}
}
