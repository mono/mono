//
// System.Web.UI.WebControls.WizardStepBase
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

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

#if NET_2_0

using System;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[ControlBuilderAttribute (typeof(WizardStepControlBuilder))]
	[BindableAttribute (false)]
	[ToolboxItemAttribute ("")]
	public abstract class WizardStepBase: View
	{
		Wizard wizard;
		
		[DefaultValueAttribute (true)]
		[ThemeableAttribute (false)]
		[FilterableAttribute (false)]
		public virtual bool AllowReturn {
			get {
				object v = ViewState ["AllowReturn"];
				return v != null ? (bool)v : true;
			}
			set {
				ViewState ["AllowReturn"] = value;
			}
		}

		[Browsable (true)]
		public override bool EnableTheming {
			get { return base.EnableTheming; }
			set { base.EnableTheming = value; }
		}

		// .NET version of this property performs design-time checks, which we don't
		// support, thus our version is just a do-nothing override
		public override string ID {
			get { return base.ID; }
			set { base.ID = value; }
		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual string Name {
			get {
				if (Title != null && Title.Length > 0) return Title;
				else if (ID != null && ID.Length > 0) return ID;
				else return null;
			}
		}
		
		[DefaultValueAttribute (WizardStepType.Auto)]
		public virtual WizardStepType StepType {
			get {
				object v = ViewState ["StepType"];
				return v != null ? (WizardStepType)v : WizardStepType.Auto;
			}
			set {
				ViewState ["StepType"] = value;
			}
		}
		
		[DefaultValueAttribute ("")]
		[LocalizableAttribute (true)]
		public virtual string Title {
			get {
				object v = ViewState ["Title"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["Title"] = value;
			}
		}
		
		[EditorBrowsableAttribute (EditorBrowsableState.Advanced)]
		[BrowsableAttribute (false)]
		public Wizard Wizard {
			get { return wizard; }
		}
		
		protected override void LoadViewState (object savedState)
		{
			// why override?
			base.LoadViewState (savedState);
		}

		protected internal override void OnLoad (EventArgs e)
		{
			// why override?
			base.OnLoad (e);
		}

		protected internal override void RenderChildren (HtmlTextWriter writer)
		{
			// why override?
			base.RenderChildren (writer);
		}
		
		internal void SetWizard (Wizard w)
		{
			wizard = w;
		}
	} 
}

#endif
