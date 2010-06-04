// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.ReflectionModel
{
    internal class PartCreatorParameterImportDefinition : ReflectionParameterImportDefinition, IPartCreatorImportDefinition
    {
        private readonly ContractBasedImportDefinition _productImportDefinition;

        public PartCreatorParameterImportDefinition(
            Lazy<ParameterInfo> importingLazyParameter,
            ICompositionElement origin,
            ContractBasedImportDefinition productImportDefinition)
            : base(importingLazyParameter, CompositionConstants.PartCreatorContractName, CompositionConstants.PartCreatorTypeIdentity,
                productImportDefinition.RequiredMetadata, productImportDefinition.Cardinality, CreationPolicy.Any, origin)
        {
            Assumes.NotNull(productImportDefinition);
            this._productImportDefinition = productImportDefinition;
        }

        public ContractBasedImportDefinition ProductImportDefinition { get { return this._productImportDefinition; } }

        public override bool IsConstraintSatisfiedBy(ExportDefinition exportDefinition)
        {
            if (!base.IsConstraintSatisfiedBy(exportDefinition))
            {
                return false;
            }
            return PartCreatorExportDefinition.IsProductConstraintSatisfiedBy(this._productImportDefinition, exportDefinition);
        }

        public override Expression<Func<ExportDefinition, bool>> Constraint
        {
            get
            {
                return ConstraintServices.CreatePartCreatorConstraint(base.Constraint, this._productImportDefinition);
            }
        }
    }
}
