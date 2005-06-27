
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
/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       DataListComponentEditor
 *
 * Author:      Gaurav Vaish
 * Maintainer:  mastergaurav AT users DOT sf DOT net
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI.Design;
using System.Windows.Forms.Design;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls
{
	public class DataListComponentEditor : BaseDataListComponentEditor
	{
		internal static int GENERAL = 0x00;
		internal static int FORMAT  = 0x01;
		internal static int BORDERS = 0x02;

		private static Type[] componentEditorPages;

		static DataListComponentEditor()
		{
			componentEditorPages = new Type[] {
				typeof(GeneralPageDataListInternal),
				typeof(FormatPageInternal),
				typeof(BordersPageInternal)
			};
		}

		public DataListComponentEditor() : base(GENERAL)
		{
		}
		
		public DataListComponentEditor(int initialPage) : base(initialPage)
		{
		}
		
		protected override Type[] GetComponentEditorPages()
		{
			return componentEditorPages;
		}
	}
}
