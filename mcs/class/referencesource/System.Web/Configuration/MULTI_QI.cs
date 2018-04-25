//------------------------------------------------------------------------------
// <copyright file="MULTI_QI.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Collections;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.Web;
    using System.Web.Util;
    using System.Security;
    using System.IO;
    using System.Web.Hosting;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Security.Principal;
    using System.Threading;
    using System.Globalization;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct MULTI_QI : IDisposable
    {
        internal MULTI_QI(IntPtr pid) {
            piid = pid;
            pItf = IntPtr.Zero;
            hr = 0;
        }

        internal IntPtr piid;        // 'Guid' can't be marshaled to GUID* here? use IntPtr buffer trick instead
        internal IntPtr pItf;
        internal int hr;

        void IDisposable.Dispose()
        {
            if (pItf != IntPtr.Zero)
            {
                Marshal.Release(pItf);
                pItf = IntPtr.Zero;
            }
            if (piid != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(piid);
                piid = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);

        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct MULTI_QI_X64 : IDisposable
    {
        internal MULTI_QI_X64(IntPtr pid)
        {
            piid = pid;
            pItf = IntPtr.Zero;
            hr = 0;
            padding = 0;
        }

        internal IntPtr piid;        // 'Guid' can't be marshaled to GUID* here? use IntPtr buffer trick instead
        internal IntPtr pItf;
        internal int hr;
#pragma warning disable 0649
        internal int padding;
#pragma warning restore 0649

        void IDisposable.Dispose()
        {
            if (pItf != IntPtr.Zero) {
                Marshal.Release(pItf);
                pItf = IntPtr.Zero;
            }
            if (piid != IntPtr.Zero) {
                Marshal.FreeCoTaskMem(piid);
                piid = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);

        }
    }
}
