namespace Microsoft.Win32 {
    using System;
    using System.Net;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ComponentModel;
    using System.Text;
    using System.Runtime.InteropServices;

// IID_IInternetSecurityManager = 79eac9ee-baf9-11ce-8c82-00aa004ba90b
// IID_IInternetZoneManager     = 79eac9ef-baf9-11ce-8c82-00aa004ba90b
// CLSID_InternetSecurityManager= 7b8a2d94-0ac9-11d1-896c-00c04Fb6bfc4
// CLSID_InternetZoneManager    = 7b8a2d95-0ac9-11d1-896c-00c04Fb6bfc4

    [ComImport, ComVisible(false), Guid("7b8a2d94-0ac9-11d1-896c-00c04Fb6bfc4")]
    internal class InternetSecurityManager {

    }

    [ComImport, ComVisible(false), Guid("79eac9ee-baf9-11ce-8c82-00aa004ba90b"), System.Runtime.InteropServices.InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInternetSecurityManager {
        unsafe void SetSecuritySite( void *pSite);
        unsafe void GetSecuritySite( /* [out] */ void **ppSite);

        [System.Security.SuppressUnmanagedCodeSecurity]
        void MapUrlToZone(
                            [In, MarshalAs(UnmanagedType.BStr)]
                                    string pwszUrl,
                            [Out]   out int pdwZone,
                            [In]    int     dwFlags);

        unsafe void GetSecurityId(  /* [in] */ string pwszUrl,
                            /* [size_is][out] */ byte *pbSecurityId,
                            /* [out][in] */ int *pcbSecurityId,
                            /* [in] */ int dwReserved);

        unsafe void ProcessUrlAction(
                            /* [in] */ string pwszUrl,
                            /* [in] */ int dwAction,
                            /* [size_is][out] */ byte *pPolicy,
                            /* [in] */ int cbPolicy,
                            /* [in] */ byte *pContext,
                            /* [in] */ int cbContext,
                            /* [in] */ int dwFlags,
                            /* [in] */ int dwReserved);

        unsafe void QueryCustomPolicy(
                            /* [in] */ string pwszUrl,
                            /* [in] */ /*REFGUID*/ void *guidKey,
                            /* [size_is][size_is][out] */ byte **ppPolicy,
                            /* [out] */ int *pcbPolicy,
                            /* [in] */ byte *pContext,
                            /* [in] */ int cbContext,
                            /* [in] */ int dwReserved);

        unsafe void SetZoneMapping( /* [in] */ int dwZone, /* [in] */ string lpszPattern, /* [in] */ int dwFlags);

        unsafe void GetZoneMappings( /* [in] */ int dwZone, /* [out] */ /*IEnumString*/ void **ppenumString, /* [in] */ int dwFlags);
    }

    public class IntranetZoneCredentialPolicy: ICredentialPolicy
    {
        private const int URLZONE_INTRANET = 1;
        IInternetSecurityManager _ManagerRef;

        public IntranetZoneCredentialPolicy()
        {
            ExceptionHelper.ControlPolicyPermission.Demand();
            _ManagerRef = (IInternetSecurityManager)new InternetSecurityManager();
        }

        //
        // Make an interop call into UriMon
        // authModule and credential parameters are not considered
        //
        public virtual bool ShouldSendCredential(Uri challengeUri, WebRequest request, NetworkCredential credential, IAuthenticationModule authModule)
        {
            int pdwZone;
            _ManagerRef.MapUrlToZone(challengeUri.AbsoluteUri, out pdwZone, 0);
            return pdwZone == URLZONE_INTRANET;
        }
    }
}


