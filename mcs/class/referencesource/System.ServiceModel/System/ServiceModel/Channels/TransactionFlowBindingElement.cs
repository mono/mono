//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ComponentModel;
    using System.ServiceModel.Transactions;
    using System.Xml;

    public sealed class TransactionFlowBindingElement : BindingElement, IPolicyExportExtension
    {
        bool transactions;
        TransactionFlowOption issuedTokens;
        TransactionProtocol transactionProtocol;

        public TransactionFlowBindingElement()
            : this(true, TransactionFlowDefaults.TransactionProtocol)
        {
        }

        public TransactionFlowBindingElement(TransactionProtocol transactionProtocol)
            : this(true, transactionProtocol)
        {
        }

        internal TransactionFlowBindingElement(bool transactions)
            : this(transactions, TransactionFlowDefaults.TransactionProtocol)
        {
        }

        internal TransactionFlowBindingElement(bool transactions, TransactionProtocol transactionProtocol)
        {
            this.transactions = transactions;
            this.issuedTokens = transactions ? TransactionFlowOption.Allowed : TransactionFlowOption.NotAllowed;

            if (!TransactionProtocol.IsDefined(transactionProtocol))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidTransactionFlowProtocolValue, transactionProtocol.ToString()));
            }

            this.transactionProtocol = transactionProtocol;
        }

        TransactionFlowBindingElement(TransactionFlowBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.transactions = elementToBeCloned.transactions;
            this.issuedTokens = elementToBeCloned.issuedTokens;

            if (!TransactionProtocol.IsDefined(elementToBeCloned.transactionProtocol))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidTransactionFlowProtocolValue, elementToBeCloned.transactionProtocol.ToString()));
            }

            this.transactionProtocol = elementToBeCloned.transactionProtocol;
            this.AllowWildcardAction = elementToBeCloned.AllowWildcardAction;
        }

        internal bool Transactions
        {
            get
            {
                return this.transactions;
            }
            set
            {
                this.transactions = value;
                this.issuedTokens = value ? TransactionFlowOption.Allowed : TransactionFlowOption.NotAllowed;
            }
        }

        internal TransactionFlowOption IssuedTokens
        {
            get
            {
                return this.issuedTokens;
            }
            set
            {
                ValidateOption(value);
                this.issuedTokens = value;
            }
        }

        public override BindingElement Clone()
        {
            return new TransactionFlowBindingElement(this);
        }

        bool IsFlowEnabled(Dictionary<DirectionalAction, TransactionFlowOption> dictionary)
        {
            if (this.issuedTokens != TransactionFlowOption.NotAllowed)
            {
                return true;
            }

            if (!this.transactions)
            {
                return false;
            }

            foreach (TransactionFlowOption option in dictionary.Values)
            {
                if (option != TransactionFlowOption.NotAllowed)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool IsFlowEnabled(ContractDescription contract)
        {
            if (this.issuedTokens != TransactionFlowOption.NotAllowed)
            {
                return true;
            }

            if (!this.transactions)
            {
                return false;
            }

            foreach (OperationDescription operation in contract.Operations)
            {
                TransactionFlowAttribute parameter = operation.Behaviors.Find<TransactionFlowAttribute>();
                if (parameter != null)
                {
                    if (parameter.Transactions != TransactionFlowOption.NotAllowed)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public TransactionProtocol TransactionProtocol
        {
            get
            {
                return this.transactionProtocol;
            }
            set
            {
                if (!TransactionProtocol.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.transactionProtocol = value;
            }
        }

        [DefaultValue(false)]
        public bool AllowWildcardAction
        {
            get;
            set;
        }

        internal static void ValidateOption(TransactionFlowOption opt)
        {
            if (!TransactionFlowOptionHelper.IsDefined(opt))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.TransactionFlowBadOption)));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransactionProtocol()
        {
            return this.TransactionProtocol != TransactionProtocol.Default;
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }

            if (typeof(TChannel) == typeof(IOutputChannel)
                || typeof(TChannel) == typeof(IDuplexChannel)
                || typeof(TChannel) == typeof(IRequestChannel)
                || typeof(TChannel) == typeof(IOutputSessionChannel)
                || typeof(TChannel) == typeof(IRequestSessionChannel)
                || typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return context.CanBuildInnerChannelFactory<TChannel>();
            }

            return false;
        }

        // The BuildChannelFactory and BuildListenerFactory methods looks for this BindingParameter
        // in the BindingContext:
        //  - Dictionary<DirectionalAction, TransactionFlowOption>
        // which has the per-operation TransactionFlowOptions
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = GetDictionary(context);

            if (!this.IsFlowEnabled(dictionary))
            {
                return context.BuildInnerChannelFactory<TChannel>();
            }

            if (this.issuedTokens == TransactionFlowOption.NotAllowed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TransactionFlowRequiredIssuedTokens)));
            }

            TransactionChannelFactory<TChannel> channelFactory =
                new TransactionChannelFactory<TChannel>(this.transactionProtocol, context, dictionary, this.AllowWildcardAction);

            channelFactory.FlowIssuedTokens = this.IssuedTokens;

            return channelFactory;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }

            if (!context.CanBuildInnerChannelListener<TChannel>())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = GetDictionary(context);

            if (!this.IsFlowEnabled(dictionary))
            {
                return context.BuildInnerChannelListener<TChannel>();
            }

            if (this.issuedTokens == TransactionFlowOption.NotAllowed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TransactionFlowRequiredIssuedTokens)));
            }

            IChannelListener<TChannel> innerListener = context.BuildInnerChannelListener<TChannel>();
            TransactionChannelListener<TChannel> listener = new TransactionChannelListener<TChannel>(this.transactionProtocol, context.Binding, dictionary, innerListener);

            listener.FlowIssuedTokens = this.IssuedTokens;

            return listener;
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (!context.CanBuildInnerChannelListener<TChannel>())
                return false;

            return (typeof(TChannel) == typeof(IInputChannel) ||
                    typeof(TChannel) == typeof(IReplyChannel) ||
                    typeof(TChannel) == typeof(IDuplexChannel) ||
                    typeof(TChannel) == typeof(IInputSessionChannel) ||
                    typeof(TChannel) == typeof(IReplySessionChannel) ||
                    typeof(TChannel) == typeof(IDuplexSessionChannel));
        }

        Dictionary<DirectionalAction, TransactionFlowOption> GetDictionary(BindingContext context)
        {
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary =
                context.BindingParameters.Find<Dictionary<DirectionalAction, TransactionFlowOption>>();
            if (dictionary == null)
                dictionary = new Dictionary<DirectionalAction, TransactionFlowOption>();
            return dictionary;
        }

        internal static MessagePartSpecification GetIssuedTokenHeaderSpecification(SecurityStandardsManager standardsManager)
        {
            MessagePartSpecification result;

            if (standardsManager.TrustDriver.IsIssuedTokensSupported)
                result = new MessagePartSpecification(new XmlQualifiedName(standardsManager.TrustDriver.IssuedTokensHeaderName, standardsManager.TrustDriver.IssuedTokensHeaderNamespace));
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TrustDriverVersionDoesNotSupportIssuedTokens)));
            }

            return result;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(ChannelProtectionRequirements))
            {
                ChannelProtectionRequirements myRequirements = this.GetProtectionRequirements();
                if (myRequirements != null)
                {
                    myRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
                    return (T)(object)myRequirements;
                }
                else
                {
                    return (T)(object)context.GetInnerProperty<ChannelProtectionRequirements>();
                }
            }
            else
            {
                return context.GetInnerProperty<T>();
            }
        }

        ChannelProtectionRequirements GetProtectionRequirements()
        {
            if (this.Transactions || (this.IssuedTokens != TransactionFlowOption.NotAllowed))
            {
                ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
                if (this.Transactions)
                {
                    MessagePartSpecification p = new MessagePartSpecification(
                        new XmlQualifiedName(CoordinationExternalStrings.CoordinationContext, CoordinationExternal10Strings.Namespace),
                        new XmlQualifiedName(CoordinationExternalStrings.CoordinationContext, CoordinationExternal11Strings.Namespace),
                        new XmlQualifiedName(OleTxTransactionExternalStrings.OleTxTransaction, OleTxTransactionExternalStrings.Namespace));
                    p.MakeReadOnly();
                    requirements.IncomingSignatureParts.AddParts(p);
                    requirements.OutgoingSignatureParts.AddParts(p);
                    requirements.IncomingEncryptionParts.AddParts(p);
                    requirements.OutgoingEncryptionParts.AddParts(p);
                }
                if (this.IssuedTokens != TransactionFlowOption.NotAllowed)
                {
                    MessagePartSpecification trustParts = GetIssuedTokenHeaderSpecification(SecurityStandardsManager.DefaultInstance);
                    trustParts.MakeReadOnly();
                    requirements.IncomingSignatureParts.AddParts(trustParts);
                    requirements.IncomingEncryptionParts.AddParts(trustParts);
                    requirements.OutgoingSignatureParts.AddParts(trustParts);
                    requirements.OutgoingEncryptionParts.AddParts(trustParts);
                }

                MessagePartSpecification body = new MessagePartSpecification(true);
                body.MakeReadOnly();
                requirements.OutgoingSignatureParts.AddParts(body, FaultCodeConstants.Actions.Transactions);
                requirements.OutgoingEncryptionParts.AddParts(body, FaultCodeConstants.Actions.Transactions);
                return requirements;
            }
            else
            {
                return null;
            }
        }

        XmlElement GetAssertion(XmlDocument doc, TransactionFlowOption option, string prefix, string name, string ns, string policyNs)
        {
            if (doc == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("doc");

            XmlElement result = null;
            switch (option)
            {
                case TransactionFlowOption.NotAllowed:
                    // Don't generate an assertion
                    break;

                case TransactionFlowOption.Allowed:
                    result = doc.CreateElement(prefix, name, ns);

                    // Always insert the real wsp:Optional attribute
                    XmlAttribute attr = doc.CreateAttribute(TransactionPolicyStrings.OptionalPrefix11,
                        TransactionPolicyStrings.OptionalLocal, policyNs);
                    attr.Value = TransactionPolicyStrings.TrueValue;
                    result.Attributes.Append(attr);

                    // For legacy protocols, also insert the legacy attribute for backward compat
                    if (this.transactionProtocol == TransactionProtocol.OleTransactions ||
                        this.transactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004)
                    {
                        XmlAttribute attrLegacy = doc.CreateAttribute(TransactionPolicyStrings.OptionalPrefix10,
                            TransactionPolicyStrings.OptionalLocal, TransactionPolicyStrings.OptionalNamespaceLegacy);
                        attrLegacy.Value = TransactionPolicyStrings.TrueValue;
                        result.Attributes.Append(attrLegacy);
                    }
                    break;

                case TransactionFlowOption.Mandatory:
                    result = doc.CreateElement(prefix, name, ns);
                    break;
            }
            return result;
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            TransactionFlowBindingElement bindingElement = context.BindingElements.Find<TransactionFlowBindingElement>();
            if (bindingElement == null || !bindingElement.Transactions)
                return;

            XmlDocument doc = new XmlDocument();
            XmlElement assertion = null;

            foreach (OperationDescription operation in context.Contract.Operations)
            {
                TransactionFlowAttribute contextParam = operation.Behaviors.Find<TransactionFlowAttribute>();
                TransactionFlowOption txFlowOption = contextParam == null ? TransactionFlowOption.NotAllowed : contextParam.Transactions;

                // Transactions
                if (bindingElement.TransactionProtocol == TransactionProtocol.OleTransactions)
                {
                    assertion = GetAssertion(doc, txFlowOption,
                                         TransactionPolicyStrings.OleTxTransactionsPrefix, TransactionPolicyStrings.OleTxTransactionsLocal,
                                         TransactionPolicyStrings.OleTxTransactionsNamespace, exporter.PolicyVersion.Namespace);
                }
                else if (bindingElement.TransactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004)
                {
                    assertion = GetAssertion(doc, txFlowOption,
                                         TransactionPolicyStrings.WsatTransactionsPrefix, TransactionPolicyStrings.WsatTransactionsLocal,
                                         TransactionPolicyStrings.WsatTransactionsNamespace10, exporter.PolicyVersion.Namespace);
                }
                else if (bindingElement.TransactionProtocol == TransactionProtocol.WSAtomicTransaction11)
                {
                    assertion = GetAssertion(doc, txFlowOption,
                                         TransactionPolicyStrings.WsatTransactionsPrefix, TransactionPolicyStrings.WsatTransactionsLocal,
                                         TransactionPolicyStrings.WsatTransactionsNamespace11, exporter.PolicyVersion.Namespace);
                }

                if (assertion != null)
                    context.GetOperationBindingAssertions(operation).Add(assertion);
            }

        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
                return false;
            TransactionFlowBindingElement txFlow = b as TransactionFlowBindingElement;
            if (txFlow == null)
                return false;
            if (this.transactions != txFlow.transactions)
                return false;
            if (this.issuedTokens != txFlow.issuedTokens)
                return false;
            if (this.transactionProtocol != txFlow.transactionProtocol)
                return false;

            return true;
        }
    }
}
