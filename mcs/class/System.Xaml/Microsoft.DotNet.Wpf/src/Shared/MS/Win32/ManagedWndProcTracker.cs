//#define LOGGING

using System;
using System.Collections;
using System.Threading;

using System.Runtime.InteropServices;
using MS.Internal;
using MS.Internal.Interop;
using System.Security;
using System.Security.Permissions;

// The SecurityHelper class differs between assemblies and could not actually be
//  shared, so it is duplicated across namespaces to prevent name collision.
#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use a class (duplicated across multiple namespaces) from an unknown assembly.
#endif

namespace MS.Win32
{
    internal static class ManagedWndProcTracker
    {
        /// <SecurityNote>
        ///     Critical: This code calls into Link demanded methods to attach handlers
        ///     TreatAsSafe: This code does not take any parameter or return state.
        ///     It simply attaches private call back.
        /// </SecurityNote>
        [SecurityCritical,SecurityTreatAsSafe]
        static ManagedWndProcTracker()
        {
            // Listen for ProcessExit so we can detach ourselves when the CLR shuts down
            // and avoid unmanaged code from calling back in to managed code during shutdown.
            ManagedWndProcTrackerShutDownListener listener = new ManagedWndProcTrackerShutDownListener();
        }

        /// <SecurityNote>
        ///     Critical: Uses critical member _hwndList
        /// </SecurityNote>
        [SecurityCritical]
        internal static void TrackHwndSubclass(HwndSubclass subclass, IntPtr hwnd)
        {
            lock (_hwndList)
            {
                // We use HwndSubclass as the key and the hwnd ptr as the value.
                // This supports the case where two (or more) HwndSubclasses
                // get attached to the same Hwnd.  At AppDomain shutdown, we may
                // end up sending the Detach message to the Hwnd more than once,
                // but that won't cause any harm.
                _hwndList[subclass] = hwnd;
            }

#if LOGGING
            LogStartHWND(hwnd, "Core HwndWrapper..ctor");
#endif
        }

        /// <SecurityNote>
        ///     Critical: Uses critical member _hwndList
        /// </SecurityNote>
        [SecurityCritical]
        internal static void UnhookHwndSubclass(HwndSubclass subclass)
        {
            // if exiting the AppDomain, ignore this call.  This avoids changing
            // the list during the loop in OnAppDomainProcessExit
            if (_exiting)
                return;

            lock (_hwndList)
            {
                _hwndList.Remove(subclass);
            }
        }

        ///<SecurityNote>
        ///     Critical performs an elevation to call HookUpDefWindowProc.
        ///     TreatAsSafe - net effect of this is to remove our already registered WndProc's on domain shutdown.
        ///                          safe - as you had to elevate to add these already. Removing them is considered safe.
        ///</SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        private static void OnAppDomainProcessExit()
        {
            // AppDomain is exiting -- if anyone tries to call back into managed code
            // after this point, bad things will happen.  We must remove all unmanaged
            // code references to our WndProc delegates.  USER will explode if we set the
            // WndProc to null, so the next most reasonable thing we can do is hook up
            // the DefaultWindowProc.
            //DbgUserBreakPoint();

            _exiting = true;

            lock (_hwndList)
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert(); // BlessedAssert:
                try
                {
                    foreach (DictionaryEntry entry in _hwndList)
                    {
                        IntPtr hwnd = (IntPtr)entry.Value;

                        int windowStyle = UnsafeNativeMethods.GetWindowLong(new HandleRef(null,hwnd), NativeMethods.GWL_STYLE);
                        if((windowStyle & NativeMethods.WS_CHILD) != 0)
                        {
                            // Tell all the HwndSubclass WndProcs for WS_CHILD windows
                            // to detach themselves. This is particularly important when
                            // the parent hwnd belongs to a separate AppDomain in a
                            // cross AppDomain hosting scenario. In this scenario it is
                            // possible that the host has subclassed the WS_CHILD window
                            // and hence it is important to notify the host before we set the
                            // WndProc to DefWndProc. Also note that we do not want to make a
                            // blocking SendMessage call to all the subclassed Hwnds in the
                            // AppDomain because this can lead to slow shutdown speed.
                            // Eg. Consider a MessageOnlyHwnd created and subclassed on a
                            // worker thread which is no longer responsive. The SendMessage
                            // call in this case will block. To avoid this we limit the conversation
                            // only to WS_CHILD windows. We understand that this solution is
                            // not foolproof but it is the best outside of re-designing the cleanup
                            // of Hwnd subclasses.

                            UnsafeNativeMethods.SendMessage(hwnd, HwndSubclass.DetachMessage,
                                                                IntPtr.Zero /* wildcard */,
                                                                (IntPtr) 2 /* force and forward */);
                        }

                        // the last WndProc on the chain might be managed as well
                        // (see HwndSubclass.SubclassWndProc for explanation).
                        // Just in case, restore the DefaultWindowProc.
                        HookUpDefWindowProc(hwnd);
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }

            }
        }

