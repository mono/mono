//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Activities.Validation;
    using System.Linq;
    using System.Activities.Expressions;

    public sealed class Compensate : NativeActivity
    {
        static Constraint compensateWithNoTarget = Compensate.CompensateWithNoTarget();

        InternalCompensate internalCompensate;
        DefaultCompensation defaultCompensation;

        Variable<CompensationToken> currentCompensationToken;

        public Compensate()
            : base()
        {
            this.currentCompensationToken = new Variable<CompensationToken>();
        }

        [DefaultValue(null)]
        public InArgument<CompensationToken> Target
        {
            get;
            set;
        }

        DefaultCompensation DefaultCompensation
        {
            get
            {
                if (this.defaultCompensation == null)
                {
                    this.defaultCompensation = new DefaultCompensation()
                        {
                            Target = new InArgument<CompensationToken>(this.currentCompensationToken),
                        };
                }

                return this.defaultCompensation;
            }
        }

        InternalCompensate InternalCompensate
        {
            get
            {
                if (this.internalCompensate == null)
                {
                    this.internalCompensate = new InternalCompensate()
                        {
                            Target = new InArgument<CompensationToken>(new ArgumentValue<CompensationToken> { ArgumentName = "Target" }),
                        };
                }

                return this.internalCompensate;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument targetArgument = new RuntimeArgument("Target", typeof(CompensationToken), ArgumentDirection.In);
            metadata.Bind(this.Target, targetArgument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { targetArgument });

            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.currentCompensationToken });

            Fx.Assert(DefaultCompensation != null, "DefaultCompensation must be valid");
            Fx.Assert(InternalCompensate != null, "InternalCompensate must be valid");
            metadata.SetImplementationChildrenCollection(
                new Collection<Activity>
                {
                    DefaultCompensation,
                    InternalCompensate
                });
        }

        internal override IList<Constraint> InternalGetConstraints()
        {
            return new List<Constraint>(1) { compensateWithNoTarget };
        }

        static Constraint CompensateWithNoTarget()
        {
            DelegateInArgument<Compensate> element = new DelegateInArgument<Compensate> { Name = "element" };
            DelegateInArgument<ValidationContext> validationContext = new DelegateInArgument<ValidationContext> { Name = "validationContext" };
            Variable<bool> assertFlag = new Variable<bool> { Name = "assertFlag" };
            Variable<IEnumerable<Activity>> elements = new Variable<IEnumerable<Activity>>() { Name = "elements" };
            Variable<int> index = new Variable<int>() { Name = "index" };

            return new Constraint<Compensate>
            {
                Body = new ActivityAction<Compensate, ValidationContext>
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
                            new If
                            {
                                Condition = new InArgument<bool>((env) => element.Get(env).Target != null),
                                Then = new Assign<bool>
                                {
                                    To = assertFlag,
                                    Value = true
                                },
                                Else = new Sequence
                                {
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
                                        new While(env => (assertFlag.Get(env) != true) && index.Get(env) < elements.Get(env).Count())
                                        {
                                            Body = new Sequence
                                            {
                                                Activities = 
                                                {
                                                    new If(env => (elements.Get(env).ElementAt(index.Get(env))).GetType() == typeof(CompensationParticipant))
                                                    {
                                                        Then = new Assign<bool>
                                                        {
                                                            To = assertFlag,
                                                            Value = true                                                            
                                                        },
                                                    },
                                                    new Assign<int>
                                                    {
                                                        To = index,
                                                        Value = new InArgument<int>(env => index.Get(env) + 1)
                                                    },
                                                }
                                            }
                                        }
                                    }
                                }                                
                            },
                            new AssertValidation
                            {
                                Assertion = new InArgument<bool>(assertFlag),
                                Message = new InArgument<string>(SR.CompensateWithNoTargetConstraint)   
                            }
                        }
                    }
                }
            };
        }

        protected override void Execute(NativeActivityContext context)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            if (compensationExtension == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CompensateWithoutCompensableActivity(this.DisplayName)));
            }

            if (Target.IsEmpty)
            {
                CompensationToken ambientCompensationToken = (CompensationToken)context.Properties.Find(CompensationToken.PropertyName);
                CompensationTokenData ambientTokenData = ambientCompensationToken == null ? null : compensationExtension.Get(ambientCompensationToken.CompensationId);

                if (ambientTokenData != null && ambientTokenData.IsTokenValidInSecondaryRoot)
                {
                    this.currentCompensationToken.Set(context, ambientCompensationToken);
                    if (ambientTokenData.ExecutionTracker.Count > 0)
                    {
                        context.ScheduleActivity(DefaultCompensation);
                    }
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidCompensateActivityUsage(this.DisplayName)));
                }
            }
            else
            {
                CompensationToken compensationToken = Target.Get(context);
                CompensationTokenData tokenData = compensationToken == null ? null : compensationExtension.Get(compensationToken.CompensationId);

                if (compensationToken == null)
                {
                    throw FxTrace.Exception.Argument("Target", SR.InvalidCompensationToken(this.DisplayName));
                }

                if (compensationToken.CompensateCalled)
                {
                    // No-Op
                    return;
                }

                if (tokenData == null || tokenData.CompensationState != CompensationState.Completed)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CompensableActivityAlreadyConfirmedOrCompensated));
                }

                // A valid in-arg was passed...            
                tokenData.CompensationState = CompensationState.Compensating;
                compensationToken.CompensateCalled = true;
                context.ScheduleActivity(InternalCompensate);
            }
        }

        protected override void Cancel(NativeActivityContext context)
        {
            // Suppress Cancel   
        }
    }
}
