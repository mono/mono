//------------------------------------------------------------------------------
// <copyright file="CodeBlockBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Handle <%= ... %>, <% ... %>, <%# ... %>, <%: ... %>, <%#: ... %> blocks
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {

using System;
using System.IO;

internal class CodeBlockBuilder : ControlBuilder, ICodeBlockTypeAccessor {
    protected CodeBlockType _blockType;
    protected string _content;
    private int _column;

    internal CodeBlockBuilder(CodeBlockType blockType, string content, int lineNumber, int column, VirtualPath virtualPath, bool encode) {
        _content = content;
        _blockType = blockType;
        _column = column;
        IsEncoded = encode;

        Line = lineNumber;
        VirtualPath = virtualPath;
    }

    internal CodeBlockBuilder(CodeBlockType blockType, string content, int lineNumber, int column, VirtualPath virtualPath)
        : this(blockType, content, lineNumber, column, virtualPath, false) {
    }

    public override object BuildObject() {
        return null;
    }

    internal /*public*/ string Content {
        get {
            return _content;
        }
    }

    public CodeBlockType BlockType {
        get { return _blockType;}
    }

    internal int Column { get { return _column; } }
    
    // This is used by only DataBinding CodeBlockType.
    internal bool IsEncoded { 
        get;
        private set;
    }
}

public enum CodeBlockType {
    Code,               // <% ... %>
    Expression,         // <%= ... %>
    DataBinding,        // <%# ... %>
    EncodedExpression   // <%: ... %>
}

}
