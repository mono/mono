//
// System.Web.UI.WebControls.Panel.cs
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
	[Designer ("System.Web.UI.Design.WebControls.PanelDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[ParseChildren(false)]
	[PersistChildren(true)]
	[ToolboxData("<{0}:Panel runat=\"server\">Panel</{0}:Panel>")]
	public class Panel: WebControl
	{
		public Panel(): base(HtmlTextWriterTag.Div)
		{
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("An Url specifying the background image for the panel.")]
		public virtual string BackImageUrl
		{
			get
			{
				object o = ViewState["BackImageUrl"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["BackImageUrl"] = value;
			}
		}

		[DefaultValue (typeof (HorizontalAlign), "NotSet"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The horizonal alignment of the panel.")]
		public virtual HorizontalAlign HorizontalAlign
		{
			get
			{
				object o = ViewState["HorizontalAlign"];
				if(o != null)
					return (HorizontalAlign)o;
				return HorizontalAlign.NotSet;
			}
			set
			{
				if(!Enum.IsDefined(typeof(HorizontalAlign), value))
				{
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
				}
				ViewState["HorizontalAlign"] = value;
			}
		}

		[DefaultValue (true), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("Determines if the content wraps at line-end.")]
		public virtual bool Wrap
		{
			get
			{
				object o = ViewState["Wrap"];
				if(o != null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["Wrap"] = value;
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(BackImageUrl.Length > 0)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundImage, "url(" + ResolveUrl(BackImageUrl) + ")");
			}
			if(HorizontalAlign != HorizontalAlign.NotSet)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Align, TypeDescriptor.GetConverter(typeof(HorizontalAlign)).ConvertToString(HorizontalAlign));
			}
			if(Wrap)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "nowrap");
			}
		}
	}
}
