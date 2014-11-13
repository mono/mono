//------------------------------------------------------------------------------
// <copyright file="HandlerMappingMemo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Config related classes for HttpApplication
 */

namespace System.Web.Configuration {

    using System;
    using System.Text;
    using System.Globalization;

    internal class HandlerMappingMemo {
        internal HandlerMappingMemo(HttpHandlerAction mapping, String verb, VirtualPath path) {
            _mapping = mapping;
            _verb = verb;
            _path = path;
        }

        private HttpHandlerAction _mapping;
        private String _verb;
        private VirtualPath _path;

        internal /*public*/ bool IsMatch(String verb, VirtualPath path) {
            return _verb.Equals(verb) && _path.Equals(path);
        }

        internal /*public*/ HttpHandlerAction Mapping {
            get {
                return _mapping;
            }
        }
    }
}
