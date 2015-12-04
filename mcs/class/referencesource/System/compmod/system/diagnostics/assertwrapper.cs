// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.Permissions;
using System.Security;
using System.Collections;
using System.Globalization;
using System.Threading;
#if !SILVERLIGHT
using System.Runtime.Versioning;
#endif
using System.Diagnostics.CodeAnalysis;
using NativeMethods = Microsoft.Win32.NativeMethods;

namespace System.Diagnostics {

    internal static class AssertWrapper {

#if DEBUG && !FEATURE_PAL && !SILVERLIGHT
        static BooleanSwitch DisableVsAssert = new BooleanSwitch("DisableVsAssert", "Switch to disable usage of VSASSERT for DefaultTraceListener.");
        static volatile bool vsassertPresent = true;
        static Hashtable ignoredAsserts = new Hashtable(StringComparer.OrdinalIgnoreCase);

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private static void ShowVsAssert(string stackTrace, StackFrame frame, string message, string detailMessage) {
            int[] disable = new int[1];
            try {
                string detailMessage2;
                
                if (detailMessage == null)
                    detailMessage2 = stackTrace; 
                else
                    detailMessage2 = detailMessage + Environment.NewLine + stackTrace;                
                string fileName = (frame == null) ? string.Empty : frame.GetFileName();
                if (fileName == null) {
                    fileName = string.Empty;
                }

                int lineNumber = (frame == null) ? 0 : frame.GetFileLineNumber();
                int returnCode = VsAssert(detailMessage2, message, fileName, lineNumber, disable);
                if (returnCode != 0) {
                    if (!System.Diagnostics.Debugger.IsAttached) {
                        System.Diagnostics.Debugger.Launch();
                    }
                    System.Diagnostics.Debugger.Break();
                }
                if (disable[0] != 0)
                    ignoredAsserts[MakeAssertKey(fileName, lineNumber)] = null;
            }
            catch (Exception) {
                vsassertPresent = false;
            }
        }

