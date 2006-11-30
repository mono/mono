//
// ToolStripRenderer.cs
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
// Copyright (c) Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace System.Windows.Forms
{
	public abstract class ToolStripRenderer
	{
		private static ColorMatrix grayscale_matrix = new ColorMatrix (new float[][] {
						//new float[]{0.3f,0.3f,0.3f,0,0},
						//new float[]{0.59f,0.59f,0.59f,0,0},
						//new float[]{0.11f,0.11f,0.11f,0,0},
						//new float[]{0,0,0,1,0,0},
						//new float[]{0,0,0,0,1,0},
						//new float[]{0,0,0,0,0,1}
					  new float[]{0.2f,0.2f,0.2f,0,0},
					  new float[]{0.41f,0.41f,0.41f,0,0},
					  new float[]{0.11f,0.11f,0.11f,0,0},
					  new float[]{0.15f,0.15f,0.15f,1,0,0},
					  new float[]{0.15f,0.15f,0.15f,0,1,0},
					  new float[]{0.15f,0.15f,0.15f,0,0,1}
				  });

		protected ToolStripRenderer () 
		{
		}

		#region Public Methods
		public static Image CreateDisabledImage(Image normalImage)
		{
			if (normalImage == null)
				return null;
				
			// Code adapted from ThemeWin32Classic.cs
			ImageAttributes ia = new ImageAttributes();
			ia.SetColorMatrix (grayscale_matrix);
			
			Bitmap b = new Bitmap(normalImage);
			Graphics.FromImage(b).DrawImage(normalImage, new Rectangle (0, 0, normalImage.Width, normalImage.Height), 0, 0, normalImage.Width, normalImage.Height, GraphicsUnit.Pixel, ia);
			
			return b;
		}

		public void DrawArrow (ToolStripArrowRenderEventArgs e)
		{ this.OnRenderArrow (e); }
		
		public void DrawButtonBackground (ToolStripItemRenderEventArgs e)
		{ this.OnRenderButtonBackground (e); }

		public void DrawDropDownButtonBackground (ToolStripItemRenderEventArgs e)
		{ this.OnRenderDropDownButtonBackground (e); }

		public void DrawGrip (ToolStripGripRenderEventArgs e)
		{ this.OnRenderGrip (e); }

		public void DrawImageMargin (ToolStripRenderEventArgs e)
		{ this.OnRenderImageMargin (e); }

		public void DrawItemBackground (ToolStripItemRenderEventArgs e)
		{ this.OnRenderItemBackground (e); }

		public void DrawItemCheck (ToolStripItemImageRenderEventArgs e)
		{ this.OnRenderItemCheck (e); }

		public void DrawItemImage (ToolStripItemImageRenderEventArgs e)
		{ this.OnRenderItemImage (e); }

		public void DrawItemText (ToolStripItemTextRenderEventArgs e)
		{ this.OnRenderItemText (e); }

		public void DrawLabelBackground (ToolStripItemRenderEventArgs e)
		{ this.OnRenderLabelBackground (e); }

		public void DrawMenuItemBackground (ToolStripItemRenderEventArgs e)
		{ this.OnRenderMenuItemBackground (e); }

		public void DrawOverflowButtonBackground (ToolStripItemRenderEventArgs e)
		{ this.OnRenderOverflowButtonBackground (e); }

		public void DrawSeparator (ToolStripSeparatorRenderEventArgs e)
		{ this.OnRenderSeparator (e); }

		public void DrawSplitButton (ToolStripItemRenderEventArgs e)
		{ this.OnRenderSplitButtonBackground (e); }

		public void DrawStatusStripSizingGrip (ToolStripRenderEventArgs e)
		{ this.OnRenderStatusStripSizingGrip (e); }

		public void DrawToolStripBackground (ToolStripRenderEventArgs e)
		{ this.OnRenderToolStripBackground (e); }

		public void DrawToolStripBorder (ToolStripRenderEventArgs e)
		{ this.OnRenderToolStripBorder (e); }

		public void DrawToolStripContentPanelBackground (ToolStripContentPanelRenderEventArgs e)
		{ this.OnRenderToolStripContentPanelBackground (e); }

		public void DrawToolStripPanelBackground (ToolStripPanelRenderEventArgs e)
		{ this.OnRenderToolStripPanelBackground (e); }

		public void DrawToolStripStatusLabelBackground (ToolStripItemRenderEventArgs e)
		{ this.OnRenderToolStripStatusLabelBackground (e); }
		#endregion

		#region Protected Methods
		protected internal virtual void Initialize (ToolStrip toolStrip) {}
		protected internal virtual void InitializeContentPanel (ToolStripContentPanel contentPanel) {}
		protected internal virtual void InitializeItem (ToolStripItem item) {}
		protected internal virtual void InitializePanel (ToolStripPanel toolStripPanel) {}

		protected virtual void OnRenderArrow (ToolStripArrowRenderEventArgs e)
		{ if (RenderArrow != null) RenderArrow (this, e); }

		protected virtual void OnRenderButtonBackground (ToolStripItemRenderEventArgs e)
		{ if (RenderButtonBackground != null) RenderButtonBackground (this, e); }

		protected virtual void OnRenderDropDownButtonBackground (ToolStripItemRenderEventArgs e)
		{ if (RenderDropDownButtonBackground != null) RenderDropDownButtonBackground (this, e); }
		
		protected virtual void OnRenderGrip (ToolStripGripRenderEventArgs e)
		{ if (RenderGrip != null) RenderGrip (this, e); }

		protected virtual void OnRenderImageMargin (ToolStripRenderEventArgs e)
		{ if (RenderImageMargin != null) RenderImageMargin (this, e); }

		protected virtual void OnRenderItemBackground (ToolStripItemRenderEventArgs e)
		{ if (RenderItemBackground != null) RenderItemBackground (this, e); }

		protected virtual void OnRenderItemCheck (ToolStripItemImageRenderEventArgs e)
		{ if (RenderItemCheck != null) RenderItemCheck (this, e); }

		protected virtual void OnRenderItemImage (ToolStripItemImageRenderEventArgs e)
		{ if (RenderItemImage != null) RenderItemImage (this, e); }

		protected virtual void OnRenderItemText (ToolStripItemTextRenderEventArgs e)
		{ if (RenderItemText != null) RenderItemText (this, e); }

		protected virtual void OnRenderLabelBackground (ToolStripItemRenderEventArgs e)
		{ if (RenderLabelBackground != null) RenderLabelBackground (this, e); }

		protected virtual void OnRenderMenuItemBackground (ToolStripItemRenderEventArgs e)
		{ if (RenderMenuItemBackground != null) RenderMenuItemBackground (this, e); }

		protected virtual void OnRenderOverflowButtonBackground (ToolStripItemRenderEventArgs e)
		{ if (RenderOverflowButtonBackground != null) RenderOverflowButtonBackground (this, e); }

		protected virtual void OnRenderSeparator (ToolStripSeparatorRenderEventArgs e)
		{ if (RenderSeparator != null) RenderSeparator (this, e); }

		protected virtual void OnRenderSplitButtonBackground (ToolStripItemRenderEventArgs e)
		{ if (RenderSplitButtonBackground != null) RenderSplitButtonBackground (this, e); }

		protected virtual void OnRenderStatusStripSizingGrip (ToolStripRenderEventArgs e)
		{ if (RenderStatusStripSizingGrip != null) RenderStatusStripSizingGrip (this, e); }

		protected virtual void OnRenderToolStripBackground (ToolStripRenderEventArgs e)
		{ if (RenderToolStripBackground != null) RenderToolStripBackground (this, e); }

		protected virtual void OnRenderToolStripBorder (ToolStripRenderEventArgs e)
		{ if (RenderToolStripBorder != null) RenderToolStripBorder (this, e); }

		protected virtual void OnRenderToolStripContentPanelBackground (ToolStripContentPanelRenderEventArgs e)
		{ if (RenderToolStripContentPanelBackground != null) RenderToolStripContentPanelBackground (this, e); }

		protected virtual void OnRenderToolStripPanelBackground (ToolStripPanelRenderEventArgs e)
		{ if (RenderToolStripPanelBackground != null) RenderToolStripPanelBackground (this, e); }

		protected virtual void OnRenderToolStripStatusLabelBackground (ToolStripItemRenderEventArgs e)
		{ if (RenderToolStripStatusLabelBackground != null) RenderToolStripStatusLabelBackground (this, e); }
		#endregion

		#region Public Events
		public event ToolStripArrowRenderEventHandler RenderArrow;
		public event ToolStripItemRenderEventHandler RenderButtonBackground;
		public event ToolStripItemRenderEventHandler RenderDropDownButtonBackground;
		public event ToolStripGripRenderEventHandler RenderGrip;
		public event ToolStripRenderEventHandler RenderImageMargin;
		public event ToolStripItemRenderEventHandler RenderItemBackground;
		public event ToolStripItemImageRenderEventHandler RenderItemCheck;
		public event ToolStripItemImageRenderEventHandler RenderItemImage;
		public event ToolStripItemTextRenderEventHandler RenderItemText;
		public event ToolStripItemRenderEventHandler RenderLabelBackground;
		public event ToolStripItemRenderEventHandler RenderMenuItemBackground;
		public event ToolStripItemRenderEventHandler RenderOverflowButtonBackground;
		public event ToolStripSeparatorRenderEventHandler RenderSeparator;
		public event ToolStripItemRenderEventHandler RenderSplitButtonBackground;
		public event ToolStripRenderEventHandler RenderStatusStripSizingGrip;
		public event ToolStripRenderEventHandler RenderToolStripBackground;
		public event ToolStripRenderEventHandler RenderToolStripBorder;
		public event ToolStripContentPanelRenderEventHandler RenderToolStripContentPanelBackground;
		public event ToolStripPanelRenderEventHandler RenderToolStripPanelBackground;
		public event ToolStripItemRenderEventHandler RenderToolStripStatusLabelBackground;
		#endregion
	}
}
#endif