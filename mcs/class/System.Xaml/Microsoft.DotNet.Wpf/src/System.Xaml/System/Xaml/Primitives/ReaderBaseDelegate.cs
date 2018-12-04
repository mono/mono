// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace System.Xaml
{
    // This is the base class for the simplest implementation of a
    // Node based XamlReader.
    // It serves up the values of the current node.
    // Advancing to the next node with Read() is left to be defined
    // in the deriving class.
    //
    abstract internal class ReaderBaseDelegate: XamlReader, IXamlLineInfo
    {
        protected XamlSchemaContext _schemaContext;
        protected XamlNode _currentNode;
        protected LineInfo _currentLineInfo;
        protected bool _hasLineInfo;

        protected ReaderBaseDelegate(XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            _schemaContext = schemaContext;            
        }

        public override XamlNodeType NodeType
        {
            get { return _currentNode.NodeType; }
        }

        public override bool IsEof
        {
            get { return _currentNode.IsEof; }
        }

        public override NamespaceDeclaration  Namespace
        {
            get { return _currentNode.NamespaceDeclaration; }
        }

        public override XamlType Type
        {
            get { return _currentNode.XamlType; }
        }

        public override object Value
        {
            get { return _currentNode.Value; }
        }

        public override XamlMember Member
        {
            get { return _currentNode.Member; }
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return _schemaContext; }
        }

        #region IXamlLineInfo Members

        public bool HasLineInfo
        {
            get
            {
                return _hasLineInfo;
            }
            set
            {
                _hasLineInfo = value;
            }
        }

        public int LineNumber
        {
            get 
            {
                if (_currentLineInfo != null)
                {
                    return _currentLineInfo.LineNumber;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int LinePosition
        {
            get
            {
                if (_currentLineInfo != null)
                {
                    return _currentLineInfo.LinePosition;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion
    }
}
