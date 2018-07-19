//------------------------------------------------------------------------------
// <copyright file="DataSourceHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;

namespace System.Web.UI.MobileControls 
{

    /*
     * Data Source Helper class. Copied fairly verbatim from ASP.NET code base, and modified
     * to match our coding standards and, more importantly, use our exceptions.
     * The ASP.NET file is /system/web/ui/DataSourceHelper.cs (a private class)
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class DataSourceHelper 
    {

        private DataSourceHelper() 
        {
        }

        internal static IEnumerable GetResolvedDataSource(Object dataSource, String dataMember) 
        {
            if (dataSource == null)
            {
                return null;
            }

            IListSource listSource = dataSource as IListSource;
            if (listSource != null)
            {
                IList memberList = listSource.GetList();

                if (listSource.ContainsListCollection == false) 
                {
                    // The returned list is itself the list we need to bind to.
                    // (Ignore DataMember parameter.)
                    return (IEnumerable)memberList;
                }

                if ((memberList != null) && (memberList is ITypedList)) 
                {
                    ITypedList typedMemberList = (ITypedList)memberList;

                    PropertyDescriptorCollection propDescs = 
                        typedMemberList.GetItemProperties (new PropertyDescriptor[0]);
                    if ((propDescs != null) && (propDescs.Count != 0)) 
                    {
                        PropertyDescriptor listProperty = null;

                        if ((dataMember == null) || (dataMember.Length == 0)) 
                        {
                            listProperty = propDescs[0];
                        }
                        else 
                        {
                            listProperty = propDescs.Find(dataMember, true);
                        }

                        if (listProperty != null) 
                        {
                            Object listRow = memberList[0];
                            Object list = listProperty.GetValue(listRow);

                            if ((list != null) && (list is IEnumerable)) 
                            {
                                return (IEnumerable)list;
                            }
                        }

                        throw new ArgumentException(
                            SR.GetString(SR.DataSourceHelper_MissingDataMember,
                                         dataMember));
                    }
                    else 
                    {
                        throw new ArgumentException(
                            SR.GetString(SR.DataSourceHelper_DataSourceWithoutDataMember,
                                         "List DataSource"));
                    }
                }
            }

            if (dataSource is IEnumerable) 
            {
                return (IEnumerable)dataSource;
            }

            return null;
        }
    }
}

