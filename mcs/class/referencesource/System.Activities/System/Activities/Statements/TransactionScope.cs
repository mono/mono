//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Transactions;
    using System.Windows.Markup;
    using System.Activities.Expressions;
    using System.Collections.ObjectModel;

    [ContentProperty("Body")]
    public sealed class TransactionScope : NativeActivity
    {
        const IsolationLevel defaultIsolationLevel = default(IsolationLevel);

        InArgument<TimeSpan> timeout;
        bool isTimeoutSetExplicitly;
        Variable<RuntimeTransactionHandle> runtimeTransactionHandle;
        bool abortInstanceOnTransactionFailure;
        bool abortInstanceFlagWasExplicitlySet;
        Delay nestedScopeTimeoutWorkflow;
        Variable<bool> delayWasScheduled;
        Variable<TimeSpan> nestedScopeTimeout;
        Variable<ActivityInstance> nestedScopeTimeoutActivityInstance;
        static string runtimeTransactionHandlePropertyName = typeof(RuntimeTransactionHandle).FullName;
        const string AbortInstanceOnTransactionFailurePropertyName = "AbortInstanceOnTransactionFailure";
        const string IsolationLevelPropertyName = "IsolationLevel";
        const string BodyPropertyName = "Body";

        public TransactionScope()
            : base()
        {
            this.timeout = new InArgument<TimeSpan>(TimeSpan.FromMinutes(1));
            this.runtimeTransactionHandle = new Variable<RuntimeTransactionHandle>();
            this.abortInstanceOnTransactionFailure = true;
            this.nestedScopeTimeout = new Variable<TimeSpan>();
            this.delayWasScheduled = new Variable<bool>();
            this.nestedScopeTimeoutActivityInstance = new Variable<ActivityInstance>();

            base.Constraints.Add(ProcessParentChainConstraints());
            base.Constraints.Add(ProcessChildSubtreeConstraints());
        }

        [DefaultValue(null)]
        public Activity Body
        {
            get;
            set;
        }

        [DefaultValue(true)]
        public bool AbortInstanceOnTransactionFailure
        {
            get
            {
                return this.abortInstanceOnTransactionFailure;
            }
            set
            {
                this.abortInstanceOnTransactionFailure = value;
                this.abortInstanceFlagWasExplicitlySet = true;
            }
        }

        public IsolationLevel IsolationLevel
        {
            get;
            set;
        }

        public InArgument<TimeSpan> Timeout
        {
            get
            {
                return this.timeout;
            }

            set
            {
                this.timeout = value;
                this.isTimeoutSetExplicitly = true;
            }
        }

        Delay NestedScopeTimeoutWorkflow
        {
            get
            {
                if (this.nestedScopeTimeoutWorkflow == null)
                {
                    this.nestedScopeTimeoutWorkflow = new Delay
                    {
                        Duration = new InArgument<TimeSpan>(this.nestedScopeTimeout)
                    };

                }
                return this.nestedScopeTimeoutWorkflow;
            }
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeIsolationLevel()
        {
            return this.IsolationLevel != defaultIsolationLevel;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTimeout()
        {
            return this.isTimeoutSetExplicitly;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument timeoutArgument = new RuntimeArgument("Timeout", typeof(TimeSpan), ArgumentDirection.In, false);
            metadata.Bind(this.Timeout, timeoutArgument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { timeoutArgument });
            metadata.AddImplementationChild(this.NestedScopeTimeoutWorkflow);

            if (this.Body != null)
            {
                metadata.AddChild(this.Body);
            }

            metadata.AddImplementationVariable(this.runtimeTransactionHandle);
            metadata.AddImplementationVariable(this.nestedScopeTimeout);
            metadata.AddImplementationVariable(this.delayWasScheduled);
            metadata.AddImplementationVariable(this.nestedScopeTimeoutActivityInstance);
        }

        Constraint ProcessParentChainConstraints()
        {
            DelegateInArgument<TransactionScope> element = new DelegateInArgument<TransactionScope> { Name = "element" };
            DelegateInArgument<ValidationContext> validationContext = new DelegateInArgument<ValidationContext> { Name = "validationContext" };
            DelegateInArgument<Activity> parent = new DelegateInArgument<Activity> { Name = "parent" };

            return new Constraint<TransactionScope>
            {
                Body = new ActivityAction<TransactionScope, ValidationContext>
                {
                    Argument1 = element,
                    Argument2 = validationContext,
                    Handler = new Sequence
                    {
                        Activities = 
                        {
                            new ForEach<Activity>
                            {
                                Values = new GetParentChain
                                {
                                    ValidationContext = validationContext,
                                },
                                Body = new ActivityAction<Activity>
                                {
                                    Argument = parent,
                                    Handler = new Sequence                                   
                                    {
                                        Activities = 
                                        {
                                            new If()
                                            {
                                                Condition = new Equal<Type, Type, bool>
                                                {
                                                    Left = new ObtainType
                                                    {
                                                        Input = parent,
                                                    },
                                                    Right = new InArgument<Type>(context => typeof(TransactionScope))
                                                },
                                                Then = new Sequence
                                                {
                                                    Activities = 
                                                    {
                                                        new AssertValidation
                                                        {
                                                            IsWarning = true,
                                                            Assertion = new AbortInstanceFlagValidator
                                                            {
                                                                ParentActivity = parent,
                                                                TransactionScope = new InArgument<TransactionScope>(element)
                                                            },
                                                            Message = new InArgument<string>(SR.AbortInstanceOnTransactionFailureDoesNotMatch),
                                                            PropertyName = AbortInstanceOnTransactionFailurePropertyName
                                                        },
                                                        new AssertValidation
                                                        {
                                                            Assertion = new IsolationLevelValidator
                                                            {
                                                                ParentActivity = parent,
                                                                //CurrentIsolationLevel = new InArgument<IsolationLevel>(context => element.Get(context).IsolationLevel)
                                                                CurrentIsolationLevel = new InArgument<IsolationLevel>
                                                                {
                                                                    Expression = new IsolationLevelValue
                                                                    {
                                                                        Scope = element
                                                                    }
                                                                }
                                                            },
                                                            Message = new InArgument<string>(SR.IsolationLevelValidation),
                                                            PropertyName = IsolationLevelPropertyName
                                                        }                                                      

                                                    }
                                                }
                                                
                                            }
                                        }
                                    }                                   
                                }
                            }
                        }
                    }
                }
            };
        }

        Constraint ProcessChildSubtreeConstraints()
        {
            DelegateInArgument<TransactionScope> element = new DelegateInArgument<TransactionScope> { Name = "element" };
            DelegateInArgument<ValidationContext> validationContext = new DelegateInArgument<ValidationContext> { Name = "validationContext" };
            DelegateInArgument<Activity> child = new DelegateInArgument<Activity> { Name = "child" };
            Variable<bool> nestedCompensableActivity = new Variable<bool>();

            return new Constraint<TransactionScope>
            {
                Body = new ActivityAction<TransactionScope, ValidationContext>
                {
                    Argument1 = element,
                    Argument2 = validationContext,
                    Handler = new Sequence
                    {
                        Variables = { nestedCompensableActivity },
                        Activities = 
                        {                            
                            new ForEach<Activity>
                            {
                                Values = new GetChildSubtree
                                {
                                    ValidationContext = validationContext,
                                },
                                Body = new ActivityAction<Activity>
                                {
                                    Argument = child,
                                    Handler = new Sequence                                   
                                    {
                                        Activities = 
                                        {
                                            new If()
                                            {
                                                Condition = new Equal<Type, Type, bool>()
                                                {
                                                     Left = new ObtainType
                                                     {
                                                          Input = new InArgument<Activity>(child)
                                                     },
                                                     Right = new InArgument<Type>(context => typeof(CompensableActivity))
                                                },
                                                Then = new Assign<bool>
                                                {
                                                    To = new OutArgument<bool>(nestedCompensableActivity),
                                                    Value = new InArgument<bool>(true)
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new AssertValidation()
                            {
                                 Assertion = new InArgument<bool>(new Not<bool, bool> { Operand = new VariableValue<bool> { Variable = nestedCompensableActivity } }),
                                 Message = new InArgument<string>(SR.CompensableActivityInsideTransactionScopeActivity),
                                 PropertyName = BodyPropertyName
                            }
                        }
                    }
                }
            };
        }

        protected override void Execute(NativeActivityContext context)
        {
            RuntimeTransactionHandle transactionHandle = this.runtimeTransactionHandle.Get(context);
            Fx.Assert(transactionHandle != null, "RuntimeTransactionHandle is null");

            RuntimeTransactionHandle foundHandle = context.Properties.Find(runtimeTransactionHandlePropertyName) as RuntimeTransactionHandle;
            if (foundHandle == null)
            {
                //Note, once the property is registered, we cannot change the state of this flag
                transactionHandle.AbortInstanceOnTransactionFailure = this.AbortInstanceOnTransactionFailure;
                context.Properties.Add(transactionHandle.ExecutionPropertyName, transactionHandle);
            }
            else
            {
                //nested case
                //foundHandle.IsRuntimeOwnedTransaction will be true only in the Invoke case within an ambient Sys.Tx transaction. 
                //If this TSA is nested inside the ambient transaction from Invoke, then the AbortInstanceFlag is always false since the RTH corresponding to the ambient
                //transaction has this flag as false. In this case, we ignore if this TSA has this flag explicitly set to true. 
                if (!foundHandle.IsRuntimeOwnedTransaction && this.abortInstanceFlagWasExplicitlySet && (foundHandle.AbortInstanceOnTransactionFailure != this.AbortInstanceOnTransactionFailure))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.AbortInstanceOnTransactionFailureDoesNotMatch));
                }

                if (foundHandle.SuppressTransaction)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotNestTransactionScopeWhenAmbientHandleIsSuppressed(this.DisplayName)));
                }
                transactionHandle = foundHandle;
            }

            Transaction transaction = transactionHandle.GetCurrentTransaction(context);
            //Check if there is already a transaction (Requires Semantics)
            if (transaction == null)
            {
                //If not, request one..
                transactionHandle.RequestTransactionContext(context, OnContextAcquired, null);
            }
            else
            {
                //Most likely, you are inside a nested TSA
                if (transaction.IsolationLevel != this.IsolationLevel)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.IsolationLevelValidation));
                }

                //Check if the nested TSA had a timeout specified explicitly
                if (this.isTimeoutSetExplicitly)
                {
                    TimeSpan timeout = this.Timeout.Get(context);
                    this.delayWasScheduled.Set(context, true);
                    this.nestedScopeTimeout.Set(context, timeout);

                    this.nestedScopeTimeoutActivityInstance.Set(context, context.ScheduleActivity(this.NestedScopeTimeoutWorkflow, new CompletionCallback(OnDelayCompletion)));
                }

                //execute the Body under the current runtime transaction
                ScheduleBody(context);
            }
        }

        void OnContextAcquired(NativeActivityTransactionContext context, object state)
        {
            Fx.Assert(context != null, "ActivityTransactionContext was null");

            TimeSpan transactionTimeout = this.Timeout.Get(context);
            TransactionOptions transactionOptions = new TransactionOptions()
            {
                IsolationLevel = this.IsolationLevel,
                Timeout = transactionTimeout
            };

            context.SetRuntimeTransaction(new CommittableTransaction(transactionOptions));

            ScheduleBody(context);
        }

        void ScheduleBody(NativeActivityContext context)
        {
            if (this.Body != null)
            {
                context.ScheduleActivity(this.Body, new CompletionCallback(OnCompletion));
            }
        }

        void OnCompletion(NativeActivityContext context, ActivityInstance instance)
        {
            RuntimeTransactionHandle transactionHandle = this.runtimeTransactionHandle.Get(context);
            Fx.Assert(transactionHandle != null, "RuntimeTransactionHandle is null");

            if (this.delayWasScheduled.Get(context))
            {
                transactionHandle.CompleteTransaction(context, new BookmarkCallback(OnTransactionComplete));
            }
            else
            {
                transactionHandle.CompleteTransaction(context);
            }
        }

        void OnDelayCompletion(NativeActivityContext context, ActivityInstance instance)
        {
            if (instance.State == ActivityInstanceState.Closed)
            {
                RuntimeTransactionHandle handle = context.Properties.Find(runtimeTransactionHandlePropertyName) as RuntimeTransactionHandle;
                Fx.Assert(handle != null, "Internal error.. If we are here, there ought to be an ambient transaction handle");
                handle.GetCurrentTransaction(context).Rollback();
            }
        }

        void OnTransactionComplete(NativeActivityContext context, Bookmark bookmark, object state)
        {
            Fx.Assert(this.delayWasScheduled.Get(context), "Internal error..Delay should have been scheduled if we are here");
            ActivityInstance delayActivityInstance = this.nestedScopeTimeoutActivityInstance.Get(context);
            if (delayActivityInstance != null)
            {
                context.CancelChild(delayActivityInstance);
            }
        }

        //
        class ObtainType : CodeActivity<Type>
        {
            public ObtainType()
            {
            }

            public InArgument<Activity> Input
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument inputArgument = new RuntimeArgument("Input", typeof(Activity), ArgumentDirection.In);
                if (this.Input == null)
                {
                    this.Input = new InArgument<Activity>();
                }
                metadata.Bind(this.Input, inputArgument);

                RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(Type), ArgumentDirection.Out);
                if (this.Result == null)
                {
                    this.Result = new OutArgument<Type>();
                }
                metadata.Bind(this.Result, resultArgument);

                metadata.SetArgumentsCollection(
                    new Collection<RuntimeArgument>
                {
                    inputArgument,
                    resultArgument
                });
            }

            protected override Type Execute(CodeActivityContext context)
            {
                return this.Input.Get(context).GetType();
            }
        }

        class IsolationLevelValue : CodeActivity<IsolationLevel>
        {
            public InArgument<TransactionScope> Scope
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument scopeArgument = new RuntimeArgument("Scope", typeof(TransactionScope), ArgumentDirection.In);
                if (this.Scope == null)
                {
                    this.Scope = new InArgument<TransactionScope>();
                }
                metadata.Bind(this.Scope, scopeArgument);

                RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(IsolationLevel), ArgumentDirection.Out);
                if (this.Result == null)
                {
                    this.Result = new OutArgument<IsolationLevel>();
                }
                metadata.Bind(this.Result, resultArgument);

                metadata.SetArgumentsCollection(
                    new Collection<RuntimeArgument>
                {
                    scopeArgument,
                    resultArgument
                });
            }

            protected override IsolationLevel Execute(CodeActivityContext context)
            {
                return this.Scope.Get(context).IsolationLevel;
            }
        }

        class IsolationLevelValidator : CodeActivity<bool>
        {
            public InArgument<Activity> ParentActivity
            {
                get;
                set;
            }

            public InArgument<IsolationLevel> CurrentIsolationLevel
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument parentActivityArgument = new RuntimeArgument("ParentActivity", typeof(Activity), ArgumentDirection.In);
                if (this.ParentActivity == null)
                {
                    this.ParentActivity = new InArgument<Activity>();
                }
                metadata.Bind(this.ParentActivity, parentActivityArgument);

                RuntimeArgument isoLevelArgument = new RuntimeArgument("CurrentIsolationLevel", typeof(IsolationLevel), ArgumentDirection.In);
                if (this.CurrentIsolationLevel == null)
                {
                    this.CurrentIsolationLevel = new InArgument<IsolationLevel>();
                }
                metadata.Bind(this.CurrentIsolationLevel, isoLevelArgument);

                RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(bool), ArgumentDirection.Out);
                if (this.Result == null)
                {
                    this.Result = new OutArgument<bool>();
                }
                metadata.Bind(this.Result, resultArgument);

                metadata.SetArgumentsCollection(
                    new Collection<RuntimeArgument>
                {
                    parentActivityArgument,
                    isoLevelArgument,
                    resultArgument
                });
            }

            protected override bool Execute(CodeActivityContext context)
            {
                Activity parent = this.ParentActivity.Get(context);

                if (parent != null)
                {
                    TransactionScope transactionScope = parent as TransactionScope;
                    Fx.Assert(transactionScope != null, "ParentActivity was not of expected type");

                    if (transactionScope.IsolationLevel != this.CurrentIsolationLevel.Get(context))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        class AbortInstanceFlagValidator : CodeActivity<bool>
        {
            public InArgument<Activity> ParentActivity
            {
                get;
                set;
            }

            public InArgument<TransactionScope> TransactionScope
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument parentActivityArgument = new RuntimeArgument("ParentActivity", typeof(Activity), ArgumentDirection.In);
                if (this.ParentActivity == null)
                {
                    this.ParentActivity = new InArgument<Activity>();
                }
                metadata.Bind(this.ParentActivity, parentActivityArgument);

                RuntimeArgument txScopeArgument = new RuntimeArgument("TransactionScope", typeof(TransactionScope), ArgumentDirection.In);
                if (this.TransactionScope == null)
                {
                    this.TransactionScope = new InArgument<TransactionScope>();
                }
                metadata.Bind(this.TransactionScope, txScopeArgument);

                RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(bool), ArgumentDirection.Out);
                if (this.Result == null)
                {
                    this.Result = new OutArgument<bool>();
                }
                metadata.Bind(this.Result, resultArgument);

                metadata.SetArgumentsCollection(
                    new Collection<RuntimeArgument>
                {
                    parentActivityArgument,
                    txScopeArgument,
                    resultArgument
                });
            }

            protected override bool Execute(CodeActivityContext context)
            {
                Activity parent = this.ParentActivity.Get(context);

                if (parent != null)
                {
                    TransactionScope parentTransactionScope = parent as TransactionScope;
                    Fx.Assert(parentTransactionScope != null, "ParentActivity was not of expected type");
                    TransactionScope currentTransactionScope = this.TransactionScope.Get(context);

                    if (parentTransactionScope.AbortInstanceOnTransactionFailure != currentTransactionScope.AbortInstanceOnTransactionFailure)
                    {
                        //If the Inner TSA was default and still different from outer, we dont flag validation warning. See design spec for all variations
                        if (!currentTransactionScope.abortInstanceFlagWasExplicitlySet)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
