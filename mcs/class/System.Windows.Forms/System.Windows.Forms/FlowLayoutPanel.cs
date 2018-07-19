//
// FlowLayoutPanel.cs
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

using System.Windows.Forms.Layout;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing;

namespace System.Windows.Forms
{
	[ComVisibleAttribute (true)]
	[ClassInterfaceAttribute (ClassInterfaceType.AutoDispatch)]
	[ProvideProperty ("FlowBreak", typeof (Control))]
	[DefaultProperty ("FlowDirection")]
	[Docking (DockingBehavior.Ask)]
	[Designer ("System.Windows.Forms.Design.FlowLayoutPanelDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class FlowLayoutPanel : Panel, IExtenderProvider
	{
		private FlowLayoutSettings settings;

		public FlowLayoutPanel () : base ()
		{
			CreateDockPadding ();
		}

		#region Properties
		[Localizable (true)]
		[DefaultValue (FlowDirection.LeftToRight)]
		public FlowDirection FlowDirection {
			get { return LayoutSettings.FlowDirection; }
			set { LayoutSettings.FlowDirection = value; }
		}

		[LocalizableAttribute (true)]
		[DefaultValue (true)]
		public bool WrapContents {
			get { return LayoutSettings.WrapContents; }
			set { LayoutSettings.WrapContents = value; }
		}

		public override LayoutEngine LayoutEngine {
			get { return System.Windows.Forms.Layout.FlowLayout.Instance; }
		}

		internal FlowLayoutSettings LayoutSettings {
			get { 
				if (this.settings == null)
					this.settings = new FlowLayoutSettings (this);
					
				return this.settings;
			}
		}
		#endregion

		#region Public Methods
		[DefaultValue (false)]
		[DisplayName ("FlowBreak")]
		public bool GetFlowBreak (Control control)
		{
			return LayoutSettings.GetFlowBreak (control);
		}

		[DisplayName ("FlowBreak")]
		public void SetFlowBreak (Control control, bool value)
		{
			LayoutSettings.SetFlowBreak (control, value);
		}		
		#endregion
		
		#region IExtenderProvider Members
		bool IExtenderProvider.CanExtend (object obj)
		{
			if (obj is Control)
				if ((obj as Control).Parent == this)
					return true;

			return false;
		}
		#endregion
	}
}
