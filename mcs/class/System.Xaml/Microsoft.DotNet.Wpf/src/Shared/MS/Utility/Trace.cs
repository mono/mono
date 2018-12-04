/***************************************************************************\
*
* File: Trace.cs
*
* Description:
* Implements ETW tracing for Avalon Managed Code
*
* Copyright (C) 2001 by Microsoft Corporation.  All rights reserved.
*
\***************************************************************************/
#if !SILVERLIGHTXAML

using System.Collections;
using System.Diagnostics.Tracing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security;
using System.Threading;
using System;
using MS.Internal.WindowsBase;


#if SYSTEM_XAML
namespace MS.Internal.Xaml
#else
namespace MS.Utility
#endif
{
    #region Trace

    static internal partial class EventTrace
    {
        static readonly internal TraceProvider EventProvider;

        // EasyTraceEvent
        // Checks the keyword and level before emiting the event
        static internal void EasyTraceEvent(Keyword keywords, Event eventID)
        {
            if (IsEnabled(keywords, Level.Info))
            {
                EventProvider.TraceEvent(eventID, keywords, Level.Info);
            }
        }

        // EasyTraceEvent
        // Checks the keyword and level before emiting the event
        static internal void EasyTraceEvent(Keyword keywords, Level level, Event eventID)
        {
            if (IsEnabled(keywords, level))
            {
                EventProvider.TraceEvent(eventID, keywords, level);
            }
        }

        // EasyTraceEvent
        // Checks the keyword and level before emiting the event
        static internal void EasyTraceEvent(Keyword keywords, Event eventID, object param1)
        {
            if (IsEnabled(keywords, Level.Info))
            {
                EventProvider.TraceEvent(eventID, keywords, Level.Info, param1);
            }
        }

        // EasyTraceEvent
        // Checks the keyword and level before emiting the event
        static internal void EasyTraceEvent(Keyword keywords, Level level, Event eventID, object param1)
        {
            if (IsEnabled(keywords, level))
            {
                EventProvider.TraceEvent(eventID, keywords, level, param1);
            }
        }

        // EasyTraceEvent
        // Checks the keyword and level before emiting the event
        static internal void EasyTraceEvent(Keyword keywords, Event eventID, object param1, object param2)
        {
            if (IsEnabled(keywords, Level.Info))
            {
                EventProvider.TraceEvent(eventID, keywords, Level.Info, param1, param2);
            }
        }

        static internal void EasyTraceEvent(Keyword keywords, Level level, Event eventID, object param1, object param2)
        {
            if (IsEnabled(keywords, Level.Info))
            {
                EventProvider.TraceEvent(eventID, keywords, Level.Info, param1, param2);
            }
        }

        // EasyTraceEvent
        // Checks the keyword and level before emiting the event
        static internal void EasyTraceEvent(Keyword keywords, Event eventID, object param1, object param2, object param3)
        {
            if (IsEnabled(keywords, Level.Info))
            {
                EventProvider.TraceEvent(eventID, keywords, Level.Info, param1, param2, param3);
            }
        }

        #region Trace related enumerations

        public enum LayoutSource : byte
        {
            LayoutManager,
            HwndSource_SetLayoutSize,
            HwndSource_WMSIZE
        }

        #endregion

        /// <summary>
        /// Callers use this to check if they should be logging.
        /// </summary>
        static internal bool IsEnabled(Keyword flag, Level level)
        {
            return EventProvider.IsEnabled(flag, level);
        }

        /// <summary>
        /// Internal operations associated with initializing the event provider and
        /// monitoring the Dispatcher and input components.
        /// </summary>
        ///<SecurityNote>
        /// Critical:  This calls critical code in TraceProvider
        /// TreatAsSafe:  it generates the GUID that is passed into the TraceProvider
        /// WPF versions prior to 4.0 used provider guid: {a42c77db-874f-422e-9b44-6d89fe2bd3e5}
        ///</SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        static EventTrace()
        {
            Guid providerGuid = new Guid("E13B77A8-14B6-11DE-8069-001B212B5009");

            if (Environment.OSVersion.Version.Major < 6 ||
                IsClassicETWRegistryEnabled())
            {
                EventProvider = new ClassicTraceProvider();
            }
            else
            {
                EventProvider = new ManifestTraceProvider();
            }
            EventProvider.Register(providerGuid);
        }

        [SecurityCritical]
        static bool IsClassicETWRegistryEnabled()
        {
            try
            {
                string regKey = @"HKEY_CURRENT_USER\Software\Microsoft\Avalon.Graphics\";
                new RegistryPermission(RegistryPermissionAccess.Read, regKey).Assert();
                
                return int.Equals(1, Microsoft.Win32.Registry.GetValue(regKey, "ClassicETW", 0));
            }
            finally
            {
                RegistryPermission.RevertAssert();
            }
        }
    }

    #endregion Trace

}
#endif 
