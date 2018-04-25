//------------------------------------------------------------------------------
// <copyright file="Util.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Implements various utility functions used by the template code
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Security;
    using System.Web.Security.Cryptography;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    using Microsoft.Win32;
    using Debug = System.Web.Util.Debug;

internal static class Util {
    private static string[] s_invalidCultureNames = new string[] { "aspx", "ascx", "master" };

    [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
    [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "This is being tracked as DevDiv #21217 (http://vstfdevdiv:8080/DevDiv2/web/wi.aspx?id=21217).")]
    internal static string SerializeWithAssert(IStateFormatter formatter, object stateGraph) {
        return formatter.Serialize(stateGraph);
    }

    [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
    [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "This is being tracked as DevDiv #21217 (http://vstfdevdiv:8080/DevDiv2/web/wi.aspx?id=21217).")]
    internal static string SerializeWithAssert(IStateFormatter2 formatter, object stateGraph, Purpose purpose) {
        return formatter.Serialize(stateGraph, purpose);
    }

    [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
    [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "This is being tracked as DevDiv #21217 (http://vstfdevdiv:8080/DevDiv2/web/wi.aspx?id=21217).")]
    internal static object DeserializeWithAssert(IStateFormatter2 formatter, string serializedState, Purpose purpose) {
        return formatter.Deserialize(serializedState, purpose);
    }
    
    internal static bool CanConvertToFrom(TypeConverter converter, Type type) {
        return (converter != null && converter.CanConvertTo(type) &&
                converter.CanConvertFrom(type) && !(converter is ReferenceConverter));
    }
    
    internal static void CopyBaseAttributesToInnerControl(WebControl control, WebControl child) {
        short oldTab = control.TabIndex;
        string oldAccess = control.AccessKey;
        try {
            control.AccessKey = String.Empty;
            control.TabIndex = 0;
            child.CopyBaseAttributes(control);
        }
        finally {
            control.TabIndex = oldTab;
            control.AccessKey = oldAccess;
        }
    }

    internal static long GetRecompilationHash(PagesSection ps)
    {
        HashCodeCombiner recompilationHash = new HashCodeCombiner();
        NamespaceCollection namespaces;
        TagPrefixCollection controls;
        TagMapCollection tagMapping;

        // Combine items from Pages section
        recompilationHash.AddObject(ps.Buffer);
        recompilationHash.AddObject(ps.EnableViewState);
        recompilationHash.AddObject(ps.EnableViewStateMac);
        recompilationHash.AddObject(ps.EnableEventValidation);
        recompilationHash.AddObject(ps.SmartNavigation);
        recompilationHash.AddObject(ps.ValidateRequest);
        recompilationHash.AddObject(ps.AutoEventWireup);
        if (ps.PageBaseTypeInternal != null) {
            recompilationHash.AddObject(ps.PageBaseTypeInternal.FullName);
        }
        if (ps.UserControlBaseTypeInternal != null) {
            recompilationHash.AddObject(ps.UserControlBaseTypeInternal.FullName);
        }
        if (ps.PageParserFilterTypeInternal != null) {
            recompilationHash.AddObject(ps.PageParserFilterTypeInternal.FullName);
        }
        recompilationHash.AddObject(ps.MasterPageFile);
        recompilationHash.AddObject(ps.Theme);
        recompilationHash.AddObject(ps.StyleSheetTheme);
        recompilationHash.AddObject(ps.EnableSessionState);
        recompilationHash.AddObject(ps.CompilationMode);
        recompilationHash.AddObject(ps.MaxPageStateFieldLength);
        recompilationHash.AddObject(ps.ViewStateEncryptionMode);
        recompilationHash.AddObject(ps.MaintainScrollPositionOnPostBack);

        // Combine items from Namespaces collection
        namespaces = ps.Namespaces;

        recompilationHash.AddObject(namespaces.AutoImportVBNamespace);
        if (namespaces.Count == 0) {
            recompilationHash.AddObject("__clearnamespaces");
        }
        else {
            foreach (NamespaceInfo ni in namespaces) {
                recompilationHash.AddObject(ni.Namespace);
            }
        }

        // Combine items from the Controls collection
        controls = ps.Controls;

        if (controls.Count == 0) {
            recompilationHash.AddObject("__clearcontrols");
        }
        else {
            foreach (TagPrefixInfo tpi in controls) {
                recompilationHash.AddObject(tpi.TagPrefix);

                if (tpi.TagName != null && tpi.TagName.Length != 0) {
                    recompilationHash.AddObject(tpi.TagName);
                    recompilationHash.AddObject(tpi.Source);
                }
                else {
                    recompilationHash.AddObject(tpi.Namespace);
                    recompilationHash.AddObject(tpi.Assembly);
                }
            }
        }

        // Combine items from the TagMapping Collection
        tagMapping = ps.TagMapping;

        if (tagMapping.Count == 0) {
            recompilationHash.AddObject("__cleartagmapping");
        }
        else {
            foreach (TagMapInfo tmi in tagMapping) {
                recompilationHash.AddObject(tmi.TagType);
                recompilationHash.AddObject(tmi.MappedTagType);
            }
        }

        return recompilationHash.CombinedHash;
    }
    

    internal static Encoding GetEncodingFromConfigPath(VirtualPath configPath) {

        Debug.Assert(configPath != null, "configPath != null");

        // Check if a file encoding is specified in the config
        Encoding fileEncoding = null;
        GlobalizationSection globConfig = RuntimeConfig.GetConfig(configPath).Globalization;
        fileEncoding = globConfig.FileEncoding;

        // If not, use the default encoding
        if (fileEncoding == null)
            fileEncoding = Encoding.Default;

        return fileEncoding;
    }

    /*
     * Return a reader which holds the contents of a file.  If a configPath is passed
     * in, try to get an encoding for it
     */
    internal /*public*/ static StreamReader ReaderFromFile(string filename, VirtualPath configPath) {

        StreamReader reader;

        // Check if a file encoding is specified in the config
        Encoding fileEncoding = Encoding.Default;
        if (configPath != null) fileEncoding = GetEncodingFromConfigPath(configPath);

        try {
            // Create a reader on the file, using the encoding
            // Throws an exception if the file can't be opened.
            reader = new StreamReader(filename, fileEncoding,
                true /*detectEncodingFromByteOrderMarks*/, 4096);
        }
        catch (UnauthorizedAccessException) {
            // AccessException might mean two very different things: it could be a real
            // access problem, or it could be that it's actually a directory.

            // It's a directory: give a specific error.
            if (FileUtil.DirectoryExists(filename)) {
                throw new HttpException(
                    SR.GetString(SR.Unexpected_Directory, HttpRuntime.GetSafePath(filename)));
            }

            // It's a real access problem, so just rethrow it
            throw;
        }

        return reader;
    }

