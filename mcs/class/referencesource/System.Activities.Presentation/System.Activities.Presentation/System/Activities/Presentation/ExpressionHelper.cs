using System;
using System.Activities.Expressions;
using System.Activities.Presentation.Expressions;
using System.Activities.Presentation.Internal.PropertyEditing;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Activities.Presentation;
using Microsoft.VisualBasic.Activities;

namespace System.Activities.Presentation
{
    internal static class ExpressionHelper
    {
        internal static string GetExpressionString(Activity expression)
        {
            return ExpressionHelper.GetExpressionString(expression, null as ParserContext);
        }

        internal static string GetExpressionString(Activity expression, ModelItem owner)
        {
            ParserContext context = new ParserContext(owner);
            return ExpressionHelper.GetExpressionString(expression, context);
        }

        internal static string GetExpressionString(Activity expression, ParserContext context)
        {
            string expressionString = null;            
            if (expression != null)
            {
                Type expressionType = expression.GetType();
                Type expressionArgumentType = expressionType.IsGenericType ? expressionType.GetGenericArguments()[0] : typeof(object);
                bool isLiteral = expressionType.IsGenericType ? Type.Equals(typeof(Literal<>), expressionType.GetGenericTypeDefinition()) : false;

                //handle ITextExpression
                if (expression is ITextExpression)
                {
                    ITextExpression textExpression = expression as ITextExpression;
                    expressionString = textExpression.ExpressionText;
                }
                //handle Literal Expression
                else if (isLiteral)
                {
                    TypeConverter converter = XamlUtilities.GetConverter(expressionArgumentType);
                    if (converter != null && converter.CanConvertTo(context, typeof(string)))
                    {
                        PropertyInfo literalValueProperty = expressionType.GetProperty("Value");
                        Fx.Assert(literalValueProperty != null && literalValueProperty.GetGetMethod() != null, "Literal<T> must have the Value property with a public get accessor.");
                        object literalValue = literalValueProperty.GetValue(expression, null);
                        string convertedString = null;
                        if (literalValue != null)
                        {
                            try
                            {
                                convertedString = converter.ConvertToString(context, literalValue);
                            }
                            catch (ArgumentException)
                            {
                                convertedString = literalValue.ToString();
                            }
                        }
                        expressionString = expressionArgumentType == typeof(string) ? ("\"" + convertedString + "\"") : convertedString;
                    }
                }
                else if (expressionType.IsGenericType &&
                (expressionType.GetGenericTypeDefinition() == typeof(VariableValue<>) ||
                expressionType.GetGenericTypeDefinition() == typeof(VariableReference<>)))
                {
                    PropertyInfo variableProperty = expression.GetType().GetProperty("Variable");
                    Variable variable = variableProperty.GetValue(expression, null) as Variable;
                    if (variable != null)
                    {
                        expressionString = variable.Name;
                    }
                }
            }

            return expressionString;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
    Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        internal static ActivityWithResult TryCreateLiteral(Type type, string expressionText, ParserContext context)
        {
            //try easy way first - look if there is a type conversion which supports conversion between expression type and string
            TypeConverter literalValueConverter = null;
            bool isQuotedString = false;
            if (CanTypeBeSerializedAsLiteral(type))
            {
                bool shouldBeQuoted = typeof(string) == type;

                //whether string begins and ends with quotes '"'. also, if there are
                //more quotes within than those begining and ending ones, do not bother with literal - assume this is an expression.
                isQuotedString = shouldBeQuoted &&
                        expressionText.StartsWith("\"", StringComparison.CurrentCulture) &&
                        expressionText.EndsWith("\"", StringComparison.CurrentCulture) &&
                        expressionText.IndexOf("\"", 1, StringComparison.CurrentCulture) == expressionText.Length - 1;

                //if expression is a string, we must ensure it is quoted, in case of other types - just get the converter
                if ((shouldBeQuoted && isQuotedString) || !shouldBeQuoted)
                {
                    literalValueConverter = TypeDescriptor.GetConverter(type);
                }
            }

            //if there is converter - try to convert
            if (null != literalValueConverter && literalValueConverter.CanConvertFrom(context, typeof(string)))
            {
                try
                {
                    var valueToConvert = isQuotedString ? expressionText.Substring(1, expressionText.Length - 2) : expressionText;
                    var converterValue = literalValueConverter.ConvertFrom(context, CultureInfo.CurrentCulture, valueToConvert);
                    //ok, succeeded - create literal of given type
                    var concreteExpType = typeof(Literal<>).MakeGenericType(type);
                    return (ActivityWithResult)Activator.CreateInstance(concreteExpType, converterValue);
                }
                //conversion failed - do nothing, let VB compiler take care of the expression
                catch { }
            }

            return null;
        }

        internal static bool CanTypeBeSerializedAsLiteral(Type type)
        {
            //type must be set and cannot be object
            if (null == type || typeof(object) == type)
            {
                return false;
            }

            return type.IsPrimitive || type == typeof(string) || type == typeof(TimeSpan) || type == typeof(DateTime);
        }

        // Test whether this activity is Expression
        internal static bool IsExpression(this Activity activity)
        {
            return activity is ActivityWithResult;
        }

        internal static bool IsGenericLocationExpressionType(ActivityWithResult expression)
        {
            Type expressionType = expression.ResultType;
            return expressionType.IsGenericType && typeof(Location<>) == expressionType.GetGenericTypeDefinition();
        }

        internal static bool TryMorphExpression(ActivityWithResult originalExpression, bool isLocation, Type targetType, 
            EditingContext context, out ActivityWithResult morphedExpression)
        {
            bool succeeded = false;            
            morphedExpression = null;
            if (originalExpression != null)
            {
                Type resultType = originalExpression.ResultType;
                if ((isLocation) && (ExpressionHelper.IsGenericLocationExpressionType(originalExpression) && (targetType == resultType.GetGenericArguments()[0])) ||
                    (!isLocation) && (resultType == targetType))
                {
                    //no need to morph
                    succeeded = true;
                    morphedExpression = originalExpression;
                }
                else
                {
                    Type expressionType = originalExpression.GetType();
                    if (expressionType.IsGenericType)
                    {
                        expressionType = expressionType.GetGenericTypeDefinition();
                    }

                    ExpressionMorphHelperAttribute morphHelperAttribute = ExtensibilityAccessor.GetAttribute<ExpressionMorphHelperAttribute>(expressionType);
                    if (morphHelperAttribute != null)
                    {
                        ExpressionMorphHelper morphHelper = Activator.CreateInstance(morphHelperAttribute.ExpressionMorphHelperType) as ExpressionMorphHelper;
                        if (morphHelper != null)
                        {
                            succeeded = morphHelper.TryMorphExpression(originalExpression, isLocation, targetType, context, out morphedExpression);
                            if (succeeded && morphedExpression != null)
                            {
                                string editorName = ExpressionActivityEditor.GetExpressionActivityEditor(originalExpression);
                                if (!string.IsNullOrWhiteSpace(editorName))
                                {
                                    ExpressionActivityEditor.SetExpressionActivityEditor(morphedExpression, editorName);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                succeeded = true;
            }
            return succeeded;
        }

        // this method may has side effect, it may modify model.
        internal static bool TryInferReturnType(ActivityWithResult expression, EditingContext context, out Type returnType)
        {
            bool succeeded = false;
            returnType = null;
            Type expressionType = expression.GetType();
            if (expressionType.IsGenericType)
            {
                expressionType = expressionType.GetGenericTypeDefinition();
                ExpressionMorphHelperAttribute morphHelperAttribute = ExtensibilityAccessor.GetAttribute<ExpressionMorphHelperAttribute>(expressionType);
                if (morphHelperAttribute != null)
                {
                    ExpressionMorphHelper morphHelper = Activator.CreateInstance(morphHelperAttribute.ExpressionMorphHelperType) as ExpressionMorphHelper;
                    if (morphHelper != null)
                    {
                        succeeded = morphHelper.TryInferReturnType(expression, context, out returnType);
                    }
                }
            }
            return succeeded;   
        }

        internal static string GetRootEditorSetting(ModelTreeManager modelTreeManager, FrameworkName targetFramework)
        {
            return ExpressionSettingHelper.GetRootEditorSetting(modelTreeManager, targetFramework);
        }
    }
}
