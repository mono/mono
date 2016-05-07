//------------------------------------------------------------------------------
// <copyright file="FormViewCommandEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <devdoc>
    /// <para>Provides data for some <see cref='System.Web.UI.WebControls.FormView'/> events.</para>
    /// </devdoc>
    public class FormViewCommandEventArgs : CommandEventArgs {

        private object _commandSource;
        

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FormViewCommandEventArgs'/>
        /// class.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public FormViewCommandEventArgs(object commandSource, CommandEventArgs originalArgs) : base(originalArgs) {
            this._commandSource = commandSource;
        }


        /// <devdoc>
        ///    <para>Gets the source of the command. This property is read-only.</para>
        /// </devdoc>
        public object CommandSource {
            get {
                return _commandSource;
            }
        }

        /// <summary>
        /// Set by the user to skip databound or datasource handling of the event.
        /// </summary>
        public bool Handled { get; set; }

    }
}

