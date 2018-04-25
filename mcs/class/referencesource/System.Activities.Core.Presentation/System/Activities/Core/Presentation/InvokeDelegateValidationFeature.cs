//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Expressions;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Text;

    internal class InvokeDelegateValidationFeature : DesignTimeValidationFeature
    {
        private List<Constraint> constraints;
        private EditingContext editingContext;

        protected override Type ApplyTo
        {
            get { return typeof(InvokeDelegate); }
        }

        protected override IList<Constraint> DesignTimeConstraints
        {
            get
            {
                if (this.constraints == null)
                {
                    this.constraints = new List<Constraint> { this.CreateCheckDelegateRule(this.editingContext) };
                }

                return this.constraints;
            }
        }

        public override void Initialize(EditingContext context, Type modelType)
        {
            this.editingContext = context;
            base.Initialize(context, modelType);
        }

        private Constraint CreateCheckDelegateRule(EditingContext editingContext)
        {
            DelegateInArgument<InvokeDelegate> invokeDelegate = new DelegateInArgument<InvokeDelegate>();
            DelegateInArgument<ValidationContext> context = new DelegateInArgument<ValidationContext>();

            return new Constraint<InvokeDelegate>
            {
                Body = new ActivityAction<InvokeDelegate, ValidationContext>
                {
                    Argument1 = invokeDelegate,
                    Argument2 = context,
                    Handler = new CheckDelegateRule
                    {
                        Activity = invokeDelegate,
                        EditingContext = editingContext,
                    }
                }
            };
        }

        private class CheckDelegateRule : NativeActivity
        {
            public CheckDelegateRule()
            {
                this.WarningMessageVariable = new Variable<string>();
                this.ErrorMessageVariable = new Variable<string>();
                this.ShowWarning = new AssertValidation()
                {
                    Assertion = false,
                    IsWarning = true,
                    Message = new VariableValue<string> { Variable = this.WarningMessageVariable }
                };
                this.ShowError = new AssertValidation()
                {
                    Assertion = false,
                    IsWarning = false,
                    Message = new VariableValue<string> { Variable = this.ErrorMessageVariable }
                };
            }

            [RequiredArgument]
            public InArgument<InvokeDelegate> Activity { get; set; }

            public EditingContext EditingContext { get; set; }

            private Variable<string> WarningMessageVariable { get; set; }

            private Variable<string> ErrorMessageVariable { get; set; }

            private AssertValidation ShowWarning { get; set; }

            private AssertValidation ShowError { get; set; }

            protected override void CacheMetadata(NativeActivityMetadata metadata)
            {
                RuntimeArgument activityArgument = new RuntimeArgument("Activity", typeof(InvokeDelegate), ArgumentDirection.In, true);
                metadata.Bind(this.Activity, activityArgument);
                metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { activityArgument });

                metadata.AddImplementationChild(this.ShowWarning);
                metadata.AddImplementationChild(this.ShowError);
                metadata.AddImplementationVariable(this.WarningMessageVariable);
                metadata.AddImplementationVariable(this.ErrorMessageVariable);
            }

            protected override void Execute(NativeActivityContext context)
            {
                StringBuilder errorBuilder = new StringBuilder();
                InvokeDelegate activity = this.Activity.Get(context);

                string reference = PropertyReferenceUtilities.GetPropertyReference(activity, "Delegate");

                if (reference != null)
                {
                    DynamicActivityProperty property = null;

                    ModelTreeManager manager = this.EditingContext.Services.GetService<ModelTreeManager>();
                    if (manager.Root.ItemType == typeof(ActivityBuilder))
                    {
                        property = DynamicActivityPropertyUtilities.Find(manager.Root.Properties["Properties"].Collection, reference);
                    }

                    if (property == null)
                    {
                        this.EmitValidationError(context, string.Format(CultureInfo.CurrentUICulture, SR.PropertyReferenceNotResolved, reference));
                        return;
                    }

                    if (property.Type == typeof(ActivityDelegate))
                    {
                        this.EmitValidationWarning(context, string.Format(CultureInfo.CurrentUICulture, SR.PropertyIsNotAConcreteActivityDelegate, reference));
                        return;
                    }

                    if (!property.Type.IsSubclassOf(typeof(ActivityDelegate)))
                    {
                        this.EmitValidationError(context, string.Format(CultureInfo.CurrentUICulture, SR.PropertyIsNotAnActivityDelegate, reference));
                        return;
                    }

                    if (property.Type.IsAbstract)
                    {
                        this.EmitValidationWarning(context, string.Format(CultureInfo.CurrentUICulture, SR.PropertyIsNotAConcreteActivityDelegate, reference));
                        return;
                    }

                    ActivityDelegateMetadata metadata = ActivityDelegateUtilities.GetMetadata(property.Type);

                    if (activity.DelegateArguments.Count != metadata.Count)
                    {
                        this.EmitValidationWarning(context, SR.WrongNumberOfArgumentsForActivityDelegate);
                        return;
                    }

                    foreach (ActivityDelegateArgumentMetadata expectedArgument in metadata)
                    {
                        Argument delegateArgument = null;

                        if (activity.DelegateArguments.TryGetValue(expectedArgument.Name, out delegateArgument))
                        {
                            if ((expectedArgument.Direction == ActivityDelegateArgumentDirection.In && delegateArgument.Direction != ArgumentDirection.In) ||
                                (expectedArgument.Direction == ActivityDelegateArgumentDirection.Out && delegateArgument.Direction != ArgumentDirection.Out))
                            {
                                errorBuilder.AppendFormat(CultureInfo.CurrentUICulture, SR.DelegateArgumentsDirectionalityMismatch, expectedArgument.Name, delegateArgument.Direction, expectedArgument.Direction);
                            }

                            if (delegateArgument.ArgumentType != expectedArgument.Type)
                            {
                                if (expectedArgument.Direction == ActivityDelegateArgumentDirection.In)
                                {
                                    if (!TypeHelper.AreTypesCompatible(delegateArgument.ArgumentType, expectedArgument.Type))
                                    {
                                        errorBuilder.AppendFormat(CultureInfo.CurrentUICulture, SR.DelegateInArgumentTypeMismatch, expectedArgument.Name, expectedArgument.Type, delegateArgument.ArgumentType);
                                    }
                                }
                                else
                                {
                                    if (!TypeHelper.AreTypesCompatible(expectedArgument.Type, delegateArgument.ArgumentType))
                                    {
                                        errorBuilder.AppendFormat(CultureInfo.CurrentUICulture, SR.DelegateOutArgumentTypeMismatch, expectedArgument.Name, expectedArgument.Type, delegateArgument.ArgumentType);
                                    }
                                }
                            }
                        }
                        else
                        {
                            errorBuilder.AppendFormat(CultureInfo.CurrentUICulture, SR.DelegateArgumentMissing, expectedArgument.Name);
                        }
                    }

                    if (errorBuilder.Length > 0)
                    {
                        this.EmitValidationWarning(context, errorBuilder.ToString());
                    }
                }
            }

            private void EmitValidationWarning(NativeActivityContext context, string message)
            {
                this.WarningMessageVariable.Set(context, message);
                context.ScheduleActivity(this.ShowWarning);
            }

            private void EmitValidationError(NativeActivityContext context, string message)
            {
                this.ErrorMessageVariable.Set(context, message);
                context.ScheduleActivity(this.ShowError);
            }
        }
    }
}
