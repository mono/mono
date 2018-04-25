//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Transactions;
    using SR2 = System.ServiceModel.Routing.SR;
    using System.Runtime;

    static class RoutingUtilities
    {
        internal const string RoutingNamespace = "http://schemas.microsoft.com/netfx/2009/05/routing";

        internal static void Abort(ICommunicationObject commObj, object identifier)
        {
            if (TD.RoutingServiceAbortingChannelIsEnabled())
            {
                TD.RoutingServiceAbortingChannel(identifier != null ? identifier.ToString() : string.Empty);
            }

            //The Exception contract for ICommunicationObject.Abort is to never throw, anything else is a fatal error.
            commObj.Abort();
        }

        internal static bool IsMessageUsingWSSecurity(UnderstoodHeaders understoodHeaders)
        {
            foreach (MessageHeaderInfo headerInfo in understoodHeaders)
            {
                if (string.Equals(headerInfo.Namespace, "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", StringComparison.Ordinal) || //wsu
                    string.Equals(headerInfo.Namespace, "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", StringComparison.Ordinal) || //wsse
                    string.Equals(headerInfo.Namespace, "http://docs.oasis-open.org/wss/oasis-wss-wsecurity-secext-1.1.xsd", StringComparison.Ordinal)) //wsse11
                {
                    return true;
                }
            }
            return false;
        }

        internal static string GetAddressingNamespace(AddressingVersion addressing)
        {
            string ns;
            if (addressing == AddressingVersion.WSAddressingAugust2004)
            {
                ns = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
            }
            else if (addressing == AddressingVersion.WSAddressing10)
            {
                ns = "http://www.w3.org/2005/08/addressing";
            }
            else if (addressing == AddressingVersion.None)
            {
                ns = "http://schemas.microsoft.com/ws/2005/05/addressing/none";
            }
            else
            {
                throw FxTrace.Exception.Argument("addressing", SR2.AddressingVersionInvalid(addressing.ToString()));
            }
            return ns;
        }

        internal static bool IsRoutingServiceNamespace(string contractNamespace)
        {
            return string.Equals(contractNamespace, RoutingUtilities.RoutingNamespace, StringComparison.Ordinal);
        }

        internal static bool IsTransactedReceive(Binding binding, BindingParameterCollection bindingParameters)
        {
            // New school
            ITransactedBindingElement transactedBindingElement = binding.GetProperty<ITransactedBindingElement>(bindingParameters);
            if (transactedBindingElement != null)
            {
                return transactedBindingElement.TransactedReceiveEnabled;
            }

            // Old School
            foreach (BindingElement element in binding.CreateBindingElements())
            {
                transactedBindingElement = element as ITransactedBindingElement;
                if (transactedBindingElement != null && transactedBindingElement.TransactedReceiveEnabled)
                {
                    return true;
                }
            }
            return false;
        }

        internal static void SafeRollbackTransaction(CommittableTransaction transaction)
        {
            if (transaction != null)
            {
                try
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }
                catch (TransactionException transactionException)
                {
                    if (TD.RoutingServiceHandledExceptionIsEnabled())
                    {
                        TD.RoutingServiceHandledException(null, transactionException);
                    }
                }
                catch (ObjectDisposedException disposedException)
                {
                    if (TD.RoutingServiceHandledExceptionIsEnabled())
                    {
                        TD.RoutingServiceHandledException(null, disposedException);
                    }
                }
            }
        }
    }
}
