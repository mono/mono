//
// System.Drawing.Test.TextLineIterator.jvm.cs
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
	internal sealed class TextLineIterator {

		#region Fields

		readonly float _width;
		readonly float _height;
		readonly StringFormat _format;
		readonly font.FontRenderContext _frc;
		readonly string _s;
		readonly Font _font;
		readonly float _margin;

		static readonly string NewLine;

		static readonly geom.AffineTransform Rotate90Transform = 
			geom.AffineTransform.getRotateInstance(Math.PI/2);

		font.TextMeasurer _measurer;
		int _charsConsumed = 0;
		int _currentPos = 0;
		int _currentRun = 0;
		float _accumulatedHeight = 0;

		#endregion

		#region ctors

		static TextLineIterator() {
			string newLine = Environment.NewLine;
			if (newLine == null || newLine.Length == 0 || newLine[newLine.Length - 1] == '\n')
				newLine = "\n";

			NewLine = newLine;
		}

		internal TextLineIterator(string s, Font font, font.FontRenderContext frc, StringFormat format, float width, float height) {
			_format = (format != null) ? format : new StringFormat();
			_font = font;
			_s = (s != null) ? s : String.Empty;
			_frc = frc;
			FontFamily ff = font.FontFamily;
			_margin = font.Size*ff.GetDrawMargin(font.Style)/ff.GetEmHeight(font.Style);

			_width = width;
			_height = height;
		}

		#endregion

		#region Properties

		float WrapWidth
		{
			get {
				float widthOrHeight = _format.IsVertical ? Height : Width;
				if (!_format.IsGenericTypographic) {
					widthOrHeight =
						((_format.IsVertical ? Height : Width) - (0.463f * FontSize)) / 1.028f;
				}
				return widthOrHeight;
			}
		}

		internal float WrapHeight {
			get {
				float widthOrHeight = _format.IsVertical ? Width : Height;
				if (!_format.IsGenericTypographic) {
					widthOrHeight = (_format.IsVertical ? Width : Height) / 1.08864f;
				}
				return widthOrHeight;
			}
		}

		internal float Width {
			get { return _width; }
		}

		internal float Height {
			get { return _height; }
		}

		internal StringFormat Format {
			get { return _format; }
		}

		internal float PadWidth (float origWidth)
		{
			if (Format.IsGenericTypographic)
				return origWidth;

			//This is a proximity to .NET calculated Width.
			return origWidth * 1.028f + 0.463f * FontSize;
		}

		internal float PadHeight (float origHeight)
		{
			if (Format.IsGenericTypographic)
				return origHeight;

			//This is a proximity to .NET calculated Height.
			return 1.08864f * origHeight;
		}		

		internal float FontSize
		{
			get {
				return _font.Size;
			}
		}

		internal int CharsConsumed {
			get { return _charsConsumed; }
		}

		internal int CurrentRun {
			get { return _currentRun; }
		}

		internal int CurrentPosition {
			get { return _currentPos; }
		}

		internal float AccumulatedHeight {
			get { return _accumulatedHeight; }
		}

		internal float GetAdvanceBetween(int start, int limit) {
			return _measurer.getAdvanceBetween(start, limit);
		}

		internal geom.AffineTransform Transform {
			get { return Format.IsVertical ? Rotate90Transform : Matrix.IdentityTransform.NativeObject; }
		}

		#endregion

		#region Methods

		LineLayout NextTextLayoutFromMeasurer() {
			if (_accumulatedHeight >= WrapHeight) {
				_charsConsumed += _currentPos;
				return null;
			}

			int limit = _measurer.getLineBreakIndex(_currentPos, WrapWidth);

			int wordBreak = limit;
			if (wordBreak < _currentRun) {
				while (wordBreak >= _currentPos && char.IsLetterOrDigit(_s, _charsConsumed + wordBreak))
					wordBreak--;

				if (wordBreak > _currentPos)
					limit = wordBreak + 1;
			}
			font.TextLayout layout = _measurer.getLayout(_currentPos, limit);
			
			LineLayout lineLayout = new LineLayout(
				layout,
				this,
				_accumulatedHeight);

			float lineHeight = PadHeight (lineLayout.Ascent + lineLayout.Descent + lineLayout.Leading);
			
			if (Format.LineLimit && (_accumulatedHeight + lineHeight > WrapHeight)) {
				_charsConsumed += _currentPos;
				return null;
			}

			_accumulatedHeight += lineHeight + lineLayout.Leading;

			_currentPos = limit;

			while (_currentPos < _currentRun) {
				if (char.IsWhiteSpace(_s, _charsConsumed + _currentPos))
					_currentPos++;
				else
					break;
			}
			return lineLayout;
		}

		internal LineLayout NextLine() {
			if (_currentPos < _currentRun && !Format.NoWrap)
				return NextTextLayoutFromMeasurer();

			_charsConsumed += _currentRun;
			if (_charsConsumed >= _s.Length)
				return null;

			string s;
			int lineBreakIndex = _s.IndexOf(NewLine, _charsConsumed);
			if (lineBreakIndex >= 0) {
				s = _s.Substring(_charsConsumed, lineBreakIndex - _charsConsumed + NewLine.Length);
			}
			else
				s = _s.Substring(_charsConsumed);

			_currentRun = s.Length;
			_currentPos = 0;

			text.AttributedString aS = new text.AttributedString(s);
				
			// TODO: add more attribs according to StringFormat
			aS.addAttribute(font.TextAttribute.FONT, _font.NativeObject);
			if((_font.Style & FontStyle.Underline) != FontStyle.Regular)
				aS.addAttribute(font.TextAttribute.UNDERLINE, font.TextAttribute.UNDERLINE_ON);
			if((_font.Style & FontStyle.Strikeout) != FontStyle.Regular)
				aS.addAttribute(font.TextAttribute.STRIKETHROUGH, font.TextAttribute.STRIKETHROUGH_ON);

			text.AttributedCharacterIterator charIter = aS.getIterator();

			_measurer = new font.TextMeasurer(charIter, _frc);
			return NextTextLayoutFromMeasurer();
		}

		internal geom.AffineTransform CalcLineAlignmentTransform() {
			if (Format.LineAlignment == StringAlignment.Near)
				return null;
			float height = WrapHeight;
			if (float.IsPositiveInfinity(height))
				height = 0;

			float shift = height - AccumulatedHeight;
			if (height > 0 && shift <= 0)
				return null;

			if (Format.LineAlignment == StringAlignment.Center)
				shift /= 2;
			else
				if (Format.IsVertical && Format.IsRightToLeft)
				return null;

			return Format.IsVertical ?
				geom.AffineTransform.getTranslateInstance(shift, 0) :
				geom.AffineTransform.getTranslateInstance(0, shift);
		}

		#endregion
	}
}
