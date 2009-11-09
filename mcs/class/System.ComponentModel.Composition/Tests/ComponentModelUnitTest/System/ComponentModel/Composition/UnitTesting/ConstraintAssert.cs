// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.UnitTesting
{
    internal static class ConstraintAssert
    {
        public static void Contains(Expression<Func<ExportDefinition, bool>> constraint, string contractName)
        {
            Contains(constraint, contractName, Enumerable.Empty<KeyValuePair<string, Type>>());
        }

        public static void Contains(Expression<Func<ExportDefinition, bool>> constraint, string contractName, IEnumerable<KeyValuePair<string, Type>> requiredMetadata)
        {
            string actualContractName;
            IEnumerable<KeyValuePair<string, Type>> actualRequiredMetadata;
            bool success = TryParseConstraint(constraint, out actualContractName, out actualRequiredMetadata);

            Assert.IsTrue(success);
            Assert.AreEqual(contractName, actualContractName);
            EnumerableAssert.AreEqual(requiredMetadata, actualRequiredMetadata);
        }

        private static bool TryParseConstraint(Expression<Func<ExportDefinition, bool>> constraint, out string contractName, out IEnumerable<KeyValuePair<string, Type>> requiredMetadata)
        {
            return ContraintParser.TryParseConstraint(constraint, out contractName, out requiredMetadata);
        }
    }
}
