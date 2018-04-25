//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Windows.Markup;
    using SR2 = System.ServiceModel.Activities.SR;

    [ContentProperty("Body")]
    public sealed class TransactedReceiveScope : NativeActivity
    {
        Variable<RuntimeTransactionHandle> transactionHandle;
        Collection<Variable> variables;
        const string AbortInstanceOnTransactionFailurePropertyName = "AbortInstanceOnTransactionFailure";
        const string RequestPropertyName = "Request";
        const string BodyPropertyName = "Body";
        Variable<bool> isNested;
        static AsyncCallback transactionCommitCallback;

        public TransactedReceiveScope()
        {
            this.transactionHandle = new Variable<RuntimeTransactionHandle>           
            {
                Name = "TransactionHandle"
            };
            this.isNested = new Variable<bool>();

            base.Constraints.Add(ProcessChildSubtreeConstraints());
        }

        [DefaultValue(null)]
        public Receive Request
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public Activity Body
        {
            get;
            set;
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
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                }
                return this.variables;
            }
        }

        internal static AsyncCallback TransactionCommitAsyncCallback
        {
            get
            {
                if (transactionCommitCallback == null)
                {
                    transactionCommitCallback = Fx.ThunkCallback(new AsyncCallback(TransactionCommitCallback));
                }

                return transactionCommitCallback;
            }
        }

        Constraint ProcessChildSubtreeConstraints()
        {
            DelegateInArgument<TransactedReceiveScope> element = new DelegateInArgument<TransactedReceiveScope> { Name = "element" };
            DelegateInArgument<ValidationContext> validationContext = new DelegateInArgument<ValidationContext> { Name = "validationContext" };
            DelegateInArgument<Activity> child = new DelegateInArgument<Activity> { Name = "child" };
            Variable<bool> nestedCompensableActivity = new Variable<bool>
            {
                Name = "nestedCompensableActivity"
            };

            return new Constraint<TransactedReceiveScope>
            {
                Body = new ActivityAction<TransactedReceiveScope, ValidationContext>
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
                                                Condition = new Equal<Type, Type, bool>
                                                {
                                                    Left = new ObtainType
                                                    {
                                                        Input = new InArgument<Activity>(child)
                                                    },
                                                    Right = new InArgument<Type>(context => typeof(TransactionScope))
                                                },
                                                Then = new AssertValidation
                                                {
                                                    IsWarning = true,
                                                    Assertion = new NestedChildTransactionScopeActivityAbortInstanceFlagValidator
                                                    {
                                                         Child = child
                                                    },
                                                    //Message = new InArgument<string>(env => SR.AbortInstanceOnTransactionFailureDoesNotMatch(child.Get(env).DisplayName, this.DisplayName)),
                                                    Message = new InArgument<string>
                                                    {
                                                        Expression = new NestedChildTransactionScopeActivityAbortInstanceFlagValidatorMessage
                                                        {
                                                            Child = child,
                                                            ParentDisplayName = this.DisplayName
                                                        }
                                                    },
                                                    PropertyName = AbortInstanceOnTransactionFailurePropertyName
                                                }
                                            },
                                            new If()
                                            {
                                                Condition = new Equal<Type, Type, bool>
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
                            new AssertValidation
                            {
                                //Assertion = new InArgument<bool>(env => !nestedCompensableActivity.Get(env)),
                                Assertion = new InArgument<bool>
                                {
                                    Expression = new Not<bool, bool>
                                    {
                                        Operand = new VariableValue<bool>
                                        {
                                            Variable = nestedCompensableActivity
                                        }
                                    }
                                },
                                Message = new InArgument<string>(SR2.CompensableActivityInsideTransactedReceiveScope),
                                PropertyName = BodyPropertyName
                            }
                        }
                    }
                }
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (this.Request == null)
            {
                metadata.AddValidationError(new ValidationError(SR2.TransactedReceiveScopeMustHaveValidReceive(this.DisplayName), false, RequestPropertyName));
            }
            metadata.AddChild(this.Request);
            metadata.AddChild(this.Body);
            metadata.SetVariablesCollection(this.Variables);
            metadata.AddImplementationVariable(this.transactionHandle);
            metadata.AddImplementationVariable(this.isNested);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.Request == null)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.TransactedReceiveScopeRequiresReceive(this.DisplayName)));
            }
            // we have to do this in code since we aren't fully modeled (in order for 
            // dynamic update to work correctly)
            RuntimeTransactionHandle handleInstance = this.transactionHandle.Get(context);
            Fx.Assert(handleInstance != null, "RuntimeTransactionHandle is null");

            //This is used by InternalReceiveMessage to update the InitiatingTransaction so that we can later call Commit/Complete on it
            context.Properties.Add(TransactedReceiveData.TransactedReceiveDataExecutionPropertyName, new TransactedReceiveData());

            RuntimeTransactionHandle foundHandle = context.Properties.Find(handleInstance.ExecutionPropertyName) as RuntimeTransactionHandle;
            if (foundHandle == null)
            {
                context.Properties.Add(handleInstance.ExecutionPropertyName, handleInstance);
            }
            else
            {
                 //nested case
                if (foundHandle.SuppressTransaction)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.CannotNestTransactedReceiveScopeWhenAmbientHandleIsSuppressed(this.DisplayName)));
                }

                // Verify if TRS is root and if the foundHandle is not from the parent HandleScope<RTH>
                if (foundHandle.GetCurrentTransaction(context) != null)
                {
                    handleInstance = foundHandle;
                    this.isNested.Set(context, true);
                }                
            }
            context.ScheduleActivity(this.Request, new CompletionCallback(OnReceiveCompleted));
        }

        void OnReceiveCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (this.Body != null)
            {
                context.ScheduleActivity(this.Body, new CompletionCallback(OnBodyCompleted));
            }
            else if (completedInstance.State == ActivityInstanceState.Closed)
            {
                OnBodyCompleted(context, completedInstance);
            }
        }

        void OnBodyCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            TransactedReceiveData transactedReceiveData = context.Properties.Find(TransactedReceiveData.TransactedReceiveDataExecutionPropertyName) as TransactedReceiveData;
            Fx.Assert(transactedReceiveData != null, "TransactedReceiveScope.OnBodyComplete - transactedreceivedata is null");

            //Non Nested
            if (!this.isNested.Get(context))
            {
                Fx.Assert(transactedReceiveData.InitiatingTransaction != null, "TransactedReceiveScope.OnBodyComplete - Initiating transaction is null");
                System.Transactions.CommittableTransaction committableTransaction = transactedReceiveData.InitiatingTransaction as System.Transactions.CommittableTransaction;
                //If the initiating transaction was a committable transaction => this is a server side only transaction. Commit it here instead of letting the dispatcher deal with it 
                //since we are Auto Complete = false and we want the completion of the TransactedReceiveScope to initiate the Commit.
                if (committableTransaction != null)
                {
                    committableTransaction.BeginCommit(TransactionCommitAsyncCallback, committableTransaction);
                }
                else
                {
                    //If the initiating transaction was a dependent transaction instead => this is a flowed in transaction, let's just complete the dependent clone
                    System.Transactions.DependentTransaction dependentTransaction = transactedReceiveData.InitiatingTransaction as System.Transactions.DependentTransaction;
                    Fx.Assert(dependentTransaction != null, "TransactedReceiveScope.OnBodyComplete - DependentClone was null");
                    dependentTransaction.Complete();
                }
            }
            else //Nested scenario - e.g TRS inside a TSA and in a flow case :- we still need to complete the dependent transaction
            {
                System.Transactions.DependentTransaction dependentTransaction = transactedReceiveData.InitiatingTransaction as System.Transactions.DependentTransaction;
                if (dependentTransaction != null)
                {
                    dependentTransaction.Complete();
                }
            }
        }

        static void TransactionCommitCallback(IAsyncResult result)
        {
            System.Transactions.CommittableTransaction committableTransaction = result.AsyncState as System.Transactions.CommittableTransaction;
            Fx.Assert(committableTransaction != null, "TransactedReceiveScope - In the static TransactionCommitCallback, the committable transaction was null");
            try
            {
                committableTransaction.EndCommit(result);
            }
            catch (System.Transactions.TransactionException ex)
            {
                //At this point, the activity has completed. Since the runtime is enlisted in the transaction, it knows that the transaction aborted.
                //The runtime will do the right thing based on the AbortInstanceOnTransactionFailure flag. We simply trace out that the call to EndCommit failed from this static callback
                if (TD.TransactedReceiveScopeEndCommitFailedIsEnabled())
                {
                    TD.TransactedReceiveScopeEndCommitFailed(committableTransaction.TransactionInformation.LocalIdentifier, ex.Message);
                }
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

        class NestedChildTransactionScopeActivityAbortInstanceFlagValidator : CodeActivity<bool>
        {
            public InArgument<Activity> Child
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument childArgument = new RuntimeArgument("Child", typeof(Activity), ArgumentDirection.In);
                if (this.Child == null)
                {
                    this.Child = new InArgument<Activity>();
                }
                metadata.Bind(this.Child, childArgument);

                RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(bool), ArgumentDirection.Out);
                metadata.Bind(this.Result, resultArgument);

                metadata.SetArgumentsCollection(
                    new Collection<RuntimeArgument>
                {
                    childArgument,
                    resultArgument
                });
            }

            protected override bool Execute(CodeActivityContext context)
            {
                Activity child = this.Child.Get(context);

                if (child != null)
                {
                    TransactionScope transactionScopeActivity = child as TransactionScope;
                    Fx.Assert(transactionScopeActivity != null, "Child was not of expected type");

                    //We dont care whether the flag was explicitly set
                    // a) We cant tell whether the flag was explicitly set on the child
                    // b) This is mostly a scenario where the WF calls into a library. It is OK 
                    // to flag the warning either

                    return transactionScopeActivity.AbortInstanceOnTransactionFailure; 
                }

                return true;
            }
        }

        // Message = new InArgument<string>(env => SR.AbortInstanceOnTransactionFailureDoesNotMatch(child.Get(env).DisplayName, this.DisplayName)),
        class NestedChildTransactionScopeActivityAbortInstanceFlagValidatorMessage : CodeActivity<string>
        {
            public InArgument<Activity> Child
            {
                get;
                set;
            }

            public InArgument<string> ParentDisplayName
            {
                get;
                set;
            }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                RuntimeArgument childArgument = new RuntimeArgument("Child", typeof(Activity), ArgumentDirection.In);
                if (this.Child == null)
                {
                    this.Child = new InArgument<Activity>();
                }
                metadata.Bind(this.Child, childArgument);

                RuntimeArgument parentDisplayNameArgument = new RuntimeArgument("ParentDisplayName", typeof(string), ArgumentDirection.In);
                if (this.ParentDisplayName == null)
                {
                    this.ParentDisplayName = new InArgument<string>();
                }
                metadata.Bind(this.ParentDisplayName, parentDisplayNameArgument);

                RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(string), ArgumentDirection.Out);
                if (this.Result == null)
                {
                    this.Result = new OutArgument<string>();
                }
                metadata.Bind(this.Result, resultArgument);

                metadata.SetArgumentsCollection(
                    new Collection<RuntimeArgument>
                {
                    childArgument,
                    parentDisplayNameArgument,
                    resultArgument
                });
            }

            protected override string Execute(CodeActivityContext context)
            {
                // env => SR.AbortInstanceOnTransactionFailureDoesNotMatch(child.Get(env).DisplayName, this.DisplayName)
                return SR.AbortInstanceOnTransactionFailureDoesNotMatch(this.Child.Get(context).DisplayName, this.ParentDisplayName.Get(context));
            }
        }
    }  
}
