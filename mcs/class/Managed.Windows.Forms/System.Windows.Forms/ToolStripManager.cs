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
using System.Collections.Generic;

namespace System.Windows.Forms
{
	public sealed class ToolStripManager
	{
		private static ToolStripRenderer renderer;
		private static ToolStripManagerRenderMode render_mode;
		private static bool visual_styles_enabled;
		private static List<ToolStrip> toolstrips;
		private static List<ToolStripMenuItem> menu_items;
		
		#region Static Constructor
		static ToolStripManager ()
		{
			toolstrips = new List<ToolStrip> ();
			menu_items = new List<ToolStripMenuItem> ();
			ToolStripManager.renderer = new ToolStripProfessionalRenderer ();
			ToolStripManager.render_mode = ToolStripManagerRenderMode.Professional;
			ToolStripManager.visual_styles_enabled = Application.RenderWithVisualStyles;
		}

		private ToolStripManager ()
		{
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

		#region Public Methods
		public static ToolStrip FindToolStrip (string toolStripName)
		{
			lock (toolstrips)
				foreach (ToolStrip ts in toolstrips)
					if (ts.Name == toolStripName)
						return ts;
						
			return null;
		}
		
		public static bool IsValidShortcut (Keys shortcut)
		{
			// Anything with an F1 - F12 is a shortcut
			if ((shortcut & Keys.F1) == Keys.F1)
				return true;
			else if ((shortcut & Keys.F2) == Keys.F2)
				return true;
			else if ((shortcut & Keys.F3) == Keys.F3)
				return true;
			else if ((shortcut & Keys.F4) == Keys.F4)
				return true;
			else if ((shortcut & Keys.F5) == Keys.F5)
				return true;
			else if ((shortcut & Keys.F6) == Keys.F6)
				return true;
			else if ((shortcut & Keys.F7) == Keys.F7)
				return true;
			else if ((shortcut & Keys.F8) == Keys.F8)
				return true;
			else if ((shortcut & Keys.F9) == Keys.F9)
				return true;
			else if ((shortcut & Keys.F10) == Keys.F10)
				return true;
			else if ((shortcut & Keys.F11) == Keys.F11)
				return true;
			else if ((shortcut & Keys.F12) == Keys.F12)
				return true;
				
			// Modifier keys alone are not shortcuts
			switch (shortcut) {
				case Keys.Alt:
				case Keys.Control:
				case Keys.Shift:
				case Keys.Alt | Keys.Control:
				case Keys.Alt | Keys.Shift:
				case Keys.Control | Keys.Shift:
				case Keys.Alt | Keys.Control | Keys.Shift:
					return false;
			}
	
			// Anything else with a modifier key is a shortcut
			if ((shortcut & Keys.Alt) == Keys.Alt)
				return true;
			else if ((shortcut & Keys.Control) == Keys.Control)
				return true;
			else if ((shortcut & Keys.Shift) == Keys.Shift)
				return true;

			// Anything else is not a shortcut
			return false;
		}
		#endregion
		
		#region Public Events
		public static event EventHandler RendererChanged;
		#endregion

		#region Private/Internal Methods
		internal static void AddToolStrip (ToolStrip ts)
		{
			lock (toolstrips)
				toolstrips.Add (ts);
		}

		internal static bool ProcessCmdKey (ref Message m, Keys keyData)
		{
			lock (menu_items)
				foreach (ToolStripMenuItem tsmi in menu_items)
					if (tsmi.ProcessCmdKey (ref m, keyData) == true)
						return true;
			
			return false;
		}
		
		internal static void AddToolStripMenuItem (ToolStripMenuItem tsmi)
		{
			lock (menu_items)
				menu_items.Add (tsmi);
		}
		
		internal static void RemoveToolStrip (ToolStrip ts)
		{
			lock (toolstrips)
				toolstrips.Remove (ts);
		}

		internal static void RemoveToolStripMenuItem (ToolStripMenuItem tsmi)
		{
			lock (menu_items)
				menu_items.Remove (tsmi);
		}

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
