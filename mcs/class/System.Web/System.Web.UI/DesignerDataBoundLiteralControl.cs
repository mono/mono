//
// System.Web.UI.DesignerDataBoundLiteralControl.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;

namespace System.Web.UI
{
	[ToolboxItem(false)]
	[DataBindingHandler ("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	public sealed class DesignerDataBoundLiteralControl : Control
	{
		private string text = "";

		public DesignerDataBoundLiteralControl ()
		{
			base.PreventAutoID ();
		}

		public string Text {
			get { return text;}
			set {
				if (value == null)
					text = string.Empty;
				else
					text = value;
			}
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState != null)
				text = (string) savedState;
		}

		protected override void Render (HtmlTextWriter output)
		{
			output.Write (text);
		}

		protected override object SaveViewState ()
		{
			return text;
		}
	}
}
