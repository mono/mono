//
// ToolStripManager.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public sealed class ToolStripManager
	{
		private static ToolStripRenderer renderer;
		private static ToolStripManagerRenderMode render_mode;
		private static bool visual_styles_enabled;

		#region Static Cnstructor
		static ToolStripManager ()
		{
			ToolStripManager.renderer = new ToolStripProfessionalRenderer ();
			ToolStripManager.render_mode = ToolStripManagerRenderMode.Professional;
			ToolStripManager.visual_styles_enabled = Application.RenderWithVisualStyles;
		}
		#endregion

		#region Public Properties
		public static ToolStripRenderer Renderer {
			get { return ToolStripManager.renderer; }
			set {
				if (ToolStripManager.Renderer != value) {
					ToolStripManager.renderer = value;
					ToolStripManager.OnRendererChanged (EventArgs.Empty);
				}
			}
		}

		public static ToolStripManagerRenderMode RenderMode {
			get { return ToolStripManager.render_mode; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripManagerRenderMode), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripManagerRenderMode", value));

				if (ToolStripManager.render_mode != value) {
					ToolStripManager.render_mode = value;

					switch (value) {
						case ToolStripManagerRenderMode.Custom:
							throw new NotSupportedException ();
						case ToolStripManagerRenderMode.System:
							ToolStripManager.Renderer = new ToolStripProfessionalRenderer ();
							break;
						case ToolStripManagerRenderMode.Professional:
							ToolStripManager.Renderer = new ToolStripProfessionalRenderer ();
							break;
					}
				}
			}
		}

		public static bool VisualStylesEnabled {
			get { return ToolStripManager.visual_styles_enabled; }
			set {
				if (ToolStripManager.visual_styles_enabled != value) {
					ToolStripManager.visual_styles_enabled = value;

					if (ToolStripManager.render_mode == ToolStripManagerRenderMode.Professional) {
						(ToolStripManager.renderer as ToolStripProfessionalRenderer).ColorTable.UseSystemColors = !value;
						ToolStripManager.OnRendererChanged (EventArgs.Empty);
					}
				}
			}
		}
		#endregion

		#region Public Events
		public static event EventHandler RendererChanged;
		#endregion

		#region Private/Internal Methods
		internal static void FireAppClicked ()
		{
			if (AppClicked != null) AppClicked (null, EventArgs.Empty);
		}

		internal static void FireAppFocusChanged (Form form)
		{
			if (AppFocusChange != null) AppFocusChange (form, EventArgs.Empty);
		}

		internal static void FireAppFocusChanged (object sender)
		{
			if (AppFocusChange != null) AppFocusChange (sender, EventArgs.Empty);
		}
		private static void OnRendererChanged (EventArgs e)
		{
			if (RendererChanged != null) RendererChanged (null, e);
		}

		internal static event EventHandler AppClicked;
		internal static event EventHandler AppFocusChange;
		#endregion
	}
}
#endif