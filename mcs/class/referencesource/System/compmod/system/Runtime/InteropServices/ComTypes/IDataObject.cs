//------------------------------------------------------------------------------
// <copyright file="IDataObject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IDataObject.GetDataHere(System.Runtime.InteropServices.ComTypes.FORMATETC&,System.Runtime.InteropServices.ComTypes.STGMEDIUM&):System.Void")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IDataObject.GetDataHere(System.Runtime.InteropServices.ComTypes.FORMATETC&,System.Runtime.InteropServices.ComTypes.STGMEDIUM&):System.Void")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IDataObject.QueryGetData(System.Runtime.InteropServices.ComTypes.FORMATETC&):System.Int32")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IDataObject.DAdvise(System.Runtime.InteropServices.ComTypes.FORMATETC&,System.Runtime.InteropServices.ComTypes.ADVF,System.Runtime.InteropServices.ComTypes.IAdviseSink,System.Int32&):System.Int32")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC&,System.Runtime.InteropServices.ComTypes.STGMEDIUM&):System.Void")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IDataObject.SetData(System.Runtime.InteropServices.ComTypes.FORMATETC&,System.Runtime.InteropServices.ComTypes.STGMEDIUM&,System.Boolean):System.Void")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IDataObject.SetData(System.Runtime.InteropServices.ComTypes.FORMATETC&,System.Runtime.InteropServices.ComTypes.STGMEDIUM&,System.Boolean):System.Void")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IDataObject.GetCanonicalFormatEtc(System.Runtime.InteropServices.ComTypes.FORMATETC&,System.Runtime.InteropServices.ComTypes.FORMATETC&):System.Int32")]

namespace System.Runtime.InteropServices.ComTypes {

    /// <devdoc>
    ///     The IDataObject interface specifies methods that enable data transfer 
    ///     and notification of changes in data. Data transfer methods specify 
    ///     the format of the transferred data along with the medium through 
    ///     which the data is to be transferred. Optionally, the data can be 
    ///     rendered for a specific target device. In addition to methods for 
    ///     retrieving and storing data, the IDataObject interface specifies 
    ///     methods for enumerating available formats and managing connections 
    ///     to advisory sinks for handling change notifications.
    /// </devdoc>
    [ComImport()]
    [Guid("0000010E-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDataObject {

        /// <devdoc>
        ///     Called by a data consumer to obtain data from a source data object. 
        ///     The GetData method renders the data described in the specified FORMATETC 
        ///     structure and transfers it through the specified STGMEDIUM structure. 
        ///     The caller then assumes responsibility for releasing the STGMEDIUM structure.
        /// </devdoc>
        void GetData([In] ref FORMATETC format, out STGMEDIUM medium);

        /// <devdoc>
        ///     Called by a data consumer to obtain data from a source data object. 
        ///     This method differs from the GetData method in that the caller must 
        ///     allocate and free the specified storage medium.
        /// </devdoc>
        void GetDataHere([In] ref FORMATETC format, ref STGMEDIUM medium);

        /// <devdoc>
        ///     Determines whether the data object is capable of rendering the data 
        ///     described in the FORMATETC structure. Objects attempting a paste or 
        ///     drop operation can call this method before calling IDataObject::GetData 
        ///     to get an indication of whether the operation may be successful.
        /// </devdoc>
        [PreserveSig]
        int QueryGetData([In] ref FORMATETC format);

        /// <devdoc>
        ///     Provides a standard FORMATETC structure that is logically equivalent to one that is more 
        ///     complex. You use this method to determine whether two different 
        ///     FORMATETC structures would return the same data, removing the need 
        ///     for duplicate rendering.
        /// </devdoc>
        [PreserveSig]
        int GetCanonicalFormatEtc([In] ref FORMATETC formatIn, out FORMATETC formatOut);

        /// <devdoc>
        ///     Called by an object containing a data source to transfer data to 
        ///     the object that implements this method.
        /// </devdoc>
        void SetData([In] ref FORMATETC formatIn, [In] ref STGMEDIUM medium, [MarshalAs(UnmanagedType.Bool)] bool release);

        /// <devdoc>
        ///     Creates an object for enumerating the FORMATETC structures for a 
        ///     data object. These structures are used in calls to IDataObject::GetData 
        ///     or IDataObject::SetData. 
        /// </devdoc>
        IEnumFORMATETC EnumFormatEtc(DATADIR direction);

        /// <devdoc>
        ///     Called by an object supporting an advise sink to create a connection between 
        ///     a data object and the advise sink. This enables the advise sink to be 
        ///     notified of changes in the data of the object.
        /// </devdoc>
        [PreserveSig]
        int DAdvise([In] ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection);

        /// <devdoc>
        ///     Destroys a notification connection that had been previously set up.
        /// </devdoc>
        void DUnadvise(int connection);

        /// <devdoc>
        ///     Creates an object that can be used to enumerate the current advisory connections.
        /// </devdoc>
        [PreserveSig]
        int EnumDAdvise(out IEnumSTATDATA enumAdvise);
    }
}

