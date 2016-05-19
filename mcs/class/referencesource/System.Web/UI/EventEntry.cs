//------------------------------------------------------------------------------
// <copyright file="EventEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.Security.Permissions;

    /// <devdoc>
    /// PropertyEntry for event handler
    /// </devdoc>
    public class EventEntry {
        private Type _handlerType;
        private string _handlerMethodName;
        private string _name;


        /// <devdoc>
        /// </devdoc>
        public string HandlerMethodName {
            get {
                return _handlerMethodName;
            }
            set {
                _handlerMethodName = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public Type HandlerType {
            get {
                return _handlerType;
            }
            set {
                _handlerType = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
       }
    }

}


