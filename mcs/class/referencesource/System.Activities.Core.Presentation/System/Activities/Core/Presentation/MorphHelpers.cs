//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Hosting;
    using System.ComponentModel;
    using Microsoft.VisualBasic.Activities;
    using System.Runtime;
    using System.Linq;
    using System.Activities.Expressions;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Activities.ExpressionParser;
    using System.Diagnostics;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.IO;
    using System.Activities.Presentation;
    using System.Windows;

    static class MorphHelpers
    {

        public static object ArgumentMorphHelper(ModelItem originalValue, ModelProperty newModelProperty)
        {
            Type expressionTypeArgument = newModelProperty.PropertyType.GetGenericArguments()[0];

            return MorphArgument(originalValue, expressionTypeArgument);
        }

        internal static Argument MorphArgument(ModelItem originalValue, Type targetType)
        {
            Argument morphed = null;
            Argument original = (Argument)originalValue.GetCurrentValue();
            ActivityWithResult originalExpression = original.Expression;
            
            if (originalExpression != null)
            {
                Type expressionType = originalExpression.GetType();
                Type expressionGenericType = expressionType.IsGenericType ? expressionType.GetGenericTypeDefinition() : null;

                if (expressionGenericType != null)
                {       
                    bool isLocation = ExpressionHelper.IsGenericLocationExpressionType(originalExpression);
                    ActivityWithResult morphedExpression;
                    EditingContext context = originalValue.GetEditingContext();
                    morphed = Argument.Create(targetType, original.Direction);
                    if (ExpressionHelper.TryMorphExpression(originalExpression, isLocation, targetType, 
                        context, out morphedExpression))
                    {                        
                        morphed.Expression = morphedExpression;
                    }
                    //Microsoft 

                }
            }
            return morphed;
        }

        public static object ActivityActionMorphHelper(ModelItem originalValue, ModelProperty newModelProperty)
        {
            Fx.Assert(newModelProperty.PropertyType.GetGenericArguments().Count() == 1, "This should only be applied for ActivityAction<T>");
            Type activityActionTypeArgument = newModelProperty.PropertyType.GetGenericArguments()[0];
            Type activityActionType = typeof(ActivityAction<>).MakeGenericType(activityActionTypeArgument);
            object activityAction = Activator.CreateInstance(activityActionType);
            ModelItem morphed = ModelFactory.CreateItem(originalValue.GetEditingContext(), activityAction);

            ModelItem originalActivityActionArgument = originalValue.Properties[PropertyNames.ActionArgument].Value;
            if (originalActivityActionArgument != null)
            {
                Type variableType = typeof(DelegateInArgument<>).MakeGenericType(activityActionTypeArgument);
                DelegateInArgument iterationDelegateArgument = (DelegateInArgument)Activator.CreateInstance(variableType);
                iterationDelegateArgument.Name = (string)originalActivityActionArgument.Properties[PropertyNames.NameProperty].Value.GetCurrentValue();
                morphed.Properties[PropertyNames.ActionArgument].SetValue(iterationDelegateArgument);
            }

            ModelItem originalActivityActionHandler = originalValue.Properties[PropertyNames.ActionHandler].Value;
            if (originalActivityActionHandler != null)
            {
                morphed.Properties[PropertyNames.ActionHandler].SetValue(originalActivityActionHandler);
                originalValue.Properties[PropertyNames.ActionHandler].SetValue(null);
            }

            return morphed;
        }

        public static object ActivityFuncMorphHelper(ModelItem originalValue, ModelProperty newModelProperty)
        {
            Fx.Assert(newModelProperty.PropertyType.GetGenericArguments().Count() == 2, "This should only be applied for ActivityFunc<TArgument, TResult>");
            Type activityFuncArgumentType = newModelProperty.PropertyType.GetGenericArguments()[0];
            Type activityFuncResultType = newModelProperty.PropertyType.GetGenericArguments()[1];
            Type activityFuncType = typeof(ActivityFunc<,>).MakeGenericType(activityFuncArgumentType, activityFuncResultType);
            object activityFunc = Activator.CreateInstance(activityFuncType);
            ModelItem morphed = ModelFactory.CreateItem(originalValue.GetEditingContext(), activityFunc);

            ModelItem originalActivityFuncArgument = originalValue.Properties[PropertyNames.ActionArgument].Value;
            if (originalActivityFuncArgument != null)
            {
                Type argumentType = typeof(DelegateInArgument<>).MakeGenericType(activityFuncArgumentType);
                DelegateInArgument newActivityActionArgument = (DelegateInArgument)Activator.CreateInstance(argumentType);
                newActivityActionArgument.Name = (string)originalActivityFuncArgument.Properties[PropertyNames.NameProperty].Value.GetCurrentValue();
                morphed.Properties[PropertyNames.ActionArgument].SetValue(newActivityActionArgument);
            }

            ModelItem originalActivityFuncResult = originalValue.Properties[PropertyNames.ResultProperty].Value;
            if (originalActivityFuncResult != null)
            {
                Type resultType = typeof(DelegateOutArgument<>).MakeGenericType(activityFuncResultType);
                DelegateOutArgument newActivityActionResult = (DelegateOutArgument)Activator.CreateInstance(resultType);
                newActivityActionResult.Name = (string)originalActivityFuncResult.Properties[PropertyNames.NameProperty].Value.GetCurrentValue();
                morphed.Properties[PropertyNames.ResultProperty].SetValue(newActivityActionResult);
            }

            ModelItem originalActivityActionHandler = originalValue.Properties[PropertyNames.ActionHandler].Value;
            if (originalActivityActionHandler != null)
            {
                morphed.Properties[PropertyNames.ActionHandler].SetValue(originalActivityActionHandler);
                originalValue.Properties[PropertyNames.ActionHandler].SetValue(null);
            }

            return morphed;
        }

    }
}
