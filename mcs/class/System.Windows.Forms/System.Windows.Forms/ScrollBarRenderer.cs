//
// ScrollBarRenderer.cs
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
	public sealed class ScrollBarRenderer
	{
		#region Private Constructor
		private ScrollBarRenderer () { }
		#endregion

		#region Public Static Methods
		public static void DrawArrowButton (Graphics g, Rectangle bounds, ScrollBarArrowButtonState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case ScrollBarArrowButtonState.DownDisabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.DownDisabled);
					break;
				case ScrollBarArrowButtonState.DownHot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.DownHot);
					break;
				case ScrollBarArrowButtonState.DownNormal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.DownNormal);
					break;
				case ScrollBarArrowButtonState.DownPressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.DownPressed);
					break;
				case ScrollBarArrowButtonState.LeftDisabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.LeftDisabled);
					break;
				case ScrollBarArrowButtonState.LeftHot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.LeftHot);
					break;
				case ScrollBarArrowButtonState.LeftNormal:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.LeftNormal);
					break;
				case ScrollBarArrowButtonState.LeftPressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.LeftPressed);
					break;
				case ScrollBarArrowButtonState.RightDisabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.RightDisabled);
					break;
				case ScrollBarArrowButtonState.RightHot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.RightHot);
					break;
				case ScrollBarArrowButtonState.RightNormal:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.RightNormal);
					break;
				case ScrollBarArrowButtonState.RightPressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.RightPressed);
					break;
				case ScrollBarArrowButtonState.UpDisabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.UpDisabled);
					break;
				case ScrollBarArrowButtonState.UpHot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.UpHot);
					break;
				case ScrollBarArrowButtonState.UpNormal:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.UpNormal);
					break;
				case ScrollBarArrowButtonState.UpPressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ArrowButton.UpPressed);
					break;
			}
			
			vsr.DrawBackground(g, bounds);
		}

		public static void DrawHorizontalThumb (Graphics g, Rectangle bounds, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case ScrollBarState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Disabled);
					break;
				case ScrollBarState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Hot);
					break;
				case ScrollBarState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Normal);
					break;
				case ScrollBarState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawHorizontalThumbGrip (Graphics g, Rectangle bounds, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.GripperHorizontal.Normal);

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawLeftHorizontalTrack (Graphics g, Rectangle bounds, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case ScrollBarState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.LeftTrackHorizontal.Disabled);
					break;
				case ScrollBarState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.LeftTrackHorizontal.Hot);
					break;
				case ScrollBarState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.LeftTrackHorizontal.Normal);
					break;
				case ScrollBarState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.LeftTrackHorizontal.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawLowerVerticalTrack (Graphics g, Rectangle bounds, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case ScrollBarState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.LowerTrackVertical.Disabled);
					break;
				case ScrollBarState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.LowerTrackVertical.Hot);
					break;
				case ScrollBarState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.LowerTrackVertical.Normal);
					break;
				case ScrollBarState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.LowerTrackVertical.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}
		
		public static void DrawRightHorizontalTrack (Graphics g, Rectangle bounds, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case ScrollBarState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.RightTrackHorizontal.Disabled);
					break;
				case ScrollBarState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.RightTrackHorizontal.Hot);
					break;
				case ScrollBarState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.RightTrackHorizontal.Normal);
					break;
				case ScrollBarState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.RightTrackHorizontal.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawSizeBox (Graphics g, Rectangle bounds, ScrollBarSizeBoxState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case ScrollBarSizeBoxState.LeftAlign:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.SizeBox.LeftAlign);
					break;
				case ScrollBarSizeBoxState.RightAlign:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.SizeBox.RightAlign);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawUpperVerticalTrack (Graphics g, Rectangle bounds, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case ScrollBarState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.UpperTrackVertical.Disabled);
					break;
				case ScrollBarState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.UpperTrackVertical.Hot);
					break;
				case ScrollBarState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.UpperTrackVertical.Normal);
					break;
				case ScrollBarState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.UpperTrackVertical.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawVerticalThumb (Graphics g, Rectangle bounds, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr;

			switch (state) {
				case ScrollBarState.Disabled:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ThumbButtonVertical.Disabled);
					break;
				case ScrollBarState.Hot:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ThumbButtonVertical.Hot);
					break;
				case ScrollBarState.Normal:
				default:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ThumbButtonVertical.Normal);
					break;
				case ScrollBarState.Pressed:
					vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.ThumbButtonVertical.Pressed);
					break;
			}

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawVerticalThumbGrip (Graphics g, Rectangle bounds, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.GripperVertical.Normal); ;

			vsr.DrawBackground (g, bounds);
		}

		public static Size GetSizeBoxSize (Graphics g, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.SizeBox.LeftAlign);
			
			return vsr.GetPartSize(g, ThemeSizeType.Draw);
		}

		public static Size GetThumbGripSize (Graphics g, ScrollBarState state)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.ScrollBar.GripperVertical.Normal);

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
