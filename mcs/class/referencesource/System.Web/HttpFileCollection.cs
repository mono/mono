//------------------------------------------------------------------------------
// <copyright file="HttpFileCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Collection of posted files for the request intrinsic
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>
    ///       Accesses incoming files uploaded by a client (using
    ///       multipart MIME and the Http Content-Type of multipart/formdata).
    ///    </para>
    /// </devdoc>
    public sealed class HttpFileCollection : NameObjectCollectionBase {
        // cached All[] arrays
        private HttpPostedFile[] _all;
        private String[] _allKeys;

        // for implementing granular request validation
        private ValidateStringCallback _validationCallback;
        private HashSet<HttpPostedFile> _filesAwaitingValidation;


        [SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.Collections.Specialized.NameObjectCollectionBase.#ctor(System.Collections.IEqualityComparer)", Justification = @"By design")]
        internal HttpFileCollection()
            : base(StringComparer.InvariantCultureIgnoreCase) {
        }

        // This copy constructor is used by the granular request validation feature. Since these collections are immutable
        // once created, it's ok for us to have two collections containing the same data.
        internal HttpFileCollection(HttpFileCollection col)
            : this() {

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

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Array dest, int index) {
            if (_all == null) {
                int n = Count;
                HttpPostedFile[] all = new HttpPostedFile[n];
                for (int i = 0; i < n; i++)
                    all[i] = Get(i);
                _all = all; // wait until end of loop to set _all reference in case Get throws
            }

            if (_all != null) {
                _all.CopyTo(dest, index);
            }
        }

        internal void AddFile(String key, HttpPostedFile file) {
            ThrowIfMaxHttpCollectionKeysExceeded();

            _all = null;
            _allKeys = null;

            BaseAdd(key, file);
        }

        // MSRC 12038: limit the maximum number of items that can be added to the collection,
        // as a large number of items potentially can result in too many hash collisions that may cause DoS
        private void ThrowIfMaxHttpCollectionKeysExceeded() {
            if (Count >= AppSettings.MaxHttpCollectionKeys) {
                throw new InvalidOperationException(SR.GetString(SR.CollectionCountExceeded_HttpValueCollection, AppSettings.MaxHttpCollectionKeys));
            }
        }

        internal void EnableGranularValidation(ValidateStringCallback validationCallback) {
            // Iterate over all the files, adding each to the set containing the files awaiting validation.
            // Unlike dictionaries, HashSet<T> can contain null keys, so don't need to special-case them.
            _filesAwaitingValidation = new HashSet<HttpPostedFile>();
            for (int i = 0; i < Count; i++) {
                _filesAwaitingValidation.Add((HttpPostedFile)BaseGet(i));
            }
            _validationCallback = validationCallback;
        }

        private void EnsureFileValidated(HttpPostedFile file) {
            if (_filesAwaitingValidation == null) {
                // If dynamic validation hasn't been enabled, no-op.
                return;
            }

            if (!_filesAwaitingValidation.Contains(file)) {
                // If this file has already been validated (or is excluded), no-op.
                return;
            }

            // If validation fails, the callback will throw an exception. If validation succeeds,
            // we can remove it from the candidates list. Key is unused.
            _validationCallback(null /* key */, file.FileName);
            _filesAwaitingValidation.Remove(file);
        }

        //
        //  Access by name
        //


        /// <devdoc>
        ///    <para>
        ///       Returns a file from
        ///       the collection by file name.
        ///    </para>
        /// </devdoc>
        public HttpPostedFile Get(String name) {
            HttpPostedFile file = (HttpPostedFile)BaseGet(name);
            if (file != null) {
                EnsureFileValidated(file);
            }
            return file;
        }

        /// <devdoc>
        /// Returns all files from this collection that match the given name.
        /// This is to support multi-file uploads (either via HTML5 or JS emulators).
        /// This method returns a new collection instance for each invocation and callers are
        /// encouraged to call this method once per name per request.
        /// </devdoc>

        [SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.String.Equals(System.String,System.String,System.StringComparison)", Justification = @"By design")]
        public IList<HttpPostedFile> GetMultiple(string name) {
            List<HttpPostedFile> result = new List<HttpPostedFile>();
            for (int i = 0; i < Count; i++) {
                string key = GetKey(i);
                // Use InvariantCultureIgnoreCase since this is the comparison used for this collection
                if (String.Equals(key, name, StringComparison.InvariantCultureIgnoreCase)) {
                    // Call the Get() method instead of looking at the _all array directly
                    // to ensure that request validation happens for the file.
                    result.Add(Get(i));
                }
            }
            return result.AsReadOnly();
        }


        /// <devdoc>
        ///    <para>Returns item value from collection.</para>
        /// </devdoc>
        public HttpPostedFile this[String name]
        {
            get { return Get(name);}
        }

        //
        // Indexed access
        //


        /// <devdoc>
        ///    <para>
        ///       Returns a file from
        ///       the file collection by index.
        ///    </para>
        /// </devdoc>
        public HttpPostedFile Get(int index) {
            HttpPostedFile file = (HttpPostedFile)BaseGet(index);
            if (file != null) {
                EnsureFileValidated(file);
            }
            return file;
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
        ///       Returns an
        ///       item from the collection.
        ///    </para>
        /// </devdoc>
        public HttpPostedFile this[int index]
        {
            get { return Get(index);}
        }

        //
        // Access to keys and values as arrays
        //
        

        /// <devdoc>
        ///    <para>
        ///       Creates an
        ///       array of keys in the collection.
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
