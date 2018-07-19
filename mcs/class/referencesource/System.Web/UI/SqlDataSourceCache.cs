//------------------------------------------------------------------------------
// <copyright file="SqlDataSourceCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.Collections;
    using System.ComponentModel;
    using System.Web.Caching;


    internal sealed class SqlDataSourceCache : DataSourceCache {

        internal const string Sql9CacheDependencyDirective = "CommandNotification";


        /// <devdoc>
        /// A semi-colon delimited string indicating which databases to use for the dependency in the format "database1:table1;database2:table2".
        /// </devdoc>
        public string SqlCacheDependency {
            get {
                object o = ViewState["SqlCacheDependency"];
                if (o != null)
                    return (string)o;
                return String.Empty;
            }
            set {
                ViewState["SqlCacheDependency"] = value;
            }
        }


#if !FEATURE_PAL // FEATURE_PAL does not fully enable SQL dependencies
        /// <devdoc>
        /// Saves data to the ASP.NET cache using the specified key.
        /// </devdoc>
        protected override void SaveDataToCacheInternal(string key, object data, CacheDependency dependency) {
            string sqlCacheDependency = SqlCacheDependency;
            // Here we only create cache dependencies for SQL Server 2000 and
            // earlier that use a polling based mechanism. For SQL Server 2005
            // and after, the data source itself creates the SqlCacheDependency
            // and passes it in as a parameter.
            if (sqlCacheDependency.Length > 0 && !String.Equals(sqlCacheDependency, Sql9CacheDependencyDirective, StringComparison.OrdinalIgnoreCase)) {
                // Call internal helper method to parse the dependency list
                CacheDependency sqlDependency = System.Web.Caching.SqlCacheDependency.CreateOutputCacheDependency(sqlCacheDependency);

                if (dependency != null) {
                    // There was another dependency passed in, aggregate them
                    AggregateCacheDependency aggregateDependency = new AggregateCacheDependency();
                    aggregateDependency.Add(sqlDependency, dependency);
                    dependency = aggregateDependency;
                }
                else {
                    // No other dependencies, just the SQL one
                    dependency = sqlDependency;
                }
            }
            base.SaveDataToCacheInternal(key, data, dependency);
        }
#endif // !FEATURE_PAL
    }
}