        [DllImport(ExternDll.Fxassert, CharSet=System.Runtime.InteropServices.CharSet.Ansi, BestFitMapping=true)]
        [ResourceExposure(ResourceScope.None)]
        [SuppressMessage("Microsoft.Globalization","CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId="0", Justification="[....]: VsAssert isn't making a security decision here and they don't provide Unicode versions, also it is internal to MS")]
        [SuppressMessage("Microsoft.Globalization","CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId="1", Justification="[....]: VsAssert isn't making a security decision here and they don't provide Unicode versions, also it is internal to MS")]
        [SuppressMessage("Microsoft.Globalization","CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId="2", Justification="[....]: VsAssert isn't making a security decision here and they don't provide Unicode versions, also it is internal to MS")]
        public static extern int VsAssert(string message, string assert, string file, int line, [In, Out]int[] pfDisable);

        [ResourceExposure(ResourceScope.None)]
        public static void ShowAssert(string stackTrace, StackFrame frame, string message, string detailMessage) {
            bool userInteractive = Environment.UserInteractive;

            string fileName = (frame == null) ? string.Empty : frame.GetFileName();
            if (fileName == null) {
                fileName = string.Empty;
            }

            int lineNumber = (frame == null) ? 0 : frame.GetFileLineNumber();
            
            if (ignoredAsserts.ContainsKey(MakeAssertKey(fileName, lineNumber)))
                return;

            if (vsassertPresent && !DisableVsAssert.Enabled && userInteractive)
                ShowVsAssert(stackTrace, frame, message, detailMessage);

            // the following is not in an 'else' because vsassertPresent might
            // have gone from true to false.

            if (!vsassertPresent || DisableVsAssert.Enabled || !userInteractive)
                ShowMessageBoxAssert(stackTrace, message, detailMessage);
        }

        private static string MakeAssertKey(string fileName, int lineNumber) {
            return fileName + ":" + lineNumber.ToString(CultureInfo.InvariantCulture);
        }

#else // DEBUG && !FEATURE_PAL && !SILVERLIGHT

        public static void ShowAssert(string stackTrace, StackFrame frame, string message, string detailMessage) {
#if SILVERLIGHT && FEATURE_PAL
                ShowCFUserNotificationDisplayAlertAssert(stackTrace, message, detailMessage);
#else // !(SILVERLIGHT && FEATURE_PAL)
                ShowMessageBoxAssert(stackTrace, message, detailMessage);
#endif
        }

#endif // DEBUG && !FEATURE_PAL && !SILVERLIGHT

#if !SILVERLIGHT
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
#endif
        [SecuritySafeCritical]
        private static void ShowMessageBoxAssert(string stackTrace, string message, string detailMessage) {
            string fullMessage = message + Environment.NewLine + detailMessage + Environment.NewLine + stackTrace;

#if !FEATURE_PAL && !FEATURE_CORESYSTEM
            fullMessage = TruncateMessageToFitScreen(fullMessage);
#endif // !FEATURE_PAL && !FEATURE_CORESYSTEM

#if SILVERLIGHT
            int flags = 0x00000001 /*OkCancel*/ | 0x00000010 /*IconHand*/ |
                        0x00040000 /* TopMost */;
#else
            int flags = 0x00000002 /*AbortRetryIgnore*/ | 0x00000200 /*DefaultButton3*/ | 0x00000010 /*IconHand*/ |
                        0x00040000 /* TopMost */;
#endif

#if !SILVERLIGHT    
            if (!Environment.UserInteractive)
                flags = flags | 0x00200000 /*ServiceNotification */;
#endif

            if (IsRTLResources)
                flags = flags | SafeNativeMethods.MB_RIGHT | SafeNativeMethods.MB_RTLREADING;
            int rval = 0;
#if !SILVERLIGHT
            if (!UnsafeNativeMethods.IsPackagedProcess.Value)
            {
                rval = SafeNativeMethods.MessageBox(IntPtr.Zero, fullMessage, SR.GetString(SR.DebugAssertTitle), flags);
            }
            else
            {
#endif
            // Run the message box on its own thread.
            rval = new MessageBoxPopup(fullMessage, SR.GetString(SR.DebugAssertTitle), flags).ShowMessageBox();

#if !SILVERLIGHT
            }
#endif
            switch (rval) {
#if !SILVERLIGHT
                case 3: // abort
                    System.Environment.Exit(1);                    
                    break;
                case 4: // retry
#else
                case 2: // cancel
#endif
                    if (!System.Diagnostics.Debugger.IsAttached) {
                        System.Diagnostics.Debugger.Launch();
                    }
                    System.Diagnostics.Debugger.Break();
                    break;
            }
        }


        private static bool IsRTLResources {
            get {
                return SR.GetString(SR.RTL) != "RTL_False";
            }
        }

#if SILVERLIGHT && FEATURE_PAL
        [SecuritySafeCritical]
        private static void ShowCFUserNotificationDisplayAlertAssert(string stackTrace, string message, string detailMessage)
        {
            IntPtr cfsContinueText, cfsFullMessage, cfsTitle;

            string fullMessage = message + Environment.NewLine + detailMessage + Environment.NewLine + stackTrace;

            cfsFullMessage = SafeNativeMethods.CFStringCreateWithCharacters(IntPtr.Zero, fullMessage, fullMessage.Length);
            cfsContinueText = SafeNativeMethods.CFStringCreateWithCharacters(IntPtr.Zero, SR.GetString(SR.ContinueButtonText), SR.GetString(SR.ContinueButtonText).Length);
            cfsTitle = SafeNativeMethods.CFStringCreateWithCharacters(IntPtr.Zero, SR.GetString(SR.DebugAssertTitleShort), SR.GetString(SR.DebugAssertTitleShort).Length);

            uint rval = 0;

            SafeNativeMethods.CFUserNotificationDisplayAlert(0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, cfsTitle, cfsFullMessage, cfsContinueText, IntPtr.Zero, IntPtr.Zero, ref rval);

            SafeNativeMethods.CFRelease(cfsFullMessage);
            SafeNativeMethods.CFRelease(cfsContinueText);
            SafeNativeMethods.CFRelease(cfsTitle);
            
            // We don't care about the return value of CFUserNotificationDisplayAlert. 
        }
#endif


#if !FEATURE_PAL && !FEATURE_CORESYSTEM
        // Since MessageBox will grow taller than the screen if there are too many lines do
        // a rough calculation to make it fit.
#if !SILVERLIGHT
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
#endif
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Win32.UnsafeNativeMethods.ReleaseDC(System.IntPtr,System.IntPtr)", Justification = "[....]: If the DC is not released there's not much we can do.")]
        private static string TruncateMessageToFitScreen(string message) {            
            const int MaxCharsPerLine = 80;

            IntPtr hFont = SafeNativeMethods.GetStockObject(NativeMethods.DEFAULT_GUI_FONT);
            IntPtr hdc = UnsafeNativeMethods.GetDC(IntPtr.Zero);
            NativeMethods.TEXTMETRIC tm = new NativeMethods.TEXTMETRIC();
            hFont = UnsafeNativeMethods.SelectObject(hdc, hFont);
            SafeNativeMethods.GetTextMetrics(hdc, tm);
            UnsafeNativeMethods.SelectObject(hdc, hFont);
            UnsafeNativeMethods.ReleaseDC(IntPtr.Zero, hdc);
            hdc = IntPtr.Zero;
            int cy = UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
            int maxLines = cy / tm.tmHeight - 15;
    
            int lineCount = 0;
            int lineCharCount = 0;
            int i = 0;
            while (lineCount < maxLines && i < message.Length - 1) { 
                char ch = message[i];
                lineCharCount++;
                if (ch == '\n' || ch == '\r' || lineCharCount > MaxCharsPerLine) {
                    lineCount++;
                    lineCharCount = 0;
                }

                // treat \r\n or \n\r as a single line break
                if (ch == '\r' && message[i + 1] == '\n')  
                    i+=2;
                else if (ch == '\n' && message[i + 1] == '\r') 
                    i+=2;
                else
                    i++;
            }
            if (i < message.Length - 1)
                message = SR.GetString(SR.DebugMessageTruncated, message.Substring(0, i));
            return message;          
        }
#endif // !FEATURE_PAL && !FEATURE_CORESYSTEM
    }

    internal class MessageBoxPopup {
        public int ReturnValue { get; set; }
        private AutoResetEvent m_Event;
        private string m_Body;
        private string m_Title;
        private int m_Flags;

        [SecurityCritical]
        public MessageBoxPopup(string body, string title, int flags) {
            m_Event = new AutoResetEvent(false);
            m_Body = body;
            m_Title = title;
            m_Flags = flags;
        }

        public int ShowMessageBox() {

            Thread t = new Thread(DoPopup);
            t.Start();
            m_Event.WaitOne();
            return ReturnValue;
        }

        [SecuritySafeCritical]
        public void DoPopup() {
            ReturnValue = SafeNativeMethods.MessageBox(IntPtr.Zero, m_Body, m_Title, m_Flags);
            m_Event.Set();
        }
    }
}
