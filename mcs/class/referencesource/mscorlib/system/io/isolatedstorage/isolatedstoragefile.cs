#if !FEATURE_ISOSTORE_LIGHT
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
 * 
 * Class:  IsolatedStorageFile
// 
// <OWNER>Microsoft</OWNER>
// <OWNER>Microsoft</OWNER>
 *
 *
 * Purpose: Provides access to Application files and folders
 *
 * Date:  Feb 18, 2000
 *
 ===========================================================*/
namespace System.IO.IsolatedStorage {
    using System;
    using System.Diagnostics.Contracts;
    using System.Text;
    using System.IO;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading;
    using System.Security.Policy;
    using System.Security.Permissions;
    using System.Security.Cryptography;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
#if FEATURE_CORRUPTING_EXCEPTIONS
    using System.Runtime.ExceptionServices;
#endif // FEATURE_CORRUPTING_EXCEPTIONS
    using System.Runtime.ConstrainedExecution;    
    using System.Runtime.Versioning;
    using System.Globalization;
    using System.Collections.ObjectModel;

#if FEATURE_SERIALIZATION
    using System.Runtime.Serialization.Formatters.Binary;
#endif // FEATURE_SERIALIZATION

    [System.Runtime.InteropServices.ComVisible(true)] 
    public sealed class IsolatedStorageFile : IsolatedStorage, IDisposable    
    {

        
        private  const int    s_BlockSize = 1024;
        private  const int    s_DirSize   = s_BlockSize;
        private  const String s_name      = "file.store";
        internal const String s_Files     = "Files";
        internal const String s_AssemFiles= "AssemFiles";
        internal const String s_AppFiles= "AppFiles";
        internal const String s_IDFile    = "identity.dat";
        internal const String s_InfoFile  = "info.dat";
        internal const String s_AppInfoFile  = "appinfo.dat";

        private static volatile String s_RootDirUser;
        private static volatile String s_RootDirMachine;
        private static volatile String s_RootDirRoaming;
        private static volatile String s_appDataDir;
        private static volatile FileIOPermission s_PermUser;

        private static volatile FileIOPermission s_PermMachine;
        private static volatile FileIOPermission s_PermRoaming;
        private static volatile IsolatedStorageFilePermission s_PermAdminUser;

        private FileIOPermission m_fiop;
        private String           m_RootDir;
        private String           m_InfoFile;
        private String           m_SyncObjectName;
        [System.Security.SecurityCritical] // auto-generated
        private SafeIsolatedStorageFileHandle   m_handle;
        private bool             m_closed;
        private bool             m_bDisposed = false;

        private object           m_internalLock = new object();

#if !FEATURE_SERIALIZATION
        private String m_Id;
#endif
        private IsolatedStorageScope m_StoreScope;


        internal IsolatedStorageFile() {}

        [ResourceExposure(ResourceScope.AppDomain | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.AppDomain | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetUserStoreForDomain()
        {
            return GetStore(
                IsolatedStorageScope.Assembly|
                IsolatedStorageScope.Domain|
                IsolatedStorageScope.User, 
                null, null);
        }

        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.Machine | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetUserStoreForAssembly()
        {
            return GetStore(
                IsolatedStorageScope.Assembly|
                IsolatedStorageScope.User, 
                null, null);
        }

        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.Machine | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetUserStoreForApplication()
        {
            return GetStore(
                IsolatedStorageScope.Application|
                IsolatedStorageScope.User, 
                null);
        }

        [ComVisible(false)]
        public static IsolatedStorageFile GetUserStoreForSite() {
            throw new NotSupportedException(Environment.GetResourceString("IsolatedStorage_NotValidOnDesktop"));
        }

        [ResourceExposure(ResourceScope.AppDomain | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.AppDomain | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetMachineStoreForDomain()
        {
            return GetStore(
                IsolatedStorageScope.Assembly|
                IsolatedStorageScope.Domain|
                IsolatedStorageScope.Machine, 
                null, null);
        }

        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.Machine | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetMachineStoreForAssembly()
        {
            return GetStore(
                IsolatedStorageScope.Assembly|
                IsolatedStorageScope.Machine, 
                null, null);
        }

        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.Machine | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetMachineStoreForApplication()
        {
            return GetStore(
                IsolatedStorageScope.Application|
                IsolatedStorageScope.Machine, 
                null);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, 
            Type domainEvidenceType, Type assemblyEvidenceType)
        {
            if (domainEvidenceType != null)
                DemandAdminPermission();

            IsolatedStorageFile sf = new IsolatedStorageFile();
            sf.InitStore(scope, domainEvidenceType, assemblyEvidenceType);
            sf.Init(scope);
            return sf;
        }

        internal void EnsureStoreIsValid() {
            if(m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if(m_closed)
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, 
            Object domainIdentity, Object assemblyIdentity)
        {
            // Verify input params.
            if (assemblyIdentity == null)
                throw new ArgumentNullException("assemblyIdentity");
            Contract.EndContractBlock();

            if (IsDomain(scope) && (domainIdentity == null))
                throw new ArgumentNullException("domainIdentity");

            DemandAdminPermission();

            IsolatedStorageFile sf = new IsolatedStorageFile();
            sf.InitStore(scope, domainIdentity, assemblyIdentity, null); 
            sf.Init(scope);

            return sf;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, 
            Evidence domainEvidence, Type  domainEvidenceType,
            Evidence assemblyEvidence, Type assemblyEvidenceType)
        {
            // Verify input params.
            if (assemblyEvidence == null)
                throw new ArgumentNullException("assemblyEvidence");
            Contract.EndContractBlock();

            if (IsDomain(scope) && (domainEvidence == null))
                throw new ArgumentNullException("domainEvidence");

            DemandAdminPermission();

            IsolatedStorageFile sf = new IsolatedStorageFile();
            sf.InitStore(scope, domainEvidence, domainEvidenceType,
                assemblyEvidence, assemblyEvidenceType, null, null);
            sf.Init(scope);

            return sf;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, 
                                                                    Type applicationEvidenceType)
        {
            if (applicationEvidenceType != null)
                DemandAdminPermission();

            IsolatedStorageFile sf = new IsolatedStorageFile();
            sf.InitStore(scope, applicationEvidenceType);
            sf.Init(scope);
            return sf;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly)]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, 
                                                                    Object applicationIdentity)
        {
            if (applicationIdentity == null)
                throw new ArgumentNullException("applicationIdentity");
            Contract.EndContractBlock();

            DemandAdminPermission();

            IsolatedStorageFile sf = new IsolatedStorageFile();
            sf.InitStore(scope, null, null, applicationIdentity); 
            sf.Init(scope);

            return sf;
        }

        public override long UsedSize {
            [System.Security.SecuritySafeCritical]  // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {

                if (IsRoaming())
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "IsolatedStorage_CurrentSizeUndefined"));
                lock (m_internalLock) {
                    if (m_bDisposed)
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

                    if (m_closed)
                        throw new InvalidOperationException(
                            Environment.GetResourceString(
                                "IsolatedStorage_StoreNotOpen"));

                    if (InvalidFileHandle)
                        m_handle = Open(m_InfoFile, GetSyncObjectName());

                    return (long) GetUsage(m_handle);
                }
            }
        }

