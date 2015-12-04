//------------------------------------------------------------------------------
// <copyright file="HttpListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

//disabled until BindHandle has an overload that accepts Criticalhandles
#pragma warning disable 618

namespace System.Net {
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.Diagnostics.CodeAnalysis;

    public class HttpListenerBasicIdentity : GenericIdentity
    {
        private string m_Password;

        public HttpListenerBasicIdentity(string username, string password) :
            base(username, BasicClient.AuthType)
        {
            m_Password = password;
        }

        public virtual string Password
        {
            get
            {
                return m_Password;
            }
        }
    }

    internal abstract unsafe class RequestContextBase : IDisposable
    {
        private UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* m_MemoryBlob;
        private UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* m_OriginalBlobAddress; 
        private byte[] m_BackingBuffer;

        // Must call this from derived class' constructors.
        protected void BaseConstruction(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* requestBlob)
        {
            if (requestBlob == null)
            {
                GC.SuppressFinalize(this);
            }
            else
            {
                m_MemoryBlob = requestBlob;
            }
        }

        // ReleasePins() should be called exactly once.  It must be called before Dispose() is called, which means it must be called
        // before an object (HttpListenerReqeust) which closes the RequestContext on demand is returned to the application.
        internal void ReleasePins()
        {
            GlobalLog.Assert(m_MemoryBlob != null || m_BackingBuffer == null, "RequestContextBase::ReleasePins()|ReleasePins() called twice.");
            m_OriginalBlobAddress = m_MemoryBlob;
            UnsetBlob();
            OnReleasePins();
        }

        protected abstract void OnReleasePins();

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            GlobalLog.Assert(m_MemoryBlob == null, "RequestContextBase::Dispose()|Dispose() called before ReleasePins().");
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }

        ~RequestContextBase()
        {
            Dispose(false);
        }

        internal UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* RequestBlob
        {
            get
            {
                GlobalLog.Assert(m_MemoryBlob != null || m_BackingBuffer == null, "RequestContextBase::Dispose()|RequestBlob requested after ReleasePins().");
                return m_MemoryBlob;
            }
        }

        internal byte[] RequestBuffer
        {
            get
            {
                return m_BackingBuffer;
            }
        }

        internal uint Size
        {
            get
            {
                return (uint) m_BackingBuffer.Length;
            }
        }

        internal IntPtr OriginalBlobAddress
        {
            get
            {
                UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* blob = m_MemoryBlob;
                return (IntPtr) (blob == null ? m_OriginalBlobAddress : blob);
            }
        }

        protected void SetBlob(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* requestBlob)
        {
            GlobalLog.Assert(m_MemoryBlob != null || m_BackingBuffer == null, "RequestContextBase::Dispose()|SetBlob() called after ReleasePins().");
            if (requestBlob == null)
            {
                UnsetBlob();
                return;
            }

            if (m_MemoryBlob == null)
            {
                GC.ReRegisterForFinalize(this);
            }
            m_MemoryBlob = requestBlob;
        }

        protected void UnsetBlob()
        {
            if (m_MemoryBlob != null)
            {
                GC.SuppressFinalize(this);
            }
            m_MemoryBlob = null;
        }

