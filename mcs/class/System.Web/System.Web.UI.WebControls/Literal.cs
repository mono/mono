//
// System.Web.UI.WebControls.Literal.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Text")]
	[ControlBuilder(typeof(LiteralControlBuilder))]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	public class Literal : Control
	{
		public Literal () : base ()
		{
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The text for the literal WebControl.")]
		public string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		protected override void AddParsedSubObject (object obj)
		{
			if (!(obj is LiteralControl))
				throw new HttpException (HttpRuntime.FormatResourceString (
							"Cannot_Have_Children_Of_Type", "Literal",
							obj.GetType ().Name.ToString ()));

			Text = ((LiteralControl) obj).Text;
		}

		protected override void Render (HtmlTextWriter writer)
		{
			if (Text.Length > 0)
				writer.Write (Text);
		}
	}
}

