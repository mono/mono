/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Image
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
	[DefaultProperty("ImageUrl")]
	[ParseChildren(true)]
	[PersistChildren(false)]
	public class Image : WebControl
	{
		public Image(): base(HtmlTextWriterTag.Img)
		{
		}

		public virtual string AlternateText
		{
			get
			{
				object o = ViewState["AlternateText"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["AlternateText"] = value;
			}
		}

		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				base.Enabled = value;
			}
		}

		public override FontInfo Font
		{
			get
			{
				return base.Font;
			}
		}

		public virtual ImageAlign ImageAlign
		{
			get
			{
				object o = ViewState["ImageAlign"];
				if(o!=null)
					return (ImageAlign)o;
				return ImageAlign.NotSet;
			}
			set
			{
				ViewState["ImageAlign"] = value;
			}
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

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(ImageUrl.Length > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Src, ResolveUrl(ImageUrl));
			}
			if(AlternateText.Length > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Alt, AlternateText);
			}
			if(BorderWidth.IsEmpty)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
			}
			if(ImageAlign != ImageAlign.NotSet)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Align, Enum.Format(typeof(ImageAlign), ImageAlign, "G"));
			}
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
		}
	}
}
