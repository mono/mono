
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
using System.ComponentModel.Design;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design {
	[Obsolete ("Use IControlDesignerTag and IControlDesignerView instead")]
	public interface IHtmlControlDesignerBehavior {
		object GetAttribute (string attribute, bool ignoreCase);
		object GetStyleAttribute (string attribute, bool designTimeOnly, bool ignoreCase);
		void RemoveAttribute (string attribute, bool ignoreCase);
		void RemoveStyleAttribute (string attribute, bool designTimeOnly, bool ignoreCase);
		void SetAttribute (string attribute, object value, bool ignoreCase);
		void SetStyleAttribute (string attribute, bool designTimeOnly, object value, bool ignoreCase);
		HtmlControlDesigner Designer { get; set; }
		object DesignTimeElement { get; }
	}
}