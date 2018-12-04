// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using MS.Internal.Xaml.Context;
using System.Windows.Markup;

namespace System.Xaml
{
    internal enum SavedContextType { Template, ReparseValue, ReparseMarkupExtension }

    internal class XamlSavedContext
    {
        private XamlSchemaContext _context;
        private XamlContextStack<ObjectWriterFrame> _stack;
        private SavedContextType _savedContextType;

        public XamlSavedContext(SavedContextType savedContextType, ObjectWriterContext owContext, XamlContextStack<ObjectWriterFrame> stack)
        {
            //We should harvest all information necessary from the xamlContext so that we can answer all ServiceProvider based questions.
            _savedContextType = savedContextType;
            _context = owContext.SchemaContext;
            _stack = stack;

            // Null out CurrentFrameValue in case of template to save on survived allocations
            if (savedContextType == SavedContextType.Template)
            {
                stack.CurrentFrame.Instance = null;
            }
            this.BaseUri = owContext.BaseUri;
        }

        public SavedContextType SaveContextType { get { return _savedContextType; } }
        public XamlContextStack<ObjectWriterFrame> Stack { get { return _stack; } }
        public XamlSchemaContext SchemaContext { get { return _context; } }
        public Uri BaseUri { get; private set; }
    }
}
