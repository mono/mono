// ---------------------------------------------------------------------------
// Copyright (C) 2005 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#pragma warning disable 1634, 1691
#define CODE_ANALYSIS
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;

namespace System.Workflow.Activities.Rules
{
    #region RuleExpressionResult class hierarchy
    public abstract class RuleExpressionResult
    {
        public abstract object Value { get; set; }
    }

    public class RuleLiteralResult : RuleExpressionResult
    {
        private object literal;

        public RuleLiteralResult(object literal)
        {
            this.literal = literal;
        }

        public override object Value
        {
            get
            {
                return literal;
            }
            set
            {
                throw new InvalidOperationException(Messages.CannotWriteToExpression);
            }
        }
    }

    internal class RuleFieldResult : RuleExpressionResult
    {
        private object targetObject;
        private FieldInfo fieldInfo;

        public RuleFieldResult(object targetObject, FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException("fieldInfo");

            this.targetObject = targetObject;
            this.fieldInfo = fieldInfo;
        }

        public override object Value
        {
            get
            {
#pragma warning disable 56503
                if (!fieldInfo.IsStatic && targetObject == null)
                {
                    // Accessing a non-static field from null target.
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullField, fieldInfo.Name);
                    RuleEvaluationException exception = new RuleEvaluationException(message);
                    exception.Data[RuleUserDataKeys.ErrorObject] = fieldInfo;
                    throw exception;
                }

                return fieldInfo.GetValue(targetObject);
#pragma warning restore 56503
            }
            set
            {
                if (!fieldInfo.IsStatic && targetObject == null)
                {
                    // Accessing a non-static field from null target.
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullField, fieldInfo.Name);
                    RuleEvaluationException exception = new RuleEvaluationException(message);
                    exception.Data[RuleUserDataKeys.ErrorObject] = fieldInfo;
                    throw exception;
                }

                fieldInfo.SetValue(targetObject, value);
            }
        }
    }

    internal class RulePropertyResult : RuleExpressionResult
    {
        private PropertyInfo propertyInfo;
        private object targetObject;
        private object[] indexerArguments;

        public RulePropertyResult(PropertyInfo propertyInfo, object targetObject, object[] indexerArguments)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            this.targetObject = targetObject;
            this.propertyInfo = propertyInfo;
            this.indexerArguments = indexerArguments;
        }

        public override object Value
        {
            get
            {
#pragma warning disable 56503
                if (!propertyInfo.GetGetMethod(true).IsStatic && targetObject == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullProperty, propertyInfo.Name);
                    RuleEvaluationException exception = new RuleEvaluationException(message);
                    exception.Data[RuleUserDataKeys.ErrorObject] = propertyInfo;
                    throw exception;
                }

                try
                {
                    return propertyInfo.GetValue(targetObject, indexerArguments);
                }
                catch (TargetInvocationException e)
                {
                    // if there is no inner exception, leave it untouched
                    if (e.InnerException == null)
                        throw;
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.Error_PropertyGet,
                        RuleDecompiler.DecompileType(propertyInfo.ReflectedType), propertyInfo.Name, e.InnerException.Message);
                    throw new TargetInvocationException(message, e.InnerException);
                }
