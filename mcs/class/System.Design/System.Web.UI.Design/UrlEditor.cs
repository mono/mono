//
// System.Web.UI.Design.UrlEditor
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
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
using System.Drawing.Design;

namespace System.Web.UI.Design {

	public class UrlEditor : UITypeEditor {

		public UrlEditor ()
		{
		}

		protected virtual string Caption {
			get {
				return "Select URL";
			}
		}

		protected virtual string Filter {
			get {
				return "All Files(*.*)|*.*|";
			}
		}

		protected virtual UrlBuilderOptions Options {
			get {
				return UrlBuilderOptions.None;
			}
		}

		[MonoTODO]
		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}
	}
}

