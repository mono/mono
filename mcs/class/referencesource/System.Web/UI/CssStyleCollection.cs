//------------------------------------------------------------------------------
// <copyright file="CssStyleCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web.UI;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>
    ///       The <see langword='CssStyleCollection'/>
    ///       class contains HTML
    ///       cascading-style sheets (CSS) inline style attributes. It automatically parses
    ///       and exposes CSS properties through a dictionary pattern API. Each CSS key can be
    ///       manipulated using a key/value indexed collection.
    ///    </para>
    /// </devdoc>
    public sealed class CssStyleCollection {

        private static readonly Regex _styleAttribRegex = new Regex(
                                                                   "\\G(\\s*(;\\s*)*" +        // match leading semicolons and spaces
                                                                   "(?<stylename>[^:]+?)" +    // match stylename - chars up to the semicolon
                                                                   "\\s*:\\s*" +               // spaces, then the colon, then more spaces
                                                                   "(?<styleval>[^;]*)" +      // now match styleval
                                                                   ")*\\s*(;\\s*)*$",          // match a trailing semicolon and trailing spaces
                                                                   RegexOptions.Singleline | 
                                                                   RegexOptions.Multiline |
                                                                   RegexOptions.ExplicitCapture);    
        private StateBag _state;
        private string _style;

        private IDictionary _table;
        private IDictionary _intTable;


        internal CssStyleCollection() : this(null) {
        }

        /*
         * Constructs an CssStyleCollection given a StateBag.
         */
        internal CssStyleCollection(StateBag state) {
            _state = state;
        }

        /*
         * Automatically adds new keys.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a specified CSS value.
        ///    </para>
        /// </devdoc>
        public string this[string key] {
            get {
                if (_table == null)
                    ParseString();
                string value = (string)_table[key];

                if (value == null) {
                    HtmlTextWriterStyle style = CssTextWriter.GetStyleKey(key);
                    if (style != (HtmlTextWriterStyle)(-1)) {
                        value = this[style];
                    }
                }

                return value;
            }
            set { 
                Add(key, value); 
            }
        }


        /// <devdoc>
        /// Gets or sets the specified known CSS value.
        /// </devdoc>
        public string this[HtmlTextWriterStyle key] {
            get {
                if (_intTable == null) {
                    return null;
                }
                return (string)_intTable[(int)key];
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
        ///       Gets a collection of keys to all the styles in the
        ///    <see langword='CssStyleCollection'/>. 
        ///    </para>
        /// </devdoc>
        public ICollection Keys {
            get { 
                if (_table == null)
                    ParseString();

                if (_intTable != null) {
                    // combine the keys into a single table. Note that to preserve existing
                    // behavior, we convert enum values into strings to maintain a homogeneous collection.

                    string[] keys = new string[_table.Count + _intTable.Count];
                    int i = 0;
                    
                    foreach (string s in _table.Keys) {
                        keys[i] = s;
                        i++;
                    }
                    foreach (HtmlTextWriterStyle style in _intTable.Keys) {
                        keys[i] = CssTextWriter.GetStyleName(style);
                        i++;
                    }

                    return keys;
                }
                
                return _table.Keys; 
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets the number of items in the <see langword='CssStyleCollection'/>.
        ///    </para>
        /// </devdoc>
        public int Count {
            get { 
                if (_table == null)
                    ParseString();
                return _table.Count + ((_intTable != null) ? _intTable.Count : 0); 
            }
        }


        public string Value {
            get { 
                if (_state == null) {
                    if (_style == null) {
                        _style = BuildString();
                    }
                    return _style;
                }
                return(string)_state["style"];
            }
            set { 
                if (_state == null) {
                    _style = value;
                }
                else {
                    _state["style"] = value;
                }
                _table = null;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Adds a style to the CssStyleCollection.
        ///    </para>
        /// </devdoc>
        public void Add(string key, string value) {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentNullException("key");
            }
            
            if (_table == null)
                ParseString();
            _table[key] = value;

            if (_intTable != null) {
                // Remove from the other table to avoid duplicates.
                HtmlTextWriterStyle style = CssTextWriter.GetStyleKey(key);
                if (style != (HtmlTextWriterStyle)(-1)) {
                    _intTable.Remove(style);
                }
            }

            if (_state != null) {
                // keep style attribute synchronized
                _state["style"] = BuildString();
            }
            _style = null;
        }


        public void Add(HtmlTextWriterStyle key, string value) {
            if (_intTable == null) {
                _intTable = new HybridDictionary();
            }
            _intTable[(int)key] = value;

            string name = CssTextWriter.GetStyleName(key);
            if (name.Length != 0) {
                // Remove from the other table to avoid duplicates.
                if (_table == null)
                    ParseString();

                _table.Remove(name);
            }

            if (_state != null) {
                // keep style attribute synchronized
                _state["style"] = BuildString();
            }
            _style = null;
        }


        /// <devdoc>
        ///    <para>
        ///       Removes a style from the <see langword='CssStyleCollection'/>.
        ///    </para>
        /// </devdoc>
        public void Remove(string key) {
            if (_table == null)
                ParseString();
            if (_table[key] != null) {
                _table.Remove(key);

                if (_state != null) {
                    // keep style attribute synchronized
                    _state["style"] = BuildString();
                }
                _style = null;
            }
        }


        public void Remove(HtmlTextWriterStyle key) {
            if (_intTable == null) {
                return;
            }
            _intTable.Remove((int)key);

            if (_state != null) {
                // keep style attribute synchronized
                _state["style"] = BuildString();
            }
            _style = null;
        }


        /// <devdoc>
        ///    <para>
        ///       Removes all styles from the <see langword='CssStyleCollection'/>.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            _table = null;
            _intTable = null;

            if (_state != null) {
                _state.Remove("style");
            }
            _style = null;
        }

        /*  BuildString
         *  Form the style string from data contained in the 
         *  hash table
         */
        private string BuildString() {
            // if the tables are null, there is nothing to build
            if (((_table == null) || (_table.Count == 0)) &&
                ((_intTable == null) || (_intTable.Count == 0))) {
                return null;
            }

            StringWriter sw = new StringWriter();
            CssTextWriter writer = new CssTextWriter(sw);

            Render(writer);
            return sw.ToString();
        }

        /*  ParseString
         *  Parse the style string and fill the hash table with
         *  corresponding values.  
         */
        private void ParseString() {
            // create a case-insensitive dictionary
            _table = new HybridDictionary(true);

            string s = (_state == null) ? _style : (string)_state["style"];
            if (s != null) {
                Match match;

                if ((match = _styleAttribRegex.Match( s, 0)).Success) {
                    CaptureCollection stylenames = match.Groups["stylename"].Captures;
                    CaptureCollection stylevalues = match.Groups["styleval"].Captures;

                    for (int i = 0; i < stylenames.Count; i++) {
                        String styleName = stylenames[i].ToString();
                        String styleValue = stylevalues[i].ToString();

                        _table[styleName] = styleValue;
                    }
                }
            }
        }


        /// <devdoc>
        /// Render out the attribute collection into a CSS TextWriter. This
        /// effectively renders the value of an inline style attribute.
        /// </devdoc>
        internal void Render(CssTextWriter writer) {
            if (_table != null && _table.Count > 0) {
                foreach (DictionaryEntry entry in _table) {
                    writer.WriteAttribute((string)entry.Key, (string)entry.Value);
                }
            }
            if (_intTable != null && _intTable.Count > 0) {
                foreach (DictionaryEntry entry in _intTable) {
                    writer.WriteAttribute((HtmlTextWriterStyle)entry.Key, (string)entry.Value);
                }
            }
        }

        /// <devdoc>
        /// Render out the attribute collection into a CSS TextWriter. This
        /// effectively renders the value of an inline style attribute.
        /// Used by a Style object to render out its CSS attributes into an HtmlTextWriter.
        /// </devdoc>
        internal void Render(HtmlTextWriter writer) {
            if (_table != null && _table.Count > 0) {
                foreach (DictionaryEntry entry in _table) {
                    writer.AddStyleAttribute((string)entry.Key, (string)entry.Value);
                }
            }
            if (_intTable != null && _intTable.Count > 0) {
                foreach (DictionaryEntry entry in _intTable) {
                    writer.AddStyleAttribute((HtmlTextWriterStyle)entry.Key, (string)entry.Value);
                }
            }
        }
    }
}
