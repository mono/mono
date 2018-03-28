//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Runtime;

    class CompiledScopeCriteria
    {
        string compiledScope;
        CompiledScopeCriteriaMatchBy matchBy;

        public CompiledScopeCriteria(string compiledScope, CompiledScopeCriteriaMatchBy matchBy)
        {
            Fx.Assert(compiledScope != null, "The compiledScope must be non null.");
            this.compiledScope = compiledScope;
            this.matchBy = matchBy;
        }

        public string CompiledScope
        {
            get
            {
                return this.compiledScope;
            }
        }

        public CompiledScopeCriteriaMatchBy MatchBy
        {
            get
            {
                return this.matchBy;
            }
        }
    }
}
