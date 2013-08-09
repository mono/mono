// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
// (C) Francis Fisher 2013
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

using System.Drawing;
namespace System.Windows.Forms.DataVisualization.Charting
{
	public class CustomLabel : ChartNamedElement
	{
		public CustomLabel ()
		{
		}
		public CustomLabel (double fromPosition, double toPosition, string text, int labelRow, LabelMarkStyle markStyle)
		{
			FromPosition = fromPosition;
			ToPosition = toPosition;
			Text = text;
			RowIndex = labelRow;
			LabelMark = markStyle;
		}
		public CustomLabel (double fromPosition, double toPosition, string text, int labelRow, LabelMarkStyle markStyle, GridTickTypes gridTick)
		{
			FromPosition = fromPosition;
			ToPosition = toPosition;
			Text = text;
			RowIndex = labelRow;
			LabelMark = markStyle;
			GridTicks = gridTick;
		}

		public Axis Axis { get; private set; } 
		public Color ForeColor { get; set; }
		public double FromPosition { get; set; }
		public GridTickTypes GridTicks { get; set; }
		public string Image { get; set; }
		public Color ImageTransparentColor { get; set; }
		public LabelMarkStyle LabelMark { get; set; }
		public Color MarkColor { get; set; }
		public override string Name { get; set; }
		public int RowIndex { get; set; }
		public string Text { get; set; }
		public string ToolTip { get; set; }
		public double ToPosition { get; set; }

		[MonoTODO]
		public CustomLabel Clone ()
		{
			throw new NotImplementedException ();
		}
	}
}
