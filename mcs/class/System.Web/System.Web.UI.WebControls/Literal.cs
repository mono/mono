/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Literal
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Text")]
	[ControlBuilder(typeof(LiteralControlBuilder))]
	//[DataBindingHandler("??")]
	public class Literal : Control
	{
		public Literal () : base ()
		{
		}

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