    /*
     * Attempt to delete a file, but don't throw if it can't be done
     */
    internal static void DeleteFileNoException(string path) {
        Debug.Assert(File.Exists(path), path);
        try {
            File.Delete(path);
        }
        catch { } // Ignore all exceptions
    }

    internal static void DeleteFileIfExistsNoException(string path) {
        if (File.Exists(path))
            DeleteFileNoException(path);
    }

    /*
     * Return true if the directory exists and is not empty.
     */
    internal static bool IsNonEmptyDirectory(string dir) {

        // Does it exist
        if (!Directory.Exists(dir))
            return false;

        // It exists, but maybe it's empty
        try {
            string[] entries = Directory.GetFileSystemEntries(dir);
            return entries.Length > 0;
        }
        catch {
            // If it throws, assume it's non-empty
            return true;
        }
    }

    /*
     * Return true if string is a valid simple file name (with no path or wild cards)
     */
    private static char[] invalidFileNameChars = new char[] { '/', '\\', '?', '*', ':' } ;
    internal static bool IsValidFileName(string fileName) {

        // Check for the special names "." and ".."
        if (fileName == "." || fileName == "..")
            return false;

        // Check for invalid characters
        if (fileName.IndexOfAny(invalidFileNameChars) >= 0)
            return false;

        return true;
    }

    /*
     * Replace all invalid chars in a filename by underscores.
     */
    internal static string MakeValidFileName(string fileName) {

        // If it's already valid, nothing to do
        if (IsValidFileName(fileName))
            return fileName;
            
        // Replace all the invalid chars by '_'
        for (int i = 0; i < invalidFileNameChars.Length; ++i)  {
            fileName = fileName.Replace(invalidFileNameChars[i], '_');
        }

        // Shoud always be valid now
        Debug.Assert(IsValidFileName(fileName));

        return fileName;
    }

    /*
     * Return true if the current user has write access to the directory
     */
    internal static bool HasWriteAccessToDirectory(string dir) {

        // If it doesn't even exist (or we can't determine that it does), return false
        if (!Directory.Exists(dir))
            return false;

        // Get the path to a dummy file in that directory
        string dummyFile = Path.Combine(dir, "~AspAccessCheck_" +
            HostingEnvironment.AppDomainUniqueInteger.ToString(
                "x", CultureInfo.InvariantCulture) + SafeNativeMethods.GetCurrentThreadId() + ".tmp");
        FileStream fs = null;

        bool success = false;
        try {
            // Attempt to create the file
            fs = new FileStream(dummyFile, FileMode.Create);
        }
        catch {
        }
        finally {
            if (fs != null) {
                // If successfully created, close and delete it
                fs.Close();
                File.Delete(dummyFile);
                success = true;
            }
        }

        return success;
    }

    internal static VirtualPath GetScriptLocation() {
        // prepare script include
        // Dev10 Bug564221: we need to detect if app level web.config overwrites the root web.config
        string location = (string) RuntimeConfig.GetAppConfig().WebControls["clientScriptsLocation"];
        
        // If there is a formatter, as there will be for the default machine.config, insert the assembly name and version.
        if (location.IndexOf("{0}", StringComparison.Ordinal) >= 0) {
            string assembly = "system_web";

            // QFE number is not included in client path
            string version = VersionInfo.SystemWebVersion.Substring(0, VersionInfo.SystemWebVersion.LastIndexOf('.')).Replace('.', '_');
            location = String.Format(CultureInfo.InvariantCulture, location, assembly, version);
        }

        return VirtualPath.Create(location);
    }

    /*
     * Return a reader which holds the contents of a file.  If a configPath is passed
     * in, try to get a encoding for it
     */
    internal /*public*/ static StreamReader ReaderFromStream(Stream stream, VirtualPath configPath) {

        // Check if a file encoding is specified in the config
        Encoding fileEncoding = GetEncodingFromConfigPath(configPath);

        // Create a reader on the file, using the encoding
        return new StreamReader(stream, fileEncoding,
            true /*detectEncodingFromByteOrderMarks*/, 4096);
    }

    /*
     * Return a String which holds the contents of a file
     */
    internal /*public*/ static String StringFromVirtualPath(VirtualPath virtualPath) {

        using (Stream stream = virtualPath.OpenFile()) {
            // Create a reader on the file, and read the whole thing
            TextReader reader = Util.ReaderFromStream(stream, virtualPath);
            return reader.ReadToEnd();
        }
    }

    /*
     * Return a String which holds the contents of a file
     */
    internal /*public*/ static String StringFromFile(string path) {
        Encoding encoding = Encoding.Default;
        return StringFromFile(path, ref encoding);
    }

    /*
     * Return a String which holds the contents of a file with specific encoding.
     */
    internal /*public*/ static String StringFromFile(string path, ref Encoding encoding) {

        // Create a reader on the file.
        // Generates an exception if the file can't be opened.
        StreamReader reader = new StreamReader(path, encoding, true /*detectEncodingFromByteOrderMarks*/);

        try {
            string content = reader.ReadToEnd();
            encoding = reader.CurrentEncoding;

            return content;
        }
        finally {
            // Make sure we always close the stream
            if (reader != null)
                reader.Close();
        }
    }

    /*
     * Return a String which holds the contents of a file, or null if the file
     * doesn't exist.
     */
    internal /*public*/ static String StringFromFileIfExists(string path) {

        if (!File.Exists(path)) return null;

        return StringFromFile(path);
    }

    /*
     * If the file doesn't exist, do nothing.  If it does try to delete it if possible.
     * If that fails, rename it with by appending a .delete extension to it
     */
    internal static void RemoveOrRenameFile(string filename) {
        FileInfo fi = new FileInfo(filename);
        RemoveOrRenameFile(fi);
    }

