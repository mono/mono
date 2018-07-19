//------------------------------------------------------------------------------
// <copyright file="IImplicitResourceProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.IO;
    using System.CodeDom;
    using System.Globalization;
    using System.Resources;
    using System.Web.Compilation;
    using System.Web.Util;
    using System.Web.UI;
    using System.Security.Permissions;

    /*
     * Interface to access implicit (automatic) page resources
     */
    public interface IImplicitResourceProvider {

        /*
         * Retrieve a resource object for the passed in key and culture
         */
        object GetObject(ImplicitResourceKey key, CultureInfo culture);

        /*
         * Return a collection of ImplicitResourceKey's for the given prefix
         */
        ICollection GetImplicitResourceKeys(string keyPrefix);
    }

    /*
     * Contains fields which identify a specific implicit resource key
     */
    public sealed class ImplicitResourceKey {
        private string _filter;
        private string _keyPrefix;
        private string _property;

        public ImplicitResourceKey() {
        }

        public ImplicitResourceKey(string filter, string keyPrefix, string property) {
            _filter = filter;
            _keyPrefix = keyPrefix;
            _property = property;
        }

        /*
         * The filter, if any
         */
        public string Filter {
            get {
                return _filter;
            }
            set {
                _filter = value;
            }
        }

        /*
         * The prefix, as appears on tag's meta:resourcekey attributes
         */
        public string KeyPrefix {
            get {
                return _keyPrefix;
            }
            set {
                _keyPrefix = value;
            }
        }

        /*
         * The property, possibly including sub-properties (e.g. MyProp.MySubProp)
         */
        public string Property {
            get {
                return _property;
            }
            set {
                _property = value;
            }
        }
    }

    /*
     * IImplicitResourceProvider implementation on top of an arbitrary IResourceProvider
     */
    internal class DefaultImplicitResourceProvider: IImplicitResourceProvider {

        private IResourceProvider _resourceProvider;
        private IDictionary _implicitResources;
        private bool _attemptedGetPageResources;

        internal DefaultImplicitResourceProvider(IResourceProvider resourceProvider) {
            _resourceProvider = resourceProvider;
        }

        public virtual object GetObject(ImplicitResourceKey entry, CultureInfo culture) {

            // Put together the full resource key based on the ImplicitResourceKey
            string fullResourceKey = ConstructFullKey(entry);

            // Look it up in the resource provider
            return _resourceProvider.GetObject(fullResourceKey, culture);
        }

        public virtual ICollection GetImplicitResourceKeys(string keyPrefix) {

            // Try to get the page resources
            EnsureGetPageResources();

            // If there aren't any, return null
            if (_implicitResources == null)
                return null;

            // Return the collection of resources for this key prefix
            return (ICollection) _implicitResources[keyPrefix];
        }

        /*
         * Create a dictionary, in which the key is a resource key (as found on meta:resourcekey
         * attributes), and the value is an ArrayList containing all the resources for that
         * resource key.  Each element of the ArrayList is an ImplicitResourceKey
         */
        internal void EnsureGetPageResources() {

            // If we already attempted to get them, don't do it again
            if (_attemptedGetPageResources)
                return;

            _attemptedGetPageResources = true;

            IResourceReader resourceReader = _resourceProvider.ResourceReader;
            if (resourceReader == null)
                return;

            _implicitResources = new Hashtable(StringComparer.OrdinalIgnoreCase);

            // Enumerate through all the page resources
            foreach (DictionaryEntry entry in resourceReader) {

                // Attempt to parse the key into a ImplicitResourceKey
                ImplicitResourceKey implicitResKey = ParseFullKey((string) entry.Key);

                // If we couldn't parse it as such, skip it
                if (implicitResKey == null)
                    continue;

                // Check if we already have an entry for this resource key prefix
                ArrayList controlResources = (ArrayList) _implicitResources[implicitResKey.KeyPrefix];

                // If not, create one
                if (controlResources == null) {
                    controlResources = new ArrayList();
                    _implicitResources[implicitResKey.KeyPrefix] = controlResources;
                }

                // Add an entry in the ArrayList for this property
                controlResources.Add(implicitResKey);
            }
        }

        /*
         * Parse a complete page resource key into a ImplicitResourceKey.
         * Return null if the key does not appear to be meant for implict resources.
         */
        private static ImplicitResourceKey ParseFullKey(string key) {

            string filter = String.Empty;

            // A page resource key looks like [myfilter:]MyResKey.MyProp[.MySubProp]

            // Check if there is a filter
            if (key.IndexOf(':') > 0) {
                string[] parts = key.Split(':');

                // Shouldn't be multiple ':'.  If there is, ignore it
                if (parts.Length > 2)
                    return null;

                filter = parts[0];
                key = parts[1];
            }

            int periodIndex = key.IndexOf('.');

            // There should be at least one period, for the meta:resourcekey part. If not, ignore.
            if (periodIndex <= 0)
                return null;

            string keyPrefix = key.Substring(0, periodIndex);

            // The rest of the string is the property (e.g. MyProp.MySubProp)
            string property = key.Substring(periodIndex+1);

            // Create a ImplicitResourceKey with the parsed data
            ImplicitResourceKey implicitResKey = new ImplicitResourceKey();
            implicitResKey.Filter = filter;
            implicitResKey.KeyPrefix = keyPrefix;
            implicitResKey.Property = property;

            return implicitResKey;
        }

        private static string ConstructFullKey(ImplicitResourceKey entry) {
            string key = entry.KeyPrefix + "." + entry.Property;

            if (entry.Filter.Length > 0)
                key = entry.Filter + ":" + key;

            return key;
        }
    }
}

