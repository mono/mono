//
// System.Web.UI.WebControls.Label.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Text")]
	[Designer("System.Web.UI.Design.WebControls.LabelDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[ControlBuilder(typeof(LabelControlBuilder))] 
	[ParseChildren(false)]
	[ToolboxData("<{0}:Label runat=\"server\">Label</{0}:Label>")]
	public class Label : WebControl
	{
		public Label (): base ()
		{
		}

		internal Label (HtmlTextWriterTag tagKey) : base (tagKey)
		{
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("The text that should be shown on this Label.")]
		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}

		protected override void AddParsedSubObject (object obj)
		{
			if(HasControls ()){
				base.AddParsedSubObject (obj);
				return;
			}

			if(obj is LiteralControl){
				Text = ((LiteralControl) obj).Text;
				return;
			}

			if(Text.Length > 0){
				base.AddParsedSubObject (new LiteralControl (Text));
				Text = String.Empty;
			}

			base.AddParsedSubObject (obj);
		}

		protected override void LoadViewState (object savedState)
		{
			if(savedState != null) {
				base.LoadViewState (savedState);
				string savedText = ViewState ["Text"] as string;
				if(savedText != null)
					Text = savedText;
			}
		}

		protected override void RenderContents (HtmlTextWriter writer)
		{
			if(HasControls ())
				base.RenderContents (writer);
			else
				writer.Write (Text);
		}
	}
}
