//------------------------------------------------------------------------------
// <copyright file="HttpCookieCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Collection of Http cookies for request and response intrinsics
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>
    ///       Provides a type-safe
    ///       way to manipulate HTTP cookies.
    ///    </para>
    /// </devdoc>
    public sealed class HttpCookieCollection : NameObjectCollectionBase {
        // Response object to notify about changes in collection
        private HttpResponse _response;

        // cached All[] arrays
        private HttpCookie[] _all;
        private String[] _allKeys;
        private bool    _changed;

        // for implementing granular request validation
        private ValidateStringCallback _validationCallback;
        private HashSet<string> _keysAwaitingValidation;

        internal HttpCookieCollection(HttpResponse response, bool readOnly)
            : base(StringComparer.OrdinalIgnoreCase)  {
            _response = response;
            IsReadOnly = readOnly;
        }


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the HttpCookieCollection
        ///       class.
        ///    </para>
        /// </devdoc>
        public HttpCookieCollection(): base(StringComparer.OrdinalIgnoreCase)  {
        }

        // This copy constructor is used by the granular request validation feature. The collections are mutable once
        // created, but nobody should ever be mutating them, so it's ok for these to be out of [....]. Additionally,
        // we don't copy _response since this should only ever be called for the request cookies.
        internal HttpCookieCollection(HttpCookieCollection col)
            : base(StringComparer.OrdinalIgnoreCase) {

            // We explicitly don't copy validation-related fields, as we want the copy to "reset" validation state.

            // Copy the file references from the original collection into this instance
            for (int i = 0; i < col.Count; i++) {
                ThrowIfMaxHttpCollectionKeysExceeded();
                string key = col.BaseGetKey(i);
                object value = col.BaseGet(i);
                BaseAdd(key, value);
            }

            IsReadOnly = col.IsReadOnly;
        }

        internal bool Changed {
            get { return _changed; }
            set { _changed = value; }
        }
        internal void AddCookie(HttpCookie cookie, bool append) {
            ThrowIfMaxHttpCollectionKeysExceeded();

            _all = null;
            _allKeys = null;

            if (append) {
                // DevID 251951	Cookie is getting duplicated by ASP.NET when they are added via a native module
                // Need to not double add response cookies from native modules
                if (!cookie.FromHeader) {
                    // mark cookie as new
                    cookie.Added = true;
                }
                BaseAdd(cookie.Name, cookie);
            }
            else {
                if (BaseGet(cookie.Name) != null) {                   
                    // mark the cookie as changed because we are overriding the existing one
                    cookie.Changed = true;
                }
                BaseSet(cookie.Name, cookie);
            }
        }

        // MSRC 12038: limit the maximum number of items that can be added to the collection,
        // as a large number of items potentially can result in too many hash collisions that may cause DoS
        private void ThrowIfMaxHttpCollectionKeysExceeded() {
            if (Count >= AppSettings.MaxHttpCollectionKeys) {
                throw new InvalidOperationException(SR.GetString(SR.CollectionCountExceeded_HttpValueCollection, AppSettings.MaxHttpCollectionKeys));
            }
        }

        internal void EnableGranularValidation(ValidateStringCallback validationCallback) {
            // Iterate over all the keys, adding each to the set containing the keys awaiting validation.
            // Unlike dictionaries, HashSet<T> can contain null keys, so don't need to special-case them.
            _keysAwaitingValidation = new HashSet<string>(Keys.Cast<string>(), StringComparer.OrdinalIgnoreCase);
            _validationCallback = validationCallback;
        }

        private void EnsureKeyValidated(string key, string value) {
            if (_keysAwaitingValidation == null) {
                // If dynamic validation hasn't been enabled, no-op.
                return;
            }

            if (!_keysAwaitingValidation.Contains(key)) {
                // If this key has already been validated (or is excluded), no-op.
                return;
            }

            // If validation fails, the callback will throw an exception. If validation succeeds,
            // we can remove it from the candidates list. A note:
            // - Eager validation skips null/empty values, so we should, also.
            if (!String.IsNullOrEmpty(value)) {
                _validationCallback(key, value);
            }
            _keysAwaitingValidation.Remove(key);
        }

        internal void MakeReadOnly() {
            IsReadOnly = true;
        }

        internal void RemoveCookie(String name) {
            _all = null;
            _allKeys = null;

            BaseRemove(name);

            _changed = true;
        }

        internal void Reset() {
            _all = null;
            _allKeys = null;

            BaseClear();
            _changed = true;
            _keysAwaitingValidation = null;
        }

        //
        //  Public APIs to add / remove
        //


        /// <devdoc>
        ///    <para>
        ///       Adds a cookie to the collection.
        ///    </para>
        /// </devdoc>
        public void Add(HttpCookie cookie) {
            if (_response != null)
                _response.BeforeCookieCollectionChange();

            AddCookie(cookie, true);

            if (_response != null)
                _response.OnCookieAdd(cookie);
        }
        

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Array dest, int index) {
            if (_all == null) {
                int n = Count;
                HttpCookie[] all = new HttpCookie[n];

                for (int i = 0; i < n; i++)
                    all[i] = Get(i);

                _all = all; // wait until end of loop to set _all reference in case Get throws
            }
            _all.CopyTo(dest, index);
        }


        /// <devdoc>
        ///    <para> Updates the value of a cookie.</para>
        /// </devdoc>
        public void Set(HttpCookie cookie) {
            if (_response != null)
                _response.BeforeCookieCollectionChange();

            AddCookie(cookie, false);

            if (_response != null)
                _response.OnCookieCollectionChange();
        }


        /// <devdoc>
        ///    <para>
        ///       Removes a cookie from the collection.
        ///    </para>
        /// </devdoc>
        public void Remove(String name) {
            if (_response != null)
                _response.BeforeCookieCollectionChange();

            RemoveCookie(name);

            if (_response != null)
                _response.OnCookieCollectionChange();
        }


        /// <devdoc>
        ///    <para>
        ///       Clears all cookies from the collection.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            Reset();
        }

        //
        //  Access by name
        //


        /// <devdoc>
        /// <para>Returns an <see cref='System.Web.HttpCookie'/> item from the collection.</para>
        /// </devdoc>
        public HttpCookie Get(String name) {
            HttpCookie cookie = (HttpCookie)BaseGet(name);

            if (cookie == null && _response != null) {
                // response cookies are created on demand
                cookie = new HttpCookie(name);
                AddCookie(cookie, true);
                _response.OnCookieAdd(cookie);
            }

            if (cookie != null) {
                EnsureKeyValidated(name, cookie.Value);
            }

            return cookie;
        }


        /// <devdoc>
        ///    <para>Indexed value that enables access to a cookie in the collection.</para>
        /// </devdoc>
        public HttpCookie this[String name]
        {
            get { return Get(name);}
        }

        //
        // Indexed access
        //


        /// <devdoc>
        ///    <para>
        ///       Returns an <see cref='System.Web.HttpCookie'/>
        ///       item from the collection.
        ///    </para>
        /// </devdoc>
        public HttpCookie Get(int index) {
            HttpCookie cookie = (HttpCookie)BaseGet(index);

            // Call GetKey so that we can pass the key to the validation routine.
            if (cookie != null) {
                EnsureKeyValidated(GetKey(index), cookie.Value);
            }
            return cookie;
        }


        /// <devdoc>
        ///    <para>
        ///       Returns key name from collection.
        ///    </para>
        /// </devdoc>
        public String GetKey(int index) {
            return BaseGetKey(index);
        }


        /// <devdoc>
        ///    <para>
        ///       Default property.
        ///       Indexed property that enables access to a cookie in the collection.
        ///    </para>
        /// </devdoc>
        public HttpCookie this[int index]
        {
            get { return Get(index);}
        }

        //
        // Access to keys and values as arrays
        //
        
        /*
         * All keys
         */

        /// <devdoc>
        ///    <para>
        ///       Returns
        ///       an array of all cookie keys in the cookie collection.
        ///    </para>
        /// </devdoc>
        public String[] AllKeys {
            get {
                if (_allKeys == null)
                    _allKeys = BaseGetAllKeys();

                return _allKeys;
            }
        }
    }
}
