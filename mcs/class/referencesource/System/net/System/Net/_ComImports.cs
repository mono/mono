//------------------------------------------------------------------------------
// <copyright file="_ComImports.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#define AUTOPROXY_MANAGED_JSCRIPT
#if !AUTOPROXY_MANAGED_JSCRIPT

namespace System.Net.ComImports
{
    using System.Runtime.InteropServices;
    using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

    //
    // HRESULTs
    //

    internal enum HRESULT
    {
        TYPE_E_ELEMENTNOTFOUND  = unchecked((int) 0x8002802B),
        SCRIPT_E_REPORTED       = unchecked((int) 0x80020101),
        E_NOTIMPL               = unchecked((int)0x80004001),
        E_NOINTERFACE           = unchecked((int)0x80004002),
        S_OK                    = 0x00000000,
        S_FALSE                 = 0x00000001
    }


    //
    // Scripting Interfaces
    //

    internal enum ScriptState : uint
    {
        Uninitialized   = 0,
        Started         = 1,
        Connected       = 2,
        Disconnected    = 3,
        Closed          = 4,
        Initialized     = 5,
    }

    internal enum ScriptThreadState : uint
    {
        NotInScript = 0,
        Running     = 1,
    }

    [Flags]
    internal enum ScriptText : uint
    {
        None                = 0x0000,

        DelayExecution      = 0x0001,
        IsVisible           = 0x0002,
        IsExpression        = 0x0020,
        IsPersistent        = 0x0040,
        HostManageSource    = 0x0080,
    }

    [Flags]
    internal enum ScriptItem : uint
    {
        None                = 0x0000,

        IsVisible           = 0x0002,
        IsSource            = 0x0004,
        GlobalMembers       = 0x0008,
        IsPersistent        = 0x0040,
        CodeOnly            = 0x0200,
        NoCode              = 0x0400,
    }

    [Flags]
    internal enum ScriptInfo : uint
    {
        None        = 0x0000,

        IUnknown    = 0x0001,
        ITypeInfo   = 0x0002,
    }

