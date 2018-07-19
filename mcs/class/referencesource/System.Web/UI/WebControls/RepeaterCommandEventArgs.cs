//------------------------------------------------------------------------------
// <copyright file="RepeaterCommandEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;

    /// <devdoc>
    ///    <para>Provides data for the
    ///    <see langword='ItemCommand'/> event of the <see cref='System.Web.UI.WebControls.Repeater'/> .</para>
    /// </devdoc>
    public class RepeaterCommandEventArgs : CommandEventArgs {

        private RepeaterItem item;
        private object commandSource;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.RepeaterCommandEventArgs'/> 
        /// class.</para>
        /// </devdoc>
        public RepeaterCommandEventArgs(RepeaterItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs) {
            this.item = item;
            this.commandSource = commandSource;
        }



        /// <devdoc>
        /// <para>Gets the <see cref='System.Web.UI.WebControls.RepeaterItem'/>associated with the event.</para>
        /// </devdoc>
        public RepeaterItem Item {
            get {
                return item;
            }
        }


        /// <devdoc>
        ///    Gets the command source.
        /// </devdoc>
        public object CommandSource {
            get {
                return commandSource;
            }
        }
    }
}

