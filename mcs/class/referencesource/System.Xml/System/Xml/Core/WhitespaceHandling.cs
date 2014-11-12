//------------------------------------------------------------------------------
// <copyright file="WhiteSpaceHandling.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml
{
    // Specifies how whitespace is handled in XmlTextReader.
#if SILVERLIGHT
    internal enum WhitespaceHandling
#else
    public enum WhitespaceHandling
#endif
    {
        // Return all Whitespace and SignificantWhitespace nodes. This is the default.
        All              = 0,

        // Return just SignificantWhitespace, i.e. whitespace nodes that are in scope of xml:space="preserve"
        Significant      = 1,

        // Do not return any Whitespace or SignificantWhitespace nodes.
        None             = 2
    }
}
