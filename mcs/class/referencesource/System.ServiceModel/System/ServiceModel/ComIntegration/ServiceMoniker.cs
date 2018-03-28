//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices.ComTypes;
    using Microsoft.Win32;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Threading;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Services;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;


    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Runtime.InteropServices.Guid("CE39D6F3-DAB7-41b3-9F7D-BD1CC4E92399")]
    [MonikerProxyAttribute]
    public sealed class ServiceMoniker : ContextBoundObject
    {



    }


    internal sealed class ServiceMonikerInternal : ContextBoundObject, IMoniker, IParseDisplayName, IDisposable
    {
        void IDisposable.Dispose()
        {


        }
        public ServiceMonikerInternal()
        {
            PropertyTable = new Dictionary<MonikerHelper.MonikerAttribute, string>();
        }
        void IMoniker.GetClassID(out System.Guid clsid)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        int IMoniker.IsDirty()
        {
            return HR.S_FALSE;
        }

        void IMoniker.Load(IStream stream)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IMoniker.Save(IStream stream, bool isDirty)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IMoniker.GetSizeMax(out Int64 size)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IMoniker.BindToStorage(IBindCtx pbc, IMoniker pmkToLeft, ref Guid riid, out object ppvObj)
        {

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());

        }

        void IMoniker.BindToObject(IBindCtx pbc, IMoniker pmkToLeft, ref Guid riidResult, IntPtr ppvResult)
        {
            ProxyBuilder.Build(PropertyTable, ref riidResult, ppvResult);
        }

        void IMoniker.Hash(IntPtr pdwHash)
        {
            if (IntPtr.Zero == pdwHash)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pdwHash");

            System.Runtime.InteropServices.Marshal.WriteInt32(pdwHash, 0);
        }

        void IMoniker.CommonPrefixWith(IMoniker pmkOther,
                                       out IMoniker ppmkPrefix)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }


        void IMoniker.ComposeWith(IMoniker pmkRight, bool fOnlyIfNotGeneric, out IMoniker ppmkComposite)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IMoniker.Enum(bool fForward, out IEnumMoniker ppenumMoniker)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IMoniker.GetDisplayName(IBindCtx pbc, IMoniker pmkToLeft, out string ppszDisplayName)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IMoniker.GetTimeOfLastChange(IBindCtx pbc, IMoniker pmkToLeft, out FILETIME pFileTime)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IMoniker.Inverse(out IMoniker ppmk)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        int IMoniker.IsEqual(IMoniker pmkOtherMoniker)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }


        int IMoniker.IsRunning(IBindCtx pbc,
                                 IMoniker pmkToLeft,
                                 IMoniker pmkNewlyRunning)
        {
            return HR.S_FALSE;
        }
        int IMoniker.IsSystemMoniker(IntPtr pdwMksys)
        {
            if (IntPtr.Zero == pdwMksys)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pdwMksys");

            System.Runtime.InteropServices.Marshal.WriteInt32(pdwMksys, 0);

            return HR.S_FALSE;

        }

        void IMoniker.ParseDisplayName(IBindCtx pbc,
                                       IMoniker pmkToLeft,
                                       string pszDisplayName,
                                       out int pchEaten,
                                       out IMoniker ppmkOut)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IMoniker.Reduce(IBindCtx pbc, int dwReduceHowFar, ref IMoniker ppmkToLeft, out IMoniker ppmkReduced)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IMoniker.RelativePathTo(IMoniker pmkOther, out IMoniker ppmkRelPath)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void IParseDisplayName.ParseDisplayName(IBindCtx pbc, string pszDisplayName, IntPtr pchEaten, IntPtr ppmkOut)
        {
            if (IntPtr.Zero == ppmkOut)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ppmkOut");

            System.Runtime.InteropServices.Marshal.WriteIntPtr(ppmkOut, IntPtr.Zero);

            if (IntPtr.Zero == pchEaten)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pchEaten");

            if (string.IsNullOrEmpty(pszDisplayName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pszDisplayName");


            MonikerUtility.Parse(pszDisplayName, ref PropertyTable);
            ComPlusServiceMonikerTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationServiceMonikerParsed,
                         SR.TraceCodeComIntegrationServiceMonikerParsed, PropertyTable);

            System.Runtime.InteropServices.Marshal.WriteInt32(pchEaten, pszDisplayName.Length);

            IntPtr ppv = InterfaceHelper.GetInterfacePtrForObject(typeof(IMoniker).GUID, this);

            System.Runtime.InteropServices.Marshal.WriteIntPtr(ppmkOut, ppv);
        }

        private Dictionary<MonikerHelper.MonikerAttribute, string> PropertyTable;
    }
}
