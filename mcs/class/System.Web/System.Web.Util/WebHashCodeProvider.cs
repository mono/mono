/**

 * Namespace: System.Web.Util
 * Class:     WebHashCodeProvider
 *

 * Author:  Gaurav Vaish

 * Maintainer: gvaish@iitk.ac.in

 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>

 * Implementation: yes

 * Status:  ??%

 *

 * (C) Gaurav Vaish (2001)

 */


using System.Collections;
using System.Globalization;

namespace System.Web.Util
{
	public class WebHashCodeProvider : IHashCodeProvider
	{
		private static IHashCodeProvider defHcp;

		public WebHashCodeProvider()
		{
		}
		
		int IHashCodeProvider.GetHashCode(object key)
		{
			return Default.GetHashCode(key);
		}

		public static IHashCodeProvider Default
		{
			get
			{
				if(defHcp==null)
				{
					 defHcp = new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture);
				}
				return defHcp;
			}
		}
	}
}
