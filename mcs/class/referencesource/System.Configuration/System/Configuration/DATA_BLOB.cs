//------------------------------------------------------------------------------
// <copyright file="DATA_BLOB.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Configuration.Provider;
    using System.Xml;
    using System.Text;
    using  System.Runtime.InteropServices;
    using Microsoft.Win32;

    ////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential)]
    internal struct DATA_BLOB : IDisposable
    {
        public int cbData;
        public IntPtr pbData;
        void IDisposable.Dispose()
        {
            if (pbData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pbData);
                pbData = IntPtr.Zero;
            }
        }
    }
}
