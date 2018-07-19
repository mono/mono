//------------------------------------------------------------------------------
// <copyright file="IndexedString.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;

    [Serializable]
    public sealed class IndexedString {

        private string _value;


        public IndexedString (string s) {
            if (String.IsNullOrEmpty(s)) {
                throw new ArgumentNullException("s");
            }
            _value = s;
        }


        public string Value {
            get {
                return _value;
            }
        }
    }
}