        protected void SetBuffer(int size)
        {
            m_BackingBuffer = size == 0 ? null : new byte[size];
        }
    }

    internal unsafe class AsyncRequestContext : RequestContextBase
    {
        private NativeOverlapped* m_NativeOverlapped;
        private ListenerAsyncResult m_Result;

        internal AsyncRequestContext(ListenerAsyncResult result)
        {
            m_Result = result;
            BaseConstruction(Allocate(0));
        }

        private UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* Allocate(uint size)
        {
            uint newSize = size != 0 ? size : RequestBuffer == null ? 4096 : Size;
            if (m_NativeOverlapped != null && newSize != RequestBuffer.Length)
            {
                NativeOverlapped* nativeOverlapped = m_NativeOverlapped;
                m_NativeOverlapped = null;
                Overlapped.Free(nativeOverlapped);
            }
            if (m_NativeOverlapped == null)
            {
                SetBuffer(checked((int) newSize));
                Overlapped overlapped = new Overlapped();
                overlapped.AsyncResult = m_Result;
                m_NativeOverlapped = overlapped.Pack(ListenerAsyncResult.IOCallback, RequestBuffer);
                return (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*) Marshal.UnsafeAddrOfPinnedArrayElement(RequestBuffer, 0);
            }
            return RequestBlob;
        }

        internal void Reset(ulong requestId, uint size)
        {
            SetBlob(Allocate(size));
            RequestBlob->RequestId = requestId;
        }

        protected override void OnReleasePins()
        {
            if (m_NativeOverlapped != null)
            {
                NativeOverlapped* nativeOverlapped = m_NativeOverlapped;
                m_NativeOverlapped = null;
                Overlapped.Free(nativeOverlapped);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (m_NativeOverlapped != null)
            {
                GlobalLog.Assert(!disposing, "AsyncRequestContext::Dispose()|Must call ReleasePins() before calling Dispose().");
                if (!NclUtilities.HasShutdownStarted || disposing)
                {
                    Overlapped.Free(m_NativeOverlapped);
                }
            }
            base.Dispose(disposing);
        }

        internal NativeOverlapped* NativeOverlapped
        {
            get
            {
                return m_NativeOverlapped;
            }
        }
    }

    internal unsafe class SyncRequestContext : RequestContextBase
    {
        private GCHandle m_PinnedHandle;

        internal SyncRequestContext(int size)
        {
            BaseConstruction(Allocate(size));
        }

        private UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* Allocate(int size)
        {
            if (m_PinnedHandle.IsAllocated)
            {
                if (RequestBuffer.Length == size)
                {
                    return RequestBlob;
                }
                m_PinnedHandle.Free();
            }
            SetBuffer(size);
            if (RequestBuffer == null)
            {
                return null;
            }
            m_PinnedHandle = GCHandle.Alloc(RequestBuffer, GCHandleType.Pinned);
            return (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*) Marshal.UnsafeAddrOfPinnedArrayElement(RequestBuffer, 0);
        }

        internal void Reset(int size)
        {
            SetBlob(Allocate(size));
        }

        protected override void OnReleasePins()
        {
            if (m_PinnedHandle.IsAllocated)
            {
                m_PinnedHandle.Free();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (m_PinnedHandle.IsAllocated)
            {
                GlobalLog.Assert(!disposing, "AsyncRequestContext::Dispose()|Must call ReleasePins() before calling Dispose().");
                if (!NclUtilities.HasShutdownStarted || disposing)
                {
                    m_PinnedHandle.Free();
                }
            }
            base.Dispose(disposing);
        }
    }

    public sealed unsafe class HttpListener : IDisposable
    {
        private static readonly Type ChannelBindingStatusType = typeof(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_CHANNEL_BIND_STATUS);
        private static readonly int RequestChannelBindStatusSize =
            Marshal.SizeOf(typeof(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_CHANNEL_BIND_STATUS));

        // Win8# 559317 fixed a bug in Http.sys's HttpReceiveClientCertificate method.
        // Without this fix IOCP callbacks were not being called although ERROR_IO_PENDING was
        // returned from HttpReceiveClientCertificate when using the 
        // FileCompletionNotificationModes.SkipCompletionPortOnSuccess flag.
        // This bug was only hit when the buffer passed into HttpReceiveClientCertificate
        // (1500 bytes initially) is tool small for the certificate.
        // Due to this bug in downlevel operating systems the FileCompletionNotificationModes.SkipCompletionPortOnSuccess
        // flag is only used on Win8 and later.
        internal static readonly bool SkipIOCPCallbackOnSuccess = ComNetOS.IsWin8orLater;

        // Mitigate potential DOS attacks by limiting the number of unknown headers we accept.  Numerous header names 
        // with hash collisions will cause the server to consume excess CPU.  1000 headers limits CPU time to under 
        // 0.5 seconds per request.  Respond with a 400 Bad Request.
        private const int UnknownHeaderLimit = 1000;

        private static byte[] s_WwwAuthenticateBytes = new byte[]
        {
            (byte) 'W', (byte) 'W', (byte) 'W', (byte) '-', (byte) 'A', (byte) 'u', (byte) 't', (byte) 'h',
            (byte) 'e', (byte) 'n', (byte) 't', (byte) 'i', (byte) 'c', (byte) 'a', (byte) 't', (byte) 'e'
        };

        private class AuthenticationSelectorInfo
        {
            private AuthenticationSchemeSelector m_SelectorDelegate;
            private bool m_CanUseAdvancedAuth;

            internal AuthenticationSelectorInfo(AuthenticationSchemeSelector selectorDelegate, bool canUseAdvancedAuth)
            {
                Debug.Assert(selectorDelegate != null);

                m_SelectorDelegate = selectorDelegate;
                m_CanUseAdvancedAuth = canUseAdvancedAuth;
            }

            internal AuthenticationSchemeSelector Delegate
            {
                get
                {
                    return m_SelectorDelegate;
                }
            }

            internal bool AdvancedAuth
            {
                get
                {
                    return m_CanUseAdvancedAuth;
                }
            }
        }

        private AuthenticationSelectorInfo m_AuthenticationDelegate;
        private AuthenticationSchemes m_AuthenticationScheme = AuthenticationSchemes.Anonymous;
        private SecurityException m_SecurityException;
        private string m_Realm;
        private CriticalHandle m_RequestQueueHandle;
        private bool m_RequestHandleBound;
        private volatile State m_State; // m_State is set only within lock blocks, but often read outside locks. 
        private HttpListenerPrefixCollection m_Prefixes;
        private bool m_IgnoreWriteExceptions;
        private bool m_UnsafeConnectionNtlmAuthentication;
        private ExtendedProtectionSelector m_ExtendedProtectionSelectorDelegate;
        private ExtendedProtectionPolicy m_ExtendedProtectionPolicy;
        private ServiceNameStore m_DefaultServiceNames;
        private HttpServerSessionHandle m_ServerSessionHandle;
        private ulong m_UrlGroupId;
        private HttpListenerTimeoutManager m_TimeoutManager;
        private bool m_V2Initialized;

        private Hashtable m_DisconnectResults;         // ulong -> DisconnectAsyncResult
        private object m_InternalLock;

        internal Hashtable m_UriPrefixes = new Hashtable();

        public delegate ExtendedProtectionPolicy ExtendedProtectionSelector(HttpListenerRequest request);

        public HttpListener()
        {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "HttpListener", "");
            if (!UnsafeNclNativeMethods.HttpApi.Supported) {
                throw new PlatformNotSupportedException();
            }

            Debug.Assert(UnsafeNclNativeMethods.HttpApi.ApiVersion == 
                UnsafeNclNativeMethods.HttpApi.HTTP_API_VERSION.Version20, "Invalid Http api version");

            m_State = State.Stopped;
            m_InternalLock = new object();
            m_DefaultServiceNames = new ServiceNameStore();

            m_TimeoutManager = new HttpListenerTimeoutManager(this);

            // default: no CBT checks on any platform (appcompat reasons); applies also to PolicyEnforcement 
            // config element
            m_ExtendedProtectionPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);

            if (Logging.On) Logging.Exit(Logging.HttpListener, this, "HttpListener", "");
        }

        internal CriticalHandle RequestQueueHandle {
            get {
                return m_RequestQueueHandle;
            }
        }

        public AuthenticationSchemeSelector AuthenticationSchemeSelectorDelegate {
            get {
                AuthenticationSelectorInfo selector = m_AuthenticationDelegate;
                return selector == null ? null : selector.Delegate;
            }
            set {
                CheckDisposed();

                try
                {
                    new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Demand();
                    m_AuthenticationDelegate = new AuthenticationSelectorInfo(value, true);
                }
                catch (SecurityException exception)
                {
                    m_SecurityException = exception;
                    m_AuthenticationDelegate = new AuthenticationSelectorInfo(value, false);
                }
            }
        }

        public ExtendedProtectionSelector ExtendedProtectionSelectorDelegate
        {
            get {
                return m_ExtendedProtectionSelectorDelegate;
            }
            set {
                CheckDisposed();
                if (value == null) {
                    throw new ArgumentNullException();
                }

                if (!AuthenticationManager.OSSupportsExtendedProtection) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.security_ExtendedProtection_NoOSSupport));
                }

                m_ExtendedProtectionSelectorDelegate = value;
            }
        }

        public AuthenticationSchemes AuthenticationSchemes {
            get {
                return m_AuthenticationScheme;
            }
            set {
                CheckDisposed();

                // Enabling certain schemes requires special permissions.
                if ((value & (AuthenticationSchemes.Digest | AuthenticationSchemes.Negotiate | AuthenticationSchemes.Ntlm)) != 0)
                {
                    new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Demand();
                }

                m_AuthenticationScheme = value;
            }
        }

        public ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get {
                return m_ExtendedProtectionPolicy;
            }
            set {
                CheckDisposed();
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!AuthenticationManager.OSSupportsExtendedProtection && value.PolicyEnforcement == PolicyEnforcement.Always)
                {
                    throw new PlatformNotSupportedException(SR.GetString(SR.security_ExtendedProtection_NoOSSupport));
                }
                if (value.CustomChannelBinding != null)
                {
                    throw new ArgumentException(SR.GetString(SR.net_listener_cannot_set_custom_cbt), "CustomChannelBinding");
                }

                m_ExtendedProtectionPolicy = value;
            }
        }

        public ServiceNameCollection DefaultServiceNames
        {
            get { 
                return m_DefaultServiceNames.ServiceNames; 
            }
        }

        public string Realm {
            get {
                return m_Realm;
            }
            set {
                CheckDisposed();
                m_Realm = value;
            }
        }

        private void ValidateV2Property() {
            // Make sure that calling CheckDisposed and SetupV2Config is an atomic operation. This 
            // avoids race conditions if the listener is aborted/closed after CheckDisposed(), but 
            // before SetupV2Config().
            lock (m_InternalLock) {
                CheckDisposed();
                SetupV2Config();
            }
        }

        private void SetUrlGroupProperty(UnsafeNclNativeMethods.HttpApi.HTTP_SERVER_PROPERTY property, IntPtr info, uint infosize) {
            uint statusCode = UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS;

            GlobalLog.Assert(m_UrlGroupId != 0, "SetUrlGroupProperty called with invalid url group id");
            GlobalLog.Assert(info != IntPtr.Zero, "SetUrlGroupProperty called with invalid pointer");

            //
            // Set the url group property using Http Api.
            //
            statusCode = UnsafeNclNativeMethods.HttpApi.HttpSetUrlGroupProperty(
                m_UrlGroupId, property, info, infosize);

            if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                HttpListenerException exception = new HttpListenerException((int)statusCode);
                if (Logging.On) Logging.Exception(Logging.HttpListener, this, "HttpSetUrlGroupProperty:: Property: " +
                    property, exception);
                throw exception;
            }
        }

        internal void SetServerTimeout(int[] timeouts, uint minSendBytesPerSecond) {
            ValidateV2Property(); // CheckDispose and initilize HttpListener in the case of app.config timeouts

            UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_LIMIT_INFO timeoutinfo =
                new UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_LIMIT_INFO();

            timeoutinfo.Flags = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_PROPERTY_FLAG_PRESENT;
            timeoutinfo.DrainEntityBody = 
                (ushort)timeouts[(int)UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.DrainEntityBody];
            timeoutinfo.EntityBody =
                (ushort)timeouts[(int)UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.EntityBody];
            timeoutinfo.RequestQueue =
                (ushort)timeouts[(int)UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.RequestQueue];
            timeoutinfo.IdleConnection =
                (ushort)timeouts[(int)UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.IdleConnection];
            timeoutinfo.HeaderWait =
                (ushort)timeouts[(int)UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_TYPE.HeaderWait];
            timeoutinfo.MinSendRate = minSendBytesPerSecond;                

            IntPtr infoptr = new IntPtr(&timeoutinfo);

            SetUrlGroupProperty(
                UnsafeNclNativeMethods.HttpApi.HTTP_SERVER_PROPERTY.HttpServerTimeoutsProperty,
                infoptr, (uint)Marshal.SizeOf(typeof(UnsafeNclNativeMethods.HttpApi.HTTP_TIMEOUT_LIMIT_INFO)));
        }

        public HttpListenerTimeoutManager TimeoutManager {
            get {
                ValidateV2Property();
                Debug.Assert(m_TimeoutManager != null, "Timeout manager is not assigned");
                return m_TimeoutManager;
            }
        }       

        public static bool IsSupported {
            get {
                return UnsafeNclNativeMethods.HttpApi.Supported;
            }
        }

        public bool IsListening {
            get {
                return m_State==State.Started;
            }
        }

        public bool IgnoreWriteExceptions {
            get {
                return m_IgnoreWriteExceptions;
            }
            set {
                CheckDisposed();
                m_IgnoreWriteExceptions = value;
            }
        }

        public bool UnsafeConnectionNtlmAuthentication {
            get {
                return m_UnsafeConnectionNtlmAuthentication;
            }

            set {
                CheckDisposed();
                if (m_UnsafeConnectionNtlmAuthentication==value) {
                    return;
                }
                lock (DisconnectResults.SyncRoot)
                {
                    if (m_UnsafeConnectionNtlmAuthentication == value)
                    {
                        return;
                    }
                    m_UnsafeConnectionNtlmAuthentication = value;
                    if (!value)
                    {
                        foreach (DisconnectAsyncResult result in DisconnectResults.Values)
                        {
                            result.AuthenticatedConnection = null;
                        }
                    }
                }
            }
        }

        private Hashtable DisconnectResults
        {
            get
            {
                if (m_DisconnectResults == null)
                {
                    lock (m_InternalLock)
                    {
                        if (m_DisconnectResults == null)
                        {
                            m_DisconnectResults = Hashtable.Synchronized(new Hashtable());
                        }
                    }
                }
                return m_DisconnectResults;
            }
        }

        internal void AddPrefix(string uriPrefix)
        {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "AddPrefix", "uriPrefix:" + uriPrefix);
            string registeredPrefix = null;
            try {
                if (uriPrefix==null) {
                    throw new ArgumentNullException("uriPrefix");
                }
                (new WebPermission(NetworkAccess.Accept, uriPrefix)).Demand();
                CheckDisposed();
                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::AddPrefix() uriPrefix:" + uriPrefix);
                int i;
                if (string.Compare(uriPrefix, 0, "http://", 0, 7, StringComparison.OrdinalIgnoreCase)==0) {
                    i = 7;
                }
                else if (string.Compare(uriPrefix, 0, "https://", 0, 8, StringComparison.OrdinalIgnoreCase)==0) {
                    i = 8;
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.net_listener_scheme), "uriPrefix");
                }
                bool inSquareBrakets = false;
                int j = i;
                while (j<uriPrefix.Length && uriPrefix[j]!='/' && (uriPrefix[j]!=':' || inSquareBrakets)) {
                    if (uriPrefix[j]=='[') {
                        if (inSquareBrakets) {
                            j = i;
                            break;
                        }
                        inSquareBrakets = true;
                    }
                    if (inSquareBrakets && uriPrefix[j]==']') {
                        inSquareBrakets = false;
                    }
                    j++;
                }
                if (i==j) {
                    throw new ArgumentException(SR.GetString(SR.net_listener_host), "uriPrefix");
                }
                if (uriPrefix[uriPrefix.Length-1]!='/') {
                    throw new ArgumentException(SR.GetString(SR.net_listener_slash), "uriPrefix");
                }
                registeredPrefix = uriPrefix[j]==':' ? String.Copy(uriPrefix) : uriPrefix.Substring(0, j) + (i==7 ? ":80" : ":443") + uriPrefix.Substring(j);
                fixed (char* pChar = registeredPrefix) {
                    i = 0;
                    while (pChar[i]!=':') {
                        pChar[i] = (char)CaseInsensitiveAscii.AsciiToLower[(byte)pChar[i]];
                        i++;
                    }
                }
                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::AddPrefix() mapped uriPrefix:" + uriPrefix + " to registeredPrefix:" + registeredPrefix);
                if (m_State==State.Started) {
                    GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::AddPrefix() calling UnsafeNclNativeMethods.HttpApi.HttpAddUrl[ToUrlGroup]");
                    uint statusCode = InternalAddPrefix(registeredPrefix);
                    if (statusCode!=UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                        if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_ALREADY_EXISTS)
                            throw new HttpListenerException((int)statusCode, SR.GetString(SR.net_listener_already, registeredPrefix));
                        else
                            throw new HttpListenerException((int)statusCode);
                    }
                }
                m_UriPrefixes[uriPrefix] = registeredPrefix;
                m_DefaultServiceNames.Add(uriPrefix);
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.HttpListener, this, "AddPrefix", exception);
                throw;
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "AddPrefix", "prefix:" + registeredPrefix);
            }
        }

        public HttpListenerPrefixCollection Prefixes {
            get {
                if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Prefixes_get", "");
                CheckDisposed();
                if (m_Prefixes==null) {
                    m_Prefixes = new HttpListenerPrefixCollection(this);
                }
                return m_Prefixes;
            }
        }

        internal bool RemovePrefix(string uriPrefix) {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "RemovePrefix", "uriPrefix:" + uriPrefix);
            try {
                CheckDisposed();
                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::RemovePrefix() uriPrefix:" + uriPrefix);
                if (uriPrefix==null) {
                    throw new ArgumentNullException("uriPrefix");
                }
                
                if(!m_UriPrefixes.Contains(uriPrefix)){
                    return false;
                }
                
                if (m_State==State.Started) {
                    InternalRemovePrefix((string)m_UriPrefixes[uriPrefix]);
                }

                m_UriPrefixes.Remove(uriPrefix);
                m_DefaultServiceNames.Remove(uriPrefix);
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.HttpListener, this, "RemovePrefix", exception);
                throw;
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "RemovePrefix", "uriPrefix:" + uriPrefix);
            }
            return true;
        }

        internal void RemoveAll(bool clear) {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "RemoveAll", "");
            try {
                CheckDisposed();
                // go through the uri list and unregister for each one of them
                if (m_UriPrefixes.Count>0) {
                    if (m_State==State.Started) {
                        foreach (string registeredPrefix in m_UriPrefixes.Values) {
                            // ignore possible failures
                            InternalRemovePrefix(registeredPrefix);
                        }
                    }
                    
                    if (clear) {
                        m_UriPrefixes.Clear();
                        m_DefaultServiceNames.Clear();
                    }
                }
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "RemoveAll", "");
            }
        }

        private IntPtr DangerousGetHandle() {
            return ((HttpRequestQueueV2Handle)m_RequestQueueHandle).DangerousGetHandle();
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.UnmanagedCode)]
        internal void EnsureBoundHandle()
        {
            if (!m_RequestHandleBound)
            {
                lock (m_InternalLock)
                {
                    if (!m_RequestHandleBound)
                    {
                        ThreadPool.BindHandle(DangerousGetHandle());
                        m_RequestHandleBound = true;
                    }
                }
            }
        }

        private void SetupV2Config() {
            uint statusCode = UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS;
            ulong id = 0;

            //
            // If we have already initialized V2 config, then nothing to do.
            //
            if (m_V2Initialized) {
                return;
            }

            //
            // V2 initialization sequence:
            // 1. Create server session
            // 2. Create url group
            // 3. Create request queue - Done in Start()
            // 4. Add urls to url group - Done in Start()
            // 5. Attach request queue to url group - Done in Start()
            //

            try {
                statusCode = UnsafeNclNativeMethods.HttpApi.HttpCreateServerSession(
                    UnsafeNclNativeMethods.HttpApi.Version, &id, 0);

                if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                    throw new HttpListenerException((int)statusCode);
                }

                GlobalLog.Assert(id != 0, "Invalid id returned by HttpCreateServerSession");
                
                m_ServerSessionHandle = new HttpServerSessionHandle(id);

                id = 0;
                statusCode = UnsafeNclNativeMethods.HttpApi.HttpCreateUrlGroup(
                    m_ServerSessionHandle.DangerousGetServerSessionId(), &id, 0);

                if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                    throw new HttpListenerException((int)statusCode);
                }

                GlobalLog.Assert(id != 0, "Invalid id returned by HttpCreateUrlGroup");
                m_UrlGroupId = id;                

                m_V2Initialized = true;
            }
            catch (Exception exception) {
                //
                // If V2 initialization fails, we mark object as unusable.
                //
                m_State = State.Closed;

                //
                // If Url group or request queue creation failed, close server session before throwing.
                //
                if (m_ServerSessionHandle != null) {
                    m_ServerSessionHandle.Close();
                }
                if (Logging.On) Logging.Exception(Logging.HttpListener, this, "SetupV2Config", exception);
                throw;
            }
        }

        public void Start() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Start", "");

            // Make sure there are no race conditions between Start/Stop/Abort/Close/Dispose and
            // calls to SetupV2Config: Start needs to setup all resources (esp. in V2 where besides
            // the request handle, there is also a server session and a Url group. Abort/Stop must
            // not interfere while Start is allocating those resources. The lock also makes sure
            // all methods changing state can read and change the state in an atomic way.
            lock (m_InternalLock) {
                try {
                    CheckDisposed();
                    if (m_State==State.Started) {
                        return;
                    }

                    // SetupV2Config() is not called in the ctor, because it may throw. This would
                    // be a regression since in v1 the ctor never threw. Besides, ctors should do 
                    // minimal work according to the framework design guidelines.
                    SetupV2Config();
                    CreateRequestQueueHandle();
                    AttachRequestQueueToUrlGroup();

                    // All resources are set up correctly. Now add all prefixes.
                    try {
                        AddAllPrefixes();
                    }
                    catch (HttpListenerException) {
                        // If an error occured while adding prefixes, free all resources allocated by previous steps.
                        DetachRequestQueueFromUrlGroup();
                        ClearDigestCache();
                        throw;
                    }

                    m_State = State.Started;
                } catch (Exception exception) {
                    // Make sure the HttpListener instance can't be used if Start() failed.
                    m_State = State.Closed;
                    CloseRequestQueueHandle();
                    CleanupV2Config();
                    if (Logging.On) Logging.Exception(Logging.HttpListener, this, "Start", exception);
                    throw;
                } finally {
                    if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Start", "");
                }
            }
        }

        private void CleanupV2Config() {

            //
            // If we never setup V2, just return.
            //
            if (!m_V2Initialized) {
                return;
            }

            //
            // V2 stopping sequence:
            // 1. Detach request queue from url group - Done in Stop()/Abort()
            // 2. Remove urls from url group - Done in Stop()
            // 3. Close request queue - Done in Stop()/Abort()
            // 4. Close Url group.
            // 5. Close server session.
            
            GlobalLog.Assert(m_UrlGroupId != 0, "HttpCloseUrlGroup called with invalid url group id");

            uint statusCode = UnsafeNclNativeMethods.HttpApi.HttpCloseUrlGroup(m_UrlGroupId);

            if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                if (Logging.On) Logging.PrintError(Logging.HttpListener, this, "CloseV2Config", SR.GetString(SR.net_listener_close_urlgroup_error, statusCode));
            }
            m_UrlGroupId = 0;

            GlobalLog.Assert(m_ServerSessionHandle != null, "ServerSessionHandle is null in CloseV2Config");
            GlobalLog.Assert(!m_ServerSessionHandle.IsInvalid, "ServerSessionHandle is invalid in CloseV2Config");

            m_ServerSessionHandle.Close();
        }

        private void AttachRequestQueueToUrlGroup() {
            //
            // Set the association between request queue and url group. After this, requests for registered urls will 
            // get delivered to this request queue.
            //
            UnsafeNclNativeMethods.HttpApi.HTTP_BINDING_INFO info = new UnsafeNclNativeMethods.HttpApi.HTTP_BINDING_INFO();
            info.Flags = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_PROPERTY_FLAG_PRESENT;
            info.RequestQueueHandle = DangerousGetHandle();

            IntPtr infoptr = new IntPtr(&info);

            SetUrlGroupProperty(UnsafeNclNativeMethods.HttpApi.HTTP_SERVER_PROPERTY.HttpServerBindingProperty,
                infoptr, (uint)Marshal.SizeOf(typeof(UnsafeNclNativeMethods.HttpApi.HTTP_BINDING_INFO)));
        }

        private void DetachRequestQueueFromUrlGroup() {
            GlobalLog.Assert(m_UrlGroupId != 0, "DetachRequestQueueFromUrlGroup can't detach using Url group id 0.");

            //
            // Break the association between request queue and url group. After this, requests for registered urls 
            // will get 503s.
            // Note that this method may be called multiple times (Stop() and then Abort()). This
            // is fine since http.sys allows to set HttpServerBindingProperty multiple times for valid 
            // Url groups.
            //
            UnsafeNclNativeMethods.HttpApi.HTTP_BINDING_INFO info = new UnsafeNclNativeMethods.HttpApi.HTTP_BINDING_INFO();
            info.Flags = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE;
            info.RequestQueueHandle = IntPtr.Zero;

            IntPtr infoptr = new IntPtr(&info);

            uint statusCode = UnsafeNclNativeMethods.HttpApi.HttpSetUrlGroupProperty(m_UrlGroupId, 
                UnsafeNclNativeMethods.HttpApi.HTTP_SERVER_PROPERTY.HttpServerBindingProperty,
                infoptr, (uint)Marshal.SizeOf(typeof(UnsafeNclNativeMethods.HttpApi.HTTP_BINDING_INFO)));

            if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                if (Logging.On) Logging.PrintError(Logging.HttpListener, this, "DetachRequestQueueFromUrlGroup", SR.GetString(SR.net_listener_detach_error, statusCode));
            }
        }

        public void Stop() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Stop", "");
            try {
                lock (m_InternalLock) {
                    CheckDisposed();
                    if (m_State==State.Stopped) {
                        return;
                    }

                    RemoveAll(false);
                    DetachRequestQueueFromUrlGroup();

                    // Even though it would be enough to just detach the request queue in v2, in order to
                    // keep app compat with earlier versions of the framework, we need to close the request queue.
                    // This will make sure that pending GetContext() calls will complete and throw an exception. Just
                    // detaching the url group from the request queue would not cause GetContext() to return.
                    CloseRequestQueueHandle();
                                    
                    m_State = State.Stopped;
                }

                ClearDigestCache();
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.HttpListener, this, "Stop", exception);
                throw;
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Stop", "");
            }
        }

        private unsafe void CreateRequestQueueHandle() {
            uint statusCode = UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS;
            
            HttpRequestQueueV2Handle requestQueueHandle = null;
            statusCode =
                UnsafeNclNativeMethods.SafeNetHandles.HttpCreateRequestQueue(
                    UnsafeNclNativeMethods.HttpApi.Version, null, null, 0, out requestQueueHandle);

            if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                throw new HttpListenerException((int)statusCode);
            }

            // Disabling callbacks when IO operation completes synchronously (returns ErrorCodes.ERROR_SUCCESS)
            if (SkipIOCPCallbackOnSuccess &&
                !UnsafeNclNativeMethods.SetFileCompletionNotificationModes(
                    requestQueueHandle,
                    UnsafeNclNativeMethods.FileCompletionNotificationModes.SkipCompletionPortOnSuccess |
                    UnsafeNclNativeMethods.FileCompletionNotificationModes.SkipSetEventOnHandle))
            {
                throw new HttpListenerException(Marshal.GetLastWin32Error());
            }

            m_RequestQueueHandle = requestQueueHandle;
        }

        private unsafe void CloseRequestQueueHandle() {

            if ((m_RequestQueueHandle != null) && (!m_RequestQueueHandle.IsInvalid)) {
                m_RequestQueueHandle.Close();
                m_RequestHandleBound = false;
            }
        }

        public void Abort() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Abort", "");
            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::Abort()");

            lock (m_InternalLock) {
                try {
                    if (m_State == State.Closed) {
                        return;
                    }

                    // Just detach and free resources. Don't call Stop (which may throw). Behave like v1: just 
                    // clean up resources.   
                    if (m_State == State.Started) {
                        DetachRequestQueueFromUrlGroup();
                        CloseRequestQueueHandle();
                    }
                    CleanupV2Config();               
                    ClearDigestCache();
                } catch (Exception exception) {
                    if(Logging.On)Logging.Exception(Logging.HttpListener, this, "Abort", exception);
                    throw;
                } finally {
                    m_State = State.Closed;
                    if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Abort", "");
                }
            }
        }

        public void Close() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Close", "");
            try {
                GlobalLog.Print("HttpListenerRequest::Close()");
                ((IDisposable)this).Dispose();
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.HttpListener, this, "Close", exception);
                throw;
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Close", "");
            }
        }

        // old API, now private, and helper methods
        private void Dispose(bool disposing) {
            GlobalLog.Assert(disposing, "Dispose(bool) does nothing if called from the finalizer.");

            if (!disposing) {
                return;
            }

            if (Logging.On) Logging.Enter(Logging.HttpListener, this, "Dispose", "");
            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::Dispose()");

            lock (m_InternalLock) {
                try {
                    if (m_State == State.Closed){
                        return;
                    }

                    Stop();
                    CleanupV2Config();
                } catch (Exception exception) {
                    if (Logging.On) Logging.Exception(Logging.HttpListener, this, "Dispose", exception);
                    throw;
                } finally {
                    m_State = State.Closed;
                    if (Logging.On) Logging.Exit(Logging.HttpListener, this, "Dispose", "");
                }
            }
        }

        /// <internalonly/>
        void IDisposable.Dispose() {
            Dispose(true);
        }

        private uint InternalAddPrefix(string uriPrefix) {            
            uint statusCode = 0;

            statusCode =
                UnsafeNclNativeMethods.HttpApi.HttpAddUrlToUrlGroup(
                    m_UrlGroupId,
                    uriPrefix,
                    0,
                    0);
            
            return statusCode;
        }

        private bool InternalRemovePrefix(string uriPrefix) {
            uint statusCode = 0;

            statusCode = 
                UnsafeNclNativeMethods.HttpApi.HttpRemoveUrlFromUrlGroup(
                    m_UrlGroupId, 
                    uriPrefix, 
                    0);
                        
            if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_NOT_FOUND)
            {
                return false;
            }
            return true;
        }

        private void AddAllPrefixes() {
            // go through the uri list and register for each one of them
            if (m_UriPrefixes.Count>0) {
                foreach (string registeredPrefix in m_UriPrefixes.Values) {
                    uint statusCode = InternalAddPrefix(registeredPrefix);
                    if (statusCode!=UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                        if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_ALREADY_EXISTS)
                            throw new HttpListenerException((int)statusCode, SR.GetString(SR.net_listener_already, registeredPrefix));
                        else
                            throw new HttpListenerException((int)statusCode);
                    }
                }
            }
        }

        public HttpListenerContext GetContext() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "GetContext", "");

            SyncRequestContext memoryBlob = null;
            HttpListenerContext httpContext = null;
            bool stoleBlob = false;

            try {
                CheckDisposed();
                if (m_State==State.Stopped) {
                    throw new InvalidOperationException(SR.GetString(SR.net_listener_mustcall, "Start()"));
                }
                if (m_UriPrefixes.Count==0) {
                    throw new InvalidOperationException(SR.GetString(SR.net_listener_mustcall, "AddPrefix()"));
                }
                uint statusCode = UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS;
                uint size = 4096;
                ulong requestId = 0;
                memoryBlob = new SyncRequestContext((int) size);
                for (;;) {
                    for (;;) {
                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::GetContext() calling UnsafeNclNativeMethods.HttpApi.HttpReceiveHttpRequest RequestId:" + requestId);
                        uint bytesTransferred = 0;
                        statusCode =
                            UnsafeNclNativeMethods.HttpApi.HttpReceiveHttpRequest(
                                m_RequestQueueHandle,
                                requestId,
                                (uint)UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_RECEIVE_REQUEST_FLAG_COPY_BODY,
                                memoryBlob.RequestBlob,
                                size,
                                &bytesTransferred,
                                null);

                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::GetContext() call to UnsafeNclNativeMethods.HttpApi.HttpReceiveHttpRequest returned:" + statusCode);

                        if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_INVALID_PARAMETER && requestId != 0) {
                            // we might get this if somebody stole our RequestId,
                            // we need to start all over again but we can reuse the buffer we just allocated
                            requestId = 0;
                            continue;
                        }
                        else if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA)
                        {
                            // the buffer was not big enough to fit the headers, we need
                            // to read the RequestId returned, allocate a new buffer of the required size
                            size = bytesTransferred;
                            requestId = memoryBlob.RequestBlob->RequestId;
                            memoryBlob.Reset(checked((int) size));
                            continue;
                        }
                        break;
                    }
                    if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS)
                    {
                        // someother bad error, possible(?) return values are:
                        // ERROR_INVALID_HANDLE, ERROR_INSUFFICIENT_BUFFER, ERROR_OPERATION_ABORTED
                        throw new HttpListenerException((int)statusCode);
                    }

                    if (ValidateRequest(memoryBlob))
                    {
                        // We need to hook up our authentication handling code here.
                        httpContext = HandleAuthentication(memoryBlob, out stoleBlob);
                    }

                    if (stoleBlob)
                    {
                        // The request has been handed to the user, which means this code can't reuse the blob.  Reset it here.
                        memoryBlob = null;
                        stoleBlob = false;
                    }
                    GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::GetContext() HandleAuthentication() returned httpContext#" + ValidationHelper.HashString(httpContext));
                    // if the request survived authentication, return it to the user
                    if (httpContext != null) {
                        return httpContext;
                    }

                    // HandleAuthentication may have cleaned this up.
                    if (memoryBlob == null)
                    {
                        memoryBlob = new SyncRequestContext(checked((int) size));
                    }

                    requestId = 0;
                }
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.HttpListener, this, "GetContext", exception);
                throw;
            } finally {
                if (memoryBlob != null && !stoleBlob)
                {
                    memoryBlob.ReleasePins();
                    memoryBlob.Close();
                }
                if (Logging.On) Logging.Exit(Logging.HttpListener, this, "GetContext", "HttpListenerContext#" + ValidationHelper.HashString(httpContext) + " RequestTraceIdentifier#" + (httpContext != null ? httpContext.Request.RequestTraceIdentifier.ToString() : "<null>"));
            }
        }

        internal unsafe bool ValidateRequest(RequestContextBase requestMemory)
        {
            // Block potential DOS attacks
            if (requestMemory.RequestBlob->Headers.UnknownHeaderCount > UnknownHeaderLimit)
            {
                SendError(requestMemory.RequestBlob->RequestId, HttpStatusCode.BadRequest, null);
                return false;
            }
            return true;
        }

        [HostProtection(ExternalThreading=true)]
        public IAsyncResult BeginGetContext(AsyncCallback callback, object state) {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "BeginGetContext", "");
            ListenerAsyncResult asyncResult = null;
            try {
                CheckDisposed();
                if (m_State==State.Stopped) {
                    throw new InvalidOperationException(SR.GetString(SR.net_listener_mustcall, "Start()"));
                }
                // prepare the ListenerAsyncResult object (this will have it's own
                // event that the user can wait on for IO completion - which means we
                // need to signal it when IO completes)
                asyncResult = new ListenerAsyncResult(this, state, callback);
                uint statusCode = asyncResult.QueueBeginGetContext();
                if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS &&
                    statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING)
                {
                    // someother bad error, possible(?) return values are:
                    // ERROR_INVALID_HANDLE, ERROR_INSUFFICIENT_BUFFER, ERROR_OPERATION_ABORTED
                    throw new HttpListenerException((int)statusCode);
                }
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.HttpListener, this, "BeginGetContext", exception);
                throw;
            } finally {
                if(Logging.On)Logging.Enter(Logging.HttpListener, this, "BeginGetContext", "IAsyncResult#" + ValidationHelper.HashString(asyncResult));
            }

            return asyncResult;
        }

        public HttpListenerContext EndGetContext(IAsyncResult asyncResult) {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "EndGetContext", "IAsyncResult#" + ValidationHelper.HashString(asyncResult));
            HttpListenerContext httpContext = null;
            try {
                CheckDisposed();
                if (asyncResult==null) {
                    throw new ArgumentNullException("asyncResult");
                }
                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::EndGetContext() asyncResult:" + ValidationHelper.ToString(asyncResult));
                ListenerAsyncResult castedAsyncResult = asyncResult as ListenerAsyncResult;
                if (castedAsyncResult==null || castedAsyncResult.AsyncObject!=this) {
                    throw new ArgumentException(SR.GetString(SR.net_io_invalidasyncresult), "asyncResult");
                }
                if (castedAsyncResult.EndCalled) {
                    throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndGetContext"));
                }
                castedAsyncResult.EndCalled = true;
                httpContext = castedAsyncResult.InternalWaitForCompletion() as HttpListenerContext;
                if (httpContext == null) {
                    GlobalLog.Assert(castedAsyncResult.Result is Exception, "EndGetContext|The result is neither a HttpListenerContext nor an Exception.");
                    throw castedAsyncResult.Result as Exception;
                }
            } catch (Exception exception) {
                if(Logging.On)Logging.Exception(Logging.HttpListener, this, "EndGetContext", exception);
                throw;
            } finally {
                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::EndGetContext() returning HttpListenerContext#" + ValidationHelper.ToString(httpContext));
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "EndGetContext", httpContext == null ? "<no context>" : "HttpListenerContext#" + ValidationHelper.HashString(httpContext) + " RequestTraceIdentifier#" + httpContext.Request.RequestTraceIdentifier);
            }
            return httpContext;
        }

        //************* Task-based async public methods *************************
        [HostProtection(ExternalThreading = true)]
        public Task<HttpListenerContext> GetContextAsync()
        {
            return Task<HttpListenerContext>.Factory.FromAsync(BeginGetContext, EndGetContext, null);
        }


        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        internal static WindowsIdentity CreateWindowsIdentity(IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated)
        {
            return new WindowsIdentity(userToken, type, acctType, isAuthenticated);
        }

        internal HttpListenerContext HandleAuthentication(RequestContextBase memoryBlob, out bool stoleBlob)
        {
            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() memoryBlob:0x" + ((IntPtr) memoryBlob.RequestBlob).ToString("x"));

            string challenge = null;
            stoleBlob = false;

            // Some things we need right away.  Lift them out now while it's convenient.
            string verb = UnsafeNclNativeMethods.HttpApi.GetVerb(memoryBlob.RequestBlob);
            string authorizationHeader = UnsafeNclNativeMethods.HttpApi.GetKnownHeader(memoryBlob.RequestBlob, (int) HttpRequestHeader.Authorization);
            ulong connectionId = memoryBlob.RequestBlob->ConnectionId;
            ulong requestId = memoryBlob.RequestBlob->RequestId;
            bool isSecureConnection = memoryBlob.RequestBlob->pSslInfo != null;

            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() authorizationHeader:" + ValidationHelper.ToString(authorizationHeader));

            // if the app has turned on AuthPersistence, an anonymous request might
            // be authenticated by virtue of it coming on a connection that was
            // previously authenticated.
            // assurance that we do this only for NTLM/Negotiate is not here, but in the
            // code that caches WindowsIdentity instances in the Dictionary.
            DisconnectAsyncResult disconnectResult = (DisconnectAsyncResult) DisconnectResults[connectionId];
            if (UnsafeConnectionNtlmAuthentication)
            {
                if (authorizationHeader == null)
                {
                    WindowsPrincipal principal = disconnectResult == null ? null : disconnectResult.AuthenticatedConnection;
                    if (principal != null)
                    {
                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() got principal:" + ValidationHelper.ToString(principal) + " principal.Identity.Name:" + ValidationHelper.ToString(principal.Identity.Name) + " creating request");
                        stoleBlob = true;
                        HttpListenerContext ntlmContext = new HttpListenerContext(this, memoryBlob);
                        ntlmContext.SetIdentity(principal, null);
                        ntlmContext.Request.ReleasePins();
                        return ntlmContext;
                    }
                }
                else
                {
                    // They sent an authorization - destroy their previous credentials.
                    GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() clearing principal cache");
                    if (disconnectResult != null)
                    {
                        disconnectResult.AuthenticatedConnection = null;
                    }
                }
            }

            // Figure out what schemes we're allowing, what context we have.
            stoleBlob = true;
            HttpListenerContext httpContext = null;
            NTAuthentication oldContext = null;
            NTAuthentication newContext = null;
            NTAuthentication context = null;
            AuthenticationSchemes headerScheme = AuthenticationSchemes.None;
            AuthenticationSchemes authenticationScheme = AuthenticationSchemes;
            ExtendedProtectionPolicy extendedProtectionPolicy = m_ExtendedProtectionPolicy;
            try
            {
                // Take over handling disconnects for now.
                if (disconnectResult != null && !disconnectResult.StartOwningDisconnectHandling())
                {
                    // Oops!  Just disconnected just then.  Pretend we didn't see the disconnectResult.
                    disconnectResult = null;
                }

                // Pick out the old context now.  By default, it'll be removed in the finally, unless context is set somewhere. 
                if (disconnectResult != null)
                {
                    oldContext = disconnectResult.Session;
                }

                httpContext = new HttpListenerContext(this, memoryBlob);

                AuthenticationSelectorInfo authenticationSelector = m_AuthenticationDelegate;
                if (authenticationSelector != null)
                {
                    try
                    {
                        httpContext.Request.ReleasePins();
                        authenticationScheme = authenticationSelector.Delegate(httpContext.Request);
                        // Cache the results of authenticationSelector (if any)
                        httpContext.AuthenticationSchemes = authenticationScheme;
                        if (!authenticationSelector.AdvancedAuth &&
                            (authenticationScheme & (AuthenticationSchemes.Negotiate | AuthenticationSchemes.Ntlm | AuthenticationSchemes.Digest)) != 0)
                        {
                            throw m_SecurityException;
                        }
                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() AuthenticationSchemeSelectorDelegate() returned authenticationScheme:" + authenticationScheme);
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception)) throw;

                        if (Logging.On) Logging.PrintError(Logging.HttpListener, this, "HandleAuthentication", SR.GetString(SR.net_log_listener_delegate_exception, exception));
                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() AuthenticationSchemeSelectorDelegate() returned authenticationScheme:" + authenticationScheme);
                        SendError(requestId, HttpStatusCode.InternalServerError, null);
                        httpContext.Close();
                        return null;
                    }
                }
                else
                {
                    // We didn't give the request to the user yet, so we haven't lost control of the unmanaged blob and can
                    // continue to reuse the buffer.
                    stoleBlob = false;
                }

                ExtendedProtectionSelector extendedProtectionSelector = m_ExtendedProtectionSelectorDelegate;
                if (extendedProtectionSelector != null) 
                {
                    extendedProtectionPolicy = extendedProtectionSelector(httpContext.Request);

                    if (extendedProtectionPolicy == null) 
                    {
                        extendedProtectionPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
                    }
                    // Cache the results of extendedProtectionSelector (if any)
                    httpContext.ExtendedProtectionPolicy = extendedProtectionPolicy;
                }

                // Then figure out what scheme they're trying (if any are allowed)
                int index = -1;
                if (authorizationHeader != null && (authenticationScheme & ~AuthenticationSchemes.Anonymous) != AuthenticationSchemes.None)
                {
                    // Find the end of the scheme name.  Trust that HTTP.SYS parsed out just our header ok.
                    for (index = 0; index < authorizationHeader.Length; index++)
                    {
                        if (authorizationHeader[index] == ' ' || authorizationHeader[index] == '\t' ||
                            authorizationHeader[index] == '\r' || authorizationHeader[index] == '\n')
                        {
                            break;
                        }
                    }

                    // Currently only allow one Authorization scheme/header per request.
                    if (index < authorizationHeader.Length)
                    {
                        if ((authenticationScheme & AuthenticationSchemes.Negotiate) != AuthenticationSchemes.None &&
                            string.Compare(authorizationHeader, 0, NegotiateClient.AuthType, 0, index, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            headerScheme = AuthenticationSchemes.Negotiate;
                        }
                        else if ((authenticationScheme & AuthenticationSchemes.Ntlm) != AuthenticationSchemes.None &&
                            string.Compare(authorizationHeader, 0, NtlmClient.AuthType, 0, index, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            headerScheme = AuthenticationSchemes.Ntlm;
                        }
                        else if ((authenticationScheme & AuthenticationSchemes.Digest) != AuthenticationSchemes.None &&
                            string.Compare(authorizationHeader, 0, DigestClient.AuthType, 0, index, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            headerScheme = AuthenticationSchemes.Digest;
                        }
                        else if ((authenticationScheme & AuthenticationSchemes.Basic) != AuthenticationSchemes.None &&
                            string.Compare(authorizationHeader, 0, BasicClient.AuthType, 0, index, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            headerScheme = AuthenticationSchemes.Basic;
                        }
                        else
                        {
                            if (Logging.On) Logging.PrintWarning(Logging.HttpListener, this, "HandleAuthentication", SR.GetString(SR.net_log_listener_unsupported_authentication_scheme, authorizationHeader , authenticationScheme));
                        }
                    }
                }

                // httpError holds the error we will return if an Authorization header is present but can't be authenticated
                HttpStatusCode httpError = HttpStatusCode.InternalServerError;
                bool error = false;

                // See if we found an acceptable auth header
                if (headerScheme == AuthenticationSchemes.None)
                {
                    if (Logging.On) Logging.PrintWarning(Logging.HttpListener, this, "HandleAuthentication", SR.GetString(SR.net_log_listener_unmatched_authentication_scheme, ValidationHelper.ToString(authenticationScheme), (authorizationHeader == null ? "<null>" : authorizationHeader)));

                    // If anonymous is allowed, just return the context.  Otherwise go for the 401.
                    if ((authenticationScheme & AuthenticationSchemes.Anonymous) != AuthenticationSchemes.None)
                    {
                        if (!stoleBlob)
                        {
                            stoleBlob = true;
                            httpContext.Request.ReleasePins();
                        }
                        return httpContext;
                    }

                    httpError = HttpStatusCode.Unauthorized;
                    httpContext.Request.DetachBlob(memoryBlob);
                    httpContext.Close();
                    httpContext = null;
                }
                else
                {
                    // Perform Authentication
                    byte[] bytes = null;
                    byte[] decodedOutgoingBlob = null;
                    string outBlob = null;

                    // Find the beginning of the blob.  Trust that HTTP.SYS parsed out just our header ok.
                    for (index++; index < authorizationHeader.Length; index++)
                    {
                        if (authorizationHeader[index] != ' ' && authorizationHeader[index] != '\t' &&
                            authorizationHeader[index] != '\r' && authorizationHeader[index] != '\n')
                        {
                            break;
                        }
                    }
                    string inBlob = index < authorizationHeader.Length ? authorizationHeader.Substring(index) : "";

                    IPrincipal principal = null;
                    SecurityStatus statusCodeNew;
                    ChannelBinding binding;
                    GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() Performing Authentication headerScheme:" + ValidationHelper.ToString(headerScheme));
                    switch (headerScheme)
                    {
                        case AuthenticationSchemes.Digest:
                            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() package:WDigest headerScheme:" + headerScheme);

                            // WDigest had some weird behavior.  This is what I have discovered:
                            // Local accounts don't work, only domain accounts.  The domain (i.e. REDMOND) is implied.  Not sure how it is chosen.
                            // If the domain is specified and the credentials are correct, it works.  If they're not (domain, username or password):
                            //      AcceptSecurityContext (GetOutgoingDigestBlob) returns success but with a bogus 4k challenge, and
                            //      QuerySecurityContextToken (GetContextToken) fails with NoImpersonation.
                            // If the domain isn't specified, AcceptSecurityContext returns NoAuthenticatingAuthority for a bad username,
                            // and LogonDenied for a bad password.

                            // Also interesting is that WDigest requires us to keep a reference to the previous context, but fails if we
                            // actually pass it in!  (It't ok to pass it in for the first request, but not if nc > 1.)  For Whidbey,
                            // we create a new context and associate it with the connection, just like NTLM, but instead of using it for
                            // the next request on the connection, we always create a new context and swap the old one out.  As long
                            // as we keep the old one around until after we authenticate with the new one, it works.  For this reason,
                            // we also keep these contexts around past the lifetime of the connection, so that KeepAlive=false works.
                            binding = GetChannelBinding(connectionId, isSecureConnection, extendedProtectionPolicy);

                            context = new NTAuthentication(true, NegotiationInfoClass.WDigest, null,
                                GetContextFlags(extendedProtectionPolicy, isSecureConnection), binding);

                            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() verb:" + verb + " context.IsValidContext:" + context.IsValidContext.ToString());

                            outBlob = context.GetOutgoingDigestBlob(inBlob, verb, null, Realm, false, false, out statusCodeNew);
                            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() GetOutgoingDigestBlob() returned IsCompleted:" + context.IsCompleted + " statusCodeNew:" + statusCodeNew + " outBlob:[" + outBlob + "]");

                            // WDigest bug: sometimes when AcceptSecurityContext returns success, it provides a bogus, empty 4k buffer.
                            // Ignore it.  (Should find out what's going on here from WDigest people.)
                            if (statusCodeNew == SecurityStatus.OK)
                            {
                                outBlob = null;
                            }

                            if (context.IsValidContext)
                            {
                                SafeCloseHandle userContext = null;
                                try
                                {
                                    if (!CheckSpn(context, isSecureConnection, extendedProtectionPolicy)) {
                                        httpError = HttpStatusCode.Unauthorized;
                                    }
                                    else {
                                        httpContext.Request.ServiceName = context.ClientSpecifiedSpn;

                                        userContext = context.GetContextToken(out statusCodeNew);
                                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() GetContextToken() returned:" + statusCodeNew.ToString());
                                        if (statusCodeNew != SecurityStatus.OK)
                                        {
                                             httpError = HttpStatusFromSecurityStatus(statusCodeNew);
                                        }
                                        else if (userContext == null)
                                        {
                                            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() error: GetContextToken() returned:null statusCodeNew:" + statusCodeNew.ToString());
                                            httpError = HttpStatusCode.Unauthorized;
                                        }
                                        else
                                        {
                                            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() creating new WindowsIdentity() from userContext:" + userContext.DangerousGetHandle().ToString("x8"));
                                            principal = new WindowsPrincipal(CreateWindowsIdentity(userContext.DangerousGetHandle(), DigestClient.AuthType, WindowsAccountType.Normal, true));
                                        }
                                    }
                                }
                                finally {
                                    if (userContext!=null) {
                                        userContext.Close();
                                    }
                                }

                                newContext = context;

                                if (outBlob != null)
                                {
                                    challenge = DigestClient.AuthType + " " + outBlob;
                                }
                            }
                            else
                            {
                                httpError = HttpStatusFromSecurityStatus(statusCodeNew);
                            }
                            break;

                        case AuthenticationSchemes.Negotiate:
                        case AuthenticationSchemes.Ntlm:
                            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() headerScheme:" + headerScheme);
                            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() returned context#" + ValidationHelper.HashString(oldContext) + " for connectionId:" + connectionId);

                            string package = headerScheme == AuthenticationSchemes.Ntlm ? NtlmClient.AuthType : NegotiateClient.AuthType;
                            if (oldContext != null && oldContext.Package == package)
                            {
                                context = oldContext;
                            }
                            else
                            {
                                binding = GetChannelBinding(connectionId, isSecureConnection, extendedProtectionPolicy);

                                context = new NTAuthentication(true, package, null,
                                    GetContextFlags(extendedProtectionPolicy, isSecureConnection), binding);
                            }

                            try
                            {
                                bytes = Convert.FromBase64String(inBlob);
                            }
                            catch (FormatException)
                            {
                                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() FromBase64String threw a FormatException.");
                                httpError = HttpStatusCode.BadRequest;
                                error = true;
                            }
                            if (!error)
                            {
                                decodedOutgoingBlob = context.GetOutgoingBlob(bytes, false, out statusCodeNew);
                                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() GetOutgoingBlob() returned IsCompleted:" + context.IsCompleted + " statusCodeNew:" + statusCodeNew);
                                error = !context.IsValidContext;
                                if (error)
                                {
                                    // Bug #474228: SSPI Workaround
                                    // If a client sends up a blob on the initial request, Negotiate returns SEC_E_INVALID_HANDLE
                                    // when it should return SEC_E_INVALID_TOKEN.
                                    if (statusCodeNew == SecurityStatus.InvalidHandle && oldContext == null && bytes != null && bytes.Length > 0)
                                    {
                                        statusCodeNew = SecurityStatus.InvalidToken;
                                    }

                                    httpError = HttpStatusFromSecurityStatus(statusCodeNew);
                                }
                            }

                            if (decodedOutgoingBlob!=null) {
                                outBlob = Convert.ToBase64String(decodedOutgoingBlob);
                            }

                            if (!error)
                            {
                                if (context.IsCompleted) {
                                    SafeCloseHandle userContext = null;
                                    try
                                    {
                                        if (!CheckSpn(context, isSecureConnection, extendedProtectionPolicy)) {
                                            httpError = HttpStatusCode.Unauthorized;
                                        }
                                        else {
                                            httpContext.Request.ServiceName = context.ClientSpecifiedSpn;

                                            userContext = context.GetContextToken(out statusCodeNew);
                                            if (statusCodeNew != SecurityStatus.OK)
                                            {
                                                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() GetContextToken() failed with statusCodeNew:" + statusCodeNew.ToString());
                                                httpError = HttpStatusFromSecurityStatus(statusCodeNew);
                                            }
                                            else
                                            {
                                                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() creating new WindowsIdentity() from userContext:" + userContext.DangerousGetHandle().ToString("x8"));
                                                WindowsPrincipal windowsPrincipal = new WindowsPrincipal(CreateWindowsIdentity(userContext.DangerousGetHandle(), context.ProtocolName, WindowsAccountType.Normal, true));
                                                principal = windowsPrincipal;

                                                // if appropriate, cache this credential on this connection
                                                if (UnsafeConnectionNtlmAuthentication && context.ProtocolName == NegotiationInfoClass.NTLM)
                                                {
                                                    GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() inserting principal#" + ValidationHelper.HashString(principal) + " for connectionId:" + connectionId);

                                                    // We may need to call WaitForDisconnect.
                                                    if (disconnectResult == null)
                                                    {
                                                        RegisterForDisconnectNotification(connectionId, ref disconnectResult);
                                                    }
                                                    if (disconnectResult != null)
                                                    {
                                                        lock (DisconnectResults.SyncRoot)
                                                        {
                                                            if (UnsafeConnectionNtlmAuthentication)
                                                            {
                                                                disconnectResult.AuthenticatedConnection = windowsPrincipal;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Registration failed - UnsafeConnectionNtlmAuthentication ignored.
                                                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() RegisterForDisconnectNotification() failed.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    finally {
                                        if (userContext!=null) {
                                            userContext.Close();
                                        }
                                    }
                                }
                                else
                                {
                                    // auth incomplete
                                    newContext = context;

                                    challenge = (headerScheme==AuthenticationSchemes.Ntlm ? NtlmClient.AuthType : NegotiateClient.AuthType);
                                    if (!String.IsNullOrEmpty(outBlob))
                                    {
                                        challenge += " " + outBlob;
                                    }
                                }
                            }
                            break;

                        case AuthenticationSchemes.Basic:
                            try
                            {
                                bytes = Convert.FromBase64String(inBlob);

                                inBlob = WebHeaderCollection.HeaderEncoding.GetString(bytes, 0, bytes.Length);
                                index = inBlob.IndexOf(':');

                                if (index!=-1) {
                                    string userName = inBlob.Substring(0, index);
                                    string password = inBlob.Substring(index+1);

                                    GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() basic identity found, userName:" + userName);
                                    principal = new GenericPrincipal(new HttpListenerBasicIdentity(userName, password), null);
                                }
                                else
                                {
                                    httpError = HttpStatusCode.BadRequest;
                                }
                            }
                            catch (FormatException)
                            {
                                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() FromBase64String threw a FormatException.");
                            }
                            break;
                    }

                    if (principal != null)
                    {
                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() got principal:" + ValidationHelper.ToString(principal) + " principal.Identity.Name:" + ValidationHelper.ToString(principal.Identity.Name) + " creating request");
                        httpContext.SetIdentity(principal, outBlob);
                    }
                    else
                    {
                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() handshake has failed");
                        if(Logging.On)Logging.PrintWarning(Logging.HttpListener, this, "HandleAuthentication", SR.GetString(SR.net_log_listener_create_valid_identity_failed));
                        httpContext.Request.DetachBlob(memoryBlob);
                        httpContext.Close();
                        httpContext = null;
                    }
                }

                // if we're not giving a request to the application, we need to send an error
                ArrayList challenges = null;
                if (httpContext == null)
                {
                    // If we already have a challenge, just use it.  Otherwise put a challenge for each acceptable scheme.
                    if (challenge != null)
                    {
                        AddChallenge(ref challenges, challenge);
                    }
                    else
                    {
                        // We're starting over.  Any context SSPI might have wanted us to keep is useless.
                        if (newContext != null)
                        {
                            if (newContext == context)
                            {
                                context = null;
                            }

                            if (newContext != oldContext)
                            {
                                NTAuthentication toClose = newContext;
                                newContext = null;
                                toClose.CloseContext();
                            }
                            else
                            {
                                newContext = null;
                            }
                        }

                        // If we're sending something besides 401, do it here.
                        if (httpError != HttpStatusCode.Unauthorized)
                        {
                            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() failed context#" + ValidationHelper.HashString(context) + " for connectionId:" + connectionId + " because of error:" + httpError.ToString());
                            SendError(requestId, httpError, null);
                            return null;
                        }

                        challenges = BuildChallenge(authenticationScheme, connectionId, out newContext,
                            extendedProtectionPolicy, isSecureConnection);
                    }
                }

                // Check if we need to call WaitForDisconnect, because if we do and it fails, we want to send a 500 instead.
                if (disconnectResult == null && newContext != null)
                {
                    RegisterForDisconnectNotification(connectionId, ref disconnectResult);

                    // Failed - send 500.
                    if (disconnectResult == null)
                    {
                        if (newContext != null)
                        {
                            if (newContext == context)
                            {
                                context = null;
                            }

                            if (newContext != oldContext)
                            {
                                NTAuthentication toClose = newContext;
                                newContext = null;
                                toClose.CloseContext();
                            }
                            else
                            {
                                newContext = null;
                            }
                        }

                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() failed context#" + ValidationHelper.HashString(context) + " for connectionId:" + connectionId + " because of failed HttpWaitForDisconnect");
                        SendError(requestId, HttpStatusCode.InternalServerError, null);
                        httpContext.Request.DetachBlob(memoryBlob);
                        httpContext.Close();
                        return null;
                    }
                }

                // Update Session if necessary.
                if (oldContext != newContext)
                {
                    if (oldContext == context)
                    {
                        // Prevent the finally from closing this twice.
                        context = null;
                    }

                    NTAuthentication toClose = oldContext;
                    oldContext = newContext;
                    disconnectResult.Session = newContext;

                    if (toClose != null)
                    {
                        // Save digest context in digest cache, we may need it later because of
                        // subsequest responses to the same req on the same/diff connection
                        if ((authenticationScheme & AuthenticationSchemes.Digest) != 0)
                        {
                            SaveDigestContext(toClose);
                        }
                        else
                        {
                            toClose.CloseContext();
                        }
                    }
                }

                // Send the 401 here.
                if (httpContext == null)
                {
                    SendError(requestId, challenges != null && challenges.Count > 0 ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden, challenges);
                    GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() SendUnauthorized(Scheme:" + authenticationScheme + ")");
                    return null;
                }

                if (!stoleBlob)
                {
                    stoleBlob = true;
                    httpContext.Request.ReleasePins();
                }
                return httpContext;
            }
            catch
            {
                if (httpContext != null)
                {
                    httpContext.Request.DetachBlob(memoryBlob);
                    httpContext.Close();
                }
                if (newContext != null)
                {
                    if (newContext == context)
                    {
                        // Prevent the finally from closing this twice.
                        context = null;
                    }

                    if (newContext != oldContext)
                    {
                        NTAuthentication toClose = newContext;
                        newContext = null;
                        toClose.CloseContext();
                    }
                    else
                    {
                        newContext = null;
                    }
                }
                throw;
            }
            finally
            {
                try
                {
                    // Clean up the previous context if necessary.
                    if (oldContext != null && oldContext != newContext)
                    {
                        // Clear out Session if it wasn't already.
                        if (newContext == null && disconnectResult != null)
                        {
                            disconnectResult.Session = null;
                        }

                        // Save digest context in digest cache, we may need it later because of
                        // subsequest responses to the same req on the same/diff connection

                        if ((authenticationScheme & AuthenticationSchemes.Digest) != 0)
                        {
                            SaveDigestContext(oldContext);
                        }
                        else
                        {
                            oldContext.CloseContext();
                        }
                    }

                    // Delete any context created but not stored.
                    if (context != null && oldContext != context && newContext != context)
                    {
                        context.CloseContext();
                    }
                }
                finally
                {
                    // Check if the connection got deleted while in this method, and clear out the hashtables if it did.
                    // In a nested finally because if this doesn't happen, we leak.
                    if (disconnectResult != null)
                    {
                        disconnectResult.FinishOwningDisconnectHandling();
                    }
                }
            }
        }

        // Using the configured Auth schemes, populate the auth challenge headers. This is for scenarios where 
        // Anonymous access is allowed for some resources, but the server later determines that authorization 
        // is required for this request.
        internal void SetAuthenticationHeaders(HttpListenerContext context)
        {
            Debug.Assert(context != null, "Null Context");

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // We use the cached results from the delegates so that we don't have to call them again here.
            NTAuthentication newContext;
            ArrayList challenges = BuildChallenge(context.AuthenticationSchemes, request.m_ConnectionId, 
                out newContext, context.ExtendedProtectionPolicy, request.IsSecureConnection);

            // Setting 401 without setting WWW-Authenticate is a protocol violation
            // but throwing from HttpListener would be a breaking change.
            if (challenges != null) // null == Anonymous
            {
                if (newContext != null) // Digest challenge, keep it alive for 10s - 5min.
                {
                    SaveDigestContext(newContext);
                }

                // Add the new WWW-Authenticate headers
                foreach (String challenge in challenges)
                {
                    response.Headers.AddInternal(HttpKnownHeaderNames.WWWAuthenticate, challenge);
                }
            }
        }

        private static bool ScenarioChecksChannelBinding(bool isSecureConnection, ProtectionScenario scenario)
        {
            return (isSecureConnection && scenario == ProtectionScenario.TransportSelected);
        }

        private ChannelBinding GetChannelBinding(ulong connectionId, bool isSecureConnection, ExtendedProtectionPolicy policy)
        {
            if (policy.PolicyEnforcement == PolicyEnforcement.Never)
            {
                if (Logging.On) Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_no_cbt_disabled));
                return null;
            }

            if (!isSecureConnection)
            {
                if (Logging.On) Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_no_cbt_http));
                return null;
            }

            if (!AuthenticationManager.OSSupportsExtendedProtection)
            {
                GlobalLog.Assert(policy.PolicyEnforcement != PolicyEnforcement.Always, "User managed to set PolicyEnforcement.Always when the OS does not support extended protection!");
                if (Logging.On) Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_no_cbt_platform));
                return null;
            }

            if (policy.ProtectionScenario == ProtectionScenario.TrustedProxy)
            {
                if (Logging.On) Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_no_cbt_trustedproxy));
                return null;
            }

            ChannelBinding result = GetChannelBindingFromTls(connectionId);

            GlobalLog.Assert(result != null, "GetChannelBindingFromTls returned null even though OS supposedly supports Extended Protection");
            if (Logging.On) Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_cbt));
            return result;
        }

        private bool CheckSpn(NTAuthentication context, bool isSecureConnection, ExtendedProtectionPolicy policy)
        {
            // Kerberos does SPN check already in ASC
            if (context.IsKerberos)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_no_spn_kerberos));
                }
                return true;
            }

            // Don't check the SPN if Extended Protection is off or we already checked the CBT
            if (policy.PolicyEnforcement == PolicyEnforcement.Never)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_no_spn_disabled));
                }
                return true;
            }

            if (ScenarioChecksChannelBinding(isSecureConnection, policy.ProtectionScenario))
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_no_spn_cbt));
                }
                return true;
            }

            if (!AuthenticationManager.OSSupportsExtendedProtection)
            {
                GlobalLog.Assert(policy.PolicyEnforcement != PolicyEnforcement.Always, "User managed to set PolicyEnforcement.Always when the OS does not support extended protection!");
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_no_spn_platform));
                }
                return true;
            }

            string clientSpn = context.ClientSpecifiedSpn;

            // An empty SPN is only allowed in the WhenSupported case
            if (String.IsNullOrEmpty(clientSpn))
            {
                if (policy.PolicyEnforcement == PolicyEnforcement.WhenSupported)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.HttpListener, this, 
                            SR.GetString(SR.net_log_listener_no_spn_whensupported));
                    }
                    return true;
                }
                else
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.HttpListener, this, 
                            SR.GetString(SR.net_log_listener_spn_failed_always));
                    }
                    return false;
                }
            }
            else if (ServiceNameCollection.Match(clientSpn, "http/localhost"))
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_no_spn_loopback));
                }

                return true;
            }
            else
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_spn, clientSpn));
                }

                ServiceNameCollection serviceNames = GetServiceNames(policy);

                bool found = serviceNames.Contains(clientSpn);

                if (Logging.On)
                {
                    if (found)
                    {
                        Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_spn_passed));
                    }
                    else
                    {
                        Logging.PrintInfo(Logging.HttpListener, this, SR.GetString(SR.net_log_listener_spn_failed));

                        if (serviceNames.Count == 0)
                        {
                            Logging.PrintWarning(Logging.HttpListener, this, "CheckSpn",
                                SR.GetString(SR.net_log_listener_spn_failed_empty));
                        }
                        else
                        {
                            Logging.PrintInfo(Logging.HttpListener, this, 
                                SR.GetString(SR.net_log_listener_spn_failed_dump));

                            foreach (string serviceName in serviceNames)
                            {
                                Logging.PrintInfo(Logging.HttpListener, this, "\t" + serviceName);
                            }
                        }
                    }
                }

                return found;
            }
        }

        private ServiceNameCollection GetServiceNames(ExtendedProtectionPolicy policy)
        {
            ServiceNameCollection serviceNames;

            if (policy.CustomServiceNames == null) {
                
                if (m_DefaultServiceNames.ServiceNames.Count == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.net_listener_no_spns));
                }
                serviceNames = m_DefaultServiceNames.ServiceNames;
            }
            else {
                serviceNames = policy.CustomServiceNames;
            }
            return serviceNames;
        }

        private ContextFlags GetContextFlags(ExtendedProtectionPolicy policy, bool isSecureConnection)
        {
            ContextFlags result = ContextFlags.Connection;

            if (policy.PolicyEnforcement != PolicyEnforcement.Never) {

                if (policy.PolicyEnforcement == PolicyEnforcement.WhenSupported) {
                    result |= ContextFlags.AllowMissingBindings;
                }

                if (policy.ProtectionScenario == ProtectionScenario.TrustedProxy) {
                    result |= ContextFlags.ProxyBindings;
                }
            }

            return result;
        }

        private static void AddChallenge(ref ArrayList challenges, string challenge)
        {
            if (challenge!=null) {
                challenge = challenge.Trim();
                if (challenge.Length>0) {
                    GlobalLog.Print("HttpListener:AddChallenge() challenge:" + challenge);
                    if (challenges == null)
                    {
                        challenges = new ArrayList(4);
                    }
                    challenges.Add(challenge);
                }
            }
        }

        private ArrayList BuildChallenge(AuthenticationSchemes authenticationScheme, ulong connectionId, 
            out NTAuthentication newContext, ExtendedProtectionPolicy policy, bool isSecureConnection)
        {
            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::BuildChallenge()  authenticationScheme:" + authenticationScheme.ToString());
            ArrayList challenges = null;
            newContext = null;

            if ((authenticationScheme & AuthenticationSchemes.Negotiate) != 0)
            {
                AddChallenge(ref challenges, NegotiateClient.AuthType);
            }

            if ((authenticationScheme & AuthenticationSchemes.Ntlm) != 0)
            {
                AddChallenge(ref challenges, NtlmClient.AuthType);
            }

            if ((authenticationScheme & AuthenticationSchemes.Digest) != 0)
            {
                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::BuildChallenge() package:WDigest");

                NTAuthentication context = null;
                try
                {
                    string outBlob = null;
                    ChannelBinding binding = GetChannelBinding(connectionId, isSecureConnection, policy);

                    context = new NTAuthentication(true, NegotiationInfoClass.WDigest, null,
                        GetContextFlags(policy, isSecureConnection), binding);

                    SecurityStatus statusCode;
                    outBlob = context.GetOutgoingDigestBlob(null, null, null, Realm, false, false, out statusCode);
                    GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::BuildChallenge() GetOutgoingDigestBlob() returned IsCompleted:" + context.IsCompleted + " statusCode:" + statusCode + " outBlob:[" + outBlob + "]");

                    if (context.IsValidContext)
                    {
                        newContext = context;
                    }

                    AddChallenge(ref challenges, DigestClient.AuthType + (string.IsNullOrEmpty(outBlob) ? "" : " " + outBlob));
                }
                finally
                {
                    if (context != null && newContext != context)
                    {
                        context.CloseContext();
                    }
                }
            }

            if ((authenticationScheme & AuthenticationSchemes.Basic) != 0)
            {
                AddChallenge(ref challenges, BasicClient.AuthType + " realm=\"" + Realm + "\"");
            }

            return challenges;
        }

        private void RegisterForDisconnectNotification(ulong connectionId, ref DisconnectAsyncResult disconnectResult)
        {
            GlobalLog.Assert(disconnectResult == null, "HttpListener#{0}::RegisterForDisconnectNotification()|Called with a disconnectResult.", ValidationHelper.HashString(this));

            try
            {
                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::RegisterForDisconnectNotification() calling UnsafeNclNativeMethods.HttpApi.HttpWaitForDisconnect");

                DisconnectAsyncResult result = new DisconnectAsyncResult(this, connectionId);

                EnsureBoundHandle();
                uint statusCode = UnsafeNclNativeMethods.HttpApi.HttpWaitForDisconnect(
                    m_RequestQueueHandle,
                    connectionId,
                    result.NativeOverlapped);

                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::RegisterForDisconnectNotification() call to UnsafeNclNativeMethods.HttpApi.HttpWaitForDisconnect returned:" + statusCode);

                if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS ||
                    statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING)
                {
                    // Need to make sure it's going to get returned before adding it to the hash.  That way it'll be handled
                    // correctly in HandleAuthentication's finally.
                    disconnectResult = result;
                    DisconnectResults[connectionId] = disconnectResult;
                }

                if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS && HttpListener.SkipIOCPCallbackOnSuccess)
                {
                    // IO operation completed synchronously - callback won't be called to signal completion.
                    result.IOCompleted(statusCode, 0, result.NativeOverlapped);
                }
            }
            catch (Win32Exception exception)
            {
                uint statusCode = (uint) exception.NativeErrorCode;
                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::RegisterForDisconnectNotification() call to UnsafeNclNativeMethods.HttpApi.HttpWaitForDisconnect threw.  statusCode:" + statusCode);
            }
        }

        private void SendError(ulong requestId, HttpStatusCode httpStatusCode, ArrayList challenges)
        {
            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::SendInternalError() requestId:" + ValidationHelper.ToString(requestId));
            UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE httpResponse = new UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE();
            httpResponse.Version = new UnsafeNclNativeMethods.HttpApi.HTTP_VERSION();
            httpResponse.Version.MajorVersion = (ushort)1;
            httpResponse.Version.MinorVersion = (ushort)1;
            httpResponse.StatusCode = (ushort)httpStatusCode;
            string statusDescription = HttpStatusDescription.Get(httpStatusCode);
            uint DataWritten = 0;
            uint statusCode;
            byte[] byteReason = Encoding.Default.GetBytes(statusDescription);
            fixed (byte* pReason = byteReason)
            {
                httpResponse.pReason = (sbyte*)pReason;
                httpResponse.ReasonLength = (ushort)byteReason.Length;

                byte[] byteContentLength = Encoding.Default.GetBytes("0");
                fixed (byte* pContentLength = byteContentLength)
                {
                    (&httpResponse.Headers.KnownHeaders)[(int)HttpResponseHeader.ContentLength].pRawValue = (sbyte*)pContentLength;
                    (&httpResponse.Headers.KnownHeaders)[(int)HttpResponseHeader.ContentLength].RawValueLength = (ushort)byteContentLength.Length;

                    httpResponse.Headers.UnknownHeaderCount = checked((ushort) (challenges == null ? 0 : challenges.Count));
                    GCHandle[] challengeHandles = null;
                    UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER[] headersArray = null;
                    GCHandle headersArrayHandle = new GCHandle();
                    GCHandle wwwAuthenticateHandle = new GCHandle();
                    if (httpResponse.Headers.UnknownHeaderCount > 0)
                    {
                        challengeHandles = new GCHandle[httpResponse.Headers.UnknownHeaderCount];
                        headersArray = new UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER[httpResponse.Headers.UnknownHeaderCount];
                    }

                    try
                    {
                        if (httpResponse.Headers.UnknownHeaderCount > 0)
                        {
                            headersArrayHandle = GCHandle.Alloc(headersArray, GCHandleType.Pinned);
                            httpResponse.Headers.pUnknownHeaders = (UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER*) Marshal.UnsafeAddrOfPinnedArrayElement(headersArray, 0);
                            wwwAuthenticateHandle = GCHandle.Alloc(s_WwwAuthenticateBytes, GCHandleType.Pinned);
                            sbyte* wwwAuthenticate = (sbyte*) Marshal.UnsafeAddrOfPinnedArrayElement(s_WwwAuthenticateBytes, 0);

                            for (int i = 0; i < challengeHandles.Length; i++)
                            {
                                byte[] byteChallenge = Encoding.Default.GetBytes((string) challenges[i]);
                                challengeHandles[i] = GCHandle.Alloc(byteChallenge, GCHandleType.Pinned);
                                headersArray[i].pName = wwwAuthenticate;
                                headersArray[i].NameLength = (ushort) s_WwwAuthenticateBytes.Length;
                                headersArray[i].pRawValue = (sbyte*) Marshal.UnsafeAddrOfPinnedArrayElement(byteChallenge, 0);
                                headersArray[i].RawValueLength = checked((ushort) byteChallenge.Length);
                            }
                        }

                        GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::SendInternalError() calling UnsafeNclNativeMethods.HttpApi.HttpSendHtthttpResponse");
                        statusCode =
                            UnsafeNclNativeMethods.HttpApi.HttpSendHttpResponse(
                                m_RequestQueueHandle,
                                requestId,
                                0,
                                &httpResponse,
                                null,
                                &DataWritten,
                                SafeLocalFree.Zero,
                                0,
                                null,
                                null );
                    }
                    finally
                    {
                        if (headersArrayHandle.IsAllocated)
                        {
                            headersArrayHandle.Free();
                        }
                        if (wwwAuthenticateHandle.IsAllocated)
                        {
                            wwwAuthenticateHandle.Free();
                        }
                        if (challengeHandles != null)
                        {
                            for (int i = 0; i < challengeHandles.Length; i++)
                            {
                                if (challengeHandles[i].IsAllocated)
                                {
                                    challengeHandles[i].Free();
                                }
                            }
                        }
                    }
                }
            }
            GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::SendInternalError() call to UnsafeNclNativeMethods.HttpApi.HttpSendHttpResponse returned:" + statusCode);
            if (statusCode!=UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                // if we fail to send a 401 something's seriously wrong, abort the request
                GlobalLog.Print("HttpListener#" + ValidationHelper.HashString(this) + "::HandleAuthentication() SendUnauthorized() returned:" + statusCode);
                HttpListenerContext.CancelRequest(m_RequestQueueHandle, requestId);
            }
        }

        private unsafe static int GetTokenOffsetFromBlob(IntPtr blob)
        {
            Debug.Assert(blob != IntPtr.Zero);
            IntPtr tokenPointer = Marshal.ReadIntPtr((IntPtr)blob, (int)Marshal.OffsetOf(ChannelBindingStatusType, "ChannelToken"));

            Debug.Assert(tokenPointer != IntPtr.Zero);
            return (int)IntPtrHelper.Subtract(tokenPointer, blob);
        }

        private unsafe static int GetTokenSizeFromBlob(IntPtr blob)
        {
            Debug.Assert(blob != IntPtr.Zero);
            return Marshal.ReadInt32(blob, (int)Marshal.OffsetOf(ChannelBindingStatusType, "ChannelTokenSize"));
        }

        internal ChannelBinding GetChannelBindingFromTls(ulong connectionId)
        {
            if (Logging.On) {
                Logging.Enter(Logging.HttpListener, "HttpListener#" + ValidationHelper.HashString(this) +
                    "::GetChannelBindingFromTls() connectionId: " + connectionId.ToString());
            }

            // +128 since a CBT is usually <128 thus we need to call HRCC just once. If the CBT
            // is >128 we will get ERROR_MORE_DATA and call again
            int size = RequestChannelBindStatusSize + 128;

            Debug.Assert(size >= 0);

            byte[] blob = null;
            SafeLocalFreeChannelBinding token = null;

            uint bytesReceived = 0;
            uint statusCode;

            do {
                blob = new byte[size];
                fixed (byte* blobPtr = blob)
                {
                    // Http.sys team: ServiceName will always be null if 
                    // HTTP_RECEIVE_SECURE_CHANNEL_TOKEN flag is set.
                    statusCode = UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate(
                        RequestQueueHandle,
                        connectionId,
                        (uint)UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_RECEIVE_SECURE_CHANNEL_TOKEN,
                        blobPtr,
                        (uint)size,
                        &bytesReceived,
                        null);

                    if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS)
                    {
                        int tokenOffset = GetTokenOffsetFromBlob((IntPtr)blobPtr);
                        int tokenSize = GetTokenSizeFromBlob((IntPtr)blobPtr);
                        Debug.Assert(tokenSize < Int32.MaxValue);

                        token = SafeLocalFreeChannelBinding.LocalAlloc(tokenSize);
                        if (token.IsInvalid)
                        {
                            throw new OutOfMemoryException();
                        }
                        Marshal.Copy(blob, tokenOffset, token.DangerousGetHandle(), tokenSize);
                    }
                    else if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA)
                    {
                        int tokenSize = GetTokenSizeFromBlob((IntPtr)blobPtr);
                        Debug.Assert(tokenSize < Int32.MaxValue);

                        size = RequestChannelBindStatusSize + tokenSize;
                    }
                    else if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_INVALID_PARAMETER)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.HttpListener, "HttpListener#" +
                                ValidationHelper.HashString(this) + "::GetChannelBindingFromTls() " +
                                SR.GetString(SR.net_ssp_dont_support_cbt));
                        }
                        return null; // old schannel library which doesn't support CBT
                    }
                    else
                    {
                        throw new HttpListenerException((int)statusCode);
                    }
                }
            } while (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS);

            return token;
        }

        internal void CheckDisposed() {
            if (m_State==State.Closed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        // This only works for context-destroying errors.
        private HttpStatusCode HttpStatusFromSecurityStatus(SecurityStatus status)
        {
            if (NclUtilities.IsCredentialFailure(status))
            {
                return HttpStatusCode.Unauthorized;
            }
            if (NclUtilities.IsClientFault(status))
            {
                return HttpStatusCode.BadRequest;
            }
            return HttpStatusCode.InternalServerError;
        }

        enum State {
            Stopped,
            Started,
            Closed,
        }

        private const int DigestLifetimeSeconds = 300;
        private const int MaximumDigests = 1024;  // Must be a power of two.
        private const int MinimumDigestLifetimeSeconds = 10;

        private struct DigestContext
        {
            internal NTAuthentication context;
            internal int timestamp;
        }

        private DigestContext[] m_SavedDigests;
        private ArrayList m_ExtraSavedDigests;
        private ArrayList m_ExtraSavedDigestsBaking;
        private int m_ExtraSavedDigestsTimestamp;
        private int m_NewestContext;
        private int m_OldestContext;

        private void SaveDigestContext(NTAuthentication digestContext)
        {
            if (m_SavedDigests == null)
            {
                Interlocked.CompareExchange<DigestContext[]>(ref m_SavedDigests, new DigestContext[MaximumDigests], null);
            }

            // We want to actually close the contexts outside the lock.
            NTAuthentication oldContext = null;
            ArrayList digestsToClose = null;
            lock (m_SavedDigests)
            {
                // If we're stopped, just throw it away.
                if (!IsListening)
                {
                    digestContext.CloseContext();
                    return;
                }

                int now = ((now = Environment.TickCount) == 0 ? 1 : now);

                m_NewestContext = (m_NewestContext + 1) & (MaximumDigests - 1);

                int oldTimestamp = m_SavedDigests[m_NewestContext].timestamp;
                oldContext = m_SavedDigests[m_NewestContext].context;
                m_SavedDigests[m_NewestContext].timestamp = now;
                m_SavedDigests[m_NewestContext].context = digestContext;

                // May need to move this up.
                if (m_OldestContext == m_NewestContext)
                {
                    m_OldestContext = (m_NewestContext + 1) & (MaximumDigests - 1);
                }

                // Delete additional contexts older than five minutes.
                while (unchecked(now - m_SavedDigests[m_OldestContext].timestamp) >= DigestLifetimeSeconds && m_SavedDigests[m_OldestContext].context != null)
                {
                    if (digestsToClose == null)
                    {
                        digestsToClose = new ArrayList();
                    }
                    digestsToClose.Add(m_SavedDigests[m_OldestContext].context);
                    m_SavedDigests[m_OldestContext].context = null;
                    m_OldestContext = (m_OldestContext + 1) & (MaximumDigests - 1);
                }

                // If the old context is younger than 10 seconds, put it in the backup pile.
                if (oldContext != null && unchecked(now - oldTimestamp) <= MinimumDigestLifetimeSeconds * 1000)
                {
                    // Use a two-tier ArrayList system to guarantee each entry lives at least 10 seconds.
                    if (m_ExtraSavedDigests == null ||
                        unchecked(now - m_ExtraSavedDigestsTimestamp) > MinimumDigestLifetimeSeconds * 1000)
                    {
                        digestsToClose = m_ExtraSavedDigestsBaking;
                        m_ExtraSavedDigestsBaking = m_ExtraSavedDigests;
                        m_ExtraSavedDigestsTimestamp = now;
                        m_ExtraSavedDigests = new ArrayList();
                    }
                    m_ExtraSavedDigests.Add(oldContext);
                    oldContext = null;
                }
            }

            if (oldContext != null)
            {
                oldContext.CloseContext();
            }
            if (digestsToClose != null)
            {
                for (int i = 0; i < digestsToClose.Count; i++)
                {
                    ((NTAuthentication)digestsToClose[i]).CloseContext();
                }
            }
        }

        private void ClearDigestCache()
        {
            if (m_SavedDigests == null)
            {
                return;
            }

            ArrayList[] toClose = new ArrayList[3];
            lock (m_SavedDigests)
            {
                toClose[0] = m_ExtraSavedDigestsBaking;
                m_ExtraSavedDigestsBaking = null;
                toClose[1] = m_ExtraSavedDigests;
                m_ExtraSavedDigests = null;

                m_NewestContext = 0;
                m_OldestContext = 0;

                toClose[2] = new ArrayList();
                for (int i = 0; i < MaximumDigests; i++)
                {
                    if (m_SavedDigests[i].context != null)
                    {
                        toClose[2].Add(m_SavedDigests[i].context);
                        m_SavedDigests[i].context = null;
                    }
                    m_SavedDigests[i].timestamp = 0;
                }
            }

            for (int j = 0; j < toClose.Length; j++)
            {
                if (toClose[j] != null)
                {
                    for (int k = 0; k < toClose[j].Count; k++)
                    {
                        ((NTAuthentication) toClose[j][k]).CloseContext();
                    }
                }
            }
        }

        class DisconnectAsyncResult : IAsyncResult {
            private static readonly IOCompletionCallback s_IOCallback = new IOCompletionCallback(WaitCallback);

            private ulong m_ConnectionId;
            private HttpListener m_HttpListener;
            NativeOverlapped* m_NativeOverlapped;
            private int m_OwnershipState;   // 0 = normal, 1 = in HandleAuthentication(), 2 = disconnected, 3 = cleaned up

            private WindowsPrincipal m_AuthenticatedConnection;
            private NTAuthentication m_Session;

            internal const string NTLM = "NTLM";

            internal NativeOverlapped* NativeOverlapped{
                get{
                    return m_NativeOverlapped;
                }
            }

            public object AsyncState {
                get {
                    throw ExceptionHelper.PropertyNotImplementedException;
                }                                       
            }
            public WaitHandle AsyncWaitHandle {
                get {
                    throw ExceptionHelper.PropertyNotImplementedException;
                }
            }
            public bool CompletedSynchronously {
                get {
                    throw ExceptionHelper.PropertyNotImplementedException;
                }
            }
            public bool IsCompleted {
                get {
                    throw ExceptionHelper.PropertyNotImplementedException;
                }
            }

            internal unsafe DisconnectAsyncResult(HttpListener httpListener, ulong connectionId) {
                GlobalLog.Print("DisconnectAsyncResult#" + ValidationHelper.HashString(this) + "::.ctor() httpListener#" + ValidationHelper.HashString(httpListener) + " connectionId:" + connectionId);
                m_OwnershipState = 1;
                m_HttpListener = httpListener;
                m_ConnectionId = connectionId;
                Overlapped overlapped = new Overlapped();
                overlapped.AsyncResult = this;
                // we can call the Unsafe API here, we won't ever call user code
                m_NativeOverlapped = overlapped.UnsafePack(s_IOCallback, null);
                GlobalLog.Print("DisconnectAsyncResult#" + ValidationHelper.HashString(this) + "::.ctor() overlapped#" + ValidationHelper.HashString(overlapped) + " nativeOverlapped:" + ((IntPtr)m_NativeOverlapped).ToString("x"));
            }

            internal bool StartOwningDisconnectHandling()
            {
                int oldValue;

                while ((oldValue = Interlocked.CompareExchange(ref m_OwnershipState, 1, 0)) == 2)
                {
                    // Must block until it equals 3 - we must be in the callback right now.
                    Thread.SpinWait(1);
                }

                GlobalLog.Assert(oldValue != 1, "DisconnectAsyncResult#{0}::HandleDisconnect()|StartOwningDisconnectHandling() called twice.", ValidationHelper.HashString(this));
                return oldValue < 2;
            }

            internal void FinishOwningDisconnectHandling()
            {
                // If it got disconnected, run the disconnect code.
                if (Interlocked.CompareExchange(ref m_OwnershipState, 0, 1) == 2)
                {
                    HandleDisconnect();
                }
            }

            internal unsafe void IOCompleted(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
            {
                IOCompleted(this, errorCode, numBytes, nativeOverlapped);
            }

            private static unsafe void IOCompleted(DisconnectAsyncResult asyncResult, uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
            {
                GlobalLog.Print("DisconnectAsyncResult#" + ValidationHelper.HashString(asyncResult) + "::WaitCallback() m_ConnectionId:" + asyncResult.m_ConnectionId);
                Overlapped.Free(nativeOverlapped);
                if (Interlocked.Exchange(ref asyncResult.m_OwnershipState, 2) == 0)
                {
                    asyncResult.HandleDisconnect();
                }
            }

            private static unsafe void WaitCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped) {
                GlobalLog.Print("DisconnectAsyncResult::WaitCallback() errorCode:" + errorCode + " numBytes:" + numBytes + " nativeOverlapped:" + ((IntPtr)nativeOverlapped).ToString("x"));
                // take the DisconnectAsyncResult object from the state
                Overlapped callbackOverlapped = Overlapped.Unpack(nativeOverlapped);
                DisconnectAsyncResult asyncResult = (DisconnectAsyncResult) callbackOverlapped.AsyncResult;
                GlobalLog.Print("DisconnectAsyncResult#" + ValidationHelper.HashString(asyncResult) + "::WaitCallback() callbackOverlapped#" + ValidationHelper.HashString(callbackOverlapped) + " m_ConnectionId:" + asyncResult.m_ConnectionId);
                IOCompleted(asyncResult, errorCode, numBytes, nativeOverlapped);
            }

            private void HandleDisconnect()
            {
                GlobalLog.Print("DisconnectAsyncResult#" + ValidationHelper.HashString(this) + "::HandleDisconnect() DisconnectResults#" + ValidationHelper.HashString(m_HttpListener.DisconnectResults) + " removing for m_ConnectionId:" + m_ConnectionId);
                m_HttpListener.DisconnectResults.Remove(m_ConnectionId);
                if (m_Session != null)
                {
                    if (m_Session.Package == NegotiationInfoClass.WDigest)
                    {
                        // VSWhidbey #497767
                        // WDigest doesn't like having the context passed back in on the next request on a connection, but it does want
                        // the server to keep a reference to it for as long as a client might reuse the nonce.  The heuristic we use is,
                        // keep contexts for five minutes, up to a maximum of 1024, except also keep all contexts at least 10 seconds to avoid
                        // total DoS (where no handshakes can be completed in time).
                        m_HttpListener.SaveDigestContext(m_Session);
                    }
                    else
                    {
                        m_Session.CloseContext();
                    }
                }

                // Clean up the identity. This is for scenarios where identity was not cleaned up before due to
                // identity caching for unsafe ntlm authentication

                IDisposable identity = m_AuthenticatedConnection == null ? null : m_AuthenticatedConnection.Identity as IDisposable;
                if ((identity != null) &&
                    (m_AuthenticatedConnection.Identity.AuthenticationType == NTLM) &&
                    (m_HttpListener.UnsafeConnectionNtlmAuthentication))
                {
                    identity.Dispose();
                }

                int oldValue = Interlocked.Exchange(ref m_OwnershipState, 3);
                GlobalLog.Assert(oldValue == 2, "DisconnectAsyncResult#{0}::HandleDisconnect()|Expected OwnershipState of 2, saw {1}.", ValidationHelper.HashString(this), oldValue);
            }

            internal WindowsPrincipal AuthenticatedConnection
            {
                get
                {
                    return m_AuthenticatedConnection;
                }

                set
                {
                    // The previous value can't be disposed because it may be in use by the app.
                    m_AuthenticatedConnection = value;
                }
            }

            internal NTAuthentication Session
            {
                get
                {
                    return m_Session;
                }

                set
                {
                    m_Session = value;
                }
            }
        }
    }

/*  Proposed Future HTTP Base Classes
    see \ndp\mb\docs\specs\NetworkFramework\HTTPSYS\ASP.NET\stub.cs

    // System.Net exposes base abstract classes that System.Web will inherit from

    public abstract class BaseHttpContext {
        public virtual IPrincipal User { get; set; }

        // it doesn't make sense to make these virtual because we can't override
        // and change the returned type for System.Web or people would break.
        // the only thing we can do is to declare them normally and hide the
        // base implementation like "public new System.Web.HttpRequest Request"

        public BaseHttpRequest Request { get; }
        public BaseHttpResponse Response { get; }

        // these provide plumbing to make the above two methods callable with a BaseHttpContext reference

        protected virtual BaseHttpRequest GetRequest();
        protected virtual BaseHttpResponse GetResponse();
    }

    public abstract class BaseHttpRequest {
        public virtual string[] AcceptTypes { get; }
        public virtual Encoding ContentEncoding { get; set; }
        public virtual string ContentType { get; set; }
        public virtual NameValueCollection Headers { get; }
        public virtual string HttpMethod { get; }
        public virtual Stream InputStream { get; }
        public virtual bool IsAuthenticated { get; }
        public virtual bool IsLocal { get; }
        public virtual bool IsSecureConnection { get; }
        public virtual NameValueCollection QueryString { get; }
        public virtual string RawUrl { get; }
        public virtual Uri Url { get; }
        public virtual Uri UrlReferrer { get; }
        public virtual string UserAgent { get; }
        public virtual string UserHostAddress { get; }
        public virtual string UserHostName { get; }
        public virtual string[] UserLanguages { get; }

        // APIs that are in the base class but are new to ASP .NET

        public virtual long ContentLengthLong { get; }
        public virtual bool HasEntityBody { get; }
        public virtual bool KeepAlive { get; }
        public virtual IPEndPoint RemoteEndPoint { get; }
        public virtual IPEndPoint LocalEndPoint { get; }
    }

    public abstract class BaseHttpResponse {
        public virtual void AppendHeader(string name, string value);
        public virtual void Close();
        public virtual Encoding ContentEncoding { get; set; }
        public virtual string ContentType { get; set; }
        public virtual Stream OutputStream { get; }
        public virtual string RedirectLocation { get; set; }
        public virtual int StatusCode { get; set; }
        public virtual string StatusDescription { get; set; }

        // APIs that are in the base class but are new to ASP .NET

        public virtual bool KeepAlive { get; set; }
    }
*/
}

