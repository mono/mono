//------------------------------------------------------------------------------
// <copyright file="SafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace MS.Win32 {
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.Diagnostics;   


    using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use FriendAccessAllowedAttribute from an unknown assembly.
    using MS.Internal.YourAssemblyName;
#endif

    //<SecurityNote>
    // Critical - This entire class is critical as it has SuppressUnmanagedCodeSecurity. 
    // TreatAsSafe - These Native methods have been reviewed as safe to call. 
    //
    // The attributes are commented out here because this is a partial class and the attributes are already
    // applied in SafeNativeMethodsCLR.cs
    //</SecurityNote> 
    //[SecurityCritical, SecurityTreatAsSafe]
    //[SuppressUnmanagedCodeSecurity]
    [FriendAccessAllowed]
    internal partial class SafeNativeMethods
    {        
        //////////////////////////////
        // from Framework

        [Flags]
        internal enum PlaySoundFlags
        {
            SND_SYNC        =   0x00000000, /* play synchronously (default) */
            SND_ASYNC       =   0x00000001, /* play asynchronously */
            SND_NODEFAULT   =   0x00000002, /* silence (!default) if sound not found */
            SND_MEMORY      =   0x00000004, /* pszSound points to a memory file */
            SND_LOOP        =   0x00000008, /* loop the sound until next sndPlaySound */
            SND_NOSTOP      =   0x00000010, /* don't stop any currently playing sound */
            SND_PURGE       =   0x00000040, /* purge non-static events for task */
            SND_APPLICATION =   0x00000080, /* look for application specific association */
            SND_NOWAIT      =   0x00002000, /* don't wait if the driver is busy */
            SND_ALIAS       =   0x00010000, /* name is a registry alias */
            SND_FILENAME    =   0x00020000, /* name is file name */
            SND_RESOURCE    =   0x00040000, /* name is resource name or atom */
        }

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        internal static  bool InSendMessage()
        {
             return SafeNativeMethodsPrivate.InSendMessage();
        }


#if never
        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static int GetQueueStatus(uint flags)
        {
             return SafeNativeMethodsPrivate.GetQueueStatus(flags);
        }
        
        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        internal static  int GetInputState()
        {
             return SafeNativeMethodsPrivate.GetInputState();
        }
#endif

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static bool IsUxThemeActive() { return SafeNativeMethodsPrivate.IsThemeActive() != 0; }


        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static  bool SetCaretPos(int x, int y)
        {
            // To be consistent with our other PInvoke wrappers
            // we should "throw" a Win32Exception on error here.
            // But we don't want to introduce new "throws" w/o 
            // time to follow up on any new problems that causes.

            return SafeNativeMethodsPrivate.SetCaretPos(x,y);
        }
        

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static  bool DestroyCaret()
        {
            // To be consistent with our other PInvoke wrappers
            // we should "throw" a Win32Exception on error here.
            // But we don't want to introduce new "throws" w/o 
            // time to follow up on any new problems that causes.

            return SafeNativeMethodsPrivate.DestroyCaret();
        }

        // NOTE:  CLR has this in UnsafeNativeMethodsCLR.cs.  Not sure why it is unsafe - need to follow up.
        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static int GetCaretBlinkTime()
        {
            // To be consistent with our other PInvoke wrappers
            // we should "throw" a Win32Exception on error here.
            // But we don't want to introduce new "throws" w/o 
            // time to follow up on any new problems that causes.

            return SafeNativeMethodsPrivate.GetCaretBlinkTime();
        }

        // Constants for GetStringTypeEx.
        public const uint CT_CTYPE1 = 1;
        public const uint CT_CTYPE2 = 2;
        public const uint CT_CTYPE3 = 4;

        public const UInt16 C1_SPACE = 0x0008;
        public const UInt16 C1_PUNCT = 0x0010;
        public const UInt16 C1_BLANK = 0x0040;

        public const UInt16 C3_NONSPACING = 0x0001;
        public const UInt16 C3_DIACRITIC  = 0x0002;
        public const UInt16 C3_VOWELMARK  = 0x0004;
        public const UInt16 C3_KATAKANA   = 0x0010;
        public const UInt16 C3_HIRAGANA   = 0x0020;
        public const UInt16 C3_HALFWIDTH  = 0x0040;
        public const UInt16 C3_FULLWIDTH  = 0x0080;
        public const UInt16 C3_IDEOGRAPH  = 0x0100;
        public const UInt16 C3_KASHIDA    = 0x0200;

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static bool GetStringTypeEx(uint locale, uint infoType, char[] sourceString, int count,
            UInt16[] charTypes)
        {
            bool win32Return = SafeNativeMethodsPrivate.GetStringTypeEx(locale, infoType, sourceString, count, charTypes);
            int win32Error = Marshal.GetLastWin32Error();

            if (!win32Return)
            {
                throw new Win32Exception(win32Error);
            }

            return win32Return;
        }
        
        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
            public static int GetSysColor(int nIndex)
        {
                return SafeNativeMethodsPrivate.GetSysColor(nIndex);
        }

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: Exposes no critical data and doesn't affect clipboard state
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static bool IsClipboardFormatAvailable(int format)
        {
            return SafeNativeMethodsPrivate.IsClipboardFormatAvailable(format);
        }

#if never
        [DllImport(ExternDll.User32, EntryPoint = "DestroyIcon", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern bool IntDestroyIcon(NativeMethods.IconHandle hIcon);
        internal static void DestroyIcon(NativeMethods.IconHandle hIcon)
        {
            if (IntDestroyIcon(hIcon) == false)
            {
                throw new Win32Exception();
            }
        }
#endif
        /////////////////////////////
        // Used by BASE and FRAMEWORK

#if FRAMEWORK_NATIVEMETHODS || BASE_NATIVEMETHODS 


       /// <SecurityNote>
       ///     Critical: This code elevates to unmanaged code permission
       ///     TreatAsSafe: This function is safe to call
       /// </SecurityNote>
       [SecurityCritical,SecurityTreatAsSafe]
        public static bool  IsDebuggerPresent() { return SafeNativeMethodsPrivate.IsDebuggerPresent(); }
#endif
#if BASE_NATIVEMETHODS

        /////////////////////
        // used by BASE

        /// <SecurityNote>
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static void QueryPerformanceCounter(out long lpPerformanceCount)
        {
            if (!SafeNativeMethodsPrivate.QueryPerformanceCounter(out lpPerformanceCount))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <SecurityNote>
        /// Critical: This code elevates to unmanaged code permission
        /// TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        public static void QueryPerformanceFrequency(out long lpFrequency)
        { 
            if (!SafeNativeMethodsPrivate.QueryPerformanceFrequency(out lpFrequency))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        ///     TreatAsSafe: This function is safe to call
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        internal static  int GetMessageTime()
        {
            return SafeNativeMethodsPrivate.GetMessageTime();
        }
#endif // BASE_NATIVEMETHODS


        /// <SecurityNote>
        ///  This method accesses an UnsafeNativeMethod under an elevation.  This is
        ///  still safe because it just returns the style or ex style which we consider safe.
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        internal static Int32 GetWindowStyle(HandleRef hWnd, bool exStyle)
        {
            int nIndex = exStyle ? NativeMethods.GWL_EXSTYLE : NativeMethods.GWL_STYLE;
            return UnsafeNativeMethods.GetWindowLong(hWnd, nIndex);
        }
        [SuppressUnmanagedCodeSecurity]
        private static partial class SafeNativeMethodsPrivate
        {
            [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
            internal static extern bool InSendMessage();

//            [DllImport(ExternDll.User32, ExactSpelling = true)]
//            public static extern int GetQueueStatus(uint flags);
    
//            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
//            internal static extern int GetInputState();
            
            [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Unicode)]
            public static extern int IsThemeActive();
            
            [DllImport(ExternDll.User32, SetLastError=true, CharSet=CharSet.Auto)]
            public static extern bool SetCaretPos(int x, int y);

            [DllImport(ExternDll.User32, SetLastError=true, CharSet=CharSet.Auto)]
            public static extern bool DestroyCaret();

            // NOTE:  CLR has this in UnsafeNativeMethodsCLR.cs.  Not sure why it is unsafe - need to follow up.
            [DllImport(ExternDll.User32, SetLastError=true, CharSet=CharSet.Auto)]
            public static extern int GetCaretBlinkTime();

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool GetStringTypeEx(uint locale, uint infoType, char[] sourceString, int count,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] UInt16[] charTypes);
            
            [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int GetSysColor(int nIndex);

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern bool IsClipboardFormatAvailable(int format);

#if FRAMEWORK_NATIVEMETHODS || BASE_NATIVEMETHODS

            [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
              internal static extern bool IsDebuggerPresent();
#endif
#if BASE_NATIVEMETHODS

              [DllImport("kernel32.dll", SetLastError = true)]
              public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
        
              [DllImport("kernel32.dll", SetLastError = true)]
              public static extern bool QueryPerformanceFrequency(out long lpFrequency);
        
              [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
              internal static extern int GetMessageTime();
#endif
        }

    }
}

