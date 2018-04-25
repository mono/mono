//------------------------------------------------------------------------------
// <copyright file="IHelpService.cs" company="Microsoft">
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
    ///    <para> 
    ///       Provides the Integrated Development Environment (IDE) help
    ///       system with contextual information for the current task.</para>
    /// </devdoc>
    public interface IHelpService {
        /// <devdoc>
        ///    <para>Adds a context attribute to the document.</para>
        /// </devdoc>
        void AddContextAttribute(string name, string value, HelpKeywordType keywordType);
        
        /// <devdoc>
        ///     Clears all existing context attributes from the document.
        /// </devdoc>
        void ClearContextAttributes();
        
        /// <devdoc>
        ///     Creates a Local IHelpService to manage subcontexts.
        /// </devdoc>
        IHelpService CreateLocalContext(HelpContextType contextType);

        /// <devdoc>
        ///    <para>
        ///       Removes a previously added context attribute.
        ///    </para>
        /// </devdoc>
        void RemoveContextAttribute(string name, string value);
        
        /// <devdoc>
        ///     Removes a context that was created with CreateLocalContext
        /// </devdoc>
        void RemoveLocalContext(IHelpService localContext);

        /// <devdoc>
        ///    <para>Shows the help topic that corresponds to the specified keyword.</para>
        /// </devdoc>
        void ShowHelpFromKeyword(string helpKeyword);

        /// <devdoc>
        ///    <para>
        ///       Shows the help topic that corresponds with the specified Url and topic navigation ID.
        ///    </para>
        /// </devdoc>
        void ShowHelpFromUrl(string helpUrl);
    }
}
