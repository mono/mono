//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Reflection;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.Transactions;
    using System.ServiceModel.Security;
    using System.Security.Principal;
    using System.Collections.Generic;

    [AttributeUsage(ServiceModelAttributeTargets.OperationBehavior)]
    public sealed class OperationBehaviorAttribute : Attribute, IOperationBehavior
    {
        internal const ImpersonationOption DefaultImpersonationOption = ImpersonationOption.NotAllowed;
        bool autoCompleteTransaction = true;
        bool autoEnlistTransaction = false;
        bool autoDisposeParameters = true;
        bool preferAsyncInvocation = false;
        ImpersonationOption impersonation = ImpersonationOption.NotAllowed;
        ReleaseInstanceMode releaseInstance = ReleaseInstanceMode.None;


        public bool TransactionAutoComplete
        {
            get { return this.autoCompleteTransaction; }
            set { this.autoCompleteTransaction = value; }
        }

        public bool TransactionScopeRequired
        {
            get { return this.autoEnlistTransaction; }
            set { this.autoEnlistTransaction = value; }
        }

        public bool AutoDisposeParameters
        {
            get { return this.autoDisposeParameters; }
            set { this.autoDisposeParameters = value; }
        }

        public ImpersonationOption Impersonation
        {
            get
            {
                return this.impersonation;
            }
            set
            {
                if (!ImpersonationOptionHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.impersonation = value;
            }
        }

        public ReleaseInstanceMode ReleaseInstanceMode
        {
            get { return this.releaseInstance; }
            set
            {
                if (!ReleaseInstanceModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.releaseInstance = value;
            }
        }

        internal bool PreferAsyncInvocation
        {
            get { return this.preferAsyncInvocation; }
            set { this.preferAsyncInvocation = value; }
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (dispatch == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatch");
            }
            if (description.IsServerInitiated() && this.releaseInstance != ReleaseInstanceMode.None)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxOperationBehaviorAttributeReleaseInstanceModeDoesNotApplyToCallback,
                    description.Name)));
            }
            dispatch.TransactionRequired = this.autoEnlistTransaction;
            dispatch.TransactionAutoComplete = this.autoCompleteTransaction;
            dispatch.AutoDisposeParameters = this.autoDisposeParameters;
            dispatch.ReleaseInstanceBeforeCall = (this.releaseInstance & ReleaseInstanceMode.BeforeCall) != 0;
            dispatch.ReleaseInstanceAfterCall = (this.releaseInstance & ReleaseInstanceMode.AfterCall) != 0;
            dispatch.Impersonation = this.Impersonation;
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
        }
    }
}
