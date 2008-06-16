//
// System.Drawing.Test.LineLayout.jvm.cs
//
// Author:
// Konstantin Triger <kostat@mainsoft.com>
//
// Copyright (C) 2005 Mainsoft Corporation, (http://www.mainsoft.com)
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

using font = java.awt.font;
using text = java.text;
using awt = java.awt;
using geom = java.awt.geom;

namespace System.Drawing.Text {
	
	internal sealed class LineLayout {

		#region Fields

		readonly font.TextLayout _layout;

		readonly float _accumulatedHeight;
		readonly TextLineIterator _lineIter;

		#endregion

		#region ctor

		internal LineLayout(font.TextLayout layout,
			TextLineIterator lineIter,
			float accumulatedHeight) {

			_layout = layout;
			_lineIter = lineIter;
			_accumulatedHeight = accumulatedHeight;
		}

		#endregion

		#region Properties

		internal float AccumulatedHeight {
			get { return _accumulatedHeight; }
		}

		internal float MeasureWidth {
			get {
				return _lineIter.PadWidth (Width);
			}
		}

		internal float WidthPadding {
			get {
				return _lineIter.PadWidth (Width) - Width;
			}
		}		

		internal int CharacterCount {
			get { return _layout.getCharacterCount(); }
		}

		internal float Ascent {
			get { return _layout.getAscent(); }
		}

		internal float Descent {
			get { return _layout.getDescent(); }
		}

		public float Leading {
			get { return _layout.getLeading(); }
		}

		internal float NativeY {
			get {
				if (_lineIter.Format.IsVertical) {
					float height = _lineIter.Height;
					if (float.IsPositiveInfinity(height))
						height = 0;
					switch (_lineIter.Format.Alignment) {
						case StringAlignment.Center:
							return (height - Width) / 2;
						case StringAlignment.Far:
							return height - _layout.getVisibleAdvance () - WidthPadding;
						default:
							return WidthPadding;
					}
				}
				else
					return AccumulatedHeight + Ascent;
			}
		}

		internal float NativeX {
			get {
				float width = _lineIter.Width;
				if (float.IsPositiveInfinity(width))
					width = 0;
				if (_lineIter.Format.IsVertical)
					return (_lineIter.Format.IsRightToLeft) ?
						width - AccumulatedHeight - Ascent :
						AccumulatedHeight + Leading + Descent;					
				else {
					float xOffset;
					switch ( _lineIter.Format.Alignment) {
						case StringAlignment.Center:
							xOffset = (width - Width) / 2;
							break;
						case StringAlignment.Far:
							if (_lineIter.Format.IsRightToLeft)
								xOffset = WidthPadding/2;
							else
								xOffset = width - _layout.getVisibleAdvance () - WidthPadding/2;
							break;
						default:
							if (_lineIter.Format.IsRightToLeft)
								xOffset = width - _layout.getVisibleAdvance () - WidthPadding/2;
							else
								xOffset = WidthPadding / 2;
							break;
					}

					return xOffset;
				}
			}
		}

		internal float Height {
			get {
				return Ascent + Descent + Leading;
			}
		}

		internal float Width {
			get {
				if (_lineIter.Format.MeasureTrailingSpaces)
					if (!(_lineIter.Format.IsRightToLeft ^ 
						(_lineIter.Format.Alignment == StringAlignment.Far)))
						return _layout.getAdvance();
					
				return _layout.getVisibleAdvance();
			}
		}

		#endregion

		#region Methods

		internal void Draw(awt.Graphics2D g2d, float x, float y) {
			if (_lineIter.Format.IsVertical)
				_layout.draw (g2d, y + NativeY, -(x + NativeX));
			else
				_layout.draw(g2d, x + NativeX, y + NativeY );
		}

		internal awt.Shape GetOutline(float x, float y) {
			geom.AffineTransform t = (geom.AffineTransform) _lineIter.Transform.clone();

			if (_lineIter.Format.IsVertical)
				t.translate(y + NativeY, -(x + NativeX));
			else
				t.translate(x + NativeX, y + NativeY);

			return _layout.getOutline(t);
		}

		#endregion
	}

}
