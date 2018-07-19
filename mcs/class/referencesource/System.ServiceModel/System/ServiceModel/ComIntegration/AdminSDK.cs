//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    /*
     * Stolen from ES.
     * I removed SuppressUnmanagedCodeSecurity. We can re-add it later.
     */

    enum COMAdminThreadingModel
    {
        Apartment = 0,
        Free = 1,
        Main = 2,
        Both = 3,
        Neutral = 4,
        NotSpecified = 5
    }

    enum COMAdminIsolationLevel
    {
        Any = 0,
        ReadUncommitted = 1,
        ReadCommitted = 2,
        RepeatableRead = 3,
        Serializable = 4
    }

    [ComImport]
    [Guid("790C6E0B-9194-4cc9-9426-A48A63185696")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface ICatalog2
    {
        [DispId(0x00000001)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object GetCollection([In, MarshalAs(UnmanagedType.BStr)] 
            String bstrCollName);

        [DispId(0x00000002)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object Connect([In, MarshalAs(UnmanagedType.BStr)] String connectStr);

        [DispId(0x00000003)]
        int MajorVersion();

        [DispId(0x00000004)]
        int MinorVersion();

        [DispId(0x00000005)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object GetCollectionByQuery([In, MarshalAs(UnmanagedType.BStr)] 
            String collName,
            [In, MarshalAs(UnmanagedType.SafeArray)] 
            ref Object[] aQuery);

        [DispId(0x00000006)]
        void ImportComponent([In, MarshalAs(UnmanagedType.BStr)] String bstrApplIdOrName,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrCLSIDOrProgId);

        [DispId(0x00000007)]
        void InstallComponent([In, MarshalAs(UnmanagedType.BStr)] String bstrApplIdOrName,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrDLL,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrTLB,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrPSDLL);

        [DispId(0x00000008)]
        void ShutdownApplication([In, MarshalAs(UnmanagedType.BStr)] String bstrApplIdOrName);

        [DispId(0x00000009)]
        void ExportApplication([In, MarshalAs(UnmanagedType.BStr)] String bstrApplIdOrName,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationFile,
            [In] int lOptions);

        [DispId(0x0000000a)]
        void InstallApplication([In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationFile,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrDestinationDirectory,
            [In] int lOptions,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrUserId,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrPassword,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrRSN);

        [DispId(0x0000000b)]
        void StopRouter();

        [DispId(0x0000000c)]
        void RefreshRouter();

        [DispId(0x0000000d)]
        void StartRouter();

        [DispId(0x0000000e)]
        void Reserved1();

        [DispId(0x0000000f)]
        void Reserved2();

        [DispId(0x00000010)]
        void InstallMultipleComponents([In, MarshalAs(UnmanagedType.BStr)] String bstrApplIdOrName,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Object[] fileNames,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Object[] CLSIDS);

        [DispId(0x00000011)]
        void GetMultipleComponentsInfo([In, MarshalAs(UnmanagedType.BStr)] String bstrApplIdOrName,
            [In] Object varFileNames,
            [Out, MarshalAs(UnmanagedType.SafeArray)] out Object[] varCLSIDS,
            [Out, MarshalAs(UnmanagedType.SafeArray)] out Object[] varClassNames,
            [Out, MarshalAs(UnmanagedType.SafeArray)] out Object[] varFileFlags,
            [Out, MarshalAs(UnmanagedType.SafeArray)] out Object[] varComponentFlags);

        [DispId(0x00000012)]
        void RefreshComponents();

        [DispId(0x00000013)]
        void BackupREGDB([In, MarshalAs(UnmanagedType.BStr)] String bstrBackupFilePath);

        [DispId(0x00000014)]
        void RestoreREGDB([In, MarshalAs(UnmanagedType.BStr)] String bstrBackupFilePath);

        [DispId(0x00000015)]
        void QueryApplicationFile([In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationFile,
            [Out, MarshalAs(UnmanagedType.BStr)] out String bstrApplicationName,
            [Out, MarshalAs(UnmanagedType.BStr)] out String bstrApplicationDescription,
            [Out, MarshalAs(UnmanagedType.VariantBool)] out bool bHasUsers,
            [Out, MarshalAs(UnmanagedType.VariantBool)] out bool bIsProxy,
            [Out, MarshalAs(UnmanagedType.SafeArray)] out Object[] varFileNames);

        [DispId(0x00000016)]
        void StartApplication([In, MarshalAs(UnmanagedType.BStr)] String bstrApplIdOrName);

        [DispId(0x00000017)]
        int ServiceCheck([In] int lService);

        [DispId(0x00000018)]
        void InstallMultipleEventClasses([In, MarshalAs(UnmanagedType.BStr)] String bstrApplIdOrName,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Object[] fileNames,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Object[] CLSIDS);

        [DispId(0x00000019)]
        void InstallEventClass([In, MarshalAs(UnmanagedType.BStr)] String bstrApplIdOrName,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrDLL,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrTLB,
            [In, MarshalAs(UnmanagedType.BStr)] String bstrPSDLL);

        [DispId(0x0000001a)]
        void GetEventClassesForIID([In] String bstrIID,
            [In, Out, MarshalAs(UnmanagedType.SafeArray)] ref Object[] varCLSIDS,
            [In, Out, MarshalAs(UnmanagedType.SafeArray)] ref Object[] varProgIDs,
            [In, Out, MarshalAs(UnmanagedType.SafeArray)] ref Object[] varDescriptions);

        [DispId(0x0000001b)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object GetCollectionByQuery2(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrCollectionName,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarQueryStrings);

        [DispId(0x0000001c)]
        [return: MarshalAs(UnmanagedType.BStr)]
        String GetApplicationInstanceIDFromProcessID([In, MarshalAs(UnmanagedType.I4)] int lProcessID);

        [DispId(0x0000001d)]
        void ShutdownApplicationInstances([In, MarshalAs(UnmanagedType.LPStruct)] Object pVarApplicationInstanceID);

        [DispId(0x0000001e)]
        void PauseApplicationInstances([In, MarshalAs(UnmanagedType.LPStruct)] Object pVarApplicationInstanceID);

        [DispId(0x0000001f)]
        void ResumeApplicationInstances([In, MarshalAs(UnmanagedType.LPStruct)] Object pVarApplicationInstanceID);

        [DispId(0x00000020)]
        void RecycleApplicationInstances(
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarApplicationInstanceID,
                                [In, MarshalAs(UnmanagedType.I4)] int lReasonCode);

        [DispId(0x00000021)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        bool AreApplicationInstancesPaused([In, MarshalAs(UnmanagedType.LPStruct)] Object pVarApplicationInstanceID);

        [DispId(0x00000022)]
        [return: MarshalAs(UnmanagedType.BStr)]
        String DumpApplicationInstance(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationInstanceID,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrDirectory,
                                [In, MarshalAs(UnmanagedType.I4)] int lMaxImages);

        [DispId(0x00000023)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        bool IsApplicationInstanceDumpSupported();

        [DispId(0x00000024)]
        void CreateServiceForApplication(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationIDOrName,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrServiceName,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrStartType,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrErrorControl,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrDependencies,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrRunAs,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrPassword,
                                [In, MarshalAs(UnmanagedType.VariantBool)] bool bDesktopOk);

        [DispId(0x00000025)]
        void DeleteServiceForApplication([In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationIDOrName);

        [DispId(0x00000026)]
        [return: MarshalAs(UnmanagedType.BStr)]
        String GetPartitionID([In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationIDOrName);

        [DispId(0x00000027)]
        [return: MarshalAs(UnmanagedType.BStr)]
        String GetPartitionName([In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationIDOrName);

        [DispId(0x00000028)]
        void CurrentPartition([In, MarshalAs(UnmanagedType.BStr)]String bstrPartitionIDOrName);

        [DispId(0x00000029)]
        [return: MarshalAs(UnmanagedType.BStr)]
        String CurrentPartitionID();

        [DispId(0x0000002A)]
        [return: MarshalAs(UnmanagedType.BStr)]
        String CurrentPartitionName();

        [DispId(0x0000002B)]
        [return: MarshalAs(UnmanagedType.BStr)]
        String GlobalPartitionID();

        [DispId(0x0000002C)]
        void FlushPartitionCache();

        [DispId(0x0000002D)]
        void CopyApplications(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrSourcePartitionIDOrName,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarApplicationID,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrDestinationPartitionIDOrName);

        [DispId(0x0000002E)]
        void CopyComponents(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrSourceApplicationIDOrName,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarCLSIDOrProgID,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrDestinationApplicationIDOrName);

        [DispId(0x0000002F)]
        void MoveComponents(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrSourceApplicationIDOrName,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarCLSIDOrProgID,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrDestinationApplicationIDOrName);

        [DispId(0x00000030)]
        void AliasComponent(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrSrcApplicationIDOrName,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrCLSIDOrProgID,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrDestApplicationIDOrName,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrNewProgId,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrNewClsid);

        [DispId(0x00000031)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object IsSafeToDelete([In, MarshalAs(UnmanagedType.BStr)] String bstrDllName);

        [DispId(0x00000032)]
        void ImportUnconfiguredComponents(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationIDOrName,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarCLSIDOrProgID,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarComponentType);

        [DispId(0x00000033)]
        void PromoteUnconfiguredComponents(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationIDOrName,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarCLSIDOrProgID,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarComponentType);

        [DispId(0x00000034)]
        void ImportComponents(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationIDOrName,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarCLSIDOrProgID,
                                [In, MarshalAs(UnmanagedType.LPStruct)] Object pVarComponentType);

        [DispId(0x00000035)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        bool Is64BitCatalogServer();

        [DispId(0x00000036)]
        void ExportPartition(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrPartitionIDOrName,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrPartitionFileName,
                                [In, MarshalAs(UnmanagedType.I4)] int lOptions);

        [DispId(0x00000037)]
        void InstallPartition(
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrFileName,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrDestDirectory,
                                [In, MarshalAs(UnmanagedType.I4)] int lOptions,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrUserID,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrPassword,
                                [In, MarshalAs(UnmanagedType.BStr)] String bstrRSN);

        [DispId(0x00000038)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        Object QueryApplicationFile2([In, MarshalAs(UnmanagedType.BStr)] String bstrApplicationFile);

        [DispId(0x00000039)]
        [return: MarshalAs(UnmanagedType.I4)]
        int GetComponentVersionCount([In, MarshalAs(UnmanagedType.BStr)] String bstrCLSIDOrProgID);
    }

    [ComImport]
    [Guid("6EB22871-8A19-11D0-81B6-00A0C9231C29")]
    internal interface ICatalogObject
    {
        [DispId(0x00000001)]
        Object GetValue([In, MarshalAs(UnmanagedType.BStr)] String propName);

        [DispId(0x00000001)]
        void SetValue([In, MarshalAs(UnmanagedType.BStr)] String propName,
                      [In] Object value);

        [DispId(0x00000002)]
        Object Key();

        [DispId(0x00000003)]
        Object Name();

        [DispId(0x00000004)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        bool IsPropertyReadOnly([In, MarshalAs(UnmanagedType.BStr)] String bstrPropName);

        bool Valid
        {
            [DispId(0x00000005)]
            [return: MarshalAs(UnmanagedType.VariantBool)]
            get;
        }

        [DispId(0x00000006)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        bool IsPropertyWriteOnly([In, MarshalAs(UnmanagedType.BStr)] String bstrPropName);
    }

    [ComImport]
    [Guid("6EB22872-8A19-11D0-81B6-00A0C9231C29")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface ICatalogCollection
    {
        [DispId(unchecked((int)0xfffffffc))]
        void GetEnumerator(out IEnumerator pEnum);

        [DispId(0x00000001)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object Item([In] int lIndex);

        [DispId(0x60020002)]
        int Count();

        [DispId(0x60020003)]
        void Remove([In] int lIndex);

        [DispId(0x60020004)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object Add();

        [DispId(0x00000002)]
        void Populate();

        [DispId(0x00000003)]
        int SaveChanges();

        [DispId(0x00000004)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object GetCollection([In, MarshalAs(UnmanagedType.BStr)] String bstrCollName,
                             [In] Object varObjectKey);

        [DispId(0x00000006)]
        Object Name();

        bool IsAddEnabled
        {
            [DispId(0x00000007)]
            [return: MarshalAs(UnmanagedType.VariantBool)]
            get;
        }

        bool IsRemoveEnabled
        {
            [DispId(0x00000008)]
            [return: MarshalAs(UnmanagedType.VariantBool)]
            get;
        }

        [DispId(0x00000009)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object GetUtilInterface();

        int DataStoreMajorVersion
        {
            [DispId(0x0000000a)]
            get;
        }

        int DataStoreMinorVersion
        {
            [DispId(0x0000000b)]
            get;
        }

        void PopulateByKey([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
                           Object[] aKeys);

        [DispId(0x0000000d)]
        void PopulateByQuery([In, MarshalAs(UnmanagedType.BStr)] String bstrQueryString,
                             [In] int lQueryType);
    }

    [ComImport]
    [Guid("F618C514-DFB8-11D1-A2CF-00805FC79235")]
    internal class xCatalog { }
}
