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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Everaldo Canuto (ecanuto@novell.com)

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.Theming.Default
{
	internal class LinkLabelPainter
	{
		public LinkLabelPainter ()
		{
		}

		private Color GetPieceColor (LinkLabel label, LinkLabel.Piece piece, int i)
		{
			if (!label.Enabled)
				return label.DisabledLinkColor;

			if (piece.link == null)
				return label.ForeColor;

			if (!piece.link.Enabled)
				return label.DisabledLinkColor;
				
			if (piece.link.Active)
				return label.ActiveLinkColor;
				
			if ((label.LinkVisited && i == 0) || piece.link.Visited)
				return label.VisitedLinkColor;
			
			return label.LinkColor;
		}
		
		public virtual void Draw (Graphics dc, Rectangle clip_rectangle, LinkLabel label)
		{
			Rectangle client_rect = label.PaddingClientRectangle;

			label.DrawImage (dc, label.Image, client_rect, label.ImageAlign);

			if (label.pieces == null)
				return;

			// Paint all text as disabled.
			if (!label.Enabled) {
				dc.SetClip (clip_rectangle);
				ThemeEngine.Current.CPDrawStringDisabled (
					dc, label.Text, label.Font, label.BackColor, client_rect, label.string_format);
				return;
			}

			Font font, link_font = ThemeEngine.Current.GetLinkFont (label);
			
			Region text_region = new Region (new Rectangle());

			// Draw links.
			for (int i = 0; i < label.pieces.Length; i ++) {
				LinkLabel.Piece piece = label.pieces[i];
				
				if (piece.link == null) {
					text_region.Union (piece.region);
					continue;
				}

				Color color = GetPieceColor (label, piece, i);

				if ( (label.LinkBehavior == LinkBehavior.AlwaysUnderline) || 
					 (label.LinkBehavior == LinkBehavior.SystemDefault) ||
					 ((label.LinkBehavior == LinkBehavior.HoverUnderline) && piece.link.Hovered) )
					font = link_font;
				else
					font = label.Font;
				
				dc.Clip = piece.region;
				dc.Clip.Intersect (clip_rectangle);
				dc.DrawString (label.Text, font, 
						ThemeEngine.Current.ResPool.GetSolidBrush (color), 
						client_rect, label.string_format);
			
				// Draw focus rectangle
				if ((piece.link != null) && piece.link.Focused) {
					foreach (RectangleF rect in piece.region.GetRegionScans (dc.Transform))
						ControlPaint.DrawFocusRectangle (dc, Rectangle.Round (rect), label.ForeColor, label.BackColor);
				}
			}
			
			// Draw normal text (without links).
			if (!text_region.IsEmpty (dc)) {
				dc.Clip = text_region;
				dc.Clip.Intersect (clip_rectangle);
				if (!dc.Clip.IsEmpty (dc))
					dc.DrawString(label.Text, label.Font, 
						ThemeEngine.Current.ResPool.GetSolidBrush(label.ForeColor),
						client_rect, label.string_format);
			}
		}
	}
}