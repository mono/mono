//---------------------------------------------------------------------
// <copyright file="ParameterRetriever.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;

using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Diagnostics;

namespace System.Data.Common.CommandTrees.Internal
{
    internal sealed class ParameterRetriever : BasicCommandTreeVisitor
    {
        private readonly Dictionary<string, DbParameterReferenceExpression> paramMappings = new Dictionary<string, DbParameterReferenceExpression>();

        private ParameterRetriever()
        {
        }

        internal static System.Collections.ObjectModel.ReadOnlyCollection<DbParameterReferenceExpression> GetParameters(DbCommandTree tree)
        {
            Debug.Assert(tree != null, "Ensure command tree is non-null before calling ParamterRetriever.GetParameters");

            ParameterRetriever retriever = new ParameterRetriever();
            retriever.VisitCommandTree(tree);
            return retriever.paramMappings.Values.ToList().AsReadOnly();
        }

        public override void Visit(DbParameterReferenceExpression expression)
        {
            Debug.Assert(expression != null, "Command tree subexpressions should never be null");

            this.paramMappings[expression.ParameterName] = expression;
        }
    }
}