    /*
     * If the file doesn't exist, do nothing.  If it does try to delete it if possible.
     * If that fails, rename it with by appending a .delete extension to it
     */
    internal static bool RemoveOrRenameFile(FileInfo f) {
        try {
            // First, just try to delete the file
            f.Delete();

            // It was successfully deleted, so return true
            return true;
        }
        catch {

            try {
                // If the delete failed, rename it to ".delete"
                // Don't do that if it already has the delete extension
                if (f.Extension != ".delete") {

                    // include a unique token as part of the new name, to avoid
                    // conflicts with previous renames (VSWhidbey 79996)
                    string uniqueToken = DateTime.Now.Ticks.GetHashCode().ToString("x", CultureInfo.InvariantCulture);
                    string newName = f.FullName + "." + uniqueToken + ".delete";
                    f.MoveTo(newName);
                }
            }
            catch {
                // Ignore all exceptions
            }
        }

        // Return false because we couldn't delete it, and had to rename it
        return false;
    }

    /*
     * Clears a file's readonly attribute if it has one
     */
    internal static void ClearReadOnlyAttribute(string path) {

        FileAttributes attribs = File.GetAttributes(path);
        if ((attribs & FileAttributes.ReadOnly) != 0) {
            File.SetAttributes(path, attribs & ~FileAttributes.ReadOnly);
        }
    }

    internal static void CheckVirtualFileExists(VirtualPath virtualPath) {
        if (!virtualPath.FileExists()) {
            throw new HttpException(
                HttpStatus.NotFound,
                SR.GetString(SR.FileName_does_not_exist,
                    virtualPath.VirtualPathString));
        }
    }

    internal static bool VirtualFileExistsWithAssert(VirtualPath virtualPath) {
        string physicalDir = virtualPath.MapPathInternal();

        if (physicalDir != null) {
            (InternalSecurityPermissions.PathDiscovery(physicalDir)).Assert();
        }

        return virtualPath.FileExists();
    }

    internal static void CheckThemeAttribute(string themeName) {
        if (themeName.Length > 0) {
            if (!FileUtil.IsValidDirectoryName(themeName)) {
                throw new HttpException(SR.GetString(SR.Page_theme_invalid_name, themeName));
            }

            if (!ThemeExists(themeName)) {
                throw new HttpException(SR.GetString(SR.Page_theme_not_found, themeName));
            }
        }
    }

    internal static bool ThemeExists(string themeName) {
        VirtualPath virtualDir = ThemeDirectoryCompiler.GetAppThemeVirtualDir(themeName);
        if (!VirtualDirectoryExistsWithAssert(virtualDir)) {
            virtualDir = ThemeDirectoryCompiler.GetGlobalThemeVirtualDir(themeName);
            if (!VirtualDirectoryExistsWithAssert(virtualDir)) {
                return false;
            }
        }

        return true;
    }

    private static bool VirtualDirectoryExistsWithAssert(VirtualPath virtualDir) {
        try {
            String physicalDir = virtualDir.MapPathInternal();
            if (physicalDir != null) {
                new FileIOPermission(FileIOPermissionAccess.Read, physicalDir).Assert();
            }

            return virtualDir.DirectoryExists();
        }
        catch {
            return false;
        }
    }

    internal static void CheckAssignableType(Type baseType, Type type) {
        if (!baseType.IsAssignableFrom(type)) {
            throw new HttpException(
                SR.GetString(SR.Type_doesnt_inherit_from_type,
                    type.FullName, baseType.FullName));
        }
    }

    internal /*public*/ static int LineCount(string text, int offset, int newoffset) {

        Debug.Assert(offset <= newoffset);

        int linecount = 0;

        while (offset < newoffset) {
            if (text[offset] == '\r' || (text[offset] == '\n' && (offset == 0 || text[offset - 1] != '\r')))
                linecount++;
            offset++;
        }

        return linecount;
    }

    /*
     * Calls Invoke on a MethodInfo.  If an exception happens during the
     * method call, catch it and throw it back.
     */
    internal static object InvokeMethod(
                                       MethodInfo methodInfo,
                                       object obj,
                                       object[] parameters) {
        try {
            return methodInfo.Invoke(obj, parameters);
        }
        catch (TargetInvocationException e) {
            throw e.InnerException;
        }
    }

