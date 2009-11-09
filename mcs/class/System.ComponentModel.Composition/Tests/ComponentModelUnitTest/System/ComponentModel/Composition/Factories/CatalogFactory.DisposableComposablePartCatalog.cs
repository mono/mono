// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition.Factories
{
    partial class CatalogFactory
    {
        // NOTE: Do not add any more behavior to this class, as ComposablePartCatalogTests.cs 
        // uses this to verify default behavior of the base class.
        private class DisposableComposablePartCatalog : ComposablePartCatalog
        {
            private readonly Action<bool> _disposeCallback;

            public DisposableComposablePartCatalog(Action<bool> disposeCallback)
            {
                Assert.IsNotNull(disposeCallback);

                _disposeCallback = disposeCallback;
            }

            ~DisposableComposablePartCatalog()
            {
                Dispose(false);
            }

            public override IQueryable<ComposablePartDefinition> Parts
            {
                get { throw new NotImplementedException(); }
            }

            protected override void Dispose(bool disposing)
            {
                _disposeCallback(disposing);

                base.Dispose(disposing);
            }
        }
    }
}