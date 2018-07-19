//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    
   
    [System.Runtime.InteropServices.Guid("0000000f-0000-0000-C000-000000000046")]
    [System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    [System.Runtime.InteropServices.ComImport]
    internal interface IMoniker 
    {
        // IPersist portion
        void GetClassID(out Guid pClassID);

        // IPersistStream portion
        [System.Runtime.InteropServices.PreserveSig]
        int IsDirty();
        void Load(IStream pStm);
        void Save(IStream pStm, [MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
        void GetSizeMax(out Int64 pcbSize);

        // IMoniker portion
        void BindToObject(IBindCtx pbc, IMoniker pmkToLeft, [In()] ref Guid riidResult, IntPtr ppvResult);
        void BindToStorage(IBindCtx pbc, IMoniker pmkToLeft, [In()] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out Object ppvObj);
        void Reduce(IBindCtx pbc, int dwReduceHowFar, ref IMoniker ppmkToLeft, out IMoniker ppmkReduced);
        void ComposeWith(IMoniker pmkRight, [MarshalAs(UnmanagedType.Bool)] bool fOnlyIfNotGeneric, out IMoniker ppmkComposite);
        void Enum([MarshalAs(UnmanagedType.Bool)] bool fForward, out IEnumMoniker ppenumMoniker);
        [System.Runtime.InteropServices.PreserveSig]
        int IsEqual(IMoniker pmkOtherMoniker);
        void Hash(IntPtr pdwHash);
        [System.Runtime.InteropServices.PreserveSig]
        int IsRunning(IBindCtx pbc, IMoniker pmkToLeft, IMoniker pmkNewlyRunning);
        void GetTimeOfLastChange(IBindCtx pbc, IMoniker pmkToLeft, out System.Runtime.InteropServices.ComTypes.FILETIME pFileTime);
        void Inverse(out IMoniker ppmk);
        void CommonPrefixWith(IMoniker pmkOther, out IMoniker ppmkPrefix);
        void RelativePathTo(IMoniker pmkOther, out IMoniker ppmkRelPath);
        void GetDisplayName(IBindCtx pbc, IMoniker pmkToLeft, [MarshalAs(UnmanagedType.LPWStr)] out String ppszDisplayName);
        void ParseDisplayName(IBindCtx pbc, IMoniker pmkToLeft, [MarshalAs(UnmanagedType.LPWStr)] String pszDisplayName, out int pchEaten, out IMoniker ppmkOut);
        [System.Runtime.InteropServices.PreserveSig]
        int IsSystemMoniker(IntPtr pdwMksys);
    }
}
