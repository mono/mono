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
    using System.Text;

    class DemultiplexingDispatchMessageFormatter : IDispatchMessageFormatter
    {
        IDispatchMessageFormatter defaultFormatter;
        Dictionary<WebContentFormat, IDispatchMessageFormatter> formatters;
        string supportedFormats;

        public DemultiplexingDispatchMessageFormatter(IDictionary<WebContentFormat, IDispatchMessageFormatter> formatters, IDispatchMessageFormatter defaultFormatter)
        {
            if (formatters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("formatters");
            }
            this.formatters = new Dictionary<WebContentFormat, IDispatchMessageFormatter>();
            foreach (WebContentFormat key in formatters.Keys)
            {
                this.formatters.Add(key, formatters[key]);
            }
            this.defaultFormatter = defaultFormatter;
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            if (message == null)
            {
                return;
            }
            WebContentFormat format;
            IDispatchMessageFormatter selectedFormatter;
            if (TryGetEncodingFormat(message, out format))
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
            selectedFormatter.DeserializeRequest(message, parameters);
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.SerializingReplyNotSupportedByFormatter, this)));
        }

        internal static string GetSupportedFormats(IEnumerable<WebContentFormat> formats)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (WebContentFormat format in formats)
            {
                if (i > 0)
                {
                    sb.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                    sb.Append(" ");
                }
                sb.Append("'" + format.ToString() + "'");
                ++i;
            }
            return sb.ToString();
        }

        internal static bool TryGetEncodingFormat(Message message, out WebContentFormat format)
        {
            object prop;
            message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out prop);
            WebBodyFormatMessageProperty formatProperty = prop as WebBodyFormatMessageProperty;
            if (formatProperty == null)
            {
                format = WebContentFormat.Default;
                return false;
            }
            format = formatProperty.Format;
            return true;
        }

        string GetSupportedFormats()
        {
            if (this.supportedFormats == null)
            {
                this.supportedFormats = GetSupportedFormats(this.formatters.Keys);
            }
            return this.supportedFormats;
        }
    }
}

