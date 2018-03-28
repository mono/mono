//------------------------------------------------------------------------------
// <copyright file="ReadOnlyHierarchicalDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    /// Helper class for hierarchical data bound controls to wrap an generic
    /// data source with a strongly typed IHierarchicalDataSource. This class
    /// automatically handles other IHierarchicalDataSources and
    /// IHierarchicalEnumerables. This class uses
    /// ReadOnlyHierarchicalDataSourceView to represent individual views.
    /// </devdoc>
    internal sealed class ReadOnlyHierarchicalDataSource : IHierarchicalDataSource {

        private object _dataSource;

        public ReadOnlyHierarchicalDataSource(object dataSource) {
            Debug.Assert(dataSource == null || (dataSource is IHierarchicalEnumerable || dataSource is IHierarchicalDataSource), "Expected dataSource to be either null, an IHierarchicalEnumerable, or an IHierarchicalDataSource.");
            _dataSource = dataSource;
        }

        #region Implementation of IHierarchicalDataSource
        event EventHandler IHierarchicalDataSource.DataSourceChanged {
            add {
            }
            remove {
            }
        }

        /// <devdoc>
        /// Check for IHierarchicalDataSource and IHierarchicalEnumerable, and
        /// return an approprite HierarchicalDataSourceView.
        /// </devdoc>
        HierarchicalDataSourceView IHierarchicalDataSource.GetHierarchicalView(string viewPath) {
            // Check first for IHierarchicalDataSource
            IHierarchicalDataSource ds = _dataSource as IHierarchicalDataSource;
            if (ds != null) {
                return ds.GetHierarchicalView(viewPath);
            }

            IHierarchicalEnumerable enumerable = _dataSource as IHierarchicalEnumerable;
            if (enumerable != null && viewPath != null && viewPath.Length != 0) {
                throw new InvalidOperationException(SR.GetString(SR.ReadOnlyHierarchicalDataSourceView_CantAccessPathInEnumerable));
            }
            return new ReadOnlyHierarchicalDataSourceView(enumerable);
        }
        #endregion
    }
}

