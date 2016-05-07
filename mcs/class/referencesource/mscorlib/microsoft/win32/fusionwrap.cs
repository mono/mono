// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//-------------------------------------------------------------
// FusionInterfaces.cs
//
// This implements wrappers to Fusion interfaces
//-------------------------------------------------------------
namespace Microsoft.Win32
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Globalization;     
    using StringBuilder = System.Text.StringBuilder;

    //-------------------------------------------------------------
    // Interfaces defined by fusion
    //-------------------------------------------------------------
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
    interface IAssemblyEnum
    { 
        [PreserveSig()]
        int GetNextAssembly(out IApplicationContext ppAppCtx, out IAssemblyName ppName, uint dwFlags);
        [PreserveSig()]
        int Reset();
        [PreserveSig()]
        int Clone(out IAssemblyEnum ppEnum);
    }
    
    [ComImport,InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7c23ff90-33af-11d3-95da-00a024a85b51")]
    interface IApplicationContext
    {
        void SetContextNameObject(IAssemblyName pName); 
        void GetContextNameObject(out IAssemblyName ppName);
        void Set([MarshalAs(UnmanagedType.LPWStr)] String szName, int pvValue, uint cbValue, uint dwFlags);
        void Get([MarshalAs(UnmanagedType.LPWStr)] String szName, out int pvValue, ref uint pcbValue, uint dwFlags);
        void GetDynamicDirectory(out int wzDynamicDir, ref uint pdwSize);
    }
    
    
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
    interface IAssemblyName
    {
        [PreserveSig()]
        int SetProperty(uint PropertyId, IntPtr pvProperty, uint cbProperty);
        [PreserveSig()]
        int GetProperty(uint PropertyId, IntPtr pvProperty, ref uint pcbProperty);
        [PreserveSig()]
        int Finalize();
        [PreserveSig()]
        int GetDisplayName(IntPtr szDisplayName, ref uint pccDisplayName, uint dwDisplayFlags);
        [PreserveSig()]
        int BindToObject(Object /*REFIID*/ refIID,  
                         Object /*IAssemblyBindSink*/ pAsmBindSink, 
                         IApplicationContext pApplicationContext,
                         [MarshalAs(UnmanagedType.LPWStr)] String szCodeBase,
                         Int64 llFlags,
                         int pvReserved,
                         uint cbReserved,
                         out int ppv);
        [PreserveSig()] 
        int GetName(out uint lpcwBuffer, out int pwzName);
        [PreserveSig()]
        int GetVersion(out uint pdwVersionHi, out uint pdwVersionLow);
        [PreserveSig()]
        int IsEqual(IAssemblyName pName, uint dwCmpFlags);
        [PreserveSig()]
        int Clone(out IAssemblyName pName);
    }

    internal static class ASM_CACHE
    {
         public const uint ZAP          = 0x1;
         public const uint GAC          = 0x2;
         public const uint DOWNLOAD     = 0x4;
    }

    internal static class CANOF
    {
        public const uint PARSE_DISPLAY_NAME = 0x1;
        public const uint SET_DEFAULT_VALUES = 0x2;
    }
    
    internal static class ASM_NAME
    {   
         public const uint PUBLIC_KEY            = 0;
         public const uint PUBLIC_KEY_TOKEN      = PUBLIC_KEY + 1;
         public const uint HASH_VALUE            = PUBLIC_KEY_TOKEN + 1;
         public const uint NAME                  = HASH_VALUE + 1;
         public const uint MAJOR_VERSION         = NAME + 1;
         public const uint MINOR_VERSION         = MAJOR_VERSION + 1;
         public const uint BUILD_NUMBER          = MINOR_VERSION + 1;
         public const uint REVISION_NUMBER       = BUILD_NUMBER + 1;
         public const uint CULTURE               = REVISION_NUMBER + 1;
         public const uint PROCESSOR_ID_ARRAY    = CULTURE + 1;
         public const uint OSINFO_ARRAY          = PROCESSOR_ID_ARRAY + 1;
         public const uint HASH_ALGID            = OSINFO_ARRAY + 1;
         public const uint ALIAS                 = HASH_ALGID + 1;
         public const uint CODEBASE_URL          = ALIAS + 1;
         public const uint CODEBASE_LASTMOD      = CODEBASE_URL + 1;
         public const uint NULL_PUBLIC_KEY       = CODEBASE_LASTMOD + 1;
         public const uint NULL_PUBLIC_KEY_TOKEN  = NULL_PUBLIC_KEY + 1;
         public const uint CUSTOM                = NULL_PUBLIC_KEY_TOKEN + 1;
         public const uint NULL_CUSTOM           = CUSTOM + 1;
         public const uint MVID                  = NULL_CUSTOM + 1;
         public const uint _32_BIT_ONLY          = MVID + 1;
         public const uint MAX_PARAMS            = _32_BIT_ONLY + 1;
     }

    internal static class Fusion
    {
        [System.Security.SecurityCritical]  // auto-generated
        public static void ReadCache(ArrayList alAssems, String name, uint nFlag)
        {
            IAssemblyEnum aEnum = null;
            IAssemblyName aName = null;
            IAssemblyName aNameEnum = null;
            IApplicationContext AppCtx = null;
            int hr;

            if (name != null)
            {
                hr = Win32Native.CreateAssemblyNameObject(out aNameEnum, name, CANOF.PARSE_DISPLAY_NAME, IntPtr.Zero);
                if (hr != 0)
                    Marshal.ThrowExceptionForHR(hr);
            }

            hr = Win32Native.CreateAssemblyEnum(out aEnum, AppCtx, aNameEnum, nFlag, IntPtr.Zero);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);

            for (; ; )
            {
                hr = aEnum.GetNextAssembly(out AppCtx, out aName, 0);
                if (hr != 0)
                {
                    if (hr < 0)
                        Marshal.ThrowExceptionForHR(hr);
                    break;
                }

                String sDisplayName = GetDisplayName(aName, 0);
                if (sDisplayName == null)
                    continue;

                alAssems.Add(sDisplayName);
            } // for (;;)
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        static unsafe String GetDisplayName(IAssemblyName aName, uint dwDisplayFlags)
        {
            uint iLen=0;            
            String sDisplayName = null;

            aName.GetDisplayName((IntPtr)0, ref iLen, dwDisplayFlags);
            if (iLen > 0) {
                IntPtr pDisplayName=(IntPtr)0;
               
                // Do some memory allocating here
                // We need to assume that a wide character is 2 bytes.
                byte[] data = new byte[((int)iLen+1)*2];
                fixed (byte *dataptr = data) {
                    pDisplayName = new IntPtr((void *) dataptr);
                    aName.GetDisplayName(pDisplayName, ref iLen, dwDisplayFlags);
                    sDisplayName = Marshal.PtrToStringUni(pDisplayName);                
                }
            }
            
            return sDisplayName;
        }
        
    }
 }
