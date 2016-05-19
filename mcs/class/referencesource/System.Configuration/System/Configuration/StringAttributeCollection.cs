//------------------------------------------------------------------------------
// <copyright file="CommaDelimitedStringCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace System.Configuration {

    public sealed class CommaDelimitedStringCollection : StringCollection {
        private bool _Modified;
        private bool _ReadOnly;
        private string _OriginalString;

        //
        // Constructor
        //

        public CommaDelimitedStringCollection() {
            _ReadOnly = false;
            _Modified = false;
            _OriginalString = ToString();
        }

        internal void FromString(string list) {
            char[] _delimiters = { ',' };
            if (list != null) {
                string[] items = list.Split(_delimiters);
                foreach (string item in items) {
                    string trimmedItem = item.Trim();
                    if (trimmedItem.Length != 0) {
                        Add(item.Trim());
                    }
                }
            }
            _OriginalString = ToString();
            _ReadOnly = false;
            _Modified = false;
        }

        public override string ToString() {
            string returnString = null;

            if (Count > 0) {
                StringBuilder sb = new StringBuilder();
                foreach (string str in this) {
                    ThrowIfContainsDelimiter(str); // Since the add methods are not virtual they could still add bad data
                                                   // by casting the collection to a string collection.  This check will catch
                                                   // it before serialization, late is better than never.
                    sb.Append(str.Trim());
                    sb.Append(',');
                }
                returnString = sb.ToString();
                if (returnString.Length > 0) {
                    returnString = returnString.Substring(0, returnString.Length - 1);
                }
                if (returnString.Length == 0) {
                    returnString = null;
                }
            }
            return returnString;
        }

        private void ThrowIfReadOnly() {
            if (IsReadOnly == true) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_read_only));
            }
        }

        private void ThrowIfContainsDelimiter(string value) {
            if (value.Contains(",")) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_value_cannot_contain,","));
            }
        }

        public void SetReadOnly() {
            _ReadOnly = true;
        }

        public bool IsModified {
            get {
                return _Modified || (ToString() != _OriginalString);
            }
        }

        public new bool IsReadOnly {
            get {
                return _ReadOnly;
            }
        }

        //
        // Accessors
        //
        public new string this[int index] {
            get {
                return (string)base[index];
            }
            set {
                ThrowIfReadOnly();
                ThrowIfContainsDelimiter(value);
                _Modified = true;
                base[index] = value.Trim();
            }
        }

        //
        // Methods
        //

        public new void Add(string value) {
            ThrowIfReadOnly();
            ThrowIfContainsDelimiter(value);
            _Modified = true;
            base.Add(value.Trim());
        }

        public new void AddRange(string[] range) {
            ThrowIfReadOnly();
            _Modified = true;
            foreach (string str in range) {
                ThrowIfContainsDelimiter(str);
                base.Add(str.Trim());
            }
        }

        public new void Clear() {
            ThrowIfReadOnly();
            _Modified = true;
            base.Clear();
        }
        public new void Insert(int index, string value) {
            ThrowIfReadOnly();
            ThrowIfContainsDelimiter(value);
            _Modified = true;
            base.Insert(index, value.Trim());
        }

        public new void Remove(string value) {
            ThrowIfReadOnly();
            ThrowIfContainsDelimiter(value);
            _Modified = true;
            base.Remove(value.Trim());
        }

        // Clone
        //
        // Clone the object, to get back and object that we can modify
        // without changing the original object
        //
        public CommaDelimitedStringCollection Clone() {
            CommaDelimitedStringCollection copy;

            copy = new CommaDelimitedStringCollection();

            // Copy all values
            foreach (string str in this) {
                copy.Add(str);
            }

            // Copy Attributes
            copy._Modified = false;
            copy._ReadOnly = _ReadOnly;
            copy._OriginalString = _OriginalString;

            return copy;
        }
    }
}
