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
