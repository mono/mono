/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       TableDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class TableDesigner : TextControlDesigner
	{
		public TableDesigner(): base()
		{
		}

		[MonoTODO]
		public override string GetDesignTimeHtml()
		{
			if(Component != null && Component is Table)
			{
				Table table = (Table) Component;
				throw new NotImplementedException();
			}
			return String.Empty;
		}

		[MonoTODO]
		public override string GetPersistInnerHtml()
		{
			throw new NotImplementedException();
		}
	}
}