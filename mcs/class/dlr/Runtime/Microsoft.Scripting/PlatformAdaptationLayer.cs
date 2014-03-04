/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Microsoft.Scripting {

#if !FEATURE_PROCESS
    public class ExitProcessException : Exception {

        public int ExitCode { get { return exitCode; } }
        int exitCode;

        public ExitProcessException(int exitCode) {
            this.exitCode = exitCode;
        }
    }
#endif

    /// <summary>
    /// Abstracts system operations that are used by DLR and could potentially be platform specific.
    /// The host can implement its PAL to adapt DLR to the platform it is running on.
    /// For example, the Silverlight host adapts some file operations to work against files on the server.
    /// </summary>
    [Serializable]
    public class PlatformAdaptationLayer {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlatformAdaptationLayer Default = new PlatformAdaptationLayer();

#if MONO_INTERPRETER
        public const bool IsCompactFramework = false;
#else
        public static readonly bool IsCompactFramework =
#if WIN8
            false;
#else
            Environment.OSVersion.Platform == PlatformID.WinCE ||
            Environment.OSVersion.Platform == PlatformID.Xbox;
#endif

#endif

#if SILVERLIGHT

        // this dictionary is readonly after initialization:
        private Dictionary<string, string> _assemblyFullNames = new Dictionary<string, string>();

        public PlatformAdaptationLayer() {
            LoadSilverlightAssemblyNameMapping();
        }

        // TODO: remove the need for this
        private void LoadSilverlightAssemblyNameMapping() {
            // non-trasparent assemblies
            AssemblyName platformKeyVer = new AssemblyName(typeof(object).Assembly.FullName);
            AddAssemblyMappings(platformKeyVer,
                "mscorlib",
                "System",
                "System.Core",
                "System.Net",
                "System.Runtime.Serialization",
                "System.ServiceModel.Web",
                "System.Windows",
                "System.Windows.Browser",
                "System.Xml",
                "Microsoft.VisualBasic"
            );

            // DLR + language assemblies
            AssemblyName languageKeyVer = new AssemblyName(typeof(PlatformAdaptationLayer).Assembly.FullName);
            AddAssemblyMappings(languageKeyVer, 
                "Microsoft.Scripting",
                "Microsoft.Dynamic",
                "Microsoft.Scripting.Core",
                "Microsoft.Scripting.Silverlight",
                "IronPython",
                "IronPython.Modules",
                "IronRuby",
                "IronRuby.Libraries"
            );

            // transparent assemblies => same version as mscorlib but uses transparent key (same as languages)
            AssemblyName transparentKeyVer = new AssemblyName(typeof(object).Assembly.FullName);
            transparentKeyVer.SetPublicKeyToken(languageKeyVer.GetPublicKeyToken());
            AddAssemblyMappings(transparentKeyVer,
                "System.ServiceModel",
                "System.ServiceModel.Syndication",
                "System.Windows.Controls",
                "System.Windows.Controls.Data",
                "System.Windows.Controls.Data.Design",
                "System.Windows.Controls.Design",
                "System.Windows.Controls.Extended",
                "System.Windows.Controls.Extended.Design",
                "System.Xml.Linq",
                "System.Xml.Serialization"
            );
        }

        private void AddAssemblyMappings(AssemblyName keyVersion, params string[] names) {
            foreach (string asm in names) {
                keyVersion.Name = asm;
                _assemblyFullNames.Add(asm.ToLower(), keyVersion.FullName);
            }
        }

        protected string LookupFullName(string name) {
            AssemblyName asm = new AssemblyName(name);
            if (asm.Version != null || asm.GetPublicKeyToken() != null || asm.GetPublicKey() != null) {
                return name;
            }
            return _assemblyFullNames.ContainsKey(name.ToLower()) ? _assemblyFullNames[name.ToLower()] : name;
        }
#endif
        #region Assembly Loading

        public virtual Assembly LoadAssembly(string name) {
#if WIN8
            throw new NotImplementedException();
#elif !SILVERLIGHT
            return Assembly.Load(name);
#else
            return Assembly.Load(LookupFullName(name));
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public virtual Assembly LoadAssemblyFromPath(string path) {
#if FEATURE_FILESYSTEM
            return Assembly.LoadFile(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual void TerminateScriptExecution(int exitCode) {
#if FEATURE_PROCESS
            System.Environment.Exit(exitCode);
#else
            throw new ExitProcessException(exitCode);
#endif
        }

        #endregion

        #region Virtual File System

        public virtual bool IsSingleRootFileSystem {
            get {
#if FEATURE_FILESYSTEM
                return Environment.OSVersion.Platform == PlatformID.Unix
                    || Environment.OSVersion.Platform == PlatformID.MacOSX;
#elif WIN8
                return false;
#else
                return true;
#endif
            }
        }

        public virtual StringComparer PathComparer {
            get {
#if FEATURE_FILESYSTEM
                return Environment.OSVersion.Platform == PlatformID.Unix ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
#else
                return StringComparer.OrdinalIgnoreCase;
#endif
            }
        }

        public virtual bool FileExists(string path) {
#if FEATURE_FILESYSTEM
            return File.Exists(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual bool DirectoryExists(string path) {
#if FEATURE_FILESYSTEM
            return Directory.Exists(path);
#else
            throw new NotImplementedException();
#endif
        }

#if !CLR2
        // TODO: better APIs
        public virtual Stream OpenFileStream(string path, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.Read, int bufferSize = 8192) {
#if FEATURE_FILESYSTEM
            if (string.Equals("nul", path, StringComparison.InvariantCultureIgnoreCase)) {
                return Stream.Null;
            }
            return new FileStream(path, mode, access, share, bufferSize);
#else
            throw new NotImplementedException();
#endif
        }

        // TODO: better APIs
        public virtual Stream OpenInputFileStream(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read, int bufferSize = 8192) {
            return OpenFileStream(path, mode, access, share, bufferSize);
        }

        // TODO: better APIs
        public virtual Stream OpenOutputFileStream(string path) {
            return OpenFileStream(path, FileMode.Create, FileAccess.Write);
        }
#else
        public virtual Stream OpenFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
#if FEATURE_FILESYSTEM
            if (string.Equals("nul", path, StringComparison.InvariantCultureIgnoreCase)) {
                return Stream.Null;
            }
            return new FileStream(path, mode, access, share, bufferSize);
#else
            throw new NotImplementedException();
#endif
        }

        // TODO: better APIs
        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share) {
            return OpenFileStream(path, mode, access, share, 8912);
        }

        // TODO: better APIs
        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
            return OpenFileStream(path, mode, access, share, bufferSize);
        }

        // TODO: better APIs
        public virtual Stream OpenInputFileStream(string path) {
            return OpenFileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 8912);
        }

        // TODO: better APIs
        public virtual Stream OpenOutputFileStream(string path) {
            return OpenFileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8912);
        }
#endif

        public virtual void DeleteFile(string path, bool deleteReadOnly) {
#if FEATURE_FILESYSTEM
            FileInfo info = new FileInfo(path);
#if !ANDROID
            if (deleteReadOnly && info.IsReadOnly) {
                info.IsReadOnly = false;
            }
#endif
            info.Delete();
#else
            throw new NotImplementedException();
#endif
        }

        public string[] GetFiles(string path, string searchPattern) {
            return GetFileSystemEntries(path, searchPattern, true, false);
        }

        public string[] GetDirectories(string path, string searchPattern) {
            return GetFileSystemEntries(path, searchPattern, false, true);
        }

        public string[] GetFileSystemEntries(string path, string searchPattern) {
            return GetFileSystemEntries(path, searchPattern, true, true);
        }

        public virtual string[] GetFileSystemEntries(string path, string searchPattern, bool includeFiles, bool includeDirectories) {
#if FEATURE_FILESYSTEM
            if (includeFiles && includeDirectories) {
                return Directory.GetFileSystemEntries(path, searchPattern);
            }
            if (includeFiles) {
                return Directory.GetFiles(path, searchPattern);
            }
            if (includeDirectories) {
                return Directory.GetDirectories(path, searchPattern);
            }
            return ArrayUtils.EmptyStrings;
#else
            throw new NotImplementedException();
#endif
        }

        /// <exception cref="ArgumentException">Invalid path.</exception>
        public virtual string GetFullPath(string path) {
#if FEATURE_FILESYSTEM
            try {
                return Path.GetFullPath(path);
            } catch (Exception) {
                throw Error.InvalidPath();
            }
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string CombinePaths(string path1, string path2) {
            return Path.Combine(path1, path2);
        }

        public virtual string GetFileName(string path) {
            return Path.GetFileName(path);
        }

        public virtual string GetDirectoryName(string path) {
            return Path.GetDirectoryName(path);
        }

        public virtual string GetExtension(string path) {
            return Path.GetExtension(path);
        }

        public virtual string GetFileNameWithoutExtension(string path) {
            return Path.GetFileNameWithoutExtension(path);
        }

        /// <exception cref="ArgumentException">Invalid path.</exception>
        public virtual bool IsAbsolutePath(string path) {
            if (String.IsNullOrEmpty(path)) {
                return false;
            }

            // no drives, no UNC:
            if (IsSingleRootFileSystem) {
                return IsDirectorySeparator(path[0]);
            }

            if (IsDirectorySeparator(path[0])) {
                // UNC path
                return path.Length > 1 && IsDirectorySeparator(path[1]);
            }

            if (path.Length > 2 && path[1] == ':' && IsDirectorySeparator(path[2])) {
                return true;
            }

            return false;
        }

#if FEATURE_FILESYSTEM
        private bool IsDirectorySeparator(char c) {
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
        }
#else
        private bool IsDirectorySeparator(char c) {
            return c == '\\' || c == '/';
        }
#endif

        public virtual string CurrentDirectory {
            get {
#if FEATURE_FILESYSTEM
                return Directory.GetCurrentDirectory();
#else
                throw new NotImplementedException();
#endif
            }
            set {
#if FEATURE_FILESYSTEM
                Directory.SetCurrentDirectory(value);
#else
                throw new NotImplementedException();
#endif
            }
        }

        public virtual void CreateDirectory(string path) {
#if FEATURE_FILESYSTEM
            Directory.CreateDirectory(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual void DeleteDirectory(string path, bool recursive) {
#if FEATURE_FILESYSTEM
            Directory.Delete(path, recursive);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual void MoveFileSystemEntry(string sourcePath, string destinationPath) {
#if FEATURE_FILESYSTEM
            Directory.Move(sourcePath, destinationPath);
#else
            throw new NotImplementedException();
#endif
        }

        #endregion

        #region Environmental Variables

        public virtual string GetEnvironmentVariable(string key) {
#if FEATURE_PROCESS
            return Environment.GetEnvironmentVariable(key);
#else
            throw new NotImplementedException();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public virtual void SetEnvironmentVariable(string key, string value) {
#if FEATURE_PROCESS
            if (value != null && value.Length == 0) {
                SetEmptyEnvironmentVariable(key);
            } else {
                Environment.SetEnvironmentVariable(key, value);
            }
#else
            throw new NotImplementedException();
#endif
        }

#if FEATURE_PROCESS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2149:TransparentMethodsMustNotCallNativeCodeFxCopRule")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void SetEmptyEnvironmentVariable(string key) {
            // System.Environment.SetEnvironmentVariable interprets an empty value string as 
            // deleting the environment variable. So we use the native SetEnvironmentVariable 
            // function here which allows setting of the value to an empty string.
            // This will require high trust and will fail in sandboxed environments
            if (!NativeMethods.SetEnvironmentVariable(key, String.Empty)) {
                throw new ExternalException("SetEnvironmentVariable failed", Marshal.GetLastWin32Error());
            }
        }
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual Dictionary<string, string> GetEnvironmentVariables() {
#if FEATURE_PROCESS
            var result = new Dictionary<string, string>();

            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                result.Add((string)entry.Key, (string)entry.Value);
            }

            return result;
#else
            throw new NotImplementedException();
#endif
        }

        #endregion
    }
}
