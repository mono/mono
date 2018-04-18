//------------------------------------------------------------------------------
// <copyright file="DataSourceCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.ComponentModel;
    using System.Web.Caching;
    using System.Web.Util;


    internal class DataSourceCache : IStateManager {


        public const int Infinite = 0;

        private bool _tracking;
        private StateBag _viewState;



        /// <devdoc>
        /// The duration, in seconds, of the expiration. The expiration policy is specified by the ExpirationPolicy property.
        /// </devdoc>
        public virtual int Duration {
            get {
                object o = ViewState["Duration"];
                if (o != null)
                    return (int)o;
                return Infinite;
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.DataSourceCache_InvalidDuration));
                }
                ViewState["Duration"] = value;
            }
        }


        /// <devdoc>
        /// Whether caching is enabled for this data source.
        /// </devdoc>
        public virtual bool Enabled {
            get {
                object o = ViewState["Enabled"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                ViewState["Enabled"] = value;
            }
        }


        /// <devdoc>
        /// The expiration policy of the cache. The duration for the expiration is specified by the Duration property.
        /// </devdoc>
        public virtual DataSourceCacheExpiry ExpirationPolicy {
            get {
                object o = ViewState["ExpirationPolicy"];
                if (o != null)
                    return (DataSourceCacheExpiry)o;
                return DataSourceCacheExpiry.Absolute;
            }
            set {
                if (value < DataSourceCacheExpiry.Absolute || value > DataSourceCacheExpiry.Sliding) {
                    throw new ArgumentOutOfRangeException(SR.GetString(SR.DataSourceCache_InvalidExpiryPolicy));
                }
                ViewState["ExpirationPolicy"] = value;
            }
        }


        /// <devdoc>
        /// Indicates an arbitrary cache key to make this cache entry depend on. This allows
        /// the user to further customize when this cache entry will expire.
        /// </devdoc>
        [
        DefaultValue(""),
        NotifyParentProperty(true),
        WebSysDescription(SR.DataSourceCache_KeyDependency),
        ]
        public virtual string KeyDependency {
            get {
                object o = ViewState["KeyDependency"];
                if (o != null)
                    return (string)o;
                return String.Empty;
            }
            set {
                ViewState["KeyDependency"] = value;
            }
        }


        /// <devdoc>
        /// Indicates a dictionary of state information that allows you to save and restore
        /// the state of an object across multiple requests for the same page.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        protected StateBag ViewState {
            get {
                if (_viewState == null) {
                    _viewState = new StateBag();
                    if (_tracking)
                        _viewState.TrackViewState();
                }
                return _viewState;
            }
        }



        /// <devdoc>
        /// Invalidates an ASP.NET cache entry using the specified key.
        /// SECURITY: This method should never accept user-defined inputs
        /// because it invalidates the internal ASP.net cache.
        /// </devdoc>
        public void Invalidate(string key) {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentNullException("key");
            }

            Debug.Assert(key.StartsWith(CacheInternal.PrefixDataSourceControl, StringComparison.Ordinal), "All keys passed in should start with the prefix specified in CacheInternal.PrefixDataSourceControl.");

            if (!Enabled) {
                throw new InvalidOperationException(SR.GetString(SR.DataSourceCache_CacheMustBeEnabled));
            }

            HttpRuntime.Cache.InternalCache.Remove(key);
        }


        /// <devdoc>
        /// Loads data from the ASP.NET cache using the specified key.
        /// </devdoc>
        public object LoadDataFromCache(string key) {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentNullException("key");
            }

            Debug.Assert(key.StartsWith(CacheInternal.PrefixDataSourceControl, StringComparison.Ordinal), "All keys passed in should start with the prefix specified in CacheInternal.PrefixDataSourceControl.");

            if (!Enabled) {
                throw new InvalidOperationException(SR.GetString(SR.DataSourceCache_CacheMustBeEnabled));
            }

            return HttpRuntime.Cache.InternalCache.Get(key);
        }


        /// <devdoc>
        /// Loads the state of the DataSourceCache object.
        /// </devdoc>
        protected virtual void LoadViewState(object savedState) {
            if (savedState != null) {
                ((IStateManager)ViewState).LoadViewState(savedState);
            }
        }


        /// <devdoc>
        /// Saves data to the ASP.NET cache using the specified key.
        /// </devdoc>
        public void SaveDataToCache(string key, object data) {
            SaveDataToCache(key, data, null);
        }


        /// <devdoc>
        /// Saves data to the ASP.NET cache using the specified key and makes
        /// this entry dependent on the specified dependency.
        /// </devdoc>
        public void SaveDataToCache(string key, object data, CacheDependency dependency) {
            SaveDataToCacheInternal(key, data, dependency);
        }


        /// <devdoc>
        /// Saves data to the ASP.NET cache using the specified key, and makes
        /// it dependent on the specified CacheDependency object.
        /// Override this method if you need to create your own cache dependencies
        /// and call this base implementation to actually save the data to the
        /// cache with the standard properties (expiration policy, duration, etc.).
        /// </devdoc>
        protected virtual void SaveDataToCacheInternal(string key, object data, CacheDependency dependency) {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentNullException("key");
            }

            Debug.Assert(key.StartsWith(CacheInternal.PrefixDataSourceControl, StringComparison.Ordinal), "All keys passed in should start with the prefix specified in CacheInternal.PrefixDataSourceControl.");

            if (!Enabled) {
                throw new InvalidOperationException(SR.GetString(SR.DataSourceCache_CacheMustBeEnabled));
            }

            DateTime utcAbsoluteExpiryTime = Cache.NoAbsoluteExpiration;
            TimeSpan slidingExpiryTimeSpan = Cache.NoSlidingExpiration;
            switch (ExpirationPolicy) {
                case DataSourceCacheExpiry.Absolute:
                    // The caching APIs for absolute expiry expect a duration of 0 to mean no expiry,
                    // but for us it means infinite so we use Int32.MaxValue instead.
                    utcAbsoluteExpiryTime = DateTime.UtcNow.AddSeconds(Duration == 0 ? Int32.MaxValue : Duration);
                    break;
                case DataSourceCacheExpiry.Sliding:
                    slidingExpiryTimeSpan = TimeSpan.FromSeconds(Duration);
                    break;
            }

            AggregateCacheDependency aggregateCacheDependency = new AggregateCacheDependency();

            // Set up key dependency, if any
            string[] keyDependencies = null;
            if (KeyDependency.Length > 0) {
                keyDependencies = new string[] { KeyDependency };
                aggregateCacheDependency.Add(new CacheDependency[] { new CacheDependency(null, keyDependencies) });
            }

            // If there are any additional dependencies, create a new CacheDependency for them
            if (dependency != null) {
                aggregateCacheDependency.Add(new CacheDependency[] { dependency });
            }

            HttpRuntime.Cache.InternalCache.Insert(key, data, new CacheInsertOptions() {
                                                                Dependencies = aggregateCacheDependency,
                                                                AbsoluteExpiration = utcAbsoluteExpiryTime,
                                                                SlidingExpiration = slidingExpiryTimeSpan
                                                            });
        }


        /// <devdoc>
        /// Saves the current state of the DataSourceCache object.
        /// </devdoc>
        protected virtual object SaveViewState() {
            return (_viewState != null ? ((IStateManager)_viewState).SaveViewState() : null);
        }


        /// <devdoc>
        /// Starts tracking view state.
        /// </devdoc>
        protected void TrackViewState() {
            _tracking = true;

            if (_viewState != null) {
                _viewState.TrackViewState();
            }
        }


        #region IStateManager implementation

        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return _tracking;
            }
        }


        /// <internalonly/>
        void IStateManager.LoadViewState(object savedState) {
            LoadViewState(savedState);
        }


        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }
        #endregion
    }
}

