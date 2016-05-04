//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Administration;

    [AttributeUsage(AttributeTargets.Method)]
    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public sealed class DurableOperationAttribute : Attribute, IOperationBehavior, IWmiInstanceProvider
    {
        static DurableOperationAttribute defaultInstance = new DurableOperationAttribute();
        bool canCreateInstance;
        bool canCreateInstanceSetExplicitly;
        bool completesInstance;

        public DurableOperationAttribute()
        {
            this.completesInstance = false;
        }

        public bool CanCreateInstance
        {
            get
            {
                return this.canCreateInstance;
            }
            set
            {
                this.canCreateInstance = value;
                this.canCreateInstanceSetExplicitly = true;
            }
        }

        public bool CompletesInstance
        {
            get
            {
                return this.completesInstance;
            }
            set
            {
                this.completesInstance = value;
            }
        }

        internal static DurableOperationAttribute DefaultInstance
        {
            get
            {
                return defaultInstance;
            }
        }

        public void AddBindingParameters(
            OperationDescription operationDescription,
            BindingParameterCollection bindingParameters)
        {
            // empty
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            // empty
        }

        public void ApplyDispatchBehavior(
            OperationDescription operationDescription,
            DispatchOperation dispatchOperation)
        {
            if (dispatchOperation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatchOperation");
            }

            if (dispatchOperation.Invoker == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.ExistingIOperationInvokerRequired,
                    typeof(DurableOperationAttribute).Name)));
            }

            if (operationDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationDescription");
            }

            if (operationDescription.DeclaringContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "operationDescription",
                    SR2.GetString(SR2.OperationDescriptionNeedsDeclaringContract));
            }

            bool canCreate = CanCreateInstanceForOperation(dispatchOperation.IsOneWay);

            dispatchOperation.Invoker =
                new ServiceOperationInvoker(
                dispatchOperation.Invoker,
                this.CompletesInstance,
                canCreate,
                operationDescription.DeclaringContract.SessionMode != SessionMode.NotAllowed);
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            wmiInstance.SetProperty("CanCreateInstance", this.CanCreateInstance);
            wmiInstance.SetProperty("CompletesInstance", this.CompletesInstance);
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return "DurableOperationAttribute";
        }

        public void Validate(OperationDescription operationDescription)
        {
            // empty
        }

        internal bool CanCreateInstanceForOperation(bool isOneWay)
        {
            bool canCreate = false;

            if (this.canCreateInstanceSetExplicitly)
            {
                canCreate = this.canCreateInstance;
            }
            else
            {
                if (isOneWay)
                {
                    canCreate = false;
                }
                else
                {
                    canCreate = true;
                }
            }

            return canCreate;
        }
    }
}
