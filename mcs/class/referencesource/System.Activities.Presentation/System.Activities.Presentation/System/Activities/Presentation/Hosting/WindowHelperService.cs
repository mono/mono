//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Hosting
{
    using System;
    using System.Windows;
    using System.Windows.Interop;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Tools.Common;

    public delegate void WindowMessage(int msgId, IntPtr parameter1, IntPtr parameter2 );

    [Fx.Tag.XamlVisible(false)]
    public class WindowHelperService
    {
        HwndSource hwndSource;
        WindowMessage listeners;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "hwnd is a well known name.")]
        public WindowHelperService(IntPtr hwnd)
        {
            this.ParentWindowHwnd = hwnd;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "hwnd is a well known name.")]
        public IntPtr ParentWindowHwnd
        {
            get;
            private set;
        }

        internal FrameworkElement View
        {
            get;
            set;
        }

        public bool TrySetWindowOwner(DependencyObject source, Window target)
        {
            bool result = false;
            Fx.Assert(target != null, "Target window cannot be null");
            if (null != target)
            {
                if (source != null)
                {
                    //try the easy way first
                    Window owner = Window.GetWindow(source);
                    if (null != owner)
                    {
                        target.Owner = owner;
                        result = true;
                    }
                }
                //no - it didn't work
                if (!result)
                {
                    IntPtr ownerHwnd = Win32Interop.GetActiveWindow();
                    if (ownerHwnd == IntPtr.Zero)
                    {
                        ownerHwnd = this.ParentWindowHwnd;
                    }
                    WindowInteropHelper interopHelper = new WindowInteropHelper(target);
                    interopHelper.Owner = ownerHwnd;
                    result = true;
                }
            }

            return result;
        }

        public bool RegisterWindowMessageHandler(WindowMessage callback)
        {
            bool result = true;
            if (null == callback)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("callback"));
            }

            //if there are no other callbacks registered - create initial multicast delegate 
            //and hookup message processing
            if (null == this.listeners)
            {
                this.listeners = (WindowMessage)Delegate.Combine(callback);
                this.HookupMessageProcessing();
            }
                //otherwise, check if callback is not in the list already
            else 
            {
                Delegate[] initial = this.listeners.GetInvocationList();
                //if it isn't - add it to callback list
                if (-1 == Array.IndexOf<Delegate>(initial, callback))
                {
                    Delegate[] combined = new Delegate[initial.Length + 1];
                    combined[initial.Length] = callback;
                    Array.Copy(initial, combined, initial.Length);

                    this.listeners = (WindowMessage)Delegate.Combine(combined);
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public bool UnregisterWindowMessageHandler(WindowMessage callback)
        {
            bool result = false;
            if (null == callback)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("callback"));
            }
            //if there are any callbacks
            if (null != this.listeners)
            {
                Delegate[] list = this.listeners.GetInvocationList();
                //check if given delegate belongs to the list
                if (-1 != Array.IndexOf<Delegate>(list, callback))
                {
                    //if list contained 1 element - there is nobody listening for events - remove hook
                    if (list.Length == 1)
                    {
                        this.hwndSource.RemoveHook(new HwndSourceHook(OnMessage));
                    }
                    //yes - remove it
                    this.listeners = (WindowMessage)Delegate.Remove(this.listeners, callback);
                    result = true;
                }
            }
            if (!result)
            {
                System.Diagnostics.Debug.WriteLine("UnregisterWindowMessageHandler - callback not in list");
            }
            return result;
        }

        internal static void TrySetWindowOwner(DependencyObject owner, EditingContext editingContext, Window wnd)
        {
            if (null != editingContext)
            {
                WindowHelperService service = editingContext.Services.GetService<WindowHelperService>();
                if (null != service)
                {
                    service.TrySetWindowOwner(owner, wnd);
                }
            }
        }

        void HookupMessageProcessing()
        {
            //try to create hwnd source object
            if (null == this.hwndSource)
            {
                //first - try to create it using ParentWindow handle
                if (IntPtr.Zero != this.ParentWindowHwnd)
                {
                    this.hwndSource = HwndSource.FromHwnd(this.ParentWindowHwnd);
                }
                //if didn't succeed - (either handle is null or we are hosted in [....] app)
                //try to create hwnd source out of designer's view 
                if (null == this.hwndSource)
                {
                    this.hwndSource = HwndSource.FromVisual(this.View) as HwndSource;
                }
            }
            Fx.Assert(null != this.hwndSource, "HwndSource should not be null!");
            if (null != this.hwndSource)
            {
                //register for event notifications
                this.hwndSource.AddHook(new HwndSourceHook(OnMessage));
            }
        }

        IntPtr OnMessage(IntPtr hwnd, int msgId, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //notify all listeners about window message
            this.listeners(msgId, wParam, lParam);
            return IntPtr.Zero;
        }

    }
}
