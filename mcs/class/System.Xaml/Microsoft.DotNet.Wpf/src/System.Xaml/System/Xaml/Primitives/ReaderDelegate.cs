// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace System.Xaml
{
    // This is the simplest implementation of a Node based XamlReader.
    // This version advances through an externally provided list
    // of nodes with a "Next" delegate.
    // So is suitable for Queues and other simple single reader situations.
    //
    class ReaderDelegate : ReaderBaseDelegate
    {
        // InfosetNode _currentNode is inherited.
        XamlNodeNextDelegate _nextDelegate;

        public ReaderDelegate(XamlSchemaContext schemaContext, XamlNodeNextDelegate next, bool hasLineInfo)
            : base(schemaContext)
        {
            _nextDelegate = next;
            _currentNode = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
            _currentLineInfo = null;
            _hasLineInfo = hasLineInfo;
        }

        public override bool Read()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("XamlReader"); // Can't say ReaderDelegate because its internal.
            }
            do
            {
                _currentNode = _nextDelegate();

                if (_currentNode.NodeType != XamlNodeType.None)
                {
                    return true;   // This is the common/fast path
                }
                // else do the NONE node stuff
                if (_currentNode.IsLineInfo)
                {
                    _currentLineInfo = _currentNode.LineInfo;
                }
                else if (_currentNode.IsEof)
                {
                    break;
                }
            } while (_currentNode.NodeType == XamlNodeType.None);

            return !IsEof;
        }
    }
}
