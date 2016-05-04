namespace System.Web.DynamicData.Util {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.Resources;
    using System.Web.UI;
    using System.ComponentModel;
    using IDataBoundControlInterface = System.Web.UI.WebControls.IDataBoundControl;

    internal static class MetaTableHelper {

        private static object s_mappingKey = new object();

        internal static Dictionary<object, MappingInfo> GetMapping(HttpContextBase httpContext) {
            Dictionary<object, MappingInfo> mapping = httpContext.Items[s_mappingKey] as Dictionary<object, MappingInfo>;
            if (mapping == null) {
                mapping = new Dictionary<object, MappingInfo>();
                httpContext.Items[s_mappingKey] = mapping;
            }
            return mapping;
        }

        /// <summary>
        /// Gets a table from the mapping. Does not throw.
        /// </summary>
        internal static MetaTable GetTableFromMapping(HttpContextBase httpContext, object control) {
            IDictionary<object, MappingInfo> mapping = GetMapping(httpContext);
            // don't throw if no mapping found
            MappingInfo mappingInfo;
            if (mapping.TryGetValue(control, out mappingInfo)) {
                return mappingInfo.Table;
            }
            return null;
        }

        private static MappingInfo GetMappingInfo(object control, HttpContextBase httpContext) {
            IDictionary<object, MappingInfo> mapping = GetMapping(httpContext);
            MappingInfo mappingInfo;
            if (!mapping.TryGetValue(control, out mappingInfo)) {
                mappingInfo = new MappingInfo();
                mapping[control] = mappingInfo;
            }
            return mappingInfo;
        }

        internal static void SetTableInMapping(HttpContextBase httpContext, object control, MetaTable table, IDictionary<string, object> defaultValues) {
            MappingInfo mappingInfo = GetMappingInfo(control, httpContext);
            mappingInfo.Table = table;
            if (defaultValues != null) {
                mappingInfo.DefaultValueMapping = new DefaultValueMapping(defaultValues);
            }
        }

        internal static MetaTable GetTableWithFullFallback(IDataSource dataSource, HttpContextBase context) {
            MetaTable table = GetTableFromMapping(context, dataSource);
            if (table != null) {
                return table;
            }

            IDynamicDataSource dynamicDataSource = dataSource as IDynamicDataSource;
            if (dynamicDataSource != null) {
                table = GetTableFromDynamicDataSource(dynamicDataSource);
                if (table != null) {
                    return table;
                }
            }

            table = DynamicDataRouteHandler.GetRequestMetaTable(context);
            if (table != null) {
                return table;
            }

            Control c = dataSource as Control;
            string id = (c != null ? c.ID : String.Empty);
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                DynamicDataResources.MetaTableHelper_CantFindTable,
                id));
        }

        internal static MetaTable GetMetaTableFromObject(object dataSource) {
            IEnumerable enumerable = dataSource as IEnumerable;
            if (enumerable == null) {
                return null;
            }

            // Use the first item to determine the type
            // 
            foreach (object o in enumerable) {
                if (o != null) {
                    return MetaTable.GetTable(o.GetType());
                }
            }

            return null;
        }

        /// Gets a table from an IDynamicDataSource's ContextType and EntitySetName.
        /// Does not throw, returns null on failure.
        internal static MetaTable GetTableFromDynamicDataSource(IDynamicDataSource dynamicDataSource) {
            string tableName = dynamicDataSource.EntitySetName;
            Type contextType = dynamicDataSource.ContextType;
            if (contextType == null || String.IsNullOrEmpty(tableName)) {
                return null;
            }


            MetaModel model;
            if (MetaModel.MetaModelManager.TryGetModel(contextType, out model)) {
                Debug.Assert(model != null);
                MetaTable table;
                if (model.TryGetTable(tableName, out table)) {
                    return table;
                }
            }

            return null;
        }

        internal static MetaTable FindMetaTable(Control current) {
            return FindMetaTable(current, HttpContext.Current.ToWrapper());
        }

        internal static DefaultValueMapping GetDefaultValueMapping(Control current, HttpContextBase context) {
            IDictionary<object, MappingInfo> mapping = GetMapping(context);
            if (!(current is INamingContainer)) {
                current = current.NamingContainer;
            }

            for (; current != null; current = current.NamingContainer) {
                MappingInfo mappingInfo;
                // If we find a mapping then return that value
                if (mapping.TryGetValue(current, out mappingInfo)) {
                    return mappingInfo.DefaultValueMapping;
                }
            }
            return null;
        }

        internal static MetaTable FindMetaTable(Control current, HttpContextBase context) {
            MetaTable table = null;
            // Start from the first naming container
            if (!(current is INamingContainer)) {
                current = current.NamingContainer;
            }
            for (; current != null; current = current.NamingContainer) {
                // Find the first table mapped to a control
                table = GetTableFromMapping(context, current);
                if (table != null) {
                    return table;
                }

                IDataBoundControlInterface dataBoundControl = DataControlHelper.GetDataBoundControl(current, false /*failIfNotFound*/);

                // Not a data control: continue searching
                if (dataBoundControl == null) {
                    continue;
                }

                IDataSource dataSourceControl = dataBoundControl.DataSourceObject;
                // Check if it's associated with a DataSource or can be retrieved from the current route
                if (dataSourceControl != null) {
                    return GetTableWithFullFallback(dataSourceControl, context);
                }

                // Check if it has a datasource (i.e. not a control, but directly some data)
                object dataSource = dataBoundControl.DataSource;
                if (dataSource != null) {
                    // Try to get a MetaTable from it.  If so, we're done
                    table = GetMetaTableFromObject(dataSource);
                    if (table != null) {
                        return table;
                    }
                }
            }

            return null;
        }
    }
}