        [CLSCompliant(false)]
        [Obsolete("IsolatedStorageFile.CurrentSize has been deprecated because it is not CLS Compliant.  To get the current size use IsolatedStorageFile.UsedSize")]
        public override ulong CurrentSize
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get { 
                       
                if (IsRoaming())
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "IsolatedStorage_CurrentSizeUndefined"));
                lock (m_internalLock)
                {
                    if (m_bDisposed)
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

                    if (m_closed)
                        throw new InvalidOperationException(
                            Environment.GetResourceString(
                                "IsolatedStorage_StoreNotOpen"));

                    if (InvalidFileHandle)
                        m_handle = Open(m_InfoFile, GetSyncObjectName());

                    return GetUsage(m_handle); 
                }
            }
        }

        [System.Runtime.InteropServices.ComVisible(false)] 
        public override long AvailableFreeSpace {
            [System.Security.SecuritySafeCritical]  // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {

                if (IsRoaming()) {
                    return Int64.MaxValue;
                }

                long currentSize;

                lock (m_internalLock) {
                    if (m_bDisposed)
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

                    if (m_closed)
                        throw new InvalidOperationException(
                            Environment.GetResourceString(
                                "IsolatedStorage_StoreNotOpen"));

                    if (InvalidFileHandle)
                        m_handle = Open(m_InfoFile, GetSyncObjectName());

                    currentSize = (long) GetUsage(m_handle);
                }

                return Quota - currentSize;
            }
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public override long Quota {
            get {

                if (IsRoaming()) {
                    return Int64.MaxValue;
                }

                return base.Quota;
            }

            [System.Security.SecuritySafeCritical]
            internal set {

                bool locked = false;

                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    Lock(ref locked); // protect book-keeping info.

                    lock (m_internalLock) {

                        if (InvalidFileHandle)
                            m_handle = Open(m_InfoFile, GetSyncObjectName());

                        SetQuota(m_handle, value);
                    }

                } finally {
                    if (locked)
                        Unlock();
                }

                base.Quota = value;
            }
        }

        [System.Runtime.InteropServices.ComVisible(false)] 
        public static bool IsEnabled { get { return true; /* IsoStore always avaliable in the desktop */ } }

        [CLSCompliant(false)]
        [Obsolete("IsolatedStorageFile.MaximumSize has been deprecated because it is not CLS Compliant.  To get the maximum size use IsolatedStorageFile.Quota")]
        public override ulong MaximumSize
        {
            get 
            {

                if (IsRoaming())
                    return Int64.MaxValue;

                return base.MaximumSize;
            }
        }

        [System.Security.SecuritySafeCritical]
        [System.Runtime.InteropServices.ComVisible(false)] 
        public override Boolean IncreaseQuotaTo(Int64 newQuotaSize) {
            if(newQuotaSize <= Quota) {
                throw new ArgumentException(Environment.GetResourceString("IsolatedStorage_OldQuotaLarger"));
            }
            Contract.EndContractBlock();

            if (m_StoreScope != (IsolatedStorageScope.Application | IsolatedStorageScope.User)) {
                throw new NotSupportedException(Environment.GetResourceString("IsolatedStorage_OnlyIncreaseUserApplicationStore"));
            }

            IsolatedStorageSecurityState s = IsolatedStorageSecurityState.CreateStateToIncreaseQuotaForApplication(newQuotaSize, Quota - AvailableFreeSpace);
            try {
                s.EnsureState();
            } catch(IsolatedStorageException) {
                return false;
            }

            Quota = newQuotaSize;

            return true;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal void Reserve(ulong lReserve)
        {
            if (IsRoaming())  // No Quota enforcement for roaming
                return;

            ulong quota = (ulong) this.Quota;
            ulong reserve = lReserve;

            lock (m_internalLock)
            {
                if (m_bDisposed)
                    throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

                if (m_closed)
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "IsolatedStorage_StoreNotOpen"));

                if (InvalidFileHandle)
                    m_handle = Open(m_InfoFile, GetSyncObjectName());

                Reserve(m_handle, quota, reserve, false);
            }
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal void Unreserve(ulong lFree)
        {
            if (IsRoaming())  // No Quota enforcement for roaming
                return;

            ulong quota = (ulong) this.Quota;

            Unreserve(lFree, quota);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal void Unreserve(ulong lFree, ulong quota) {
            if (IsRoaming())  // No Quota enforcement for roaming
                return;

            ulong free = lFree;

            lock (m_internalLock) {
                if (m_bDisposed)
                    throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

                if (m_closed)
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "IsolatedStorage_StoreNotOpen"));

                if (InvalidFileHandle)
                    m_handle = Open(m_InfoFile, GetSyncObjectName());

                Reserve(m_handle, quota, free, true);
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void DeleteFile(String file)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            Contract.EndContractBlock();

            m_fiop.Assert();
            m_fiop.PermitOnly();

            long oldLen = 0;

            bool locked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                Lock(ref locked); // protect oldLen                
                try {
                    String fullPath = GetFullPath(file);
                    oldLen = LongPathFile.GetLength(fullPath);
                    LongPathFile.Delete(fullPath);
#if !DEBUG
                } catch {
                    throw new IsolatedStorageException(
                        Environment.GetResourceString(
                            "IsolatedStorage_DeleteFile"));
                }
#else
                } catch (Exception e) {
                    throw new IsolatedStorageException(
                        Environment.GetResourceString(
                            "IsolatedStorage_DeleteFile"), e);
                }
#endif
                Unreserve(RoundToBlockSize((ulong)oldLen));
            } finally {
                if(locked)
                    Unlock();
            }
            CodeAccessPermission.RevertAll();
            
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [System.Runtime.InteropServices.ComVisible(false)] 
        public bool FileExists(string path) {
            if (path == null)
                throw new ArgumentNullException("path");
            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();

            String isPath = GetFullPath(path); // Prepend IS root
            String fullPath = LongPath.NormalizePath(isPath);

            if (path.EndsWith(Path.DirectorySeparatorChar + ".", StringComparison.Ordinal)) {
                if (fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)) {
                    fullPath += ".";
                } else {
                    fullPath += Path.DirectorySeparatorChar + ".";
                }
            }

            // Make sure that we have permission to check the file so we don't
            // paths like ..\..\..\..\Windows
            try {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false));
            } catch {
                // LongPathFile.Exists returns false if the demand fails as well.
                return false;
            }

            bool ret = LongPathFile.Exists(fullPath);

            CodeAccessPermission.RevertAll();
            return ret;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [System.Runtime.InteropServices.ComVisible(false)] 
        public bool DirectoryExists(string path) {
            if (path == null)
                throw new ArgumentNullException("path");
            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();

            String isPath = GetFullPath(path); // Prepend IS root
            String fullPath = LongPath.NormalizePath(isPath);

            if (isPath.EndsWith(Path.DirectorySeparatorChar + ".", StringComparison.Ordinal)) {
                if (fullPath.EndsWith(Path.DirectorySeparatorChar)) {
                    fullPath += ".";
                } else {
                    fullPath += Path.DirectorySeparatorChar + ".";
                }
            }

            // Make sure that we have permission to check the directory so we don't
            // paths like ..\..\..\..\Windows
            try {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false));
            } catch {
                // LongPathDirectory.Exists returns false if the demand fails as well.
                return false;
            }

            bool ret = LongPathDirectory.Exists(fullPath);

            CodeAccessPermission.RevertAll();
            return ret;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]  // Scoping should be done when opening isolated storage
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void CreateDirectory(String dir)
        {
            if (dir == null)
                throw new ArgumentNullException("dir");
            Contract.EndContractBlock();

            m_fiop.Assert();
            m_fiop.PermitOnly();

            String isPath = GetFullPath(dir); // Prepend IS root
            String fullPath = LongPath.NormalizePath(isPath);


            // Make sure that we have permission to create the directory, so that we don't try to process
            // paths like ..\..\..\..\Windows
            try {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false));
            } catch {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_CreateDirectory"));
            }
            String [] dirList = DirectoriesToCreate(fullPath);
    
            
            if (dirList == null || dirList.Length == 0)
            {
                if (LongPathDirectory.Exists(isPath))
                    return;
                else
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_CreateDirectory"));
            }
            Reserve(s_DirSize*((ulong)dirList.Length));
            try {
                LongPathDirectory.CreateDirectory(dirList[dirList.Length-1]);
#if !DEBUG
            } catch {
#else
            } catch (Exception e) {
#endif
                Unreserve(s_DirSize*((ulong)dirList.Length));
                // force delete any new directories we created
                try {
                    LongPathDirectory.Delete(dirList[0], true);
                } catch {
                    // If the above failed (on index 0) then this could fail as well.
                }
#if !DEBUG
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_CreateDirectory"));
#else
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_CreateDirectory"), e);
#endif
            }
            CodeAccessPermission.RevertAll();
            
        }

        [System.Security.SecuritySafeCritical]
        [System.Runtime.InteropServices.ComVisible(false)] 
        public DateTimeOffset GetCreationTime(string path) {

            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "path");
            }

            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();

            String isPath = GetFullPath(path); // Prepend IS root
            String fullPath = LongPath.NormalizePath(isPath);

            // Make sure that we have permission to check the directory so we don't
            // paths like ..\..\..\..\Windows
            try {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false));
            } catch {
                return new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero).ToLocalTime();
            }

            DateTimeOffset ret = LongPathFile.GetCreationTime(fullPath);

            CodeAccessPermission.RevertAll();
            return ret;
        }

        [System.Security.SecuritySafeCritical]
        [System.Runtime.InteropServices.ComVisible(false)] 
        public DateTimeOffset GetLastAccessTime(string path) {

            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "path");
            }

            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();

            String isPath = GetFullPath(path); // Prepend IS root
            String fullPath = LongPath.NormalizePath(isPath);

            // Make sure that we have permission to check the directory so we don't
            // paths like ..\..\..\..\Windows
            try {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false));
            } catch {
                return new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero).ToLocalTime();
            }

            DateTimeOffset ret = LongPathFile.GetLastAccessTime(fullPath);

            CodeAccessPermission.RevertAll();
            return ret;
        }

        [System.Security.SecuritySafeCritical]
        [System.Runtime.InteropServices.ComVisible(false)] 
        public DateTimeOffset GetLastWriteTime(string path) {

            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "path");
            }

            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();

            String isPath = GetFullPath(path); // Prepend IS root
            String fullPath = LongPath.NormalizePath(isPath);

            // Make sure that we have permission to check the directory so we don't
            // paths like ..\..\..\..\Windows
            try {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new String[] { fullPath }, false, false));
            } catch {
                return new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero).ToLocalTime();
            }

            DateTimeOffset ret = LongPathFile.GetLastWriteTime(fullPath);

            CodeAccessPermission.RevertAll();
            return ret;
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public void CopyFile(string sourceFileName, string destinationFileName) {
            if (sourceFileName == null)
                throw new ArgumentNullException("sourceFileName");

            if (destinationFileName == null)
                throw new ArgumentNullException("destinationFileName");

            if (sourceFileName.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "sourceFileName");
            }

            if (destinationFileName.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "destinationFileName");
            }

            Contract.EndContractBlock();

            CopyFile(sourceFileName, destinationFileName, false);
        }

        [System.Security.SecuritySafeCritical]
        [System.Runtime.InteropServices.ComVisible(false)]
        public void CopyFile(string sourceFileName, string destinationFileName, bool overwrite) {
            if (sourceFileName == null)
                throw new ArgumentNullException("sourceFileName");

            if (destinationFileName == null)
                throw new ArgumentNullException("destinationFileName");

            if (sourceFileName.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "sourceFileName");
            }

            if (destinationFileName.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "destinationFileName");
            }

            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();

            String sourceFileNameFullPath = LongPath.NormalizePath(GetFullPath(sourceFileName));
            String destinationFileNameFullPath = LongPath.NormalizePath(GetFullPath(destinationFileName));

            // Make sure that we have permission to check the directory so we don't
            // paths like ..\..\..\..\Windows
            try {
                Demand(new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new String[] { sourceFileNameFullPath }, false, false));
                Demand(new FileIOPermission(FileIOPermissionAccess.Write, new String[] { destinationFileNameFullPath }, false, false));                
