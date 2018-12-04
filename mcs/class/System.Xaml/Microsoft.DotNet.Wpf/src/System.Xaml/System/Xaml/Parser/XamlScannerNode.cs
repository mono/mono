// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Xaml;

namespace MS.Internal.Xaml.Parser
{
    [DebuggerDisplay("{NodeType}")]
    internal class XamlScannerNode
    {
        public XamlScannerNode(IXmlLineInfo lineInfo)
        {
            if (lineInfo != null)
            {
                LineNumber = lineInfo.LineNumber;
                LinePosition = lineInfo.LinePosition;
            }
        }

        public XamlScannerNode(XamlAttribute attr)
        {
            LineNumber = attr.LineNumber;
            LinePosition = attr.LinePosition;
        }

        public ScannerNodeType NodeType { get; set; }
        public XamlType Type { get; set; }
        public string TypeNamespace { get; set; }
        public XamlMember PropertyAttribute  { get; set; }
        public XamlText PropertyAttributeText { get; set; }
        public bool IsCtorForcingMember { get; set; }
        public XamlMember PropertyElement { get; set; }
        public bool IsEmptyTag { get; set; }
        public XamlText TextContent { get; set; }
        public bool IsXDataText { get; set; }
        public string Prefix { get; set; }
        public int LineNumber { get; private set; }
        public int LinePosition  { get; private set; }
    }
}
