//------------------------------------------------------------------------------
// <copyright file="AttributeCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * AttributeCollection.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI {
    using System.IO;
    using System.Collections;
    using System.Reflection;
    using System.Web.UI;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.Util;
    
/*
 * The AttributeCollection represents Attributes on an Html control.
 */

/// <devdoc>
///    <para>
///       The <see langword='AttributeCollection'/> class provides object-model access
///       to all attributes declared on an HTML server control element.
///    </para>
/// </devdoc>
    public sealed class AttributeCollection {
        private StateBag _bag;
        private CssStyleCollection _styleColl;

        /*
         *      Constructs an AttributeCollection given a StateBag.
         */

        /// <devdoc>
        /// </devdoc>
        public AttributeCollection(StateBag bag) {
            _bag = bag;
        }

        /*
         * Automatically adds new keys.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a specified attribute value.
        ///    </para>
        /// </devdoc>
        public string this[string key]
        {
            get {
                if (_styleColl != null && StringUtil.EqualsIgnoreCase(key, "style"))
                    return _styleColl.Value;
                else
                    return _bag[key] as string;
            }
            set { 
                Add(key, value); 
            }
        }

        /*
         * Returns a collection of keys.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of keys to all the attributes in the
        ///    <see langword='AttributeCollection'/>.
        ///    </para>
        /// </devdoc>
        public ICollection Keys {
            get { 
                return _bag.Keys;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets the number of items in the <see langword='AttributeCollection'/>.
        ///    </para>
        /// </devdoc>
        public int Count {
            get { 
                return _bag.Count; 
            }
        }


        /// <devdoc>
        /// </devdoc>
        public CssStyleCollection CssStyle {
            get {
                if (_styleColl == null) {
                    _styleColl = new CssStyleCollection(_bag);
                }
                return _styleColl;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Adds an item to the <see langword='AttributeCollection'/>.
        ///    </para>
        /// </devdoc>
        public void Add(string key, string value) {
            if (_styleColl != null && StringUtil.EqualsIgnoreCase(key, "style"))
                _styleColl.Value = value;
            else
                _bag[key] = value;
        }

        public override bool Equals(object o) {
            // This implementation of Equals relies on mutable properties and is therefore broken,
            // but we shipped it this way in V1 so it will be a breaking change to fix it.
            AttributeCollection attrs = o as AttributeCollection;

            if (attrs != null) {
                if (attrs.Count != _bag.Count) {
                    return false;
                }
                foreach (DictionaryEntry attr in _bag) {
                    if (this[(string)attr.Key] != attrs[(string)attr.Key]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode() {
            // This implementation of GetHashCode uses mutable properties but matches the V1 implementation
            // of Equals.
            HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();
            foreach (DictionaryEntry attr in _bag) {
                hashCodeCombiner.AddObject(attr.Key);
                hashCodeCombiner.AddObject(attr.Value);
            }
            return hashCodeCombiner.CombinedHash32;
        }
        

        /// <devdoc>
        ///    <para>
        ///       Removes an attribute from the <see langword='AttributeCollection'/>.
        ///    </para>
        /// </devdoc>
        public void Remove(string key) {
            if (_styleColl != null && StringUtil.EqualsIgnoreCase(key, "style"))
                _styleColl.Clear();
            else
                _bag.Remove(key);
        }


        /// <devdoc>
        ///    <para>
        ///       Removes all attributes from the <see langword='AttributeCollection'/>.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            _bag.Clear();
            if (_styleColl != null)
                _styleColl.Clear();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Render(HtmlTextWriter writer) {
            if (_bag.Count > 0) {
                IDictionaryEnumerator e = _bag.GetEnumerator();

                while (e.MoveNext()) {
                    StateItem item = e.Value as StateItem;
                    if (item != null) {
                        string value = item.Value as string;
                        string key = e.Key as string;
                        if (key != null && value != null) {
                            writer.WriteAttribute(key, value, true /*fEncode*/);
                        }
                    }
                }
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void AddAttributes(HtmlTextWriter writer) {
            if (_bag.Count > 0) {
                IDictionaryEnumerator e = _bag.GetEnumerator();

                while (e.MoveNext()) {
                    StateItem item = e.Value as StateItem;
                    if (item != null) {
                        string value = item.Value as string;
                        string key = e.Key as string;
                        if (key != null && value != null) {
                            writer.AddAttribute(key, value, true /*fEncode*/);
                        }
                    }
                }
            }
        }
    }
}