#if !DEBUG
            } catch {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
#else 
            } catch (Exception e) {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"), e);
            }
#endif
            bool isLocked = false;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                Lock(ref isLocked);

                long fileLen = 0;
                
                try {
                    fileLen = LongPathFile.GetLength(sourceFileNameFullPath);
                } catch (FileNotFoundException) {
                    throw new FileNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", sourceFileName));
                } catch (DirectoryNotFoundException) {
                    throw new DirectoryNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", sourceFileName));
#if !DEBUG
                } catch {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
                }
#else
                } catch (Exception e) {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"), e);
                }
#endif

                long destLen = 0;
                if (LongPathFile.Exists(destinationFileNameFullPath)) {
                    try { 
                        destLen = LongPathFile.GetLength(destinationFileNameFullPath);
#if !DEBUG
                    } catch {
                        throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
                    }
#else
                    } catch (Exception e) {
                        throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"), e);
                    }
#endif
                }

                if (destLen < fileLen) {
                    Reserve(RoundToBlockSize((ulong)(fileLen - destLen)));
                }

                try {
                    LongPathFile.Copy(sourceFileNameFullPath, destinationFileNameFullPath, overwrite);
                } catch (FileNotFoundException) {

                    // Copying the file failed, undo our reserve.
                    if (destLen < fileLen) {
                        Unreserve(RoundToBlockSize((ulong)(fileLen - destLen)));
                    }

                    throw new FileNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", sourceFileName));
#if !DEBUG
                } catch {
#else
                } catch (Exception e) {
#endif

                    // Copying the file failed, undo our reserve.
                    if (destLen < fileLen) {
                        Unreserve(RoundToBlockSize((ulong)(fileLen - destLen)));
                    }

#if !DEBUG
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
#else
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"), e);
#endif
                }

                // If the file we we overwrote was larger than the source file, then we can free some used blocks.
                if (destLen > fileLen && overwrite) {
                    Unreserve(RoundToBlockSizeFloor((ulong) (destLen - fileLen)));
                }

            } finally {
                if (isLocked) {
                    Unlock();
                }
            }

        }

        [System.Security.SecuritySafeCritical]
        [System.Runtime.InteropServices.ComVisible(false)]
        public void MoveFile(string sourceFileName, string destinationFileName) {

            if (sourceFileName == null)
                throw new ArgumentNullException("sourceFileName");

            if (destinationFileName == null)
                throw new ArgumentNullException("destinationFileName");

            if (sourceFileName.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "sourceFileName");
            }

            if (destinationFileName.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "destinationFileName");
            }

            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();

            String sourceFileNameFullPath = LongPath.NormalizePath(GetFullPath(sourceFileName));
            String destinationFileNameFullPath = LongPath.NormalizePath(GetFullPath(destinationFileName));

            // Make sure that we have permission to check the directory so we don't
            // paths like ..\..\..\..\Windows
            try {
                Demand(new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new String[] { sourceFileNameFullPath }, false, false));
                Demand(new FileIOPermission(FileIOPermissionAccess.Write, new String[] { destinationFileNameFullPath }, false, false));
#if !DEBUG
            } catch {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
#else
            } catch (Exception e) {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"), e);
            }
#endif

            try {
                LongPathFile.Move(sourceFileNameFullPath, destinationFileNameFullPath);
            } catch (FileNotFoundException) {
                throw new FileNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", sourceFileName));
#if !DEBUG
            } catch {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
#else
            } catch (Exception e) {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"), e);
            }
#endif

            CodeAccessPermission.RevertAll();
        }

        [System.Security.SecuritySafeCritical]
        [System.Runtime.InteropServices.ComVisible(false)]
        public void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName) {

            if (sourceDirectoryName == null)
                throw new ArgumentNullException("sourceDirectoryName");

            if (destinationDirectoryName == null)
                throw new ArgumentNullException("destinationDirectoryName");

            if (sourceDirectoryName.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "sourceDirectoryName");
            }

            if (destinationDirectoryName.Trim().Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "destinationDirectoryName");
            }

            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();

            String sourceDirectoryNameFullPath = LongPath.NormalizePath(GetFullPath(sourceDirectoryName));
            String destinationDirectoryNameFullPath = LongPath.NormalizePath(GetFullPath(destinationDirectoryName));

            // Make sure that we have permission to check the directory so we don't
            // paths like ..\..\..\..\Windows
            try {
                Demand(new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new String[] { sourceDirectoryNameFullPath }, false, false));
                Demand(new FileIOPermission(FileIOPermissionAccess.Write, new String[] { destinationDirectoryNameFullPath }, false, false));
#if !DEBUG
            } catch {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
#else
            } catch (Exception e) {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"), e);
            }
#endif

            try {
                LongPathDirectory.Move(sourceDirectoryNameFullPath, destinationDirectoryNameFullPath);
            } catch (DirectoryNotFoundException) {
                throw new DirectoryNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", sourceDirectoryName));
#if !DEBUG
            } catch {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
#else
            } catch (Exception e) {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"), e);
            }
