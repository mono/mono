// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Xaml
{
    // This is the simplest implementation of a Node based XamlWriter.
    // It turns XamlWriter calls into nodes and passes them up to the
    // provided _addDelegate.
    //
    class WriterDelegate : XamlWriter, IXamlLineInfoConsumer
    {
        XamlNodeAddDelegate _addDelegate;
        XamlLineInfoAddDelegate _addLineInfoDelegate;
        XamlSchemaContext _schemaContext;
        
        public WriterDelegate(XamlNodeAddDelegate add, XamlLineInfoAddDelegate addlineInfoDelegate, XamlSchemaContext xamlSchemaContext)
        {
            _addDelegate = add;
            _addLineInfoDelegate = addlineInfoDelegate;
            _schemaContext = xamlSchemaContext;
        }

        #region XamlWriter Members

        public override void WriteGetObject()
        {
            ThrowIsDisposed();
            _addDelegate(XamlNodeType.GetObject, null);
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            ThrowIsDisposed();
            _addDelegate(XamlNodeType.StartObject, xamlType);
        }

        public override void WriteEndObject()
        {
            ThrowIsDisposed();
            _addDelegate(XamlNodeType.EndObject, null);
        }

        public override void WriteStartMember(XamlMember member)
        {
            ThrowIsDisposed();
            _addDelegate(XamlNodeType.StartMember, member);
        }

        public override void WriteEndMember()
        {
            ThrowIsDisposed();
            _addDelegate(XamlNodeType.EndMember, null);
        }

        public override void WriteValue(object value)
        {
            ThrowIsDisposed();
            _addDelegate(XamlNodeType.Value, value);
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            ThrowIsDisposed();
            _addDelegate(XamlNodeType.NamespaceDeclaration, namespaceDeclaration);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !IsDisposed)
                {
                    _addDelegate(XamlNodeType.None, XamlNode.InternalNodeType.EndOfStream);
                    _addDelegate = new XamlNodeAddDelegate(ThrowBecauseWriterIsClosed);
                    _addLineInfoDelegate = (_addLineInfoDelegate != null)
                                ? new XamlLineInfoAddDelegate(ThrowBecauseWriterIsClosed2)
                                : null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return _schemaContext; }
        }
        #endregion

        #region IConsumeXamlLineInfo Members
        /// <summary>
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="linePosition"></param>
        public void SetLineInfo(int lineNumber, int linePosition)
        {
            ThrowIsDisposed();
            _addLineInfoDelegate(lineNumber, linePosition);
        }

        public bool ShouldProvideLineInfo
        {
            get
            {
                ThrowIsDisposed();
                return _addLineInfoDelegate != null;
            }
        }
        #endregion


        private void ThrowBecauseWriterIsClosed(XamlNodeType nodeType, object data)
        {
            throw new XamlException(SR.Get(SRID.WriterIsClosed));
        }

        private void ThrowBecauseWriterIsClosed2(int lineNumber, int linePosition)
        {
            throw new XamlException(SR.Get(SRID.WriterIsClosed));
        }

        private void ThrowIsDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("XamlWriter"); // Can't say ReaderMultiIndexDelegate because its internal.
            }
        }

    }
}
