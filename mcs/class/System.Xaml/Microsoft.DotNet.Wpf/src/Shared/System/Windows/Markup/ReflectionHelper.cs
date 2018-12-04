// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Description: Specifies that the whitespace surrounding an element should be trimmed.
//

using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using MS.Internal;

#if PBTCOMPILER
using MS.Utility;
namespace MS.Internal.Markup

#elif WINDOWS_BASE
using MS.Utility;
using MS.Internal.WindowsBase;
namespace System.Windows.Markup
#else
namespace System.Xaml
#endif
{

    /// <summary>
    /// Class that provides helper functions for the parser to reflect on types, properties,
    /// custom attributes and load assemblies.
    /// </summary>
    internal static class ReflectionHelper
    {
        // System assembly name used by GetSystemType() to provide reflection
        // types at markup compile time through System.Reflection.TypeLoader
        private const string SystemReflectionAssemblyName = "System";

        // TypeLoader core assembly name, also used by GetMscorlibType() to provide
        // reflection types at markup compile time
        private const string MscorlibReflectionAssemblyName = "mscorlib";

#if PBTCOMPILER
        // System.Reflection.TypeLoader instance
        private static TypeLoader _typeLoader = null;

        // TypeLoader Assembly cache 
        private static Dictionary<string, Assembly> _cachedTypeLoaderAssemblies = null; 

        // TypeLoader reference paths 
        private static HashSet<string> _cachedTypeLoaderReferencePaths = null;

        static ReflectionHelper()
        {
            // System.Reflection.TypeLoader Assembly cache 
            _cachedTypeLoaderAssemblies = new Dictionary<string, Assembly>();

            // System.Reflection.TypeLoader assembly reference paths
            _cachedTypeLoaderReferencePaths = new HashSet<string>();

            // Initializes the System.Reflection.TypeLoader instance
            _typeLoader = new TypeLoader(MscorlibReflectionAssemblyName);

            // Attaches event handler that implements a probing strategy for
            // locating assemblies or dependent assemblies requested by the
            // TypeLoader in response to a reflection operation.
            _typeLoader.Resolving += TypeLoaderResolvingHandler;
        }
#endif

#region Type
        /// <summary>
        /// Parse and get the type of the passed in string
        /// </summary>
        internal static Type GetQualifiedType(string typeName)
        {
            // ISSUE: we only parse the assembly name and type name
            // all other Type.GetType() type fragments (version, culture info, pub key token etc) are ignored!!!
            string[] nameFrags = typeName.Split(new Char[] { ',' }, 2);
            Type type = null;
            if (nameFrags.Length == 1)
            {
                // treat it as an absolute name
                type = Type.GetType(nameFrags[0]);
            }
            else
            {
                if (nameFrags.Length != 2)
                    throw new InvalidOperationException(SR.Get(SRID.QualifiedNameHasWrongFormat, typeName));

                Assembly a = null;
                try
                {
                    a = LoadAssembly(nameFrags[1].TrimStart(), null);
                }
                // ifdef magic to save compiler update.
                // the fix below is for an FxCop rule about non-CLR exceptions.
                // however this rule has now been removed.
                catch (Exception e)   // Load throws generic Exceptions, so this can't be made more specific.
                {
                    if (CriticalExceptions.IsCriticalException(e))
                    {
                        throw;
                    }
                    else
                    {
                        // If we can't load the assembly, just return null (fall-through).
                        a = null;
                    }
                }

                if (a != null)
                {
                    try
                    {
                        type = a.GetType(nameFrags[0]);
                        // If we can't get the type, just return null (fall-through).
                    }
                    catch (ArgumentException)
                    {
                        a = null;
                    }
                    catch (System.Security.SecurityException)
                    {
                        a = null;
                    }
                }
            }

            return type;
        }

        internal static bool IsNullableType(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        internal static bool IsInternalType(Type type)
        {
            Type origType = type;
            Debug.Assert(null != type, "Type passed to IsInternalType is null");

            // If this is an internal nested type or a parent nested public type, walk up the declaring types.
            while (type.IsNestedAssembly || type.IsNestedFamORAssem || (origType != type && type.IsNestedPublic))
            {
                type = type.DeclaringType;
            }

            // If we're on a non-internal nested type, IsNotPublic & IsPublic will both return false.
            // If we were originally on a nested type and have currently reached a parent
            // top-level(non nested) type, then it must be top level internal or public type.
            return type.IsNotPublic || (origType != type && type.IsPublic);
        }

        /// <summary>
        /// Helper for determine if the type is a public class.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if type is public</returns>
        internal static bool IsPublicType(Type type)
        {
            Debug.Assert(null != type, "Type passed to IsPublicType is null");

            // If this is a nested internal type, walk up the declaring types.
            while (type.IsNestedPublic)
            {
                type = type.DeclaringType;
            }

            // If we're on a non-public nested type, IsPublic will return false.
            return type.IsPublic;
        }

        // Since assemblies may be loaded in regular load context as well ROL context
        // we need to get the ROL type from the real type to compare a type with at compile
        // time. At run-time, the same type can be used.
        internal static Type GetFrameworkType(string assemblyName, Type type)
        {
#if PBTCOMPILER
            Assembly reflectionAssembly = LoadAssembly(assemblyName, null);
    
            if (reflectionAssembly != null)
            {
                type = reflectionAssembly.GetType(type.FullName);
            }
            else
            {
                type = null;
            }
#endif
            return type;
        }

        internal static Type GetMscorlibType(Type type)
        {
            return GetFrameworkType(MscorlibReflectionAssemblyName, type);
        }

        internal static Type GetSystemType(Type type)
        {
            return GetFrameworkType(SystemReflectionAssemblyName, type);
        }

#if WINDOWS_BASE
        /// <summary>
        /// Get the type to use for reflection:  the custom type, if any, otherwise just the type.
        /// </summary>
        internal static Type GetReflectionType(object item)
        {
            if (item == null)
                return null;

            ICustomTypeProvider ictp = item as ICustomTypeProvider;
            if (ictp == null)
                return item.GetType();
            else
                return ictp.GetCustomType();
        }
#endif
        
#endregion Type

        #region Attributes

        internal static string GetTypeConverterAttributeData(Type type, out Type converterType)
        {
            bool foundTC = false;
            return GetCustomAttributeData(type, GetSystemType(typeof(TypeConverterAttribute)), true, ref foundTC, out converterType);
        }

        internal static string GetTypeConverterAttributeData(MemberInfo mi, out Type converterType)
        {
            return GetCustomAttributeData(mi, GetSystemType(typeof(TypeConverterAttribute)), out converterType);
        }

        // Given a ReflectionOnlyLoaded member, returns the value of a metadata attribute of
        // Type attrType if set on that member. Looks only for attributes that have a ctor with
        // one parameter that is of Type string or Type.
        private static string GetCustomAttributeData(MemberInfo mi, Type attrType, out Type typeValue)
        {
            IList<CustomAttributeData> list = CustomAttributeData.GetCustomAttributes(mi);
            string attrValue = GetCustomAttributeData(list, attrType, out typeValue, true, false);
            return attrValue == null ? string.Empty : attrValue;
        }

#if PBTCOMPILER
        // Given a ReflectionOnlyLoaded type, returns the value of a metadata attribute of
        // Type attrType if set on that type. Looks only for attributes that have a ctor with
        // one parameter that is of Type string.
        internal static string GetCustomAttributeData(Type t, Type attrType, bool allowZeroArgs)
        {
            Type typeValue = null;
            IList<CustomAttributeData> list = CustomAttributeData.GetCustomAttributes(t);
            return GetCustomAttributeData(list, attrType, out typeValue, false, allowZeroArgs);
        }
#endif

        // Helper that enumerates a list of CustomAttributeData obtained via ReflectionOnlyLoad, and
        // looks for a specific attribute of Type attrType. It only looks for attribiutes with a single
        // value of Type string that is passed in via a ctor. If allowTypeAlso is true, then it looks for
        // values of typeof(Type) as well.
        private static string GetCustomAttributeData(IList<CustomAttributeData> list, Type attrType, out Type typeValue, bool allowTypeAlso, bool allowZeroArgs)
        {
            typeValue = null;
            string attrValue = null;
            for (int j = 0; j < list.Count; j++)
            {
                attrValue = GetCustomAttributeData(list[j], attrType, out typeValue, allowTypeAlso, false, allowZeroArgs);
                if (attrValue != null)
                {
                    break;
                }
            }

            return attrValue;
        }

        // Special version of type-based GetCustomAttributeData that does two
        //  additional tasks:
        //  1) Retrieves the attributes even if it's defined on a base type, and
        //  2) Distinguishes between "attribute found and said null" and
        //     "no attribute found at all" via the ref bool.
        internal static string GetCustomAttributeData(Type t,
                                                      Type attrType,
                                                      bool allowTypeAlso,
                                                  ref bool attributeDataFound,
                                                  out Type typeValue)
        {
            typeValue = null;
            attributeDataFound = false;
            Type currentType = t;
            string attributeDataString = null;
            CustomAttributeData cad;

            while (currentType != null && !attributeDataFound)
            {
                IList<CustomAttributeData> list = CustomAttributeData.GetCustomAttributes(currentType);

                for (int j = 0; j < list.Count && !attributeDataFound; j++)
                {
                    cad = list[j];

                    if (cad.Constructor.ReflectedType == attrType)
                    {
                        attributeDataFound = true;
                        attributeDataString = GetCustomAttributeData(cad, attrType, out typeValue, allowTypeAlso, false, false);
                    }
                }

                if (!attributeDataFound)
                {
                    currentType = currentType.BaseType; // object.BaseType is null, used as terminating condition for the while() loop.
                }
            }

            return attributeDataString;
        }

        // Helper that inspects a specific CustomAttributeData obtained via ReflectionOnlyLoad, and
        // returns its value if the Type of the attribiutes matches the passed in attrType. It only
        // looks for attributes with no values or a single value of Type string that is passed in via
        // a ctor. If allowTypeAlso is true, then it looks for values of typeof(Type) as well in the
        // single value case. If noArgs == false and zeroArgsAllowed = true, that means 0 or 1 args
        // are permissible.
        private static string GetCustomAttributeData(CustomAttributeData cad,
                                                     Type attrType,
                                                 out Type typeValue,
                                                     bool allowTypeAlso,
                                                     bool noArgs,
                                                     bool zeroArgsAllowed)
        {
            string attrValue = null;
            typeValue = null;

            // get the Constructor info
            ConstructorInfo cinfo = cad.Constructor;
            if (cinfo.ReflectedType == attrType)
            {
                // typedConstructorArguments (the Attribute constructor arguments)
                // [MyAttribute("test", Name=Hello)]
                // "test" is the Constructor Argument
                IList<CustomAttributeTypedArgument> constructorArguments = cad.ConstructorArguments;
                if (constructorArguments.Count == 1 && !noArgs)
                {
                    CustomAttributeTypedArgument tca = constructorArguments[0];
                    attrValue = tca.Value as String;
                    if (attrValue == null && allowTypeAlso && tca.ArgumentType == typeof(Type))
                    {
                        typeValue = tca.Value as Type;
                        attrValue = typeValue.AssemblyQualifiedName;
                    }

                    if (attrValue == null)
                    {
                        throw new ArgumentException(SR.Get(SRID.ParserAttributeArgsLow, attrType.Name));
                    }
                }
                else if (constructorArguments.Count == 0)
                {
                    // zeroArgsAllowed = true for CPA for example.
                    // CPA with no args is valid and would mean that this type is overriding a base CPA
                    if (noArgs || zeroArgsAllowed)
                    {
                        attrValue = string.Empty;
                    }
                    else
                    {
                        throw new ArgumentException(SR.Get(SRID.ParserAttributeArgsLow, attrType.Name));
                    }
                }
                else
                {
                    throw new ArgumentException(SR.Get(SRID.ParserAttributeArgsHigh, attrType.Name));
                }
            }

            return attrValue;
        }

#endregion Attributes

#region Assembly Loading
        //
        // Clean up the cache entry for the given assembly, so that it can be reloaded for the next build cycle.
        // Usually it is called by MarkupCompiler task.
        //
        internal static void ResetCacheForAssembly(string assemblyName)
        {
            string assemblyNameLookup = assemblyName.ToUpper(CultureInfo.InvariantCulture);
#if PBTCOMPILER
            // TODO: PresentationBuildTasks porting : Reset TypeLoader between compile passes.
            // Do not do this here, which will recreate the entire TypeLoader. There doesn't
            // appear to be way to release a specific assembly in TypeLoader.
            //
            // Explicitly dispose and re-create the TypeLoader to release all assemblies. 
            // The intermediate assembly created by MarkupCompilePass1 is used as a reference 
            // and needs to be writable in compile pass 2. 
            _typeLoader.Dispose();
            _cachedTypeLoaderAssemblies.Clear();
            _cachedTypeLoaderReferencePaths.Clear();
            _typeLoader = new TypeLoader();
            _reflectionOnlyLoadedAssembliesHash[assemblyNameLookup] = null;
#else
            _loadedAssembliesHash[assemblyNameLookup] = null;
#endif
        }

        internal static Assembly LoadAssembly(string assemblyName, string assemblyPath)
        {
#if PBTCOMPILER
            return ReflectionOnlyLoadAssembly(assemblyName, assemblyPath);
#else
            return LoadAssemblyHelper(assemblyName, assemblyPath);
#endif
        }

#if !PBTCOMPILER
        internal static Assembly GetAlreadyLoadedAssembly(string assemblyNameLookup)
        {
            return (Assembly)_loadedAssembliesHash[assemblyNameLookup];
        }

        // Loads the Assembly with the specified name at the specified optional location.
        //
        // assemblyName is either short name or full name.
        // assemblyPath is either full file path or null.
        //
        private static Assembly LoadAssemblyHelper(string assemblyGivenName, string assemblyPath)
        {
            AssemblyName assemblyName = new AssemblyName(assemblyGivenName);
            string assemblyShortName = assemblyName.Name;
            assemblyShortName = assemblyShortName.ToUpper(CultureInfo.InvariantCulture);

            // Check if the assembly has already been loaded.
            Assembly retassem = (Assembly)_loadedAssembliesHash[assemblyShortName];

            if (retassem != null)
            {
                if (assemblyName.Version != null)
                {
                    AssemblyName cachedName = new AssemblyName(retassem.FullName);
                    if (!AssemblyName.ReferenceMatchesDefinition(assemblyName, cachedName))
                    {
                        string request = assemblyName.ToString();
                        string found = cachedName.ToString();
                        throw new InvalidOperationException(SR.Get(SRID.ParserAssemblyLoadVersionMismatch, request, found));
                    }
                }
            }
            else
            {
                // Check if the current AppDomain has this assembly loaded for some other reason.
                // If so, then just use that assembly and don't attempt to load another copy of it.
                // Only do this if no path is provided.
                if (String.IsNullOrEmpty(assemblyPath))
                    retassem = SafeSecurityHelper.GetLoadedAssembly(assemblyName);

                if (retassem == null)
                {
                    if (!String.IsNullOrEmpty(assemblyPath))
                    {

                        // assemblyPath is set, Load the assembly from this specified place.
                        // the path must be full file path which contains directory, file name and extension.
                        Debug.Assert(!assemblyPath.EndsWith("\\", StringComparison.Ordinal), "the assembly path should be a full file path containing file extension");

                        // LoadFile will only override your request only if it is in the GAC
                        retassem = Assembly.LoadFile(assemblyPath);
                    }
                    //
                    // At compile time, the build task should always pass the full path of the referenced assembly, even if it
                    // comes from GAC. But below code snippet can run if parser wants to try loading an assembly w/o a path.
                    // This also makes run-time assembly load consistent with compile-time semantics.
                    else
                    {
                        try
                        {
                            retassem = Assembly.Load(assemblyGivenName);
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            // This may be a locally defined assembly that has not been created yet.
                            // To support these cases, just set a null assembly and return.  This
                            // will fail downstream if it really was an assembly miss.
                            retassem = null;
                        }
                    }
                }

                // Cache the assembly
                if (retassem != null)
                {
                    _loadedAssembliesHash[assemblyShortName] = retassem;
                }
            }

            return retassem;
        }

        private static Hashtable _loadedAssembliesHash = new Hashtable(8);
#else
        // returns true is sourceAssembly declares LocalAssemblyName as a friend
        internal static bool IsFriendAssembly(Assembly sourceAssembly)
        {
            bool isFriend = false;
            Type typeValue = null;

            string friendAssemblyName = string.Empty;
            IList<CustomAttributeData> list = CustomAttributeData.GetCustomAttributes(sourceAssembly);

            for (int j = 0; j < list.Count; j++)
            {
                friendAssemblyName = GetCustomAttributeData(list[j], typeof(InternalsVisibleToAttribute), out typeValue, false, false, false);
                if (friendAssemblyName != null && friendAssemblyName == LocalAssemblyName)
                {
                    isFriend = true;
                    break;
                }
            }

            return isFriend;
        }

        internal static bool IsInternalAllowedOnType(Type type)
        {
            return ((LocalAssemblyName == type.Assembly.GetName().Name) || IsFriendAssembly(type.Assembly));
        }

        // The local assembly that contains the baml.
        internal static string LocalAssemblyName
        {
            get { return _localAssemblyName; }
            set { _localAssemblyName = value; }
        }

        private static string _localAssemblyName = string.Empty;

        internal static bool HasAlreadyReflectionOnlyLoaded(string assemblyNameLookup)
        {
             //
             // If the cache contains an entry for the given assemblyname, and its value is not
             // null, it marks the assembly has been loaded.
             //
             // Since ResetCacheForAssembly( ) just sets "null" in the hashtable for a given assembly
             // without really removing it, it is possible that an assembly is not reloaded before this
             // method is called.
             // Such as for the local-type-ref xaml file compilation,  the cache entry for the temporary
             // assembly is reset to null, but it is not reloaded for MCPass1.
             //
             // We don't want to change the behavior of ResetCacheForAssembly( ) at this moment. (Resetting
             // the value to null without really removing the entry is helpful for the perf)
             //

             return (_reflectionOnlyLoadedAssembliesHash.Contains(assemblyNameLookup) && _reflectionOnlyLoadedAssembliesHash[assemblyNameLookup] != null);
        }

        internal static Assembly GetAlreadyReflectionOnlyLoadedAssembly(string assemblyNameLookup)
        {
             return (Assembly)_reflectionOnlyLoadedAssembliesHash[assemblyNameLookup];
        }

        //
        // For a given assembly name and its full path, Reflection-Only load the assembly directly
        // from the file in disk or load the file to memory and then create assembly instance from
        // memory buffer data.
        // 
        private static Assembly ReflectionOnlyLoadAssembly(string assemblyName, string fullPathToAssembly)
        {
            Assembly assembly = null; 

            // If the assembly path is empty, try to load assembly by name. LoadFromAssemblyName 
            // will result in a TypeLoader.Resolve event that will contain more information about the 
            // requested assembly.
            if (String.IsNullOrEmpty(fullPathToAssembly))
            {
                return _typeLoader.LoadFromAssemblyName(assemblyName);
            }
            else if (_cachedTypeLoaderAssemblies.TryGetValue(fullPathToAssembly, out assembly))
            {
                return assembly;
            }
            else if (!String.IsNullOrEmpty(assemblyName) && _cachedTypeLoaderAssemblies.TryGetValue(assemblyName, out assembly))
            {
                return assembly;
            }
            else
            {
                assembly = _typeLoader.LoadFromAssemblyPath(fullPathToAssembly);
            }

            // Add the assembly to the cache. ReflectionHelper.ReflectionOnlyLoadAssembly
            // receives frequent calls requesting the same assembly.
            if (assembly != null && fullPathToAssembly != null)
            {
                _cachedTypeLoaderAssemblies.Add(fullPathToAssembly, assembly);
                _cachedTypeLoaderReferencePaths.Add(Path.GetDirectoryName(fullPathToAssembly));
            }

            return assembly;
        }

        // Handler for TypeLoader.Resolving event responsible for assembly probing and loading.  
        // TypeLoader will fire the Resolving event when an assembly or a dependent assembly is 
        // required in response to a reflection request.  Probing strategy that will
        // find all dependent DLLs, even those in other NuGet packages, must be implemented
        // here.  The probing responsibility up to the consumer of System.Reflection.TypeLoader.
        private static System.Reflection.Assembly TypeLoaderResolvingHandler(
                System.Reflection.TypeLoader typeLoader, System.Reflection.AssemblyName assemblyName)
        {
            // Resolve is called at most once per assembly.
            foreach (var path in _cachedTypeLoaderReferencePaths)
            {
                string fullFilePath = path + "\\" + assemblyName.Name + ".dll";

                if (System.IO.File.Exists(fullFilePath))
                {
                    // TODO: PBT Porting: Temporarily relax this restriction for testing. 
                    // Do not require a specific version, culture, or public key token 
                    // of the assembly. Update this block when testing is complete.
                    return _typeLoader.LoadFromStream(File.OpenRead(fullFilePath));
                }
            }

            return null;
        }

        private static Hashtable _reflectionOnlyLoadedAssembliesHash = new Hashtable(8);

        //
        // Copy assembly file from disk to memory, and return the memory buffer.
        //
        internal static byte[] GetAssemblyContent(string filepath)
        {
            byte[] asmContents = null;

            using (FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // FileStream.Read does not support offsets or lengths
                // larger than int.MaxValue.
                if (fileStream.Length > int.MaxValue)
                {
                    return null;
                }

                int size = (int)fileStream.Length;
                asmContents = new byte[size];
                if (size > 0)
                {
                    ReliableRead(fileStream, asmContents, 0, size);
                }

                // With using statement, fileStream can always be disposed,
                // there is no need to put code here to explicitly dispose the
                // file stream object.
            }

            return asmContents;
        }

        //
        // set flag for the assembly to indicate that this assembly should be loaded from memory buffer
        // instead of file in disk.
        //
        // Usually it is called by MarkupCompiler task.
        //
        internal static void SetContentLoadForAssembly(string assemblyName)
        {
            string assemblyNameLookup = assemblyName.ToUpper(CultureInfo.InvariantCulture);
            _contentLoadAssembliesHash[assemblyNameLookup] = true;
        }

        /// <summary>
        /// Read utility that is guaranteed to return the number of bytes requested
        /// if they are available.
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <param name="buffer">buffer to read into</param>
        /// <param name="offset">offset in buffer to write to</param>
        /// <param name="count">bytes to read</param>
        /// <returns>bytes read</returns>
        /// <remarks>Normal Stream.Read does not guarantee how many bytes it will
        /// return.  This one does.</remarks>
        private static int ReliableRead(Stream stream, byte[] buffer, int offset, int count)
        {
            /* Invariant.Assert is not available in PBT
            Invariant.Assert(stream != null);
            Invariant.Assert(buffer != null);
            Invariant.Assert(buffer.Length > 0);
            Invariant.Assert(offset >= 0);
            Invariant.Assert(count >= 0);
            Invariant.Assert(checked(offset + count<= buffer.Length));
            */

            // let's read the whole block into our buffer
            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int bytesRead = stream.Read(buffer,
                                offset + totalBytesRead,
                                count - totalBytesRead);
                if (bytesRead == 0)
                {
                    break;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        private static Hashtable _contentLoadAssembliesHash = new Hashtable(1);
#endif

        #endregion Assembly Loading
    }
}

