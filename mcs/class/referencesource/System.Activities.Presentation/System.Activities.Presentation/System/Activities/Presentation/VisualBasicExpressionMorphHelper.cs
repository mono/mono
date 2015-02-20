//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.ExpressionParser;
    using System.Activities.Presentation.Expressions;
    using System.Activities.Presentation.View;
    using System.Runtime;
    using Microsoft.VisualBasic.Activities;

    class VisualBasicExpressionMorphHelper : ExpressionMorphHelper
    {
        public override bool TryInferReturnType(ActivityWithResult expression, EditingContext context, out Type returnType)
        {
            bool succeeded = false;
            Fx.Assert(expression.GetType().GetGenericTypeDefinition() == typeof(VisualBasicValue<>) ||
                expression.GetType().GetGenericTypeDefinition() == typeof(VisualBasicReference<>), "VisualBasicExpressionMorphHelper should only apply to VB expression.");
                            
            SourceExpressionException compileError;
            VisualBasicSettings settings;
            if (ExpressionHelper.IsGenericLocationExpressionType(expression))
            {
                VisualBasicDesignerHelper.RecompileVisualBasicReference(
                        expression,
                        out returnType,
                        out compileError,
                        out settings);
            }
            else
            {
                VisualBasicDesignerHelper.RecompileVisualBasicValue(
                     expression,
                     out returnType,
                     out compileError,
                     out settings);
            }
            if (compileError == null)
            {
                succeeded = true;                    
                if (settings != null)
                {
                    //merge with import designer
                    foreach (VisualBasicImportReference reference in settings.ImportReferences)
                    {
                        ImportDesigner.AddImport(reference.Import, context);
                    }
                }
            }
            return succeeded;
        }
        
        public override bool TryMorphExpression(ActivityWithResult expression, bool isLocationExpression, Type newType, 
            EditingContext context, out ActivityWithResult newExpression)
        {
            string expressionText = ExpressionHelper.GetExpressionString(expression);            
            newExpression = VisualBasicEditor.CreateExpressionFromString(newType, expressionText, isLocationExpression, new ParserContext());
            return true;
        }
    }
}
