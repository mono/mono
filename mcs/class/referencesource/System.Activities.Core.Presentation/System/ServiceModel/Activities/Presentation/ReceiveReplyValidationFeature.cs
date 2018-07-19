//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System.Runtime;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Validation;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Activities;
    using System.Activities.Statements;
    using System.Globalization;

    class ReceiveReplyValidationFeature : DesignTimeValidationFeature
    {
        List<Constraint> constraints;

        protected override Type ApplyTo
        {
            get { return typeof(ReceiveReply); }
        }

        protected override IList<Constraint> DesignTimeConstraints
        {
            get
            {
                if (this.constraints == null)
                {
                    this.constraints = new List<Constraint> { UnrootedRequestRule() };
                }
                return this.constraints;
            }
        }

        Constraint UnrootedRequestRule()
        {
            DelegateInArgument<ReceiveReply> receiveReply = new DelegateInArgument<ReceiveReply>();
            DelegateInArgument<ValidationContext> context = new DelegateInArgument<ValidationContext>();
            DelegateInArgument<Activity> activityInTree = new DelegateInArgument<Activity>();
            Variable<bool> requestInTree = new Variable<bool> { Default = false };

            return new Constraint<ReceiveReply>
            {
                Body = new ActivityAction<ReceiveReply, ValidationContext>
                {
                    Argument1 = receiveReply,
                    Argument2 = context,
                    Handler = new Sequence
                    {
                        Variables = { requestInTree },
                        Activities =
                        {
                            new If
                            {
                                Condition = new InArgument<bool>(ctx => receiveReply.Get(ctx).Request != null),
                                Then = new Sequence
                                {
                                    Activities = 
                                    {
                                        new ForEach<Activity>
                                        {
                                            Values = new GetWorkflowTree
                                            {
                                                ValidationContext = context,
                                            },
                                            Body = new ActivityAction<Activity>
                                            {
                                                Argument = activityInTree,
                                                Handler = new If
                                                {
                                                    Condition = new InArgument<bool>(ctx => activityInTree.Get(ctx) == receiveReply.Get(ctx).Request),
                                                    Then = new Assign<bool>
                                                    {
                                                        To = requestInTree,
                                                        Value = true,
                                                    }                                                    
                                                }
                                            }
                                        },                            
                                        new AssertValidation
                                        {                                
                                            Assertion = new InArgument<bool>(ctx => requestInTree.Get(ctx)),
                                            IsWarning = false,
                                            Message = new InArgument<string>(ctx => string.Format(CultureInfo.CurrentCulture, System.Activities.Core.Presentation.SR.UnrootedRequestInReceiveReply, receiveReply.Get(ctx).DisplayName))
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
