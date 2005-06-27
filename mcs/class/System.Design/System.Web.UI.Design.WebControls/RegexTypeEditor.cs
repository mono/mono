
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
 * Class:       RegexTypeEditor
 *
 * Author:      Gaurav Vaish
 * Maintainer:  mastergaurav AT users DOT sf DOT net
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web.UI.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls
{
	public class RegexTypeEditor : UITypeEditor
	{
		public RegexTypeEditor() : base()
		{
		}

		public override object EditValue(ITypeDescriptorContext context,
		                       IServiceProvider provider, object value)
		{
			if(provider != null)
			{
				IWindowsFormsEditorService winEdit = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				ISite site = null;
				if(winEdit != null)
				{
					if(context.Instance is IComponent)
					{
						site = ((IComponent)context.Instance).Site;
					} else if(context.Instance is object[] &&
					          ((object[])context.Instance)[0] is IComponent)
					{
						site = ((IComponent)(((object[])context.Instance)[0])).Site;
					}
					RegexEditorDialog dlg = new RegexEditorDialog(site);
					dlg.RegularExpression = value.ToString();
					DialogResult res = dlg.ShowDialog();
					if(res == DialogResult.OK)
						value = dlg.RegularExpression;
				}
			}
			return value;
		}
		
		public override UITypeEditorEditStyle GetEditStyle(
		                                      ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}
	}
}
