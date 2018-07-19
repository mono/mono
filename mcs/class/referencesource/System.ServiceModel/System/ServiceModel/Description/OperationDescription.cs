//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net.Security;
    using System.Reflection;
    using System.ServiceModel.Security;

    [DebuggerDisplay("Name={name}, IsInitiating={isInitiating}, IsTerminating={isTerminating}")]
    public class OperationDescription
    {
        internal const string SessionOpenedAction = Channels.WebSocketTransportSettings.ConnectionOpenedAction;
        XmlName name;
        bool isInitiating;
        bool isTerminating;
        bool isSessionOpenNotificationEnabled;
        ContractDescription declaringContract;
        FaultDescriptionCollection faults;
        MessageDescriptionCollection messages;
        KeyedByTypeCollection<IOperationBehavior> behaviors;
        Collection<Type> knownTypes;
        MethodInfo beginMethod;
        MethodInfo endMethod;
        MethodInfo syncMethod;
        MethodInfo taskMethod;
        ProtectionLevel protectionLevel;
        bool hasProtectionLevel;
        bool validateRpcWrapperName = true;
        bool hasNoDisposableParameters;

        public OperationDescription(string name, ContractDescription declaringContract)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (name.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("name", SR.GetString(SR.SFxOperationDescriptionNameCannotBeEmpty)));
            }
            this.name = new XmlName(name, true /*isEncoded*/);
            if (declaringContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("declaringContract");
            }
            this.declaringContract = declaringContract;
            this.isInitiating = true;
            this.isTerminating = false;
            this.faults = new FaultDescriptionCollection();
            this.messages = new MessageDescriptionCollection();
            this.behaviors = new KeyedByTypeCollection<IOperationBehavior>();
            this.knownTypes = new Collection<Type>();
        }

        internal OperationDescription(string name, ContractDescription declaringContract, bool validateRpcWrapperName)
            : this(name, declaringContract)
        {
            this.validateRpcWrapperName = validateRpcWrapperName;
        }

        public KeyedCollection<Type, IOperationBehavior> OperationBehaviors
        {
            get { return this.Behaviors; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)] 
        public KeyedByTypeCollection<IOperationBehavior> Behaviors
        {
            get { return behaviors; }
        }

        // Not serializable on purpose, metadata import/export cannot
        // produce it, only available when binding to runtime
        public MethodInfo TaskMethod
        {
            get { return this.taskMethod; }
            set { this.taskMethod = value; }
        }

        // Not serializable on purpose, metadata import/export cannot
        // produce it, only available when binding to runtime
        public MethodInfo SyncMethod
        {
            get { return this.syncMethod; }
            set { this.syncMethod = value; }
        }

        // Not serializable on purpose, metadata import/export cannot
        // produce it, only available when binding to runtime
        public MethodInfo BeginMethod
        {
            get { return this.beginMethod; }
            set { this.beginMethod = value; }
        }

        internal MethodInfo OperationMethod
        {
            get
            {
                if (this.SyncMethod == null)
                {
                    return this.TaskMethod ?? this.BeginMethod;
                }
                else
                {
                    return this.SyncMethod;
                }
            }
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

        internal bool HasNoDisposableParameters
        {
            get { return this.hasNoDisposableParameters; }
            set { this.hasNoDisposableParameters = value; }
        }

        // Not serializable on purpose, metadata import/export cannot
        // produce it, only available when binding to runtime
        public MethodInfo EndMethod
        {
            get { return this.endMethod; }
            set { this.endMethod = value; }
        }

        public ContractDescription DeclaringContract
        {
            get { return this.declaringContract; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("DeclaringContract");
                }
                else
                {
                    this.declaringContract = value;
                }
            }
        }

        public FaultDescriptionCollection Faults
        {
            get { return faults; }
        }

        public bool IsOneWay
        {
            get { return this.Messages.Count == 1; }
        }

        [DefaultValue(false)]
        public bool IsInitiating
        {
            get { return this.isInitiating; }
            set { this.isInitiating = value; }
        }

        internal bool IsServerInitiated()
        {
            EnsureInvariants();
            return Messages[0].Direction == MessageDirection.Output;
        }

        [DefaultValue(false)]
        public bool IsTerminating
        {
            get { return this.isTerminating; }
            set { this.isTerminating = value; }
        }

        public Collection<Type> KnownTypes
        {
            get { return this.knownTypes; }
        }

        // Messages[0] is the 'request' (first of MEP), and for non-oneway MEPs, Messages[1] is the 'response' (second of MEP)
        public MessageDescriptionCollection Messages
        {
            get { return messages; }
        }

        internal XmlName XmlName
        {
            get { return name; }
        }

        internal string CodeName
        {
            get { return name.DecodedName; }
        }

        public string Name
        {
            get { return name.EncodedName; }
        }

        internal bool IsValidateRpcWrapperName { get { return validateRpcWrapperName; } }


        //This property is set during contract inference in a hosted workflow scenario. This is required to handle correct
        //transactional invocation from the dispatcher in regards to scenarios involving the TransactedReceiveScope activity
        internal bool IsInsideTransactedReceiveScope
        {
            get;
            set;
        }

        //This property is set during contract inference in a hosted workflow scenario. This is required to handle correct
        //transactional invocation from the dispatcher in regards to scenarios involving the TransactedReceiveScope activity
        internal bool IsFirstReceiveOfTransactedReceiveScopeTree
        {
            get;
            set;
        }

        internal Type TaskTResult
        {
            get;
            set;
        }

        internal bool HasOutputParameters
        {
            get
            {
                // For non-oneway operations, Messages[1] is the 'response'
                return (this.Messages.Count > 1) &&
                    (this.Messages[1].Body.Parts.Count > 0);
            }
        }

        internal bool IsSessionOpenNotificationEnabled
        {
            get { return this.isSessionOpenNotificationEnabled; }
            set { this.isSessionOpenNotificationEnabled = value; }
        }

        internal void EnsureInvariants()
        {
            if (this.Messages.Count != 1 && this.Messages.Count != 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new System.InvalidOperationException(SR.GetString(SR.SFxOperationMustHaveOneOrTwoMessages, this.Name)));
            }
        }

        internal void ResetProtectionLevel()
        {
            this.protectionLevel = ProtectionLevel.None;
            this.hasProtectionLevel = false;
        }
    }
}
