//------------------------------------------------------------------------------
// <copyright file="RemoteWebConfigurationHostServer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Collections;
    using System.Configuration;
    using System.Security;
    using System.IO;
    using System.Globalization;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Web.Util;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Security.Cryptography;
#if !FEATURE_PAL // FEATURE_PAL does not enable access control
    using System.Security.AccessControl;
#endif // !FEATURE_PAL
    using System.Security.Permissions;


#if !FEATURE_PAL // FEATURE_PAL does not enable COM
    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual),
#if WIN64
    Guid("DFD0D215-72C0-450d-92B5-10971FC24625"),  ProgId("System.Web.Configuration.RemoteWebConfigurationHostServerV4_64")]
#else
    Guid("9FDB6D2C-90EA-4e42-99E6-38B96E28698E"), ProgId("System.Web.Configuration.RemoteWebConfigurationHostServerV4_32")]
#endif

#endif // FEATURE_PAL does not enable COM
    [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
    public class RemoteWebConfigurationHostServer : IRemoteWebConfigurationHostServer
    {
        internal const char             FilePathsSeparatorChar = '<';
        static internal readonly char[] FilePathsSeparatorParams = new char[] {FilePathsSeparatorChar};

        public byte[] GetData(string fileName, bool getReadTimeOnly, out long readTime)
        {
            if (!fileName.ToLowerInvariant().EndsWith(".config", StringComparison.Ordinal))
                throw new Exception(SR.GetString(SR.Can_not_access_files_other_than_config));

            byte [] buf;
            if (File.Exists(fileName)) {
                if (getReadTimeOnly) {
                    buf = new byte[0];
                }
                else {
                    buf = File.ReadAllBytes(fileName);
                }
                DateTime lastWrite = File.GetLastWriteTimeUtc(fileName);
                readTime = (DateTime.UtcNow > lastWrite ? DateTime.UtcNow.Ticks : lastWrite.Ticks);
            } else {
                buf = new byte[0];
                readTime = DateTime.UtcNow.Ticks;
            }
            return buf;
        }

        public void WriteData(string fileName, string templateFileName, byte[] data, ref long readTime)
        {
            if (!fileName.ToLowerInvariant().EndsWith(".config", StringComparison.Ordinal))
                throw new Exception(SR.GetString(SR.Can_not_access_files_other_than_config));

            bool            fileExists          = File.Exists(fileName);
            FileInfo        fileInfo            = null;
            FileAttributes  fileAttributes      = FileAttributes.Normal;
            string          tempFile            = null;
            Exception       createStreamExcep   = null;
            FileStream      tempFileStream      = null;
            long            lastWriteTicks      = 0;
            long            utcNowTicks         = 0;

            /////////////////////////////////////////////////////////////////////
            // Step 1: If the file exists, then make sure it hasn't been written to since it was read
            if (fileExists && File.GetLastWriteTimeUtc(fileName).Ticks > readTime) {
                throw new Exception(SR.GetString(SR.File_changed_since_read, fileName));
            }

            /////////////////////////////////////////////////////////////////////
            // Step 2: Get the security-descriptor and attributes of the file
            if (fileExists) {
                try {
                    fileInfo = new FileInfo(fileName);
                    fileAttributes = fileInfo.Attributes;
                } catch { }
                if (((int)(fileAttributes & (FileAttributes.ReadOnly | FileAttributes.Hidden))) != 0)
                    throw new Exception(SR.GetString(SR.File_is_read_only, fileName));
            }

            /////////////////////////////////////////////////////////////////////
            // Step 3: Generate a temp file name. Make sure that the temp file doesn't exist
            tempFile = fileName + "." + GetRandomFileExt() + ".tmp";
            for (int iter = 0; File.Exists(tempFile); iter++) { // if it exists, then use a different random name
                if (iter > 100) // don't try more than 100 times
                    throw new Exception(SR.GetString(SR.Unable_to_create_temp_file));
                else
                    tempFile = fileName + "." + GetRandomFileExt() + ".tmp";
            }

            /////////////////////////////////////////////////////////////////////
            // Step 4: Write the buffer to the temp file, and move it to the actual file
            try {
                tempFileStream = new FileStream(tempFile, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite, data.Length, FileOptions.WriteThrough);
                tempFileStream.Write(data, 0, data.Length);
            } catch (Exception e) {
                createStreamExcep = e;
            } finally {
                if (tempFileStream != null)
                    tempFileStream.Close();
            }
            if (createStreamExcep != null) {
                try {
                    File.Delete(tempFile);
                } catch { }
                throw createStreamExcep;
            }
            if (fileExists) {
                try {
                    DuplicateFileAttributes(fileName, tempFile);
                } catch { }
            }
            else if ( templateFileName != null ) {
                try {
                    DuplicateTemplateAttributes(fileName, templateFileName);
                } catch { }
            }

            /////////////////////////////////////////////////////////////////////
            // Step 4: Move the temp filt to the actual file
            if (!UnsafeNativeMethods.MoveFileEx(tempFile, fileName, MOVEFILE_COPY_ALLOWED | MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH)) {
                try {
                    File.Delete(tempFile);
                } catch { }
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            /////////////////////////////////////////////////////////////////////
            // Step 5: Set the attributes of the file
            if (fileExists) {
                fileInfo = new FileInfo(fileName);
                fileInfo.Attributes = fileAttributes;
            }

            /////////////////////////////////////////////////////////////////////
            // Step 6: Record the current time as the read-time
            lastWriteTicks = File.GetLastWriteTimeUtc(fileName).Ticks;
            utcNowTicks = DateTime.UtcNow.Ticks;
            readTime = (utcNowTicks > lastWriteTicks ? utcNowTicks : lastWriteTicks);
        }

        public string GetFilePaths(int webLevelAsInt, string path, string site, string locationSubPath)
        {
            WebLevel webLevel = (WebLevel) webLevelAsInt;

            IConfigMapPath configMapPath = IISMapPath.GetInstance();

            // Get the configuration paths and application information
            string appSiteName, appSiteID;
            VirtualPath appPath;
            string configPath, locationConfigPath;
            WebConfigurationHost.GetConfigPaths(configMapPath, webLevel, VirtualPath.CreateNonRelativeAllowNull(path), site, locationSubPath,
                    out appPath, out appSiteName, out appSiteID, out configPath, out locationConfigPath);

            //
            // Format of filePaths:
            //      appPath < appSiteName < appSiteID < configPath < locationConfigPath [< configPath < fileName]+
            //
            ArrayList filePaths = new ArrayList();
            filePaths.Add(VirtualPath.GetVirtualPathString(appPath));
            filePaths.Add(appSiteName);
            filePaths.Add(appSiteID);
            filePaths.Add(configPath);
            filePaths.Add(locationConfigPath);

            string dummySiteID;
            VirtualPath virtualPath;
            WebConfigurationHost.GetSiteIDAndVPathFromConfigPath(configPath, out dummySiteID, out virtualPath);

            // pathmap for machine.config
            filePaths.Add(WebConfigurationHost.MachineConfigPath);
            filePaths.Add(HttpConfigurationSystem.MachineConfigurationFilePath);

            // pathmap for root web.config
            if (webLevel != WebLevel.Machine) {
                filePaths.Add(WebConfigurationHost.RootWebConfigPath);
                filePaths.Add(HttpConfigurationSystem.RootWebConfigurationFilePath);

                // pathmap for other paths
                for (VirtualPath currentVirtualPath = virtualPath; currentVirtualPath != null; currentVirtualPath = currentVirtualPath.Parent)
                {
                    string currentConfigPath = WebConfigurationHost.GetConfigPathFromSiteIDAndVPath(appSiteID, currentVirtualPath);
                    string currentFilePath = configMapPath.MapPath(appSiteID, currentVirtualPath.VirtualPathString);
                    currentFilePath = System.IO.Path.Combine(currentFilePath, HttpConfigurationSystem.WebConfigFileName);

                    filePaths.Add(currentConfigPath);
                    filePaths.Add(currentFilePath);
                }
            }

            // join into a single string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < filePaths.Count; i++) {
                if (i > 0) {
                    sb.Append(FilePathsSeparatorChar);
                }

                string part = (string) filePaths[i];
                sb.Append(part);
            }

            return sb.ToString();
        }

        public string DoEncryptOrDecrypt(bool doEncrypt, string xmlString, string protectionProviderName, string protectionProviderType, string[] paramKeys, string[] paramValues)
        {
            Type t = Type.GetType(protectionProviderType, true);
            if (!typeof(ProtectedConfigurationProvider).IsAssignableFrom(t)) {
                throw new Exception(SR.GetString(SR.WrongType_of_Protected_provider));
            }

            ProtectedConfigurationProvider  provider        = (ProtectedConfigurationProvider)Activator.CreateInstance(t);
            NameValueCollection             cloneParams     = new NameValueCollection(paramKeys.Length);
            XmlNode                         node;

            for(int iter=0; iter<paramKeys.Length; iter++)
                cloneParams.Add(paramKeys[iter], paramValues[iter]);

            provider.Initialize(protectionProviderName, cloneParams);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(xmlString);
            if (doEncrypt) {
                node = provider.Encrypt(xmlDocument.DocumentElement);
            } else {
                node = provider.Decrypt(xmlDocument.DocumentElement);
            }

            return node.OuterXml;
        }
        public void GetFileDetails(string name, out bool exists, out long size, out long createDate, out long lastWriteDate) {
            if (!name.ToLowerInvariant().EndsWith(".config", StringComparison.Ordinal))
                throw new Exception(SR.GetString(SR.Can_not_access_files_other_than_config));
            UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA data;
            if (UnsafeNativeMethods.GetFileAttributesEx(name, UnsafeNativeMethods.GetFileExInfoStandard, out data) && (data.fileAttributes & (int)FileAttributes.Directory) == 0) {
                exists = true;
                size = (long)(uint)data.fileSizeHigh << 32 | (long)(uint)data.fileSizeLow;
                createDate = (((long)data.ftCreationTimeHigh) << 32) | ((long)data.ftCreationTimeLow);
                lastWriteDate = (((long)data.ftLastWriteTimeHigh) << 32) | ((long)data.ftLastWriteTimeLow);
            } else {
                exists = false;
                size = 0;
                createDate = 0;
                lastWriteDate = 0;
            }
        }

        private static string GetRandomFileExt() {
            byte[] buf = new byte[2];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return buf[1].ToString("X", CultureInfo.InvariantCulture) + buf[0].ToString("X", CultureInfo.InvariantCulture);
        }

        private void DuplicateFileAttributes(string oldFileName, string newFileName) {
#if !FEATURE_PAL // FEATURE_PAL does not enable access control
            FileAttributes attributes;
            DateTime creationTime;

            // Copy File Attributes, ie. Hidden, Readonly, etc.
            attributes = File.GetAttributes(oldFileName);
            File.SetAttributes(newFileName, attributes);

            // Copy Creation Time
            creationTime = File.GetCreationTimeUtc(oldFileName);
            File.SetCreationTimeUtc(newFileName, creationTime);

            DuplicateTemplateAttributes( oldFileName, newFileName);
        }

        private void DuplicateTemplateAttributes(string oldFileName, string newFileName) {
            FileSecurity fileSecurity;

            // Copy Security information

            // If we don't have the privelege to get the Audit information,
            // then just persist the DACL
            try {
                fileSecurity = File.GetAccessControl(oldFileName,
                                                      AccessControlSections.Access |
                                                      AccessControlSections.Audit);

                // Mark dirty, so effective for write
                fileSecurity.SetAuditRuleProtection(fileSecurity.AreAuditRulesProtected, true);
            } catch (UnauthorizedAccessException) {
                fileSecurity = File.GetAccessControl(oldFileName,
                                                      AccessControlSections.Access);
            }

            // Mark dirty, so effective for write
            fileSecurity.SetAccessRuleProtection(fileSecurity.AreAccessRulesProtected, true);
            File.SetAccessControl(newFileName, fileSecurity);
#endif // !FEATURE_PAL
        }
        const int MOVEFILE_REPLACE_EXISTING = 0x00000001;
        const int MOVEFILE_COPY_ALLOWED           = 0x00000002;
        const int MOVEFILE_DELAY_UNTIL_REBOOT     = 0x00000004;
        const int MOVEFILE_WRITE_THROUGH          = 0x00000008;
    }
}
