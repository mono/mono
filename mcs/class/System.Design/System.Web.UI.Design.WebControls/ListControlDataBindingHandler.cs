/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       ListControlDataBindingHandler
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class ListControlDataBindingHandler : DataBindingHandler
	{
		public ListControlDataBindingHandler()
		{
		}

		[MonoTODO]
		public override void DataBindControl(IDesignerHost designerHost,
		                                     Control control)
		{
			DataBinding db = ((IDataBindingsAccessor)control).DataBindings["DataSource"];
			if(db != null)
			{
				ListControl ctrl = (ListControl)designerHost;
				ctrl.Items.Clear();
				throw new NotImplementedException();
				//ctrl.Items.Add("Sample_Databound_Text"???);
			}
		}
	}
}
