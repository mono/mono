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
