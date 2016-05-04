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
    using System.Net.Security;
    using System.ServiceModel.Security;

    [DebuggerDisplay("Name={name}, Namespace={ns}, ContractType={contractType}")]
    public class ContractDescription
    {
        Type callbackContractType;
        string configurationName;
        Type contractType;
        XmlName name;
        string ns;
        OperationDescriptionCollection operations;
        SessionMode sessionMode;
        KeyedByTypeCollection<IContractBehavior> behaviors = new KeyedByTypeCollection<IContractBehavior>();
        ProtectionLevel protectionLevel;
        bool hasProtectionLevel;

        public ContractDescription(string name)
            : this(name, null)
        {
        }

        public ContractDescription(string name, string ns)
        {
            // the property setter validates given value
            this.Name = name;
            if (!string.IsNullOrEmpty(ns))
                NamingHelper.CheckUriParameter(ns, "ns");

            this.operations = new OperationDescriptionCollection();
            this.ns = ns ?? NamingHelper.DefaultNamespace; // ns can be ""
        }

        internal string CodeName
        {
            get { return this.name.DecodedName; }
        }

        [DefaultValue(null)]
        public string ConfigurationName
        {
            get { return this.configurationName; }
            set { this.configurationName = value; }
        }

        public Type ContractType
        {
            get { return this.contractType; }
            set { this.contractType = value; }
        }

        public Type CallbackContractType
        {
            get { return this.callbackContractType; }
            set { this.callbackContractType = value; }
        }

        public string Name
        {
            get { return this.name.EncodedName; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (value.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException("value", SR.GetString(SR.SFxContractDescriptionNameCannotBeEmpty)));
                }
                this.name = new XmlName(value, true /*isEncoded*/);
            }
        }

        public string Namespace
        {
            get { return this.ns; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    NamingHelper.CheckUriProperty(value, "Namespace");
                this.ns = value;
            }
        }

        public OperationDescriptionCollection Operations
        {
            get { return this.operations; }
        }

        public ProtectionLevel ProtectionLevel
        {
            get { return this.protectionLevel; }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.protectionLevel = value;
                this.hasProtectionLevel = true;
            }
        }

        public bool ShouldSerializeProtectionLevel()
        {
            return this.HasProtectionLevel;
        }

        public bool HasProtectionLevel
        {
            get { return this.hasProtectionLevel; }
        }

        [DefaultValue(SessionMode.Allowed)]
        public SessionMode SessionMode
        {
            get { return this.sessionMode; }
            set
            {
                if (!SessionModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.sessionMode = value;
            }
        }

        public KeyedCollection<Type, IContractBehavior> ContractBehaviors 
        {
            get { return this.Behaviors; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)] 
        public KeyedByTypeCollection<IContractBehavior> Behaviors
        {
            get { return this.behaviors; }
        }

        public static ContractDescription GetContract(Type contractType)
        {
            if (contractType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");

            TypeLoader typeLoader = new TypeLoader();
            return typeLoader.LoadContractDescription(contractType);
        }

        public static ContractDescription GetContract(Type contractType, Type serviceType)
        {
            if (contractType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");

            if (serviceType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceType");

            TypeLoader typeLoader = new TypeLoader();
            ContractDescription description = typeLoader.LoadContractDescription(contractType, serviceType);
            return description;
        }

        public static ContractDescription GetContract(Type contractType, object serviceImplementation)
        {
            if (contractType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");

            if (serviceImplementation == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceImplementation");

            TypeLoader typeLoader = new TypeLoader();
            Type serviceType = serviceImplementation.GetType();
            ContractDescription description = typeLoader.LoadContractDescription(contractType, serviceType, serviceImplementation);
            return description;
        }

        public Collection<ContractDescription> GetInheritedContracts()
        {
            Collection<ContractDescription> result = new Collection<ContractDescription>();
            for (int i = 0; i < Operations.Count; i++)
            {
                OperationDescription od = Operations[i];
                if (od.DeclaringContract != this)
                {
                    ContractDescription inheritedContract = od.DeclaringContract;
                    if (!result.Contains(inheritedContract))
                    {
                        result.Add(inheritedContract);
                    }
                }
            }
            return result;
        }

        internal void EnsureInvariants()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.AChannelServiceEndpointSContractSNameIsNull0)));
            }
            if (this.Namespace == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.AChannelServiceEndpointSContractSNamespace0)));
            }
            if (this.Operations.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxContractHasZeroOperations, this.Name)));
            }
            bool thereIsAtLeastOneInitiatingOperation = false;
            for (int i = 0; i < this.Operations.Count; i++)
            {
                OperationDescription operationDescription = this.Operations[i];
                operationDescription.EnsureInvariants();
                if (operationDescription.IsInitiating)
                    thereIsAtLeastOneInitiatingOperation = true;
                if ((!operationDescription.IsInitiating || operationDescription.IsTerminating)
                    && (this.SessionMode != SessionMode.Required))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.ContractIsNotSelfConsistentItHasOneOrMore2, this.Name)));
                }
            }
            if (!thereIsAtLeastOneInitiatingOperation)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxContractHasZeroInitiatingOperations, this.Name)));
            }
        }

        internal bool IsDuplex()
        {
            for (int i = 0; i < this.operations.Count; ++i)
            {
                if (this.operations[i].IsServerInitiated())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
