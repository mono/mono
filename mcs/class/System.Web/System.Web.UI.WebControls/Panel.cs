/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Panel
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	//[Designer("??")]
	[DefaultProperty("ID")]
	[ParseChildren(false)]
	[PersistChildren(true)]
	[ToolboxData("<{0}:Panel runat=\"server\">Panel</{0}:Panel>")]
	public class Panel: WebControl
	{
		public Panel(): base(HtmlTextWriterTag.Div)
		{
		}

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
					throw new ArgumentException();
				}
				ViewState["HorizontalAlign"] = value;
			}
		}

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
