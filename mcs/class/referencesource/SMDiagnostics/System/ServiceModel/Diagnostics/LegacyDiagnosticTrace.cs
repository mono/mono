//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;
    using System.Diagnostics.CodeAnalysis;

    class LegacyDiagnosticTrace : DiagnosticTraceBase
    {
        const int MaxTraceSize = 65535;
        bool shouldUseActivity = false;
        TraceSourceKind traceSourceType = TraceSourceKind.PiiTraceSource;
        const string subType = "";
        const string version = "1";
        const int traceFailureLogThreshold = 1;
        const SourceLevels DefaultLevel = SourceLevels.Off;
        static object classLockObject = new object();

        protected override void OnSetLevel(SourceLevels level)
        {
            if (this.TraceSource != null)
            {
                if (this.TraceSource.Switch.Level != SourceLevels.Off &&
                    level == SourceLevels.Off)
                {
                    TraceSource temp = this.TraceSource;
                    this.CreateTraceSource();
                    temp.Close();
                }
                this.shouldUseActivity = (level & SourceLevels.ActivityTracing) != 0;
            }
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ShouldUseActivity instead")]
        internal bool ShouldUseActivity
        {
            get { return this.shouldUseActivity; }
        }

#pragma warning disable 56500
        [Obsolete("For SMDiagnostics.dll use only. Never 'new' this type up unless you are DiagnosticUtility.")]
        [Fx.Tag.SecurityNote(Critical = "Sets eventSourceName.")]
        [SecurityCritical]
        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotCallOverridableMethodsInConstructors,
                Justification = "LegacyDiagnosticTrace is an internal class without derived classes")]
        internal LegacyDiagnosticTrace(TraceSourceKind sourceType, string traceSourceName, string eventSourceName)
            : base(traceSourceName)
        {
            this.traceSourceType = sourceType;
            this.EventSourceName = eventSourceName;

            try
            {
                this.CreateTraceSource();
                this.AddDomainEventHandlersForCleanup();
            }
            catch (ConfigurationErrorsException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                System.Runtime.Diagnostics.EventLogger logger = new System.Runtime.Diagnostics.EventLogger(this.EventSourceName, null);
                logger.LogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.Tracing, (uint)System.Runtime.Diagnostics.EventLogEventId.FailedToSetupTracing, false,
                    e.ToString());
            }
        }
#pragma warning restore 56500
        
        [SecuritySafeCritical]
        void CreateTraceSource()
        {
            PiiTraceSource tempSource = null;
            if (this.traceSourceType == TraceSourceKind.PiiTraceSource)
            {
                tempSource = new PiiTraceSource(this.TraceSourceName, this.EventSourceName, LegacyDiagnosticTrace.DefaultLevel);
            }
            else
            {
                tempSource = new DiagnosticTraceSource(this.TraceSourceName, this.EventSourceName, LegacyDiagnosticTrace.DefaultLevel);
            }

            SetTraceSource(tempSource);
        }

#pragma warning disable 56500
        internal void TraceEvent(TraceEventType type, int code, string msdnTraceCode, string description, TraceRecord trace, Exception exception, object source)
        {
#pragma warning disable 618
            Fx.Assert(exception == null || type <= TraceEventType.Information, "Exceptions should be traced at Information or higher");
            Fx.Assert(!string.IsNullOrEmpty(description), "All TraceCodes should have a description");
#pragma warning restore 618
            TraceXPathNavigator navigator = null;
            try
            {
#pragma warning disable 618
                if (this.TraceSource != null && this.HaveListeners)
#pragma warning restore 618
                {
                    try
                    {
                        BuildTrace(type, msdnTraceCode, description, trace, exception, source, out navigator);
                    }
                    catch (PlainXmlWriter.MaxSizeExceededException)
                    {
                        StringTraceRecord codeTraceRecord = new StringTraceRecord("TruncatedTraceId", msdnTraceCode);
                        this.TraceEvent(type, DiagnosticsTraceCode.TraceTruncatedQuotaExceeded, LegacyDiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "TraceTruncatedQuotaExceeded"), TraceSR.GetString(TraceSR.TraceCodeTraceTruncatedQuotaExceeded), codeTraceRecord, null, null);
                    }
                    this.TraceSource.TraceData(type, code, navigator);
                    if (this.CalledShutdown)
                    {
                        this.TraceSource.Flush();
                    }
                    // Must have been a successful trace.
                    this.LastFailure = DateTime.MinValue;
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                LogTraceFailure(navigator == null ? string.Empty : navigator.ToString(), e);
            }
        }
#pragma warning restore 56500

        internal void TraceEvent(TraceEventType type, int code, string msdnTraceCode, string description, TraceRecord trace, Exception exception, Guid activityId, object source)
        {
#pragma warning disable 618
            using ((this.ShouldUseActivity && Guid.Empty != activityId) ? Activity.CreateActivity(activityId) : null)
#pragma warning restore 618
            {
                this.TraceEvent(type, code, msdnTraceCode, description, trace, exception, source);
            }
        }

        // helper for standardized trace code generation
        static internal string GenerateMsdnTraceCode(string traceSource, string traceCodeString)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "http://msdn.microsoft.com/{0}/library/{1}.{2}.aspx",
                CultureInfo.CurrentCulture.Name,
                traceSource, traceCodeString);
        }

