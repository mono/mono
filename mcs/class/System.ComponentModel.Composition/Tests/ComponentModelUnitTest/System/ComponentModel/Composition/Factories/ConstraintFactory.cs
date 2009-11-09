// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Factories
{
    internal static class ConstraintFactory
    {
        public static Expression<Func<ExportDefinition, bool>> Create(string contractName)
        {
            return definition => definition.ContractName.Equals(contractName);
        }
    }
}