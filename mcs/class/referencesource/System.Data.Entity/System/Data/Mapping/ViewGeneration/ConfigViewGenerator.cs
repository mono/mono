//---------------------------------------------------------------------
// <copyright file="ConfigViewGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Text;
using System.Diagnostics;

namespace System.Data.Mapping.ViewGeneration
{
    internal enum ViewGenMode
    {
        GenerateAllViews = 0,
        OfTypeViews,
        OfTypeOnlyViews
    }

    internal enum ViewGenTraceLevel
    {
        None = 0,
        ViewsOnly,
        Normal,
        Verbose
    }

    internal enum PerfType
    {
        InitialSetup = 0,
        CellCreation,
        KeyConstraint,
        ViewgenContext,
        UpdateViews,
        DisjointConstraint,
        PartitionConstraint,
        DomainConstraint,
        ForeignConstraint,
        QueryViews,
        BoolResolution,
        Unsatisfiability,
        ViewParsing,
    }

    /// <summary>
    /// This class holds some configuration information for the view generation code.
    /// </summary>
    internal sealed class ConfigViewGenerator : InternalBase
    {
        #region Constructors
        internal ConfigViewGenerator()
        {
            m_watch = new Stopwatch();
            m_singleWatch = new Stopwatch();
            int numEnums = Enum.GetNames(typeof(PerfType)).Length;
            m_breakdownTimes = new TimeSpan[numEnums];
            m_traceLevel = ViewGenTraceLevel.None;
            m_generateUpdateViews = false;
            StartWatch();
        }
        #endregion

        #region Fields
        private bool m_generateViewsForEachType;
        private ViewGenTraceLevel m_traceLevel;
        private readonly TimeSpan[] m_breakdownTimes;
        private Stopwatch m_watch;
        /// <summary>
        /// To measure a single thing at a time.
        /// </summary>
        private Stopwatch m_singleWatch;
        /// <summary>
        /// Perf op being measured.
        /// </summary>
        private PerfType m_singlePerfOp;
        private bool m_enableValidation = true;
        private bool m_generateUpdateViews = true;
        private bool m_generateEsql = false;
        #endregion

        #region Properties
        /// <summary>
        /// If true then view generation will produce eSQL, otherwise CQTs only.
        /// </summary>
        internal bool GenerateEsql
        {
            get { return m_generateEsql; }
            set { m_generateEsql = value; }
        }

        /// <summary>
        /// Callers can set elements in this list.
        /// </summary>
        internal TimeSpan[] BreakdownTimes
        {
            get { return m_breakdownTimes; }
        }

        internal ViewGenTraceLevel TraceLevel
        {
            get { return m_traceLevel; }
            set { m_traceLevel = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal bool IsValidationEnabled
        {
            get { return m_enableValidation; }
            set { m_enableValidation = value; }
        }

        internal bool GenerateUpdateViews
        {
            get { return m_generateUpdateViews; }
            set { m_generateUpdateViews = value; }
        }

        internal bool GenerateViewsForEachType
        {
            get { return m_generateViewsForEachType; }
            set { m_generateViewsForEachType = value; }
        }

        internal bool IsViewTracing
        {
            get { return IsTraceAllowed(ViewGenTraceLevel.ViewsOnly); }
        }

        internal bool IsNormalTracing
        {
            get { return IsTraceAllowed(ViewGenTraceLevel.Normal); }
        }

        internal bool IsVerboseTracing
        {
            get { return IsTraceAllowed(ViewGenTraceLevel.Verbose); }
        }
        #endregion

        #region Methods
        private void StartWatch()
        {
            m_watch.Start();
        }

        internal void StartSingleWatch(PerfType perfType)
        {
            m_singleWatch.Start();
            m_singlePerfOp = perfType;
        }

        /// <summary>
        /// Sets time for <paramref name="perfType"/> for the individual timer.
        /// </summary>
        internal void StopSingleWatch(PerfType perfType)
        {
            Debug.Assert(m_singlePerfOp == perfType, "Started op for different activity " + m_singlePerfOp + " -- not " + perfType);
            TimeSpan timeElapsed = m_singleWatch.Elapsed;
            int index = (int)perfType;
            m_singleWatch.Stop();
            m_singleWatch.Reset();
            BreakdownTimes[index] = BreakdownTimes[index].Add(timeElapsed);
        }

        /// <summary>
        /// Sets time for <paramref name="perfType"/> since the last call to <see cref="SetTimeForFinishedActivity"/>.
        /// </summary>
        /// <param name="perfType"></param>
        internal void SetTimeForFinishedActivity(PerfType perfType)
        {
            TimeSpan timeElapsed = m_watch.Elapsed;
            int index = (int)perfType;
            BreakdownTimes[index] = BreakdownTimes[index].Add(timeElapsed);
            m_watch.Reset();
            m_watch.Start();
        }

        internal bool IsTraceAllowed(ViewGenTraceLevel traceLevel)
        {
            return TraceLevel >= traceLevel;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            StringUtil.FormatStringBuilder(builder, "Trace Switch: {0}", m_traceLevel);
        }
        #endregion
    }
}
