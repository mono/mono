//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Xaml
{
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;

    internal static class ActivityBuilderHelper
    {
        internal static bool IsActivityBuilderType(ModelItem modelItem)
        {
            if (null == modelItem)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("modelItem"));
            }
            return modelItem.ItemType.IsAssignableFrom(typeof(ActivityBuilder));
        }

        internal static List<Variable> GetVariables(object input)
        {
            if (null == input)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("input"));
            }
            ModelItem astAsModelItem = input as ModelItem;
            ActivityBuilder instance = input as ActivityBuilder;
            if (null != astAsModelItem)
            {
                if (!astAsModelItem.ItemType.IsAssignableFrom(typeof(ActivityBuilder)))
                {
                    throw FxTrace.Exception.AsError(new InvalidCastException(astAsModelItem.ItemType.FullName));
                }
                instance = (ActivityBuilder)astAsModelItem.GetCurrentValue();
            }
            else if (null == instance)
            {
                throw FxTrace.Exception.AsError(new InvalidCastException(input.GetType().FullName));
            }

            List<Variable> variables = new List<Variable>();
            foreach (DynamicActivityProperty property in instance.Properties)
            {
                if (property != null)
                {
                    Variable autoVariable = GetVariableFromProperty(property);
                    if (autoVariable != null)
                    {
                        variables.Add(autoVariable);
                    }
                }
            }
            return variables;
        }

        internal static Variable GetVariableFromProperty(DynamicActivityProperty property)
        {
            Type variableType = null;
            Variable autoVariable = null;

            if (property.Type != null)
            {
                Type propertyType = property.Type;

                // if the property is an Argument<T> create a variable of type T
                if (propertyType != null && typeof(Argument).IsAssignableFrom(propertyType))
                {
                    if (propertyType.IsGenericType)
                    {
                        variableType = propertyType.GetGenericArguments()[0];
                    }
                    else
                    {
                        variableType = typeof(object);
                    }
                }
            }
            if (variableType != null)
            {
                autoVariable = Variable.Create(property.Name, variableType, VariableModifiers.None);
                Argument argument = property.Value as Argument;
                if (argument != null)
                {
                    autoVariable.Default = argument.Expression;
                }
            }
            return autoVariable;
        }
    }
}
