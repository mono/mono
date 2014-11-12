//------------------------------------------------------------------------------
// <copyright file="ICodeParser.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {

    using System.Diagnostics;
    using System.IO;

    /// <devdoc>
    ///    <para>
    ///       Provides a code parsing interface.
    ///    </para>
    /// </devdoc>
    public interface ICodeParser {
    
        /// <devdoc>
        ///    <para>
        ///       Compiles the given text stream into a CodeCompile unit.  
        ///    </para>
        /// </devdoc>
        CodeCompileUnit Parse(TextReader codeStream);
    }
}
