#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Services.Description;
using System.Xml;

#if WEB_EXTENSIONS_CODE
using System.Web.Resources;
#else
using Microsoft.VSDesigner.WCF.Resources;
#endif

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// This class check whether there are duplicated wsdl files in the metadata collection, and report error messages, if any contract is 
    /// defined differently in two wsdl files.
    /// </summary>
    internal class WsdlInspector
    {
        private IList<ProxyGenerationError> importErrors;
        private Dictionary<XmlQualifiedName, PortType> portTypes;
        private Dictionary<XmlQualifiedName, Message> messages;

        /// <summary>
        /// constructor
        /// </summary>
        /// <remarks></remarks>
        private WsdlInspector(IList<ProxyGenerationError> importErrors)
        {
            this.importErrors = importErrors;
            this.portTypes = new Dictionary<XmlQualifiedName, PortType>();
            this.messages = new Dictionary<XmlQualifiedName, Message>();
        }

        /// <summary>
        /// function to check duplicated items
        /// </summary>
        /// <param name="wsdlFiles"></param>
        /// <param name="importErrors"></param>
        /// <remarks></remarks>
        internal static void CheckDuplicatedWsdlItems(ICollection<ServiceDescription> wsdlFiles, IList<ProxyGenerationError> importErrors)
        {
            WsdlInspector inspector = new WsdlInspector(importErrors);
            inspector.CheckServiceDescriptions(wsdlFiles);
        }

        /// <summary>
        /// check all duplicated items in a collection of files
        /// </summary>
        /// <remarks></remarks>
        private void CheckServiceDescriptions(ICollection<ServiceDescription> wsdlFiles)
        {
            foreach (System.Web.Services.Description.ServiceDescription wsdl in wsdlFiles)
            {
                string targetNamespace = wsdl.TargetNamespace;
                if (String.IsNullOrEmpty(targetNamespace))
                {
                    targetNamespace = String.Empty;
                }

                // check all portTypes...
                foreach (PortType portType in wsdl.PortTypes)
                {
                    XmlQualifiedName portTypeName = new XmlQualifiedName(portType.Name, targetNamespace);
                    PortType definedPortType;
                    if (portTypes.TryGetValue(portTypeName, out definedPortType))
                    {
                        MatchPortTypes(definedPortType, portType);
                    }
                    else
                    {
                        portTypes.Add(portTypeName, portType);
                    }
                }

                // check all messages...
                foreach (Message message in wsdl.Messages)
                {
                    XmlQualifiedName messageName = new XmlQualifiedName(message.Name, targetNamespace);
                    Message definedMessage;
                    if (messages.TryGetValue(messageName, out definedMessage))
                    {
                        MatchMessages(definedMessage, message);
                    }
                    else
                    {
                        messages.Add(messageName, message);
                    }
                }
            }
        }

        /// <summary>
        /// Compare two port type (with same targetNamespace/name)
        /// </summary>
        /// <remarks></remarks>
        private void MatchPortTypes(PortType x, PortType y)
        {
            Operation[] operationsX = new Operation[x.Operations.Count];
            x.Operations.CopyTo(operationsX, 0);
            Array.Sort(operationsX, new OperationComparer());

            Operation[] operationsY = new Operation[y.Operations.Count];
            y.Operations.CopyTo(operationsY, 0);
            Array.Sort(operationsY, new OperationComparer());

            MatchCollections<Operation>(operationsX, operationsY,
                    delegate(Operation operationX, Operation operationY)
                    {
                        if (operationX != null && operationY != null)
                        {
                            int nameDifferent = String.Compare(operationX.Name, operationY.Name, StringComparison.Ordinal);
                            if (nameDifferent < 0)
                            {
                                ReportUniqueOperation(operationX, x, y);
                                return false;
                            }
                            else if (nameDifferent > 0)
                            {
                                ReportUniqueOperation(operationY, y, x);
                                return false;
                            }
                            else if (!MatchOperations(operationX, operationY))
                            {
                                return false;
                            }
                            return true;
                        }
                        else if (operationX != null)
                        {
                            ReportUniqueOperation(operationX, x, y);
                            return false;
                        }
                        else if (operationY != null)
                        {
                            ReportUniqueOperation(operationY, y, x);
                            return false;
                        }
                        return true;
                    }
                    );
        }

        /// <summary>
        /// Compare two operations (with same name)
        /// </summary>
        /// <remarks></remarks>
        private bool MatchOperations(Operation x, Operation y)
        {
            if (!MatchOperationMessages(x.Messages.Input, y.Messages.Input))
            {
                ReportOperationDefinedDifferently(x, y);
                return false;
            }
            if (!MatchOperationMessages(x.Messages.Output, y.Messages.Output))
            {
                ReportOperationDefinedDifferently(x, y);
                return false;
            }

            OperationFault[] faultsX = new OperationFault[x.Faults.Count];
            x.Faults.CopyTo(faultsX, 0);
            Array.Sort(faultsX, new OperationFaultComparer());

            OperationFault[] faultsY = new OperationFault[y.Faults.Count];
            y.Faults.CopyTo(faultsY, 0);
            Array.Sort(faultsY, new OperationFaultComparer());

            if (!MatchCollections<OperationFault>(faultsX, faultsY,
                    delegate(OperationFault faultX, OperationFault faultY)
                    {
                        if (faultX != null && faultY != null)
                        {
                            return MatchXmlQualifiedNames(faultX.Message, faultY.Message);
                        }
                        else if (faultX != null || faultY != null)
                        {
                            return false;
                        }
                        return true;
                    }
                    ))
            {
                ReportOperationDefinedDifferently(x, y);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Compare two messages in operations (with same name)
        /// </summary>
        /// <remarks></remarks>
        private bool MatchOperationMessages(OperationMessage x, OperationMessage y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null || y == null)
            {
                return false;
            }
            return MatchXmlQualifiedNames(x.Message, y.Message);
        }

        /// <summary>
        /// Compare two messages defined in wsdl (with same name/targetNamespace)
        /// </summary>
        /// <remarks></remarks>
        private void MatchMessages(Message x, Message y)
        {
            MessagePart[] partsX = new MessagePart[x.Parts.Count];
            x.Parts.CopyTo(partsX, 0);
            Array.Sort(partsX, new MessagePartComparer());

            MessagePart[] partsY = new MessagePart[y.Parts.Count];
            y.Parts.CopyTo(partsY, 0);
            Array.Sort(partsY, new MessagePartComparer());

            MatchCollections<MessagePart>(partsX, partsY,
                    delegate(MessagePart partX, MessagePart partY)
                    {
                        if (partX != null && partY != null)
                        {
                            int nameDifferent = String.Compare(partX.Name, partY.Name, StringComparison.Ordinal);
                            if (nameDifferent < 0)
                            {
                                ReportUniqueMessagePart(partX, x, y);
                                return false;
                            }
                            else if (nameDifferent > 0)
                            {
                                ReportUniqueMessagePart(partY, y, x);
                                return false;
                            }
                            else if (!MatchMessageParts(partX, partY))
                            {
                                return false;
                            }
                            return true;
                        }
                        else if (partX != null)
                        {
                            ReportUniqueMessagePart(partX, x, y);
                            return false;
                        }
                        else if (partY != null)
                        {
                            ReportUniqueMessagePart(partY, y, x);
                            return false;
                        }
                        return true;
                    }
                    );
        }

        /// <summary>
        /// Compare two message parts (with same name)
        /// </summary>
        /// <remarks></remarks>
        private bool MatchMessageParts(MessagePart partX, MessagePart partY)
        {
            if (!MatchXmlQualifiedNames(partX.Type, partY.Type) || !MatchXmlQualifiedNames(partX.Element, partY.Element))
            {
                ReportMessageDefinedDifferently(partX, partX.Message, partY.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// compare two XmlQualifiedName
        /// </summary>
        /// <remarks></remarks>
        private bool MatchXmlQualifiedNames(XmlQualifiedName x, XmlQualifiedName y)
        {
            if (x != null && y != null)
            {
                return x == y; // XmlQualifiedName
            }
            return x == null && y == null;
        }

        /// <summary>
        /// Report an error when we find operation defined in one place but not another
        /// </summary>
        /// <remarks></remarks>
        private void ReportUniqueOperation(Operation operation, PortType portType1, PortType portType2)
        {
            importErrors.Add(new ProxyGenerationError(
                                        ProxyGenerationError.GeneratorState.MergeMetadata,
                                        String.Empty,
                                        new InvalidOperationException(
                                            String.Format(CultureInfo.CurrentCulture, WCFModelStrings.ReferenceGroup_OperationDefinedInOneOfDuplicatedServiceContract,
                                                portType1.Name,
                                                portType1.ServiceDescription.RetrievalUrl,
                                                portType2.ServiceDescription.RetrievalUrl,
                                                operation.Name)
                                        )
                                )
                        );
        }

        /// <summary>
        /// Report an error when we find operation defined in two places differently
        /// </summary>
        /// <remarks></remarks>
        private void ReportOperationDefinedDifferently(Operation x, Operation y)
        {
            importErrors.Add(new ProxyGenerationError(
                                        ProxyGenerationError.GeneratorState.MergeMetadata,
                                        String.Empty,
                                        new InvalidOperationException(
                                            String.Format(CultureInfo.CurrentCulture, WCFModelStrings.ReferenceGroup_OperationDefinedDifferently,
                                                x.Name,
                                                x.PortType.Name,
                                                x.PortType.ServiceDescription.RetrievalUrl,
                                                y.PortType.ServiceDescription.RetrievalUrl)
                                        )
                                )
                            );
        }

        /// <summary>
        /// Report an error when we find a part of message defined in one place but not another
        /// </summary>
        /// <remarks></remarks>
        private void ReportUniqueMessagePart(MessagePart part, Message message1, Message message2)
        {
            importErrors.Add(new ProxyGenerationError(
                                        ProxyGenerationError.GeneratorState.MergeMetadata,
                                        String.Empty,
                                        new InvalidOperationException(
                                            String.Format(CultureInfo.CurrentCulture, WCFModelStrings.ReferenceGroup_FieldDefinedInOneOfDuplicatedMessage,
                                                message1.Name,
                                                message1.ServiceDescription.RetrievalUrl,
                                                message2.ServiceDescription.RetrievalUrl,
                                                part.Name)
                                        )
                                )
                        );
        }

        /// <summary>
        /// Report an error when we find message defined in two places differently
        /// </summary>
        /// <remarks></remarks>
        private void ReportMessageDefinedDifferently(MessagePart part, Message x, Message y)
        {
            importErrors.Add(new ProxyGenerationError(
                                        ProxyGenerationError.GeneratorState.MergeMetadata,
                                        String.Empty,
                                        new InvalidOperationException(
                                            String.Format(CultureInfo.CurrentCulture, WCFModelStrings.ReferenceGroup_FieldDefinedDifferentlyInDuplicatedMessage,
                                                part.Name,
                                                x.Name,
                                                x.ServiceDescription.RetrievalUrl,
                                                y.ServiceDescription.RetrievalUrl)
                                        )
                                )
                            );
        }

        /// <summary>
        /// Helper class to sort Operations
        /// </summary>
        /// <remarks></remarks>
        private class OperationComparer : System.Collections.Generic.IComparer<Operation>
        {

            public int Compare(Operation x, Operation y)
            {
                return String.Compare(x.Name, y.Name, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Helper class to sort OperationFaults
        /// </summary>
        /// <remarks></remarks>
        private class OperationFaultComparer : System.Collections.Generic.IComparer<OperationFault>
        {

            public int Compare(OperationFault x, OperationFault y)
            {
                int namespaceResult = String.Compare(x.Message.Namespace, y.Message.Namespace, StringComparison.Ordinal);
                if (namespaceResult != 0)
                {
                    return namespaceResult;
                }

                return String.Compare(x.Message.Name, y.Message.Name, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Helper class to sort MessageParts
        /// </summary>
        /// <remarks></remarks>
        private class MessagePartComparer : System.Collections.Generic.IComparer<MessagePart>
        {

            public int Compare(MessagePart x, MessagePart y)
            {
                return String.Compare(x.Name, y.Name, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Helper function to compare two collections
        /// </summary>
        /// <remarks></remarks>
        private delegate bool MatchCollectionItemDelegate<T>(T x, T y);
        private bool MatchCollections<T>(T[] x, T[] y, MatchCollectionItemDelegate<T> compareItems) where T : class
        {
            System.Collections.IEnumerator enumeratorX = x.GetEnumerator();
            System.Collections.IEnumerator enumeratorY = y.GetEnumerator();

            T tX;
            T tY;

            do
            {
                tX = enumeratorX.MoveNext() ? (T)enumeratorX.Current : null;
                tY = enumeratorY.MoveNext() ? (T)enumeratorY.Current : null;
                if (tX != null && tY != null)
                {
                    if (!compareItems(tX, tY))
                    {
                        return false;
                    }
                }
            }
            while (tX != null && tY != null);

            return compareItems(tX, tY);
        }

    }
}

