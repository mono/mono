//------------------------------------------------------------------------------
//  Microsoft Avalon
//  Copyright (c) Microsoft Corporation, 2004
//
//  File: SafeSystemMetrics.cs
//  This class is copied from the system metrics class in frameworks. The
//  reason it exists is to consolidate all system metric calls through one layer
//  so that maintenance from a security stand point gets easier. We will add
//  mertrics on a need basis. The caching code is removed since the original calls 
//  that were moved here do not rely on caching. If there is a percieved perf. problem
//  we can work on enabling this.
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Microsoft.Win32;
using System.Security;
using System.Security.Permissions;
using MS.Win32;
using MS.Internal;
using MS.Internal.Interop;

using MS.Internal.PresentationCore;

namespace MS.Win32
{
    /// <summary>
    ///     Contains properties that are queries into the system's various settings.
    /// </summary>
    [FriendAccessAllowed] // Built into Core, also used by Framework.
    internal sealed class SafeSystemMetrics
    {

        private SafeSystemMetrics()
        {
        }

#if !PRESENTATION_CORE
        /// <summary>
        ///     Maps to SM_CXVIRTUALSCREEN
        /// </summary>
        /// <SecurityNote>
        ///    TreatAsSafe --There exists a demand
        ///    Security Critical -- Calling UnsafeNativeMethods
        /// </SecurityNote>
        internal static int VirtualScreenWidth
        {
            [SecurityCritical,SecurityTreatAsSafe]
            get
            {
                SecurityHelper.DemandUnmanagedCode();                

                return UnsafeNativeMethods.GetSystemMetrics(SM.CXVIRTUALSCREEN);
            }
        }

        /// <summary>
        ///     Maps to SM_CYVIRTUALSCREEN
        /// </summary>
        /// <SecurityNote>
        ///    TreatAsSafe --There exists a demand
        ///    Security Critical -- Calling UnsafeNativeMethods
        /// </SecurityNote>
        internal static int VirtualScreenHeight
        {
            [SecurityCritical,SecurityTreatAsSafe]
            get
            {
                SecurityHelper.DemandUnmanagedCode();
                return UnsafeNativeMethods.GetSystemMetrics(SM.CYVIRTUALSCREEN);
            }
        }
#endif //end !PRESENTATIONCORE

        /// <summary>
        ///     Maps to SM_CXDOUBLECLK
        /// </summary>
        /// <SecurityNote>
        ///     TreatAsSafe --This data is safe to expose
        ///     Security Critical -- Calling UnsafeNativeMethods
        /// </SecurityNote>
        internal static int DoubleClickDeltaX
        {
            [SecurityCritical, SecurityTreatAsSafe]
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(SM.CXDOUBLECLK);
            }
        }

        /// <summary>
        ///     Maps to SM_CYDOUBLECLK
        /// </summary>
        /// <SecurityNote>
        ///    TreatAsSafe --This data is safe to expose
        ///    Security Critical -- Calling UnsafeNativeMethods
        /// </SecurityNote>
        internal static int DoubleClickDeltaY
        {
            [SecurityCritical, SecurityTreatAsSafe]
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(SM.CYDOUBLECLK);
            }
        }

            
        /// <summary>
        ///     Maps to SM_CXDRAG
        /// </summary>
        /// <SecurityNote>
        ///     TreatAsSafe --This data is safe to expose
        ///     Security Critical -- Calling UnsafeNativeMethods
        /// </SecurityNote>
        internal static int DragDeltaX
        {
            [SecurityCritical, SecurityTreatAsSafe]
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(SM.CXDRAG);
            }
        }

        /// <summary>
        ///     Maps to SM_CYDRAG
        /// </summary>
        /// <SecurityNote>
        ///    TreatAsSafe --This data is safe to expose
        ///    Security Critical -- Calling UnsafeNativeMethods
        /// </SecurityNote>
        internal static int DragDeltaY
        {
            [SecurityCritical, SecurityTreatAsSafe]
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(SM.CYDRAG);
            }
        }

        ///<summary> 
        /// Is an IMM enabled ? Maps to SM_IMMENABLED
        ///</summary> 
        ///<SecurityNote> 
        ///Critical - calls a method that performs an elevation. 
        /// TreatAsSafe - data is considered safe to expose. 
        ///</SecurityNote> 
        internal static bool IsImmEnabled
        {
            [SecurityCritical, SecurityTreatAsSafe]
            get
            {
                return  (UnsafeNativeMethods.GetSystemMetrics(SM.IMMENABLED) != 0);
            }

        }

    }
}
