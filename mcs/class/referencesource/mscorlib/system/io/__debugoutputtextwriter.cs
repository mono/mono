// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

#if _DEBUG
// This class writes to wherever OutputDebugString writes to.  If you don't have
// a Windows app (ie, something hosted in IE), you can use this to redirect Console
// output for some good old-fashioned console spew in MSDEV's debug output window.

// <STRIP>This really shouldn't ship at all, but is intended as a quick, inefficient hack
// for debugging.  -- [....], 9/26/2000</STRIP>

using System;
using System.IO;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;
using System.Globalization;

namespace System.IO {
    internal class __DebugOutputTextWriter : TextWriter {
        private readonly String _consoleType;

        internal __DebugOutputTextWriter(String consoleType): base(CultureInfo.InvariantCulture)
        {
            _consoleType = consoleType;
        }

        public override Encoding Encoding {
#if FEATURE_CORECLR
            [System.Security.SecuritySafeCritical] 
#endif
            get {
                if (Marshal.SystemDefaultCharSize == 1)
                    return Encoding.Default;
                else
                    return new UnicodeEncoding(false, false);
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override void Write(char c)
        {
            OutputDebugString(c.ToString());
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override void Write(String str)
        {
            OutputDebugString(str);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override void Write(char[] array)
        {
            if (array != null) 
                OutputDebugString(new String(array));
        }
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        public override void WriteLine(String str)
        {
            if (str != null)
                OutputDebugString(_consoleType + str);
            else
                OutputDebugString("<null>");
            OutputDebugString(new String(CoreNewLine));
        }

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(Win32Native.KERNEL32, CharSet=CharSet.Auto)]
        [SuppressUnmanagedCodeSecurityAttribute()]
        [ResourceExposure(ResourceScope.None)]
        private static extern void OutputDebugString(String output);
    }
}
       
#endif // _DEBUG
