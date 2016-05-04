using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Drawing.Design;
using System.Workflow.Activities.Rules.Design;
using System.ComponentModel.Design.Serialization;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    #region RuleSetReference Class

    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    [ActivityValidator(typeof(RuleSetReferenceValidator))]
    [Editor(typeof(RuleSetNameEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(RuleSetReferenceTypeConverter))]
    [Browsable(true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    sealed public class RuleSetReference : DependencyObject
    {
        #region members and constructors

        private bool _runtimeInitialized;
        private string _name;

        public RuleSetReference()
        {
        }

        public RuleSetReference(string ruleSetName)
        {
            _name = ruleSetName;
        }
        #endregion

        #region public members

        public string RuleSetName
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        #endregion

        #region IInitializeForRuntime Members

        [NonSerialized]
        private object syncLock = new object();

        protected override void InitializeProperties()
        {
            lock (syncLock)
            {
                if (this._runtimeInitialized)
                    return;

                Activity ownerActivity = base.ParentDependencyObject as Activity;

                CompositeActivity declaringActivity = Helpers.GetDeclaringActivity(ownerActivity);
                if (declaringActivity == null)
                {
                    declaringActivity = Helpers.GetRootActivity(ownerActivity) as CompositeActivity;
                }

                RuleDefinitions rules = ConditionHelper.Load_Rules_RT(declaringActivity);
                if (rules != null)
                    rules.OnRuntimeInitialized();
                base.InitializeProperties();
                _runtimeInitialized = true;
            }
        }
        #endregion
    }

    #endregion

    #region RuleSetReferenceValidator Class

    internal sealed class RuleSetReferenceValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            RuleSetReference ruleSetReference = obj as RuleSetReference;
            if (ruleSetReference == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.UnexpectedArgumentType, typeof(RuleSetReference).FullName, "obj");
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
            if (!string.IsNullOrEmpty(ruleSetReference.RuleSetName))
            {
                RuleDefinitions rules = null;
                RuleSetCollection ruleSetCollection = null;

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
                    ruleSetCollection = rules.RuleSets;

                if (ruleSetCollection == null || !ruleSetCollection.Contains(ruleSetReference.RuleSetName))
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.RuleSetNotFound, ruleSetReference.RuleSetName);
                    ValidationError validationError = new ValidationError(message, ErrorNumbers.Error_RuleSetNotFound);
                    validationError.PropertyName = GetFullPropertyName(manager) + "." + "RuleSetName";
                    validationErrors.Add(validationError);
                }
                else
                {
                    RuleSet actualRuleSet = ruleSetCollection[ruleSetReference.RuleSetName];

                    ITypeProvider typeProvider = (ITypeProvider)manager.GetService(typeof(ITypeProvider));

                    IDisposable localContextScope = (WorkflowCompilationContext.Current == null ? WorkflowCompilationContext.CreateScope(manager) : null);
                    try
                    {

                        RuleValidation validation = new RuleValidation(activity, typeProvider, WorkflowCompilationContext.Current.CheckTypes);
                        actualRuleSet.Validate(validation);
                        ValidationErrorCollection actualRuleSetErrors = validation.Errors;

                        if (actualRuleSetErrors.Count > 0)
                        {
                            string expressionPropertyName = GetFullPropertyName(manager);
                            string genericErrorMsg = string.Format(CultureInfo.CurrentCulture, Messages.InvalidRuleSetExpression, expressionPropertyName);
                            int errorNumber = ErrorNumbers.Error_InvalidRuleSetExpression;

                            if (activity.Site != null)
                            {
                                foreach (ValidationError actualError in actualRuleSetErrors)
                                {
                                    ValidationError validationError = new ValidationError(actualError.ErrorText, errorNumber);
                                    validationError.PropertyName = expressionPropertyName + "." + "RuleSet Definition";
                                    validationErrors.Add(validationError);
                                }
                            }
                            else
                            {
                                foreach (ValidationError actualError in actualRuleSetErrors)
                                {
                                    ValidationError validationError = new ValidationError(genericErrorMsg + " " + actualError.ErrorText, errorNumber);
                                    validationError.PropertyName = expressionPropertyName;
                                    validationErrors.Add(validationError);
                                }
                            }
                        }
                        // 











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
                string message = string.Format(CultureInfo.CurrentCulture, Messages.InvalidRuleSetName, "RuleSetReference");
                ValidationError validationError = new ValidationError(message, ErrorNumbers.Error_InvalidRuleSetName);
                validationError.PropertyName = GetFullPropertyName(manager) + "." + "RuleSetName";
                validationErrors.Add(validationError);
            }
            return validationErrors;
        }
    }

    #endregion

}