        /// <SecurityNote>
        ///  TreatAsSafe:  Demands for unmanaged code
        ///  Critical: Elevates by calling an unverifieds UnsafeNativeMethod call
        ///</SecurityNote>
        [SecurityTreatAsSafe, SecurityCritical]
        private static void HookUpDefWindowProc(IntPtr hwnd)
        {

            SecurityHelper.DemandUnmanagedCode();

#if LOGGING
            LogFinishHWND(hwnd, "Core HookUpDWP");
#endif

            IntPtr result = IntPtr.Zero ;

            // We've already cleaned up, return immediately.
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            IntPtr defWindowProc = GetDefWindowProcAddress(hwnd);

            if (defWindowProc != IntPtr.Zero)
            {
                try
                {
                    result = UnsafeNativeMethods.SetWindowLong(new HandleRef(null,hwnd), NativeMethods.GWL_WNDPROC, defWindowProc);

                }
                catch(System.ComponentModel.Win32Exception e)
                {
                    // We failed to change the window proc.  Now what?

                    if (e.NativeErrorCode != 1400) // ERROR_INVALID_WINDOW_HANDLE
                    {
                        // For debugging purposes, throw an exception so we can debug
                        // this and know if it's possible to call SetWindowLong on
                        // the wrong thread.
                        throw;
                    }
                }
                if (result != IntPtr.Zero )
                {
                    UnsafeNativeMethods.PostMessage(new HandleRef(null,hwnd), WindowMessage.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }

        // Get the DWP for the given HWND -- returns DefWindowProcA or DefWindowProcW
        // depending on IsWindowUnicode(hwnd).
        private static IntPtr GetDefWindowProcAddress(IntPtr hwnd)
        {
            // We need to swap back in the DefWindowProc, but which one we use depends on
            // what the Unicode-ness of the window.
            if (SafeNativeMethods.IsWindowUnicode(new HandleRef(null,hwnd)))
            {
                if (_cachedDefWindowProcW == IntPtr.Zero)
                {
                    _cachedDefWindowProcW = GetUser32ProcAddress("DefWindowProcW");
                }

                return _cachedDefWindowProcW;
            }
            else
            {
                if (_cachedDefWindowProcA == IntPtr.Zero)
                {
                    _cachedDefWindowProcA = GetUser32ProcAddress("DefWindowProcA") ;
                }

                return _cachedDefWindowProcA;
            }
        }

        ///<SecurityNote>
        ///  SecurityCritical: elevates via a call to unsafe native methods
        ///  SecurityTreatAsSafe: Demands unmgd code permission via SecurityHelper
        ///</SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        private static IntPtr GetUser32ProcAddress(string export)
        {

            SecurityHelper.DemandUnmanagedCode();
            IntPtr hModule = UnsafeNativeMethods.GetModuleHandle(ExternDll.User32);


            if (hModule != IntPtr.Zero)
            {
                return UnsafeNativeMethods.GetProcAddress(new HandleRef(null, hModule), export);

            }
            return IntPtr.Zero;
        }

        private sealed class ManagedWndProcTrackerShutDownListener : ShutDownListener
        {
            /// <SecurityNote>
            ///     Critical: accesses AppDomain.DomainUnload event
            ///     TreatAsSafe: This code does not take any parameter or return state.
            ///                  It simply attaches private callbacks.
            /// </SecurityNote>
            [SecurityCritical,SecurityTreatAsSafe]
            public ManagedWndProcTrackerShutDownListener()
                : base(null, ShutDownEvents.AppDomain)
            {
            }

            internal override void OnShutDown(object target, object sender, EventArgs e)
            {
                ManagedWndProcTracker.OnAppDomainProcessExit();
            }
        }

#if LOGGING
        [DllImport("ntdll.dll")]
        private static extern void DbgUserBreakPoint();

        [DllImport("ntdll.dll")]
        private static extern void DbgPrint(string msg);

        internal static void LogStartHWND(IntPtr hwnd, string fromWhere)
        {
            string msg = String.Format("BEGIN: {0:X} -- Setting DWP, process = {1} ({2}) {3}",
                   hwnd,
                   System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                   fromWhere,
                   System.Environment.NewLine);

            Log(msg);
        }

        internal static void LogFinishHWND(IntPtr hwnd, string fromWhere)
        {
            string msg = String.Format("END:   {0:X} -- Setting DWP, process = {1} ({2}) {3}",
                   hwnd,
                   System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                   fromWhere,
                   System.Environment.NewLine);

            Log(msg);
        }

        private static void Log(string msg)
        {
            //DbgUserBreakPoint();
            /*
            byte[] msgBytes = System.Text.Encoding.ASCII.GetBytes(msg);
            System.IO.FileStream fs = System.IO.File.Open("c:\\dwplog.txt", System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite);

            fs.Write(msgBytes, 0, msgBytes.Length);
            fs.Flush();
            fs.Close();
            */
        }

#endif

        private static IntPtr _cachedDefWindowProcA = IntPtr.Zero;
        private static IntPtr _cachedDefWindowProcW = IntPtr.Zero;

        ///<SecurityNote>
        ///     Critical - used as input to unsafe calls
        ///</SecurityNote>
        [SecurityCritical]
        private static Hashtable _hwndList = new Hashtable(10);
        private static bool _exiting = false;
    }
}
