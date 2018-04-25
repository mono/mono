//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Xml;

    class ActivityIdHeader : DictionaryHeader
    {
        Guid guid;
        Guid headerId;

        internal ActivityIdHeader(Guid activityId)
            : base()
        {
            this.guid = activityId;
            this.headerId = Guid.NewGuid();
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.ActivityIdFlowDictionary.ActivityId; }
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get { return XD.ActivityIdFlowDictionary.ActivityIdNamespace; }
        }

        internal static Guid ExtractActivityId(Message message)
        {
            Guid guid = Guid.Empty;
            try
            {
                if (message != null && message.State != MessageState.Closed && message.Headers != null)
                {
                    int index = message.Headers.FindHeader(DiagnosticStrings.ActivityId, DiagnosticStrings.DiagnosticsNamespace);

                    // Check the state again, in case the message was closed after we found the header
                    if (index >= 0)
                    {
                        using (XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(index))
                        {
                            guid = reader.ReadElementContentAsGuid();
                        }
                    }
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.FailedToReadAnActivityIdHeader,
                        SR.GetString(SR.TraceCodeFailedToReadAnActivityIdHeader), null, e);
                }
            }

            return guid;
        }

        internal static bool ExtractActivityAndCorrelationId(Message message, out Guid activityId, out Guid correlationId)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            activityId = Guid.Empty;
            correlationId = Guid.Empty;

            try
            {
                if (message.State != MessageState.Closed && message.Headers != null)
                {
                    int index = message.Headers.FindHeader(DiagnosticStrings.ActivityId, DiagnosticStrings.DiagnosticsNamespace);

                    // Check the state again, in case the message was closed after we found the header
                    if (index >= 0)
                    {
                        using (XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(index))
                        {
                            correlationId = Fx.CreateGuid(reader.GetAttribute("CorrelationId", null));
                            activityId = reader.ReadElementContentAsGuid();
                            return activityId != Guid.Empty;
                        }
                    }
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.FailedToReadAnActivityIdHeader,
                        SR.GetString(SR.TraceCodeFailedToReadAnActivityIdHeader), null, e);
                }
            }
            return false;
        }

        internal void AddTo(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (message.State != MessageState.Closed && message.Headers.MessageVersion.Envelope != EnvelopeVersion.None)
            {
                int index = message.Headers.FindHeader(DiagnosticStrings.ActivityId, DiagnosticStrings.DiagnosticsNamespace);
                if (index < 0)
                {
                    message.Headers.Add(this);
                }
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteAttributeString("CorrelationId", this.headerId.ToString());
            writer.WriteValue(this.guid);
        }

    }
}
