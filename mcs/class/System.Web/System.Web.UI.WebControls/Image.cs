//
// System.Web.UI.WebControls.Image.cs
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
	[DefaultProperty("ImageUrl")]
	public class Image : WebControl
	{
		public Image(): base(HtmlTextWriterTag.Img)
		{
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("An alternative text that is shown if the image cannot be displayed.")]
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

		[Browsable (false), EditorBrowsable (EditorBrowsableState.Never)]
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

		[Browsable (false), EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override FontInfo Font
		{
			get
			{
				return base.Font;
			}
		}

		[DefaultValue (typeof (ImageAlign), "NotSet"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The alignment of the image.")]
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