#endif
            CodeAccessPermission.RevertAll();
        }

        // Given a path to a dir to create, will return the list of directories to create and the last one in the array is the actual dir to create.
        // for example if dir is a\\b\\c and none of them exist, the list returned will be a, a\\b, a\\b\\c.
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private String[] DirectoriesToCreate(String fullPath)
        {
            
            List<String> list = new List<String>();
            int length = fullPath.Length;

            // We need to trim the trailing slash or the code will try to create 2 directories of the same name.
            if (length >= 2 && fullPath[length - 1] == SeparatorExternal)
                length--;
            int i = LongPath.GetRootLength(fullPath);

            // Attempt to figure out which directories don't exist
            while (i < length) {
                i++;
                while (i < length && fullPath[i] != SeparatorExternal) 
                    i++;
                String currDir = fullPath.Substring(0, i);
                    
                if (!LongPathDirectory.InternalExists(currDir)) 
                { // Create only the ones missing
                    list.Add(currDir);
                }
            }

            if (list.Count != 0)
            {
                return list.ToArray();
            }
            return null;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void DeleteDirectory(String dir)
        {
            if (dir == null)
                throw new ArgumentNullException("dir");
            Contract.EndContractBlock();

            m_fiop.Assert();
            m_fiop.PermitOnly();

            bool locked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                Lock(ref locked); // Delete *.*, will beat quota enforcement without this lock
                try {
                    String normalizedDir = LongPath.NormalizePath(GetFullPath(dir));

                    if (normalizedDir.Equals(LongPath.NormalizePath(GetFullPath(".")), StringComparison.Ordinal)) {
                        throw new IsolatedStorageException(
                            Environment.GetResourceString(
                                "IsolatedStorage_DeleteDirectory"));
                    }

                    LongPathDirectory.Delete(normalizedDir, false);

#if !DEBUG
                } catch {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectory"));
                }
#else
                } catch (Exception e) {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectory"), e);
                }
#endif
                Unreserve(s_DirSize);
            } finally {
                if(locked)
                    Unlock();
            }
            CodeAccessPermission.RevertAll();
        }

        // Stack walks start at the frame above the demanding frame, so if we need to catch a PermitOnly
        // that was added on the current frame, we need to use this utility function to do the demand.
        [System.Security.SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Demand(CodeAccessPermission permission)
        {
            permission.Demand();
        }


        [System.Runtime.InteropServices.ComVisible(false)] 
        public String[] GetFileNames() {
            return GetFileNames("*");
        }

        /*
         * foo\abc*.txt will give all abc*.txt files in foo directory
         */
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine | ResourceScope.Assembly)]
        public String[] GetFileNames(String searchPattern)
        {
            if (searchPattern == null)
                throw new ArgumentNullException("searchPattern");
            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();
            String[] retVal = GetFileDirectoryNames(GetFullPath(searchPattern), searchPattern, true);
            CodeAccessPermission.RevertAll();
            return retVal;
        }
            
        [System.Runtime.InteropServices.ComVisible(false)] 
        public String[] GetDirectoryNames() {
            return GetDirectoryNames("*");
        }

        /*
         * foo\data* will give all directory names in foo directory that 
         * starts with data
         */
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]  // Scoping should be done when opening isolated storage.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public String[] GetDirectoryNames(String searchPattern)
        {
            if (searchPattern == null)
                throw new ArgumentNullException("searchPattern");
            Contract.EndContractBlock();

            if (m_bDisposed)
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

            if (m_closed)
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "IsolatedStorage_StoreNotOpen"));

            m_fiop.Assert();
            m_fiop.PermitOnly();
            String[] retVal = GetFileDirectoryNames(GetFullPath(searchPattern), searchPattern, false);
            CodeAccessPermission.RevertAll();
            return retVal;
        }

        private static String NormalizeSearchPattern(String searchPattern)
        {
            Contract.Requires(searchPattern != null);

            // Win32 normalization trims only U+0020. 
            String tempSearchPattern = searchPattern.TrimEnd(Path.TrimEndChars);
            Path.CheckSearchPattern(tempSearchPattern);
            return tempSearchPattern;
        }

        [System.Runtime.InteropServices.ComVisible(false)] 
        public IsolatedStorageFileStream OpenFile(string path, FileMode mode) {
            return new IsolatedStorageFileStream(path, mode, this);
            
        }

        [System.Runtime.InteropServices.ComVisible(false)] 
        public IsolatedStorageFileStream OpenFile(string path, FileMode mode, FileAccess access) {
            return new IsolatedStorageFileStream(path, mode, access, this);
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public IsolatedStorageFileStream OpenFile(string path, FileMode mode, FileAccess access, FileShare share) {
            return new IsolatedStorageFileStream(path, mode, access, share, this);
        }

        [System.Runtime.InteropServices.ComVisible(false)] 
        public IsolatedStorageFileStream CreateFile(string path) {
            return new IsolatedStorageFileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, this);
        }

        // Remove this individual store
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]  // Scoping should be done when opening isolated storage.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public override void Remove()
        {
            // No security check required here since we have already done
            // that during creation

            String rootDir, domainRoot = null;

            // First remove the logical root directory of this store, this
            // will not delete the quota file. Removes all the files and dirs
            // that applications see.

            RemoveLogicalDir();
            Close();

            // Now try to remove other files folders that become unnecessary
            // if the application directory is deleted.

            StringBuilder sb = new StringBuilder();

            sb.Append(GetRootDir(this.Scope));

            if (IsApp())
            {
                sb.Append(this.AppName);
                sb.Append(this.SeparatorExternal);
            }
            else 
            {
                if (IsDomain())
                {
                    sb.Append(this.DomainName);
                    sb.Append(this.SeparatorExternal);
                    domainRoot = sb.ToString();
                }

                sb.Append(this.AssemName);
                sb.Append(this.SeparatorExternal);
            }
            rootDir = sb.ToString();

            new FileIOPermission(
                FileIOPermissionAccess.AllAccess, rootDir).Assert();

            if (ContainsUnknownFiles(rootDir))
                return;

            try {

                LongPathDirectory.Delete(rootDir, true);

            } catch {
                return; // OK to ignore this exception.
            }

            // If this was a domain store, and if this happens to be
            // the only store around, then delete the root store for this
            // domain

            if (IsDomain())
            {
                CodeAccessPermission.RevertAssert();

                new FileIOPermission(
                    FileIOPermissionAccess.AllAccess, domainRoot).Assert();

                if (!ContainsUnknownFiles(domainRoot))
                {

                    try {

                        LongPathDirectory.Delete(domainRoot, true);

                    } catch {
                        return; // OK to ignore this exception.
                    }
                }
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]  // Scoping should be done when opening isolated storage.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void RemoveLogicalDir()
        {
            m_fiop.Assert();

            ulong oldLen;
            ulong oldQuota;

            bool locked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                Lock(ref locked);   // A ---- here with delete dir/delete file can get around 
                                    // quota enforcement.

                if (!Directory.Exists(RootDirectory)) {
                    // Remove() was already called on another object that represented the same store.
                    return;
                }
                
                oldLen = IsRoaming() ? 0 : (ulong) (Quota - AvailableFreeSpace);
                oldQuota = (ulong)Quota;
    
                try {
    
                    LongPathDirectory.Delete(RootDirectory, true);
#if !DEBUG
                } catch {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"));
                }
#else
                } catch (Exception e) {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"), e);
                }   
#endif
                Unreserve(oldLen, oldQuota);
            } finally {            
                if(locked)
                    Unlock();
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private bool ContainsUnknownFiles(String rootDir)
        {
            String[] dirs, files;

            // Delete everything in the root directory of this store
            // if there are no Domain Stores / other files
            // Make sure that there are no other subdirs present here other
            // than the ones used by IsolatedStorageFile (Cookies in future
            // releases ?)

            try {
                files = GetFileDirectoryNames(rootDir + "*", "*", true);
                dirs = GetFileDirectoryNames(rootDir + "*", "*", false);
#if !DEBUG
            } catch {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"));
            }
#else
            } catch (Exception e) {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"), e);
            }   
#endif
            // First see if there are any unkonwn Folders
            if ((dirs != null) && (dirs.Length > 0))
            {
                if (dirs.Length > 1)
                {
                    // More than one directory present
                    return true;
                }

                if (IsApp())
                {
                    if (NotAppFilesDir(dirs[0]))
                        return true;
                }
                else if (IsDomain())
                {
                    if (NotFilesDir(dirs[0]))
                        return true;
                }
                else
                {
                    if (NotAssemFilesDir(dirs[0]))
                        return true;
                }
            }

            // Now look at the files

            if ((files == null) || (files.Length == 0))
                return false;

            if (IsRoaming())
            {
                if ((files.Length > 1) || NotIDFile(files[0]))
                {
                    // There is one or more files unknown to this version
                    // of IsoStoreFile

                    return true;
                }

                return false;
            }

            if ((files.Length > 2) ||
                (NotIDFile(files[0]) && NotInfoFile(files[0])) ||
                ((files.Length == 2) &&
                NotIDFile(files[1]) && NotInfoFile(files[1])))
            {
                // There is one or more files unknown to this version
                // of IsoStoreFile

                return true;
            }

            return false; 
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.None)]
        public void Close()
        {
            if (IsRoaming())
                return;
            
            lock (m_internalLock) {

                if (!m_closed) {
                    m_closed = true;

                    if(m_handle != null)
                        m_handle.Dispose();

                    GC.SuppressFinalize(this);
                }

            }
        }

        public void Dispose()
        {
            Close();
            m_bDisposed = true;
        }

        ~IsolatedStorageFile()
        {
            Dispose();
        }

        // Macros, expect JIT to expand this
        private static bool NotIDFile(String file)
        {
            return (String.Compare(
                file, IsolatedStorageFile.s_IDFile, StringComparison.Ordinal) != 0); 
        }

        private static bool NotInfoFile(String file)
        {
            return (
                String.Compare(file, IsolatedStorageFile.s_InfoFile, StringComparison.Ordinal) != 0 && 
                String.Compare(file, IsolatedStorageFile.s_AppInfoFile, StringComparison.Ordinal) != 0);
        }

        private static bool NotFilesDir(String dir)
        {
            return (String.Compare(
                dir, IsolatedStorageFile.s_Files, StringComparison.Ordinal) != 0);
        }
        internal static bool NotAssemFilesDir(String dir)
        {
            return (String.Compare(
                dir, IsolatedStorageFile.s_AssemFiles, StringComparison.Ordinal) != 0);
        }

        internal static bool NotAppFilesDir(String dir)
        {
            return (String.Compare(
                dir, IsolatedStorageFile.s_AppFiles, StringComparison.Ordinal) != 0);
        }

        // Remove store for all identities
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]  // Scoping should be done when opening isolated storage.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public static void Remove(IsolatedStorageScope scope)
        {

            VerifyGlobalScope(scope);
            DemandAdminPermission();
            String rootDir = GetRootDir(scope);

            new FileIOPermission(
                FileIOPermissionAccess.Write, rootDir).Assert();

            try {
                LongPathDirectory.Delete(rootDir, true);    // Remove all sub dirs and files
                LongPathDirectory.CreateDirectory(rootDir); // Recreate the root dir
#if !DEBUG
            } catch {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"));
            }
#else
            } catch (Exception e) {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"), e);
            }   
