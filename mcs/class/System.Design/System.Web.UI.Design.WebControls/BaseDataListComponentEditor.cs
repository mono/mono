/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       BaseDataListComponentEditor
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
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.Web.UI.Design.WebControls
{
	public abstract class BaseDataListComponentEditor : WindowsFormsComponentEditor
	{
		private int initialPage;

		public BaseDataListComponentEditor(int initialPage) : base()
		{
			this.initialPage = initialPage;
		}

		public override bool EditComponent(ITypeDescriptorContext context,
		                                   object obj, IWin32Window parent)
		{
			IComponent comp = (IComponent) obj;
			ISite      site = comp.Site;
			bool retVal  = false;
			bool inTemplateMode = false;

			if(site != null)
			{
				IDesignerHost dh = (IDesignerHost)site.GetService(typeof(IDesignerHost));
				inTemplateMode = ((TemplatedControlDesigner)dh.GetDesigner(comp)).InTemplateMode;
			}
			if(inTemplateMode)
			{
				throw new NotImplementedException();
			} else
			{
				retVal = base.EditComponent(context, obj, parent);
			}
			return retVal;
		}

		protected override int GetInitialComponentEditorPageIndex()
		{
			return initialPage;
		}
	}
}
