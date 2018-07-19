//------------------------------------------------------------------------------
// <copyright file="FileVersionInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using Microsoft.Win32;
    using System.Runtime.Serialization.Formatters;
    using System.Text;
    using System.Configuration.Assemblies;
    using System.Runtime.InteropServices;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System;
    using System.Globalization;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///    <para>Provides version information for a physical file on disk.</para>
    /// </devdoc>
    [
    // Disabling partial trust scenarios
    PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")
    ]
    public sealed class FileVersionInfo {

        private string fileName;
        private string companyName;
        private string fileDescription;
        private string fileVersion;
        private string internalName;
        private string legalCopyright;
        private string originalFilename;
        private string productName;
        private string productVersion;
        private string comments;
        private string legalTrademarks;
        private string privateBuild;
        private string specialBuild;
        private string language;
        private int fileMajor;
        private int fileMinor;
        private int fileBuild;
        private int filePrivate;
        private int productMajor;
        private int productMinor;
        private int productBuild;
        private int productPrivate;
        private int fileFlags;

        private FileVersionInfo(string fileName) {
            this.fileName = fileName;
        }

        /// <devdoc>
        ///    <para>Gets the comments associated with the file.</para>
        /// </devdoc>
        public string Comments {
            get {
                return comments;
            }
        }

        /// <devdoc>
        ///    <para>Gets the name of the company that produced the file.</para>
        /// </devdoc>
        public string CompanyName {
            get {
                return companyName;
            }
        }

        /// <devdoc>
        ///    <para>Gets the build number of the file.</para>
        /// </devdoc>
        public int FileBuildPart {
            get {
                return fileBuild;
            }
        }

        /// <devdoc>
        ///    <para>Gets the description of the file.</para>
        /// </devdoc>
        public string FileDescription {
            get {
                return fileDescription;
            }
        }

        /// <devdoc>
        ///    <para>Gets the major part of the version number.</para>
        /// </devdoc>
        public int FileMajorPart {
            get {
                return fileMajor;
            }
        }

        /// <devdoc>
        ///    <para>Gets the minor
        ///       part of the version number of the file.</para>
        /// </devdoc>
        public int FileMinorPart {
            get {
                return fileMinor;
            }
        }

        /// <devdoc>
        ///    <para>Gets the name of the file that this instance of System.Windows.Forms.FileVersionInfo
        ///       describes.</para>
        /// </devdoc>
        public string FileName {
            get {
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fileName).Demand();
                return fileName;
            }
        }

        /// <devdoc>
        ///    <para>Gets the file private part number.</para>
        /// </devdoc>
        public int FilePrivatePart {
            get {
                return filePrivate;
            }
        }

        /// <devdoc>
        ///    <para>Gets the file version number.</para>
        /// </devdoc>
        public string FileVersion {
            get {
                return fileVersion;
            }
        }

        /// <devdoc>
        ///    <para>Gets the internal name of the file, if one exists.</para>
        /// </devdoc>
        public string InternalName {
            get {
                return internalName;
            }
        }

        /// <devdoc>
        ///    <para>Gets a value that specifies whether the file
        ///       contains debugging information or is compiled with debugging features enabled.</para>
        /// </devdoc>
        public bool IsDebug {
            get {
                return (fileFlags & NativeMethods.VS_FF_DEBUG) != 0;
            }
        }

        /// <devdoc>
        ///    <para>Gets a value that specifies whether the file has been modified and is not identical to
        ///       the original shipping file of the same version number.</para>
        /// </devdoc>
        public bool IsPatched {
            get {
                return (fileFlags & NativeMethods.VS_FF_PATCHED) != 0;
            }
        }

        /// <devdoc>
        ///    <para>Gets a value that specifies whether the file was built using standard release procedures.</para>
        /// </devdoc>
        public bool IsPrivateBuild {
            get {
                return (fileFlags & NativeMethods.VS_FF_PRIVATEBUILD) != 0;
            }
        }

        /// <devdoc>
        ///    <para>Gets a value that specifies whether the file
        ///       is a development version, rather than a commercially released product.</para>
        /// </devdoc>
        public bool IsPreRelease {
            get {
                return (fileFlags & NativeMethods.VS_FF_PRERELEASE) != 0;
            }
        }

        /// <devdoc>
        ///    <para>Gets a value that specifies whether the file is a special build.</para>
        /// </devdoc>
        public bool IsSpecialBuild {
            get {
                return (fileFlags & NativeMethods.VS_FF_SPECIALBUILD) != 0;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the default language string for the version info block.
        ///    </para>
        /// </devdoc>
        public string Language {
            get {
                return language;
            }
        }
        
        /// <devdoc>
        ///    <para>Gets all copyright notices that apply to the specified file.</para>
        /// </devdoc>
        public string LegalCopyright {
            get {
                return legalCopyright;
            }
        }

        /// <devdoc>
        ///    <para>Gets the trademarks and registered trademarks that apply to the file.</para>
        /// </devdoc>
        public string LegalTrademarks {
            get {
                return legalTrademarks;
            }
        }

        /// <devdoc>
        ///    <para>Gets the name the file was created with.</para>
        /// </devdoc>
        public string OriginalFilename {
            get {
                return originalFilename;
            }
        }

        /// <devdoc>
        ///    <para>Gets information about a private version of the file.</para>
        /// </devdoc>
        public string PrivateBuild {
            get {
                return privateBuild;
            }
        }

        /// <devdoc>
        ///    <para>Gets the build number of the product this file is associated with.</para>
        /// </devdoc>
        public int ProductBuildPart {
            get {
                return productBuild;
            }
        }

        /// <devdoc>
        ///    <para>Gets the major part of the version number for the product this file is associated with.</para>
        /// </devdoc>
        public int ProductMajorPart {
            get {
                return productMajor;
            }
        }

        /// <devdoc>
        ///    <para>Gets the minor part of the version number for the product the file is associated with.</para>
        /// </devdoc>
        public int ProductMinorPart {
            get {
                return productMinor;
            }
        }

        /// <devdoc>
        ///    <para>Gets the name of the product this file is distributed with.</para>
        /// </devdoc>
        public string ProductName {
            get {
                return productName;
            }
        }

        /// <devdoc>
        ///    <para>Gets the private part number of the product this file is associated with.</para>
        /// </devdoc>
        public int ProductPrivatePart {
            get {
                return productPrivate;
            }
        }

        /// <devdoc>
        ///    <para>Gets the version of the product this file is distributed with.</para>
        /// </devdoc>
        public string ProductVersion {
            get {
                return productVersion;
            }
        }

        /// <devdoc>
        ///    <para>Gets the special build information for the file.</para>
        /// </devdoc>
        public string SpecialBuild {
            get {
                return specialBuild;
            }
        }

        private static string ConvertTo8DigitHex(int value) {
            string s = Convert.ToString(value, 16);
            s = s.ToUpper(CultureInfo.InvariantCulture);           
            if (s.Length == 8) {
                return s;
            }
            else {
                StringBuilder b = new StringBuilder(8);
                for (int l = s.Length;l<8; l++) {
                    b.Append("0");
                }
                b.Append(s);
                return b.ToString();
            }
        }
        
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private static NativeMethods.VS_FIXEDFILEINFO GetFixedFileInfo(IntPtr memPtr) {
            IntPtr memRef = IntPtr.Zero;
            int memLen;

            if (UnsafeNativeMethods.VerQueryValue(new HandleRef(null, memPtr), "\\", ref memRef, out memLen)) {
                NativeMethods.VS_FIXEDFILEINFO fixedFileInfo = new NativeMethods.VS_FIXEDFILEINFO();
                Marshal.PtrToStructure(memRef, fixedFileInfo);
                return fixedFileInfo;
            }

            return new NativeMethods.VS_FIXEDFILEINFO();
        }

        private static string GetFileVersionLanguage( IntPtr memPtr ) {
            int langid = GetVarEntry( memPtr ) >> 16;
            
            StringBuilder lang = new StringBuilder( 256 );
            UnsafeNativeMethods.VerLanguageName( langid, lang, lang.Capacity );
            return lang.ToString();
        }
        
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private static string GetFileVersionString(IntPtr memPtr, string name) {
            string data = "";

            IntPtr memRef = IntPtr.Zero;
            int memLen;

            if (UnsafeNativeMethods.VerQueryValue(new HandleRef(null, memPtr), name, ref memRef, out memLen)) {

                if (memRef != IntPtr.Zero) {
                    data = Marshal.PtrToStringAuto(memRef);
                }
            }
            return data;
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private static int GetVarEntry(IntPtr memPtr) {
            IntPtr memRef = IntPtr.Zero;
            int memLen;

            if (UnsafeNativeMethods.VerQueryValue(new HandleRef(null, memPtr), "\\VarFileInfo\\Translation", ref memRef, out memLen)) {
                return(Marshal.ReadInt16(memRef) << 16) + Marshal.ReadInt16((IntPtr)((long)memRef + 2));
            }

            return 0x040904E4;
        }

        // 
        // This function tries to find version informaiton for a specific codepage.
        // Returns true when version information is found.
        //
        private bool GetVersionInfoForCodePage(IntPtr memIntPtr, string codepage) {
            string template = "\\\\StringFileInfo\\\\{0}\\\\{1}";

            companyName = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "CompanyName"));
            fileDescription = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "FileDescription"));
            fileVersion = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "FileVersion"));
            internalName = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "InternalName"));
            legalCopyright = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "LegalCopyright"));
            originalFilename = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "OriginalFilename"));
            productName = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "ProductName"));
            productVersion = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "ProductVersion"));
            comments = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "Comments"));
            legalTrademarks = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "LegalTrademarks"));
            privateBuild = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "PrivateBuild"));
            specialBuild = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, template, codepage, "SpecialBuild"));

            language = GetFileVersionLanguage( memIntPtr );

            NativeMethods.VS_FIXEDFILEINFO ffi = GetFixedFileInfo(memIntPtr);
            fileMajor = HIWORD(ffi.dwFileVersionMS);
            fileMinor = LOWORD(ffi.dwFileVersionMS);
            fileBuild = HIWORD(ffi.dwFileVersionLS);
            filePrivate = LOWORD(ffi.dwFileVersionLS);
            productMajor = HIWORD(ffi.dwProductVersionMS);
            productMinor = LOWORD(ffi.dwProductVersionMS);
            productBuild = HIWORD(ffi.dwProductVersionLS);
            productPrivate = LOWORD(ffi.dwProductVersionLS);
            fileFlags = ffi.dwFileFlags;
            
            // fileVersion is chosen based on best guess. Other fields can be used if appropriate. 
            return (fileVersion != string.Empty);
        }

        //
        // Get the full path of fileName using a declarative Assert.
        //
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        static string GetFullPathWithAssert(string fileName) {
            return Path.GetFullPath(fileName);
        }

        /// <devdoc>
        /// <para>Returns a System.Windows.Forms.FileVersionInfo representing the version information associated with the specified file.</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public unsafe static FileVersionInfo GetVersionInfo(string fileName) {
            // Check for the existence of the file. File.Exists returns false
            // if Read permission is denied.
            if (!File.Exists(fileName)) {
                // 
                // The previous version of this code in the success case would require 
                // one imperative Assert for PathDiscovery permission, one Demand for 
                // PathDiscovery permission (blocked by the Assert), and 2 demands for
                // Read permission. It turns out that File.Exists does a demand for 
                // Read permission, so in the success case, we only need to do a single Demand. 
                // In the success case, this change increases the performance of this
                // function dramatically.
                // 
                // In the failure case, we want to remain backwardly compatible by throwing 
                // a SecurityException in the case where Read access is denied 
                // (it can be argued that this is less secure than throwing a FileNotFoundException, 
                // but perhaps not so much as to be worth a breaking change).
                // File.Exists eats a SecurityException, so we need to Demand for it
                // here. Since performance in the failure case is not crucial, as an
                // exception will be thrown anyway, we do a Demand for Read access.
                // If that does not throw an exception, then we will throw a FileNotFoundException.
                //
                // We also change the code to do a declarative Assert for PathDiscovery,
                // as that performs much better than an imperative Assert.
                //
                string fullPath = GetFullPathWithAssert(fileName);
                new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
                throw new FileNotFoundException(fileName);
            }

            int handle;  // This variable is not used, but we need an out variable.
            int infoSize = UnsafeNativeMethods.GetFileVersionInfoSize(fileName, out handle);
            FileVersionInfo versionInfo = new FileVersionInfo(fileName);

            if (infoSize != 0) {
                byte[] mem = new byte[infoSize];
                fixed (byte* memPtr = mem) {
                    IntPtr memIntPtr = new IntPtr((void*) memPtr);                    
                    if (UnsafeNativeMethods.GetFileVersionInfo(fileName, 0, infoSize, new HandleRef(null, memIntPtr))) {
                        int langid = GetVarEntry(memIntPtr);
                        if( !versionInfo.GetVersionInfoForCodePage(memIntPtr, ConvertTo8DigitHex(langid))) {
                            // Some dlls might not contain correct codepage information. In this case we will fail during lookup. 
                            // Explorer will take a few shots in dark by trying following ID:
                            //
                            // 040904B0 // US English + CP_UNICODE
                            // 040904E4 // US English + CP_USASCII
                            // 04090000 // US English + unknown codepage
                            // Explorer also randomly guess 041D04B0=Swedish+CP_UNICODE and 040704B0=German+CP_UNICODE) sometimes.
                            // We will try to simulate similiar behavior here.            
                            int[] ids = new int[] {0x040904B0, 0x040904E4, 0x04090000};
                            foreach( int id in ids) {
                                if( id != langid) { 
                                    if(versionInfo.GetVersionInfoForCodePage(memIntPtr, ConvertTo8DigitHex(id))) {
                                        break;
                                    }
                                }                               
                            }
                        }

                    }
                }
            }
            return versionInfo;         
        }

        private static int HIWORD(int dword) {
            return NativeMethods.Util.HIWORD(dword);
        }

        private static int LOWORD(int dword) {
            return NativeMethods.Util.LOWORD(dword);
        }

        /// <devdoc>
        /// <para>Returns a partial list of properties in System.Windows.Forms.FileVersionInfo
        /// and their values.</para>
        /// </devdoc>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(128);
            String nl = "\r\n";
            sb.Append("File:             ");   sb.Append(FileName);   sb.Append(nl);
            sb.Append("InternalName:     ");   sb.Append(InternalName);   sb.Append(nl);
            sb.Append("OriginalFilename: ");   sb.Append(OriginalFilename);   sb.Append(nl);
            sb.Append("FileVersion:      ");   sb.Append(FileVersion);   sb.Append(nl);
            sb.Append("FileDescription:  ");   sb.Append(FileDescription);   sb.Append(nl);
            sb.Append("Product:          ");   sb.Append(ProductName);   sb.Append(nl);
            sb.Append("ProductVersion:   ");   sb.Append(ProductVersion);   sb.Append(nl);
            sb.Append("Debug:            ");   sb.Append(IsDebug.ToString());   sb.Append(nl);
            sb.Append("Patched:          ");   sb.Append(IsPatched.ToString());   sb.Append(nl);
            sb.Append("PreRelease:       ");   sb.Append(IsPreRelease.ToString());   sb.Append(nl);
            sb.Append("PrivateBuild:     ");   sb.Append(IsPrivateBuild.ToString());   sb.Append(nl);
            sb.Append("SpecialBuild:     ");   sb.Append(IsSpecialBuild.ToString());   sb.Append(nl);
            sb.Append("Language:         ");   sb.Append(Language);  sb.Append(nl);
            return sb.ToString();
        }

    }
}
