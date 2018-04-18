//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    [DebuggerDisplay("Address={address}")]
    [DebuggerDisplay("Name={name}")]
    public class ServiceEndpoint
    {
        EndpointAddress address;
        Binding binding;
        ContractDescription contract;
        Uri listenUri;
        ListenUriMode listenUriMode = ListenUriMode.Explicit;
        KeyedByTypeCollection<IEndpointBehavior> behaviors;
        string id;
        XmlName name;
        bool isEndpointFullyConfigured = false;

        public ServiceEndpoint(ContractDescription contract)
        {
            if (contract == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            this.contract = contract;
        }

        public ServiceEndpoint(ContractDescription contract, Binding binding, EndpointAddress address)
        {
            if (contract == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");            

            this.contract = contract;
            this.binding = binding;
            this.address = address;
        }

        public EndpointAddress Address
        {
            get { return this.address; }
            set { this.address = value; }
        }

        public KeyedCollection<Type, IEndpointBehavior> EndpointBehaviors
        {
            get { return this.Behaviors; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public KeyedByTypeCollection<IEndpointBehavior> Behaviors
        {
            get
            {
                if (this.behaviors == null)
                {
                    this.behaviors = new KeyedByTypeCollection<IEndpointBehavior>();
                }

                return this.behaviors;
            }
        }

        public Binding Binding
        {
            get { return this.binding; }
            set { this.binding = value; }
        }

        public ContractDescription Contract
        {
            get { return this.contract; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.contract = value;
            }
        }

        public bool IsSystemEndpoint
        {
            get;
            set;
        }

        public string Name
        {
            get
            {
                if (!XmlName.IsNullOrEmpty(name))
                {
                    return name.EncodedName;
                }
                else if (binding != null)
                {
                    // Microsoft: composing names have potential problem of generating name that looks like an encoded name, consider avoiding '_'
                    return String.Format(CultureInfo.InvariantCulture, "{0}_{1}", new XmlName(Binding.Name).EncodedName, Contract.Name);
                }
                else
                {
                    return Contract.Name;
                }
            }
            set
            {
                name = new XmlName(value, true /*isEncoded*/);
            }
        }

        public Uri ListenUri
        {
            get 
            {
                if (this.listenUri == null)
                {
                    if (this.address == null)
                    {
                        return null;
                    }
                    else
                    {
                        return this.address.Uri;
                    }
                }
                else
                {
                    return this.listenUri;
                }
            }
            set
            {
                if (value != null && !value.IsAbsoluteUri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.UriMustBeAbsolute));
                }
                this.listenUri = value;
            }
        }

        public ListenUriMode ListenUriMode
        {
            get { return this.listenUriMode; }
            set
            {
                if (!ListenUriModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.listenUriMode = value;
            }
        }

        internal string Id
        {
            get 
            { 
                if (id == null)
                    id = Guid.NewGuid().ToString();
                return id; 
            }
        }

        internal Uri UnresolvedAddress
        {
            get;
            set;
        }

        internal Uri UnresolvedListenUri
        {
            get;
            set;
        }

        // This method ensures that the description object graph is structurally sound and that none
        // of the fundamental SFx framework assumptions have been violated.
        internal void EnsureInvariants()
        {
            if (Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.AChannelServiceEndpointSBindingIsNull0)));
            }
            if (Contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.AChannelServiceEndpointSContractIsNull0)));
            }
            this.Contract.EnsureInvariants();
            this.Binding.EnsureInvariants(this.Contract.Name);
        }

        internal void ValidateForClient()
        {
            Validate(true, false);
        }

        internal void ValidateForService(bool runOperationValidators)
        {
            Validate(runOperationValidators, true);
        }

        internal bool IsFullyConfigured 
        {
            get { return this.isEndpointFullyConfigured; }
            set { this.isEndpointFullyConfigured = value; }
        }

        // for V1 legacy reasons, a mex endpoint is considered a system endpoint even if IsSystemEndpoint = false
        internal bool InternalIsSystemEndpoint(ServiceDescription description)
        {
            if (ServiceMetadataBehavior.IsMetadataEndpoint(description, this))
            {
                return true;
            }
            return this.IsSystemEndpoint;
        }

        // This method runs validators (both builtin and ones in description).  
        // Precondition: EnsureInvariants() should already have been called.
        void Validate(bool runOperationValidators, bool isForService)
        {
            // contract behaviors
            ContractDescription contract = this.Contract;
            for (int j = 0; j < contract.Behaviors.Count; j++)
            {
                IContractBehavior iContractBehavior = contract.Behaviors[j];
                iContractBehavior.Validate(contract, this);
            }
            // endpoint behaviors
            if (!isForService)
            {
                (PartialTrustValidationBehavior.Instance as IEndpointBehavior).Validate(this);
#pragma warning disable 0618
                (PeerValidationBehavior.Instance as IEndpointBehavior).Validate(this);
#pragma warning restore 0618
                (TransactionValidationBehavior.Instance as IEndpointBehavior).Validate(this);
                (SecurityValidationBehavior.Instance as IEndpointBehavior).Validate(this);
                (System.ServiceModel.MsmqIntegration.MsmqIntegrationValidationBehavior.Instance as IEndpointBehavior).Validate(this);
            }
            for (int j = 0; j < this.Behaviors.Count; j++)
            {
                IEndpointBehavior ieb = this.Behaviors[j];
                ieb.Validate(this);
            }
            // operation behaviors
            if (runOperationValidators)
            {
                for (int j = 0; j < contract.Operations.Count; j++)
                {
                    OperationDescription op = contract.Operations[j];
                    TaskOperationDescriptionValidator.Validate(op, isForService);
                    for (int k = 0; k < op.Behaviors.Count; k++)
                    {
                        IOperationBehavior iob = op.Behaviors[k];
                        iob.Validate(op);
                    }
                }
            }
        }
    }
}
