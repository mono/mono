//------------------------------------------------------------------------------
// <copyright file="IISUnsafeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Configuration;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Diagnostics.CodeAnalysis;

    [
    System.Runtime.InteropServices.ComVisible(false),
    System.Security.SuppressUnmanagedCodeSecurityAttribute()
    ]
    // contains only method decls and data, so no instantiation
    internal unsafe static class UnsafeIISMethods {

        const string _IIS_NATIVE_DLL = ModName.MGDENG_FULL_NAME;
        static internal readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport(_IIS_NATIVE_DLL)]
        internal unsafe static extern int MgdGetRequestBasics(
            IntPtr           pRequestContext,
            out int          pContentType,
            out int          pContentTotalLength,
            out IntPtr       pPathTranslated,
            out int          pcchPathTranslated,
            out IntPtr       pCacheUrl,
            out int          pcchCacheUrl,
            out IntPtr       pHttpMethod,
            out IntPtr       pCookedUrl);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetHeaderChanges(
            IntPtr           pRequestContext,
            bool             fResponse,
            out IntPtr       knownHeaderSnapshot,
            out int          unknownHeaderSnapshotCount,
            out IntPtr       unknownHeaderSnapshotNames,
            out IntPtr       unknownHeaderSnapshotValues,
            out IntPtr       diffKnownIndicies,
            out int          diffUnknownCount,
            out IntPtr       diffUnknownIndicies);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetServerVarChanges(
            IntPtr           pRequestContext,
            out int          count,
            out IntPtr       names,
            out IntPtr       values,
            out int          diffCount,
            out IntPtr       diffIndicies);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetServerVariableW(
            IntPtr           pHandler,
            string           pszVarName,
            out IntPtr       ppBuffer,
            out int          pcchBufferSize);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetServerVariableA(
            IntPtr           pHandler,
            string           pszVarName,
            out IntPtr       ppBuffer,
            out int          pcchBufferSize);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern IntPtr MgdGetStopListeningEventHandle();

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdSetBadRequestStatus(
            IntPtr           pHandler);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdSetManagedHttpContext(
            IntPtr           pHandler,
            IntPtr           pManagedHttpContext);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdSetStatusW(
            IntPtr           pRequestContext,
            int              dwStatusCode,
            int              dwSubStatusCode,
            string           pszReason,
            string           pszErrorDescription /* optional, can be null */,
            bool             fTrySkipCustomErrors);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdSetKnownHeader(
            IntPtr           pRequestContext,
            bool             fRequest,
            bool             fReplace,
            ushort           uHeaderIndex,
            byte[]           value,
            ushort           valueSize);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdSetUnknownHeader(
            IntPtr           pRequestContext,
            bool             fRequest,
            bool             fReplace,
            byte []          header,
            byte []          value,
            ushort           valueSize);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdFlushCore(
            IntPtr    pRequestContext,
            bool      keepConnected,
            int       numBodyFragments,
            IntPtr[]  bodyFragments,
            int[]     bodyFragmentLengths,
            int[]     fragmentsNative);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdSetKernelCachePolicy(
            IntPtr    pHandler,
            int       secondsToLive);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdFlushKernelCache(
            string    cacheKey);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdDisableKernelCache(
            IntPtr    pHandler);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdDisableUserCache(
            IntPtr pHandler);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdRegisterEventSubscription(
            IntPtr           pAppContext,
            string           pszModuleName,
            [MarshalAs(UnmanagedType.U4)]
            RequestNotification requestNotifications,
            [MarshalAs(UnmanagedType.U4)]
            RequestNotification postRequestNotifications,
            string           pszModuleType,
            string           pszModulePrecondition,
            IntPtr           moduleSpecificData,
            bool             useHighPriority);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdIndicateCompletion(
            IntPtr           pHandler,
            [MarshalAs(UnmanagedType.U4)]
            ref RequestNotificationStatus notificationStatus );

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdInsertEntityBody(
            IntPtr           pHandler,
            byte[]           buffer,
            int              offset,
            int              count);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdPostCompletion(
            IntPtr           pHandler,
            [MarshalAs(UnmanagedType.U4)]
            RequestNotificationStatus notificationStatus );

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdReadEntityBody(
            IntPtr           pHandler,
            byte[]           pBuffer,
            int              dwOffset,
            int              dwBytesToRead,
            bool             fAsync,
            out int          pBytesRead,
            out IntPtr       ppAsyncReceiveBuffer );

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetCorrelationIdHeader(
            IntPtr          pHandler,
            out IntPtr      correlationId,
            out ushort      correlationIdLength,
            out bool        base64BinaryFormat);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetUserToken(
            IntPtr           pHandler,
            out IntPtr       pToken );

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetVirtualToken(
            IntPtr           pHandler,
            out IntPtr       pToken );

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern bool MgdIsClientConnected(
            IntPtr           pHandler);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern bool MgdIsHandlerExecutionDenied(
            IntPtr           pHandler);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's sole caller.")]
        internal static extern void MgdAbortConnection(
            IntPtr           pHandler);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern void MgdCloseConnection(
            IntPtr           pHandler);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetHandlerTypeString(
            IntPtr           pHandler,
            out IntPtr       ppszTypeString,
            out int          pcchTypeString);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetApplicationInfo(
            IntPtr           pHandler,
            out IntPtr       pVirtualPath,
            out int          cchVirtualPath,
            out IntPtr       pPhysPath,
            out int          cchPhysPath);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetUriPath(
            IntPtr           pHandler,
            out IntPtr       ppPath,
            out int          pcchPath,
            bool             fIncludePathInfo,
            bool             fUseParentContext);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetPreloadedContent(
            IntPtr           pHandler,
            byte[]           pBuffer,
            int              lOffset,
            int              cbLen,
            out int          pcbReceived);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetPreloadedSize(
            IntPtr           pHandler,
            out int          pcbAvailable);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetPrincipal(
            IntPtr           pHandler,
            int              dwRequestingAppDomainId,
            out IntPtr       pToken,
            out IntPtr       ppAuthType,
            ref int          pcchAuthType,
            out IntPtr       ppUserName,
            ref int          pcchUserName,
            out IntPtr       pManagedPrincipal);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdIsInRole(
            IntPtr           pHandler,
            string           pszRoleName,
            out bool         pfIsInRole);
            
        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern IntPtr MgdAllocateRequestMemory(
            IntPtr           pHandler,
            int              cbSize);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdAppDomainShutdown(
            IntPtr appContext );


        // Buffer pool methods
        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern IntPtr /* W3_MGD_BUFFER_POOL* */
            MgdGetBufferPool(int cbBufferSize);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern IntPtr /* PBYTE * */
            MgdGetBuffer(IntPtr pPool);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern IntPtr /* W3_MGD_BUFFER_POOL* */
            MgdReturnBuffer(IntPtr pBuffer);


        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int /* DWORD */
           MgdGetLocalPort(IntPtr context);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int /* DWORD */
           MgdGetRemotePort(IntPtr context);


        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetUserAgent(
            IntPtr           pRequestContext,
            out IntPtr       pBuffer,
            out int          cbBufferSize);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetCookieHeader(
            IntPtr           pRequestContext,
            out IntPtr       pBuffer,
            out int          cbBufferSize);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdRewriteUrl(
            IntPtr           pRequestContext,
            string           pszUrl,
            bool             fResetQueryString );

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetMaxConcurrentRequestsPerCPU();

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetMaxConcurrentThreadsPerCPU();

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdSetMaxConcurrentRequestsPerCPU(int value);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdSetMaxConcurrentThreadsPerCPU(int value);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetCurrentModuleName(
            IntPtr           pHandler,
            out IntPtr       pBuffer,
            out int          cbBufferSize);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetCurrentNotification(
            IntPtr           pRequestContext);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdDisableNotifications(
            IntPtr           pRequestContext,
            [MarshalAs(UnmanagedType.U4)]
            RequestNotification notifications,
            [MarshalAs(UnmanagedType.U4)]
            RequestNotification postNotifications);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdSuppressSendResponseNotifications(
            IntPtr pRequestContext);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetNextNotification(
            IntPtr           pRequestContext,
            [MarshalAs(UnmanagedType.U4)]
            RequestNotificationStatus dwStatus);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdClearResponse(
            IntPtr           pRequestContext,
            bool             fClearEntity,
            bool             fClearHeaders);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdCreateNativeConfigSystem(
            out IntPtr       ppConfigSystem);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdReleaseNativeConfigSystem(
            IntPtr           pConfigSystem);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetRequestTraceGuid(
            IntPtr           pRequestContext,
            out Guid         traceContextId);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetStatusChanges(
            IntPtr           pRequestContext,
            out ushort       statusCode,
            out ushort       subStatusCode,
            out IntPtr       pBuffer,
            out ushort       cbBufferSize);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetResponseChunks(
            IntPtr           pRequestContext,
            ref int          fragmentCount,
            IntPtr[]         bodyFragments,
            int[]            bodyFragmentLengths,
            int[]            fragmentChunkType);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdEtwGetTraceConfig(
            IntPtr           pRequestContext,
            out bool         providerEnabled,
            out int          flags,
            out int          level);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdEmitSimpleTrace(
            IntPtr           pRequestContext,
            int              type,
            string           eventData);

        [DllImport(_IIS_NATIVE_DLL, CharSet = CharSet.Unicode)]
        internal static extern int MgdEmitWebEventTrace(
            IntPtr           pRequestContext,
            int              webEventType,
            int              fieldCount,
            string[]         fieldNames,
            int[]            fieldTypes,
            string[]         fieldData);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdSetRequestPrincipal(
            IntPtr           pRequestContext,
            string           userName,
            string           authType,
            IntPtr           token);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern bool MgdCanDisposeManagedContext(
            IntPtr           pRequestContext,
            [MarshalAs(UnmanagedType.U4)]
            RequestNotificationStatus dwStatus);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern bool MgdIsLastNotification(
            IntPtr           pRequestContext,
            [MarshalAs(UnmanagedType.U4)]
            RequestNotificationStatus dwStatus);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern bool MgdIsWithinApp(
            IntPtr            pConfigSystem,
            string            siteName,
            string            appPath,
            string            virtualPath);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetSiteNameFromId(
            IntPtr            pConfigSystem,
            [MarshalAs(UnmanagedType.U4)]
            uint              siteId,
            out IntPtr        bstrSiteName,
            out int           cchSiteName);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetAppPathForPath(
            IntPtr            pConfigSystem,
            [MarshalAs(UnmanagedType.U4)]
            uint              siteId,
            string            virtualPath,
            out IntPtr        bstrPath,
            out int           cchPath);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetMemoryLimitKB(
            out long          limit);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetMimeMapCollection(
            IntPtr            pConfigSystem,
            IntPtr            appContext,
            out IntPtr        pMimeMapCollection,
            out int           count);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetModuleCollection(
            IntPtr            pConfigSystem,
            IntPtr            appContext,
            out IntPtr        pModuleCollection,
            out int           count);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetNextMimeMap(
            IntPtr            pMimeMapCollection,
            uint              dwIndex,
            out IntPtr        bstrFileExtension,
            out int           cchFileExtension,
            out IntPtr        bstrMimeType,
            out int           cchMimeType);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetNextModule(
            IntPtr            pModuleCollection,
            ref uint          dwIndex,
            out IntPtr        bstrModuleName,
            out int           cchModuleName,
            out IntPtr        bstrModuleType,
            out int           cchModuleType,
            out IntPtr        bstrModulePrecondition,
            out int           cchModulePrecondition);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetVrPathCreds(
            IntPtr            pConfigSystem,
            string            siteName,
            string            virtualPath,
            out IntPtr        bstrUserName,
            out int           cchUserName,
            out IntPtr        bstrPassword,
            out int           cchPassword);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetAppCollection(
            IntPtr            pConfigSystem,
            string            siteName,
            string            virtualPath,
            out IntPtr        bstrPath,
            out int           cchPath,
            out IntPtr        pAppCollection,
            out int           count);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetNextVPath(
            IntPtr            pAppCollection,
            uint              dwIndex,
            out IntPtr        bstrPath,
            out int           cchPath);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdInitNativeConfig();

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdTerminateNativeConfig();

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdMapPathDirect(
            IntPtr            pConfigSystem,
            string            siteName,
            string            virtualPath,
            out IntPtr        bstrPhysicalPath,
            out int           cchPath);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdMapHandler(
            IntPtr            pHandler,
            string            method,
            string            virtualPath,
            out IntPtr        ppszTypeString,
            out int           pcchTypeString,
            bool              convertNativeStaticFileModule,
            bool              ignoreWildcardMappings);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdReMapHandler(
            IntPtr            pHandler,
            string            pszVirtualPath,
            out IntPtr        ppszTypeString,
            out int           pcchTypeString,
            out bool          pfHandlerExists);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdSetRemapHandler(
            IntPtr            pHandler,
            string            pszName,
            string            ppszType);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdSetScriptMapForRemapHandler(
            IntPtr            pHandler);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdSetNativeConfiguration(
            IntPtr             nativeConfig);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        internal static extern uint MgdResolveSiteName(
            IntPtr           pConfigSystem,
            string           siteName);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdSetResponseFilter(
            IntPtr             context);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetFileChunkInfo(
            IntPtr             context,
            int                chunkOffset,
            out long           offset,
            out long           length);


        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdReadChunkHandle(
            IntPtr             context,
            IntPtr             FileHandle,
            long               startOffset,
            ref int            length,
            IntPtr             chunkEntity);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdExplicitFlush(
            IntPtr             context,
            bool               async,
            out bool           completedSynchronously);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdSetServerVariableW(
            IntPtr            context,
            string            variableName,
            string            variableValue);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdExecuteUrl(
            IntPtr            context,
            string            url,
            bool              resetQuerystring,
            bool              preserveForm,
            byte[]            entityBody,
            uint              entityBodySize,
            string            method,
            int               numHeaders,
            string[]          headersNames,
            string[]          headersValues,
            bool              preserveUser);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetClientCertificate(
            IntPtr            pHandler,
            out IntPtr        ppbClientCert,
            out int           pcbClientCert,
            out IntPtr        ppbClientCertIssuer,
            out int           pcbClientCertIssuer,
            out IntPtr        ppbClientCertPublicKey,
            out int           pcbClientCertPublicKey,
            out uint          pdwCertEncodingType,
            out long          ftNotBefore,
            out long          ftNotAfter);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern int MgdGetChannelBindingToken(
            IntPtr            pHandler,
            out IntPtr        ppbToken,
            out int           pcbTokenSize);

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdGetCurrentNotificationInfo(
            IntPtr            pHandler, 
            out int           currentModuleIndex,
            out bool          isPostNotification,
            out int           currentNotification);

        [DllImport(_IIS_NATIVE_DLL)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's sole caller.")]
        internal static extern int MgdAcceptWebSocket(
            IntPtr pHandler);

        [DllImport(_IIS_NATIVE_DLL)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's sole caller.")]
        internal static extern int MgdGetWebSocketContext(
            IntPtr pHandler,
            out IntPtr ppWebSocketContext);

        [DllImport(_IIS_NATIVE_DLL, CharSet=CharSet.Unicode)]
        internal static extern int MgdGetAnonymousUserToken(
            IntPtr            pHandler,
            out IntPtr        pToken );

        [DllImport(_IIS_NATIVE_DLL)]
        internal static extern void MgdGetIISVersionInformation(
            [Out] out uint pdwVersion,
            [Out] out bool pfIsIntegratedMode);

        [DllImport(_IIS_NATIVE_DLL)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's callers.")]
        internal static extern int MgdConfigureAsyncDisconnectNotification(
            [In] IntPtr pHandler,
            [In] bool fEnable,
            [Out] out bool pfIsClientConnected);

        [DllImport(_IIS_NATIVE_DLL)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's callers.")]
        internal static extern int MgdGetIsChildContext(
            [In] IntPtr pHandler,
            [Out] out bool pfIsChildContext);

        [DllImport(_IIS_NATIVE_DLL)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's callers.")]
        internal static extern int MgdGetConfigProperty(
            [In, MarshalAs(UnmanagedType.BStr)] string appConfigMetabasePath,
            [In, MarshalAs(UnmanagedType.BStr)] string sectionName,
            [In, MarshalAs(UnmanagedType.BStr)] string propertyName,
            [Out, MarshalAs(UnmanagedType.Struct)] out object value); // marshaled as VARIANT

        [DllImport(_IIS_NATIVE_DLL)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's callers.")]
        internal static extern int MgdPushPromise(
            [In] IntPtr context,
            [In, MarshalAs(UnmanagedType.LPWStr)] string path,
            [In, MarshalAs(UnmanagedType.LPWStr)] string queryString,
            [In, MarshalAs(UnmanagedType.LPStr)] string method,
            [In] int numHeaders,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr)] string[] headersNames,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] headersValues);

        [DllImport(_IIS_NATIVE_DLL)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's callers.")]
        internal static extern bool MgdIsAppPoolShuttingDown();

        [DllImport(_IIS_NATIVE_DLL)]
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "We carefully control this method's callers.")]
        internal static extern int MgdGetTlsTokenBindingIdentifiers(
            [In] IntPtr pHandler,
            [In, Out] ref IntPtr tokenBindingHandle,
            [Out] out IntPtr providedToken,
            [Out] out uint providedTokenSize,
            [Out] out IntPtr referredToken,
            [Out] out uint referredTokenSize);
    }
}
