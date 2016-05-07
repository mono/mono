//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;

    public abstract class FaultConverter
    {
        public static FaultConverter GetDefaultFaultConverter(MessageVersion version)
        {
            return new DefaultFaultConverter(version);
        }

        protected abstract bool OnTryCreateException(Message message, MessageFault fault, out Exception exception);
        protected abstract bool OnTryCreateFaultMessage(Exception exception, out Message message);

        public bool TryCreateException(Message message, MessageFault fault, out Exception exception)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (fault == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("fault");
            }

            bool created = this.OnTryCreateException(message, fault, out exception);

            if (created)
            {
                if (exception == null)
                {
                    string text = SR.GetString(SR.FaultConverterDidNotCreateException, this.GetType().Name);
                    Exception error = new InvalidOperationException(text);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
                }
            }
            else
            {
                if (exception != null)
                {
                    string text = SR.GetString(SR.FaultConverterCreatedException, this.GetType().Name);
                    Exception error = new InvalidOperationException(text, exception);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
                }
            }

            return created;
        }

        public bool TryCreateFaultMessage(Exception exception, out Message message)
        {
            bool created = this.OnTryCreateFaultMessage(exception, out message);

            if (created)
            {
                if (message == null)
                {
                    string text = SR.GetString(SR.FaultConverterDidNotCreateFaultMessage, this.GetType().Name);
                    Exception error = new InvalidOperationException(text);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
                }
            }
            else
            {
                if (message != null)
                {
                    string text = SR.GetString(SR.FaultConverterCreatedFaultMessage, this.GetType().Name);
                    Exception error = new InvalidOperationException(text);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(error);
                }
            }

            return created;
        }

        class DefaultFaultConverter : FaultConverter
        {
            MessageVersion version;

            internal DefaultFaultConverter(MessageVersion version)
            {
                this.version = version;
            }

            protected override bool OnTryCreateException(Message message, MessageFault fault, out Exception exception)
            {
                exception = null;

                // SOAP MustUnderstand
                if (string.Compare(fault.Code.Namespace, version.Envelope.Namespace, StringComparison.Ordinal) == 0
                    && string.Compare(fault.Code.Name, MessageStrings.MustUnderstandFault, StringComparison.Ordinal) == 0)
                {
                    exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                    return true;
                }

                bool checkSender;
                bool checkReceiver;
                FaultCode code;

                if (version.Envelope == EnvelopeVersion.Soap11)
                {
                    checkSender = true;
                    checkReceiver = true;
                    code = fault.Code;
                }
                else
                {
                    checkSender = fault.Code.IsSenderFault;
                    checkReceiver = fault.Code.IsReceiverFault;
                    code = fault.Code.SubCode;
                }

                if (code == null)
                {
                    return false;
                }

                if (code.Namespace == null)
                {
                    return false;
                }

                if (checkSender)
                {
                    // WS-Addressing
                    if (string.Compare(code.Namespace, version.Addressing.Namespace, StringComparison.Ordinal) == 0)
                    {
                        if (string.Compare(code.Name, AddressingStrings.ActionNotSupported, StringComparison.Ordinal) == 0)
                        {
                            exception = new ActionNotSupportedException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                            return true;
                        }
                        else if (string.Compare(code.Name, AddressingStrings.DestinationUnreachable, StringComparison.Ordinal) == 0)
                        {
                            exception = new EndpointNotFoundException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                            return true;
                        }
                        else if (string.Compare(code.Name, Addressing10Strings.InvalidAddressingHeader, StringComparison.Ordinal) == 0)
                        {
                            if (code.SubCode != null && string.Compare(code.SubCode.Namespace, version.Addressing.Namespace, StringComparison.Ordinal) == 0 &&
                                string.Compare(code.SubCode.Name, Addressing10Strings.InvalidCardinality, StringComparison.Ordinal) == 0)
                            {
                                exception = new MessageHeaderException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text, true);
                                return true;
                            }
                        }
                        else if (version.Addressing == AddressingVersion.WSAddressing10)
                        {
                            if (string.Compare(code.Name, Addressing10Strings.MessageAddressingHeaderRequired, StringComparison.Ordinal) == 0)
                            {
                                exception = new MessageHeaderException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                                return true;
                            }
                            else if (string.Compare(code.Name, Addressing10Strings.InvalidAddressingHeader, StringComparison.Ordinal) == 0)
                            {
                                exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                                return true;
                            }
                        }
                        else
                        {
                            if (string.Compare(code.Name, Addressing200408Strings.MessageInformationHeaderRequired, StringComparison.Ordinal) == 0)
                            {
                                exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                                return true;
                            }
                            else if (string.Compare(code.Name, Addressing200408Strings.InvalidMessageInformationHeader, StringComparison.Ordinal) == 0)
                            {
                                exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                                return true;
                            }
                        }
                    }
                }

                if (checkReceiver)
                {
                    // WS-Addressing
                    if (string.Compare(code.Namespace, version.Addressing.Namespace, StringComparison.Ordinal) == 0)
                    {
                        if (string.Compare(code.Name, AddressingStrings.EndpointUnavailable, StringComparison.Ordinal) == 0)
                        {
                            exception = new ServerTooBusyException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                            return true;
                        }
                    }
                }

                return false;
            }

            protected override bool OnTryCreateFaultMessage(Exception exception, out Message message)
            {
                // WSA
                if (this.version.Addressing == AddressingVersion.WSAddressing10)
                {
                    if (exception is MessageHeaderException)
                    {
                        MessageHeaderException mhe = exception as MessageHeaderException;
                        if (mhe.HeaderNamespace == AddressingVersion.WSAddressing10.Namespace)
                        {
                            message = mhe.ProvideFault(this.version);
                            return true;
                        }
                    }
                    else if (exception is ActionMismatchAddressingException)
                    {
                        ActionMismatchAddressingException amae = exception as ActionMismatchAddressingException;
                        message = amae.ProvideFault(this.version);
                        return true;
                    }
                }
                if (this.version.Addressing != AddressingVersion.None)
                {
                    if (exception is ActionNotSupportedException)
                    {
                        ActionNotSupportedException anse = exception as ActionNotSupportedException;
                        message = anse.ProvideFault(this.version);
                        return true;
                    }
                }

                // SOAP
                if (exception is MustUnderstandSoapException)
                {
                    MustUnderstandSoapException muse = exception as MustUnderstandSoapException;
                    message = muse.ProvideFault(this.version);
                    return true;
                }

                message = null;
                return false;
            }
        }
    }
}
