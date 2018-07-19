// ---------------------------------------------------------------------------
// Copyright (C) 2005 Microsoft Corporation - All Rights Reserved
// ---------------------------------------------------------------------------

using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    #region RuleCondition base class
    [Serializable]
    public abstract class RuleCondition
    {
        public abstract bool Validate(RuleValidation validation);
        public abstract bool Evaluate(RuleExecution execution);
        public abstract ICollection<string> GetDependencies(RuleValidation validation);

        public abstract string Name { get; set; }
        public virtual void OnRuntimeInitialized() { }

        public abstract RuleCondition Clone();
    }
    #endregion

    #region RuleExpressionCondition Class
    [Serializable]
    public sealed class RuleExpressionCondition : RuleCondition
    {
        #region Properties
        private CodeExpression _expression;
        private string _name;
        private bool _runtimeInitialized;
        [NonSerialized]
        private object _expressionLock = new object();

        public override string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (this._runtimeInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

                this._name = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public CodeExpression Expression
        {
            get
            {
                return _expression;
            }
            set
            {
                if (this._runtimeInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

                lock (this._expressionLock)
                {
                    _expression = value;
                }
            }
        }

        #endregion

        #region Constructors
        public RuleExpressionCondition()
        {
        }

        public RuleExpressionCondition(string conditionName)
        {
            if (null == conditionName)
            {
                throw new ArgumentNullException("conditionName");
            }
            _name = conditionName;
        }

        public RuleExpressionCondition(string conditionName, CodeExpression expression)
            : this(conditionName)
        {
            _expression = expression;
        }

        public RuleExpressionCondition(CodeExpression expression)
        {
            _expression = expression;
        }
        #endregion

        #region Public Methods

        public override void OnRuntimeInitialized()
        {
            if (this._runtimeInitialized)

                return;

            _runtimeInitialized = true;
        }


        public override bool Equals(object obj)
        {
            bool equals = false;
            RuleExpressionCondition declarativeConditionDefinition = obj as RuleExpressionCondition;

            if (declarativeConditionDefinition != null)
            {
                equals = ((this.Name == declarativeConditionDefinition.Name) &&
                          ((this._expression == null && declarativeConditionDefinition.Expression == null) ||
                           (this._expression != null && RuleExpressionWalker.Match(this._expression, declarativeConditionDefinition.Expression))));
            }

            return equals;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (_expression != null)
            {
                StringBuilder decompilation = new StringBuilder();
                RuleExpressionWalker.Decompile(decompilation, _expression, null);
                return decompilation.ToString();
            }
            else
            {
                return "";
            }
        }

        #endregion

        #region RuleExpressionCondition methods

        public override bool Validate(RuleValidation validation)
        {
            if (validation == null)
                throw new ArgumentNullException("validation");

            bool valid = true;

            if (_expression == null)
            {
                valid = false;

                string message = string.Format(CultureInfo.CurrentCulture, Messages.ConditionExpressionNull, typeof(CodePrimitiveExpression).ToString());
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_EmptyExpression);
                error.UserData[RuleUserDataKeys.ErrorObject] = this;
                validation.AddError(error);

            }
            else
            {
                valid = validation.ValidateConditionExpression(_expression);
            }

            return valid;
        }

        public override bool Evaluate(RuleExecution execution)
        {
            if (_expression == null)
                return true;

            return Executor.EvaluateBool(_expression, execution);
        }

        public override ICollection<string> GetDependencies(RuleValidation validation)
        {
            RuleAnalysis analyzer = new RuleAnalysis(validation, false);
            if (_expression != null)
                RuleExpressionWalker.AnalyzeUsage(analyzer, _expression, true, false, null);
            return analyzer.GetSymbols();
        }
        #endregion

        #region Cloning

        public override RuleCondition Clone()
        {
            RuleExpressionCondition ruleCondition = (RuleExpressionCondition)this.MemberwiseClone();
            ruleCondition._runtimeInitialized = false;
            ruleCondition._expression = RuleExpressionWalker.Clone(this._expression);
            return ruleCondition;
        }

        #endregion
    }
    #endregion

    #region RuleConditionReference Class
    [TypeConverter(typeof(Design.RuleConditionReferenceTypeConverter))]
    [ActivityValidator(typeof(RuleConditionReferenceValidator))]
    [SRDisplayName(SR.RuleConditionDisplayName)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class RuleConditionReference : ActivityCondition
    {
        private bool _runtimeInitialized;
        private string _condition;
        private string declaringActivityId = string.Empty;

        public RuleConditionReference()
        {
        }

        public string ConditionName
        {
            get { return this._condition; }
            set { this._condition = value; }
        }

        public override bool Evaluate(Activity activity, IServiceProvider provider)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (string.IsNullOrEmpty(this._condition))
            {
                throw new InvalidOperationException(SR.GetString(SR.Error_MissingConditionName, activity.Name));
            }

            RuleDefinitions defs = null;

            if (string.IsNullOrEmpty(this.declaringActivityId))
            {
                // No Runtime Initialization.
                CompositeActivity declaringActivity = null;
                defs = RuleConditionReference.GetRuleDefinitions(activity, out declaringActivity);
            }
            else
            {
                // Runtime Initialized.
                defs = (RuleDefinitions)activity.GetActivityByName(declaringActivityId).GetValue(RuleDefinitions.RuleDefinitionsProperty);
            }

            if ((defs == null) || (defs.Conditions == null))
            {
                throw new InvalidOperationException(SR.GetString(SR.Error_MissingRuleConditions));
            }

            RuleCondition conditionDefinitionToEvaluate = defs.Conditions[this._condition];
            if (conditionDefinitionToEvaluate != null)
            {
                Activity contextActivity = System.Workflow.Activities.Common.Helpers.GetEnclosingActivity(activity);
                RuleValidation validation = new RuleValidation(contextActivity);
                if (!conditionDefinitionToEvaluate.Validate(validation))
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.ConditionValidationFailed, this._condition);
                    throw new InvalidOperationException(message);
                }

                RuleExecution context = new RuleExecution(validation, contextActivity, provider as ActivityExecutionContext);
                return conditionDefinitionToEvaluate.Evaluate(context);
            }
            else
            {
                // no condition, so defaults to true
                return true;
            }
        }

        //
        //  Method extracted from OnRuntimeInitialized 
        //  used on special Evaluation case too.
        //
        private static RuleDefinitions GetRuleDefinitions(
            Activity activity, out CompositeActivity declaringActivity)
        {
            declaringActivity = Helpers.GetDeclaringActivity(activity);
            if (declaringActivity == null)
            {
                declaringActivity = Helpers.GetRootActivity(activity) as CompositeActivity;
            }
            return ConditionHelper.Load_Rules_RT(declaringActivity);
        }

        #region IInitializeForRuntime Members

        [NonSerialized]
        private object syncLock = new object();

        protected override void InitializeProperties()
        {
            lock (syncLock)
            {
                if (this._runtimeInitialized)
                    return;

                CompositeActivity declaringActivity = null;
                Activity ownerActivity = base.ParentDependencyObject as Activity;

                RuleDefinitions definitions = GetRuleDefinitions(ownerActivity, out declaringActivity);
                definitions.OnRuntimeInitialized();

                this.declaringActivityId = declaringActivity.QualifiedName;
                base.InitializeProperties();
                _runtimeInitialized = true;
            }
        }
        #endregion

    }
    #endregion

    #region RuleConditionReferenceValidator Class
    internal sealed class RuleConditionReferenceValidator : ConditionValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (manager.Context == null)
            {
                throw new InvalidOperationException(Messages.ContextStackMissing);
            }

            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            RuleConditionReference declarativeCondition = obj as RuleConditionReference;
            if (declarativeCondition == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.UnexpectedArgumentType, typeof(RuleConditionReference).FullName, "obj");
                throw new ArgumentException(message, "obj");
            }

            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.ContextStackItemMissing, typeof(Activity).Name);
                throw new InvalidOperationException(message);
            }

            PropertyValidationContext validationContext = manager.Context[typeof(PropertyValidationContext)] as PropertyValidationContext;
            if (validationContext == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.ContextStackItemMissing, typeof(PropertyValidationContext).Name);
                throw new InvalidOperationException(message);
            }
            if (!string.IsNullOrEmpty(declarativeCondition.ConditionName))
            {
                RuleDefinitions rules = null;
                RuleConditionCollection conditionDefinitions = null;

                CompositeActivity declaringActivity = Helpers.GetDeclaringActivity(activity);
                if (declaringActivity == null)
                {
                    declaringActivity = Helpers.GetRootActivity(activity) as CompositeActivity;
                }
                if (activity.Site != null)
                    rules = ConditionHelper.Load_Rules_DT(activity.Site, declaringActivity);
                else
                    rules = ConditionHelper.Load_Rules_RT(declaringActivity);

                if (rules != null)
                    conditionDefinitions = rules.Conditions;

                if (conditionDefinitions == null || !conditionDefinitions.Contains(declarativeCondition.ConditionName))
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.ConditionNotFound, declarativeCondition.ConditionName);
                    ValidationError validationError = new ValidationError(message, ErrorNumbers.Error_ConditionNotFound);
                    validationError.PropertyName = GetFullPropertyName(manager) + "." + "ConditionName";
                    validationErrors.Add(validationError);
                }
                else
                {
                    RuleCondition actualCondition = conditionDefinitions[declarativeCondition.ConditionName];

                    ITypeProvider typeProvider = (ITypeProvider)manager.GetService(typeof(ITypeProvider));

                    IDisposable localContextScope = (WorkflowCompilationContext.Current == null ? WorkflowCompilationContext.CreateScope(manager) : null);
                    try
                    {
                        RuleValidation ruleValidator = new RuleValidation(activity, typeProvider, WorkflowCompilationContext.Current.CheckTypes);
                        actualCondition.Validate(ruleValidator);

                        ValidationErrorCollection actualConditionErrors = ruleValidator.Errors;

                        if (actualConditionErrors.Count > 0)
                        {
                            string expressionPropertyName = GetFullPropertyName(manager);
                            string genericErrorMsg = string.Format(CultureInfo.CurrentCulture, Messages.InvalidConditionExpression, expressionPropertyName);
                            int errorNumber = ErrorNumbers.Error_InvalidConditionExpression;

                            if (activity.Site != null)
                            {
                                ValidationError validationError = new ValidationError(genericErrorMsg, errorNumber);
                                validationError.PropertyName = expressionPropertyName + "." + "Expression";
                                validationErrors.Add(validationError);
                            }
                            else
                            {
                                foreach (ValidationError actualError in actualConditionErrors)
                                {
                                    ValidationError validationError = new ValidationError(genericErrorMsg + " " + actualError.ErrorText, errorNumber);
                                    validationError.PropertyName = expressionPropertyName + "." + "Expression";
                                    validationErrors.Add(validationError);
                                }
                            }
                        }

                        // Test duplicates
                        foreach (RuleCondition definition in conditionDefinitions)
                        {
                            if (definition.Name == declarativeCondition.ConditionName && definition != actualCondition)
                            {
                                string message = string.Format(CultureInfo.CurrentCulture, Messages.DuplicateConditions, declarativeCondition.ConditionName);
                                ValidationError validationError = new ValidationError(message, ErrorNumbers.Error_DuplicateConditions);
                                validationError.PropertyName = GetFullPropertyName(manager) + "." + "ConditionName";
                                validationErrors.Add(validationError);
                            }
                        }
                    }
                    finally
                    {
                        if (localContextScope != null)
                        {
                            localContextScope.Dispose();
                        }
                    }
                }
            }
            else
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.InvalidConditionName, "ConditionName");
                ValidationError validationError = new ValidationError(message, ErrorNumbers.Error_InvalidConditionName);
                validationError.PropertyName = GetFullPropertyName(manager) + "." + "ConditionName";
                validationErrors.Add(validationError);
            }
            return validationErrors;
        }
    }
    #endregion
}