#pragma warning restore 56503
            }

            set
            {
                if (!propertyInfo.GetSetMethod(true).IsStatic && targetObject == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullProperty, propertyInfo.Name);
                    RuleEvaluationException exception = new RuleEvaluationException(message);
                    exception.Data[RuleUserDataKeys.ErrorObject] = propertyInfo;
                    throw exception;
                }

                try
                {
                    propertyInfo.SetValue(targetObject, value, indexerArguments);
                }
                catch (TargetInvocationException e)
                {
                    // if there is no inner exception, leave it untouched
                    if (e.InnerException == null)
                        throw;
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.Error_PropertySet,
                        RuleDecompiler.DecompileType(propertyInfo.ReflectedType), propertyInfo.Name, e.InnerException.Message);
                    throw new TargetInvocationException(message, e.InnerException);
                }

            }
        }
    }

    internal class RuleArrayElementResult : RuleExpressionResult
    {
        private Array targetArray;
        private long[] indexerArguments;

        public RuleArrayElementResult(Array targetArray, long[] indexerArguments)
        {
            if (targetArray == null)
                throw new ArgumentNullException("targetArray");
            if (indexerArguments == null)
                throw new ArgumentNullException("indexerArguments");

            this.targetArray = targetArray;
            this.indexerArguments = indexerArguments;
        }

        public override object Value
        {
            get
            {
                return targetArray.GetValue(indexerArguments);
            }

            set
            {
                targetArray.SetValue(value, indexerArguments);
            }
        }
    }
    #endregion

    #region RuleExecution Class
    public class RuleExecution
    {
        private bool halted;    // "Halt" was executed?
        private Activity activity;
        private object thisObject;
        private RuleValidation validation;
        private ActivityExecutionContext activityExecutionContext;
        private RuleLiteralResult thisLiteralResult;

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "1#")]
        public RuleExecution(RuleValidation validation, object thisObject)
        {
            if (validation == null)
                throw new ArgumentNullException("validation");
            if (thisObject == null)
                throw new ArgumentNullException("thisObject");
            if (validation.ThisType != thisObject.GetType())
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, Messages.ValidationMismatch,
                        RuleDecompiler.DecompileType(validation.ThisType),
                        RuleDecompiler.DecompileType(thisObject.GetType())));

            this.validation = validation;
            this.activity = thisObject as Activity;
            this.thisObject = thisObject;
            this.thisLiteralResult = new RuleLiteralResult(thisObject);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "1#")]
        public RuleExecution(RuleValidation validation, object thisObject, ActivityExecutionContext activityExecutionContext)
            : this(validation, thisObject)
        {
            this.activityExecutionContext = activityExecutionContext;
        }

        public object ThisObject
        {
            get { return thisObject; }
        }

        public Activity Activity
        {
            get
            {
#pragma warning disable 56503
                if (activity == null)
                    throw new InvalidOperationException(Messages.NoActivity);
                return activity;
#pragma warning restore 56503
            }
        }

        public RuleValidation Validation
        {
            get { return validation; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                validation = value;
            }
        }

        public bool Halted
        {
            get { return halted; }
            set { halted = value; }
        }

        public ActivityExecutionContext ActivityExecutionContext
        {
            get { return this.activityExecutionContext; }
        }

        internal RuleLiteralResult ThisLiteralResult
        {
            get { return this.thisLiteralResult; }
        }
    }
    #endregion

    #region RuleState internal class
    internal class RuleState : IComparable
    {
        internal Rule Rule;
        private ICollection<int> thenActionsActiveRules;
        private ICollection<int> elseActionsActiveRules;

        internal RuleState(Rule rule)
        {
            this.Rule = rule;
        }

        internal ICollection<int> ThenActionsActiveRules
        {
            get { return thenActionsActiveRules; }
            set { thenActionsActiveRules = value; }
        }

        internal ICollection<int> ElseActionsActiveRules
        {
            get { return elseActionsActiveRules; }
            set { elseActionsActiveRules = value; }
        }

        int IComparable.CompareTo(object obj)
        {
            RuleState other = obj as RuleState;
            int compare = other.Rule.Priority.CompareTo(Rule.Priority);
            if (compare == 0)
                // if the priorities are the same, compare names (in ascending order)
                compare = -other.Rule.Name.CompareTo(Rule.Name);
            return compare;
        }
    }
    #endregion

    #region Tracking Argument

    /// <summary>
    /// Contains the name and condition result of a rule that has caused one or more actions to execute.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class RuleActionTrackingEvent
    {
        private string ruleName;
        private bool conditionResult;

        internal RuleActionTrackingEvent(string ruleName, bool conditionResult)
        {
            this.ruleName = ruleName;
            this.conditionResult = conditionResult;
        }

        /// <summary>
        /// The name of the rule that has caused one or more actions to execute.
        /// </summary>
        public string RuleName
        {
            get { return ruleName; }
        }

        /// <summary>
        /// The rule's condition result: false means the "else" actions are executed; true means the "then" actions are executed.
        /// </summary>
        public bool ConditionResult
        {
            get { return conditionResult; }
        }
    }
    #endregion

    internal class Executor
    {
        #region Rule Set Executor

        internal static IList<RuleState> Preprocess(RuleChainingBehavior behavior, ICollection<Rule> rules, RuleValidation validation, Tracer tracer)
        {
            // start by taking the active rules and make them into a list sorted by priority
            List<RuleState> orderedRules = new List<RuleState>(rules.Count);
            foreach (Rule r in rules)
            {
                if (r.Active)
                    orderedRules.Add(new RuleState(r));
            }
            orderedRules.Sort();

            // Analyze the rules to match side-effects with dependencies.
            // Note that the RuleSet needs to have been validated prior to this.
            AnalyzeRules(behavior, orderedRules, validation, tracer);

            // return the sorted list of rules
            return orderedRules;
        }

        internal static void ExecuteRuleSet(IList<RuleState> orderedRules, RuleExecution ruleExecution, Tracer tracer, string trackingKey)
        {
            // keep track of rule execution
            long[] executionCount = new long[orderedRules.Count];
            bool[] satisfied = new bool[orderedRules.Count];
            // clear the halted flag
            ruleExecution.Halted = false;

            ActivityExecutionContext activityExecutionContext = ruleExecution.ActivityExecutionContext;

            // loop until we hit the end of the list
            int current = 0;
            while (current < orderedRules.Count)
            {
                RuleState currentRuleState = orderedRules[current];

                // does this rule need to be evaluated?
                if (!satisfied[current])
                {
                    // yes, so evaluate it and determine the list of actions needed
                    if (tracer != null)
                        tracer.StartRule(currentRuleState.Rule.Name);
                    satisfied[current] = true;
                    bool result = currentRuleState.Rule.Condition.Evaluate(ruleExecution);
                    if (tracer != null)
                        tracer.RuleResult(currentRuleState.Rule.Name, result);
                    if (activityExecutionContext != null && currentRuleState.Rule.Name != null)
                        activityExecutionContext.TrackData(trackingKey, new RuleActionTrackingEvent(currentRuleState.Rule.Name, result));

                    ICollection<RuleAction> actions = (result) ?
                        currentRuleState.Rule.thenActions :
                        currentRuleState.Rule.elseActions;
                    ICollection<int> activeRules = result ?
                        currentRuleState.ThenActionsActiveRules :
                        currentRuleState.ElseActionsActiveRules;

                    // are there any actions to be performed?
                    if ((actions != null) && (actions.Count > 0))
                    {
                        ++executionCount[current];
                        string ruleName = currentRuleState.Rule.Name;
                        if (tracer != null)
                            tracer.StartActions(ruleName, result);

                        // evaluate the actions
                        foreach (RuleAction action in actions)
                        {
                            action.Execute(ruleExecution);

                            // was Halt executed?
                            if (ruleExecution.Halted)
                                break;
                        }

                        // was Halt executed?
                        if (ruleExecution.Halted)
                            break;

                        // any fields updated?
                        if (activeRules != null)
                        {
                            foreach (int updatedRuleIndex in activeRules)
                            {
                                RuleState rs = orderedRules[updatedRuleIndex];
                                if (satisfied[updatedRuleIndex])
                                {
                                    // evaluate at least once, or repeatedly if appropriate
                                    if ((executionCount[updatedRuleIndex] == 0) || (rs.Rule.ReevaluationBehavior == RuleReevaluationBehavior.Always))
                                    {
                                        if (tracer != null)
                                            tracer.TraceUpdate(ruleName, rs.Rule.Name);
                                        satisfied[updatedRuleIndex] = false;
                                        if (updatedRuleIndex < current)
                                            current = updatedRuleIndex;
                                    }
                                }
                            }
                        }
                        continue;

                    }
                }
                ++current;
            }
            // no more rules to execute, so we are done
        }

        class RuleSymbolInfo
        {
            internal ICollection<string> conditionDependencies;
            internal ICollection<string> thenSideEffects;
            internal ICollection<string> elseSideEffects;
        }


        private static void AnalyzeRules(RuleChainingBehavior behavior, List<RuleState> ruleStates, RuleValidation validation, Tracer tracer)
        {
            int i;
            int numRules = ruleStates.Count;

            // if no chaining is required, then nothing to do
            if (behavior == RuleChainingBehavior.None)
                return;

            // Analyze all the rules and collect all the dependencies & side-effects
            RuleSymbolInfo[] ruleSymbols = new RuleSymbolInfo[numRules];
            for (i = 0; i < numRules; ++i)
                ruleSymbols[i] = AnalyzeRule(behavior, ruleStates[i].Rule, validation, tracer);

            for (i = 0; i < numRules; ++i)
            {
                RuleState currentRuleState = ruleStates[i];

                if (ruleSymbols[i].thenSideEffects != null)
                {
                    currentRuleState.ThenActionsActiveRules = AnalyzeSideEffects(ruleSymbols[i].thenSideEffects, ruleSymbols);

                    if ((currentRuleState.ThenActionsActiveRules != null) && (tracer != null))
                        tracer.TraceThenTriggers(currentRuleState.Rule.Name, currentRuleState.ThenActionsActiveRules, ruleStates);
                }

                if (ruleSymbols[i].elseSideEffects != null)
                {
                    currentRuleState.ElseActionsActiveRules = AnalyzeSideEffects(ruleSymbols[i].elseSideEffects, ruleSymbols);

                    if ((currentRuleState.ElseActionsActiveRules != null) && (tracer != null))
                        tracer.TraceElseTriggers(currentRuleState.Rule.Name, currentRuleState.ElseActionsActiveRules, ruleStates);
                }
            }
        }

        private static ICollection<int> AnalyzeSideEffects(ICollection<string> sideEffects, RuleSymbolInfo[] ruleSymbols)
        {
            Dictionary<int, object> affectedRules = new Dictionary<int, object>();

            for (int i = 0; i < ruleSymbols.Length; ++i)
            {
                ICollection<string> dependencies = ruleSymbols[i].conditionDependencies;
                if (dependencies == null)
                {
                    continue;
                }

                foreach (string sideEffect in sideEffects)
                {
                    bool match = false;

                    if (sideEffect.EndsWith("*", StringComparison.Ordinal))
                    {
                        foreach (string dependency in dependencies)
                        {
                            if (dependency.EndsWith("*", StringComparison.Ordinal))
                            {
                                // Strip the trailing "/*" from the dependency
                                string stripDependency = dependency.Substring(0, dependency.Length - 2);
                                // Strip the trailing "*" from the side-effect
                                string stripSideEffect = sideEffect.Substring(0, sideEffect.Length - 1);

                                string shortString;
                                string longString;

                                if (stripDependency.Length < stripSideEffect.Length)
                                {
                                    shortString = stripDependency;
                                    longString = stripSideEffect;
                                }
                                else
                                {
                                    shortString = stripSideEffect;
                                    longString = stripDependency;
                                }

                                // There's a match if the shorter string is a prefix of the longer string.
                                if (longString.StartsWith(shortString, StringComparison.Ordinal))
                                {
                                    match = true;
                                    break;
                                }
                            }
                            else
                            {
                                string stripSideEffect = sideEffect.Substring(0, sideEffect.Length - 1);
                                string stripDependency = dependency;
                                if (stripDependency.EndsWith("/", StringComparison.Ordinal))
                                    stripDependency = stripDependency.Substring(0, stripDependency.Length - 1);
                                if (stripDependency.StartsWith(stripSideEffect, StringComparison.Ordinal))
                                {
                                    match = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // The side-effect did not end with a wildcard
                        foreach (string dependency in dependencies)
                        {
                            if (dependency.EndsWith("*", StringComparison.Ordinal))
                            {
                                // Strip the trailing "/*"
                                string stripDependency = dependency.Substring(0, dependency.Length - 2);

                                string shortString;
                                string longString;

                                if (stripDependency.Length < sideEffect.Length)
                                {
                                    shortString = stripDependency;
                                    longString = sideEffect;
                                }
                                else
                                {
                                    shortString = sideEffect;
                                    longString = stripDependency;
                                }

                                // There's a match if the shorter string is a prefix of the longer string.
                                if (longString.StartsWith(shortString, StringComparison.Ordinal))
                                {
                                    match = true;
                                    break;
                                }
                            }
                            else
                            {
                                // The side-effect must be a prefix of the dependency (or an exact match).
                                if (dependency.StartsWith(sideEffect, StringComparison.Ordinal))
                                {
                                    match = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (match)
                    {
                        affectedRules[i] = null;
                        break;
                    }
                }
            }

            return affectedRules.Keys;
        }

        private static RuleSymbolInfo AnalyzeRule(RuleChainingBehavior behavior, Rule rule, RuleValidation validator, Tracer tracer)
        {
            RuleSymbolInfo rsi = new RuleSymbolInfo();

            if (rule.Condition != null)
            {
                rsi.conditionDependencies = rule.Condition.GetDependencies(validator);

                if ((rsi.conditionDependencies != null) && (tracer != null))
                    tracer.TraceConditionSymbols(rule.Name, rsi.conditionDependencies);
            }

            if (rule.thenActions != null)
            {
                rsi.thenSideEffects = GetActionSideEffects(behavior, rule.thenActions, validator);

                if ((rsi.thenSideEffects != null) && (tracer != null))
                    tracer.TraceThenSymbols(rule.Name, rsi.thenSideEffects);
            }

            if (rule.elseActions != null)
            {
                rsi.elseSideEffects = GetActionSideEffects(behavior, rule.elseActions, validator);

                if ((rsi.elseSideEffects != null) && (tracer != null))
                    tracer.TraceElseSymbols(rule.Name, rsi.elseSideEffects);
            }

            return rsi;
        }

        private static ICollection<string> GetActionSideEffects(RuleChainingBehavior behavior, IList<RuleAction> actions, RuleValidation validation)
        {
            // Man, I wish there were a Set<T> class...
            Dictionary<string, object> symbols = new Dictionary<string, object>();

            foreach (RuleAction action in actions)
            {
                if ((behavior == RuleChainingBehavior.Full) ||
                    ((behavior == RuleChainingBehavior.UpdateOnly) && (action is RuleUpdateAction)))
                {
                    ICollection<string> sideEffects = action.GetSideEffects(validation);
                    if (sideEffects != null)
                    {
                        foreach (string symbol in sideEffects)
                            symbols[symbol] = null;
                    }
                }
            }

            return symbols.Keys;
        }

        #endregion

        #region Condition Executors
        internal static bool EvaluateBool(CodeExpression expression, RuleExecution context)
        {
            object result = RuleExpressionWalker.Evaluate(context, expression).Value;
            if (result is bool)
                return (bool)result;

            Type expectedType = context.Validation.ExpressionInfo(expression).ExpressionType;
            if (expectedType == null)
            {
                // oops ... not a boolean, so error
                InvalidOperationException exception = new InvalidOperationException(Messages.ConditionMustBeBoolean);
                exception.Data[RuleUserDataKeys.ErrorObject] = expression;
                throw exception;
            }

            return (bool)AdjustType(expectedType, result, typeof(bool));
        }

        internal static object AdjustType(Type operandType, object operandValue, Type toType)
        {
            // if no conversion required, we are done
            if (operandType == toType)
                return operandValue;

            object converted;
            if (AdjustValueStandard(operandType, operandValue, toType, out converted))
                return converted;

            // not a standard conversion, see if it's an implicit user defined conversions
            ValidationError error;
            MethodInfo conversion = RuleValidation.FindImplicitConversion(operandType, toType, out error);
            if (conversion == null)
            {
                if (error != null)
                    throw new RuleEvaluationException(error.ErrorText);

                throw new RuleEvaluationException(
                    string.Format(CultureInfo.CurrentCulture,
                        Messages.CastIncompatibleTypes,
                        RuleDecompiler.DecompileType(operandType),
                        RuleDecompiler.DecompileType(toType)));
            }

            // now we have a method, need to do the conversion S -> Sx -> Tx -> T
            Type sx = conversion.GetParameters()[0].ParameterType;
            Type tx = conversion.ReturnType;

            object intermediateResult1;
            if (AdjustValueStandard(operandType, operandValue, sx, out intermediateResult1))
            {
                // we are happy with the first conversion, so call the user's static method
                object intermediateResult2 = conversion.Invoke(null, new object[] { intermediateResult1 });
                object intermediateResult3;
                if (AdjustValueStandard(tx, intermediateResult2, toType, out intermediateResult3))
                    return intermediateResult3;
            }
            throw new RuleEvaluationException(
                string.Format(CultureInfo.CurrentCulture,
                    Messages.CastIncompatibleTypes,
                    RuleDecompiler.DecompileType(operandType),
                    RuleDecompiler.DecompileType(toType)));
        }

        internal static object AdjustTypeWithCast(Type operandType, object operandValue, Type toType)
        {
            // if no conversion required, we are done
            if (operandType == toType)
                return operandValue;

            object converted;
            if (AdjustValueStandard(operandType, operandValue, toType, out converted))
                return converted;

            // handle enumerations (done above?)

            // now it's time for implicit and explicit user defined conversions
            ValidationError error;
            MethodInfo conversion = RuleValidation.FindExplicitConversion(operandType, toType, out error);
            if (conversion == null)
            {
                if (error != null)
                    throw new RuleEvaluationException(error.ErrorText);

                throw new RuleEvaluationException(
                    string.Format(CultureInfo.CurrentCulture,
                        Messages.CastIncompatibleTypes,
                        RuleDecompiler.DecompileType(operandType),
                        RuleDecompiler.DecompileType(toType)));
            }

            // now we have a method, need to do the conversion S -> Sx -> Tx -> T
            Type sx = conversion.GetParameters()[0].ParameterType;
            Type tx = conversion.ReturnType;

            object intermediateResult1;
            if (AdjustValueStandard(operandType, operandValue, sx, out intermediateResult1))
            {
                // we are happy with the first conversion, so call the user's static method
                object intermediateResult2 = conversion.Invoke(null, new object[] { intermediateResult1 });
                object intermediateResult3;
                if (AdjustValueStandard(tx, intermediateResult2, toType, out intermediateResult3))
                    return intermediateResult3;
            }
            throw new RuleEvaluationException(
                string.Format(CultureInfo.CurrentCulture,
                    Messages.CastIncompatibleTypes,
                    RuleDecompiler.DecompileType(operandType),
                    RuleDecompiler.DecompileType(toType)));
        }


        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static bool AdjustValueStandard(Type operandType, object operandValue, Type toType, out object converted)
        {
            // assume it's the same for now
            converted = operandValue;

            // check for null
            if (operandValue == null)
            {
                // are we converting to a value type?
                if (toType.IsValueType)
                {
                    // is the conversion to nullable?
                    if (!ConditionHelper.IsNullableValueType(toType))
                    {
                        // value type and null, so no conversion possible
                        string message = string.Format(CultureInfo.CurrentCulture, Messages.CannotCastNullToValueType, RuleDecompiler.DecompileType(toType));
                        throw new InvalidCastException(message);
                    }

                    // here we have a Nullable<T>
                    // however, we may need to call the implicit conversion operator if the types are not compatible
                    converted = Activator.CreateInstance(toType);
                    ValidationError error;
                    return RuleValidation.StandardImplicitConversion(operandType, toType, null, out error);
                }

                // not a value type, so null is valid
                return true;
            }

            // check simple cases
            Type currentType = operandValue.GetType();
            if (currentType == toType)
                return true;

            // now the fun begins
            // this should handle most class conversions
            if (toType.IsAssignableFrom(currentType))
                return true;

            // handle the numerics (both implicit and explicit), along with nullable
            // note that if the value was null, it's already handled, so value cannot be nullable
            if ((currentType.IsValueType) && (toType.IsValueType))
            {
                if (currentType.IsEnum)
                {
                    // strip off the enum representation
                    currentType = Enum.GetUnderlyingType(currentType);
                    ArithmeticLiteral literal = ArithmeticLiteral.MakeLiteral(currentType, operandValue);
                    operandValue = literal.Value;
                }

                bool resultNullable = ConditionHelper.IsNullableValueType(toType);
                Type resultType = (resultNullable) ? Nullable.GetUnderlyingType(toType) : toType;

                if (resultType.IsEnum)
                {
                    // Enum.ToObject may throw if currentType is not type SByte, 
                    // Int16, Int32, Int64, Byte, UInt16, UInt32, or UInt64.
                    // So we adjust currentValue to the underlying type (which may throw if out of range)
                    Type underlyingType = Enum.GetUnderlyingType(resultType);
                    object adjusted;
                    if (AdjustValueStandard(currentType, operandValue, underlyingType, out adjusted))
                    {
                        converted = Enum.ToObject(resultType, adjusted);
                        if (resultNullable)
                            converted = Activator.CreateInstance(toType, converted);
                        return true;
                    }
                }
                else if ((resultType.IsPrimitive) || (resultType == typeof(decimal)))
                {
                    // resultType must be a primitive to continue (not a struct)
                    // (enums and generics handled above)
                    if (currentType == typeof(char))
                    {
                        char c = (char)operandValue;
                        if (resultType == typeof(float))
                        {
                            converted = (float)c;
                        }
                        else if (resultType == typeof(double))
                        {
                            converted = (double)c;
                        }
                        else if (resultType == typeof(decimal))
                        {
                            converted = (decimal)c;
                        }
                        else
                        {
                            converted = ((IConvertible)c).ToType(resultType, CultureInfo.CurrentCulture);
                        }
                        if (resultNullable)
                            converted = Activator.CreateInstance(toType, converted);
                        return true;
                    }
                    else if (currentType == typeof(float))
                    {
                        float f = (float)operandValue;
                        if (resultType == typeof(char))
                        {
                            converted = (char)f;
                        }
                        else
                        {
                            converted = ((IConvertible)f).ToType(resultType, CultureInfo.CurrentCulture);
                        }
                        if (resultNullable)
                            converted = Activator.CreateInstance(toType, converted);
                        return true;
                    }
                    else if (currentType == typeof(double))
                    {
                        double d = (double)operandValue;
                        if (resultType == typeof(char))
                        {
                            converted = (char)d;
                        }
                        else
                        {
                            converted = ((IConvertible)d).ToType(resultType, CultureInfo.CurrentCulture);
                        }
                        if (resultNullable)
                            converted = Activator.CreateInstance(toType, converted);
                        return true;
                    }
                    else if (currentType == typeof(decimal))
                    {
                        decimal d = (decimal)operandValue;
                        if (resultType == typeof(char))
                        {
                            converted = (char)d;
                        }
                        else
                        {
                            converted = ((IConvertible)d).ToType(resultType, CultureInfo.CurrentCulture);
                        }
                        if (resultNullable)
                            converted = Activator.CreateInstance(toType, converted);
                        return true;
                    }
                    else
                    {
                        IConvertible convert = operandValue as IConvertible;
                        if (convert != null)
                        {
                            try
                            {
                                converted = convert.ToType(resultType, CultureInfo.CurrentCulture);
                                if (resultNullable)
                                    converted = Activator.CreateInstance(toType, converted);
                                return true;
                            }
                            catch (InvalidCastException)
                            {
                                // not IConvertable, so can't do it
                                return false;
                            }
                        }
                    }
                }
            }

            // no luck with standard conversions, so no conversion done
            return false;
        }
        #endregion
    }
}
