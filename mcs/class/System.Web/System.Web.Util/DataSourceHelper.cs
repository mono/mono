
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
			if(source != null && source is IListSource)
			{
				IListSource src = (IListSource)source;
				IList list = src.GetList();
				if(!src.ContainsListCollection)
				{
					return list;
				}
				if(list != null && list is ITypedList)
				{
					ITypedList tlist = (ITypedList)list;
					PropertyDescriptorCollection pdc = tlist.GetItemProperties(new PropertyDescriptor[0]);
					if(pdc != null && pdc.Count > 0)
					{
						PropertyDescriptor pd = null;
						if(member != null && member.Length > 0)
						{
							pd = pdc.Find(member, true);
						} else
						{
							pd = pdc[0];
						}
						if(pd != null)
						{
							object rv = pd.GetValue(list[0]);
							if(rv != null && rv is IEnumerable)
							{
								return (IEnumerable)rv;
							}
						}
						throw new HttpException(
						      HttpRuntime.FormatResourceString("ListSource_Missing_DataMember", member));
					}
					throw new HttpException(
					      HttpRuntime.FormatResourceString("ListSource_Without_DataMembers"));
				}
			}
			if(source is IEnumerable)
			{
				return (IEnumerable)source;
			}
			return null;
		}
	}
}
