using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Globalization;
using System.Threading;

/*
    These are the basic interfaces common to all CDF-based data sources.
    Sections with various keys are the norm.

 */

namespace System.Deployment.Internal.Isolation
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("285a8862-c84a-11d7-850f-005cd062464f")]
    internal interface ISection
    {
        object _NewEnum { [return:MarshalAs(UnmanagedType.Interface)] get; }
        uint Count { get; }
        uint SectionID { get; }
        string SectionName { [return:MarshalAs(UnmanagedType.LPWStr)] get; }
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("285a8871-c84a-11d7-850f-005cd062464f")]
    internal interface ISectionWithStringKey
    {
        void Lookup([MarshalAs(UnmanagedType.LPWStr)] string wzStringKey, [MarshalAs(UnmanagedType.Interface)] out object ppUnknown);
        bool IsCaseInsensitive { get; }
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("285a8876-c84a-11d7-850f-005cd062464f")]
    internal interface ISectionWithReferenceIdentityKey
    {
        void Lookup(IReferenceIdentity ReferenceIdentityKey, [MarshalAs(UnmanagedType.Interface)] out object ppUnknown);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("285a8861-c84a-11d7-850f-005cd062464f")]
    internal interface ISectionEntry
    {
        object GetField(uint fieldId);
        string GetFieldName(uint fieldId);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("00000100-0000-0000-C000-000000000046")]
    internal interface IEnumUnknown
    {
        [PreserveSig]
        int Next(uint celt, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.IUnknown)] object[] rgelt, ref uint celtFetched);
        [PreserveSig]
        int Skip(uint celt);
        [PreserveSig]
        int Reset();
        [PreserveSig]
        int Clone(out IEnumUnknown enumUnknown);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("285a8860-c84a-11d7-850f-005cd062464f")]
    internal interface ICDF
    {
        ISection GetRootSection(uint SectionId);
        ISectionEntry GetRootSectionEntry(uint SectionId);
        object _NewEnum { [return:MarshalAs(UnmanagedType.Interface)] get; }
        uint Count { get; }
        object GetItem(uint SectionId);
    }
}