    /*
     * If the passed in Type has a non-private field with the passed in name,
     * return the field's Type.
     */
    internal static Type GetNonPrivateFieldType(Type classType, string fieldName) {
        FieldInfo fieldInfo = classType.GetField(fieldName,
            BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        if (fieldInfo == null || fieldInfo.IsPrivate)
            return null;

        return fieldInfo.FieldType;
    }

    /*
     * If the passed in Type has a non-private property with the passed in name,
     * return the property's Type.
     */
    internal static Type GetNonPrivatePropertyType(Type classType, string propName) {
        PropertyInfo propInfo = null;

        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance
            | BindingFlags.IgnoreCase | BindingFlags.NonPublic;

        try {
            propInfo = classType.GetProperty(propName, flags);
        }
        catch (AmbiguousMatchException) {

            // We could get an AmbiguousMatchException if the property exists on two
            // different ancestor classes (VSWhidbey 216957).  When that happens, attempt
            // a lookup on the Type itself, ignoring its ancestors.

            flags |= BindingFlags.DeclaredOnly;
            propInfo = classType.GetProperty(propName, flags);
        }

        if (propInfo == null)
            return null;

        // If it doesn't have a setter, ot if it's private, fail
        MethodInfo methodInfo = propInfo.GetSetMethod(true /*nonPublic*/);
        if (methodInfo == null || methodInfo.IsPrivate)
            return null;

        return propInfo.PropertyType;
    }

    /*
     * Checks whether the property has a TemplateInstanceAttribute, and returns true if 
     * its value is TemplateInstance.Multiple.
     */
    internal static bool IsMultiInstanceTemplateProperty(PropertyInfo pInfo) {
        object[] instanceAttrs = pInfo.GetCustomAttributes(typeof(TemplateInstanceAttribute), /*inherits*/ false);

        // Default value for TemplateInstanceAttribute is TemplateInstance.Multiple
        if (instanceAttrs == null || instanceAttrs.Length == 0) {
            return true;
        }

        return ((TemplateInstanceAttribute)instanceAttrs[0]).Instances == TemplateInstance.Multiple;
    }


    /*
     * Return the first key of the dictionary as a string.  Throws if it's
     * empty or if the key is not a string.
     */
    private static string FirstDictionaryKey(IDictionary dict) {
        IDictionaryEnumerator e = dict.GetEnumerator();
        e.MoveNext();
        return (string)e.Key;
    }

    /*
     * Get a value from a dictionary, and remove it from the dictionary if
     * it exists.
     */
    private static string GetAndRemove(IDictionary dict, string key) {
        string val = (string) dict[key];
        if (val != null) {
            dict.Remove(key);
            val = val.Trim();
        }
        return val;
    }

    /*
     * Get a value from a dictionary, and remove it from the dictionary if
     * it exists.  Throw an exception if the value is a whitespace string.
     * However, don't complain about null, which simply means the value is not
     * in the dictionary.
     */
    internal static string GetAndRemoveNonEmptyAttribute(IDictionary directives, string key, bool required) {
        string val = Util.GetAndRemove(directives, key);

        if (val == null) {
            if (required)
                throw new HttpException(SR.GetString(SR.Missing_attr, key));

            return null;
        }

        return GetNonEmptyAttribute(key, val);
    }

    // Return the value, after checking that it's not empty
    internal static string GetNonEmptyAttribute(string name, string value) {

        value = value.Trim();

        if (value.Length == 0) {
            throw new HttpException(
                SR.GetString(SR.Empty_attribute, name));
        }

        return value;
    }

    // Return the value, after checking that it doesn't contain spaces
    internal static string GetNoSpaceAttribute(string name, string value) {
        if (Util.ContainsWhiteSpace(value)) {
            throw new HttpException(
                SR.GetString(SR.Space_attribute, name));
        }

        return value;
    }

    internal static string GetAndRemoveNonEmptyAttribute(IDictionary directives, string key) {
        return GetAndRemoveNonEmptyAttribute(directives, key, false /*required*/);
    }

    internal static VirtualPath GetAndRemoveVirtualPathAttribute(IDictionary directives, string key) {
        return GetAndRemoveVirtualPathAttribute(directives, key, false /*required*/);
    }

    internal static VirtualPath GetAndRemoveVirtualPathAttribute(IDictionary directives, string key, bool required) {

        string val = GetAndRemoveNonEmptyAttribute(directives, key, required);
        if (val == null)
            return null;

        return VirtualPath.Create(val);
    }

    /*
     * Parse a DeviceName:AttribName string into its components
     */
    internal const char DeviceFilterSeparator = ':';
    internal const string XmlnsAttribute = "xmlns:";
    public static string ParsePropertyDeviceFilter(string input, out string propName) {
        string deviceName = String.Empty;

        // If the string has no device filter, the whole string is the property name
        if (input.IndexOf(DeviceFilterSeparator) < 0) {
            propName = input;
        }
        // Don't treat xmlns as filters, this needs to be treated differently.
        // VSWhidbey 495125
        else if (StringUtil.StringStartsWithIgnoreCase(input, XmlnsAttribute)) {
            propName = input;
        }
        else {
            // There is a filter: parse it out
            string[] tmp = input.Split(DeviceFilterSeparator);

            if (tmp.Length > 2) {
                throw new HttpException(
                    SR.GetString(SR.Too_many_filters, input));
            }

            if (MTConfigUtil.GetPagesConfig().IgnoreDeviceFilters[tmp[0]] != null) {
                propName = input;
            }
            else {
                deviceName = tmp[0];
                propName = tmp[1];
            }
        }

        return deviceName;
    }

    /// <devdoc>
    /// Combines the filter and name
    /// </devdoc>
    public static string CreateFilteredName(string deviceName, string name) {
        if (deviceName.Length > 0) {
            return deviceName + DeviceFilterSeparator + name;
        }
        return name;
    }

    internal static string GetAndRemoveRequiredAttribute(IDictionary directives, string key) {
        return GetAndRemoveNonEmptyAttribute(directives, key, true /*required*/);
    }

    /*
     * Same as GetAndRemoveNonEmptyAttribute, but make sure the value does not
     * contain any whitespace characters.
     */
    internal static string GetAndRemoveNonEmptyNoSpaceAttribute(IDictionary directives,
        string key, bool required) {

        string val = Util.GetAndRemoveNonEmptyAttribute(directives, key, required);

        if (val == null)
            return null;

        return GetNonEmptyNoSpaceAttribute(key, val);
    }

    internal static string GetAndRemoveNonEmptyNoSpaceAttribute(IDictionary directives,
        string key) {
        return GetAndRemoveNonEmptyNoSpaceAttribute(directives, key, false /*required*/);
    }

    // Return the value, after checking that it's not empty and has no spaces
    internal static string GetNonEmptyNoSpaceAttribute(string name, string value) {
        value = GetNonEmptyAttribute(name, value);
        return GetNoSpaceAttribute(name, value);
    }

    /*
     * Same as GetAndRemoveNonEmptyNoSpaceAttribute, but make sure the value is a
     * valid language identifier
     */
    internal static string GetAndRemoveNonEmptyIdentifierAttribute(IDictionary directives,
        string key, bool required) {

        string val = Util.GetAndRemoveNonEmptyNoSpaceAttribute(directives, key, required);

        if (val == null)
            return null;

        return GetNonEmptyIdentifierAttribute(key, val);
    }

    // Return the value, after checking that it's a valid id
    internal static string GetNonEmptyIdentifierAttribute(string name, string value) {
        value = GetNonEmptyNoSpaceAttribute(name, value);

        if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(value)) {
            throw new HttpException(
                SR.GetString(SR.Invalid_attribute_value, value, name));
        }

        return value;
    }

    // Return the class and the namespace
    internal static string GetNonEmptyFullClassNameAttribute(string name, string value,
        ref string ns) {

        value = GetNonEmptyNoSpaceAttribute(name, value);

        // The value can be of the form NS1.NS2.MyClassName.  Split it into its parts.
        string[] parts = value.Split('.');

        // Check that all the parts are valid identifiers
        foreach (string part in parts) {
            if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(part)) {
                throw new HttpException(
                    SR.GetString(SR.Invalid_attribute_value, value, name));
            }
        }

        // If there is a namespace, return it
        if (parts.Length > 1)
            ns = String.Join(".", parts, 0, parts.Length-1);

