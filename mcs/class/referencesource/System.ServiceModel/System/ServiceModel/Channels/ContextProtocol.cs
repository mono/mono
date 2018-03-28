//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Net;
    using System.IO;
    using System.Text;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;

    abstract class ContextProtocol
    {
        ContextExchangeMechanism contextExchangeMechanism;

        protected ContextProtocol(ContextExchangeMechanism contextExchangeMechanism)
        {
            if (!ContextExchangeMechanismHelper.IsDefined(contextExchangeMechanism))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("contextExchangeMechanism"));
            }
            this.contextExchangeMechanism = contextExchangeMechanism;
        }

        protected ContextExchangeMechanism ContextExchangeMechanism
        {
            get { return this.contextExchangeMechanism; }
        }

        public abstract void OnIncomingMessage(Message message);

        public abstract void OnOutgoingMessage(Message message, RequestContext requestContext);

        protected void OnSendSoapContextHeader(Message message, ContextMessageProperty context)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (context.Context.Count > 0)
            {
                message.Headers.Add(new ContextMessageHeader(context.Context));
            }

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.ContextProtocolContextAddedToMessage,
                    SR.GetString(SR.TraceCodeContextProtocolContextAddedToMessage), this);
            }
        }

        internal static class HttpCookieToolbox
        {
            public const string ContextHttpCookieName = "WscContext";
            public const string RemoveContextHttpCookieHeader = ContextHttpCookieName + ";Max-Age=0";

            public static string EncodeContextAsHttpSetCookieHeader(ContextMessageProperty context, Uri uri)
            {
                if (uri == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
                }
                if (context == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
                }

                MemoryStream stream = new MemoryStream();
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.OmitXmlDeclaration = true;
                XmlWriter writer = XmlWriter.Create(stream, writerSettings);
                ContextMessageHeader contextHeader = new ContextMessageHeader(context.Context);
                contextHeader.WriteHeader(writer, MessageVersion.Default);
                writer.Flush();

                string result = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}=\"{1}\";Path={2}",
                    HttpCookieToolbox.ContextHttpCookieName,
                    Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length),
                    uri.AbsolutePath);

                return result;
            }

            public static bool TryCreateFromHttpCookieHeader(string httpCookieHeader, out ContextMessageProperty context)
            {
                if (httpCookieHeader == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("httpCookieHeader");
                }

                context = null;

                foreach (string token in httpCookieHeader.Split(';'))
                {
                    string trimmedToken = token.Trim();
                    if (trimmedToken.StartsWith(HttpCookieToolbox.ContextHttpCookieName, StringComparison.Ordinal))
                    {
                        int equalsSignIndex = trimmedToken.IndexOf('=');
                        if (equalsSignIndex < 0)
                        {
                            context = new ContextMessageProperty();
                            break;
                        }
                        if (equalsSignIndex < (trimmedToken.Length - 1))
                        {
                            string value = trimmedToken.Substring(equalsSignIndex + 1).Trim();

                            if (value.Length > 1 && (value[0] == '"') && (value[value.Length - 1] == '"'))
                            {
                                value = value.Substring(1, value.Length - 2);
                            }
                            try
                            {
                                context = ContextMessageHeader.ParseContextHeader(
                                    XmlReader.Create(new MemoryStream(Convert.FromBase64String(value))));
                                break;
                            }
                            catch (SerializationException e)
                            {
                                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                            }
                            catch (ProtocolException pe)
                            {
                                DiagnosticUtility.TraceHandledException(pe, TraceEventType.Warning);
                            }
                        }
                    }
                }

                return context != null;
            }
        }
    }
}
