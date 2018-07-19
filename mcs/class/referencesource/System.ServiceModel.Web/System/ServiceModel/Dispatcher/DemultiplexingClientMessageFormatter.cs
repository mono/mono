//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Globalization;

    class DemultiplexingClientMessageFormatter : IClientMessageFormatter
    {
        IClientMessageFormatter defaultFormatter;
        Dictionary<WebContentFormat, IClientMessageFormatter> formatters;
        string supportedFormats;

        public DemultiplexingClientMessageFormatter(IDictionary<WebContentFormat, IClientMessageFormatter> formatters, IClientMessageFormatter defaultFormatter)
        {
            if (formatters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("formatters");
            }
            this.formatters = new Dictionary<WebContentFormat, IClientMessageFormatter>();
            foreach (WebContentFormat key in formatters.Keys)
            {
                this.formatters.Add(key, formatters[key]);
            }
            this.defaultFormatter = defaultFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            if (message == null)
            {
                return null;
            }
            WebContentFormat format;
            IClientMessageFormatter selectedFormatter;
            if (DemultiplexingDispatchMessageFormatter.TryGetEncodingFormat(message, out format))
            {
                this.formatters.TryGetValue(format, out selectedFormatter);
                if (selectedFormatter == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.UnrecognizedHttpMessageFormat, format, GetSupportedFormats())));
                }
            }
            else
            {
                selectedFormatter = this.defaultFormatter;
                if (selectedFormatter == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.MessageFormatPropertyNotFound3)));
                }
            }
            return selectedFormatter.DeserializeReply(message, parameters);
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.SerializingRequestNotSupportedByFormatter, this)));
        }

        string GetSupportedFormats()
        {
            if (this.supportedFormats == null)
            {
                this.supportedFormats = DemultiplexingDispatchMessageFormatter.GetSupportedFormats(this.formatters.Keys);
            }
            return this.supportedFormats;
        }
    }
}

