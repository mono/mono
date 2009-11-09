// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.AttributedModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Hosting;

namespace System.ComponentModel.Composition.Factories
{
    internal partial class ExportProviderFactory
    {
        public static ExportProvider Create()
        {
            return new NoOverridesExportProvider();
        }

        public static RecomposableExportProvider CreateRecomposable()
        {
            return new RecomposableExportProvider();
        }
    }
}