#endif
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        public static IEnumerator GetEnumerator(IsolatedStorageScope scope)
        {

            VerifyGlobalScope(scope);
            DemandAdminPermission();

            return new IsolatedStorageFileEnumerator(scope);
        }

        // Internal & private methods

        internal String RootDirectory
        {
            get { return m_RootDir; }
        }

        // RootDirectory has been scoped already.
        internal String GetFullPath(String path)
        {
            Contract.Requires(path != null);

            if (path == String.Empty)
            {
                return this.RootDirectory;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(this.RootDirectory);

            if (path[0] == SeparatorExternal)
                sb.Append(path.Substring(1));
            else
                sb.Append(path);

            return sb.ToString();
        }

#if !FEATURE_PAL        
        [System.Security.SecurityCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Assert, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static String GetDataDirectoryFromActivationContext()
        {
            if (s_appDataDir == null)
            {
                ActivationContext activationContext = AppDomain.CurrentDomain.ActivationContext;
                if (activationContext == null)
                    throw new IsolatedStorageException(
                            Environment.GetResourceString(
                                "IsolatedStorage_ApplicationMissingIdentity"));
                String dataDir = activationContext.DataDirectory;
                if (dataDir != null)
                {
                    //Append a '\' at the end if it already does not end with one
                    if (dataDir[dataDir.Length-1] != '\\')
                        dataDir = dataDir + "\\";
                }
                s_appDataDir = dataDir;
            }
            return s_appDataDir;
        }
#endif

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine | ResourceScope.Assembly)]
        internal void Init(IsolatedStorageScope scope)
        {
            GetGlobalFileIOPerm(scope).Assert();

            m_StoreScope = scope;

            StringBuilder sb = new StringBuilder();

            // Create the root directory if it is not already there

            if (IsApp(scope))
            {
#if FEATURE_PAL
                throw new IsolatedStorageException(
                        Environment.GetResourceString(
                            "IsolatedStorage_ApplicationMissingIdentity"));
#endif // !FEATURE_PAL

                sb.Append(GetRootDir(scope));

                if (s_appDataDir == null)
                {
                    // We're not using the App Data directory...so we need to append AppName 
                    sb.Append(this.AppName);
                    sb.Append(this.SeparatorExternal);
                }

                try {

                    LongPathDirectory.CreateDirectory(sb.ToString());

                    // No exception implies this directory was created now
#if !DEBUG
                } catch {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
#else
                } catch (Exception e) {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"), e);
#endif
                }
                // Create the Identity blob file in the root 
                // directory. OK if there are more than one created
                // last one wins

                CreateIDFile(sb.ToString(), IsolatedStorageScope.Application);

                // For App Stores, accounting is done in the app root
                this.m_InfoFile = sb.ToString() + s_AppInfoFile;

                sb.Append(s_AppFiles);

            }
            else
            {
                sb.Append(GetRootDir(scope));               
                if (IsDomain(scope))
                {
                    sb.Append(this.DomainName);
                    sb.Append(this.SeparatorExternal);
        
                    try {

                        LongPathDirectory.CreateDirectory(sb.ToString());

                        // No exception implies this directory was created now

                        // Create the Identity blob file in the root 
                        // directory. OK if there are more than one created
                        // last one wins

                        CreateIDFile(sb.ToString(), IsolatedStorageScope.Domain);
#if !DEBUG
                    } catch {
                        throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
#else
                    } catch (Exception e) {
                        throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"), e);
#endif
                    }

                    // For Domain Stores, accounting is done in the domain root
                    this.m_InfoFile = sb.ToString() + s_InfoFile;
                }

                sb.Append(this.AssemName);
                sb.Append(this.SeparatorExternal);

                try {

                    LongPathDirectory.CreateDirectory(sb.ToString());

                    // No exception implies this directory was created now

                    // Create the Identity blob file in the root 
                    // directory. OK if there are more than one created
                    // last one wins
                    CreateIDFile(sb.ToString(), IsolatedStorageScope.Assembly);
#if !DEBUG
                } catch {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
#else
                } catch (Exception e) {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"), e);
#endif
                }

                if (IsDomain(scope))
                {
                    sb.Append(s_Files);
                }
                else
                {
                    // For Assem Stores, accounting is done in the assem root
                    this.m_InfoFile = sb.ToString() + s_InfoFile;

                    sb.Append(s_AssemFiles);
                }
            }

            sb.Append(this.SeparatorExternal);

            String rootDir = sb.ToString();

            try {
                LongPathDirectory.CreateDirectory(rootDir);
#if !DEBUG
            } catch {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
#else
            } catch (Exception e) {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"), e);
#endif
            }

            this.m_RootDir = rootDir;

            // Use the "new" RootDirectory to create the permission.
            // This instance of permission is not the same as the
            // one we just asserted. It uses this.base.RootDirectory.

            m_fiop = new FileIOPermission(
                FileIOPermissionAccess.AllAccess, rootDir);


            if (scope == (IsolatedStorageScope.Application | IsolatedStorageScope.User)) {
                UpdateQuotaFromInfoFile();
            }
        }

        [System.Security.SecurityCritical]
        private void UpdateQuotaFromInfoFile() {
            bool locked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                Lock(ref locked); // protect book-keeping info.

                lock (m_internalLock) {

                    if (InvalidFileHandle)
                        m_handle = Open(m_InfoFile, GetSyncObjectName());

                    long quota = 0;

                    if (GetQuota(m_handle, out quota)) {
                        base.Quota = quota;
                        return;
                    }
                }

            } finally {
                if (locked)
                    Unlock();
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly)]
        internal bool InitExistingStore(IsolatedStorageScope scope)
        {
            FileIOPermission fp;
            StringBuilder sb = new StringBuilder();

            m_StoreScope = scope;

            sb.Append(GetRootDir(scope));

            if (IsApp(scope))
            {
                sb.Append(this.AppName);
                sb.Append(this.SeparatorExternal);
                // For App Stores, accounting is done in the app root
                this.m_InfoFile = sb.ToString() + s_AppInfoFile;

                sb.Append(s_AppFiles);
            }
            else 
            {
                if (IsDomain(scope))
                {
                    sb.Append(this.DomainName);
                    sb.Append(this.SeparatorExternal);

                    // For Domain Stores, accounting is done in the domain root
                    this.m_InfoFile = sb.ToString() + s_InfoFile;
                }

                sb.Append(this.AssemName);
                sb.Append(this.SeparatorExternal);

                if (IsDomain(scope))
                {
                    sb.Append(s_Files);
                }
                else
                {
                    // For Assem Stores, accounting is done in the assem root
                    this.m_InfoFile = sb.ToString() + s_InfoFile;

                    sb.Append(s_AssemFiles);
                }
            }
            sb.Append(this.SeparatorExternal);

            fp = new FileIOPermission(
                FileIOPermissionAccess.AllAccess, sb.ToString());

            fp.Assert();

            if (!LongPathDirectory.Exists(sb.ToString()))
                return false;

            this.m_RootDir = sb.ToString();
            this.m_fiop = fp;

            if (scope == (IsolatedStorageScope.Application | IsolatedStorageScope.User)) {
                UpdateQuotaFromInfoFile();
            }

            return true;
        }

        protected override IsolatedStoragePermission GetPermission(
                PermissionSet ps)
        {
            if (ps == null)
                return null;
            else if (ps.IsUnrestricted())
                return new IsolatedStorageFilePermission(
                        PermissionState.Unrestricted);

            return (IsolatedStoragePermission) ps.
                    GetPermission(typeof(IsolatedStorageFilePermission));
        }

        internal void UndoReserveOperation(ulong oldLen, ulong newLen)
        {
            oldLen = RoundToBlockSize(oldLen);
            if (newLen > oldLen)
                Unreserve(RoundToBlockSize(newLen - oldLen));
        }

        internal void Reserve(ulong oldLen, ulong newLen)
        {
            oldLen = RoundToBlockSize(oldLen);
            if (newLen > oldLen)
                Reserve(RoundToBlockSize(newLen - oldLen));
        }

        internal void ReserveOneBlock()
        {
            Reserve(s_BlockSize);
        }

        internal void UnreserveOneBlock()
        {
            Unreserve(s_BlockSize);
        }

        internal static ulong RoundToBlockSize(ulong num)
        {
            if (num < s_BlockSize)
                return s_BlockSize;

            ulong rem = (num % s_BlockSize);

            if (rem != 0)
                num += (s_BlockSize - rem);

            return num;
        }

        internal static ulong RoundToBlockSizeFloor(ulong num) {
            if (num < s_BlockSize)
                return 0;

            ulong rem = (num % s_BlockSize);
            num -= rem;

            return num;
        }

        // Helper static methods
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static String GetRootDir(IsolatedStorageScope scope)
        {
            if (IsRoaming(scope))
            {
                if (s_RootDirRoaming == null)
                {
                    string rootDir = null;
                    GetRootDir(scope, JitHelpers.GetStringHandleOnStack(ref rootDir));
                    s_RootDirRoaming = rootDir;
                }

                return s_RootDirRoaming;
            }

            if (IsMachine(scope))
            {
                if (s_RootDirMachine == null)
                    InitGlobalsMachine(scope);

                return s_RootDirMachine;    
            }

            // This is then the non-roaming user store.
            if (s_RootDirUser == null)
                InitGlobalsNonRoamingUser(scope);
                
            return s_RootDirUser;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
#if FEATURE_CORRUPTING_EXCEPTIONS
        [HandleProcessCorruptedStateExceptions] // 
#endif // FEATURE_CORRUPTING_EXCEPTIONS
        private static void InitGlobalsMachine(IsolatedStorageScope scope)
        {
            string rootDir = null;
            GetRootDir(scope, JitHelpers.GetStringHandleOnStack(ref rootDir));
            new FileIOPermission(FileIOPermissionAccess.AllAccess, rootDir).Assert();

            String rndName = GetMachineRandomDirectory(rootDir);
            if (rndName == null) {  // Create a random directory
                Mutex m = CreateMutexNotOwned(rootDir);
                if (!m.WaitOne())
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
                try {   // finally...
                    rndName = GetMachineRandomDirectory(rootDir);  // try again with lock
                    if (rndName == null) {
                        string relRandomDirectory1 = Path.GetRandomFileName();
                        string relRandomDirectory2 = Path.GetRandomFileName();
                        try {
                            CreateDirectoryWithDacl(rootDir + relRandomDirectory1);
                            // Now create the root directory with the correct DACL
                            CreateDirectoryWithDacl(rootDir + relRandomDirectory1 + "\\" + relRandomDirectory2);
#if !DEBUG
                        } catch {
#else
                        } catch (Exception e) {
#endif
                            // We don't want to leak any information here
                            // Throw a store initialization exception instead
#if !DEBUG
                            throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
#else
                            throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"), e);
#endif
                        }
                        rndName = relRandomDirectory1 + "\\" + relRandomDirectory2;
                    }
                } finally {
                    m.ReleaseMutex();
                }
            }
            s_RootDirMachine = rootDir + rndName + "\\";
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private static void InitGlobalsNonRoamingUser(IsolatedStorageScope scope)
        {
            String rootDir = null;
#if !FEATURE_PAL             
            if (scope == c_AppUser)
            {
                rootDir = GetDataDirectoryFromActivationContext();
                if (rootDir != null)
                {
                    s_RootDirUser = rootDir;
                    return;
                }
            }
#endif            

            // Non App Data directory case or non-App case:
            GetRootDir(scope, JitHelpers.GetStringHandleOnStack(ref rootDir));
            new FileIOPermission(FileIOPermissionAccess.AllAccess, rootDir).Assert();
            bool bMigrateNeeded = false;
            string sOldStoreLocation = null;
            String rndName = GetRandomDirectory(rootDir, out bMigrateNeeded, out sOldStoreLocation);
            if (rndName == null) {  // Create a random directory
                Mutex m = CreateMutexNotOwned(rootDir);
                if (!m.WaitOne())
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
                try {   // finally...
                    rndName = GetRandomDirectory(rootDir, out bMigrateNeeded, out sOldStoreLocation);  // try again with lock
                    if (rndName == null) {
                        if (bMigrateNeeded) {
                            // We have a store directory in the old format; we need to migrate it
                            rndName = MigrateOldIsoStoreDirectory(rootDir, sOldStoreLocation);
                        } else {
                            rndName = CreateRandomDirectory(rootDir);                   
                        }
                    }
                } finally {
                    m.ReleaseMutex();
                }
            }
            s_RootDirUser = rootDir + rndName + "\\";
        }

        internal bool Disposed 
        {
            get { return m_bDisposed; }
        }

        // Check to see if m_handle represent a valid handle
        private bool InvalidFileHandle
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get { return m_handle == null || m_handle.IsClosed || m_handle.IsInvalid; }
        }

        // Migrates the old store location to a new one and returns the new location without the path separator
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine | ResourceScope.Assembly)]
#if FEATURE_CORRUPTING_EXCEPTIONS        
        [System.Security.SecuritySafeCritical]
        [HandleProcessCorruptedStateExceptions] // 
