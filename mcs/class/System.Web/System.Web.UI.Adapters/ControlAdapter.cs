//
// System.Web.UI.Adapters.ControlAdapter
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.Adapters
{
	public abstract class ControlAdapter
	{
		internal ControlAdapter (Control c)
		{
			control = c;
		}
		
		protected ControlAdapter ()
		{
		}

		protected HttpBrowserCapabilities Browser 
		{
			get {
				Page page = Page;

				if (page != null)
					return page.Request.Browser;

				return null;
			}
		}

		internal Control control;
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected Control Control 
		{
			get { return control; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		protected Page Page 
		{
			get {
				Control control = Control;

				if (control != null)
					return control.Page;

				return null;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		protected PageAdapter PageAdapter 
		{
			get {
				Page page = Page;

				if (page != null)
					return page.PageAdapter;

				return null;
			}
		}

		protected internal virtual void BeginRender (HtmlTextWriter writer)
		{
			writer.BeginRender();
		}

		protected internal virtual void CreateChildControls ()
		{
			Control control = Control;
			if (control != null)
				control.CreateChildControls ();
		}

		protected internal virtual void EndRender (HtmlTextWriter writer)
		{
			writer.EndRender ();
		}

		protected internal virtual void LoadAdapterControlState (object state)
		{
		}

		protected internal virtual void LoadAdapterViewState (object state)
		{
		}

		protected internal virtual void OnInit (EventArgs e)
		{
			Control control = Control;

			if (control != null)
				control.OnInit(e);
		}

		protected internal virtual void OnLoad (EventArgs e)
		{
			Control control = Control;

			if (control != null)
				control.OnLoad(e);
		}

		protected internal virtual void OnPreRender (EventArgs e)
		{
			Control control = Control;

			if (control != null)
				control.OnPreRender(e);
		}

		protected internal virtual void OnUnload (EventArgs e)
		{
			Control control = Control;

			if (control != null)
				control.OnUnload(e);
		}

		protected internal virtual void Render (HtmlTextWriter writer)
		{
			Control control = Control;

			if (control != null)
				control.Render (writer);
		}

		protected internal virtual void RenderChildren (HtmlTextWriter writer)
		{
			Control control = Control;

			if (control != null)
				control.RenderChildren (writer);
		}

		protected internal virtual object SaveAdapterControlState ()
		{
			return null;
		}

		protected internal virtual object SaveAdapterViewState ()
		{
			return null;
		}
	}
}

