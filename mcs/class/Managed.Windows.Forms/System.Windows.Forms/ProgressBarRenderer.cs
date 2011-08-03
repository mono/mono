//
// ProgressBarRenderer.cs
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
	public sealed class ProgressBarRenderer
	{
		#region Private Constructor
		private ProgressBarRenderer () { }
		#endregion

		#region Public Static Methods
		public static void DrawHorizontalBar (Graphics g, Rectangle bounds)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer(VisualStyleElement.ProgressBar.Bar.Normal);
			
			vsr.DrawBackground(g, bounds);
		}

		public static void DrawHorizontalChunks (Graphics g, Rectangle bounds)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.ProgressBar.Chunk.Normal);

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawVerticalBar (Graphics g, Rectangle bounds)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.ProgressBar.BarVertical.Normal);

			vsr.DrawBackground (g, bounds);
		}

		public static void DrawVerticalChunks (Graphics g, Rectangle bounds)
		{
			if (!IsSupported)
				throw new InvalidOperationException ();

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.ProgressBar.ChunkVertical.Normal);

			vsr.DrawBackground (g, bounds);
		}
		#endregion
		
		#region Public Static Properties
		public static bool IsSupported {
			get { return VisualStyleInformation.IsEnabledByUser && (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled || Application.VisualStyleState == VisualStyleState.ClientAreaEnabled); }
		}
		
		public static int ChunkSpaceThickness {
			get {
				if (!IsSupported)
					throw new InvalidOperationException();
					
				VisualStyleRenderer vsr = new VisualStyleRenderer(VisualStyleElement.ProgressBar.Chunk.Normal);
				
				return vsr.GetInteger(IntegerProperty.ProgressSpaceSize);
			}
		}

		public static int ChunkThickness {
			get {
				if (!IsSupported)
					throw new InvalidOperationException ();

				VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.ProgressBar.Chunk.Normal);

				return vsr.GetInteger (IntegerProperty.ProgressChunkSize);
			}
		}
		#endregion
	}
}
