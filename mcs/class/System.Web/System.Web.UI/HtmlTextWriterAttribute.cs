// 
// System.Web.UI.HtmlTextWriterTag
//
// Author:
//        Ben Maurer <bmaurer@novell.com>
//
// (c) 2005 Novell
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

namespace System.Web.UI {
#if !NET_2_0
	[Serializable]
#endif
	public enum HtmlTextWriterAttribute {
		Accesskey,
		Align,
		Alt,
		Background,
		Bgcolor,
		Border,
		Bordercolor,
		Cellpadding,
		Cellspacing,
		Checked,
		Class,
		Cols,
		Colspan,
		Disabled,
		For,
		Height,
		Href,
		Id,
		Maxlength,
		Multiple,
		Name,
		Nowrap,
		Onchange,
		Onclick,
		ReadOnly,
		Rows,
		Rowspan,
		Rules,
		Selected,
		Size,
		Src,
		Style,
		Tabindex,
		Target,
		Title,
		Type,
		Valign,
		Value,
		Width,
		Wrap,
#if NET_2_0
		Abbr,
		AutoComplete,
		Axis,
		Content,
		Coords,
		DesignerRegion,
		Dir,
		Headers,
		Longdesc,
		Rel,
		Scope,
		Shape,
		Usemap,
		VCardName
#endif
	}
}
