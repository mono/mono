/**
 * Namespace:   System.Web.UI.Design
 * Class:       DataBindingHandler
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel.Design;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
	public abstract class DataBindingHandler
	{
		protected DataBindingHandler()
		{
		}

		public abstract void DataBindControl(IDesignerHost designerHost,
		                                     Control control);
	}
}
