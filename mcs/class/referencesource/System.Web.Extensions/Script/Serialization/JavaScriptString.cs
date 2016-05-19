//------------------------------------------------------------------------------
// <copyright file="JavaScriptString.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Serialization {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    internal class JavaScriptString {
        private string _s;
        private int _index;

        internal JavaScriptString(string s) {
            _s = s;
        }

        internal Nullable<char> GetNextNonEmptyChar() {
            while (_s.Length > _index) {
                char c = _s[_index++];
                if (!Char.IsWhiteSpace(c)) {
                    return c;
                }
            }

            return null;
        }

        internal Nullable<char> MoveNext() {
            if (_s.Length > _index) {
                return _s[_index++];
            }

            return null;
        }

        internal string MoveNext(int count) {
            if (_s.Length >= _index + count) {
                string result = _s.Substring(_index, count);
                _index += count;

                return result;
            }

            return null;
        }

        internal void MovePrev() {
            if (_index > 0) {
                _index--;
            }
        }

        internal void MovePrev(int count) {
            while (_index > 0 && count > 0) {
                _index--;
                count--;
            }
        }

        public override string ToString() {
            if (_s.Length > _index) {
                return _s.Substring(_index);
            }

            return String.Empty;
        }

        internal string GetDebugString(string message) {
            return message + " (" + _index + "): " + _s;
        }

        internal int IndexOf(string substr) {
            if (_s.Length > _index) {
                return _s.IndexOf(substr, _index, StringComparison.CurrentCulture) - _index;
            }

            return -1;
        }

        internal string Substring(int length) {
            if (_s.Length > _index + length) {
                return _s.Substring(_index, length);
            }

            return ToString();
        }
    }
}
