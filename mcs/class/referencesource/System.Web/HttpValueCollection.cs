//------------------------------------------------------------------------------
// <copyright file="HttpValueCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Ordered String/String[] collection of name/value pairs
 * Based on NameValueCollection -- adds parsing from string, cookie collection
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Web.UI;
    using System.Web.Util;

    [Serializable()]
    internal class HttpValueCollection : NameValueCollection {

        // for implementing granular request validation
        [NonSerialized]
        private ValidateStringCallback _validationCallback;
        [NonSerialized]
        private HashSet<string> _keysAwaitingValidation;

        internal HttpValueCollection(): base(StringComparer.OrdinalIgnoreCase) {
        }

        // This copy constructor is used by the granular request validation feature. Since these collections are immutable
        // once created, it's ok for us to have two collections containing the same data.
        internal HttpValueCollection(HttpValueCollection col)
            : base(StringComparer.OrdinalIgnoreCase) {

            // We explicitly don't copy validation-related fields, as we want the copy to "reset" validation state. But we
            // do need to go against the underlying NameObjectCollectionBase directly while copying so as to avoid triggering
            // validation.
            for (int i = 0; i < col.Count; i++) {
                ThrowIfMaxHttpCollectionKeysExceeded();
                string key = col.BaseGetKey(i);
                object value = col.BaseGet(i);
                BaseAdd(key, value);
            }

            IsReadOnly = col.IsReadOnly;
        }

        internal HttpValueCollection(String str, bool readOnly, bool urlencoded, Encoding encoding): base(StringComparer.OrdinalIgnoreCase) {
            if (!String.IsNullOrEmpty(str))
                FillFromString(str, urlencoded, encoding);

            IsReadOnly = readOnly;
        }

        internal HttpValueCollection(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase) {
        }

        protected HttpValueCollection(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        /* 
         * We added granular request validation in ASP.NET 4.5 to provide a better request validation story for our developers.
         * Instead of validating the entire collection ahead of time, we'll only validate entries that are actually looked at.
         */

        internal void EnableGranularValidation(ValidateStringCallback validationCallback) {
            // Iterate over all the keys, adding each to the set containing the keys awaiting validation.
            // Unlike dictionaries, HashSet<T> can contain null keys, so don't need to special-case them.
            _keysAwaitingValidation = new HashSet<string>(Keys.Cast<string>().Where(KeyIsCandidateForValidation), StringComparer.OrdinalIgnoreCase);
            _validationCallback = validationCallback;

            // This forces CopyTo and other methods that cache entries to flush their caches, ensuring
            // that all values go through validation again.
            InvalidateCachedArrays();
        }

        internal static bool KeyIsCandidateForValidation(string key) {
            // Skip all our internal fields, since they don't need to be checked (VSWhidbey 275811)
            // 

            if (key != null && key.StartsWith(System.Web.UI.Page.systemPostFieldPrefix, StringComparison.Ordinal)) {
                return false;
            }

            return true;
        }

        private void EnsureKeyValidated(string key) {
            if (_keysAwaitingValidation == null) {
                // If dynamic validation hasn't been enabled, no-op.
                return;
            }

            if (!_keysAwaitingValidation.Contains(key)) {
                // If this key has already been validated (or is excluded), no-op.
                return;
            }

            // If validation fails, the callback will throw an exception. If validation succeeds,
            // we can remove it from the candidates list. Two notes:
            // - Use base.Get instead of this.Get so as not to enter infinite recursion.
            // - Eager validation skips null/empty values, so we should, also.
            string value = base.Get(key);
            if (!String.IsNullOrEmpty(value)) {
                _validationCallback(key, value);
            }
            _keysAwaitingValidation.Remove(key);
        }

        public override string Get(int index) {
            // Need the key so that we can pass it through validation.
            string key = GetKey(index);
            EnsureKeyValidated(key);

            return base.Get(index);
        }

        public override string Get(string name) {
            EnsureKeyValidated(name);
            return base.Get(name);
        }

        public override string[] GetValues(int index) {
            // Need the key so that we can pass it through validation.
            string key = GetKey(index);
            EnsureKeyValidated(key);

            return base.GetValues(index);
        }

        public override string[] GetValues(string name) {
            EnsureKeyValidated(name);
            return base.GetValues(name);
        }
        
        /*
         * END REQUEST VALIDATION
         */

        internal void MakeReadOnly() {
            IsReadOnly = true;
        }

        internal void MakeReadWrite() {
            IsReadOnly = false;
        }

        internal void FillFromString(String s) {
            FillFromString(s, false, null);
        }

        internal void FillFromString(String s, bool urlencoded, Encoding encoding) {
            int l = (s != null) ? s.Length : 0;
            int i = 0;

            while (i < l) {
                // find next & while noting first = on the way (and if there are more)

                ThrowIfMaxHttpCollectionKeysExceeded();

                int si = i;
                int ti = -1;

                while (i < l) {
                    char ch = s[i];

                    if (ch == '=') {
                        if (ti < 0)
                            ti = i;
                    }
                    else if (ch == '&') {
                        break;
                    }

                    i++;
                }

                // extract the name / value pair

                String name = null;
                String value = null;

                if (ti >= 0) {
                    name = s.Substring(si, ti-si);
                    value = s.Substring(ti+1, i-ti-1);
                }
                else {
                    value = s.Substring(si, i-si);
                }

                // add name / value pair to the collection

                if (urlencoded)
                    base.Add(
                       HttpUtility.UrlDecode(name, encoding),
                       HttpUtility.UrlDecode(value, encoding));
                else
                    base.Add(name, value);

                // trailing '&'

                if (i == l-1 && s[i] == '&')
                    base.Add(null, String.Empty);

                i++;
            }
        }

        internal void FillFromEncodedBytes(byte[] bytes, Encoding encoding) {
            int l = (bytes != null) ? bytes.Length : 0;
            int i = 0;

            while (i < l) {
                // find next & while noting first = on the way (and if there are more)

                ThrowIfMaxHttpCollectionKeysExceeded();

                int si = i;
                int ti = -1;

                while (i < l) {
                    byte b = bytes[i];

                    if (b == '=') {
                        if (ti < 0)
                            ti = i;
                    }
                    else if (b == '&') {
                        break;
                    }

                    i++;
                }

                // extract the name / value pair

                String name, value;

                if (ti >= 0) {
                    name  = HttpUtility.UrlDecode(bytes, si, ti-si, encoding);
                    value = HttpUtility.UrlDecode(bytes, ti+1, i-ti-1, encoding);
                }
                else {
                    name = null;
                    value = HttpUtility.UrlDecode(bytes, si, i-si, encoding);
                }

                // add name / value pair to the collection

                base.Add(name, value);

                // trailing '&'

                if (i == l-1 && bytes[i] == '&')
                    base.Add(null, String.Empty);

                i++;
            }
        }

        internal void Add(HttpCookieCollection c) {
            int n = c.Count;

            for (int i = 0; i < n; i++) {
                ThrowIfMaxHttpCollectionKeysExceeded();
                HttpCookie cookie = c.Get(i);
                base.Add(cookie.Name, cookie.Value);
            }
        }

        // MSRC 12038: limit the maximum number of items that can be added to the collection,
        // as a large number of items potentially can result in too many hash collisions that may cause DoS
        internal void ThrowIfMaxHttpCollectionKeysExceeded() {
            if (base.Count >= AppSettings.MaxHttpCollectionKeys) {
                throw new InvalidOperationException(SR.GetString(SR.CollectionCountExceeded_HttpValueCollection, AppSettings.MaxHttpCollectionKeys));
            }
        }

        internal void Reset() {
            base.Clear();
        }

        public override String ToString() {
            return ToString(true);
        }

        internal virtual String ToString(bool urlencoded) {
            return ToString(urlencoded, null);
        }

        internal virtual String ToString(bool urlencoded, IDictionary excludeKeys) {
            int n = Count;
            if (n == 0)
                return String.Empty;

            StringBuilder s = new StringBuilder();
            String key, keyPrefix, item;
            bool ignoreViewStateKeys = (excludeKeys != null && excludeKeys[Page.ViewStateFieldPrefixID] != null);

            for (int i = 0; i < n; i++) {
                key = GetKey(i);

                // Review: improve this... Special case hack for __VIEWSTATE#
                if (ignoreViewStateKeys && key != null && key.StartsWith(Page.ViewStateFieldPrefixID, StringComparison.Ordinal)) continue;
                if (excludeKeys != null && key != null && excludeKeys[key] != null)
                    continue;
                if (urlencoded)
                    key = UrlEncodeForToString(key);
                keyPrefix = (key != null) ? (key + "=") : String.Empty;

                string[] values = GetValues(i);

                if (s.Length > 0)
                    s.Append('&');

                if (values == null || values.Length == 0) {
                    s.Append(keyPrefix);
                }
                else if (values.Length == 1) {
                    s.Append(keyPrefix);
                    item = values[0];
                    if (urlencoded)
                        item = UrlEncodeForToString(item);
                    s.Append(item);
                }
                else {
                    for (int j = 0; j < values.Length; j++) {
                        if (j > 0)
                            s.Append('&');
                        s.Append(keyPrefix);
                        item = values[j];
                        if (urlencoded)
                            item = UrlEncodeForToString(item);
                        s.Append(item);
                    }
                }
            }

            return s.ToString();
        }

        // HttpValueCollection used to call UrlEncodeUnicode in its ToString method, so we should continue to
        // do so for back-compat. The result of ToString is not used to make a security decision, so this
        // code path is "safe".
        internal static string UrlEncodeForToString(string input) {
            if (AppSettings.DontUsePercentUUrlEncoding) {
                // DevDiv #762975: <form action> and other similar URLs are mangled since we use non-standard %uXXXX encoding.
                // We need to use standard UTF8 encoding for modern browsers to understand the URLs.
                return HttpUtility.UrlEncode(input);
            }
            else {
#pragma warning disable 618 // [Obsolete]
                return HttpUtility.UrlEncodeUnicode(input);
#pragma warning restore 618
            }
        }

    }

}
