//------------------------------------------------------------------------------
// <copyright file="DataSourceHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;

    internal sealed class DataSourceHelper {

        private DataSourceHelper() {
        }

        internal static IEnumerable GetResolvedDataSource(object dataSource, string dataMember) {
            if (dataSource == null)
                return null;

            IListSource listSource = dataSource as IListSource;
            if (listSource != null) {
                IList memberList = listSource.GetList();

                if (listSource.ContainsListCollection == false) {
                    // the returned list is itself the list we need to bind to
                    // NOTE: I am ignoring the DataMember parameter... ideally we might have
                    //       thrown an exception, but this would mess up design-time usage
                    //       where the user may change the data source from a DataSet to a
                    //       DataTable.
                    return (IEnumerable)memberList;
                }

                if ((memberList != null) && (memberList is ITypedList)) {
                    ITypedList typedMemberList = (ITypedList)memberList;

                    PropertyDescriptorCollection propDescs = typedMemberList.GetItemProperties(new PropertyDescriptor[0]);
                    if ((propDescs != null) && (propDescs.Count != 0)) {
                        PropertyDescriptor listProperty = null;

                        if (String.IsNullOrEmpty(dataMember)) {
                            listProperty = propDescs[0];
                        }
                        else {
                            listProperty = propDescs.Find(dataMember, true);
                        }

                        if (listProperty != null) {
                            object listRow = memberList[0];
                            object list = listProperty.GetValue(listRow);

                            if ((list != null) && (list is IEnumerable)) {
                                return (IEnumerable)list;
                            }
                        }

                        throw new HttpException(SR.GetString(SR.ListSource_Missing_DataMember, dataMember));
                    }
                    else {
                        throw new HttpException(SR.GetString(SR.ListSource_Without_DataMembers));
                    }
                }
            }

            if (dataSource is IEnumerable) {
                return (IEnumerable)dataSource;
            }

            return null;
        }
    }
}

