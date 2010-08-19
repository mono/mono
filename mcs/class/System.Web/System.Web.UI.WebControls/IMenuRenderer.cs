//
// Authors:
//	Marek Habersack <grendel@twistedcode.net>
//
// (C) 2004-2010 Novell, Inc (http://novell.com)
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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace System.Web.UI.WebControls
{
	interface IMenuRenderer
	{
		HtmlTextWriterTag Tag { get; }
		
		void AddAttributesToRender (HtmlTextWriter writer);
		void PreRender (Page page, HtmlHead head, ClientScriptManager csm, string cmenu, StringBuilder script);
		void RenderBeginTag (HtmlTextWriter writer, string skipLinkText);
		void RenderEndTag (HtmlTextWriter writer);
		void RenderContents (HtmlTextWriter writer);
		
		void RenderItemContent (HtmlTextWriter writer, MenuItem item, bool isDynamicItem);
		void RenderMenuBeginTag (HtmlTextWriter writer, bool dynamic, int menuLevel);
		void RenderMenuBody (HtmlTextWriter writer, MenuItemCollection items, bool vertical, bool dynamic, bool notLast);
		void RenderMenuEndTag (HtmlTextWriter writer, bool dynamic, int menuLevel);
		void RenderMenuItem (HtmlTextWriter writer, MenuItem item, bool notLast, bool isFirst);

		bool IsDynamicItem (MenuItem item);
		bool IsDynamicItem (Menu owner, MenuItem item);
	}
}