#endif // FEATURE_CORRUPTING_EXCEPTIONS
        internal static string MigrateOldIsoStoreDirectory(string rootDir, string oldRandomDirectory) {
            // First create the new random directory
            string relRandomDirectory1 = Path.GetRandomFileName();
            string relRandomDirectory2 = Path.GetRandomFileName();
            string firstRandomDirectory  = rootDir + relRandomDirectory1;
            string newRandomDirectory = firstRandomDirectory + "\\" + relRandomDirectory2;
            // Move the old directory to the new location, throw an exception and revert
            // the transaction if the operation is not successful
            try {
                // Create the first level of the new random directory
                LongPathDirectory.CreateDirectory(firstRandomDirectory);
                // Move the old directory under the newly created random directory
                LongPathDirectory.Move(rootDir + oldRandomDirectory, newRandomDirectory);
#if !DEBUG
            } catch {
#else
            } catch (Exception e) {
#endif
                // We don't want to leak any information here
                // Throw a store initialization exception instead
#if !DEBUG
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
#else
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"), e);
#endif
            }
            return (relRandomDirectory1 + "\\" + relRandomDirectory2);
        }

        // creates and returns the relative path to the random directory string without the path separator
        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Assembly | ResourceScope.Machine)]
#if FEATURE_CORRUPTING_EXCEPTIONS
        [System.Security.SecuritySafeCritical]
        [HandleProcessCorruptedStateExceptions] // 