    //
    // A "fake" interface to use to interact with the script itself through IDispatch.
    // It uses the IDispatch guid.  New methods can be added as needed in any order.
    //
    [ComImport]
    [Guid("00020400-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    internal interface IScript
    {
        object FindProxyForURL(string url, string host);
    }

    [ComImport]
    [Guid("BB1A2AE1-A4F9-11cf-8F20-00805F2CD064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScript
    {
        void SetScriptSite(IActiveScriptSite pass);
        void GetScriptSite(Guid riid, out IntPtr site);
        void SetScriptState(ScriptState state);
        void GetScriptState(out ScriptState scriptState);
        void Close();
        void AddNamedItem(string name, ScriptItem flags);
        void AddTypeLib(Guid typeLib, uint major, uint minor, uint flags);
        void GetScriptDispatch(string itemName, out IScript dispatch);
        void GetCurrentScriptThreadID(out uint thread);
        void GetScriptThreadID(uint win32ThreadId, out uint thread);
        void GetScriptThreadState(uint thread, out ScriptThreadState state);
        void InterruptScriptThread(uint thread, out EXCEPINFO exceptionInfo, uint flags);
        void Clone(out IActiveScript script);
    }

    [ComImport]
    [Guid("DB01A1E3-A42B-11cf-8F20-00805F2CD064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptSite
    {
        void GetLCID(out int lcid);
        void GetItemInfo(
            string name,
            ScriptInfo returnMask,
            [Out] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown)] object[] item,
            [Out] [MarshalAs(UnmanagedType.LPArray)] IntPtr[] typeInfo);
        void GetDocVersionString(out string version);
        void OnScriptTerminate(object result, EXCEPINFO exceptionInfo);
        void OnStateChange(ScriptState scriptState);
        void OnScriptError(IActiveScriptError scriptError);
        void OnEnterScript();
        void OnLeaveScript();
    }

    internal enum UrlPolicy {
        DisAllow = 0x03
    }

    [ComImport(), Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOleServiceProvider {
        [PreserveSig]
        int QueryService([In] ref Guid guidService, [In] ref Guid riid, [Out] out IntPtr ppvObject);
    }

    internal static class ComConstants {
        public const int INTERFACE_USES_SECURITY_MANAGER = 0x00000008; // Object knows to use IInternetHostSecurityManager
    }

    [ComImport]
    [Guid("CB5BDC81-93C1-11cf-8F20-00805F2CD064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IObjectSafety
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetInterfaceSafetyOptions(ref Guid riid, [Out] out int pdwSupportedOptions, [Out] out int pdwEnabledOptions);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions);
    }

    [ComImport]
    [Guid("3af280b6-cb3f-11d0-891e-00c04fb6bfc4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInternetHostSecurityManager
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetSecurityId([Out] byte[] pbSecurityId, [In, Out]ref IntPtr pcbSecurityId, IntPtr dwReserved);
        
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ProcessUrlAction(int dwAction, [Out] int[] pPolicy, int cbPolicy, [Out] byte[] pContext, int cbContext, int dwFlags, int dwReserved);
        
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int QueryCustomPolicy(Guid guidKey, [Out] out byte[] ppPolicy, [Out] out int pcbPolicy, byte[] pContext, int cbContext, int dwReserved);
    }

    [ComImport]
    [Guid("EAE1BA61-A4ED-11cf-8F20-00805F2CD064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptError
    {
        void GetExceptionInfo(out EXCEPINFO exceptionInfo);
        void GetSourcePosition(out uint sourceContext, out uint lineNumber, out int characterPosition);
        void GetSourceLineText(out string sourceLine);
    }

    [ComImport]
    [Guid("BB1A2AE2-A4F9-11cf-8F20-00805F2CD064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptParse32
    {
        void InitNew();

        void AddScriptlet(
            string defaultName,
            string code,
            string itemName,
            string subItemName,
            string eventName,
            string delimiter,
            IntPtr sourceContextCookie,
            uint startingLineNumber,
            ScriptText flags,
            out string name,
            out EXCEPINFO exceptionInfo);

        void ParseScriptText(
            string code,
            string itemName,
            [MarshalAs(UnmanagedType.IUnknown)] object context,
            string delimiter,
            IntPtr sourceContextCookie,
            uint startingLineNumber,
            ScriptText flags,
            [MarshalAs(UnmanagedType.Struct)] out object result,
            out EXCEPINFO exceptionInfo);
    }

    [ComImport]
    [Guid("C7EF7658-E1EE-480E-97EA-D52CB4D76D17")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptParse64
    {
        void InitNew();

        void AddScriptlet(
            string defaultName,
            string code,
            string itemName,
            string subItemName,
            string eventName,
            string delimiter,
            IntPtr sourceContextCookie,
            uint startingLineNumber,
            ScriptText flags,
            out string name,
            out EXCEPINFO exceptionInfo);

        void ParseScriptText(
            string code,
            string itemName,
            [MarshalAs(UnmanagedType.IUnknown)] object context,
            string delimiter,
            IntPtr sourceContextCookie,
            uint startingLineNumber,
            ScriptText flags,
            [MarshalAs(UnmanagedType.Struct)] out object result,
            out EXCEPINFO exceptionInfo);
    }

    // Use this helper to deal with the fact that two interfaces with different Guids are required on 32 vs. 64 bit.
    internal class ActiveScriptParseWrapper
    {
        private readonly IActiveScriptParse32 asp32;
        private readonly IActiveScriptParse64 asp64;

        internal ActiveScriptParseWrapper(object comObject)
        {
            if (IntPtr.Size == 4)
            {
                asp32 = (IActiveScriptParse32) comObject;
            }
            else
            {
                asp64 = (IActiveScriptParse64) comObject;
            }
        }

        internal void InitNew()
        {
            if (asp32 != null)
            {
                asp32.InitNew();
            }
            else
            {
                asp64.InitNew();
            }
        }

        internal void AddScriptlet(
            string defaultName,
            string code,
            string itemName,
            string subItemName,
            string eventName,
            string delimiter,
            IntPtr sourceContextCookie,
            uint startingLineNumber,
            ScriptText flags,
            out string name,
            out EXCEPINFO exceptionInfo)
        {
            if (asp32 != null)
            {
                asp32.AddScriptlet(defaultName, code, itemName, subItemName, eventName,
                    delimiter, sourceContextCookie, startingLineNumber, flags, out name, out exceptionInfo);
            }
            else
            {
                asp64.AddScriptlet(defaultName, code, itemName, subItemName, eventName,
                    delimiter, sourceContextCookie, startingLineNumber, flags, out name, out exceptionInfo);
            }
        }

        internal void ParseScriptText(
            string code,
            string itemName,
            [MarshalAs(UnmanagedType.IUnknown)] object context,
            string delimiter,
            IntPtr sourceContextCookie,
            uint startingLineNumber,
            ScriptText flags,
            [MarshalAs(UnmanagedType.Struct)] out object result,
            out EXCEPINFO exceptionInfo)
        {
            if (asp32 != null)
            {
                asp32.ParseScriptText(code, itemName, context, delimiter, sourceContextCookie,
                    startingLineNumber, flags, out result, out exceptionInfo);
            }
            else
            {
                asp64.ParseScriptText(code, itemName, context, delimiter, sourceContextCookie,
                    startingLineNumber, flags, out result, out exceptionInfo);
            }
        }
    }


    //
    // COM Classes
    //

    [ComImport]
    [Guid("f414c260-6ac0-11cf-b6d1-00aa00bbbb58")]
    internal class JScriptEngine { }
}

#endif
