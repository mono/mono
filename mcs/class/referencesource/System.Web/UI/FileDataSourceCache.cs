//------------------------------------------------------------------------------
// <copyright file="FileDataSourceCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.Collections.Specialized;
    using System.Web.Caching;


    internal sealed class FileDataSourceCache : DataSourceCache {

        private StringCollection _fileDependencies;


        /// <devdoc>
        /// Sets the list of files that the cache entry will be dependent on.
        /// These values are not stored in view state.
        /// </devdoc>
        public StringCollection FileDependencies {
            get {
                if (_fileDependencies == null) {
                    _fileDependencies = new StringCollection();
                }
                return _fileDependencies;
            }
        }


        /// <devdoc>
        /// Saves data to the ASP.NET cache using the specified key.
        /// </devdoc>
        protected override void SaveDataToCacheInternal(string key, object data, CacheDependency dependency) {
            int fileCount = FileDependencies.Count;
            string[] filenames = new string[fileCount];
            FileDependencies.CopyTo(filenames, 0);
            CacheDependency fileDependency = new CacheDependency(0, filenames);

            if (dependency != null) {
                // There was another dependency passed in, aggregate them
                AggregateCacheDependency aggregateDependency = new AggregateCacheDependency();
                aggregateDependency.Add(fileDependency, dependency);
                dependency = aggregateDependency;
            }
            else {
                // No other dependencies, just the file one
                dependency = fileDependency;
            }

            base.SaveDataToCacheInternal(key, data, dependency);
        }
    }
}

