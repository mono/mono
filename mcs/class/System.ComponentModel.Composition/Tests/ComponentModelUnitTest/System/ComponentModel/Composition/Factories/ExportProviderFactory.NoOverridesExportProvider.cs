// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Factories
{
    partial class ExportProviderFactory
    {
        // NOTE: Do not add any more behavior to this class, as ExportProviderTests.cs 
        // uses this to verify default behavior of the base class.
        private class NoOverridesExportProvider : ExportProvider
        {
            public NoOverridesExportProvider()
            {
            }

            protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition context)
            {
                throw new NotImplementedException();
            }
        }
    }

}