#endif // FEATURE_CORRUPTING_EXCEPTIONS
        internal static string CreateRandomDirectory(String rootDir) {
            string rndName;
            string dirToCreate;
            do {
                rndName = Path.GetRandomFileName() + "\\" + Path.GetRandomFileName();
                dirToCreate = rootDir + rndName;
            } while (LongPathDirectory.Exists(dirToCreate));
            // Note that there is still a small window (between where we check for .Exists and execute the .CreateDirectory)
            // when another process can come up with the same random name and create that directory.
            // That's potentially a security hole, but the odds of that are low enough that the risk is acceptable.
            try {
                LongPathDirectory.CreateDirectory(dirToCreate);
#if !DEBUG
            } catch {
#else
            } catch (Exception e) {
#endif
                // We don't want to leak any information here
                // Throw a store initialization exception instead
#if !DEBUG
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
#else
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"), e);
#endif
            }
            return rndName;
        }

        // returns the relative path to the current random directory string if one is there without the path separator
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static string GetRandomDirectory(String rootDir, out bool bMigrateNeeded, out string sOldStoreLocation)
        {
            // Initialize Out Parameters 
            bMigrateNeeded = false; sOldStoreLocation = null;
            String[] nodes1 = GetFileDirectoryNames(rootDir + "*", "*", false);
            // First see if there is a new store 
            for (int i=0; i<nodes1.Length; ++i) {
                if (nodes1[i].Length == 12) {
                    String[] nodes2 = GetFileDirectoryNames(rootDir + nodes1[i] + "\\" + "*", "*", false);
                    for (int j=0; j<nodes2.Length; ++j) {
                        if (nodes2[j].Length == 12) {
                            return (nodes1[i] +  "\\" + nodes2[j]); // Get the first directory
                        }
                    }
                }
            }
            // We look for directories of length 24: if we find one
            // it means we are still using the old random directory format.
            // In that case, migrate to a new store 
            for (int i=0; i<nodes1.Length; ++i) {
                if (nodes1[i].Length == 24) {
                    bMigrateNeeded = true;
                    sOldStoreLocation = nodes1[i]; // set the old store location
                    return null;
                }
            }
            // Neither old or new store formats have been encountered, return null
            return null;
        }

        // returns the relative path to the current random directory string if one is there without the path separator
        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.Assembly)]
        internal static string GetMachineRandomDirectory(string rootDir)
        {
            String[] nodes1 = GetFileDirectoryNames(rootDir + "*", "*", false);
            // First see if there is a new store 
            for (int i=0; i<nodes1.Length; ++i) {
                if (nodes1[i].Length == 12) {
                    String[] nodes2 = GetFileDirectoryNames(rootDir + nodes1[i] + "\\" + "*", "*", false);
                    for (int j=0; j<nodes2.Length; ++j) {
                        if (nodes2[j].Length == 12) {
                            return (nodes1[i] +  "\\" + nodes2[j]); // Get the first directory
                        }
                    }
                }
            }

            // No store has been encountered, return null
            return null;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal static Mutex CreateMutexNotOwned(string pathName)
        {
            return new Mutex(false, "Global\\" + GetStrongHashSuitableForObjectName(pathName));
        }

        internal static String GetStrongHashSuitableForObjectName(string name)
        {
            MemoryStream ms  = new MemoryStream();
            new BinaryWriter(ms).Write(name.ToUpper(CultureInfo.InvariantCulture));
            ms.Position = 0;
#if !FEATURE_PAL
            return Path.ToBase32StringSuitableForDirName(new SHA1CryptoServiceProvider().ComputeHash(ms));
#else
            return GetHash(ms);
#endif // !FEATURE_PAL
        }
        private String GetSyncObjectName()
        {
            if (m_SyncObjectName == null)
            {
                // Don't take a lock here,  ok to create multiple times
                m_SyncObjectName = GetStrongHashSuitableForObjectName(m_InfoFile);
            }
            return m_SyncObjectName;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal void Lock(ref bool locked)
        {
            locked = false;
            
            if (IsRoaming())     // don't lock Roaming stores
                return;

            lock (m_internalLock)
            {
                if (m_bDisposed)
                    throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

                if (m_closed)
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "IsolatedStorage_StoreNotOpen"));

                if (InvalidFileHandle)
                    m_handle = Open(m_InfoFile, GetSyncObjectName());

                // Lock(handle, true) puts us into a critical region, so we don't need to call Thread.BeginCriticalRegion()
                locked = Lock(m_handle, true);
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal void Unlock()
        {
            if (IsRoaming())     // don't lock Roaming stores
                return;

            lock (m_internalLock)
            {
                if (m_bDisposed)
                    throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));

                if (m_closed)
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "IsolatedStorage_StoreNotOpen"));

                if (InvalidFileHandle)
                    m_handle = Open(m_InfoFile, GetSyncObjectName());

                // Lock(handle, false) ends the thread affinity and critical region created by
                // the corresponding Lock(handle, true)
                Lock(m_handle, false);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static FileIOPermission GetGlobalFileIOPerm(
                IsolatedStorageScope scope)
        {
            if (IsRoaming(scope))
            {
                // no sync needed, ok to create multiple instances.
                if (s_PermRoaming == null)
                {
                    s_PermRoaming =  new FileIOPermission(
                        FileIOPermissionAccess.AllAccess, GetRootDir(scope));
                }

                return s_PermRoaming;
            }

            if (IsMachine(scope))
            {
                // no sync needed, ok to create multiple instances.
                if (s_PermMachine == null)
                {
                    s_PermMachine =  new FileIOPermission(
                        FileIOPermissionAccess.AllAccess, GetRootDir(scope));
                }

                return s_PermMachine;
            }
            // no sync needed, ok to create multiple instances.
            if (s_PermUser == null)
            {
                s_PermUser =  new FileIOPermission(
                    FileIOPermissionAccess.AllAccess, GetRootDir(scope));
            }

            return s_PermUser;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static void DemandAdminPermission()
        {
            // Ok if more than one instance is created, no need to sync.
            if (s_PermAdminUser == null)
            {
                s_PermAdminUser = new IsolatedStorageFilePermission(
                    IsolatedStorageContainment.AdministerIsolatedStorageByUser,
                        0, false);
            }

            s_PermAdminUser.Demand();
        }

        internal static void VerifyGlobalScope(IsolatedStorageScope scope)
        {
            if ((scope != IsolatedStorageScope.User) && 
                (scope != (IsolatedStorageScope.User|
                          IsolatedStorageScope.Roaming)) &&
                (scope != IsolatedStorageScope.Machine))
            {
                throw new ArgumentException(
                    Environment.GetResourceString(
                        "IsolatedStorage_Scope_U_R_M"));
            }
        }

#if false
    // Not being used right now
        internal void CreateIDFileIfNecessary(String path, IsolatedStorageScope scope)
        {
            FileInfo fi = new FileInfo(path + s_IDFile);

            if ((fi.Exists) && (fi.Length != 0))
                return;

            CreateIDFile(path, scope);
        }
#endif

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        [System.Security.SecuritySafeCritical]
        internal void CreateIDFile(String path, IsolatedStorageScope scope)
        {
            try {
                // the default DACL is fine here since we've already set it on the root
                using(FileStream fs = new FileStream(path + s_IDFile, FileMode.OpenOrCreate)) {                    
#if FEATURE_SERIALIZATION
                    MemoryStream s = GetIdentityStream(scope);
                    byte[] b = s.GetBuffer();
                    fs.Write(b, 0, (int)s.Length);
                    s.Close();
#else
                    UTF8Encoding e = new System.Text.UTF8Encoding();
                    byte[] b = e.GetBytes(m_Id);
                    fs.Write(b, 0, b.Length);
#endif
                }

            } catch {
                // OK to ignore. It is possible that another thread / process
                // is writing to this file with the same data.
            }
        }

        // From IO.Directory class (make that internal if possible)
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly)]
        internal static String[] GetFileDirectoryNames(String path, String userSearchPattern, bool file)
        {
            if (path==null)
                throw new ArgumentNullException("path", Environment.GetResourceString("ArgumentNull_Path"));
            Contract.EndContractBlock();

            int hr;
            // we've already tacked original user search pattern onto path and we can't update that. 
            // this just helps corner cases
            userSearchPattern = NormalizeSearchPattern(userSearchPattern);
            if (userSearchPattern.Length == 0)
                return new String[0];
            
            bool fEndsWithDirectory = false;
            char lastChar = path[path.Length-1];
            if (lastChar == Path.DirectorySeparatorChar || 
                lastChar == Path.AltDirectorySeparatorChar || 
                lastChar == '.')
                fEndsWithDirectory = true;
                

            // Get an absolute path and do a security check
            String fullPath = LongPath.NormalizePath(path);

            // GetFullPath() removes '\', "\." etc from path, we will restore 
            // it here. If path ends in a trailing slash (\), append a * 
            // or we'll  get a "Cannot find the file specified" exception
            if ((fEndsWithDirectory) && 
                (fullPath[fullPath.Length - 1] != lastChar))
               fullPath += "\\*";

            // Check for read permission to the directory, not to the contents.
            String dir = LongPath.GetDirectoryName(fullPath);

            if (dir != null)
                dir += "\\";
    
            try 
            {
                String[] demandPath = new String[1];
                demandPath[0] = dir == null ? fullPath : dir;
                new FileIOPermission(FileIOPermissionAccess.Read, demandPath, false, false).Demand();
            }
            catch
            {
                throw new IsolatedStorageException(
                    Environment.GetResourceString(
                        "IsolatedStorage_Operation"));
            }

            
    
            String[] list = new String[10];
            int listSize = 0;
            Win32Native.WIN32_FIND_DATA data = new Win32Native.WIN32_FIND_DATA();
                    
            // Open a Find handle 
            SafeFindHandle hnd = Win32Native.FindFirstFile(Path.AddLongPathPrefix(fullPath), data);
            if (hnd.IsInvalid) {
                // Calls to GetLastWin32Error overwrites HResult.  Store HResult.
                hr = Marshal.GetLastWin32Error();
                if (hr==Win32Native.ERROR_FILE_NOT_FOUND)
                    return new String[0];
                __Error.WinIOError(hr, userSearchPattern);
            }
    
            // Keep asking for more matching files, adding file names to list
            int numEntries = 0;  // Number of directory entities we see.
            do {
                bool includeThis;  // Should this file/directory be included in the output?
                if (file)
                    includeThis = (0==(data.dwFileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY));
                else {
                    includeThis = (0!=(data.dwFileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY));
                    // Don't add "." nor ".."
                    if (includeThis && (data.cFileName.Equals(".") || data.cFileName.Equals(".."))) 
                        includeThis = false;
                }
                
                if (includeThis) {
                    numEntries++;
                    if (listSize==list.Length) {
                        Array.Resize(ref list, 2 * list.Length);
                    }
                    list[listSize++] = data.cFileName;
                }
     
            } while (Win32Native.FindNextFile(hnd, data));
            
            // Make sure we quit with a sensible error.
            hr = Marshal.GetLastWin32Error();
            hnd.Close();  // Close Find handle in all cases.
            if (hr!=0 && hr!=Win32Native.ERROR_NO_MORE_FILES)
                __Error.WinIOError(hr, userSearchPattern);
            
            // Check for a string such as "C:\tmp", in which case we return
            // just the directory name.  FindNextFile fails first time, and
            // data still contains a directory.
            if (!file && numEntries==1 && (0!=(data.dwFileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY))) {
                String[] sa = new String[1];
                sa[0] = data.cFileName;
                return sa;
            }
            
            // Return list of files/directories as an array of strings
            if (listSize == list.Length)
                return list;
            Array.Resize(ref list, listSize);
            return list;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None)]
        internal static extern ulong GetUsage(SafeIsolatedStorageFileHandle handle);

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeIsolatedStorageFileHandle Open(String infoFile, String syncName);

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None)]
        internal static extern void Reserve(SafeIsolatedStorageFileHandle        handle, 
                                            ulong                                plQuota,
                                            ulong                                plReserve,
                                            [MarshalAs(UnmanagedType.Bool)] bool fFree);

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.Machine)]
        internal static extern void GetRootDir(IsolatedStorageScope scope, StringHandleOnStack retRootDir);

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool Lock(SafeIsolatedStorageFileHandle        handle, 
                                         [MarshalAs(UnmanagedType.Bool)] bool fLock);

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.Machine)]
        internal static extern void CreateDirectoryWithDacl(string path);

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.Machine)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetQuota(SafeIsolatedStorageFileHandle scope, out long quota);

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.Machine)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern void SetQuota(SafeIsolatedStorageFileHandle scope, long quota);

    }

    internal sealed class IsolatedStorageFileEnumerator : IEnumerator
    {
#if !FEATURE_PAL
        private const char s_SepExternal = '\\';
#else
        private static readonly char s_SepExternal = System.IO.Path.DirectorySeparatorChar;
#endif  // !FEATURE_PAL

        private IsolatedStorageFile  m_Current;
        private IsolatedStorageScope m_Scope;
        private FileIOPermission     m_fiop;
        private String               m_rootDir;

        private TwoLevelFileEnumerator  m_fileEnum;
        private bool                    m_fReset;
        private bool                    m_fEnd;


        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal IsolatedStorageFileEnumerator(IsolatedStorageScope scope) 
        {
            m_Scope    = scope;
            m_fiop     = IsolatedStorageFile.GetGlobalFileIOPerm(scope);
            m_rootDir  = IsolatedStorageFile.GetRootDir(scope);
            m_fileEnum = new TwoLevelFileEnumerator(m_rootDir);
            Reset();
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.Machine | ResourceScope.Assembly)]
        public bool MoveNext()
        {
            IsolatedStorageFile  isf;
            IsolatedStorageScope scope;
            bool     fDomain;
            TwoPaths tp;
            Stream   domain, assem, app;
            String   domainName, assemName, appName;

            m_fiop.Assert();

            m_fReset = false;

            do {

                if (m_fileEnum.MoveNext() == false)
                {
                    m_fEnd = true;
                    break;
                }

                // Create the store
                isf = new IsolatedStorageFile();
    
                tp   = (TwoPaths) m_fileEnum.Current;
                fDomain = false;

                if (IsolatedStorageFile.NotAssemFilesDir(tp.Path2) &&
                    IsolatedStorageFile.NotAppFilesDir(tp.Path2))
                    fDomain = true;

                // Create Roaming Store
                domain   = null; 
                assem = null;
                app = null;

                if (fDomain)
                {
                    if (!GetIDStream(tp.Path1, out domain))
                        continue;

                    if (!GetIDStream(tp.Path1 + s_SepExternal + tp.Path2, 
                            out assem))
                        continue;

                    domain.Position = 0;

                    if (IsolatedStorage.IsRoaming(m_Scope))
                        scope = IsolatedStorage.c_DomainRoaming;
                    else if (IsolatedStorage.IsMachine(m_Scope))
                        scope = IsolatedStorage.c_MachineDomain;
                    else
                        scope = IsolatedStorage.c_Domain;

                    domainName = tp.Path1;
                    assemName = tp.Path2;
                    appName = null;
                }
                else
                {
                    if (IsolatedStorageFile.NotAppFilesDir(tp.Path2))
                    {
                        // Assembly
                        if (!GetIDStream(tp.Path1, out assem))
                            continue;

                        if (IsolatedStorage.IsRoaming(m_Scope))
                            scope = IsolatedStorage.c_AssemblyRoaming;
                        else if(IsolatedStorage.IsMachine(m_Scope))
                            scope = IsolatedStorage.c_MachineAssembly;
                        else
                            scope = IsolatedStorage.c_Assembly;

                        domainName   = null;
                        assemName = tp.Path1;
                        appName = null;
                        assem.Position = 0;                        
                    }
                    else
                    {
                        // Application
                        if (!GetIDStream(tp.Path1, out app))
                            continue;

                        if (IsolatedStorage.IsRoaming(m_Scope))
                            scope = IsolatedStorage.c_AppUserRoaming;
                        else if(IsolatedStorage.IsMachine(m_Scope))
                            scope = IsolatedStorage.c_AppMachine;
                        else
                            scope = IsolatedStorage.c_AppUser;

                        domainName   = null;
                        assemName = null;
                        appName = tp.Path1;
                        app.Position = 0;
                    }
                        
                }

                if (!isf.InitStore(scope, domain, assem, app, domainName, assemName, appName))
                    continue;
                
                if (!isf.InitExistingStore(scope))
                    continue;

                m_Current = isf;

                return true;

            } while (true);
            return false;
        }

        public Object Current 
        {
            [ResourceExposure(ResourceScope.Machine | ResourceScope.Assembly)]
            get { 

                if (m_fReset)
                {
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_EnumNotStarted"));
                }
                else if (m_fEnd)
                {
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_EnumEnded"));
                }
    
                return (Object) m_Current; 
            }
        }

        public void Reset()
        {
            m_Current = null;
            m_fReset  = true;
            m_fEnd    = false;
            m_fileEnum.Reset();
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private bool GetIDStream(String path, out Stream s)
        {
            StringBuilder sb = new StringBuilder();
            byte[]        b;

            sb.Append(m_rootDir);
            sb.Append(path);
            sb.Append(s_SepExternal);
            sb.Append(IsolatedStorageFile.s_IDFile);

            s = null;

            try {
                using(FileStream fs = new FileStream(sb.ToString(), FileMode.Open)) {
                    int length = (int) fs.Length;
                    b = new byte[length];
                    int offset = 0;
                    while(length > 0) {
                        int n = fs.Read(b, offset, length);
                        if (n == 0)
                            __Error.EndOfFile();
                        offset += n;
                        length -= n;
                    }
                }
                s = new MemoryStream(b);
            } catch {
                return false;
            }                    

            return true;
        }
    }

    [System.Security.SecurityCritical]  // auto-generated
    internal sealed class SafeIsolatedStorageFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None),
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern void Close(IntPtr file);

        private SafeIsolatedStorageFileHandle() : base(true)
        {
            SetHandle(IntPtr.Zero);
            return;
        }

        [System.Security.SecurityCritical]
        protected override bool ReleaseHandle()
        {
            Close(handle);
            return true;
        }
    }


    internal sealed class TwoPaths
    {
        public String Path1;
        public String Path2;

    }

    // Given a directory, enumerates all subdirs of upto depth 2
    internal sealed class TwoLevelFileEnumerator : IEnumerator
    {
        private String   m_Root;
        private TwoPaths m_Current;
        private bool     m_fReset;
    
        private String[] m_RootDir;
        private int      m_nRootDir;
    
        private String[] m_SubDir;
        private int      m_nSubDir;
    
    
        public TwoLevelFileEnumerator(String root)
        {
            m_Root = root;
            Reset();
        }

        public bool MoveNext()
        {
            lock (this)
            {
                // Sepecial case the Reset State
                if (m_fReset)
                {
                    m_fReset = false;
                    return AdvanceRootDir();
                }
        
                // Don't move anything if RootDir is empty
                if (m_RootDir.Length == 0)
                    return false;
    
    
                // Get Next SubDir
    
                ++m_nSubDir;
        
                if (m_nSubDir >= m_SubDir.Length)
                {
                    m_nSubDir = m_SubDir.Length;    // to avoid wrap aournd.
                    return AdvanceRootDir();
                }
    
                UpdateCurrent();
            }
    
            return true;
        }
    

        [ResourceExposure(ResourceScope.None)]  // Scoping should be done when opening isolated storage.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private bool AdvanceRootDir()
        {
            ++m_nRootDir;
    
            if (m_nRootDir >= m_RootDir.Length)
            {
                m_nRootDir = m_RootDir.Length;  // to prevent wrap around
                return false;                   // We are at the very end.
            }

            Contract.Assert(m_RootDir[m_nRootDir].Length < Path.MaxPath);
            m_SubDir = Directory.GetDirectories(m_RootDir[m_nRootDir]);

            if (m_SubDir.Length == 0)
                return AdvanceRootDir();        // recurse here.

            m_nSubDir  = 0;

            // Set m_Current
            UpdateCurrent();
    
            return true;
        }
    
        [ResourceExposure(ResourceScope.None)]  // Scoping should be done when opening isolated storage.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void UpdateCurrent()
        {
            m_Current.Path1 = Path.GetFileName(m_RootDir[m_nRootDir]);
            m_Current.Path2 = Path.GetFileName(m_SubDir[m_nSubDir]);
        }
    
        public Object Current
        {
            get {
    
                if (m_fReset)
                {
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_EnumNotStarted"));
                }
                else if (m_nRootDir >= m_RootDir.Length)
                {
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_EnumEnded"));
                }
    
                return (Object) m_Current; 
            }
        }
    
        [ResourceExposure(ResourceScope.None)]  // Scoping should be done when opening isolated storage.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void Reset()
        {
            m_RootDir  = null;
            m_nRootDir = -1;
    
            m_SubDir   = null;
            m_nSubDir  = -1;
    
            m_Current  = new TwoPaths();
            m_fReset   = true;

            Contract.Assert(m_Root.Length < Path.MaxPath);
            m_RootDir = Directory.GetDirectories(m_Root);
        }
    }
}
#endif // !FEATURE_ISOSTORE_LIGHT