#pragma warning disable 56500
        internal void TraceTransfer(Guid newId)
        {
#pragma warning disable 618
            if (this.ShouldUseActivity)
#pragma warning restore 618
            {
                Guid oldId = LegacyDiagnosticTrace.ActivityId;
                if (newId != oldId)
                {
#pragma warning disable 618
                    if (this.HaveListeners)
#pragma warning restore 618
                    {
                        try
                        {
                            this.TraceSource.TraceTransfer(0, null, newId);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            LogTraceFailure(null, e);
                        }
                    }
                }
            }
        }

        protected override void OnShutdownTracing()
        {
            if (null != this.TraceSource)
            {
#pragma warning disable 618
                if (this.Level != SourceLevels.Off)
                {
                    if (this.ShouldTrace(TraceEventType.Information))
#pragma warning restore 618
                    {
                        Dictionary<string, string> values = new Dictionary<string, string>(3);
                        values["AppDomain.FriendlyName"] = AppDomain.CurrentDomain.FriendlyName;
                        values["ProcessName"] = DiagnosticTraceBase.ProcessName;
                        values["ProcessId"] = DiagnosticTraceBase.ProcessId.ToString(CultureInfo.CurrentCulture);
                        this.TraceEvent(TraceEventType.Information, DiagnosticsTraceCode.AppDomainUnload, LegacyDiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "AppDomainUnload"), TraceSR.GetString(TraceSR.TraceCodeAppDomainUnload),
                            new DictionaryTraceRecord(values), null, null);
                    }
                    this.TraceSource.Flush();
                }
            }
        }

        protected override void OnUnhandledException(Exception exception)
        {
            TraceEvent(TraceEventType.Critical, DiagnosticsTraceCode.UnhandledException, "UnhandledException", TraceSR.GetString(TraceSR.UnhandledException), null, exception, null);
        }

        public bool ShouldLogPii
        {
            get
            {
                PiiTraceSource traceSource = this.TraceSource as PiiTraceSource;
                if (traceSource != null)
                {
                    return traceSource.ShouldLogPii;
                }

                return false;
            }

            set
            {
                PiiTraceSource traceSource = this.TraceSource as PiiTraceSource;
                if (traceSource != null)
                {
                    traceSource.ShouldLogPii = value;
                }
            }
        }

        void BuildTrace(TraceEventType type, string msdnTraceCode, string description, TraceRecord trace,
            Exception exception, object source, out TraceXPathNavigator navigator)
        {
            PlainXmlWriter xmlWriter = new PlainXmlWriter(LegacyDiagnosticTrace.MaxTraceSize);
            navigator = xmlWriter.Navigator;

            this.BuildTrace(xmlWriter, type, msdnTraceCode, description, trace, exception, source);

            if (!ShouldLogPii)
            {
                navigator.RemovePii(DiagnosticStrings.HeadersPaths);
            }
        }

        void BuildTrace(PlainXmlWriter xml, TraceEventType type, string msdnTraceCode, string description,
            TraceRecord trace, Exception exception, object source)
        {
            xml.WriteStartElement(DiagnosticStrings.TraceRecordTag);
            xml.WriteAttributeString(DiagnosticStrings.NamespaceTag, LegacyDiagnosticTrace.TraceRecordVersion);
            xml.WriteAttributeString(DiagnosticStrings.SeverityTag, DiagnosticTraceBase.LookupSeverity(type));

            xml.WriteElementString(DiagnosticStrings.TraceCodeTag, msdnTraceCode);
            xml.WriteElementString(DiagnosticStrings.DescriptionTag, description);
            xml.WriteElementString(DiagnosticStrings.AppDomain, DiagnosticTraceBase.AppDomainFriendlyName);

            if (source != null)
            {
                xml.WriteElementString(DiagnosticStrings.SourceTag, CreateSourceString(source));
            }

            if (trace != null)
            {
                xml.WriteStartElement(DiagnosticStrings.ExtendedDataTag);
                xml.WriteAttributeString(DiagnosticStrings.NamespaceTag, trace.EventId);

                trace.WriteTo(xml);

                xml.WriteEndElement();
            }

            if (exception != null)
            {
                xml.WriteStartElement(DiagnosticStrings.ExceptionTag);
                AddExceptionToTraceString(xml, exception);
                xml.WriteEndElement();
            }

            xml.WriteEndElement();
        }

        public override bool IsEnabled()
        {
            return true;
        }

        public override void TraceEventLogEvent(TraceEventType type, TraceRecord traceRecord)
        {
            TraceEvent(type,
                DiagnosticsTraceCode.EventLog, LegacyDiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "EventLog"),
                TraceSR.GetString(TraceSR.TraceCodeEventLog),
                traceRecord, null, null);
        }
    }
}
