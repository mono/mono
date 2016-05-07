//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System.Activities.DynamicUpdate;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Windows.Markup;
    using System.Linq;
    using System.Activities.Expressions;
    using SA = System.Activities;

    [ContentProperty("Body")]
    public sealed class CompensableActivity : NativeActivity<CompensationToken>
    {
        static Constraint noCompensableActivityInSecondaryRoot = CompensableActivity.NoCompensableActivityInSecondaryRoot();

        Collection<Variable> variables;

        CompensationParticipant compensationParticipant;

        Variable<long> currentCompensationId;
        Variable<CompensationToken> currentCompensationToken;

        // This id will be passed to secondary root. 
        Variable<long> compensationId;

        public CompensableActivity()
            : base()
        {
            this.currentCompensationToken = new Variable<CompensationToken>();
            this.currentCompensationId = new Variable<long>();
            this.compensationId = new Variable<long>();
        }

        public Collection<Variable> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    this.variables = new ValidatingCollection<Variable>
                    {
                        // disallow null values
                        OnAddValidationCallback = item =>
                        {
                            if (item == null)
                            {
                                throw SA.FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                }
                return this.variables;
            }
        }

        [DefaultValue(null)]
        [DependsOn("Variables")]
        public Activity Body
        {
            get;
            set;
        }

        [DefaultValue(null)]
        [DependsOn("Body")]
        public Activity CancellationHandler
        {
            get;
            set;
        }

        [DefaultValue(null)]
        [DependsOn("CancellationHandler")]
        public Activity CompensationHandler
        {
            get;
            set;
        }

        [DefaultValue(null)]
        [DependsOn("CompensationHandler")]
        public Activity ConfirmationHandler
        {
            get;
            set;
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        // Internal properties. 
        CompensationParticipant CompensationParticipant
        {
            get
            {
                if (this.compensationParticipant == null)
                {
                    this.compensationParticipant = new CompensationParticipant(this.compensationId);

                    if (CompensationHandler != null)
                    {
                        this.compensationParticipant.CompensationHandler = CompensationHandler;
                    }

                    if (ConfirmationHandler != null)
                    {
                        this.compensationParticipant.ConfirmationHandler = ConfirmationHandler;
                    }

                    if (CancellationHandler != null)
                    {
                        this.compensationParticipant.CancellationHandler = CancellationHandler;
                    }
                }

                return this.compensationParticipant;
            }

            set
            {
                this.compensationParticipant = value;
            }
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetVariablesCollection(this.Variables);

            metadata.SetImplementationVariablesCollection(
                new Collection<Variable>
                {
                    this.currentCompensationId,
                    this.currentCompensationToken,

                    // Add the variables which are only used by the secondary root
                    this.compensationId
                });

            if (this.Body != null)
            {
                metadata.SetChildrenCollection(new Collection<Activity> { this.Body });
            }

            // Declare the handlers as public children.
            if (this.CompensationHandler != null)
            {
                metadata.AddImportedChild(this.CompensationHandler);
            }

            if (this.ConfirmationHandler != null)
            {
                metadata.AddImportedChild(this.ConfirmationHandler);
            }

            if (this.CancellationHandler != null)
            {
                metadata.AddImportedChild(this.CancellationHandler);
            }

            Collection<Activity> implementationChildren = new Collection<Activity>();

            if (!this.IsSingletonActivityDeclared(CompensationActivityStrings.WorkflowImplicitCompensationBehavior))
            {
                WorkflowCompensationBehavior workflowCompensationBehavior = new WorkflowCompensationBehavior();
                this.DeclareSingletonActivity(CompensationActivityStrings.WorkflowImplicitCompensationBehavior, workflowCompensationBehavior);
                implementationChildren.Add(workflowCompensationBehavior);

                metadata.AddDefaultExtensionProvider(CreateCompensationExtension);
            }

            // Clear the cached handler values as workflow definition could be updated.
            CompensationParticipant = null;
            implementationChildren.Add(CompensationParticipant);

            metadata.SetImplementationChildrenCollection(implementationChildren);
        }

        CompensationExtension CreateCompensationExtension()
        {
            return new CompensationExtension();
        }

        internal override IList<Constraint> InternalGetConstraints()
        {
            return new List<Constraint>(1) { noCompensableActivityInSecondaryRoot };
        }

        static Constraint NoCompensableActivityInSecondaryRoot()
        {
            DelegateInArgument<ValidationContext> validationContext = new DelegateInArgument<ValidationContext> { Name = "validationContext" };
            DelegateInArgument<CompensableActivity> element = new DelegateInArgument<CompensableActivity> { Name = "element" };
            Variable<bool> assertFlag = new Variable<bool> { Name = "assertFlag", Default = true };
            Variable<IEnumerable<Activity>> elements = new Variable<IEnumerable<Activity>>() { Name = "elements" };
            Variable<int> index = new Variable<int>() { Name = "index" };

            return new Constraint<CompensableActivity>
            {
                Body = new ActivityAction<CompensableActivity, ValidationContext>
                {
                    Argument1 = element,
                    Argument2 = validationContext,
                    Handler = new Sequence
                    {
                        Variables = 
                        {
                            assertFlag,
                            elements,
                            index
                        },
                        Activities = 
                        {
                            new Assign<IEnumerable<Activity>>
                            {
                                To = elements,
                                Value = new GetParentChain
                                {
                                    ValidationContext = validationContext,
                                },
                            },
                            // Need to replace the lambda expression with a CodeActivity for partial trust.
                            // new While(env => (assertFlag.Get(env) != false) && index.Get(env) < elements.Get(env).Count())
                            new While
                            {
                                Condition = new WhileExpression
                                {
                                    DisplayName = "env => (assertFlag.Get(env) != false) && index.Get(env) < elements.Get(env).Count())",
                                    AssertFlag = new InArgument<bool>(assertFlag),
                                    Index = new InArgument<int>(index),
                                    Elements = new InArgument<IEnumerable<Activity>>(elements)
                                },

                                Body = new Sequence
                                {
                                    Activities = 
                                    {
                                        // Need to replace the lambda expression with a CodeActivity for partial trust.
                                        // new If(env => (elements.Get(env).ElementAt(index.Get(env))).GetType() == typeof(CompensationParticipant))
                                        new If
                                        {
                                            Condition = new IfExpression
                                            {
                                                DisplayName = "env => (elements.Get(env).ElementAt(index.Get(env))).GetType() == typeof(CompensationParticipant)",
                                                Elements = new InArgument<IEnumerable<Activity>>(elements),
                                                Index = new InArgument<int>(index)
                                            },

                                            Then = new Assign<bool>
                                            {
                                                To = assertFlag,
                                                Value = false                                                            
                                            },
                                        },
                                        new Assign<int>
                                        {
                                            To = index,
                                            // Need to replace the lambda expression for partial trust. Using Add expression activity instead of a CodeActivity here.
                                            // Value = new InArgument<int>(env => index.Get(env) + 1)
                                            Value = new InArgument<int>
                                            {
                                                Expression = new Add<int, int, int>
                                                {
                                                    DisplayName = "(env => index.Get(env) + 1)",
                                                    Left = new VariableValue<int>
                                                    {
                                                        Variable = index
                                                    },
                                                    Right = 1,
                                                }
                                            }
                                        },
                                    }
                                }
                            },                            
                            new AssertValidation
                            {
                                Assertion = new InArgument<bool>(assertFlag),
                                Message = new InArgument<string>(SA.SR.NoCAInSecondaryRoot)   
                            }
                        }
                    }
                }
            };
        }

        protected override void Execute(NativeActivityContext context)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            if (compensationExtension.IsWorkflowCompensationBehaviorScheduled)
            {
                ScheduleBody(context, compensationExtension);
            }
            else
            {
                compensationExtension.SetupWorkflowCompensationBehavior(context, new BookmarkCallback(OnWorkflowCompensationBehaviorScheduled), GetSingletonActivity(CompensationActivityStrings.WorkflowImplicitCompensationBehavior));
            }
        }

        protected override void Cancel(NativeActivityContext context)
        {
            context.CancelChildren();
        }

        void OnWorkflowCompensationBehaviorScheduled(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            ScheduleBody(context, compensationExtension);
        }

        void ScheduleBody(NativeActivityContext context, CompensationExtension compensationExtension)
        {
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            CompensationToken parentToken = null;
            long parentCompensationId = CompensationToken.RootCompensationId;

            parentToken = (CompensationToken)context.Properties.Find(CompensationToken.PropertyName);

            if (parentToken != null)
            {
                if (compensationExtension.Get(parentToken.CompensationId).IsTokenValidInSecondaryRoot)
                {
                    throw SA.FxTrace.Exception.AsError(new InvalidOperationException(SA.SR.NoCAInSecondaryRoot));
                }

                parentCompensationId = parentToken.CompensationId;
            }

            CompensationTokenData tokenData = new CompensationTokenData(compensationExtension.GetNextId(), parentCompensationId)
                {
                    CompensationState = CompensationState.Active,
                    DisplayName = this.DisplayName,
                };
            CompensationToken token = new CompensationToken(tokenData);

            context.Properties.Add(CompensationToken.PropertyName, token);

            this.currentCompensationId.Set(context, token.CompensationId);
            this.currentCompensationToken.Set(context, token);

            compensationExtension.Add(token.CompensationId, tokenData);

            if (TD.CompensationStateIsEnabled())
            {
                TD.CompensationState(tokenData.DisplayName, tokenData.CompensationState.ToString());
            }

            if (this.Body != null)
            {
                context.ScheduleActivity(this.Body, new CompletionCallback(OnBodyExecutionComplete));
            }
            else
            {
                //empty body case. Assume the body has completed successfully
                tokenData.CompensationState = CompensationState.Completed;
                if (TD.CompensationStateIsEnabled())
                {
                    TD.CompensationState(tokenData.DisplayName, tokenData.CompensationState.ToString());
                }

                ScheduleSecondaryRoot(context, compensationExtension, tokenData);
            }
        }

        void OnBodyExecutionComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            CompensationTokenData token = compensationExtension.Get(this.currentCompensationId.Get(context));
            Fx.Assert(token != null, "CompensationTokenData must be valid");

            if (completedInstance.State == ActivityInstanceState.Closed)
            {
                token.CompensationState = CompensationState.Completed;
                if (TD.CompensationStateIsEnabled())
                {
                    TD.CompensationState(token.DisplayName, token.CompensationState.ToString());
                }

                if (context.IsCancellationRequested)
                {
                    token.CompensationState = CompensationState.Compensating;
                }
            }
            else if (completedInstance.State == ActivityInstanceState.Canceled || completedInstance.State == ActivityInstanceState.Faulted)
            {
                // we check for faulted as well for one odd case where an exception can be thrown from the body activity itself. 
                token.CompensationState = CompensationState.Canceling;
            }
            else
            {
                Fx.Assert(false, "completedInstance in unexpected state");
            }

            ScheduleSecondaryRoot(context, compensationExtension, token);
        }

        void ScheduleSecondaryRoot(NativeActivityContext context, CompensationExtension compensationExtension, CompensationTokenData token)
        {
            if (token.ParentCompensationId != CompensationToken.RootCompensationId)
            {
                CompensationTokenData parentToken = compensationExtension.Get(token.ParentCompensationId);
                Fx.Assert(parentToken != null, "parentToken must be valid");

                parentToken.ExecutionTracker.Add(token);
            }
            else
            {
                CompensationTokenData parentToken = compensationExtension.Get(CompensationToken.RootCompensationId);
                Fx.Assert(parentToken != null, "parentToken must be valid");

                parentToken.ExecutionTracker.Add(token);
            }

            // If we are going to Cancel, don't set the out arg...       
            if (Result != null && token.CompensationState == CompensationState.Completed)
            {
                Result.Set(context, this.currentCompensationToken.Get(context));
            }

            Fx.Assert(token.BookmarkTable[CompensationBookmarkName.OnSecondaryRootScheduled] == null, "Bookmark should not be already initialized in the bookmark table.");
            token.BookmarkTable[CompensationBookmarkName.OnSecondaryRootScheduled] = context.CreateBookmark(new BookmarkCallback(OnSecondaryRootScheduled));

            this.compensationId.Set(context, token.CompensationId);

            context.ScheduleSecondaryRoot(CompensationParticipant, context.Environment);
        }

        void OnSecondaryRootScheduled(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            long compensationId = (long)value;

            CompensationTokenData compensationToken = compensationExtension.Get(compensationId);
            Fx.Assert(compensationToken != null, "CompensationTokenData must be valid");

            if (compensationToken.CompensationState == CompensationState.Canceling)
            {
                Fx.Assert(compensationToken.BookmarkTable[CompensationBookmarkName.Canceled] == null, "Bookmark should not be already initialized in the bookmark table.");
                compensationToken.BookmarkTable[CompensationBookmarkName.Canceled] = context.CreateBookmark(new BookmarkCallback(OnCanceledOrCompensated));

                compensationExtension.NotifyMessage(context, compensationToken.CompensationId, CompensationBookmarkName.OnCancellation);
            }
            else if (compensationToken.CompensationState == CompensationState.Compensating)
            {
                Fx.Assert(compensationToken.BookmarkTable[CompensationBookmarkName.Compensated] == null, "Bookmark should not be already initialized in the bookmark table.");
                compensationToken.BookmarkTable[CompensationBookmarkName.Compensated] = context.CreateBookmark(new BookmarkCallback(OnCanceledOrCompensated));

                compensationExtension.NotifyMessage(context, compensationToken.CompensationId, CompensationBookmarkName.OnCompensation);
            }
        }

        void OnCanceledOrCompensated(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            Fx.Assert(compensationExtension != null, "CompensationExtension must be valid");

            long compensationId = (long)value;

            CompensationTokenData compensationToken = compensationExtension.Get(compensationId);
            Fx.Assert(compensationToken != null, "CompensationTokenData must be valid");

            switch (compensationToken.CompensationState)
            {
                case CompensationState.Canceling:
                    compensationToken.CompensationState = CompensationState.Canceled;
                    break;
                case CompensationState.Compensating:
                    compensationToken.CompensationState = CompensationState.Compensated;
                    break;
                default:
                    break;
            }

            if (TD.CompensationStateIsEnabled())
            {
                TD.CompensationState(compensationToken.DisplayName, compensationToken.CompensationState.ToString());
            }

            AppCompletionCleanup(context, compensationExtension, compensationToken);

            // Mark the activity as canceled. 
            context.MarkCanceled();
        }

        void AppCompletionCleanup(NativeActivityContext context, CompensationExtension compensationExtension, CompensationTokenData compensationToken)
        {
            Fx.Assert(compensationToken != null, "CompensationTokenData must be valid");

            // Remove the token from the parent! 
            if (compensationToken.ParentCompensationId != CompensationToken.RootCompensationId)
            {
                CompensationTokenData parentToken = compensationExtension.Get(compensationToken.ParentCompensationId);
                Fx.Assert(parentToken != null, "parentToken must be valid");

                parentToken.ExecutionTracker.Remove(compensationToken);
            }
            else
            {
                // remove from workflow root...
                CompensationTokenData parentToken = compensationExtension.Get(CompensationToken.RootCompensationId);
                Fx.Assert(parentToken != null, "parentToken must be valid");

                parentToken.ExecutionTracker.Remove(compensationToken);
            }

            compensationToken.RemoveBookmark(context, CompensationBookmarkName.Canceled);
            compensationToken.RemoveBookmark(context, CompensationBookmarkName.Compensated);

            // Remove the token from the extension...
            compensationExtension.Remove(compensationToken.CompensationId);
        }
    }

    // In order to run in partial trust, we can't have lambda expressions that reference local variables. So this
    // code activity replaces the lambda expression in this statement:
    // While(env => (assertFlag.Get(env) != false) && index.Get(env) < elements.Get(env).Count())
    class WhileExpression : CodeActivity<bool>
    {
        public InArgument<bool> AssertFlag
        {
            get;
            set;
        }

        public InArgument<int> Index
        {
            get;
            set;
        }

        public InArgument<IEnumerable<Activity>> Elements
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument assertFlagArgument = new RuntimeArgument("AssertFlag", typeof(bool), ArgumentDirection.In);
            if (this.AssertFlag == null)
            {
                this.AssertFlag = new InArgument<bool>();
            }
            metadata.Bind(this.AssertFlag, assertFlagArgument);

            RuntimeArgument indexArgument = new RuntimeArgument("Index", typeof(int), ArgumentDirection.In);
            if (this.Index == null)
            {
                this.Index = new InArgument<int>();
            }
            metadata.Bind(this.Index, indexArgument);

            RuntimeArgument elementsArgument = new RuntimeArgument("Elements", typeof(IEnumerable<Activity>), ArgumentDirection.In);
            if (this.Elements == null)
            {
                this.Elements = new InArgument<IEnumerable<Activity>>();
            }
            metadata.Bind(this.Elements, elementsArgument);

            RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(bool), ArgumentDirection.Out);
            if (this.Result == null)
            {
                this.Result = new OutArgument<bool>();
            }
            metadata.Bind(this.Result, resultArgument);

            metadata.SetArgumentsCollection(
                new Collection<RuntimeArgument>
                {
                    assertFlagArgument,
                    indexArgument,
                    elementsArgument,
                    resultArgument
                });
        }

        protected override bool Execute(CodeActivityContext context)
        {
            // While(env => (assertFlag.Get(env) != false) && index.Get(env) < elements.Get(env).Count())
            return ((this.AssertFlag.Get(context) != false) && (this.Index.Get(context) < this.Elements.Get(context).Count()));
        }
    }


    // If(env => (elements.Get(env).ElementAt(index.Get(env))).GetType() == typeof(CompensationParticipant))
    class IfExpression : CodeActivity<bool>
    {
        public InArgument<IEnumerable<Activity>> Elements
        {
            get;
            set;
        }

        public InArgument<int> Index
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument elementsArgument = new RuntimeArgument("Elements", typeof(IEnumerable<Activity>), ArgumentDirection.In);
            if (this.Elements == null)
            {
                this.Elements = new InArgument<IEnumerable<Activity>>();
            }
            metadata.Bind(this.Elements, elementsArgument);

            RuntimeArgument indexArgument = new RuntimeArgument("Index", typeof(int), ArgumentDirection.In);
            if (this.Index == null)
            {
                this.Index = new InArgument<int>();
            }
            metadata.Bind(this.Index, indexArgument);

            RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(bool), ArgumentDirection.Out);
            if (this.Result == null)
            {
                this.Result = new OutArgument<bool>();
            }
            metadata.Bind(this.Result, resultArgument);

            metadata.SetArgumentsCollection(
                new Collection<RuntimeArgument>
                {
                    indexArgument,
                    elementsArgument,
                    resultArgument
                });
        }

        protected override bool Execute(CodeActivityContext context)
        {
            // If(env => (elements.Get(env).ElementAt(index.Get(env))).GetType() == typeof(CompensationParticipant))
            return (this.Elements.Get(context).ElementAt(this.Index.Get(context)).GetType() == typeof(CompensationParticipant));
        }
    }
}
