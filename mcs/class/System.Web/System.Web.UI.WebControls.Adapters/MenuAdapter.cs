//
// System.Web.UI.WebControls.Adapters.MenuAdapter
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace System.Web.UI.WebControls.Adapters
{
	public class MenuAdapter : WebControlAdapter, IPostBackEventHandler
	{
		public MenuAdapter () 
		{
		}

		protected internal override void OnInit(EventArgs e)
		{
			/* registers the associated control as one that requires control state */
			Page.RegisterRequiresControlState (Control);

			/* and calls control.OnInit */
			Control.OnInit (e);
		}

		protected internal override void OnPreRender(EventArgs e)
		{
			Control.OnPreRender (e);
		}

		[MonoTODO]
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("why override?")]
		protected override void RenderBeginTag (HtmlTextWriter writer)
		{
			Control.RenderBeginTag (writer);
		}

		[MonoTODO ("we need to iterate over the MenuItems here, calling MenuAdapter.RenderItem for each one.")]
		protected override void RenderContents (HtmlTextWriter writer)
		{
			Control.RenderContents (writer);
		}

		[MonoTODO ("why override?")]
		protected override void RenderEndTag (HtmlTextWriter writer)
		{
			Control.RenderEndTag (writer);
		}

		[MonoTODO]
		protected internal virtual void RenderItem (HtmlTextWriter writer, 
							    MenuItem item,
							    int position)
		{
			throw new NotImplementedException ();
		}
	  
		[MonoTODO]
		protected internal override void LoadAdapterControlState (object state)
		{
			throw new NotImplementedException ();
		}
		    
		[MonoTODO]
		protected internal override object SaveAdapterControlState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void System.Web.UI.IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			throw new NotImplementedException ();
		}

		protected new Menu Control
		{
			get {
				return (Menu)base.Control;
			}
		}
	}
}

#endif

