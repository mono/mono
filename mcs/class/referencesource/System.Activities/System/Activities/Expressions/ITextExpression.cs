//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Linq.Expressions;

    public interface ITextExpression
    {
        string ExpressionText
        {
            get;
        }

        string Language
        {
            get;
        }

        bool RequiresCompilation
        {
            get;
        }

        Expression GetExpressionTree();
    }
}