        // Return the class name (which is the last part)
        return parts[parts.Length-1];
    }

    internal static void CheckUnknownDirectiveAttributes(string directiveName, IDictionary directive) {

        CheckUnknownDirectiveAttributes(directiveName, directive, SR.Attr_not_supported_in_directive);
    }

    internal static void CheckUnknownDirectiveAttributes(string directiveName, IDictionary directive,
        string resourceKey) {

        // If there are some attributes left, fail
        if (directive.Count > 0) {
            throw new HttpException(
                SR.GetString(resourceKey,
                    Util.FirstDictionaryKey(directive), directiveName));
        }
    }

    /*
     * Get a string value from a dictionary, and convert it to bool.  Throw an
     * exception if it's not a valid bool string.
     * However, don't complain about null, which simply means the value is not
     * in the dictionary.
     * The value is returned through a REF param (unchanged if null)
     * Return value: true if attrib exists, false otherwise
     */
    internal static bool GetAndRemoveBooleanAttribute(IDictionary directives,
                                                      string key, ref bool val) {
        string s = Util.GetAndRemove(directives, key);

        if (s == null)
            return false;

        val = GetBooleanAttribute(key, s);
        return true;
    }

    // Parse a string attribute into a bool
    internal static bool GetBooleanAttribute(string name, string value) {
        try {
            return bool.Parse(value);
        }
        catch {
            throw new HttpException(
                SR.GetString(SR.Invalid_boolean_attribute, name));
        }
    }

    /*
     * Get a string value from a dictionary, and convert it to integer.  Throw an
     * exception if it's not a valid positive integer string.
     * However, don't complain about null, which simply means the value is not
     * in the dictionary.
     * The value is returned through a REF param (unchanged if null)
     * Return value: true if attrib exists, false otherwise
     */
    internal static bool GetAndRemoveNonNegativeIntegerAttribute(IDictionary directives,
                                                              string key, ref int val) {
        string s = Util.GetAndRemove(directives, key);

        if (s == null)
            return false;

        val = GetNonNegativeIntegerAttribute(key, s);

        return true;
    }

    // Parse a string attribute into a non-negative integer
    internal static int GetNonNegativeIntegerAttribute(string name, string value) {

        int ret;

        try {
            ret = int.Parse(value, CultureInfo.InvariantCulture);
        }
        catch {
            throw new HttpException(
                SR.GetString(SR.Invalid_nonnegative_integer_attribute, name));
        }

        // Make sure it's not negative
        if (ret < 0) {
            throw new HttpException(
                SR.GetString(SR.Invalid_nonnegative_integer_attribute, name));
        }

        return ret;
    }

    internal static bool GetAndRemovePositiveIntegerAttribute(IDictionary directives,
                                                              string key, ref int val) {
        string s = Util.GetAndRemove(directives, key);

        if (s == null)
            return false;

        try {
            val = int.Parse(s, CultureInfo.InvariantCulture);
        }
        catch {
            throw new HttpException(
                SR.GetString(SR.Invalid_positive_integer_attribute, key));
        }

        // Make sure it's positive
        if (val <= 0) {
            throw new HttpException(
                SR.GetString(SR.Invalid_positive_integer_attribute, key));
        }

        return true;
    }

    internal static object GetAndRemoveEnumAttribute(IDictionary directives, Type enumType,
                                                   string key) {
        string s = Util.GetAndRemove(directives, key);

        if (s == null)
            return null;

        return GetEnumAttribute(key, s, enumType);
    }

    internal static object GetEnumAttribute(string name, string value, Type enumType) {
        return GetEnumAttribute(name, value, enumType, false);
    }

    internal static object GetEnumAttribute(string name, string value, Type enumType, bool allowMultiple) {
        object val;

        try {
            // Don't allow numbers to be specified (ASURT 71851)
            // Also, don't allow several values (e.g. "red,blue")
            if (Char.IsDigit(value[0]) || value[0] == '-' || ((!allowMultiple) && (value.IndexOf(',') >= 0)))
                throw new FormatException(SR.GetString(SR.EnumAttributeInvalidString, value, name, enumType.FullName));

            val = Enum.Parse(enumType, value, true /*ignoreCase*/);
        }
        catch {
            string names = null;
            foreach (string n in Enum.GetNames(enumType)) {
                if (names == null)
                    names = n;
                else
                    names += ", " + n;
            }
            throw new HttpException(
                SR.GetString(SR.Invalid_enum_attribute, name, names));
        }

        return val;
    }

    /*
     * Return true iff the string is made of all white space characters
     */
    internal static bool IsWhiteSpaceString(string s) {
        return (s.Trim().Length == 0);
    }

    /*
     * Return true iff the string contains some white space characters
     */
    internal static bool ContainsWhiteSpace(string s) {
        for (int i=s.Length-1; i>=0; i--) {
            if (Char.IsWhiteSpace(s[i]))
                return true;
        }

        return false;
    }

    /*
     * Return the index of the first non whitespace char.  -1 if none found.
     */
    internal static int FirstNonWhiteSpaceIndex(string s) {
        for (int i=0; i<s.Length; i++) {
            if (!Char.IsWhiteSpace(s[i]))
                return i;
        }

        return -1;
    }

    /*
     * Return true iff the string holds the value "true" (case insensitive).
     * Checks for null.
     */
    internal static bool IsTrueString(string s) {
        return s != null && (StringUtil.EqualsIgnoreCase(s, "true"));
    }

    /*
     * Return true iff the string holds the value "false" (case insensitive)
     * Checks for null.
     */
    internal static bool IsFalseString(string s) {
        return s != null && (StringUtil.EqualsIgnoreCase(s, "false"));
    }

    internal static string GetStringFromBool(bool flag) {
        return flag ? "true" : "false";
    }

    /*
     * Return a full type name from a namespace (could be empty) and a type name
     */
    internal static string MakeFullTypeName(string ns, string typeName) {
        if (String.IsNullOrEmpty(ns))
            return typeName;

        return ns + "." + typeName;
    }

    /*
     * Return a valid type name from a string by changing any character
     * that's not a letter or a digit to an '_'.
     */
    internal static string MakeValidTypeNameFromString(string s) {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < s.Length; i++) {
            // Make sure it doesn't start with a digit (ASURT 31134)
            if (i == 0 && Char.IsDigit(s[0]))
                sb.Append('_');

            if (Char.IsLetterOrDigit(s[i]))
                sb.Append(s[i]);
            else
                sb.Append('_');
        }

        return sb.ToString();
    }

    /*
     * Extract a namespace and typename from a virtualPath
     * We use all but the last two chunks as the namespace
     * e.g. Aaa.Bbb.Ccc.Wsdl will use the "Aaa.Bbb" namespace, and Ccc as the type.
     * chunksToIgnore is the number of ending chunks to ignore (e.g. 1 for the extension)
     */
    internal static string GetNamespaceAndTypeNameFromVirtualPath(VirtualPath virtualPath,
        int chunksToIgnore, out string typeName) {

        // Get the file name (with no path)
        string filename = virtualPath.FileName;

        // Split it into chunks separated by '.'
        string[] chunks = filename.Split('.');

        int chunkCount = chunks.Length - chunksToIgnore;
        Debug.Assert(chunkCount >= 1);

        if (IsWhiteSpaceString(chunks[chunkCount-1])) {
            throw new HttpException(SR.GetString(SR.Unsupported_filename, filename));
        }

        typeName = MakeValidTypeNameFromString(chunks[chunkCount-1]);

        // Turn all the relevant chunks into valid namespace chunks
        for (int i=0; i<chunkCount-1; i++) {

            if (IsWhiteSpaceString(chunks[i])) {
                throw new HttpException(SR.GetString(SR.Unsupported_filename, filename));
            }

            chunks[i] = MakeValidTypeNameFromString(chunks[i]);
        }

        // Put the relevant chunks back together
        return String.Join(".", chunks, 0, chunkCount-1);
    }

    /*
     * Same as GetNamespaceAndTypeNameFromVirtualPath, but ignore the type name
     */
    internal static string GetNamespaceFromVirtualPath(VirtualPath virtualPath) {

        string typeName;
        return GetNamespaceAndTypeNameFromVirtualPath(virtualPath, 1, out typeName);
    }

    /*
     * Return a standard path from a file:// url
     */
    internal static string FilePathFromFileUrl(string url) {

        // 
        Uri uri=new Uri(url);
        string path = uri.LocalPath;
        return HttpUtility.UrlDecode(path);
    }

    /*
     * Checks whether the passed in string is a valid culture name.
     */
    internal static bool IsCultureName(string s) {

        if (String.IsNullOrEmpty(s))
            return false;

        // Ensure current cultureName is not one of well known invalid culture names
        foreach (string name in s_invalidCultureNames) {
            if (StringUtil.EqualsIgnoreCase(name, s)) {
                return false;
            }
        }

        // 

        CultureInfo ci = null;
        try {
            ci = HttpServerUtility.CreateReadOnlyCultureInfo(s);
        }
        catch {}

        return (ci != null);
    }

    /*
     * Return the culture name for a file (e.g. "fr" or "fr-fr").
     * If no culture applies, return null.
     */
    internal static string GetCultureName(string virtualPath) {

        if (virtualPath == null) return null;

        // By default, extract the culture name from the file name (e.g. "foo.fr-fr.resx")

        string fileNameNoExt = Path.GetFileNameWithoutExtension(virtualPath);

        // If virtualPath is not a file, ie. above statement returns null, simply return null;
        if (fileNameNoExt == null)
            return null;

        // If there a dot left
        int dotIndex = fileNameNoExt.LastIndexOf('.');

        if (dotIndex < 0) return null;

        string cultureName = fileNameNoExt.Substring(dotIndex+1);

        // If it doesn't look like a culture name (e.g. "fr" or "fr-fr"), return null
        if (!IsCultureName(cultureName))
            return null;

        return cultureName;
    }


    /*
     * Returns true if the type string contains an assembly specification
     */
    internal static bool TypeNameContainsAssembly(string typeName) {
        return CommaIndexInTypeName(typeName) > 0;
    }

    /*
     * Returns the index of the comma separating the type from the assembly, or
     * -1 of there is no assembly
     */
    internal static int CommaIndexInTypeName(string typeName) {

        // Look for the last comma
        int commaIndex = typeName.LastIndexOf(',');

        // If it doesn't have one, there is no assembly
        if (commaIndex < 0)
            return -1;

        // It has a comma, we need to account for the generics syntax.
        // E.g. it could be "SomeType[int,string]

        // Check for a ]
        int rightBracketIndex = typeName.LastIndexOf(']');

        // If it has one, and it's after the last comma, there is no assembly
        if (rightBracketIndex > commaIndex)
            return -1;

        // The comma that we want is the first one after the last ']'
        commaIndex = typeName.IndexOf(',', rightBracketIndex + 1);

        // There is an assembly
        return commaIndex;
    }

    /*
     * Return the full path (non shadow copied) to the assembly that
     * the given type lives in.
     */
    internal static string GetAssemblyPathFromType(Type t) {
        return Util.FilePathFromFileUrl(t.Assembly.EscapedCodeBase);
    }

    /*
     * Same as GetAssemblyPathFromType, but with path safety check
     */
    internal static string GetAssemblySafePathFromType(Type t) {
        return HttpRuntime.GetSafePath(GetAssemblyPathFromType(t));
    }

    [FileIOPermission(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.PathDiscovery)]
    internal static string GetAssemblyQualifiedTypeName(Type t) {
        if (t.Assembly.GlobalAssemblyCache)
            return t.AssemblyQualifiedName;

        // For non-GAC types, t.AssemblyQualifiedName still returns a big ugly type string,
        // so return a simpler one instead with just "typename, assemblyName".
        return t.FullName + ", " + t.Assembly.GetName().Name;
    }

    internal static string GetAssemblyShortName(Assembly a) {

        // Getting the short name is always safe, so Assert to get it (VSWhidbey 491895)
        InternalSecurityPermissions.Unrestricted.Assert();

        return a.GetName().Name;
    }

    /*
     * Check if the passed in type is for a late bound COM object.  This
     * is what we would get when calling Type.GetTypeFromProgID() on a progid
     * that has not been tlbimp'ed.
     */
    internal static bool IsLateBoundComClassicType(Type t) {
        // 
        return (String.Compare(t.FullName, "System.__ComObject", StringComparison.Ordinal) == 0);
    }

    /*
     * Get the path to the (shadow copied) DLL behind an assembly
     */
    [FileIOPermission(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.PathDiscovery)]
    internal static string GetAssemblyCodeBase(Assembly assembly) {

        string location = assembly.Location;
        if (String.IsNullOrEmpty(location))
            return null;

        // Get the path to the assembly (from the cache if it got shadow copied)
        return location;
    }

    /*
     * Add the full path to the assembly to the string collection if it's not already there.
     * This method uses the path to the target reference assembly for framework assemblies.
     * If an assembly exists only in a higher version framework, it will be skipped and NOT
     * added to the list.
     */
    internal static void AddAssemblyToStringCollection(Assembly assembly, StringCollection toList) {

        string assemblyPath = null;

        //Skip adding Mscorlib for versions from 4.0 as that is added by CodeDomProvider (because of CoreAssemblyFileName switch).
        if (BuildManagerHost.InClientBuildManager && !MultiTargetingUtil.IsTargetFramework20 && !MultiTargetingUtil.IsTargetFramework35) {
            if (assembly.FullName == typeof(string).Assembly.FullName) {
                return;
            }
        }

        if (!MultiTargetingUtil.EnableReferenceAssemblyResolution) {
            assemblyPath = Util.GetAssemblyCodeBase(assembly);

        } else {
            // Get the full path to the reference assembly. For framework assemblies, this will be the path
            // to the actual target reference assembly.
            ReferenceAssemblyType referenceAssemblyType = AssemblyResolver.GetPathToReferenceAssembly(assembly, out assemblyPath);

            // If the assembly is only available in a higher framework version, skip it.
            // If the user tries to use anything from such an assembly, he should be getting errors 
            // during actual csc/vbc compilation reporting that the type or method is not found.
            if (referenceAssemblyType == ReferenceAssemblyType.FrameworkAssemblyOnlyPresentInHigherVersion) {
                return;
            }
        }

        Debug.Assert(!String.IsNullOrEmpty(assemblyPath));

        // Unless it's already in the list, add it
        if (!toList.Contains(assemblyPath)) {
            toList.Add(assemblyPath);
        }
    }

    /*
     * Add the full path to all the assemblies to the string collection if they're not already there.
     * This method uses the path to the target reference assembly for framework assemblies.
     */
    internal static void AddAssembliesToStringCollection(ICollection fromList, StringCollection toList) {

        // Nothing to do if either is null
        if (fromList == null || toList == null)
            return;

        foreach (Assembly assembly in fromList) {
            AddAssemblyToStringCollection(assembly, toList);
        }
    }

    /*
     * Return an AssemblySet which contains all the assemblies that
     * are referenced by the input assembly
     */
    internal static AssemblySet GetReferencedAssemblies(Assembly a) {

        AssemblySet referencedAssemblies = new AssemblySet();
        AssemblyName[] refs = a.GetReferencedAssemblies();

        foreach (AssemblyName aname in refs) {
            Assembly referencedAssembly = Assembly.Load(aname);

            // Ignore mscorlib
            if (referencedAssembly == typeof(string).Assembly)
                continue;

            referencedAssemblies.Add(referencedAssembly);
        }

        return referencedAssemblies;
    }

    /*
     * Return an assembly name from the name of an assembly dll.
     * Basically, it strips the extension.
     */
    internal static string GetAssemblyNameFromFileName(string fileName) {
        // Strip the .dll extension if any
        if (StringUtil.EqualsIgnoreCase(Path.GetExtension(fileName), ".dll"))
            return fileName.Substring(0, fileName.Length-4);

        return fileName;
    }

    /*
     * Look for a type by name in a collection of assemblies.  If it exists in multiple assemblies,
     * throw an error.
     */
    // Assert reflection in order to call assembly.GetType()
    [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
    internal static Type GetTypeFromAssemblies(IEnumerable assemblies, string typeName, bool ignoreCase) {
        if (assemblies == null)
            return null;

        Type type = null;

        foreach (Assembly assembly in assemblies) {
            Type t = assembly.GetType(typeName, false /*throwOnError*/, ignoreCase);

            if (t == null)
                continue;

            // If we had already found a different one, it's an ambiguous type reference
            if (type != null && t != type) {
                throw new HttpException(SR.GetString(SR.Ambiguous_type, typeName,
                    GetAssemblySafePathFromType(type), GetAssemblySafePathFromType(t)));
            }

            // Keep track of it
            type = t;
        }

        return type;
    }

    internal static string GetCurrentAccountName() {
        try {
            return HttpApplication.GetCurrentWindowsIdentityWithAssert().Name;
        }
        catch {
            // WindowsIdentity.GetCurrent() can throw.  Return "?" when that happens
            return "?";
        }
    }

    internal static string GetUrlWithApplicationPath(HttpContextBase context, string url) {
        string appPath = context.Request.ApplicationPath ?? String.Empty;
        if (!appPath.EndsWith("/", StringComparison.OrdinalIgnoreCase)) {
            appPath += "/";
        }

        return context.Response.ApplyAppPathModifier(appPath + url);
    }


    internal static string QuoteJScriptString(string value) {
        return QuoteJScriptString(value, false);
    }

    internal static string QuoteJScriptString(string value, bool forUrl) {
        StringBuilder b = null;

        if (String.IsNullOrEmpty(value)) {
            return String.Empty;
        }

        int startIndex = 0;
        int count = 0;
        for (int i=0; i<value.Length; i++) {
            switch (value[i]) {
                case '\r':
                    if (b == null) {
                        b = new StringBuilder(value.Length+5);
                    }

                    if (count > 0) {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\r");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\t':
                    if (b == null) {
                        b = new StringBuilder(value.Length+5);
                    }

                    if (count > 0) {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\t");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\"':
                    if (b == null) {
                        b = new StringBuilder(value.Length+5);
                    }

                    if (count > 0) {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\\"");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\'':
                    if (b == null) {
                        b = new StringBuilder(value.Length+5);
                    }

                    if (count > 0) {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\\'");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\\':
                    if (b == null) {
                        b = new StringBuilder(value.Length+5);
                    }

                    if (count > 0) {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\\\");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '\n':
                    if (b == null) {
                        b = new StringBuilder(value.Length+5);
                    }

                    if (count > 0) {
                        b.Append(value, startIndex, count);
                    }

                    b.Append("\\n");

                    startIndex = i + 1;
                    count = 0;
                    break;
                case '%':
                    if (forUrl) {
                        if (b == null) {
                            b = new StringBuilder(value.Length + 6);
                        }
                        if (count > 0) {
                            b.Append(value, startIndex, count);
                        }
                        b.Append("%25");

                        startIndex = i + 1;
                        count = 0;
                        break;
                    }
                    goto default;
                default:
                    count++;
                    break;
            }
        }

        if (b == null) {
            return value;
        }

        if (count > 0) {
            b.Append(value, startIndex, count);
        }

        return b.ToString();
    }

    private static ArrayList GetSpecificCultures(string shortName) {
        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
        ArrayList list = new ArrayList();

        for (int i=0; i<cultures.Length; i++) {
            if (StringUtil.StringStartsWith(cultures[i].Name, shortName))
                list.Add(cultures[i]);
        }

        return list;
    }

    internal static string GetSpecificCulturesFormattedList(CultureInfo cultureInfo) {
        ArrayList myCultures = GetSpecificCultures(cultureInfo.Name);

        string s = null;
        foreach (CultureInfo culture in myCultures) {
            if (s == null)
                s = culture.Name;
            else
                s += ", " + culture.Name;
        }

        return s;
    }

    // Client Validation Utility Functions

    internal static string GetClientValidateEvent(string validationGroup) {
        if (validationGroup == null) {
            validationGroup = String.Empty;
        }
        return "if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate('" +
               validationGroup +
               "'); ";
    }

    internal static string GetClientValidatedPostback(Control control, string validationGroup) {
        return GetClientValidatedPostback(control, validationGroup, string.Empty);
    }

    internal static string GetClientValidatedPostback(Control control,
                                                      string validationGroup,
                                                      string argument) {
        string postbackReference = control.Page.ClientScript.GetPostBackEventReference(control, argument, true);
        return GetClientValidateEvent(validationGroup) + postbackReference;
    }

    internal static void WriteOnClickAttribute(HtmlTextWriter writer,
                                               HtmlControls.HtmlControl control,
                                               bool submitsAutomatically,
                                               bool submitsProgramatically,
                                               bool causesValidation,
                                               string validationGroup) {
        AttributeCollection attributes = control.Attributes;
        string injectedOnClick = null;
        if (submitsAutomatically) {
            if (causesValidation) {
                injectedOnClick = Util.GetClientValidateEvent(validationGroup);
            }
            control.Page.ClientScript.RegisterForEventValidation(control.UniqueID);
        }
        else if (submitsProgramatically) {
            if (causesValidation) {
                injectedOnClick = Util.GetClientValidatedPostback(control, validationGroup);
            }
            else {
                injectedOnClick = control.Page.ClientScript.GetPostBackEventReference(control, String.Empty, true);
            }
        }
        else {
            control.Page.ClientScript.RegisterForEventValidation(control.UniqueID);
        }

        if (injectedOnClick != null) {
            string existingOnClick = attributes["onclick"];
            if (existingOnClick != null) {
                attributes.Remove("onclick");
                writer.WriteAttribute("onclick", existingOnClick + " " + injectedOnClick);
            }
            else {
                writer.WriteAttribute("onclick", injectedOnClick);
            }
        }
    }

    internal static string EnsureEndWithSemiColon(string value) {
        if (value != null) {
            int length = value.Length;
            if (length > 0 && value[length - 1] != ';') {
                return (value + ";");
            }
        }
        return value;
    }

    internal static string MergeScript(string firstScript, string secondScript) {
        Debug.Assert(!String.IsNullOrEmpty(secondScript));

        if (!String.IsNullOrEmpty(firstScript)) {
            // 
            return firstScript + secondScript;
        }
        else {
            if (secondScript.TrimStart().StartsWith(ClientScriptManager.JscriptPrefix, StringComparison.Ordinal)) {
                return secondScript;
            }
            return ClientScriptManager.JscriptPrefix + secondScript;
        }
    }

    internal static bool IsUserAllowedToPath(HttpContext context, VirtualPath virtualPath) {
        // Check FileAuthorizationModule if it's a windows identity
        if (FileAuthorizationModule.IsWindowsIdentity(context)) {
            if (HttpRuntime.IsFullTrust) {
                if (!IsUserAllowedToPathWithNoAssert(context, virtualPath)) {
                    return false;
                }
            }
            else {
                if (!IsUserAllowedToPathWithAssert(context, virtualPath)) {
                    return false;
                }
            }
        }

        // Always check UrlAuthorizationModule
        return UrlAuthorizationModule.IsUserAllowedToPath(context, virtualPath);
    }

    // Need to assert here in order to MapPath in the FileAuthorizationModule.
    [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
    private static bool IsUserAllowedToPathWithAssert(HttpContext context, VirtualPath virtualPath) {
        return IsUserAllowedToPathWithNoAssert(context, virtualPath);
    }

    private static bool IsUserAllowedToPathWithNoAssert(HttpContext context, VirtualPath virtualPath) {
        return FileAuthorizationModule.IsUserAllowedToPath(context, virtualPath);
    }

#if DBG
    internal static void DumpDictionary(string tag, IDictionary d) {
        if (d == null) return;

        Debug.Trace(tag, "Dumping IDictionary with " + d.Count + " entries:");

        for (IDictionaryEnumerator en = (IDictionaryEnumerator)d.GetEnumerator(); en.MoveNext();) {
            if (en.Value == null)
                Debug.Trace(tag, "Key='" + en.Key.ToString() + "' value=null");
            else
                Debug.Trace(tag, "Key='" + en.Key.ToString() + "' value='" + en.Value.ToString() + "'");
        }
    }

    internal static void DumpArrayList(string tag, ArrayList al) {
        if (al == null) return;

        Debug.Trace(tag, "Dumping ArrayList with " + al.Count + " entries:");

        foreach (object o in al) {
            if (o == null)
                Debug.Trace(tag, "value=null");
            else
                Debug.Trace(tag, "value='" + o.ToString() + "'");
        }
    }

    internal static void DumpString(string tag, string s) {
        Debug.Trace(tag, "Dumping string  '" + s + "':");

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < s.Length; ++i) {
            sb.Append(((int)s[i]).ToString("x", CultureInfo.InvariantCulture));
            sb.Append(" ");
        }
        Debug.Trace(tag, sb.ToString());
    }

#endif // DBG

}

}
