//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Metadata
{
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Validation;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;

    /// <summary>
    /// A helper class to provide additional functionalities regarding Activity arguments.
    /// </summary>
    public static class ActivityArgumentHelper
    {
        private static Dictionary<Type, Func<Activity, IEnumerable<ArgumentAccessor>>> argumentAccessorsGenerators = new Dictionary<Type, Func<Activity, IEnumerable<ArgumentAccessor>>>();

        /// <summary>
        /// Registers with an activity type a function to generate a list of ArgumentAccessors.
        /// </summary>
        /// <param name="activityType">The activity type.</param>
        /// <param name="argumentAccessorsGenerator">A function which takes in an activity instance (of type activityType) and returns a list of ArgumentAccessors.</param>
        public static void RegisterAccessorsGenerator(Type activityType, Func<Activity, IEnumerable<ArgumentAccessor>> argumentAccessorsGenerator)
        {
            if (activityType == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityType");
            }

            if (argumentAccessorsGenerator == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentAccessorsGenerator");
            }

            // IsAssignableFrom() returns true if activityType is the generic type definition inheriting from Activity.
            if (!typeof(Activity).IsAssignableFrom(activityType))
            {
                throw FxTrace.Exception.Argument("activityType", string.Format(CultureInfo.CurrentCulture, SR.TypeDoesNotInheritFromActivity, activityType.Name));
            }

            argumentAccessorsGenerators[activityType] = argumentAccessorsGenerator;
        }

        internal static bool TryGetArgumentAccessorsGenerator(Type activityType, out Func<Activity, IEnumerable<ArgumentAccessor>> argumentAccessorsGenerator)
        {
            Fx.Assert(activityType != null, "activityType cannot be null.");

            bool argumentAccessorsGeneratorFound = argumentAccessorsGenerators.TryGetValue(activityType, out argumentAccessorsGenerator);
            if (!argumentAccessorsGeneratorFound && activityType.IsGenericType && !activityType.IsGenericTypeDefinition)
            {
                argumentAccessorsGeneratorFound = argumentAccessorsGenerators.TryGetValue(activityType.GetGenericTypeDefinition(), out argumentAccessorsGenerator);
            }

            return argumentAccessorsGeneratorFound;
        }

        internal static void UpdateInvalidArgumentsIfNecessary(object sender, ValidationService.ErrorsMarkedEventArgs args)
        {
            if (args.Reason != ValidationReason.ModelChange)
            {
                return;
            }

            if (args.Errors.Count == 0)
            {
                return;
            }

            using (EditingScope editingScope = args.ModelTreeManager.CreateEditingScope(string.Empty))
            {
                // Prevent the validation -> fix arguments -> validation loop.
                editingScope.SuppressUndo = true;

                // Suppress validation. We will do it ourselves (see below)
                editingScope.SuppressValidationOnComplete = true;

                // Re-compile erroreous expressions to see if update is necessary
                ValidationService validationService = args.Context.Services.GetRequiredService<ValidationService>();
                ArgumentAccessorWrapperCache argumentAccessorWrapperCache = new ArgumentAccessorWrapperCache();
                List<ExpressionReplacement> expressionReplacements = ComputeExpressionReplacements(args.Errors.Select(error => error.Source).OfType<ActivityWithResult>(), args.Context, argumentAccessorWrapperCache);
                bool argumentReplacementOccurred = false;
                if (expressionReplacements.Count > 0)
                {
                    try
                    {
                        foreach (ExpressionReplacement expressionReplacement in expressionReplacements)
                        {
                            if (expressionReplacement.TryReplaceArgument(args.ModelTreeManager, validationService))
                            {
                                argumentReplacementOccurred = true;
                            }
                        }

                        if (argumentReplacementOccurred)
                        {
                            args.Handled = true;
                            editingScope.Complete();
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        // We handle exception here instead of letting WF Designer handle it, so that the validation below will run even
                        // if any of the ArgumentAccessor.Setter methods throw exceptions.
                        ErrorReporting.ShowErrorMessage(e);
                    }

                    // Since any pending validation will be canceled if argument replacement occured (=has model change), we need to re-validate the workflow.
                    // We suppressed validation upon EditingScope completion and do it ourselves, because
                    // the argument replacement could have been done directly to the underlying activity instance, rather than thru ModelItem.
                    if (argumentReplacementOccurred)
                    {
                        validationService.ValidateWorkflow();
                    }
                }
            }
        }

        internal static List<ExpressionReplacement> ComputeExpressionReplacements(IEnumerable<ActivityWithResult> expressions, EditingContext context, ArgumentAccessorWrapperCache argumentAccessorWrapperCache)
        {
            Fx.Assert(expressions != null, "expressions cannot be null.");
            Fx.Assert(context != null, "context cannot be null.");
            Fx.Assert(argumentAccessorWrapperCache != null, "argumentAccessorWrapperCache cannot be null.");

            HashSet<Assign> assignsWithRValueToFix = new HashSet<Assign>();
            Dictionary<Assign, Type> assignLValueTypes = new Dictionary<Assign, Type>();
            List<ExpressionReplacement> expressionReplacements = new List<ExpressionReplacement>();
            foreach (ActivityWithResult expression in expressions)
            {
                Activity parentActivity = ValidationService.GetParent(expression);
                if (parentActivity == null)
                {
                    continue;
                }

                Assign assignActivity = parentActivity as Assign;
                if (assignActivity != null && !ExpressionHelper.IsGenericLocationExpressionType(expression))
                {
                    assignsWithRValueToFix.Add(assignActivity);
                }
                else
                {
                    ExpressionReplacement expressionReplacement = ComputeExpressionReplacement(expression, parentActivity, context, argumentAccessorWrapperCache);
                    if (expressionReplacement != null)
                    {
                        expressionReplacements.Add(expressionReplacement);
                        if (assignActivity != null)
                        {
                            Type expectedReturnType = expressionReplacement.NewArgument.ArgumentType;
                            assignLValueTypes[assignActivity] = expectedReturnType;
                        }
                    }
                }
            }

            // Special handle Assign R-Values: Assign.To.ArgumentType must be the same as Assign.Value.ArgumentType.
            foreach (Assign assignWithRValueToFix in assignsWithRValueToFix)
            {
                Type expectedReturnType;
                if (assignLValueTypes.TryGetValue(assignWithRValueToFix, out expectedReturnType))
                {
                    assignLValueTypes.Remove(assignWithRValueToFix);
                }
                else if (assignWithRValueToFix.To != null)
                {
                    expectedReturnType = assignWithRValueToFix.To.ArgumentType;
                }

                ExpressionReplacement expressionReplacement = ComputeExpressionReplacement(assignWithRValueToFix.Value.Expression, assignWithRValueToFix, context, argumentAccessorWrapperCache, expectedReturnType);
                if (expressionReplacement != null)
                {
                    expressionReplacements.Add(expressionReplacement);
                }
            }

            // These Assign activities have their L-value argument (To) changed but not the R-value argument (Value).
            // Now make sure that the R-value arguments are compatible with the L-value argument.
            foreach (KeyValuePair<Assign, Type> kvp in assignLValueTypes)
            {
                Assign remainingAssign = kvp.Key;
                Type expectedReturnType = kvp.Value;
                if (remainingAssign.Value != null && remainingAssign.Value.Expression != null)
                {
                    ActivityWithResult expression = remainingAssign.Value.Expression;
                    if (expression.ResultType != expectedReturnType)
                    {
                        ExpressionReplacement expressionReplacement = ComputeExpressionReplacement(expression, remainingAssign, context, argumentAccessorWrapperCache, expectedReturnType);
                        if (expressionReplacement != null)
                        {
                            expressionReplacements.Add(expressionReplacement);
                        }
                    }
                }
            }

            return expressionReplacements;
        }

        internal static ExpressionReplacement ComputeExpressionReplacement(ActivityWithResult expression, Activity parentActivity, EditingContext context, ArgumentAccessorWrapperCache argumentAccessorWrapperCache, Type preferredReturnType = null)
        {
            Fx.Assert(expression != null, "expressions cannot be null.");
            Fx.Assert(parentActivity != null, "parentActivity cannot be null.");
            Fx.Assert(context != null, "context cannot be null.");
            Fx.Assert(argumentAccessorWrapperCache != null, "argumentAccessorWrapperCache cannot be null.");

            IEnumerable<ArgumentAccessorWrapper> argumentAccessorWrappers = argumentAccessorWrapperCache.GetArgumentAccessorWrappers(parentActivity);
            if (argumentAccessorWrappers != null)
            {
                ArgumentAccessorWrapper argumentAccessorWrapper = argumentAccessorWrappers.FirstOrDefault(wrapper => object.ReferenceEquals(wrapper.Argument.Expression, expression));
                if (argumentAccessorWrapper != null)
                {
                    bool isLocationExpression = ExpressionHelper.IsGenericLocationExpressionType(expression);
                    bool canInferType = true;
                    Type expectedReturnType;
                    ActivityWithResult morphedExpression;
                    if (preferredReturnType != null)
                    {
                        expectedReturnType = preferredReturnType;
                    }
                    else
                    {
                        canInferType = ExpressionHelper.TryInferReturnType(expression, context, out expectedReturnType);
                    }

                    if (canInferType && expectedReturnType != null && ExpressionHelper.TryMorphExpression(expression, isLocationExpression, expectedReturnType, context, out morphedExpression))
                    {
                        Type expressionResultType = isLocationExpression ? expression.ResultType.GetGenericArguments()[0] : expression.ResultType;
                        if (expressionResultType != expectedReturnType)
                        {
                            Argument newArgument = Argument.Create(expectedReturnType, argumentAccessorWrapper.Argument.Direction);
                            newArgument.Expression = morphedExpression;
                            return new ExpressionReplacement(expression, argumentAccessorWrapper.Argument, newArgument, argumentAccessorWrapper.ArgumentAccessor);
                        }
                    }
                }
            }

            return null;
        }

        internal sealed class ArgumentAccessorWrapper
        {
            public ArgumentAccessorWrapper(ArgumentAccessor argumentAccessor, Argument argument)
            {
                Fx.Assert(argumentAccessor != null, "argumentAccessor cannot be null.");
                Fx.Assert(argument != null, "argument cannot be null.");

                this.ArgumentAccessor = argumentAccessor;
                this.Argument = argument;
            }

            public ArgumentAccessor ArgumentAccessor { get; private set; }

            public Argument Argument { get; private set; }
        }

        // This cache ensures that during the entire argument-fixing process (i.e. within UpdateInvalidArgumentsIfNecessary):
        // 1) An argument accessor generator function is called on an activity instance at most once.
        // 2) An argument accessor Getter function is called on an activity instance at most once.
        internal sealed class ArgumentAccessorWrapperCache
        {
            private Dictionary<Activity, List<ArgumentAccessorWrapper>> argumentAccessorWrappersCache;

            public ArgumentAccessorWrapperCache()
            {
                this.argumentAccessorWrappersCache = new Dictionary<Activity, List<ArgumentAccessorWrapper>>();
            }

            public List<ArgumentAccessorWrapper> GetArgumentAccessorWrappers(Activity activity)
            {
                Fx.Assert(activity != null, "activity cannot be null.");

                List<ArgumentAccessorWrapper> argumentAccessorWrappers = null;
                if (!this.argumentAccessorWrappersCache.TryGetValue(activity, out argumentAccessorWrappers))
                {
                    Func<Activity, IEnumerable<ArgumentAccessor>> argumentAccessorsGenerator;
                    if (ActivityArgumentHelper.TryGetArgumentAccessorsGenerator(activity.GetType(), out argumentAccessorsGenerator))
                    {
                        IEnumerable<ArgumentAccessor> argumentAccessors = argumentAccessorsGenerator(activity);
                        if (argumentAccessors != null)
                        {
                            argumentAccessorWrappers = new List<ArgumentAccessorWrapper>();
                            foreach (ArgumentAccessor argumentAccessor in argumentAccessors)
                            {
                                if (argumentAccessor != null && argumentAccessor.Getter != null)
                                {
                                    Argument argument = argumentAccessor.Getter(activity);
                                    if (argument != null)
                                    {
                                        ArgumentAccessorWrapper argumentAccessorWrapper = new ArgumentAccessorWrapper(argumentAccessor, argument);
                                        argumentAccessorWrappers.Add(argumentAccessorWrapper);
                                    }
                                }
                            }

                            this.argumentAccessorWrappersCache.Add(activity, argumentAccessorWrappers);
                        }
                    }
                }

                return argumentAccessorWrappers;
            }
        }

        internal sealed class ExpressionReplacement
        {
            public ExpressionReplacement(ActivityWithResult expressionToReplace, Argument oldArgument, Argument newArgument, ArgumentAccessor argumentAccessor)
            {
                Fx.Assert(expressionToReplace != null, "expressionToReplace cannot be null.");
                Fx.Assert(oldArgument != null, "oldArgument cannot be null.");
                Fx.Assert(newArgument != null, "newArgument cannot be null.");
                Fx.Assert(argumentAccessor != null, "argumentAccessor cannot be null.");

                this.ExpressionToReplace = expressionToReplace;
                this.OldArgument = oldArgument;
                this.NewArgument = newArgument;
                this.ArgumentAccessor = argumentAccessor;
            }

            public ActivityWithResult ExpressionToReplace
            {
                get;
                private set;
            }

            public Argument OldArgument
            {
                get;
                private set;
            }

            public Argument NewArgument
            {
                get;
                private set;
            }

            public ArgumentAccessor ArgumentAccessor
            {
                get;
                private set;
            }

            public bool TryReplaceArgument(ModelTreeManager modelTreeManager, ValidationService validationService)
            {
                Fx.Assert(modelTreeManager != null, "modelTreeManager cannot be null.");
                Fx.Assert(validationService != null, "validationService cannot be null.");

                ModelItem expressionModelItem = modelTreeManager.GetModelItem(this.ExpressionToReplace);
                if (expressionModelItem != null)
                {
                    ModelItem argumentModelItem = expressionModelItem.Parent;
                    ModelItem parentObject = argumentModelItem.Parent;

                    if (argumentModelItem.Source != null)
                    {
                        ModelProperty argumentProperty = parentObject.Properties[argumentModelItem.Source.Name];
                        Type argumentPropertyType = argumentProperty.PropertyType;
                        if (argumentPropertyType == typeof(InArgument) || argumentPropertyType == typeof(OutArgument) || argumentPropertyType == typeof(InOutArgument))
                        {
                            ModelItem oldArgumentModel = argumentProperty.Value;
                            ModelItem newArgumentModel = argumentProperty.SetValue(this.NewArgument);

                            // Make sure argument.Expression is wrapped in ModelItem as well
                            ModelItem newExpressionModel = newArgumentModel.Properties["Expression"].Value;

                            return true;
                        }
                    }
                }
                else
                {
                    Activity parentActivity = ValidationService.GetParent(this.ExpressionToReplace);
                    if (this.ArgumentAccessor.Setter != null)
                    {
                        try
                        {
                            validationService.DeactivateValidation();
                            this.ArgumentAccessor.Setter(parentActivity, this.NewArgument);
                            return true;
                        }
                        finally
                        {
                            validationService.ActivateValidation();
                        }
                    }
                }

                return false;
            }
        }
    }
}
