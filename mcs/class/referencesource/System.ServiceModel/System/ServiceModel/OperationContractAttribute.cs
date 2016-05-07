//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Net.Security;
    using System.Reflection;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    [AttributeUsage(ServiceModelAttributeTargets.OperationContract)]
    public sealed class OperationContractAttribute : Attribute
    {
        string name = null;
        string action = null;
        string replyAction = null;
        bool asyncPattern = false;
        bool isInitiating = true;
        bool isTerminating = false;
        bool isOneWay = false;
        ProtectionLevel protectionLevel = ProtectionLevel.None;
        bool hasProtectionLevel = false;

        public string Name
        {
            get { return this.name; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == "")
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.SFxNameCannotBeEmpty)));
                }

                this.name = value;
            }
        }

        internal const string ActionPropertyName = "Action";
        public string Action
        {
            get { return this.action; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.action = value;
            }
        }

        internal const string ProtectionLevelPropertyName = "ProtectionLevel";
        public ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.protectionLevel = value;
                this.hasProtectionLevel = true;
            }
        }

        public bool HasProtectionLevel
        {
            get { return this.hasProtectionLevel; }
        }

        internal const string ReplyActionPropertyName = "ReplyAction";
        public string ReplyAction
        {
            get { return this.replyAction; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.replyAction = value;
            }
        }

        public bool AsyncPattern
        {
            get { return this.asyncPattern; }
            set { this.asyncPattern = value; }
        }

        public bool IsOneWay
        {
            get { return this.isOneWay; }
            set { this.isOneWay = value; }
        }

        public bool IsInitiating
        {
            get { return this.isInitiating; }
            set { this.isInitiating = value; }
        }

        public bool IsTerminating
        {
            get { return this.isTerminating; }
            set { this.isTerminating = value; }
        }

        internal bool IsSessionOpenNotificationEnabled
        {
            get 
            {
                return this.Action == OperationDescription.SessionOpenedAction;
            }
        }

        internal void EnsureInvariants(MethodInfo methodInfo, string operationName)
        {
            if (this.IsSessionOpenNotificationEnabled)
            {
                if (!this.IsOneWay
                 || !this.IsInitiating
                 || methodInfo.GetParameters().Length > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.ContractIsNotSelfConsistentWhenIsSessionOpenNotificationEnabled, operationName, "Action", OperationDescription.SessionOpenedAction, "IsOneWay", "IsInitiating")));
                }
            }
        }
    }
}
