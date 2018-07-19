//------------------------------------------------------------------------------
// <copyright file="XmlEventChangedAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">ionv</owner>
//------------------------------------------------------------------------------

namespace System.Xml
{
    // Specifies the type of node change
    public enum XmlNodeChangedAction
    {
        // A node is beeing inserted in the tree.
        Insert = 0,

        // A node is beeing removed from the tree.
        Remove = 1,

        // A node value is beeing changed.
        Change = 2
    }
}
