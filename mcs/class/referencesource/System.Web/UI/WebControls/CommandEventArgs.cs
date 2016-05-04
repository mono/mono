//------------------------------------------------------------------------------
// <copyright file="CommandEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;

    /// <devdoc>
    /// <para>Provides data for the <see langword='Command'/> event.</para>
    /// </devdoc>
    public class CommandEventArgs : EventArgs {

        private string commandName;
        private object argument;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.CommandEventArgs'/> class with another <see cref='System.Web.UI.WebControls.CommandEventArgs'/>.</para>
        /// </devdoc>
        public CommandEventArgs(CommandEventArgs e) : this(e.CommandName, e.CommandArgument) {
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.CommandEventArgs'/> class with the specified command name 
        ///    and argument.</para>
        /// </devdoc>
        public CommandEventArgs(string commandName, object argument) {
            this.commandName = commandName;
            this.argument = argument;
        }



        /// <devdoc>
        ///    <para>Gets the name of the command. This property is read-only.</para>
        /// </devdoc>
        public string CommandName {
            get {
                return commandName;
            }
        }


        /// <devdoc>
        ///    <para>Gets the argument for the command. This property is read-only.</para>
        /// </devdoc>
        public object CommandArgument {
            get {
                return argument;
            }
        }
    }
}

