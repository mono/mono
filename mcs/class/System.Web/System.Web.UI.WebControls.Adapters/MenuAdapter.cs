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
		
		internal MenuAdapter (Menu c) : base (c)
		{
		}

		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);
		}

		protected internal override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			Control.RaisePostBackEvent (eventArgument);
		}

		protected override void RenderBeginTag (HtmlTextWriter writer)
		{
			base.RenderBeginTag (writer);
		}

		protected override void RenderContents (HtmlTextWriter writer)
		{
			base.RenderContents (writer);
		}

		protected override void RenderEndTag (HtmlTextWriter writer)
		{
			base.RenderEndTag (writer);
		}

		protected internal virtual void RenderItem (HtmlTextWriter writer, 
							    MenuItem item,
							    int position)
		{
			Control.RenderItem (writer, item, position);
		}
	  
		protected internal override void LoadAdapterControlState (object state)
		{
		}
		    
		protected internal override object SaveAdapterControlState ()
		{
			return null;
		}

		void System.Web.UI.IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}

		protected new Menu Control
		{
			get {
				return (Menu)control;
			}
		}
	}
}

#endif

