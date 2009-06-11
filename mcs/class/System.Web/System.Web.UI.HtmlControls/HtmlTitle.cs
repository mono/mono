//
// System.Web.UI.HtmlControls.HtmlTitle
//
// Authors:
// 	Lluis Sanchez Gual (lluis@novell.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HtmlTitle : HtmlControl
	{
		string text;
		
		protected override void AddParsedSubObject(object obj)
		{
			LiteralControl lit = obj as LiteralControl;
			if (lit != null) text = lit.Text;
			else base.AddParsedSubObject (obj);
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new ControlCollection (this);
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue ("")]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[Localizable (true)]
		public virtual string Text {
			get { return text; }
			set { text = value; }
		}
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			writer.RenderBeginTag (HtmlTextWriterTag.Title);
			if (HasControls () || HasRenderMethodDelegate ())
				RenderChildren (writer);
			else
				writer.Write (text);
			writer.RenderEndTag ();
		}
	}
}

#endif
