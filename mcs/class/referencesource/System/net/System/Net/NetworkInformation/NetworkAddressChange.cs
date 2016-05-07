
namespace System.Net.NetworkInformation {

    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Threading;
    using System.Runtime.InteropServices;

    [Flags]
    internal enum StartIPOptions {Both = 3, None = 0, StartIPv4 = 1, StartIPv6 = 2}

    public class NetworkAvailabilityEventArgs:EventArgs{
        bool isAvailable;

        internal NetworkAvailabilityEventArgs(bool isAvailable){
            this.isAvailable = isAvailable;
        }
        
        public bool IsAvailable{
            get{
                return isAvailable;
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    public class NetworkChange{
    #region designer support for System.Windows.dll
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public NetworkChange() { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterNetworkChange(NetworkChange nc) { }
#endregion
        
        static public event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged{
            add{
                AvailabilityChangeListener.Start(value);
            }
            remove{
                AvailabilityChangeListener.Stop(value);
            }

        }
        
        
        static public event NetworkAddressChangedEventHandler NetworkAddressChanged{
            add{
                AddressChangeListener.Start(value);
            }
            remove{
                AddressChangeListener.Stop(value);
            }

        }



        static internal bool CanListenForNetworkChanges {
            get{
                return true;
            }
        }

        internal static class AvailabilityChangeListener{

            static object syncObject = new object();
            static private ListDictionary s_availabilityCallerArray = new ListDictionary();
            static NetworkAddressChangedEventHandler addressChange = ChangedAddress;
            static volatile bool isAvailable = false;
            private static ContextCallback s_RunHandlerCallback = new ContextCallback(RunHandlerCallback);


            private static void RunHandlerCallback(object state)
            {
                ((NetworkAvailabilityChangedEventHandler) state)(null, new NetworkAvailabilityEventArgs(isAvailable));
            }


            private static void ChangedAddress(object sender, EventArgs eventArgs) {

                lock(syncObject){
                    bool isAvailableNow = SystemNetworkInterface.InternalGetIsNetworkAvailable();

                    if (isAvailableNow != isAvailable) {
                        isAvailable = isAvailableNow;

                        DictionaryEntry[] callerArray = new DictionaryEntry[s_availabilityCallerArray.Count];
                        s_availabilityCallerArray.CopyTo(callerArray, 0);

                        for (int i = 0; i < callerArray.Length; i++)
                        {
                            NetworkAvailabilityChangedEventHandler handler = (NetworkAvailabilityChangedEventHandler) callerArray[i].Key;
                            ExecutionContext context = (ExecutionContext) callerArray[i].Value;
                            if (context == null)
                            {
                                handler(null, new NetworkAvailabilityEventArgs(isAvailable));
                            }
                            else
                            {
                                ExecutionContext.Run(context.CreateCopy(), s_RunHandlerCallback, handler);
                            }
                        }
                    }
                }
            }



            internal static void Start(NetworkAvailabilityChangedEventHandler caller){
                lock(syncObject){

                    if (s_availabilityCallerArray.Count == 0) {
                        isAvailable = NetworkInterface.GetIsNetworkAvailable();
                        AddressChangeListener.UnsafeStart(addressChange);
                    }

                    if ((caller != null) && (!s_availabilityCallerArray.Contains(caller))) {
                        s_availabilityCallerArray.Add(caller, ExecutionContext.Capture());
                    }
                }
            }


            internal static void Stop(NetworkAvailabilityChangedEventHandler caller)
            {
               lock(syncObject){
                    s_availabilityCallerArray.Remove(caller);
                    if(s_availabilityCallerArray.Count == 0){
                        AddressChangeListener.Stop(addressChange);
                    }
                }
            }
        }


        //helper class for detecting address change events.
        internal unsafe static class AddressChangeListener{

            static private ListDictionary s_callerArray = new ListDictionary();
            static private ContextCallback s_runHandlerCallback = new ContextCallback(RunHandlerCallback);
            static private RegisteredWaitHandle s_registeredWait;

            //need to keep the reference so it isn't GC'd before the native call executes
            static private bool s_isListening = false;
            static private bool s_isPending = false;
            static private SafeCloseSocketAndEvent s_ipv4Socket = null;
            static private SafeCloseSocketAndEvent s_ipv6Socket = null;
            static private WaitHandle s_ipv4WaitHandle = null;
            static private WaitHandle s_ipv6WaitHandle = null;

            //callback fired when an address change occurs
            private static void AddressChangedCallback(object stateObject, bool signaled) {
                lock (s_callerArray) {

                    //the listener was cancelled, which would only happen if we aren't listening
                    //for more events.
                    s_isPending = false;

                    if (!s_isListening) {
                        return;
                    }

                    s_isListening = false;

                    // Need to copy the array so the callback can call start and stop
                    DictionaryEntry[] callerArray = new DictionaryEntry[s_callerArray.Count];
                    s_callerArray.CopyTo(callerArray, 0);

                    try
                    {
                        //wait for the next address change
                        StartHelper(null, false, (StartIPOptions)stateObject);
                    }
                    catch (NetworkInformationException nie)
                    {
                        if (Logging.On) Logging.Exception(Logging.Web, "AddressChangeListener", "AddressChangedCallback", nie);
                    }

                    for (int i = 0; i < callerArray.Length; i++)
                    {
                        NetworkAddressChangedEventHandler handler = (NetworkAddressChangedEventHandler) callerArray[i].Key;
                        ExecutionContext context = (ExecutionContext) callerArray[i].Value;
                        if (context == null)
                        {
                            handler(null, EventArgs.Empty);
                        }
                        else
                        {
                            ExecutionContext.Run(context.CreateCopy(), s_runHandlerCallback, handler);
                        }
                    }
                }
            }

            private static void RunHandlerCallback(object state)
            {
                ((NetworkAddressChangedEventHandler) state)(null, EventArgs.Empty);
            }


            //start listening
            internal static void Start(NetworkAddressChangedEventHandler caller)
            {
                StartHelper(caller, true, StartIPOptions.Both);
            }

            internal static void UnsafeStart(NetworkAddressChangedEventHandler caller)
            {
                StartHelper(caller, false, StartIPOptions.Both);
            }

            private static void StartHelper(NetworkAddressChangedEventHandler caller, bool captureContext, StartIPOptions startIPOptions)
            {
                lock (s_callerArray) {
                    // setup changedEvent and native overlapped struct.
                    if(s_ipv4Socket == null){
                        Socket.InitializeSockets();

                        int blocking;

                        if(Socket.OSSupportsIPv4){
                            blocking = -1;
                            s_ipv4Socket = SafeCloseSocketAndEvent.CreateWSASocketWithEvent(AddressFamily.InterNetwork, SocketType.Dgram, (ProtocolType)0, true, false);
                            UnsafeNclNativeMethods.OSSOCK.ioctlsocket(s_ipv4Socket, IoctlSocketConstants.FIONBIO,ref blocking);
                            s_ipv4WaitHandle = s_ipv4Socket.GetEventHandle();
                        }

                        if(Socket.OSSupportsIPv6){
                            blocking = -1;
                            s_ipv6Socket = SafeCloseSocketAndEvent.CreateWSASocketWithEvent(AddressFamily.InterNetworkV6, SocketType.Dgram, (ProtocolType)0, true, false);
                            UnsafeNclNativeMethods.OSSOCK.ioctlsocket(s_ipv6Socket,IoctlSocketConstants.FIONBIO,ref blocking);
                            s_ipv6WaitHandle = s_ipv6Socket.GetEventHandle();
                        }
                    }

                    if ((caller != null) && (!s_callerArray.Contains(caller))) {
                        s_callerArray.Add(caller, captureContext ? ExecutionContext.Capture() : null);
                    }

                    //if s_listener is not null, it means we are already actively listening
                    if (s_isListening || s_callerArray.Count == 0) {
                        return;
                    }

                    if(!s_isPending){

                        int length;
                        SocketError errorCode;

                        if(Socket.OSSupportsIPv4 && (startIPOptions & StartIPOptions.StartIPv4) !=0){
                            s_registeredWait = ThreadPool.UnsafeRegisterWaitForSingleObject(
                                s_ipv4WaitHandle,
                                new WaitOrTimerCallback(AddressChangedCallback),
                                StartIPOptions.StartIPv4,
                                -1,
                                true );

                            errorCode = (SocketError) UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking(
                                s_ipv4Socket.DangerousGetHandle(),
                                (int) IOControlCode.AddressListChange,
                                null, 0, null, 0,
                                out length,
                                SafeNativeOverlapped.Zero, IntPtr.Zero);

                            if (errorCode != SocketError.Success) {
                                NetworkInformationException exception = new NetworkInformationException();
                                if (exception.ErrorCode != (uint)SocketError.WouldBlock) {
                                    throw exception;
                                }
                            }

                            errorCode = (SocketError)UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(s_ipv4Socket, s_ipv4Socket.GetEventHandle().SafeWaitHandle, AsyncEventBits.FdAddressListChange);
                            if (errorCode != SocketError.Success) {
                               throw new NetworkInformationException();
                            }
                        }

                        if(Socket.OSSupportsIPv6 && (startIPOptions & StartIPOptions.StartIPv6) !=0){
                            s_registeredWait = ThreadPool.UnsafeRegisterWaitForSingleObject(
                                s_ipv6WaitHandle,
                                new WaitOrTimerCallback(AddressChangedCallback),
                                StartIPOptions.StartIPv6,
                                -1,
                                true );

                            errorCode = (SocketError) UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking(
                                s_ipv6Socket.DangerousGetHandle(),
                                (int) IOControlCode.AddressListChange,
                                null, 0, null, 0,
                                out length,
                                SafeNativeOverlapped.Zero, IntPtr.Zero);

                            if (errorCode != SocketError.Success) {
                                NetworkInformationException exception = new NetworkInformationException();
                                if (exception.ErrorCode != (uint)SocketError.WouldBlock) {
                                    throw exception;
                                }
                            }

                            errorCode = (SocketError)UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(s_ipv6Socket, s_ipv6Socket.GetEventHandle().SafeWaitHandle, AsyncEventBits.FdAddressListChange);
                            if (errorCode != SocketError.Success) {
                               throw new NetworkInformationException();
                            }
                        }
                    }

                    s_isListening = true;
                    s_isPending = true;
                }
            }



            //stop listening
            internal static void Stop(object caller)
            {
                lock(s_callerArray){
                    s_callerArray.Remove(caller);
                    if (s_callerArray.Count == 0 && s_isListening) {
                        s_isListening = false;
                    }
                }
            } //ends ignoreaddresschanges
        }
    }

    public delegate void NetworkAddressChangedEventHandler(object sender, EventArgs e);
    public delegate void NetworkAvailabilityChangedEventHandler(object sender, NetworkAvailabilityEventArgs e);
}

