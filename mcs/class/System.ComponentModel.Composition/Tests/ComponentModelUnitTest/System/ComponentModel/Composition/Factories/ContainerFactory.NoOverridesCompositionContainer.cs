// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition.Factories
{
    partial class ContainerFactory
    {
        // NOTE: Do not add any more behavior to this class, as CompositionContainerTests.cs 
        // uses this to verify default behavior of the base class.
        private class NoOverridesCompositionContainer : CompositionContainer
        {
            public NoOverridesCompositionContainer()
            {                
            }
        }
    }
}