/**
 * Namespace: System.Web.UI.WebControls
 * Class:     HyperLink
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: gvaish@iitk.ac.in, myscripts_2001@yahoo.com
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
	//[Designer(??)]
	[ControlBuilder(typeof(HyperLinkControlBuilder))]
	//[DataBindingHandler("??")]
	[ParseChildren(false)]
	[ToolboxData("<{0}:HyperLink runat=\"server\">HyperLink</{0}:HyperLink>")]
	public class HyperLink: WebControl
	{
		bool textSet;

		public HyperLink(): base(HtmlTextWriterTag.A)
		{
		}

		public virtual string ImageUrl
		{
			get
			{
				object o = ViewState["ImageUrl"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["ImageUrl"] = value;
			}
		}

		public string NavigateUrl
		{
			get
			{
				object o = ViewState["NavigateUrl"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["NavigateUrl"] = value;
			}
		}

		public string Target
		{
			get
			{
				object o = ViewState["Target"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Target"] = value;
			}
		}

		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				if (o != null)
					return (string) o;

				return String.Empty;
			}
			set {
				ViewState["Text"] = value;
				textSet = true;
			}
		}

		string InternalText
		{
			get { return Text; }
			set { ViewState["Text"] = value; }
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(NavigateUrl.Length > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Href, NavigateUrl);
			}
			if(Target.Length > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Target, Target);
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(HasControls())
			{
				base.AddParsedSubObject(obj);
				return;
			}
			if(obj is LiteralControl)
			{
				// This is a hack to workaround the behaviour of the code generator, which
				// may split a text in several LiteralControls if there's a special character
				// such as '<' in it.
				if (textSet) {
					Text = ((LiteralControl)obj).Text;
					textSet = false;
				} else {
					InternalText += ((LiteralControl)obj).Text;
				}
				//

				return;
			}
			if(Text.Length > 0)
			{
				base.AddParsedSubObject(new LiteralControl (Text));
				Text = String.Empty;
			}
			base.AddParsedSubObject (obj);
		}

		protected override void LoadViewState(object savedState)
		{
			if(savedState != null)
			{
				base.LoadViewState(savedState);
				object o = ViewState["Text"];
				if(o!=null)
					Text = (string)o;
			}
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			if(ImageUrl.Length > 0)
			{
				Image img = new Image();
				img.ImageUrl = ResolveUrl(ImageUrl);
				if(ToolTip.Length > 0)
					img.ToolTip = ToolTip;
				if(Text.Length > 0)
					img.AlternateText = Text;
				img.RenderControl(writer);
				return;
			}
			if(HasControls())
			{
				base.RenderContents(writer);
				return;
			}
			writer.Write(Text);
		}
	}
}
