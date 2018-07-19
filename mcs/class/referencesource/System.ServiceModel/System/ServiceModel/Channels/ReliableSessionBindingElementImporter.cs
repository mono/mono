//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    static class ReliableSessionPolicyStrings
    {
        public const string AcknowledgementInterval = "AcknowledgementInterval";
        public const string AtLeastOnce = "AtLeastOnce";
        public const string AtMostOnce = "AtMostOnce";
        public const string BaseRetransmissionInterval = "BaseRetransmissionInterval";
        public const string DeliveryAssurance = "DeliveryAssurance";
        public const string ExactlyOnce = "ExactlyOnce";
        public const string ExponentialBackoff = "ExponentialBackoff";
        public const string InactivityTimeout = "InactivityTimeout";
        public const string InOrder = "InOrder";
        public const string Milliseconds = "Milliseconds";
        public const string NET11Namespace = "http://schemas.microsoft.com/ws-rx/wsrmp/200702";
        public const string NET11Prefix = "netrmp";
        public const string ReliableSessionName = "RMAssertion";
        public const string ReliableSessionFebruary2005Namespace = "http://schemas.xmlsoap.org/ws/2005/02/rm/policy";
        public const string ReliableSessionFebruary2005Prefix = "wsrm";
        public const string ReliableSession11Namespace = "http://docs.oasis-open.org/ws-rx/wsrmp/200702";
        public const string ReliableSession11Prefix = "wsrmp";
        public const string SequenceSTR = "SequenceSTR";
        public const string SequenceTransportSecurity = "SequenceTransportSecurity";
    }

    public sealed class ReliableSessionBindingElementImporter : IPolicyImportExtension
    {
        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            bool gotAssertion = false;

            XmlElement reliableSessionAssertion = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(),
                ReliableSessionPolicyStrings.ReliableSessionName,
                ReliableSessionPolicyStrings.ReliableSessionFebruary2005Namespace, true);

            if (reliableSessionAssertion != null)
            {
                ProcessReliableSessionFeb2005Assertion(reliableSessionAssertion, GetReliableSessionBindingElement(context));
                gotAssertion = true;
            }

            reliableSessionAssertion = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(),
                ReliableSessionPolicyStrings.ReliableSessionName,
                ReliableSessionPolicyStrings.ReliableSession11Namespace, true);

            if (reliableSessionAssertion != null)
            {
                if (gotAssertion)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(
                        SR.GetString(SR.MultipleVersionsFoundInPolicy,
                        ReliableSessionPolicyStrings.ReliableSessionName)));
                }

                ProcessReliableSession11Assertion(importer, reliableSessionAssertion,
                    GetReliableSessionBindingElement(context));
            }
        }

        static ReliableSessionBindingElement GetReliableSessionBindingElement(PolicyConversionContext context)
        {
            ReliableSessionBindingElement settings = context.BindingElements.Find<ReliableSessionBindingElement>();

            if (settings == null)
            {
                settings = new ReliableSessionBindingElement();
                context.BindingElements.Add(settings);
            }

            return settings;
        }

        static bool Is11Assertion(XmlNode node, string assertion)
        {
            return IsElement(node, ReliableSessionPolicyStrings.NET11Namespace, assertion);
        }

        static bool IsElement(XmlNode node, string ns, string assertion)
        {
            if (assertion == null)
            {
                throw Fx.AssertAndThrow("Argument assertion cannot be null.");
            }

            return ((node != null)
                && (node.NodeType == XmlNodeType.Element)
                && (node.NamespaceURI == ns)
                && (node.LocalName == assertion));
        }

        static bool IsFeb2005Assertion(XmlNode node, string assertion)
        {
            return IsElement(node, ReliableSessionPolicyStrings.ReliableSessionFebruary2005Namespace, assertion);
        }

        static void ProcessReliableSession11Assertion(MetadataImporter importer, XmlElement element,
            ReliableSessionBindingElement settings)
        {
            // Version
            settings.ReliableMessagingVersion = ReliableMessagingVersion.WSReliableMessaging11;

            IEnumerator assertionChildren = element.ChildNodes.GetEnumerator();
            XmlNode currentNode = SkipToNode(assertionChildren);

            // Policy
            ProcessWsrm11Policy(importer, currentNode, settings);
            currentNode = SkipToNode(assertionChildren);

            // Looking for:
            // InactivityTimeout, AcknowledgementInterval
            // InactivityTimeout
            // AcknowledgementInterval
            // or nothing at all.
            State state = State.InactivityTimeout;
            while (currentNode != null)
            {
                if (state == State.InactivityTimeout)
                {
                    // InactivityTimeout assertion
                    if (Is11Assertion(currentNode, ReliableSessionPolicyStrings.InactivityTimeout))
                    {
                        SetInactivityTimeout(settings, ReadMillisecondsAttribute(currentNode, true), currentNode.LocalName);
                        state = State.AcknowledgementInterval;
                        currentNode = SkipToNode(assertionChildren);
                        continue;
                    }
                }

                // AcknowledgementInterval assertion
                if (Is11Assertion(currentNode, ReliableSessionPolicyStrings.AcknowledgementInterval))
                {
                    SetAcknowledgementInterval(settings, ReadMillisecondsAttribute(currentNode, true), currentNode.LocalName);

                    // ignore the rest
                    break;
                }

                if (state == State.AcknowledgementInterval)
                {
                    // ignore the rest
                    break;
                }

                currentNode = SkipToNode(assertionChildren);
            }

            // Schema allows arbitrary elements from now on, ignore everything else
        }

        static void ProcessReliableSessionFeb2005Assertion(XmlElement element, ReliableSessionBindingElement settings)
        {
            // Version
            settings.ReliableMessagingVersion = ReliableMessagingVersion.WSReliableMessagingFebruary2005;

            IEnumerator nodes = element.ChildNodes.GetEnumerator();
            XmlNode currentNode = SkipToNode(nodes);

            // InactivityTimeout assertion
            if (IsFeb2005Assertion(currentNode, ReliableSessionPolicyStrings.InactivityTimeout))
            {
                SetInactivityTimeout(settings, ReadMillisecondsAttribute(currentNode, true), currentNode.LocalName);
                currentNode = SkipToNode(nodes);
            }

            // BaseRetransmissionInterval assertion is read but ignored
            if (IsFeb2005Assertion(currentNode, ReliableSessionPolicyStrings.BaseRetransmissionInterval))
            {
                ReadMillisecondsAttribute(currentNode, false);
                currentNode = SkipToNode(nodes);
            }

            // ExponentialBackoff assertion is read but ignored
            if (IsFeb2005Assertion(currentNode, ReliableSessionPolicyStrings.ExponentialBackoff))
            {
                currentNode = SkipToNode(nodes);
            }

            // AcknowledgementInterval assertion
            if (IsFeb2005Assertion(currentNode, ReliableSessionPolicyStrings.AcknowledgementInterval))
            {
                SetAcknowledgementInterval(settings, ReadMillisecondsAttribute(currentNode, true), currentNode.LocalName);
            }

            // Schema allows arbitrary elements from now on, ignore everything else
        }

        static void ProcessWsrm11Policy(MetadataImporter importer, XmlNode node, ReliableSessionBindingElement settings)
        {
            XmlElement element = ThrowIfNotPolicyElement(node, ReliableMessagingVersion.WSReliableMessaging11);
            IEnumerable<IEnumerable<XmlElement>> alternatives = importer.NormalizePolicy(new XmlElement[] { element });
            List<Wsrm11PolicyAlternative> wsrmAlternatives = new List<Wsrm11PolicyAlternative>();

            foreach (IEnumerable<XmlElement> alternative in alternatives)
            {
                Wsrm11PolicyAlternative wsrm11Policy = Wsrm11PolicyAlternative.ImportAlternative(importer, alternative);
                wsrmAlternatives.Add(wsrm11Policy);
            }

            if (wsrmAlternatives.Count == 0)
            {
                // No specific policy other than turn on WS-RM.
                return;
            }

            foreach (Wsrm11PolicyAlternative wsrmAlternative in wsrmAlternatives)
            {
                // The only policy setting that affects the binding is the InOrder assurance.
                // Even that setting does not affect the binding since InOrder is a server delivery assurance.
                // Transfer any that is valid.
                if (wsrmAlternative.HasValidPolicy)
                {
                    wsrmAlternative.TransferSettings(settings);
                    return;
                }
            }

            // Found only invalid policy.
            // This throws an exception about security since that is the only invalid policy we have.
            Wsrm11PolicyAlternative.ThrowInvalidBindingException();
        }

        static TimeSpan ReadMillisecondsAttribute(XmlNode wsrmNode, bool convertToTimeSpan)
        {
            XmlAttribute millisecondsAttribute = wsrmNode.Attributes[ReliableSessionPolicyStrings.Milliseconds];
            if (millisecondsAttribute == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(SR.GetString(SR.RequiredAttributeIsMissing, ReliableSessionPolicyStrings.Milliseconds, wsrmNode.LocalName, ReliableSessionPolicyStrings.ReliableSessionName)));

            UInt64 milliseconds = 0;
            Exception innerException = null;

            try
            {
                milliseconds = XmlConvert.ToUInt64(millisecondsAttribute.Value);
            }
            catch (FormatException exception)
            {
                innerException = exception;
            }
            catch (OverflowException exception)
            {
                innerException = exception;
            }

            if (innerException != null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(SR.GetString(SR.RequiredMillisecondsAttributeIncorrect, wsrmNode.LocalName), innerException));

            if (convertToTimeSpan)
            {
                TimeSpan interval;

                try
                {
                    interval = TimeSpan.FromMilliseconds(Convert.ToDouble(milliseconds));
                }
                catch (OverflowException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(SR.GetString(SR.MillisecondsNotConvertibleToBindingRange, wsrmNode.LocalName), exception));
                }

                return interval;
            }
            else
            {
                return default(TimeSpan);
            }
        }

        static void SetInactivityTimeout(ReliableSessionBindingElement settings, TimeSpan inactivityTimeout, string localName)
        {
            try
            {
                settings.InactivityTimeout = inactivityTimeout;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(
                    SR.GetString(SR.MillisecondsNotConvertibleToBindingRange, localName), exception));
            }
        }

        static void SetAcknowledgementInterval(ReliableSessionBindingElement settings, TimeSpan acknowledgementInterval, string localName)
        {
            try
            {
                settings.AcknowledgementInterval = acknowledgementInterval;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(SR.GetString(SR.MillisecondsNotConvertibleToBindingRange, localName), exception));
            }
        }

        static bool ShouldSkipNodeType(XmlNodeType type)
        {
            return (type == XmlNodeType.Comment || type == XmlNodeType.SignificantWhitespace || type == XmlNodeType.Whitespace || type == XmlNodeType.Notation);
        }

        static XmlNode SkipToNode(IEnumerator nodes)
        {
            while (nodes.MoveNext())
            {
                XmlNode currentNode = (XmlNode)nodes.Current;

                if (ShouldSkipNodeType(currentNode.NodeType))
                    continue;

                return currentNode;
            }

            return null;
        }

        static XmlElement ThrowIfNotPolicyElement(XmlNode node, ReliableMessagingVersion reliableMessagingVersion)
        {
            string policyLocalName = MetadataStrings.WSPolicy.Elements.Policy;

            if (!IsElement(node, MetadataStrings.WSPolicy.NamespaceUri, policyLocalName)
                && !IsElement(node, MetadataStrings.WSPolicy.NamespaceUri15, policyLocalName))
            {
                string wsrmPrefix = (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                    ? ReliableSessionPolicyStrings.ReliableSessionFebruary2005Prefix
                    : ReliableSessionPolicyStrings.ReliableSession11Prefix;

                string exceptionString = (node == null)
                    ? SR.GetString(SR.ElementRequired, wsrmPrefix,
                    ReliableSessionPolicyStrings.ReliableSessionName, MetadataStrings.WSPolicy.Prefix,
                    MetadataStrings.WSPolicy.Elements.Policy)
                    : SR.GetString(SR.ElementFound, wsrmPrefix,
                    ReliableSessionPolicyStrings.ReliableSessionName, MetadataStrings.WSPolicy.Prefix,
                    MetadataStrings.WSPolicy.Elements.Policy, node.LocalName, node.NamespaceURI);

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(exceptionString));
            }

            return (XmlElement)node;
        }

        class Wsrm11PolicyAlternative
        {
            bool hasValidPolicy = true;
            bool isOrdered = false;

            public bool HasValidPolicy
            {
                get
                {
                    return this.hasValidPolicy;
                }
            }

            public static Wsrm11PolicyAlternative ImportAlternative(MetadataImporter importer,
                IEnumerable<XmlElement> alternative)
            {
                State state = State.Security;
                Wsrm11PolicyAlternative wsrmPolicy = new Wsrm11PolicyAlternative();

                foreach (XmlElement node in alternative)
                {
                    if (state == State.Security)
                    {
                        state = State.DeliveryAssurance;

                        if (wsrmPolicy.TryImportSequenceSTR(node))
                        {
                            continue;
                        }
                    }

                    if (state == State.DeliveryAssurance)
                    {
                        state = State.Done;

                        if (wsrmPolicy.TryImportDeliveryAssurance(importer, node))
                        {
                            continue;
                        }
                    }

                    string exceptionString = SR.GetString(SR.UnexpectedXmlChildNode,
                        node.LocalName,
                        node.NodeType,
                        ReliableSessionPolicyStrings.ReliableSessionName);

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(exceptionString));
                }

                return wsrmPolicy;
            }

            public static void ThrowInvalidBindingException()
            {
                string exceptionString = SR.GetString(SR.AssertionNotSupported,
                    ReliableSessionPolicyStrings.ReliableSession11Prefix,
                    ReliableSessionPolicyStrings.SequenceTransportSecurity);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(exceptionString));
            }

            public void TransferSettings(ReliableSessionBindingElement settings)
            {
                settings.Ordered = this.isOrdered;
            }

            bool TryImportSequenceSTR(XmlElement node)
            {
                string wsrmNs = ReliableSessionPolicyStrings.ReliableSession11Namespace;

                if (IsElement(node, wsrmNs, ReliableSessionPolicyStrings.SequenceSTR))
                {
                    return true;
                }

                if (IsElement(node, wsrmNs, ReliableSessionPolicyStrings.SequenceTransportSecurity))
                {
                    this.hasValidPolicy = false;
                    return true;
                }

                return false;
            }

            bool TryImportDeliveryAssurance(MetadataImporter importer, XmlElement node)
            {
                string wsrmNs = ReliableSessionPolicyStrings.ReliableSession11Namespace;

                if (!IsElement(node, wsrmNs, ReliableSessionPolicyStrings.DeliveryAssurance))
                {
                    return false;
                }

                // Policy
                IEnumerator policyNodes = node.ChildNodes.GetEnumerator();
                XmlNode policyNode = SkipToNode(policyNodes);
                XmlElement policyElement = ThrowIfNotPolicyElement(policyNode, ReliableMessagingVersion.WSReliableMessaging11);
                IEnumerable<IEnumerable<XmlElement>> alternatives = importer.NormalizePolicy(new XmlElement[] { policyElement });

                foreach (IEnumerable<XmlElement> alternative in alternatives)
                {
                    State state = State.Assurance;

                    foreach (XmlElement element in alternative)
                    {
                        if (state == State.Assurance)
                        {
                            state = State.Order;

                            if (!IsElement(element, wsrmNs, ReliableSessionPolicyStrings.ExactlyOnce)
                                && !IsElement(element, wsrmNs, ReliableSessionPolicyStrings.AtMostOnce)
                                && !IsElement(element, wsrmNs, ReliableSessionPolicyStrings.AtMostOnce))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(SR.GetString(
                                    SR.DeliveryAssuranceRequired,
                                    wsrmNs,
                                    element.LocalName,
                                    element.NamespaceURI)));
                            }

                            // Found required DeliveryAssurance, ignore the value and skip to InOrder
                            continue;
                        }

                        if (state == State.Order)
                        {
                            state = State.Done;

                            // InOrder
                            if (IsElement(element, wsrmNs, ReliableSessionPolicyStrings.InOrder))
                            {
                                // set ordered
                                if (!this.isOrdered)
                                {
                                    this.isOrdered = true;
                                }

                                continue;
                            }
                        }

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(SR.GetString(
                            SR.UnexpectedXmlChildNode,
                            element.LocalName,
                            element.NodeType,
                            ReliableSessionPolicyStrings.DeliveryAssurance)));
                    }

                    if (state == State.Assurance)
                    {
                        string exceptionString = SR.GetString(SR.DeliveryAssuranceRequiredNothingFound, wsrmNs);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(exceptionString));
                    }
                }

                policyNode = SkipToNode(policyNodes);

                if (policyNode != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(SR.GetString(
                        SR.UnexpectedXmlChildNode,
                        policyNode.LocalName,
                        policyNode.NodeType,
                        node.LocalName)));
                }

                return true;
            }
        }

        enum State
        {
            Security,
            DeliveryAssurance,
            Assurance,
            Order,
            InactivityTimeout,
            AcknowledgementInterval,
            Done,
        }
    }
}
