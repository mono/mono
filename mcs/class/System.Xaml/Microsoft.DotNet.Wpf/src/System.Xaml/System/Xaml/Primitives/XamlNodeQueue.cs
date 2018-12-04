// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Xaml;

namespace System.Xaml
{
    // Provides a FIFO buffer for writing nodes and reading them back.
    // This is used in XamlReader wrappers.   Nodes are in a Queue and
    // thus are "consumed" from the queue when read.
    // If you want a replay-able list see XamlNodeList.

    public class XamlNodeQueue
    {
        Queue<XamlNode> _nodeQueue;
        XamlNode _endOfStreamNode;

        ReaderDelegate _reader;
        XamlWriter _writer;
        bool _hasLineInfo;

        public XamlNodeQueue(XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            _nodeQueue = new Queue<XamlNode>();
            _endOfStreamNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
            _writer = new WriterDelegate(Add, AddLineInfo, schemaContext);
        }
        
        public XamlReader Reader
        {
            get
            {
                if (_reader == null)
                {
                    _reader = new ReaderDelegate(_writer.SchemaContext, Next, _hasLineInfo);
                }
                return _reader;
            }
        }

        public XamlWriter Writer
        {
            get { return _writer; }
        }

        public bool IsEmpty
        {
            get { return _nodeQueue.Count == 0; }
        }

        public int Count
        {
            get { return _nodeQueue.Count; }
        }

        // ======================================

        private void Add(XamlNodeType nodeType, object data)
        {
            if (nodeType != XamlNodeType.None)
            {
                XamlNode node = new XamlNode(nodeType, data);
                _nodeQueue.Enqueue(node);
                return;
            }
            Debug.Assert(XamlNode.IsEof_Helper(nodeType, data));
            _nodeQueue.Enqueue(_endOfStreamNode);
        }

        private void AddLineInfo(int lineNumber, int linePosition)
        {
            LineInfo lineInfo = new LineInfo(lineNumber, linePosition);
            XamlNode node = new XamlNode(lineInfo);
            _nodeQueue.Enqueue(node);
            if (!_hasLineInfo)
            {
                _hasLineInfo = true;
            }
            if (_reader != null && !_reader.HasLineInfo)
            {
                _reader.HasLineInfo = true;
            }
        }
        
        private XamlNode Next()
        {
            XamlNode node;
            if (_nodeQueue.Count > 0)
            {
                node = _nodeQueue.Dequeue();
            }
            else
            {
                node = _endOfStreamNode;
            }
            return node;
        }
    }
}
