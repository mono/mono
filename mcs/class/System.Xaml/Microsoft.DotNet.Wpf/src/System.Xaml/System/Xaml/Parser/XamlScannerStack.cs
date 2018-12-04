// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xaml;

namespace MS.Internal.Xaml.Parser
{
    internal class XamlScannerFrame
    {
        public XamlType XamlType { get; set; }
        public XamlMember XamlProperty { get; set; }
        public bool XmlSpacePreserve { get; set; }
        public bool InContent { get; set; }
        public string TypeNamespace { get; set; }

        public XamlScannerFrame(XamlType xamlType, string ns)
        {
            XamlType = xamlType;
            TypeNamespace = ns;
        }
    }

    internal class XamlScannerStack
    {
        Stack<XamlScannerFrame> _stack;

        public XamlScannerStack()
        {
            _stack = new Stack<XamlScannerFrame>();
            _stack.Push( new XamlScannerFrame(null, null) );
        }

        public void Push(XamlType type, string ns)
        {
            // Copy the xmlSpacePreserve into each new frame.
            bool xmlSpacePreserve = CurrentXmlSpacePreserve;
            _stack.Push(new XamlScannerFrame(type, ns));
            CurrentXmlSpacePreserve = xmlSpacePreserve;
        }

        public void Pop()
        {
            _stack.Pop();
        }

        public int Depth
        {
            get { return _stack.Count - 1; }
        }

        public XamlType CurrentType
        {
            get { return (_stack.Count == 0) ? null : _stack.Peek().XamlType; }
        }

        public string CurrentTypeNamespace
        {
            get { return (_stack.Count == 0) ? null : _stack.Peek().TypeNamespace; }
        }

        public XamlMember CurrentProperty
        {
            get { return (_stack.Count == 0) ? null : _stack.Peek().XamlProperty; }
            set { _stack.Peek().XamlProperty = value; }
        }

        public bool CurrentXmlSpacePreserve
        {
            get { return (_stack.Count == 0) ? false : _stack.Peek().XmlSpacePreserve; }
            set
            {
                Debug.Assert(_stack.Count != 0);
                _stack.Peek().XmlSpacePreserve = value;
            }
        }

        public bool CurrentlyInContent
        {
            get { return (_stack.Count == 0) ? false : _stack.Peek().InContent; }
            set
            {
                Debug.Assert(_stack.Count != 0);
                _stack.Peek().InContent = value;
            }
        }

    }
}
