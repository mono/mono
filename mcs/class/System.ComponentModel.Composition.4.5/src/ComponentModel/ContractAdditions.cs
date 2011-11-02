// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace System.Diagnostics.Contracts
{ 
#if CONTRACTS_FULL
    [ContractClassFor(typeof(ComposablePart))]
    internal abstract class ComposablePartContract : ComposablePart
    {
        public override IEnumerable<ExportDefinition> ExportDefinitions
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<ExportDefinition>>() != null);

                throw new NotImplementedException();
            }
        }

        public override IEnumerable<ImportDefinition> ImportDefinitions
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<ImportDefinition>>() != null);

                throw new NotImplementedException();
            }
        }

        public override object GetExportedValue(ExportDefinition definition)
        {
            Contract.Requires(definition != null);

            throw new NotImplementedException();
        }

        public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
        {
            Contract.Requires(definition != null);
            Contract.Requires(exports != null);

            throw new NotImplementedException();
        }
    }

    [ContractClassFor(typeof(ComposablePartDefinition))]
    internal abstract class ComposablePartDefinitionContract : ComposablePartDefinition
    {
        public override IEnumerable<ExportDefinition> ExportDefinitions
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<ExportDefinition>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ExportDefinition>>(), e => e != null));

                throw new NotImplementedException();
            }
        }

        public override IEnumerable<ImportDefinition> ImportDefinitions
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<ImportDefinition>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ImportDefinition>>(), i => i != null));

                throw new NotImplementedException();
            }
        }

        public override ComposablePart CreatePart()
        {
            Contract.Ensures(Contract.Result<ComposablePart>() != null);
            throw new NotImplementedException();
        }
    }

    [ContractClassFor(typeof(ICompositionElement))]
    internal abstract class ICompositionElementContract : ICompositionElement
    {
        public string DisplayName
        {
            get 
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
                
                throw new NotImplementedException(); 
            }
        }

        public ICompositionElement Origin
        {
            get { throw new NotImplementedException(); }
        }
    }

    [ContractClassFor(typeof(ICompositionService))]
    internal abstract class ICompositionServiceContract : ICompositionService
    {
        public void SatisfyImportsOnce(ComposablePart part)
        {
            Contract.Requires(part != null);
            throw new NotImplementedException();
        }
    }
#endif
}