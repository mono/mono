//
// System.Web.UI.WebControls.HyperLink.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Text")]
	[ControlBuilder(typeof(HyperLinkControlBuilder))]
	[Designer("System.Web.UI.Design.WebControls.HyperLinkDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[DataBindingHandler("System.Web.UI.Design.HyperLinkDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[ParseChildren(false)]
	[ToolboxData("<{0}:HyperLink runat=\"server\">HyperLink</{0}:HyperLink>")]
	public class HyperLink: WebControl
	{
		bool textSet;

		public HyperLink(): base(HtmlTextWriterTag.A)
		{
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("The URL to the image file.")]
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

		[DefaultValue (""), Bindable (true), WebCategory ("Navigation")]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("The URL to navigate to.")]
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

		[DefaultValue (""), Bindable (true), WebCategory ("Navigation")]
		[TypeConverter (typeof (TargetConverter))]
		[WebSysDescription ("The target frame in which the navigation target should be opened.")]
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

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("The text that should be shown on this HyperLink.")]
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
