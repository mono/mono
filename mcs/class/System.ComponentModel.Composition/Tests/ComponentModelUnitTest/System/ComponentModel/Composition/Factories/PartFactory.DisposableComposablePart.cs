// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Factories
{
    partial class PartFactory
    {
        // NOTE: Do not add any more behavior to this class, as ComposablePartTests.cs 
        // uses this to verify default behavior of the base class.
        private class DisposableComposablePart : ComposablePart, IDisposable
        {
            private readonly Action<bool> _disposeCallback;

            public DisposableComposablePart(Action<bool> disposeCallback)
            {
                Assert.IsNotNull(disposeCallback);

                _disposeCallback = disposeCallback;
            }

            public void Dispose()
            {
                this.Dispose(true);
            }

            ~DisposableComposablePart()
            {
                Dispose(false);
            }

            protected virtual void Dispose(bool disposing)
            {
                _disposeCallback(disposing);
            }

            public override IEnumerable<ImportDefinition> ImportDefinitions
            {
                get { return Enumerable.Empty<ImportDefinition>(); }
            }

            public override IEnumerable<ExportDefinition> ExportDefinitions
            {
                get { return Enumerable.Empty<ExportDefinition>(); }
            }

            public override object GetExportedValue(ExportDefinition definition)
            {
                Assert.Fail();
                return null;
            }

            public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
            {
                Assert.Fail();
            }

            public override IDictionary<string, object> Metadata
            {
                get { return new Dictionary<string, object>(); }
            }
        }
    }
}