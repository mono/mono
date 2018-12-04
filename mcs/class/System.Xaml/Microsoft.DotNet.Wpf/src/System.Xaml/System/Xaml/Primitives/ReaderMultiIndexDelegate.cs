// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Xaml
{
    public interface IXamlIndexingReader
    {
        int Count { get; }
        int CurrentIndex { get; set; }
    }

    // This is a simple implementation of a Node based XamlReader.
    // This version maintains its own index into an externally provided list
    // of nodes And access it with an "Index" delegate.
    // So is suitable for multiple readers of fixed Lists of Nodes.
    //
    internal class ReaderMultiIndexDelegate : ReaderBaseDelegate, IXamlIndexingReader
    {
        static XamlNode s_StartOfStream = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
        static XamlNode s_EndOfStream = new XamlNode(XamlNode.InternalNodeType.EndOfStream);

        XamlNodeIndexDelegate _indexDelegate;
        int _count;
        int _idx;

        public ReaderMultiIndexDelegate(XamlSchemaContext schemaContext, XamlNodeIndexDelegate indexDelegate, int count, bool hasLineInfo)
            : base(schemaContext)
        {
            _indexDelegate = indexDelegate;
            _count = count;
            // Do not set CurrentIndex in the constructor, because it invokes the overridable method Read().
            _idx = -1;
            _currentNode = s_StartOfStream;
            _currentLineInfo = null;
            _hasLineInfo = hasLineInfo;
        }

        public override bool Read()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("XamlReader"); // Can't say ReaderMultiIndexDelegate because its internal.
            }
            do
            {
                if (_idx < _count - 1)
                {
                    _currentNode = _indexDelegate(++_idx);
                    if (_currentNode.NodeType != XamlNodeType.None)
                    {
                        return true;   // This is the common/fast path
                    }
                    // else do the NONE node stuff.
                    if (_currentNode.LineInfo != null)
                    {
                        _currentLineInfo = _currentNode.LineInfo;
                    }
                    else if (_currentNode.IsEof)
                    {
                        break;
                    }
                }
                else
                {
                    _idx = _count;
                    _currentNode = s_EndOfStream;
                    _currentLineInfo = null;
                    break;
                }
            } while (_currentNode.NodeType == XamlNodeType.None);
            return !IsEof;
        }

        public int Count { get { return _count; } }

        public int CurrentIndex
        {
            get { return _idx; }
            set
            {
                if (value < -1 || value > _count)
                {
                    throw new IndexOutOfRangeException();
                }
                else if (value == -1)
                {
                    _idx = -1;
                    _currentNode = s_StartOfStream;
                    _currentLineInfo = null;
                }
                else
                {
                    // Read() advances the value
                    _idx = value - 1;
                    Read();
                }
            }
        }
    }
}
