//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

//Turn on to log info about the before and after headers/messages
//#define DEBUG_MARSHALING

namespace System.ServiceModel.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Security;
    using System.Xml;
    using SR2 = System.ServiceModel.Routing.SR;

    public class SoapProcessingBehavior : IEndpointBehavior
    {
        const string IncomingViaName = "IncomingVia";
        const string IncomingHttpRequestName = "IncomingHttpRequest";

        public SoapProcessingBehavior()
        {
            this.ProcessMessages = true;
        }

        public bool ProcessMessages
        {
            get;
            set;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if (endpoint == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpoint");
            }

            if (this.ProcessMessages)
            {
                SoapProcessingInspector inspector = new SoapProcessingInspector(endpoint, clientRuntime);
                clientRuntime.MessageInspectors.Add(inspector);
                clientRuntime.CallbackDispatchRuntime.MessageInspectors.Add(inspector);
                foreach (ClientOperation clientOp in clientRuntime.Operations)
                {
                    clientOp.ParameterInspectors.Add(inspector);
                }
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException(SR2.MarshalingBehaviorNotSupported));
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        class SoapProcessingInspector : IClientMessageInspector, IDispatchMessageInspector, IParameterInspector
        {
            static HashSet<string> addressingHeadersToFlow = InitializeHeadersToFlow();
            
            bool manualAddressing;
            MessageVersion sourceMessageVersion;
            MessageVersion sendMessageVersion;

            public SoapProcessingInspector(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
                Fx.Assert(endpoint != null, "endpoint cannot be null");
                this.sendMessageVersion = endpoint.Binding.MessageVersion;
                this.manualAddressing = clientRuntime.ManualAddressing;
            }

            void IParameterInspector.AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
            {
                // This call occurs AFTER IClientMessageInspector.AfterReceiveReply.  
                // We don't need to do any additional marshaling here.
            }

            object IParameterInspector.BeforeCall(string operationName, object[] inputs)
            {
                if (inputs == null || inputs.Length == 0)
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("inputs");
                }

                //We have to remove some addressing headers to avoid errors before the 
                //Soap Processing MessageInspector gets called.  See CSDMain 117167.
                Message request = (Message)inputs[0];
                this.PreProcess(request);
                return null;
            }

            void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
            {
                if (reply != null)
                {
                    Message originalMessage = (Message)correlationState;
                    MessageHeaders originalHeaders = originalMessage.Headers;
                    Uri to = originalHeaders.To;
                    MessageVersion version = originalMessage.Version;
                    reply = MarshalMessage(reply, to, version);

                    //BasicHttpContext looks at the original request message when
                    //constructing the Set-Cookie header, need to put these back.
                    Uri incomingVia;
                    MessageProperties originalProperties = originalMessage.Properties;
                    if (originalProperties.TryGetValue<Uri>(IncomingViaName, out incomingVia))
                    {
                        originalProperties.Via = incomingVia;
                    }
                    object incomingHttpRequest;
                    if (originalProperties.TryGetValue(IncomingHttpRequestName, out incomingHttpRequest))
                    {
                        originalProperties[HttpRequestMessageProperty.Name] = incomingHttpRequest;
                    }

                    //Reponses from BasicHttp (Addressing.None) don't have any action.  If we're marshaling
                    //to any addressing version other than none, we create a ReplyAction here
                    MessageHeaders replyHeaders = reply.Headers;
                    if (replyHeaders.Action == null && 
                        version.Addressing != AddressingVersion.None && 
                        originalHeaders.Action != null)
                    {
                        replyHeaders.Action = originalHeaders.Action + "Response";
                    }
                }
            }

            object IClientMessageInspector.BeforeSendRequest(ref Message request, IClientChannel channel)
            {
                Message originalRequest = request;
                if (this.sourceMessageVersion == null)
                {
                    this.sourceMessageVersion = originalRequest.Version;
                }
                request = MarshalMessage(request, request.Headers.To, sendMessageVersion);
                return originalRequest;
            }

            object IDispatchMessageInspector.AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
            {
                request = MarshalMessage(request, request.Headers.To, this.sourceMessageVersion);
                return null;
            }

            void IDispatchMessageInspector.BeforeSendReply(ref Message reply, object correlationState)
            {
                if (reply != null)
                {
                    reply = MarshalMessage(reply, reply.Headers.To, sendMessageVersion);
                }
            }

            static HashSet<string> InitializeHeadersToFlow()
            {
                HashSet<string> headersToFlow = new HashSet<string>(StringComparer.Ordinal);
                headersToFlow.Add("Action");
                headersToFlow.Add("MessageID");
                headersToFlow.Add("RelatesTo");
                headersToFlow.Add("To");
                return headersToFlow;
            }

            void PreProcess(Message message)
            {
                //If the user does not have manual addressing enabled then clear these out
                if (!this.manualAddressing)
                {
                    MessageHeaders headers = message.Headers;
                    string addressingNamespace = RoutingUtilities.GetAddressingNamespace(headers.MessageVersion.Addressing);

                    //Go through in reverse to reduce shifting after RemoveAt(i)
                    for (int i = headers.Count - 1; i >= 0; --i)
                    {
                        MessageHeaderInfo header = headers[i];
                        if (string.Equals(header.Namespace, addressingNamespace, StringComparison.Ordinal))
                        {
                            if (!addressingHeadersToFlow.Contains(header.Name))
                            {
                                headers.RemoveAt(i);
                            }
                        }
                    }
                }
            }

            internal Message MarshalMessage(Message source, Uri to, MessageVersion targetVersion)
            {
                Message result;
                MessageHeaders sourceHeaders = source.Headers;
                MessageVersion sourceVersion = source.Version;
                UnderstoodHeaders understoodHeaders = sourceHeaders.UnderstoodHeaders;
                HashSet<string> understoodHeadersSet = CreateKeys(understoodHeaders);

#if DEBUG_MARSHALING
                System.Text.StringBuilder details = new System.Text.StringBuilder();
                details.AppendFormat("Original Message:\r\n{0}\r\n", source);
                details.AppendLine("Understood Headers:");
                foreach (MessageHeaderInfo understoodHeader in understoodHeaders)
                {
                    details.AppendFormat("\t{0}\t({1})\r\n", understoodHeader.Name, understoodHeader.Namespace);
                }
                details.AppendLine("Properties:");
                foreach (KeyValuePair<string, object> item in source.Properties)
                {
                    details.AppendFormat("\t{0}\t({1})\r\n", item.Key, item.Value);
                }
#endif //DEBUG_MARSHALING


                //if we've understood and verified the security of the message, we need to create a new message
                if (sourceVersion == targetVersion && !RoutingUtilities.IsMessageUsingWSSecurity(understoodHeaders))
                {
                    FilterHeaders(sourceHeaders, understoodHeadersSet);
                    FilterProperties(source.Properties);
                    result = source;
                }
                else
                {
                    if (source.IsFault)
                    {
                        MessageFault messageFault = MessageFault.CreateFault(source, int.MaxValue);
                        string action = sourceHeaders.Action;
                        if (string.Equals(action, sourceVersion.Addressing.DefaultFaultAction, StringComparison.Ordinal))
                        {
                            //The action was the default for the sourceVersion set it to the default for the targetVersion.
                            action = targetVersion.Addressing.DefaultFaultAction;
                        }
                        result = Message.CreateMessage(targetVersion, messageFault, action);
                    }
                    else if (source.IsEmpty)
                    {
                        result = Message.CreateMessage(targetVersion, sourceHeaders.Action);
                    }
                    else
                    {
                        XmlDictionaryReader bodyReader = source.GetReaderAtBodyContents();
                        result = Message.CreateMessage(targetVersion, sourceHeaders.Action, bodyReader);
                    }

                    CloneHeaders(result.Headers, sourceHeaders, to, understoodHeadersSet);
                    CloneProperties(result.Properties, source.Properties);
                }

#if DEBUG_MARSHALING
                details.AppendFormat("\r\nMarshaled Message:\r\n{0}\r\n", result);
                details.AppendLine("Properties:");
                foreach (KeyValuePair<string, object> item in result.Properties)
                {
                    details.AppendFormat("\t{0}\t({1})\r\n", item.Key, item.Value);
                }
                System.Diagnostics.Trace.WriteLine(details);
                TD.RoutingServiceDisplayConfig(details.ToString(), "");
#endif //DEBUG_MARSHALING

                return result;
            }

            static HashSet<string> CreateKeys(UnderstoodHeaders headers)
            {
                HashSet<string> toReturn = new HashSet<string>();
                foreach (MessageHeaderInfo header in headers)
                {
                    toReturn.Add(MessageHeaderKey(header));
                }
                return toReturn;
            }

            static string MessageHeaderKey(MessageHeaderInfo header)
            {
                return header.Name + "+" + header.Namespace;
            }

            void FilterHeaders(MessageHeaders headers, HashSet<string> understoodHeadersSet)
            {
                string addressingNamespace = RoutingUtilities.GetAddressingNamespace(headers.MessageVersion.Addressing);

                //Go in reverse to reduce shifting after RemoveAt(i)
                for (int i = headers.Count - 1; i >= 0; --i)
                {
                    MessageHeaderInfo header = headers[i];
                    bool removeHeader = false;

                    if (string.Equals(header.Namespace, addressingNamespace, StringComparison.Ordinal) &&
                        (addressingHeadersToFlow.Contains(header.Name) || this.manualAddressing))
                    {
                        continue;
                    }

                    if (understoodHeadersSet.Contains(MessageHeaderKey(header)))
                    {
                        // This header was understood at this endpoint, do _not_ flow it
                        removeHeader = true;
                    }
                    else if (ActorIsNextDestination(header, headers.MessageVersion))
                    {
                        //This was a header targeted at the SOAP Intermediary ("actor/next", which is us)
                        //It can explicitly tell us to relay this header when we don't understand it.
                        if (!header.Relay)
                        {
                            removeHeader = true;
                        }
                    }

                    if (removeHeader)
                    {
                        headers.RemoveAt(i);
                    }
                }
            }

            static bool ActorIsNextDestination(MessageHeaderInfo header, MessageVersion messageVersion)
            {
                return (header.Actor != null && string.Equals(header.Actor, messageVersion.Envelope.NextDestinationActorValue));
            }

            void CloneHeaders(MessageHeaders targetHeaders, MessageHeaders sourceHeaders, Uri to, HashSet<string> understoodHeadersSet)
            {
                for (int i = 0; i < sourceHeaders.Count; ++i)
                {
                    MessageHeaderInfo header = sourceHeaders[i];
                    if (!understoodHeadersSet.Contains(MessageHeaderKey(header)))
                    {
                        //If Actor is SOAP Intermediary ("*actor/next" which is us) check the Relay flag
                        if (!ActorIsNextDestination(header, sourceHeaders.MessageVersion) || header.Relay)
                        {
                            //Always wrap the header because BufferedHeader isn't smart enough to allow custom
                            //headers to switch message versions
                            MessageHeader messageHeader = new DelegatingHeader(header, sourceHeaders);
                            targetHeaders.Add(messageHeader);
                        }
                    }
                }

                // To and Action (already specified) are 'special' and may be set even with AddressingVersion.None
                targetHeaders.To = to;
                if (targetHeaders.MessageVersion.Addressing != AddressingVersion.None)
                {
                    //These are used as correlation IDs. Copy these regardless of manual addressing.
                    targetHeaders.MessageId = sourceHeaders.MessageId;
                    targetHeaders.RelatesTo = sourceHeaders.RelatesTo;

                    if (this.manualAddressing)
                    {
                        //These are addresses, only copy when ManualAddressing is enabled
                        targetHeaders.FaultTo = sourceHeaders.FaultTo;
                        targetHeaders.ReplyTo = sourceHeaders.ReplyTo;
                        targetHeaders.From = sourceHeaders.From;
                    }
                }
            }

            static void CloneProperties(MessageProperties destination, MessageProperties source)
            {
                MessageEncoder encoder = destination.Encoder;
                destination.CopyProperties(source);
                destination.Encoder = encoder;
                FilterProperties(destination);
            }

            static void FilterProperties(MessageProperties destination)
            {
                // We have the clear out the HTTP Req/Resp props, so we don't accidentally
                // tell the outbound binding to use whatever Content-Type/Method/QueryString
                // that the inboudnd message/response used.  Otherwise BasicHttp<->WsHttp won't work.
                object incomingHttpRequest;
                if (destination.TryGetValue(HttpRequestMessageProperty.Name, out incomingHttpRequest))
                {
                    //Store the inbound value for later retoration
                    destination[IncomingHttpRequestName] = incomingHttpRequest;
                    destination.Remove(HttpRequestMessageProperty.Name);
                }
                destination.Remove(HttpResponseMessageProperty.Name);

                //Preserve the Via on the outbound message, HTTP Context using cookies looks at this value
                //on the original request when sending the response "Set-Cookie" header.
                Uri incomingVia = destination.Via;
                if (incomingVia != null)
                {
                    destination[IncomingViaName] = incomingVia;
                }
            }
        }
    }
}
