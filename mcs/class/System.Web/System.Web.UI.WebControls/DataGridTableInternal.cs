/**
 * Namespace:   System.Web.UI.WebControls
 * Class:       DataGridTableInternal
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	internal class DataGridTableInternal : Table
	{
		public DataGridTableInternal() : base()
		{
			PreventAutoID ();
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(ID == null)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Id, Parent.ClientID);
			}
		}
	}
}
