/**
 * Project   : Mono
 * Namespace : System.Web.Mobile.Util
 * Class     : DataSourceHelper
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;
using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.MobileControls;

namespace System.Web.Mobile.Util
{
	internal class DataSourceHelper
	{
		private DataSourceHelper()
		{
		}

		[MonoTODO("Have_to_see_how_I_did_in_WebControls")]
		public static IEnumerable GetResolvedDataSource(object dataSource,
		                                                string dataMember)
		{
			IEnumerable retVal = null;
			if(dataSource != null)
			{
				throw new NotImplementedException();
			}
			return retVal;
		}
	}
}
