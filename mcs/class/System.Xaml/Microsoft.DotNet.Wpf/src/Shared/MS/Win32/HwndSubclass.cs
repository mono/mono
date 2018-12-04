using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using MS.Internal;
using MS.Internal.Interop;
using MS.Utility;
using System.Windows;
using System.Windows.Threading;
using System.Security;                       // CAS
using System.Threading;                      // Thread

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
    /// <summary>
    ///     The HwndSubclass class provides a managed way to subclass an existing
    ///     HWND.  This class inserts itself into the WNDPROC chain for the
    ///     window, and will call a specified delegate to process the window
    ///     messages that arrive.  The delegate has a slightly different
    ///     signature than a WNDPROC to be more specific about whether the
    ///     message was handled or not.  If the message was not handled by the
    ///     delegate, this class passes the message on down the WNDPROC chain.
    ///
    ///     To use this class properly, simply:
    ///     1) Create an instance of the HwndSubclass class and pass the delegate
    ///        to the constructor.
    ///     2) Call Attach(HWND) to subclass an existing window.
    ///     3) Call Detach(false) to unsubclass the window when you are done.
    ///
    ///     You can also just call RequestDetach() to send a message to the
    ///     window that will cause the HwndSubclass to detach itself.  This is
    ///     important if you are on a different thread, as the HwndSubclass class
    ///     is not thread safe and will be operated on by the thread that owns
    ///     the window.
    /// </summary>
    /// <remarks> Not available to partial trust callers</remarks>
    [FriendAccessAllowed] // Built into Base, also used by Framework
    internal class HwndSubclass : IDisposable
    {
        /// <SecurityNote>
        ///  Critical: Elevates by calling an UnsafeNativeMethod
        ///</SecurityNote>
        [SecurityCritical]
        static HwndSubclass()
        {
            DetachMessage = UnsafeNativeMethods.RegisterWindowMessage("HwndSubclass.DetachMessage");

            // Go find the address of DefWindowProc.
            IntPtr hModuleUser32 = UnsafeNativeMethods.GetModuleHandle(ExternDll.User32);
            IntPtr address = UnsafeNativeMethods.GetProcAddress(new HandleRef(null,hModuleUser32), "DefWindowProcW");

            DefWndProc = address;
        }

        /// <summary>
        ///     This HwndSubclass constructor binds the HwndSubclass object to the
        ///     specified delegate.  This delegate will be called to process
        ///     the messages that are sent or posted to the window.
        /// </summary>
        /// <param name="hook">
        ///     The delegate that will be called to process the messages that
        ///     are sent or posted to the window.
        /// </param>
        /// <returns>
        ///     Nothing.
        /// </returns>
        /// <SecurityNote>
        ///  Critical: This code creates an object that is not allowed in partial trust
        /// </SecurityNote>
        [SecurityCritical]
        internal HwndSubclass(HwndWrapperHook hook)
        {
            if(hook == null)
            {
                throw new ArgumentNullException("hook");
            }

            _bond = Bond.Unattached;
            _hook = new WeakReference(hook);

            // Allocate a GC handle so that we won't be collected, even if all
            // references to us get released.  This is because a component outside
            // of the managed code (ie. the window we are subclassing) still holds
            // a reference to us - just not a reference that the GC recognizes.
            _gcHandle = GCHandle.Alloc(this);
        }

        // This is LIVE OBJECT because it has a GCHandle. The only time LIVE OBJECTS 
        // are destroyed is during Shutdown. But Shutdown cleanup is handled through 
        // the ManagedWndProcTracker and hence no work needs to happen here. PLEASE 
        // NOTE that reintroducing any cleanup logic in here will conflict with the cleanup 
        // logic in ManagedWndProcTracker and hence must be avoided. If this instance 
        // has been disposed its GCHandle is released at the time and hence this object 
        // is available for GC thereafter. Even in that case since all the cleanup has been 
        // done during dispose there is no further cleanup required.
        
        // /// <SecurityNote>
        // ///  Critical - It calls the critical DisposeImpl.
        // /// </SecurityNote>
        // [SecurityCritical]
        // ~HwndSubclass()
        // {
        //     // In Shutdown, the finalizer is called on LIVE OBJECTS.
        //     //
        //     // So this method can be called.  (even though we are pinned)
        //     // If it is, we're shutting down so it's OK to force unhooking
        //     // the subclass.
        //     DisposeImpl(true);
        // }

        /// <SecurityNote>
        ///  Critical - It calls the critical DisposeImpl.
        ///  PublicOK - Demands for AllWindows permission.
        /// </SecurityNote>
        [SecurityCritical]
        public virtual void Dispose()
        {
            SecurityHelper.DemandUIWindowPermission();

            DisposeImpl(false);
        }

        /// <SecurityNote>
        ///  Critical - Calls the critical UnhookWindowProc.
        /// </SecurityNote>
        [SecurityCritical]
        private bool DisposeImpl(bool forceUnhook)
        {
            _hook = null;

            return UnhookWindowProc(forceUnhook);
        }

        /// <summary>
        ///     This method subclasses the specified window, such that the
        ///     delegate specified to the constructor will be called to process
        ///     the messages that are sent or posted to this window.
        /// </summary>
        /// <param name="hwnd">
        ///     The window to subclass.
        /// </param>
        /// <returns>
        ///     An identifier that can be used to reference this instance of
        ///     the HwndSubclass class in the static RequestDetach method.
        /// </returns>
        /// <SecurityNote>
        ///  Critical - It calls CriticalAttach.
        ///  TreatAsSafe - Demands for AllWindows permission.
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal IntPtr Attach(IntPtr hwnd)
        {
            SecurityHelper.DemandUIWindowPermission();

            if (_bond != Bond.Unattached)
                throw new InvalidOperationException(SR.Get(SRID.HwndSubclassMultipleAttach));

            return CriticalAttach( hwnd ) ;
        }


        /// <summary>
        ///     This method unsubclasses this HwndSubclass object from the window
        ///     it previously subclassed. The HwndSubclass object is not thread
        ///     safe, and should thus be called only by the thread that owns
        ///     the window being unsubclassed.
        /// </summary>
        /// <param name="force">
        ///     Whether or not the unsubclassing should be forced.  Due to the
        ///     way that Win32 implements window subclassing, it is not always
        ///     possible to safely remove a window proc from the WNDPROC chain.
        ///     However, the delegate will not be called again after this
        ///     method returns.
        /// </param>
        /// <returns>
        ///     Whether or not this HwndSubclass object was actually removed from
        ///     the WNDPROC chain.
        /// </returns>
        /// <SecurityNote>
        ///  TreatAsSafe:  Demands for all windows
        ///  Critical: Elevates by calling an UnsafeNativeMethod
        ///</SecurityNote>
        [SecurityCritical]
        internal bool Detach(bool force)
        {
            SecurityHelper.DemandUIWindowPermission();

            return CriticalDetach(force);
        }

        /// <SecurityNote>
        ///  Critical: Elevates by calling an UnsafeNativeMethod
        ///</SecurityNote>
        [SecurityCritical]
        internal bool CriticalDetach(bool force)
        {
            bool detached;

            // If we have already detached, return immediately.
            if(_bond == Bond.Detached || _bond == Bond.Unattached)
            {
                detached = true;
            }
            else
            {
                // When we detach, we simply make a note of it.
                _bond = Bond.Orphaned;

                // try to unhook the subclass
                detached = DisposeImpl(force);
            }

            return detached;
        }

        /// <summary>
        ///     This method sends a message to the window that is currently
        ///     subclassed by this instance of the HwndSubclass class, in order
        ///     to unsubclass the window.  This is important if a different
        ///     thread than the thread that owns the window wants to initiate
        ///     the unsubclassing.
        /// </summary>
        /// <param name="force">
        ///     Whether or not the unsubclassing should be forced.  Due to the
        ///     way that Win32 implements window subclassing, it is not always
        ///     possible to safely remove a window proc from the WNDPROC chain.
        ///     However, the delegate will not be called again after this
        ///     method returns.
        /// </param>
        /// <returns>
        ///     Nothing.
        /// </returns>
        /// <SecurityNote>
        ///  Critical - Probes _hwndAttached for value.
        ///  TreatAsSafe - The RequestDetach overload we're calling is public and safe (demands UIWindowPermission).
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal void RequestDetach(bool force)
        {
            // Let the static version do the work.
            if(_hwndAttached != IntPtr.Zero)
            {
                RequestDetach(_hwndAttached, (IntPtr) _gcHandle, force);
            }
        }

        /// <summary>
        ///     This method sends a message to the specified window in order to
        ///     cause the specified bridge to unsubclass the window.  This is
        ///     important if a different thread than the thread that owns the
        ///     window wants to initiate the unsubclassing.  The HwndSubclass
        ///     object must be identified by the value returned from Attach().
        /// </summary>
        /// <param name="hwnd">
        ///     The window to unsubclass.
        /// </param>
        /// <param name="subclass">
        ///     The identifier of the subclass to unsubclass.
        /// </param>
        /// <param name="force">
        ///     Whether or not the unsubclassing should be forced.  Due to the
        ///     way that Win32 implements window subclassing, it is not always
        ///     possible to safely remove a window proc from the WNDPROC chain.
        ///     However, the delegate will not be called again after this
        ///     method returns.
        /// </param>
        /// <returns>
        ///     Nothing.
        /// </returns>
        /// <SecurityNote>
        ///  Critical - This touches the underlying windowing infrastructure; we may want to intercept windows-messages for security-related purposes in the future.
        ///             In general we restrict access to window handles.
        ///  TreatAsSafe - Demands UIWindowPermission.
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal static void RequestDetach(IntPtr hwnd, IntPtr subclass, bool force)
        {
            SecurityHelper.DemandUIWindowPermission();
            if(hwnd == IntPtr.Zero)
            {
                throw new ArgumentNullException("hwnd");
            }
            if(subclass == IntPtr.Zero)
            {
                throw new ArgumentNullException("subclass");
            }

            int iForce = force ? 1 : 0;
            UnsafeNativeMethods.UnsafeSendMessage(hwnd, DetachMessage, subclass, (IntPtr) iForce);
        }

        /// <summary>
        ///     This is the WNDPROC that gets inserted into the window's
        ///     WNDPROC chain.  It responds to various conditions that
        ///     would cause this HwndSubclass object to unsubclass the window,
        ///     and then calls the delegate specified to the HwndSubclass
        ///     constructor to process the message.  If the delegate does not
        ///     handle the message, the message is then passed on down the
        ///     WNDPROC chain for further processing.
        /// </summary>
        /// <param name="hwnd">
        ///     The window that this message was sent or posted to.
        /// </param>
        /// <param name="msg">
        ///     The message that was sent or posted.
        /// </param>
        /// <param name="wParam">
        ///     A parameter for the message that was sent or posted.
        /// </param>
        /// <param name="lParam">
        ///     A parameter for the message that was sent or posted.
        /// </param>
        /// <returns>
        ///     The value that is the result of processing the message.
        /// </returns>
        ///<SecurityNote>
        ///  Critical: this function elevates via unsafe native method calls
        ///</SecurityNote>
        [SecurityCritical]
        internal IntPtr SubclassWndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr retval = IntPtr.Zero;
            bool handled = false;
            WindowMessage message = (WindowMessage)msg;

            // If we are unattached and we receive a message, then we must have
            // been used as the original window proc.  In this case, we insert
            // ourselves as if the original window proc had been DefWindowProc.
            // We pass in DefWndProcStub as a workaround for a bug in UxTheme on
            // Windows XP. For details see the comment on the DefWndProcWrapper method.
            if(_bond == Bond.Unattached)
            {
                HookWindowProc(hwnd, new NativeMethods.WndProc(SubclassWndProc),
                    Marshal.GetFunctionPointerForDelegate(DefWndProcStub));
            }
            else if(_bond == Bond.Detached)
            {
                throw new InvalidOperationException();
            }

            IntPtr oldWndProc = _oldWndProc;    // in case we get detached during this method

            if(message == DetachMessage)
            {
                // We received our special message to detach.  Make sure it is intended
                // for us by matching the bridge.
                if(wParam == IntPtr.Zero || wParam == (IntPtr)_gcHandle)
                {
                    int param = (int)lParam;    // 0 - normal, 1 - force, 2 - force and forward
                    bool force = (param > 0);

                    retval = CriticalDetach(force) ? new IntPtr(1) : IntPtr.Zero ;
                    handled = (param < 2);
                }
            }
            else
            {
                // Pass this message to our delegate function.  Do this under
                // the exception filter/handlers of the dispatcher for this thread.
                Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if(dispatcher != null && !dispatcher.HasShutdownFinished)
                {
                    if (_dispatcherOperationCallback == null)
                        _dispatcherOperationCallback = new DispatcherOperationCallback(this.DispatcherCallbackOperation);

                    // _paramDispatcherCallbackOperation is a thread static member which should be reused to avoid
                    // creating a new data structure every time we call DispatcherCallbackOperation
                    // Cache the param locally in case of reentrance and set _paramDispatcherCallbackOperation to null so reentrancy calls will create a new param
                    if (_paramDispatcherCallbackOperation == null)
                        _paramDispatcherCallbackOperation = new DispatcherOperationCallbackParameter();

                    DispatcherOperationCallbackParameter param = _paramDispatcherCallbackOperation;
                    _paramDispatcherCallbackOperation = null;
                    param.hwnd = hwnd;
                    param.msg = msg;
                    param.wParam = wParam;
                    param.lParam = lParam;
                    //synchronous call
                    object result = dispatcher.Invoke(
                             DispatcherPriority.Send,
                             _dispatcherOperationCallback,
                             param);


                    if (result != null)
                    {
                        handled = param.handled;
                        retval = param.retVal;
                    }

                    // Restore _paramDispatcherCallbackOperation to the previous value so we will reuse it on the next call
                    _paramDispatcherCallbackOperation = param;
                }

                // Handle WM_NCDESTROY explicitly to forcibly clean up.
                if(message == WindowMessage.WM_NCDESTROY)
                {
                    // The fact that we received this message means that we are
                    // still in the call chain.  This is our last chance to clean
                    // up, and no other message should be received by this window
                    // proc again. It is OK to force a cleanup now.
                    CriticalDetach(true);

                    // Always pass the WM_NCDESTROY message down the chain!
                    handled = false;
                }
            }

            // If our window proc didn't handle this message, pass it on down the
            // chain.
            if(!handled)
            {
                retval = CallOldWindowProc(oldWndProc, hwnd, message, wParam, lParam);
            }

            return retval;
        }

        // Perf bug: 1963989
        // _paramDispatcherCallbackOperation is a thread static member which should be reused to avoid
        // creating a new data structure every time we DispatcherCallbackOperation is called
        // It also contains the return results (handled and retValue) from DispatcherCallbackOperation call
        /// <SecurityNote>
        ///  Critical: DispatcherOperationCallbackParameter contains hwnd, which is critical
        ///</SecurityNote>
        [SecurityCritical]
        [ThreadStatic]
        private static DispatcherOperationCallbackParameter _paramDispatcherCallbackOperation;

        // This class is used as a parameter and return result for DispatcherCallbackOperation call
        private class DispatcherOperationCallbackParameter
        {
            internal IntPtr hwnd, wParam, lParam, retVal;
            internal int msg;
            internal bool handled;
        }

        private DispatcherOperationCallback _dispatcherOperationCallback = null;

        /// <SecurityNote>
        ///  Critical: it calls GetWindowLongPtr(), which is Critical
        ///</SecurityNote>
        [ SecurityCritical ]
        internal IntPtr CriticalAttach( IntPtr hwnd )
        {
            if(hwnd == IntPtr.Zero)
            {
                throw new ArgumentNullException("hwnd");
            }
            if(_bond != Bond.Unattached)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.WndProc newWndProc = new NativeMethods.WndProc(SubclassWndProc);
            IntPtr oldWndProc = UnsafeNativeMethods.GetWindowLongPtr(new HandleRef(this,hwnd), NativeMethods.GWL_WNDPROC);
            HookWindowProc(hwnd, newWndProc, oldWndProc);

            // Return the GC handle as a unique identifier of this
            return (IntPtr) _gcHandle;
        }

        /// <SecurityNote>
        ///     Critical: This code is a callback into the dispatcher. It is present
        ///      because under enforcements anonymous delagates throw a demand since
        ///      it becomes a transparent calling critical for this particular call
        /// </SecurityNote>
        [SecurityCritical]
        private object DispatcherCallbackOperation(object o)
        {
            DispatcherOperationCallbackParameter param = (DispatcherOperationCallbackParameter)o;
            param.handled = false;
            param.retVal = IntPtr.Zero;
            if (_bond == Bond.Attached)
            {
                HwndWrapperHook hook= _hook.Target as HwndWrapperHook;

                if (hook != null)
                {
                    // make the call
                    param.retVal = hook(param.hwnd, param.msg, param.wParam, param.lParam, ref param.handled);
                }
            }

            return param;
        }

        /// <summary>
        ///     This method lets the user call the old WNDPROC, i.e
        ///     the next WNDPROC in the chain directly.
        /// </summary>
        /// <param name="oldWndProc">
        ///     The WndProc to call.
        /// </param>
        /// <param name="hwnd">
        ///     The window that this message was sent or posted to.
        /// </param>
        /// <param name="msg">
        ///     The message that was sent or posted.
        /// </param>
        /// <param name="wParam">
        ///     A parameter for the message that was sent or posted.
        /// </param>
        /// <param name="lParam">
        ///     A parameter for the message that was sent or posted.
        /// </param>
        /// <returns>
        ///     The value that is the result of processing the message.
        /// </returns>
        /// <SecurityNote>
        ///  Critical: Elevates by calling an UnsafeNativeMethod
        ///</SecurityNote>
        [SecurityCritical]
        IntPtr CallOldWindowProc(IntPtr oldWndProc, IntPtr hwnd, WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            return UnsafeNativeMethods.CallWindowProc(oldWndProc, hwnd, (int)msg, wParam, lParam);
        }

        ///<SecurityNote>
        /// Critical - it calls CriticalSetWindowLong()
        ///</SecurityNote>
        [SecurityCritical]
        private void HookWindowProc(IntPtr hwnd, NativeMethods.WndProc newWndProc, IntPtr oldWndProc)
        {
            _hwndAttached = hwnd;
            _hwndHandleRef = new HandleRef(null,_hwndAttached);
            _bond = Bond.Attached;

            _attachedWndProc = newWndProc;
            _oldWndProc = oldWndProc;
            IntPtr oldWndProc2 = (IntPtr)UnsafeNativeMethods.CriticalSetWindowLong(_hwndHandleRef, NativeMethods.GWL_WNDPROC, _attachedWndProc);

            // Track this window so that we can rip out the managed window proc
            // when the CLR shuts down.
            ManagedWndProcTracker.TrackHwndSubclass(this, _hwndAttached);
        }

        // This method should only be called from Dispose. Otherwise assumptions about the disposing/finalize state could be violated.
        // force - when true, remove this subclass from the WndProc chain regardless of
        //          its current position.  When false, remove this subclass only
        //          if it is possible to do so without damaging other WndProcs
        //          (i.e. only if this is at the head of the chain).
        //
        // Removing this subclass from the WndProc chain when it is not at the head
        // also removes all other WndProcs that appear before this one on the chain,
        // so is generally not appropriate. It is OK in the following situations:
        //  a) in response to the WM_NCDESTROY message
        //  b) in response to the AppDomainProcessExit event
        //  c) in response to the AppDomainExit event
        // In cases (a) and (b) the HWND is being destroyed, so the earlier
        // WndProcs are no longer useful anyway.  In case (c), we have to remove
        // all managed code from the chain lest it be called after it has been
        // removed from memory;  removing earlier WndProcs is unfortunate, but
        // necessary.  [Note that at AppDomainExit we remove all managed WndProcs,
        // regardless of which AppDomain they came from.  There is room for
        // improvement here - we could remove only the ones belong to the AppDomain
        // that is exiting.  This situation seems too unlikely to worry about in V1.]
        //
        // This method returns true if the subclass is no longer in the WndProc chain.
        //
        /// <SecurityNote>
        ///  Critical - Calls CriticalSetWindowLong() and GetWindowLongWndProc().
        ///             This touches the underlying windowing infrastructure; we may want to intercept windows-messages for security-related purposes in the future.
        ///             In general we restrict access to window handles.
        /// </SecurityNote>
        [SecurityCritical]
        private bool UnhookWindowProc(bool force)
        {
            // if we're not in the WndProc chain, there's nothing to do
            if (_bond == Bond.Unattached || _bond == Bond.Detached)
            {
                return true;
            }

            // we'll remove ourselves from the chain if we're at the head, or if
            // the 'force' parameter was true.
            if (!force)
            {
                NativeMethods.WndProc currentWndProc = UnsafeNativeMethods.GetWindowLongWndProc(new HandleRef(this,_hwndAttached));
                force = (currentWndProc == _attachedWndProc);
            }

            // if we're not unhooking, return and report
            if (!force)
            {
                return false;
            }

            // unhook from the tracker
            _bond = Bond.Orphaned;  // ignore messages while we're unhooking
            ManagedWndProcTracker.UnhookHwndSubclass(this);

            // unhook, the Win32 way
            try
            {
                UnsafeNativeMethods.CriticalSetWindowLong(_hwndHandleRef, NativeMethods.GWL_WNDPROC, _oldWndProc);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (e.NativeErrorCode != 1400) // ERROR_INVALID_WINDOW_HANDLE
                {
                    throw;
                }
            }


            // clear our state
            _bond = Bond.Detached;

            _oldWndProc = IntPtr.Zero;
            _attachedWndProc = null;
            _hwndAttached = IntPtr.Zero;
            _hwndHandleRef = new HandleRef(null,IntPtr.Zero);

            // un-Pin this object.
            // Note: the GC is free to collect this object at anytime
            // after we have freed this handle - that is, once all
            // other managed references go away.

            //AvDebug.Assert(_gcHandle.IsAllocated, "External GC handle has not been allocated.");

            if(null != _gcHandle)
                _gcHandle.Free();

            return true;
        }




        /// <summary>
        /// DefWndProcWrapper is a wrapper around DefWndProc.  This is a workaround
        /// for WindowsSE bugs 124461 and 124455, which affect Windows XP SP2, Luna theme.
        ///
        /// HwndSubclass.SubclassWndProc is sometimes directly set as the window proc of a
        /// Window (HwndWrapper's constructor does this).  When this happens it subclasses
        /// itself on the first message it receives. Since the old window proc is itself,
        /// it saves off DefWndProc as the old window proc instead.
        ///
        /// As described by the bugs and and KB article 319740, if we set DefWndProc
        /// as the window proc of the window, we'll leak 6 GDI region objects
        /// corresponding to the Luna-themed non-client area.
        ///
        /// The reason for this is that the kernel will flag that window as a server-side
        /// window. If the WndProc replacement happens in response to a WM_NCDESTROY message
        /// (as it does in the case where Avalon is creating and closing windows), the Shell
        /// won't clean up the regions.
        ///
        /// The fix is slated for XP SP3, so for now WPF is implementing a workaround:
        /// if we set the window proc to be a stub that calls into DefWndProc, the kernel
        /// won't set us as a server-side window.
        /// </summary>
        /// <SecurityNote>
        ///  Critical: Elevates by calling an UnsafeNativeMethod
        ///</SecurityNote>
        [SecurityCritical]
        private static IntPtr DefWndProcWrapper(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam)
        {
            return UnsafeNativeMethods.CallWindowProc(DefWndProc, hwnd, msg, wParam, lParam);
        }



        // Message to cause a detach.
        //      WPARAM=IntPtr returned from Attach(), or 0 to match all subclasses.
        //      LPARAM= 0 - normal (unhook subclass if it is first on the chain)
        //              1 - force unhooking subclass from the chain
        //              2 - force, and forward message to next WndProc
        internal static readonly WindowMessage DetachMessage;

        private enum Bond
        {
            Unattached,
            Attached,
            Detached,
            Orphaned
        }


        /// <summary>
        /// This is a delegate that points to DefWndProcWrapper.  It is set into
        /// a Window's WndProc instead of DefWndProc in order to work around a bug.
        /// See the comment on DefWndProcWrapper.
        ///
        /// By instantiating this delegate as a static variable we ensure that
        /// it will remain alive long enough to process messages.
        /// </summary>
        /// <SecurityNote>
        ///     Critical: This will expose DefWndProc
        /// </SecurityNote>
        [SecurityCritical]
        private static NativeMethods.WndProc DefWndProcStub = new NativeMethods.WndProc(DefWndProcWrapper);

        /// <SecurityNote>
        ///     Critical: This will expose wndproc
        /// </SecurityNote>
        [SecurityCritical]
        private static IntPtr DefWndProc;

        /// <SecurityNote>
        ///     Critical: This will expose the intptr of the window
        /// </SecurityNote>
        [SecurityCritical]
        private IntPtr _hwndAttached;
        /// <SecurityNote>
        ///     Critical: This will expose window handle
        /// </SecurityNote>
        [SecurityCritical]
        private HandleRef _hwndHandleRef;
        /// <SecurityNote>
        ///     Critical: This will expose wndproc
        /// </SecurityNote>
        [SecurityCritical]
        private NativeMethods.WndProc _attachedWndProc;
        /// <SecurityNote>
        ///     Critical: This will expose wndproc
        /// </SecurityNote>
        [SecurityCritical]
        private IntPtr                _oldWndProc;
        private Bond                  _bond;
        private GCHandle              _gcHandle;
        /// <SecurityNote>
        ///     Critical: This will expose hook
        /// </SecurityNote>
        [SecurityCritical]
        private WeakReference _hook;
    };
}

