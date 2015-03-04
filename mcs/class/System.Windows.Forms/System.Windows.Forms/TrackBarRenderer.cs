//
// TrackBarRenderer.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms
{
	public sealed class TrackBarRenderer
	{
		#region Private Constructor
		private TrackBarRenderer () { }
		#endregion

		#region Public Static Methods
		public static void DrawBottomPointingThumb(Graphics g, Rectangle bounds, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbBottom.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbBottom.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbBottom.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbBottom.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawHorizontalThumb (Graphics g, Rectangle bounds, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.Thumb.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.Thumb.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.Thumb.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.Thumb.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawHorizontalTicks(Graphics g, Rectangle bounds, int numTicks, EdgeStyle edgeStyle)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			if (bounds.Height <= 0 || bounds.Width <= 0 || numTicks <= 0)
				return;
				
			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.Ticks.Normal);
			
			double x = bounds.Left;
			double delta = (double)(bounds.Width - 2) / (double)(numTicks-1);
			
			for(int i = 0; i < numTicks; i++)
			{
				vsr.DrawEdge(g, new Rectangle((int)Math.Round(x), bounds.Top, 5, bounds.Height), Edges.Left, edgeStyle, EdgeEffects.None);
				x += delta;
			}
		}
		
		public static void DrawHorizontalTrack(Graphics g, Rectangle bounds)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();
				
			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.Track.Normal);
			
			vsr.DrawBackground (g, bounds);
		}

		public static void DrawLeftPointingThumb (Graphics g, Rectangle bounds, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbLeft.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbLeft.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbLeft.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbLeft.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawRightPointingThumb (Graphics g, Rectangle bounds, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbRight.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbRight.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbRight.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbRight.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawTopPointingThumb (Graphics g, Rectangle bounds, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbTop.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbTop.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbTop.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbTop.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawVerticalThumb (Graphics g, Rectangle bounds, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbVertical.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbVertical.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbVertical.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbVertical.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawVerticalTicks (Graphics g, Rectangle bounds, int numTicks, EdgeStyle edgeStyle)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			if (bounds.Height <= 0 || bounds.Width <= 0 || numTicks <= 0)
				return;

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.TicksVertical.Normal);

			double y = bounds.Top;
			double delta = (double)(bounds.Height - 2) / (double)(numTicks - 1);

			for (int i = 0; i < numTicks; i++) {
				vsr.DrawEdge (g, new Rectangle (bounds.Left, (int)Math.Round (y), bounds.Width, 5), Edges.Top, edgeStyle, EdgeEffects.None);
				y += delta;
			}
		}

		public static void DrawVerticalTrack (Graphics g, Rectangle bounds)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.Track.Normal);

			vsr.DrawBackground (g, bounds);
		}

		public static Size GetBottomPointingThumbSize(Graphics g, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbBottom.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbBottom.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbBottom.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbBottom.Pressed);
					break;
			}

			return vsr.GetPartSize (g, ThemeSizeType.Draw);
		}

		public static Size GetLeftPointingThumbSize (Graphics g, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbLeft.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbLeft.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbLeft.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbLeft.Pressed);
					break;
			}

			return vsr.GetPartSize (g, ThemeSizeType.Draw);
		}

		public static Size GetRightPointingThumbSize (Graphics g, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbRight.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbRight.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbRight.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbRight.Pressed);
					break;
			}

			return vsr.GetPartSize (g, ThemeSizeType.Draw);
		}

		public static Size GetTopPointingThumbSize (Graphics g, TrackBarThumbState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case TrackBarThumbState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbTop.Disabled);
					break;
				case TrackBarThumbState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbTop.Hot);
					break;
				case TrackBarThumbState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbTop.Normal);
					break;
				case TrackBarThumbState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.TrackBar.ThumbTop.Pressed);
					break;
			}

			return vsr.GetPartSize (g, ThemeSizeType.Draw);
		}
		#endregion

		#region Public Static Properties
		public static bool IsSupported {
			get { return VisualStyleInformation.IsEnabledByUser && (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled || Application.VisualStyleState == VisualStyleState.ClientAreaEnabled); }
		}
		#endregion
	}
}
