//
// System.Web.UI.Adapters.ControlAdapter
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.Adapters
{
	public abstract class ControlAdapter
	{
		protected ControlAdapter ()
		{
		}

		[MonoTODO]
		protected HttpBrowserCapabilities Browser 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected Control Control 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		protected Page Page 
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		protected PageAdapter PageAdapter 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected internal virtual void BeginRender (HtmlTextWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void CreateChildControls ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void EndRender (HtmlTextWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void LoadAdapterControlState (object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void LoadAdapterViewState (object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void OnInit (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void OnLoad (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void OnPreRender (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void OnUnload (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void Render (HtmlTextWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void RenderChildren (HtmlTextWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual object SaveAdapterControlState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual object SaveAdapterViewState ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
