//------------------------------------------------------------------------------
// <copyright file="CodeBlockBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    /// <summary>
    /// Provides access to the CodeBlockType of a CodeBlockBuilder
    /// </summary>
    public interface ICodeBlockTypeAccessor {
        CodeBlockType BlockType { get; }
    }
}
