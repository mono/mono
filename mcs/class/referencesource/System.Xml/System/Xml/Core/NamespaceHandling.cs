//------------------------------------------------------------------------------
// <copyright file="NewLineHandling.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">helenak</owner>
//------------------------------------------------------------------------------

namespace System.Xml {
    
    //
    // NamespaceHandling speficies how should the XmlWriter handle namespaces.
    //  

    [Flags]
    public enum NamespaceHandling {
        
        //
        // Default behavior
        //
        Default             = 0x0,

        //
        // Duplicate namespace declarations will be removed
        //
        OmitDuplicates      = 0x1,
    }
}
