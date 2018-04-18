//------------------------------------------------------------------------------
// <copyright file="SqlConnectionTimeoutErrorInternal.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient
{

    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using SysTx = System.Transactions;
    using System.Data.SqlClient;
    using System.Text;

    // VSTFDevDiv# 643319 - Improve timeout error message reported when SqlConnection.Open fails
    internal enum SqlConnectionTimeoutErrorPhase
    {
        Undefined = 0,
        PreLoginBegin,              // [PRE-LOGIN PHASE]        Start of the pre-login phase; Initialize global variables;
        InitializeConnection,       // [PRE-LOGIN PHASE]        Create and initialize socket.
        SendPreLoginHandshake,      // [PRE-LOGIN PHASE]        Make pre-login handshake request.
        ConsumePreLoginHandshake,   // [PRE-LOGIN PHASE]        Receive pre-login handshake response and consume it; Establish an SSL channel.
        LoginBegin,                 // [LOGIN PHASE]            End of the pre-login phase; Start of the login phase; 
        ProcessConnectionAuth,      // [LOGIN PHASE]            Process SSPI or SQL Authenticate.
        PostLogin,                  // [POST-LOGIN PHASE]       End of the login phase; And post-login phase;
        Complete,                   // Marker for the succesful completion of the connection
        Count                       // ** This is to track the length of the enum. ** Do not add any phase after this. **
    }

    internal enum SqlConnectionInternalSourceType
    {
        Principle,
        Failover,
        RoutingDestination
    }

    // DEVNOTE: Class to capture the duration spent in each SqlConnectionTimeoutErrorPhase.
    internal class SqlConnectionTimeoutPhaseDuration
    {
        Stopwatch swDuration = new Stopwatch();

        internal void StartCapture()
        {
            Debug.Assert(swDuration != null, "Time capture stopwatch cannot be null.");
            swDuration.Start();
        }

        internal void StopCapture()
        {
            //Debug.Assert(swDuration.IsRunning == true, "The stop opertaion of the stopwatch cannot be called when it is not running.");
            if (swDuration.IsRunning == true)
                swDuration.Stop();
        }

        internal long GetMilliSecondDuration()
        {
            // DEVNOTE: In a phase fails in between a phase, the stop watch may still be running.
            // Hence the check to verify if the stop watch is running hasn't been added in.
            return swDuration.ElapsedMilliseconds;
        }
    }

    internal class SqlConnectionTimeoutErrorInternal
    {
        SqlConnectionTimeoutPhaseDuration[] phaseDurations = null;
        SqlConnectionTimeoutPhaseDuration[] originalPhaseDurations = null;

        SqlConnectionTimeoutErrorPhase currentPhase = SqlConnectionTimeoutErrorPhase.Undefined;
        SqlConnectionInternalSourceType currentSourceType = SqlConnectionInternalSourceType.Principle;
        bool isFailoverScenario = false;

        internal SqlConnectionTimeoutErrorPhase CurrentPhase
        {
            get { return currentPhase; }
        }

        public SqlConnectionTimeoutErrorInternal()
        {
            phaseDurations = new SqlConnectionTimeoutPhaseDuration[(int)SqlConnectionTimeoutErrorPhase.Count];
            for (int i = 0; i < phaseDurations.Length; i++)
                phaseDurations[i] = null;
        }

        public void SetFailoverScenario(bool useFailoverServer)
        {
            isFailoverScenario = useFailoverServer;
        }

        public void SetInternalSourceType(SqlConnectionInternalSourceType sourceType)
        {
            currentSourceType = sourceType;

            if (currentSourceType == SqlConnectionInternalSourceType.RoutingDestination)
            {
                // When we get routed, save the current phase durations so that we can use them in the error message later
                Debug.Assert(currentPhase == SqlConnectionTimeoutErrorPhase.PostLogin, "Should not be switching to the routing destination until Post Login is completed");
                originalPhaseDurations = phaseDurations;
                phaseDurations = new SqlConnectionTimeoutPhaseDuration[(int)SqlConnectionTimeoutErrorPhase.Count];
                SetAndBeginPhase(SqlConnectionTimeoutErrorPhase.PreLoginBegin);
            }
        }

        internal void ResetAndRestartPhase()
        {
            currentPhase = SqlConnectionTimeoutErrorPhase.PreLoginBegin;
            for (int i = 0; i < phaseDurations.Length; i++)
                phaseDurations[i] = null;
        }

        internal void SetAndBeginPhase(SqlConnectionTimeoutErrorPhase timeoutErrorPhase)
        {
            currentPhase = timeoutErrorPhase;
            if (phaseDurations[(int)timeoutErrorPhase] == null)
            {
                phaseDurations[(int)timeoutErrorPhase] = new SqlConnectionTimeoutPhaseDuration();
            }
            phaseDurations[(int)timeoutErrorPhase].StartCapture();
        }

        internal void EndPhase(SqlConnectionTimeoutErrorPhase timeoutErrorPhase)
        {
            Debug.Assert(phaseDurations[(int)timeoutErrorPhase] != null, "End phase capture cannot be invoked when the phase duration object is a null.");
            phaseDurations[(int)timeoutErrorPhase].StopCapture();
        }

        internal void SetAllCompleteMarker()
        {
            currentPhase = SqlConnectionTimeoutErrorPhase.Complete;
        }

        internal string GetErrorMessage()
        {
            StringBuilder errorBuilder;
            string durationString;
            switch(currentPhase)
            {
                case SqlConnectionTimeoutErrorPhase.PreLoginBegin:
                    errorBuilder = new StringBuilder(SQLMessage.Timeout_PreLogin_Begin());
                    durationString = SQLMessage.Duration_PreLogin_Begin(
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.PreLoginBegin].GetMilliSecondDuration());
                    break;
                case SqlConnectionTimeoutErrorPhase.InitializeConnection:
                    errorBuilder = new StringBuilder(SQLMessage.Timeout_PreLogin_InitializeConnection());
                    durationString = SQLMessage.Duration_PreLogin_Begin(
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.PreLoginBegin].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.InitializeConnection].GetMilliSecondDuration());
                    break;
                case SqlConnectionTimeoutErrorPhase.SendPreLoginHandshake:
                    errorBuilder = new StringBuilder(SQLMessage.Timeout_PreLogin_SendHandshake());
                    durationString = SQLMessage.Duration_PreLoginHandshake(
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.PreLoginBegin].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.InitializeConnection].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.SendPreLoginHandshake].GetMilliSecondDuration());
                    break;
                case SqlConnectionTimeoutErrorPhase.ConsumePreLoginHandshake:
                    errorBuilder = new StringBuilder(SQLMessage.Timeout_PreLogin_ConsumeHandshake());
                    durationString = SQLMessage.Duration_PreLoginHandshake(
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.PreLoginBegin].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.InitializeConnection].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.SendPreLoginHandshake].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.ConsumePreLoginHandshake].GetMilliSecondDuration());
                    break;
                case SqlConnectionTimeoutErrorPhase.LoginBegin:
                    errorBuilder = new StringBuilder(SQLMessage.Timeout_Login_Begin());
                    durationString = SQLMessage.Duration_Login_Begin(
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.PreLoginBegin].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.InitializeConnection].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.SendPreLoginHandshake].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.ConsumePreLoginHandshake].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.LoginBegin].GetMilliSecondDuration());
                    break;
                case SqlConnectionTimeoutErrorPhase.ProcessConnectionAuth:
                    errorBuilder = new StringBuilder(SQLMessage.Timeout_Login_ProcessConnectionAuth());
                    durationString = SQLMessage.Duration_Login_ProcessConnectionAuth(
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.PreLoginBegin].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.InitializeConnection].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.SendPreLoginHandshake].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.ConsumePreLoginHandshake].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.LoginBegin].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.ProcessConnectionAuth].GetMilliSecondDuration());
                    break;
                case SqlConnectionTimeoutErrorPhase.PostLogin:
                    errorBuilder = new StringBuilder(SQLMessage.Timeout_PostLogin());
                    durationString = SQLMessage.Duration_PostLogin(
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.PreLoginBegin].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.InitializeConnection].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.SendPreLoginHandshake].GetMilliSecondDuration() +
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.ConsumePreLoginHandshake].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.LoginBegin].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.ProcessConnectionAuth].GetMilliSecondDuration(),
                        phaseDurations[(int)SqlConnectionTimeoutErrorPhase.PostLogin].GetMilliSecondDuration());
                    break;
                default:
                    errorBuilder = new StringBuilder(SQLMessage.Timeout());
                    durationString = null;
                    break;
            }
            
            // This message is to be added only when within the various stages of a connection. 
            // In all other cases, it will default to the original error message.
            if ((currentPhase != SqlConnectionTimeoutErrorPhase.Undefined) && (currentPhase != SqlConnectionTimeoutErrorPhase.Complete))
            {
                // NOTE: In case of a failover scenario, add a string that this failure occured as part of the primary or secondary server
                if (isFailoverScenario)
                {
                    errorBuilder.Append("  ");
                    errorBuilder.AppendFormat((IFormatProvider)null, SQLMessage.Timeout_FailoverInfo(), currentSourceType);
                }
                else if (currentSourceType == SqlConnectionInternalSourceType.RoutingDestination) {
                    errorBuilder.Append("  ");
                    errorBuilder.AppendFormat((IFormatProvider)null, SQLMessage.Timeout_RoutingDestination(),
                        originalPhaseDurations[(int)SqlConnectionTimeoutErrorPhase.PreLoginBegin].GetMilliSecondDuration() +
                        originalPhaseDurations[(int)SqlConnectionTimeoutErrorPhase.InitializeConnection].GetMilliSecondDuration(),
                        originalPhaseDurations[(int)SqlConnectionTimeoutErrorPhase.SendPreLoginHandshake].GetMilliSecondDuration() +
                        originalPhaseDurations[(int)SqlConnectionTimeoutErrorPhase.ConsumePreLoginHandshake].GetMilliSecondDuration(),
                        originalPhaseDurations[(int)SqlConnectionTimeoutErrorPhase.LoginBegin].GetMilliSecondDuration(),
                        originalPhaseDurations[(int)SqlConnectionTimeoutErrorPhase.ProcessConnectionAuth].GetMilliSecondDuration(),
                        originalPhaseDurations[(int)SqlConnectionTimeoutErrorPhase.PostLogin].GetMilliSecondDuration());
                }
            }

            // NOTE: To display duration in each phase.
            if (durationString != null)
            {
                errorBuilder.Append("  ");
                errorBuilder.Append(durationString);
            }

            return errorBuilder.ToString();
        }
    }
}
