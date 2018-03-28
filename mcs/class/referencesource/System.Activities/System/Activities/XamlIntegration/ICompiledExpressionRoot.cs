//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Runtime;

    public interface ICompiledExpressionRoot
    {
        string GetLanguage();

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters, Justification = "Interface is intended to be implemented only by generated code and consumed only by internal code")]
        bool CanExecuteExpression(string expressionText, bool isReference, IList<LocationReference> locations, out int expressionId);

        object InvokeExpression(int expressionId, IList<LocationReference> locations, ActivityContext activityContext);
        object InvokeExpression(int expressionId, IList<Location> locations);

        IList<string> GetRequiredLocations(int expressionId);

        Expression GetExpressionTreeForExpression(int expressionId, IList<LocationReference> locationReferences);
    }
}
