/**
 * Namespace: System.Web.UI.Util
 * Class:     DataSourceHelper
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  10%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Web.Util
{
	internal class DataSourceHelper
	{
		public static IEnumerable GetResolvedDataSource(object source, string member)
		{
			if(source==null)
				return null;
			if(source is IListSource)
			{
				IListSource ils = (IListSource)source;
				IList       il  = ils.GetList();
				if(ils.ContainsListCollection)
				{
					return il;
				}
				if(il is ITypedList)
				{
					ITypedList itl = (ITypedList)il;
					PropertyDescriptorCollection pdc = itl.GetItemProperties(new PropertyDescriptor[0]);
					PropertyDescriptor pd = null;
					if(pdc != null)
					{
						if(pdc.Count > 0)
						{
							if(member != null)
							{
								if(member.Length > 0)
								{
									pd = pdc.Find(member, true);
								} else
								{
									pd = pdc[0];
								}
							}
						}
					}
					if(pd!=null)
					{
						object o = pd.GetValue(il[0]);
						if(o!=null)
						{
							if(o is IEnumerable)
								return (IEnumerable)o;
						}
						throw new HttpException("ListSource Empty"); // no data in ListSource object
					}
				}
			} else if(source is IEnumerable)
			{
				return (IEnumerable)source;
			}
			return null;
		}
	}
}
