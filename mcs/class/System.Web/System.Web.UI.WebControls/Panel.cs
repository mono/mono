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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

#if NET_2_0
		[UrlProperty]
#else
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Appearance")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (HorizontalAlign), "NotSet"), WebCategory ("Layout")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (true), WebCategory ("Layout")]
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
			if(!Wrap)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "nowrap");
			}
		}
	}
}
