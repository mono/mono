//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    public sealed class EtwTrackingParticipant : TrackingParticipant
    {
        const string truncatedItemsTag = "<items>...</items>";
        const string emptyItemsTag = "<items />";
        const string itemsTag = "items";
        const string itemTag = "item";
        const string nameAttribute = "name";
        const string typeAttribute = "type";

        static Hashtable diagnosticTraceCache = new Hashtable();
        NetDataContractSerializer variableSerializer;
        EtwDiagnosticTrace diagnosticTrace;
        Guid etwProviderId;

        // This constructor requires UnmanagedCode permission. It is demanded in InitializeEtwTrackingProvider, which
        // is called from set_EtwProviderId, which this calls.
        public EtwTrackingParticipant()
        {
            if (EtwDiagnosticTrace.DefaultEtwProviderId == Guid.Empty)
            {
                this.EtwProviderId = EtwDiagnosticTrace.ImmutableDefaultEtwProviderId;
            }
            else
            {
                this.EtwProviderId = EtwDiagnosticTrace.DefaultEtwProviderId;
            }
            this.ApplicationReference = string.Empty;
        }

        [Fx.Tag.KnownXamlExternal]
        public Guid EtwProviderId
        {
            get
            {
                return this.etwProviderId;
            }
            // This requires UnmanagedCode permission. It is demanded in InitializeEtwTrackingProvider.
            set
            {
                if (value == Guid.Empty)
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("value");
                }
                InitializeEtwTrackingProvider(value);
            }
        }

        public string ApplicationReference
        {
            get;

            set;
        }

        protected internal override IAsyncResult BeginTrack(TrackingRecord record, TimeSpan timeout, AsyncCallback callback, object state)
        {
            Track(record, timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected internal override void EndTrack(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected internal override void Track(TrackingRecord record, TimeSpan timeout)
        {
            if (this.diagnosticTrace.IsEtwProviderEnabled)
            {
                if (record is ActivityStateRecord)
                {
                    TrackActivityRecord((ActivityStateRecord)record);
                }
                else if (record is WorkflowInstanceRecord)
                {
                    TrackWorkflowRecord((WorkflowInstanceRecord)record);
                }
                else if (record is BookmarkResumptionRecord)
                {
                    TrackBookmarkRecord((BookmarkResumptionRecord)record);
                }
                else if (record is ActivityScheduledRecord)
                {
                    TrackActivityScheduledRecord((ActivityScheduledRecord)record);
                }
                else if (record is CancelRequestedRecord)
                {
                    TrackCancelRequestedRecord((CancelRequestedRecord)record);
                }
                else if (record is FaultPropagationRecord)
                {
                    TrackFaultPropagationRecord((FaultPropagationRecord)record);
                }
                else
                {
                    Fx.Assert(record is CustomTrackingRecord, "Expected only CustomTrackingRecord");
                    TrackCustomRecord((CustomTrackingRecord)record);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls EtwDiagonsticTrace.ctor with a provider id, which is SecurityCritical",
            Safe = "We demand UnmanagedCode.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        void InitializeEtwTrackingProvider(Guid providerId)
        {
            this.diagnosticTrace = (EtwDiagnosticTrace)diagnosticTraceCache[providerId];
            if (this.diagnosticTrace == null)
            {
                lock (diagnosticTraceCache)
                {
                    this.diagnosticTrace = (EtwDiagnosticTrace)diagnosticTraceCache[providerId];
                    if (this.diagnosticTrace == null)
                    {
                        this.diagnosticTrace = new EtwDiagnosticTrace(null, providerId);
                        diagnosticTraceCache.Add(providerId, this.diagnosticTrace);
                    }
                }
            }

            this.etwProviderId = providerId;
        }

        string PrepareDictionary(IDictionary<string, object> data)
        {
            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartElement(itemsTag);

                if (data != null)
                {
                    foreach (KeyValuePair<string, object> item in data)
                    {
                        writer.WriteStartElement(itemTag);
                        writer.WriteAttributeString(nameAttribute, item.Key);
                        if (item.Value == null)
                        {
                            writer.WriteAttributeString(typeAttribute, string.Empty);
                            writer.WriteValue(string.Empty);
                        }
                        else
                        {
                            Type valueType = item.Value.GetType();
                            writer.WriteAttributeString(typeAttribute, valueType.FullName);

                            if (valueType == typeof(int) || valueType == typeof(float) || valueType == typeof(double) ||
                                valueType == typeof(long) || valueType == typeof(bool) || valueType == typeof(uint) ||
                                valueType == typeof(ushort) || valueType == typeof(short) || valueType == typeof(ulong) ||
                                valueType == typeof(string) || valueType == typeof(DateTimeOffset))
                            {
                                writer.WriteValue(item.Value);
                            }
                            else if (valueType == typeof(Guid))
                            {
                                Guid value = (Guid)item.Value;
                                writer.WriteValue(value.ToString());
                            }
                            else if (valueType == typeof(DateTime))
                            {
                                DateTime date = ((DateTime)item.Value).ToUniversalTime();
                                writer.WriteValue(date);
                            }
                            else
                            {
                                if (this.variableSerializer == null)
                                {
                                    this.variableSerializer = new NetDataContractSerializer();
                                }

                                try
                                {
                                    this.variableSerializer.WriteObject(writer, item.Value);
                                }
                                catch (Exception e)
                                {
                                    if (Fx.IsFatal(e))
                                    {
                                        throw;
                                    }
                                    TraceItemNotSerializable(item.Key, e);
                                }
                            }
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
                writer.Flush();
                return builder.ToString();
            }
        }

        static string PrepareAnnotations(IDictionary<string, string> data)
        {
            string stringTypeName = typeof(string).FullName;

            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartElement(itemsTag);

                if (data != null)
                {
                    foreach (KeyValuePair<string, string> item in data)
                    {
                        writer.WriteStartElement(itemTag);
                        writer.WriteAttributeString(nameAttribute, item.Key);
                        writer.WriteAttributeString(typeAttribute, stringTypeName);
                        if (item.Value == null)
                        {
                            writer.WriteValue(string.Empty);
                        }
                        else
                        {
                            writer.WriteValue(item.Value);
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
                writer.Flush();
                return builder.ToString();
            }
        }

        static void TraceItemNotSerializable(string item, Exception e)
        {
            //trace the exception. 
            FxTrace.Exception.AsInformation(e);

            if (TD.TrackingValueNotSerializableIsEnabled())
            {
                TD.TrackingValueNotSerializable(item);
            }
        }

        void TraceTrackingRecordDropped(long recordNumber)
        {
            if (TD.TrackingRecordDroppedIsEnabled())
            {
                TD.TrackingRecordDropped(recordNumber, this.EtwProviderId);
            }
        }

        void TraceTrackingRecordTruncated(long recordNumber)
        {
            if (TD.TrackingRecordTruncatedIsEnabled())
            {
                TD.TrackingRecordTruncated(recordNumber, this.EtwProviderId);
            }
        }

        void TrackActivityRecord(ActivityStateRecord record)
        {
            if (EtwTrackingParticipantTrackRecords.ActivityStateRecordIsEnabled(this.diagnosticTrace))
            {
                if (!EtwTrackingParticipantTrackRecords.ActivityStateRecord(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId,
                    record.RecordNumber, record.EventTime.ToFileTime(), record.State,
                    record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName,
                    record.Arguments.Count > 0 ? PrepareDictionary(record.Arguments) : emptyItemsTag,
                    record.Variables.Count > 0 ? PrepareDictionary(record.Variables) : emptyItemsTag,
                    record.HasAnnotations ? PrepareAnnotations(record.Annotations) : emptyItemsTag,
                    this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    if (EtwTrackingParticipantTrackRecords.ActivityStateRecord(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId,
                        record.RecordNumber, record.EventTime.ToFileTime(), record.State,
                        record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName, truncatedItemsTag, truncatedItemsTag,
                        truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        TraceTrackingRecordTruncated(record.RecordNumber);
                    }
                    else
                    {
                        TraceTrackingRecordDropped(record.RecordNumber);
                    }
                }
            }
        }


        void TrackActivityScheduledRecord(ActivityScheduledRecord scheduledRecord)
        {
            if (EtwTrackingParticipantTrackRecords.ActivityScheduledRecordIsEnabled(this.diagnosticTrace))
            {
                if (!EtwTrackingParticipantTrackRecords.ActivityScheduledRecord(this.diagnosticTrace, scheduledRecord.EventTraceActivity, scheduledRecord.InstanceId,
                    scheduledRecord.RecordNumber,
                    scheduledRecord.EventTime.ToFileTime(),
                    scheduledRecord.Activity == null ? string.Empty : scheduledRecord.Activity.Name,
                    scheduledRecord.Activity == null ? string.Empty : scheduledRecord.Activity.Id,
                    scheduledRecord.Activity == null ? string.Empty : scheduledRecord.Activity.InstanceId,
                    scheduledRecord.Activity == null ? string.Empty : scheduledRecord.Activity.TypeName,
                    scheduledRecord.Child.Name, scheduledRecord.Child.Id, scheduledRecord.Child.InstanceId, scheduledRecord.Child.TypeName,
                    scheduledRecord.HasAnnotations ? PrepareAnnotations(scheduledRecord.Annotations) : emptyItemsTag,
                    this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    if (EtwTrackingParticipantTrackRecords.ActivityScheduledRecord(this.diagnosticTrace, scheduledRecord.EventTraceActivity, scheduledRecord.InstanceId,
                        scheduledRecord.RecordNumber,
                        scheduledRecord.EventTime.ToFileTime(),
                        scheduledRecord.Activity == null ? string.Empty : scheduledRecord.Activity.Name,
                        scheduledRecord.Activity == null ? string.Empty : scheduledRecord.Activity.Id,
                        scheduledRecord.Activity == null ? string.Empty : scheduledRecord.Activity.InstanceId,
                        scheduledRecord.Activity == null ? string.Empty : scheduledRecord.Activity.TypeName,
                        scheduledRecord.Child.Name, scheduledRecord.Child.Id, scheduledRecord.Child.InstanceId, scheduledRecord.Child.TypeName,
                        truncatedItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        TraceTrackingRecordTruncated(scheduledRecord.RecordNumber);
                    }
                    else
                    {
                        TraceTrackingRecordDropped(scheduledRecord.RecordNumber);
                    }
                }
            }
        }

        void TrackCancelRequestedRecord(CancelRequestedRecord cancelRecord)
        {
            if (EtwTrackingParticipantTrackRecords.CancelRequestedRecordIsEnabled(this.diagnosticTrace))
            {
                if (!EtwTrackingParticipantTrackRecords.CancelRequestedRecord(this.diagnosticTrace, cancelRecord.EventTraceActivity, cancelRecord.InstanceId,
                    cancelRecord.RecordNumber,
                    cancelRecord.EventTime.ToFileTime(),
                    cancelRecord.Activity == null ? string.Empty : cancelRecord.Activity.Name,
                    cancelRecord.Activity == null ? string.Empty : cancelRecord.Activity.Id,
                    cancelRecord.Activity == null ? string.Empty : cancelRecord.Activity.InstanceId,
                    cancelRecord.Activity == null ? string.Empty : cancelRecord.Activity.TypeName,
                    cancelRecord.Child.Name, cancelRecord.Child.Id, cancelRecord.Child.InstanceId, cancelRecord.Child.TypeName,
                    cancelRecord.HasAnnotations ? PrepareAnnotations(cancelRecord.Annotations) : emptyItemsTag,
                    this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    if (EtwTrackingParticipantTrackRecords.CancelRequestedRecord(this.diagnosticTrace, cancelRecord.EventTraceActivity, cancelRecord.InstanceId,
                        cancelRecord.RecordNumber,
                        cancelRecord.EventTime.ToFileTime(),
                        cancelRecord.Activity == null ? string.Empty : cancelRecord.Activity.Name,
                        cancelRecord.Activity == null ? string.Empty : cancelRecord.Activity.Id,
                        cancelRecord.Activity == null ? string.Empty : cancelRecord.Activity.InstanceId,
                        cancelRecord.Activity == null ? string.Empty : cancelRecord.Activity.TypeName,
                        cancelRecord.Child.Name, cancelRecord.Child.Id, cancelRecord.Child.InstanceId, cancelRecord.Child.TypeName,
                        truncatedItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        TraceTrackingRecordTruncated(cancelRecord.RecordNumber);
                    }
                    else
                    {
                        TraceTrackingRecordDropped(cancelRecord.RecordNumber);
                    }
                }
            }
        }

        void TrackFaultPropagationRecord(FaultPropagationRecord faultRecord)
        {
            if (EtwTrackingParticipantTrackRecords.FaultPropagationRecordIsEnabled(this.diagnosticTrace))
            {
                if (!EtwTrackingParticipantTrackRecords.FaultPropagationRecord(this.diagnosticTrace, faultRecord.EventTraceActivity, faultRecord.InstanceId,
                    faultRecord.RecordNumber,
                    faultRecord.EventTime.ToFileTime(),
                    faultRecord.FaultSource.Name, faultRecord.FaultSource.Id, faultRecord.FaultSource.InstanceId, faultRecord.FaultSource.TypeName,
                    faultRecord.FaultHandler != null ? faultRecord.FaultHandler.Name : string.Empty,
                    faultRecord.FaultHandler != null ? faultRecord.FaultHandler.Id : string.Empty,
                    faultRecord.FaultHandler != null ? faultRecord.FaultHandler.InstanceId : string.Empty,
                    faultRecord.FaultHandler != null ? faultRecord.FaultHandler.TypeName : string.Empty,
                    faultRecord.Fault.ToString(), faultRecord.IsFaultSource,
                    faultRecord.HasAnnotations ? PrepareAnnotations(faultRecord.Annotations) : emptyItemsTag,
                    this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    if (EtwTrackingParticipantTrackRecords.FaultPropagationRecord(this.diagnosticTrace, faultRecord.EventTraceActivity, faultRecord.InstanceId,
                        faultRecord.RecordNumber,
                        faultRecord.EventTime.ToFileTime(),
                        faultRecord.FaultSource.Name, faultRecord.FaultSource.Id, faultRecord.FaultSource.InstanceId, faultRecord.FaultSource.TypeName,
                        faultRecord.FaultHandler != null ? faultRecord.FaultHandler.Name : string.Empty,
                        faultRecord.FaultHandler != null ? faultRecord.FaultHandler.Id : string.Empty,
                        faultRecord.FaultHandler != null ? faultRecord.FaultHandler.InstanceId : string.Empty,
                        faultRecord.FaultHandler != null ? faultRecord.FaultHandler.TypeName : string.Empty,
                        faultRecord.Fault.ToString(), faultRecord.IsFaultSource,
                        truncatedItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        TraceTrackingRecordTruncated(faultRecord.RecordNumber);
                    }
                    else
                    {
                        TraceTrackingRecordDropped(faultRecord.RecordNumber);
                    }
                }
            }
        }

        void TrackBookmarkRecord(BookmarkResumptionRecord record)
        {
            if (EtwTrackingParticipantTrackRecords.BookmarkResumptionRecordIsEnabled(this.diagnosticTrace))
            {
                if (!EtwTrackingParticipantTrackRecords.BookmarkResumptionRecord(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(),
                    record.BookmarkName, record.BookmarkScope, record.Owner.Name, record.Owner.Id,
                    record.Owner.InstanceId, record.Owner.TypeName,
                    record.HasAnnotations ? PrepareAnnotations(record.Annotations) : emptyItemsTag,
                    this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    if (EtwTrackingParticipantTrackRecords.BookmarkResumptionRecord(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(),
                        record.BookmarkName, record.BookmarkScope, record.Owner.Name, record.Owner.Id,
                        record.Owner.InstanceId, record.Owner.TypeName,
                        truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        TraceTrackingRecordTruncated(record.RecordNumber);
                    }
                    else
                    {
                        TraceTrackingRecordDropped(record.RecordNumber);
                    }
                }
            }
        }

        void TrackCustomRecord(CustomTrackingRecord record)
        {
            switch (record.Level)
            {
                case TraceLevel.Error:
                    if (EtwTrackingParticipantTrackRecords.CustomTrackingRecordErrorIsEnabled(this.diagnosticTrace))
                    {
                        if (!EtwTrackingParticipantTrackRecords.CustomTrackingRecordError(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId,
                                            record.RecordNumber, record.EventTime.ToFileTime(), record.Name,
                                            record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName,
                                            PrepareDictionary(record.Data),
                                            record.HasAnnotations ? PrepareAnnotations(record.Annotations) : emptyItemsTag,
                                            this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            if (EtwTrackingParticipantTrackRecords.CustomTrackingRecordError(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId,
                                record.RecordNumber, record.EventTime.ToFileTime(), record.Name,
                                record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName,
                                truncatedItemsTag, truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                            {
                                TraceTrackingRecordTruncated(record.RecordNumber);
                            }
                            else
                            {
                                TraceTrackingRecordDropped(record.RecordNumber);
                            }
                        }
                    }
                    break;
                case TraceLevel.Warning:
                    if (EtwTrackingParticipantTrackRecords.CustomTrackingRecordWarningIsEnabled(this.diagnosticTrace))
                    {
                        if (!EtwTrackingParticipantTrackRecords.CustomTrackingRecordWarning(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId,
                                             record.RecordNumber, record.EventTime.ToFileTime(), record.Name,
                                             record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName,
                                             PrepareDictionary(record.Data),
                                             record.HasAnnotations ? PrepareAnnotations(record.Annotations) : emptyItemsTag,
                                             this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            if (EtwTrackingParticipantTrackRecords.CustomTrackingRecordWarning(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId,
                                record.RecordNumber, record.EventTime.ToFileTime(), record.Name,
                                record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName,
                                truncatedItemsTag, truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                            {
                                TraceTrackingRecordTruncated(record.RecordNumber);
                            }
                            else
                            {
                                TraceTrackingRecordDropped(record.RecordNumber);
                            }
                        }
                    }
                    break;

                default:
                    if (EtwTrackingParticipantTrackRecords.CustomTrackingRecordInfoIsEnabled(this.diagnosticTrace))
                    {
                        if (!EtwTrackingParticipantTrackRecords.CustomTrackingRecordInfo(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId,
                                        record.RecordNumber, record.EventTime.ToFileTime(), record.Name,
                                             record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName,
                                        PrepareDictionary(record.Data),
                                        record.HasAnnotations ? PrepareAnnotations(record.Annotations) : emptyItemsTag,
                                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            if (EtwTrackingParticipantTrackRecords.CustomTrackingRecordInfo(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId,
                                record.RecordNumber, record.EventTime.ToFileTime(), record.Name,
                                record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName,
                                truncatedItemsTag, truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                            {
                                TraceTrackingRecordTruncated(record.RecordNumber);
                            }
                            else
                            {
                                TraceTrackingRecordDropped(record.RecordNumber);
                            }
                        }
                    }
                    break;
            }
        }

        void TrackWorkflowRecord(WorkflowInstanceRecord record)
        {
            // In the TrackWorkflowInstance*Record methods below there are two code paths.
            // If the WorkflowIdentity is null, then we follow the exisiting 4.0 path.
            // If the WorkflowIdentity is provided, then if a particular field in the workflowInstance 
            // record is null, we need to ensure that we are passing string.Empty.
            // The WriteEvent method on the DiagnosticEventProvider which is called in the 
            // WriteEtwEvent in the EtwTrackingParticipantRecords class invokes the EventWrite
            // native method which relies on getting the record arguments in a particular order.
            if (record is WorkflowInstanceUnhandledExceptionRecord)
            {
                TrackWorkflowInstanceUnhandledExceptionRecord(record);
            }
            else if (record is WorkflowInstanceAbortedRecord)
            {
                TrackWorkflowInstanceAbortedRecord(record);                
            }
            else if (record is WorkflowInstanceSuspendedRecord)
            {
                TrackWorkflowInstanceSuspendedRecord(record);                
            }
            else if (record is WorkflowInstanceTerminatedRecord)
            {
                TrackWorkflowInstanceTerminatedRecord(record);                
            }
            else if (record is WorkflowInstanceUpdatedRecord)
            {
                TrackWorkflowInstanceUpdatedRecord(record);
            }
            else
            {
                TrackWorkflowInstanceRecord(record);
            }
        }

        void TrackWorkflowInstanceUnhandledExceptionRecord(WorkflowInstanceRecord record)
        {
            WorkflowInstanceUnhandledExceptionRecord unhandled = record as WorkflowInstanceUnhandledExceptionRecord;
            if (unhandled.WorkflowDefinitionIdentity == null)
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceUnhandledExceptionRecordIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceUnhandledExceptionRecord(this.diagnosticTrace, unhandled.EventTraceActivity, unhandled.InstanceId,
                        unhandled.RecordNumber, unhandled.EventTime.ToFileTime(), unhandled.ActivityDefinitionId,
                        unhandled.FaultSource.Name, unhandled.FaultSource.Id, unhandled.FaultSource.InstanceId, unhandled.FaultSource.TypeName,
                        unhandled.UnhandledException == null ? string.Empty : unhandled.UnhandledException.ToString(),
                        unhandled.HasAnnotations ? PrepareAnnotations(unhandled.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceUnhandledExceptionRecord(this.diagnosticTrace, unhandled.EventTraceActivity, unhandled.InstanceId,
                            unhandled.RecordNumber, unhandled.EventTime.ToFileTime(), unhandled.ActivityDefinitionId,
                            unhandled.FaultSource.Name, unhandled.FaultSource.Id, unhandled.FaultSource.InstanceId, unhandled.FaultSource.TypeName,
                            unhandled.UnhandledException == null ? string.Empty : unhandled.UnhandledException.ToString(),
                            truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(unhandled.RecordNumber);
                        }                        
                        else
                        {
                            TraceTrackingRecordDropped(unhandled.RecordNumber);
                        }
                    }
                }
            }
            else
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceUnhandledExceptionRecordWithIdIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceUnhandledExceptionRecordWithId(this.diagnosticTrace, unhandled.EventTraceActivity, unhandled.InstanceId,
                        unhandled.RecordNumber, unhandled.EventTime.ToFileTime(), unhandled.ActivityDefinitionId,
                        unhandled.FaultSource.Name, unhandled.FaultSource.Id, unhandled.FaultSource.InstanceId, unhandled.FaultSource.TypeName,
                        unhandled.UnhandledException == null ? string.Empty : unhandled.UnhandledException.ToString(),
                        unhandled.HasAnnotations ? PrepareAnnotations(unhandled.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name,
                        unhandled.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceUnhandledExceptionRecordWithId(this.diagnosticTrace, unhandled.EventTraceActivity, unhandled.InstanceId,
                            unhandled.RecordNumber, unhandled.EventTime.ToFileTime(), unhandled.ActivityDefinitionId,
                            unhandled.FaultSource.Name, unhandled.FaultSource.Id, unhandled.FaultSource.InstanceId, unhandled.FaultSource.TypeName,
                            unhandled.UnhandledException == null ? string.Empty : unhandled.UnhandledException.ToString(),
                            truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name,
                            unhandled.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(unhandled.RecordNumber);
                        }
                        else
                        {
                            TraceTrackingRecordDropped(unhandled.RecordNumber);
                        }
                    }
                }
            }
        }

        void TrackWorkflowInstanceAbortedRecord(WorkflowInstanceRecord record)
        {
            WorkflowInstanceAbortedRecord aborted = record as WorkflowInstanceAbortedRecord;
            if (aborted.WorkflowDefinitionIdentity == null)
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceAbortedRecordIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceAbortedRecord(this.diagnosticTrace, aborted.EventTraceActivity, aborted.InstanceId, aborted.RecordNumber,
                        aborted.EventTime.ToFileTime(), aborted.ActivityDefinitionId, aborted.Reason,
                        aborted.HasAnnotations ? PrepareAnnotations(aborted.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceAbortedRecord(this.diagnosticTrace, aborted.EventTraceActivity, aborted.InstanceId, aborted.RecordNumber,
                            aborted.EventTime.ToFileTime(), aborted.ActivityDefinitionId, aborted.Reason,
                            truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(aborted.RecordNumber);
                        }
                        else
                        {
                            TraceTrackingRecordDropped(aborted.RecordNumber);
                        }
                    }
                }
            }
            else
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceAbortedRecordWithIdIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceAbortedRecordWithId(this.diagnosticTrace, aborted.EventTraceActivity, aborted.InstanceId, aborted.RecordNumber,
                        aborted.EventTime.ToFileTime(), aborted.ActivityDefinitionId, aborted.Reason,
                        aborted.HasAnnotations ? PrepareAnnotations(aborted.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name,
                        aborted.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceAbortedRecordWithId(this.diagnosticTrace, aborted.EventTraceActivity, aborted.InstanceId, aborted.RecordNumber,
                            aborted.EventTime.ToFileTime(), aborted.ActivityDefinitionId, aborted.Reason,
                            truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name,
                            aborted.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(aborted.RecordNumber);
                        }
                        else
                        {
                            TraceTrackingRecordDropped(aborted.RecordNumber);
                        }
                    } 
                }
            }
        }

        void TrackWorkflowInstanceSuspendedRecord(WorkflowInstanceRecord record)
        {
            WorkflowInstanceSuspendedRecord suspended = record as WorkflowInstanceSuspendedRecord;
            if (suspended.WorkflowDefinitionIdentity == null)
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceSuspendedRecordIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceSuspendedRecord(this.diagnosticTrace, suspended.EventTraceActivity, suspended.InstanceId, suspended.RecordNumber,
                        suspended.EventTime.ToFileTime(), suspended.ActivityDefinitionId, suspended.Reason,
                        suspended.HasAnnotations ? PrepareAnnotations(suspended.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceSuspendedRecord(this.diagnosticTrace, suspended.EventTraceActivity, suspended.InstanceId, suspended.RecordNumber,
                            suspended.EventTime.ToFileTime(), suspended.ActivityDefinitionId, suspended.Reason,
                            truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(suspended.RecordNumber);
                        }
                        else
                        {
                            TraceTrackingRecordDropped(suspended.RecordNumber);
                        }
                    }
                }
            }
            else
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceSuspendedRecordWithIdIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceSuspendedRecordWithId(this.diagnosticTrace, suspended.EventTraceActivity, suspended.InstanceId, suspended.RecordNumber,
                        suspended.EventTime.ToFileTime(), suspended.ActivityDefinitionId, suspended.Reason,
                        suspended.HasAnnotations ? PrepareAnnotations(suspended.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name,
                        suspended.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceSuspendedRecordWithId(this.diagnosticTrace, suspended.EventTraceActivity, suspended.InstanceId, suspended.RecordNumber,
                            suspended.EventTime.ToFileTime(), suspended.ActivityDefinitionId, suspended.Reason,
                            truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name,
                            suspended.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(suspended.RecordNumber);
                        }
                        else
                        {
                            TraceTrackingRecordDropped(suspended.RecordNumber);
                        }
                    }
                }
            }
        }
        
        void TrackWorkflowInstanceTerminatedRecord(WorkflowInstanceRecord record)
        {
            WorkflowInstanceTerminatedRecord terminated = record as WorkflowInstanceTerminatedRecord;
            if (terminated.WorkflowDefinitionIdentity == null)
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceTerminatedRecordIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceTerminatedRecord(this.diagnosticTrace, terminated.EventTraceActivity, terminated.InstanceId, terminated.RecordNumber,
                        terminated.EventTime.ToFileTime(), terminated.ActivityDefinitionId, terminated.Reason,
                        terminated.HasAnnotations ? PrepareAnnotations(terminated.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceTerminatedRecord(this.diagnosticTrace, terminated.EventTraceActivity, terminated.InstanceId, terminated.RecordNumber,
                            terminated.EventTime.ToFileTime(), terminated.ActivityDefinitionId, terminated.Reason,
                            truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(terminated.RecordNumber);
                        }
                        else
                        {
                            TraceTrackingRecordDropped(terminated.RecordNumber);
                        }
                    }
                }
            }
            else
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceTerminatedRecordWithIdIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceTerminatedRecordWithId(this.diagnosticTrace, terminated.EventTraceActivity, terminated.InstanceId, terminated.RecordNumber,
                        terminated.EventTime.ToFileTime(), terminated.ActivityDefinitionId, terminated.Reason,
                        terminated.HasAnnotations ? PrepareAnnotations(terminated.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name,
                        terminated.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceTerminatedRecordWithId(this.diagnosticTrace, terminated.EventTraceActivity, terminated.InstanceId, terminated.RecordNumber,
                            terminated.EventTime.ToFileTime(), terminated.ActivityDefinitionId, terminated.Reason,
                            truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name,
                            terminated.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(terminated.RecordNumber);
                        }
                        else
                        {
                            TraceTrackingRecordDropped(terminated.RecordNumber);
                        }
                    }
                }
            }
        }
        
        void TrackWorkflowInstanceRecord(WorkflowInstanceRecord record)
        {
            if (record.WorkflowDefinitionIdentity == null)
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceRecordIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceRecord(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId, record.RecordNumber,
                        record.EventTime.ToFileTime(), record.ActivityDefinitionId,
                        record.State,
                        record.HasAnnotations ? PrepareAnnotations(record.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceRecord(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(),
                            record.ActivityDefinitionId, record.State, truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(record.RecordNumber);
                        }
                        else
                        {
                            TraceTrackingRecordDropped(record.RecordNumber);
                        }
                    }
                }
            }
            else
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceRecordWithIdIsEnabled(this.diagnosticTrace))
                {
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceRecordWithId(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId, record.RecordNumber,
                        record.EventTime.ToFileTime(), record.ActivityDefinitionId,
                        record.State,
                        record.HasAnnotations ? PrepareAnnotations(record.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name, 
                        record.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceRecordWithId(this.diagnosticTrace, record.EventTraceActivity, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(),
                            record.ActivityDefinitionId, record.State, truncatedItemsTag, this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name,
                            record.WorkflowDefinitionIdentity.ToString(), this.ApplicationReference))
                        {
                            TraceTrackingRecordTruncated(record.RecordNumber);
                        }
                        else
                        {
                            TraceTrackingRecordDropped(record.RecordNumber);
                        }
                    }
                }
            }
        }

        void TrackWorkflowInstanceUpdatedRecord(WorkflowInstanceRecord record)
        {
            if (EtwTrackingParticipantTrackRecords.WorkflowInstanceUpdatedRecordIsEnabled(this.diagnosticTrace))
            {
                WorkflowInstanceUpdatedRecord updatedRecord = record as WorkflowInstanceUpdatedRecord;
                if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceUpdatedRecord(this.diagnosticTrace, updatedRecord.EventTraceActivity, updatedRecord.InstanceId,
                    updatedRecord.RecordNumber, updatedRecord.EventTime.ToFileTime(), updatedRecord.ActivityDefinitionId, updatedRecord.State,
                    updatedRecord.OriginalDefinitionIdentity == null ? string.Empty : updatedRecord.OriginalDefinitionIdentity.ToString(),
                    updatedRecord.WorkflowDefinitionIdentity == null ? string.Empty : updatedRecord.WorkflowDefinitionIdentity.ToString(),
                    updatedRecord.HasAnnotations ? PrepareAnnotations(updatedRecord.Annotations) : emptyItemsTag,
                    this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    if (EtwTrackingParticipantTrackRecords.WorkflowInstanceUpdatedRecord(this.diagnosticTrace, updatedRecord.EventTraceActivity, updatedRecord.InstanceId,
                        updatedRecord.RecordNumber, updatedRecord.EventTime.ToFileTime(), updatedRecord.ActivityDefinitionId, updatedRecord.State,
                        updatedRecord.OriginalDefinitionIdentity == null ? string.Empty : updatedRecord.OriginalDefinitionIdentity.ToString(),
                        updatedRecord.WorkflowDefinitionIdentity == null ? string.Empty : updatedRecord.WorkflowDefinitionIdentity.ToString(),
                        updatedRecord.HasAnnotations ? PrepareAnnotations(updatedRecord.Annotations) : emptyItemsTag,
                        this.TrackingProfile == null ? string.Empty : this.TrackingProfile.Name == null ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        TraceTrackingRecordTruncated(updatedRecord.RecordNumber);
                    }
                    else
                    {
                        TraceTrackingRecordDropped(updatedRecord.RecordNumber);
                    }
                }
            }
        }
    }
}
