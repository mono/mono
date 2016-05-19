//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Presentation.Expressions;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using Microsoft.VisualBasic.Activities;

    //This serves as expression morph helper for following non-textual expressions:
    //1) Literal<T>
    //2) VariableValue<T>/VariableReference<T>
    //The helper will morph these expressions to what specified at root object by ExpressionActivityEditor attached property
    class NonTextualExpressionMorphHelper : ExpressionMorphHelper
    {
        public override bool TryMorphExpression(ActivityWithResult expression, bool isLocationExpression, Type newType, 
            EditingContext context, out ActivityWithResult newExpression)
        {
            Fx.Assert(expression != null, "Original expression shouldn't be null in morph helper");
            Fx.Assert(context != null, "EditingContext shouldn't be null in morph helper");
            newExpression = null;
            if (expression.ResultType == newType && 
                (ExpressionHelper.IsGenericLocationExpressionType(expression) == isLocationExpression))
            {
                newExpression = expression;
                return true;
            }

            if (context != null)
            {
                string expressionEditor = ExpressionHelper.GetRootEditorSetting(context.Services.GetService<ModelTreeManager>(), WorkflowDesigner.GetTargetFramework(context));
                ParserContext parserContext = new ParserContext();
                string expressionText = ExpressionHelper.GetExpressionString(expression, parserContext);
                if (!string.IsNullOrEmpty(expressionEditor))
                {
                    return ExpressionTextBox.TryConvertFromString(expressionEditor, expressionText, isLocationExpression, newType, out newExpression);
                }
            }

            return false;
        }
    }
}
