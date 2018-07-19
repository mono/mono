namespace System.Web.Services.Interop {
    using System;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Security;
    
    [ComImport(), Guid("1AF04045-6659-4aaa-9F4B-2741AC56224B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressUnmanagedCodeSecurity]
    internal interface INotifyConnection2 {
        [return: MarshalAs(UnmanagedType.Interface)]
        INotifySink2 RegisterNotifySource(
            [In, MarshalAs(UnmanagedType.Interface)] INotifySource2 in_pNotifySource);

        
        void UnregisterNotifySource(
            [In, MarshalAs(UnmanagedType.Interface)] INotifySource2 in_pNotifySource);
    }
}
