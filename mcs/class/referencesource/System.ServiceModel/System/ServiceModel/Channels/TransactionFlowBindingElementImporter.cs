//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;
    using System.Collections.Generic;

    static class TransactionPolicyStrings
    {
        public const string OptionalLocal = MetadataStrings.WSPolicy.Attributes.Optional;
        public const string OptionalPrefix10 = MetadataStrings.WSPolicy.Prefix + "1";
        public const string OptionalPrefix11 = MetadataStrings.WSPolicy.Prefix;
        public const string OptionalNamespaceLegacy = "http://schemas.xmlsoap.org/ws/2002/12/policy";
        public const string WsatTransactionsPrefix = AtomicTransactionExternalStrings.Prefix;
        public const string WsatTransactionsNamespace10 = AtomicTransactionExternal10Strings.Namespace;
        public const string WsatTransactionsNamespace11 = AtomicTransactionExternal11Strings.Namespace;
        public const string WsatTransactionsLocal = "ATAssertion";
        public const string OleTxTransactionsPrefix = OleTxTransactionExternalStrings.Prefix;
        public const string OleTxTransactionsNamespace = OleTxTransactionExternalStrings.Namespace;
        public const string OleTxTransactionsLocal = "OleTxAssertion";
        public const string TrueValue = "true";
    }

    public sealed class TransactionFlowBindingElementImporter : IPolicyImportExtension
    {
        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            // foreach property, we keep track of
            //  - everyoneAgreesAbout:   all operations agree on a value
            //  - anOperationCaresAbout: at least one operation has expressed a preference
            //  - agreed value itself (which only matters if anOperationCaresAbout && everyoneAgrees)
            bool everyoneAgreesAboutTransactions = true;
            bool everyoneAgreesAboutTransactionProtocol = true;
            TransactionFlowOption agreedTransactions = TransactionFlowOption.NotAllowed;
            TransactionProtocol agreedTransactionProtocol = TransactionFlowDefaults.TransactionProtocol;
            bool anOperationCaresAboutTransactions = false;
            bool anOperationCaresAboutTransactionProtocol = false;

            XmlElement oleTxTransactionsAssertion = null;
            XmlElement wsatTransactionsAssertion = null;

            foreach (OperationDescription operation in context.Contract.Operations)
            {
                ICollection<XmlElement> operationAssertions = context.GetOperationBindingAssertions(operation);
                foreach (XmlElement element in operationAssertions)
                {
                    if (element.NamespaceURI == TransactionPolicyStrings.OleTxTransactionsNamespace
                        && element.LocalName == TransactionPolicyStrings.OleTxTransactionsLocal)
                    {
                        oleTxTransactionsAssertion = element;
                        TransactionFlowOption txFlow = GetOption(element, true);
                        UpdateTransactionFlowAtribute(operation, txFlow);

                        TrackAgreement(ref everyoneAgreesAboutTransactions,
                                       txFlow,
                                       ref agreedTransactions,
                                       ref anOperationCaresAboutTransactions);
                        TrackAgreementTransactionProtocol(ref everyoneAgreesAboutTransactionProtocol,
                                           TransactionProtocol.OleTransactions,
                                           ref agreedTransactionProtocol,
                                           ref anOperationCaresAboutTransactionProtocol);
                    }
                    else if (element.NamespaceURI == TransactionPolicyStrings.WsatTransactionsNamespace10
                        && element.LocalName == TransactionPolicyStrings.WsatTransactionsLocal)
                    {
                        wsatTransactionsAssertion = element;
                        TransactionFlowOption txFlow = GetOption(element, true);
                        UpdateTransactionFlowAtribute(operation, txFlow);

                        TrackAgreement(ref everyoneAgreesAboutTransactions,
                                       txFlow,
                                       ref agreedTransactions,
                                       ref anOperationCaresAboutTransactions);
                        TrackAgreementTransactionProtocol(ref everyoneAgreesAboutTransactionProtocol,
                                           TransactionProtocol.WSAtomicTransactionOctober2004,
                                           ref agreedTransactionProtocol,
                                           ref anOperationCaresAboutTransactionProtocol);
                    }
                    else if (element.NamespaceURI == TransactionPolicyStrings.WsatTransactionsNamespace11
                        && element.LocalName == TransactionPolicyStrings.WsatTransactionsLocal)
                    {
                        wsatTransactionsAssertion = element;
                        TransactionFlowOption txFlow = GetOption(element, false);
                        UpdateTransactionFlowAtribute(operation, txFlow);

                        TrackAgreement(ref everyoneAgreesAboutTransactions,
                                       txFlow,
                                       ref agreedTransactions,
                                       ref anOperationCaresAboutTransactions);
                        TrackAgreementTransactionProtocol(ref everyoneAgreesAboutTransactionProtocol,
                                           TransactionProtocol.WSAtomicTransaction11,
                                           ref agreedTransactionProtocol,
                                           ref anOperationCaresAboutTransactionProtocol);
                    }
                }

                // remove any imported assertions.
                if (oleTxTransactionsAssertion != null)
                    operationAssertions.Remove(oleTxTransactionsAssertion);
                if (wsatTransactionsAssertion != null)
                    operationAssertions.Remove(wsatTransactionsAssertion);
            }

            // setup the ContextFlowBindingElement (if needed) with any agreed-on information            
            if (anOperationCaresAboutTransactions)
            {
                TransactionFlowBindingElement tfbe = EnsureBindingElement(context);
                tfbe.Transactions = true;

                if (anOperationCaresAboutTransactionProtocol && everyoneAgreesAboutTransactionProtocol)
                    tfbe.TransactionProtocol = agreedTransactionProtocol;
                else if (anOperationCaresAboutTransactionProtocol)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                        SR.GetString(SR.SFxCannotHaveDifferentTransactionProtocolsInOneBinding)));
            }
        }

        void UpdateTransactionFlowAtribute(OperationDescription operation, TransactionFlowOption txFlow)
        {
            operation.Behaviors.Remove<TransactionFlowAttribute>();
            operation.Behaviors.Add(new TransactionFlowAttribute(txFlow));
        }

        static void TrackAgreement(ref bool everyoneAgrees, TransactionFlowOption option,
                                   ref TransactionFlowOption agreedOption, ref bool anOperationCares)
        {
            if (!anOperationCares)
            {
                // this is the first operation to express a preference
                agreedOption = option;
                anOperationCares = true;
                // everyoneAgrees is still true
            }
            else
            {
                if (option != agreedOption)
                    everyoneAgrees = false;
            }
        }

        static void TrackAgreementTransactionProtocol(ref bool everyoneAgrees, TransactionProtocol option,
                                              ref TransactionProtocol agreedOption, ref bool anOperationCares)
        {
            if (!anOperationCares)
            {
                // this is the first operation to express a preference
                agreedOption = option;
                anOperationCares = true;
                // everyoneAgrees is still true
            }
            else
            {
                if (option != agreedOption)
                    everyoneAgrees = false;
            }
        }

        TransactionFlowOption GetOption(XmlElement elem, bool useLegacyNs)
        {
            try
            {
                if (IsRealOptionalTrue(elem) || (useLegacyNs && IsLegacyOptionalTrue(elem)))
                {
                    return TransactionFlowOption.Allowed;
                }
                return TransactionFlowOption.Mandatory;
            }
            catch (FormatException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                    SR.GetString(SR.UnsupportedBooleanAttribute, TransactionPolicyStrings.OptionalLocal, e.Message)));
            }
        }

        static bool IsRealOptionalTrue(XmlElement elem)
        {
            string value12 = elem.GetAttribute(TransactionPolicyStrings.OptionalLocal, MetadataStrings.WSPolicy.NamespaceUri);
            string value15 = elem.GetAttribute(TransactionPolicyStrings.OptionalLocal, MetadataStrings.WSPolicy.NamespaceUri15);
            return XmlUtil.IsTrue(value12) || XmlUtil.IsTrue(value15);
        }
        static bool IsLegacyOptionalTrue(XmlElement elem)
        {
            string valueLegacy = elem.GetAttribute(TransactionPolicyStrings.OptionalLocal, TransactionPolicyStrings.OptionalNamespaceLegacy);
            return XmlUtil.IsTrue(valueLegacy);
        }

        TransactionFlowBindingElement EnsureBindingElement(PolicyConversionContext context)
        {
            TransactionFlowBindingElement settings = context.BindingElements.Find<TransactionFlowBindingElement>();
            if (settings == null)
            {
                settings = new TransactionFlowBindingElement(false);
                context.BindingElements.Add(settings);
            }
            return settings;
        }

    }
}
