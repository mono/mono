
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
 * Class:       BaseDataListPageInternal
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
	abstract class BaseDataListPageInternal : ComponentEditorPage
	{
		private bool isDataGridMode;

		public BaseDataListPageInternal()
		{
		}

		protected abstract string HelpKeyword { get; }

		protected bool IsDataGridMode
		{
			get
			{
				return isDataGridMode;
			}
		}

		public override void ShowHelp()
		{
			ISite site = GetSelectedComponent().Site;
			IHelpService service = (IHelpService)site.GetService(
			                                          typeof(IHelpService));
			if(service != null)
			{
				service.ShowHelpFromKeyword(HelpKeyword);
			}
		}

		public override bool SupportsHelp()
		{
			return true;
		}

		public override void SetComponent(IComponent component)
		{
			base.SetComponent(component);
			isDataGridMode = (GetBaseControl() is DataGrid);
		}

		protected BaseDataList GetBaseControl()
		{
			return (BaseDataList)GetSelectedComponent();
		}

		protected BaseDataListDesigner GetBaseDesigner()
		{
			BaseDataListDesigner retVal = null;
			ISite site = GetSelectedComponent().Site;
			IDesignerHost designer = (IDesignerHost)site.GetService(
			                                      typeof(IDesignerHost));
			if(designer != null)
			{
				retVal = (BaseDataListDesigner)designer.GetDesigner(
				                                GetSelectedComponent());
			}
			return retVal;
		}
	}
}
