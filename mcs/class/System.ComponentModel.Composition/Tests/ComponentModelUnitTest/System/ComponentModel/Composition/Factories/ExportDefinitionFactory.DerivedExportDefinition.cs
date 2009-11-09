// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Factories
{
    partial class ExportDefinitionFactory
    {
        private class DerivedExportDefinition : ExportDefinition, ICompositionElement
        {
            private readonly string _contractName;
            private readonly IDictionary<string, object> _metadata;

            public DerivedExportDefinition(string contractName, IDictionary<string, object> metadata)
            {
                _contractName = contractName;
                _metadata = metadata ?? new Dictionary<string, object>();
            }

            public override string ContractName
            {
                get { return _contractName; }
            }

            public override IDictionary<string, object> Metadata
            {
                get { return _metadata; }
            }

            public string DisplayName
            {
                get { return base.ToString(); }
            }

            public ICompositionElement Origin
            {
                get { return null; }
            }
        }
    }
}
