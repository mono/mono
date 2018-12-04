//---------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
//---------------------------------------------------------------------------

using System;

#if WINDOWS_BASE
namespace MS.Internal.WindowsBase.Interop
#elif PRESENTATION_CORE
namespace System.Windows.Interop
#elif PRESENTATIONFRAMEWORK
namespace MS.Internal.PresentationFramework.Interop
#elif UIAUTOMATIONTYPES
namespace MS.Internal.UIAutomationTypes.Interop
#else
namespace Microsoft.Internal.Interop
#endif
{
    /// <summary>
    /// This is not a complete list of operating system versions and service packs.
    /// These are the interesting versions where features or behaviors were introduced 
    /// or changed, and code needs to detect those points and do different things.
    ///
    /// This list has been expanded in order to support our new OSVersionHelper lib.
    ///
    /// If you need to add an OS, do the following steps:
    ///     Create the appropriate native function in OSVersionHelper (see shared/OSVersionHelper/inc/OSVersionHelper.cpp)
    ///     Create the appropriate export in PresentationNative (see native/dll/dllentry.cpp)
    ///     Add it to the OperatingSystemVersion enumeration (Congrats, you're here already!)
    ///     Create the appropriate managed wrappers (see OSVersionHelper.cs)
    ///     Detect your freshly minted OS!
    /// </summary>
    internal enum OperatingSystemVersion
    {
        WindowsXPSP2,
        WindowsXPSP3,
        WindowsVista,
        WindowsVistaSP1,
        WindowsVistaSP2,
        Windows7,
        Windows7SP1,
        Windows8,
        Windows8Point1,
        Windows10,
        Windows10TH2,
        Windows10RS1,
        Windows10RS2,
        Windows10RS3,
    }
}
