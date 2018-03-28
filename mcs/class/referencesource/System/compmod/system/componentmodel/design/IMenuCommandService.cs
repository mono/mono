//------------------------------------------------------------------------------
// <copyright file="IMenuCommandService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System.Runtime.Remoting;
    using System.ComponentModel;

    using System.Diagnostics;
    
    using System;

    /// <devdoc>
    ///    <para>Provides an interface for a designer to add menu items to the Visual Studio
    ///       7.0 menu.</para>
    /// </devdoc>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IMenuCommandService {
    
        /// <devdoc>
        /// <para>Gets or sets an array of type <see cref='System.ComponentModel.Design.DesignerVerb'/> 
        /// that indicates the verbs that are currently available.</para>
        /// </devdoc>
        DesignerVerbCollection Verbs { get; }
    
        /// <devdoc>
        ///    <para>
        ///       Adds a menu command to the document.
        ///    </para>
        /// </devdoc>
        void AddCommand(MenuCommand command);
        
        /// <devdoc>
        ///    <para>
        ///       Adds a verb to the set of global verbs.
        ///    </para>
        /// </devdoc>
        void AddVerb(DesignerVerb verb);
    
        /// <devdoc>
        ///    <para>
        ///       Searches for the given command ID and returns
        ///       the <see cref='System.ComponentModel.Design.MenuCommand'/>
        ///       associated with it.
        ///    </para>
        /// </devdoc>
        MenuCommand FindCommand(CommandID commandID);
        
        /// <devdoc>
        ///    <para>Invokes a command on the local form or in the global environment.</para>
        /// </devdoc>
        bool GlobalInvoke(CommandID commandID);
    
        /// <devdoc>
        ///    <para>
        ///       Removes the specified <see cref='System.ComponentModel.Design.MenuCommand'/> from the document.
        ///    </para>
        /// </devdoc>
        void RemoveCommand(MenuCommand command);
    
        /// <devdoc>
        ///    <para>
        ///       Removes the specified verb from the document.
        ///    </para>
        /// </devdoc>
        void RemoveVerb(DesignerVerb verb);
    
        /// <devdoc>
        ///    <para>Shows the context menu with the specified command ID at the specified
        ///       location.</para>
        /// </devdoc>
        void ShowContextMenu(CommandID menuID, int x, int y);
    }
}

