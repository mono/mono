// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Factories
{
    partial class ElementFactory
    {
        private class CompositionElement : ICompositionElement
        {
            private readonly string _displayName;
            private readonly ICompositionElement _origin;

            public CompositionElement(string displayName, ICompositionElement origin)
            {
                _displayName = displayName;
                _origin = origin;
            }

            public string DisplayName
            {
                get { return _displayName; }
            }

            public ICompositionElement Origin
            {
                get { return _origin; }
            }
        }
    }
}