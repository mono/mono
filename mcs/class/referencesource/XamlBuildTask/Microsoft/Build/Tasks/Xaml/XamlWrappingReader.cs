//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xaml;
    using XamlBuildTask;

    internal class XamlWrappingReader : XamlReader, IXamlLineInfo
    {
        XamlReader _underlyingReader;

        internal XamlWrappingReader(XamlReader underlyingReader)
        {
            if (underlyingReader == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("underlyingReader"));
            }
            _underlyingReader = underlyingReader;
        }

        public override bool IsEof
        {
            get { return _underlyingReader.IsEof; }
        }

        public override XamlMember Member
        {
            get { return _underlyingReader.Member; }
        }

        public override NamespaceDeclaration Namespace
        {
            get { return _underlyingReader.Namespace; }
        }

        public override XamlNodeType NodeType
        {
            get { return _underlyingReader.NodeType; }
        }

        public override bool Read()
        {
            return _underlyingReader.Read();
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return _underlyingReader.SchemaContext; }
        }

        public override XamlType Type
        {
            get { return _underlyingReader.Type; }
        }

        public override object Value
        {
            get { return _underlyingReader.Value; }
        }

        private IXamlLineInfo LineInfo
        {
            get { return _underlyingReader as IXamlLineInfo; }
        }

        #region IXamlLineInfo Members

        public bool HasLineInfo
        {
            get { return LineInfo != null && LineInfo.HasLineInfo; }
        }

        public int LineNumber
        {
            get { return LineInfo == null ? 0 : LineInfo.LineNumber; }
        }

        public int LinePosition
        {
            get { return LineInfo == null ? 0 : LineInfo.LinePosition; }
        }

        #endregion
    }
}
