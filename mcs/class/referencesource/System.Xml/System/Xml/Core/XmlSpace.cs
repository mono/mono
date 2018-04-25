//------------------------------------------------------------------------------
// <copyright file="XmlSpace.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">helenak</owner>
//------------------------------------------------------------------------------

namespace System.Xml
{
    // An enumeration for the xml:space scope used in XmlReader and XmlWriter.
    public enum XmlSpace
    {
        // xml:space scope has not been specified.
        None          = 0,

        // The xml:space scope is "default".
        Default       = 1,

        // The xml:space scope is "preserve".
        Preserve      = 2
    }
}
