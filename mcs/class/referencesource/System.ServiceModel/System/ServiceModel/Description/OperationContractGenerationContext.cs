//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.ServiceModel;

    public class OperationContractGenerationContext
    {
        readonly CodeMemberMethod syncMethod;
        readonly CodeMemberMethod beginMethod;
        readonly ServiceContractGenerationContext contract;
        readonly CodeMemberMethod endMethod;
        readonly OperationDescription operation;
        readonly ServiceContractGenerator serviceContractGenerator;
        readonly CodeTypeDeclaration declaringType;
        readonly CodeMemberMethod taskMethod;

        CodeTypeReference declaringTypeReference;
        

        OperationContractGenerationContext(ServiceContractGenerator serviceContractGenerator, ServiceContractGenerationContext contract, OperationDescription operation, CodeTypeDeclaration declaringType)
        {
            if (serviceContractGenerator == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceContractGenerator"));
            if (contract == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contract"));
            if (declaringType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("declaringType"));

            this.serviceContractGenerator = serviceContractGenerator;
            this.contract = contract;
            this.operation = operation;
            this.declaringType = declaringType;
        }

        public OperationContractGenerationContext(ServiceContractGenerator serviceContractGenerator, ServiceContractGenerationContext contract, OperationDescription operation, CodeTypeDeclaration declaringType, CodeMemberMethod syncMethod, CodeMemberMethod beginMethod, CodeMemberMethod endMethod, CodeMemberMethod taskMethod)
            : this(serviceContractGenerator, contract, operation, declaringType)
        {
            if (syncMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncMethod"));
            if (beginMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("beginMethod"));
            if (endMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endMethod"));
            if (taskMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("taskMethod"));

            this.syncMethod = syncMethod;
            this.beginMethod = beginMethod;
            this.endMethod = endMethod;
            this.taskMethod = taskMethod;
        }

        public OperationContractGenerationContext(ServiceContractGenerator serviceContractGenerator, ServiceContractGenerationContext contract, OperationDescription operation, CodeTypeDeclaration declaringType, CodeMemberMethod syncMethod, CodeMemberMethod beginMethod, CodeMemberMethod endMethod)
            : this(serviceContractGenerator, contract, operation, declaringType)
        {
            if (syncMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncMethod"));
            if (beginMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("beginMethod"));
            if (endMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endMethod"));

            this.syncMethod = syncMethod;
            this.beginMethod = beginMethod;
            this.endMethod = endMethod;
        }

        public OperationContractGenerationContext(ServiceContractGenerator serviceContractGenerator, ServiceContractGenerationContext contract, OperationDescription operation, CodeTypeDeclaration declaringType, CodeMemberMethod syncMethod, CodeMemberMethod taskMethod)
            : this(serviceContractGenerator, contract, operation, declaringType)
        {
            if (syncMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncMethod"));
            if (taskMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("taskMethod"));

            this.syncMethod = syncMethod;
            this.taskMethod = taskMethod;
        }

        public OperationContractGenerationContext(ServiceContractGenerator serviceContractGenerator, ServiceContractGenerationContext contract, OperationDescription operation, CodeTypeDeclaration declaringType, CodeMemberMethod method)
            : this(serviceContractGenerator, contract, operation, declaringType)
        {
            if (method == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("method"));

            this.syncMethod = method;
            this.beginMethod = null;
            this.endMethod = null;
        }

        public ServiceContractGenerationContext Contract
        {
            get { return this.contract; }
        }

        public CodeTypeDeclaration DeclaringType
        {
            get { return this.declaringType; }
        }

        internal CodeTypeReference DeclaringTypeReference
        {
            get { return this.declaringTypeReference; }
            set { this.declaringTypeReference = value; }
        }

        public CodeMemberMethod BeginMethod
        {
            get { return this.beginMethod; }
        }

        public CodeMemberMethod EndMethod
        {
            get { return this.endMethod; }
        }

        public CodeMemberMethod TaskMethod
        {
            get { return this.taskMethod; }
        }

        public CodeMemberMethod SyncMethod
        {
            get { return this.syncMethod; }
        }

        public bool IsAsync
        {
            get { return this.beginMethod != null; }
        }

        public bool IsTask
        {
            get { return this.taskMethod != null; }
        }

        // true if this operation was declared somewhere up the hierarchy (rather than at this level)
        internal bool IsInherited
        {
            get { return !(this.declaringType == contract.ContractType || this.declaringType == contract.DuplexCallbackType); }
        }

        public OperationDescription Operation
        {
            get { return this.operation; }
        }

        public ServiceContractGenerator ServiceContractGenerator
        {
            get { return this.serviceContractGenerator; }
        }
    }
}
