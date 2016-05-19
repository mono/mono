//------------------------------------------------------------------------------
// <copyright file="InitializationEventAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {

    using System;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Specifies which event is fired on initialization.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InitializationEventAttribute : Attribute {

        private string eventName = null;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.InitializationEventAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public InitializationEventAttribute(string eventName) {
            this.eventName = eventName;
        }

        
        /// <devdoc>
        ///    <para>
        ///       Gets the name of the initialization event.
        ///    </para>
        /// </devdoc>
        public string EventName {
            get {
                return this.eventName;
            }
        }
    }
}
