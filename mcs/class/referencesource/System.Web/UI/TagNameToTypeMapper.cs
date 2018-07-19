//------------------------------------------------------------------------------
// <copyright file="TagNameToTypeMapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Base Control factory implementation
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Globalization;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.Compilation;
    using System.Web.Configuration;
#if !FEATURE_PAL
    using System.Web.UI.Design;
#endif // !FEATURE_PAL

// 




    /// <devdoc>
    ///    <para>Maps a sequence of text characters to a .NET Framework 
    ///       type when an .aspx file is processed on the server.</para>
    /// </devdoc>
    internal interface ITagNameToTypeMapper {
        /*
         * Return the Type of the control that should handle a tag with the
         * passed in properties.
         */

        /// <devdoc>
        ///    <para>Retrieves the .NET Framework type that should process 
        ///       the control declared in the .aspx file.</para>
        ///    </devdoc>
        Type GetControlType(string tagName, IDictionary attribs);
    }


    internal class NamespaceTagNameToTypeMapper : ITagNameToTypeMapper {
        private TagNamespaceRegisterEntry _nsRegisterEntry;
        private Assembly _assembly;
        private TemplateParser _parser;

        internal NamespaceTagNameToTypeMapper(TagNamespaceRegisterEntry nsRegisterEntry, Assembly assembly, TemplateParser parser) {
            _nsRegisterEntry = nsRegisterEntry;
            _assembly = assembly;
            _parser = parser;
        }

        public TagNamespaceRegisterEntry RegisterEntry {
            get {
                return _nsRegisterEntry;
            }
        }

        Type ITagNameToTypeMapper.GetControlType(string tagName, IDictionary attribs) {
            return GetControlType(tagName, attribs, false);
        }

        internal Type GetControlType(string tagName, IDictionary attribs, bool throwOnError) {
            string typeName;
            string ns = _nsRegisterEntry.Namespace;
            if (String.IsNullOrEmpty(ns)) {
                typeName = tagName;
            }
            else {
                typeName = ns + "." + tagName;
            }

            // Look up the type name (case insensitive)
            if (_assembly != null) {
                Type type = null;

                if (throwOnError) {
                    // If loading the type from the assembly depends on a referenced assembly that cannot
                    // be loaded, we should throw the actual exception, instead of later reporting a more 
                    // generic error saying the type was not found (Devdiv 138674)
                    try {
                        type = _assembly.GetType(typeName, true /*throwOnError*/, true /*ignoreCase*/);
                    }
                    catch (System.IO.FileNotFoundException) {
                        throw;
                    }
                    catch (System.IO.FileLoadException) {
                        throw;
                    }
                    catch (BadImageFormatException) {
                        throw;
                    }
                    catch {
                        // For all other exceptions, such as when the type is not present in the assembly,
                        // we ignore the exception so that we can continue to check other assemblies to look
                        // for the type.
                    }
                }
                else {
                    type = _assembly.GetType(typeName, false /*throwOnError*/, true /*ignoreCase*/);
                }
                return type;
            }

#if !FEATURE_PAL
            // If we're in the designer, check the WebFormsReferenceManager and ITypeResolutionService first.
            if (_parser.FInDesigner && (_parser.DesignerHost != null)) {
                // If we are in the DesignTimeThemes Host, we can't actually go down this code path, let the TypeResolutionService try instead
                if (_parser.DesignerHost.RootComponent != null) {
                    WebFormsRootDesigner rootDesigner = _parser.DesignerHost.GetDesigner(_parser.DesignerHost.RootComponent) as WebFormsRootDesigner;
                    if (rootDesigner != null) {
                        WebFormsReferenceManager referenceManager = rootDesigner.ReferenceManager;
                        if (referenceManager != null) {
                            Type type = referenceManager.GetType(_nsRegisterEntry.TagPrefix, tagName);
                            if (type != null) {
                                // Only return the type if we found it. Otherwise fall back to the next service.
                                return type;
                            }
                        }
                    }
                }
                                
                ITypeResolutionService typeResolutionService = (ITypeResolutionService)_parser.DesignerHost.GetService(typeof(ITypeResolutionService));
                if (typeResolutionService != null) {
                    Type type = typeResolutionService.GetType(typeName, false, true);
                    if (type != null) {
                        // Only return the type if we found it. Otherwise fall back to the next service.
                        return type;
                    }
                }
            }

#endif // !FEATURE_PAL

            // Nothing more to try in non-hosted appdomains
            if (!HostingEnvironment.IsHosted)
                return null;

            // If the assembly was not specified, look for the type in the code assemblies (including sub code assemblies)
            return BuildManager.GetTypeFromCodeAssembly(typeName, true /*ignoreCase*/);
        }
    }

    internal class TagPrefixTagNameToTypeMapper : ITagNameToTypeMapper {

        private string _tagPrefix;
        private ArrayList _mappers;

        internal TagPrefixTagNameToTypeMapper(string tagPrefix) {
            _tagPrefix = tagPrefix;
            _mappers = new ArrayList();
        }

        internal void AddNamespaceMapper(NamespaceTagNameToTypeMapper mapper) {
            _mappers.Add(mapper);
        }

        Type ITagNameToTypeMapper.GetControlType(string tagName, IDictionary attribs) {
            Type foundType = null;
            Exception loadException = null;

            foreach (NamespaceTagNameToTypeMapper nsMapper in _mappers) {
                Type t = ((ITagNameToTypeMapper)nsMapper).GetControlType(tagName, attribs);
                if (t != null) {
                    if (foundType == null) {
                        foundType = t;
                    }
                    else if (foundType != t) {
                        throw new HttpParseException(SR.GetString(SR.Ambiguous_server_tag, _tagPrefix + ":" + tagName),
                                                     null, nsMapper.RegisterEntry.VirtualPath, null, nsMapper.RegisterEntry.Line);
                    }
                }
            }

            // DevDiv 168561
            // If we cannot find the type, we make a second pass and allow exceptions to be thrown so that we can
            // track the actual failure.
            if (foundType == null) {
                try {
                    foreach (NamespaceTagNameToTypeMapper nsMapper in _mappers) {
                        nsMapper.GetControlType(tagName, attribs, true /* throwOnError */);
                    }
                }
                catch (System.IO.FileNotFoundException e) {
                    loadException = e;
                }
                catch (System.IO.FileLoadException e) {
                    loadException = e;
                }
                catch (BadImageFormatException e) {
                    loadException = e;
                }
            }

            if (loadException != null) {
                throw new HttpException(SR.GetString(SR.ControlAdapters_TypeNotFound, _tagPrefix + ":" + tagName) + " " + loadException.Message, loadException);
            }

            if (foundType == null) {
                throw new HttpException(SR.GetString(SR.Unknown_server_tag, _tagPrefix + ":" + tagName));
            }

            return foundType;
        }
    }


    internal class MainTagNameToTypeMapper {

        private BaseTemplateParser _parser;

        // Mapping from a tag prefix to an ITagNameToTypeMapper for that prefix
        private IDictionary _prefixedMappers;

        // Mapping from a tag name (possibly with a prefix) to a Type
        private IDictionary _mappedTags;

        // Create the Html tag mapper
        private ITagNameToTypeMapper _htmlMapper = new HtmlTagNameToTypeMapper();

        // List of UserControl <%@ Register %> directives
        // Key = "prefix:tagname", Value = UserControlRegisterEntry
        private Hashtable _userControlRegisterEntries;

        // List of custom control <%@ Register %> directives
        private List<TagNamespaceRegisterEntry> _tagRegisterEntries;

        // <prefix, TagNamespaceRegisterEntry>
        private TagNamespaceRegisterEntryTable _tagNamespaceRegisterEntries;

        internal MainTagNameToTypeMapper(BaseTemplateParser parser) {
            _parser = parser;
            
            if (parser != null) {
                PagesSection pagesConfig = parser.PagesConfig;
                if (pagesConfig != null) {
                    // Clone it so we don't modify the config settings
                    _tagNamespaceRegisterEntries = pagesConfig.TagNamespaceRegisterEntriesInternal;
                    if (_tagNamespaceRegisterEntries != null) {
                        _tagNamespaceRegisterEntries = (TagNamespaceRegisterEntryTable)_tagNamespaceRegisterEntries.Clone();
                    }

                    _userControlRegisterEntries = pagesConfig.UserControlRegisterEntriesInternal;
                    if (_userControlRegisterEntries != null) {
                        _userControlRegisterEntries = (Hashtable)_userControlRegisterEntries.Clone();
                    }
                }

                // 



                if (parser.FInDesigner && (_tagNamespaceRegisterEntries == null)) {
                    _tagNamespaceRegisterEntries = new TagNamespaceRegisterEntryTable();

                    foreach (TagNamespaceRegisterEntry entry in PagesSection.DefaultTagNamespaceRegisterEntries) {
                        _tagNamespaceRegisterEntries[entry.TagPrefix] = new ArrayList(new object[] { entry });
                    }
                }
            }
        }

        internal ICollection UserControlRegisterEntries {
            get {
                if (_userControlRegisterEntries != null) {
                    return _userControlRegisterEntries.Values;
                }

                return null;
            }
        }

        internal List<TagNamespaceRegisterEntry> TagRegisterEntries {
            get {
                if (_tagRegisterEntries == null) {
                    _tagRegisterEntries = new List<TagNamespaceRegisterEntry>();
                }
                return _tagRegisterEntries;
            }
        }

        // Called to process register directive on pages
        internal void ProcessTagNamespaceRegistration(TagNamespaceRegisterEntry nsRegisterEntry) {
            string tagPrefix = nsRegisterEntry.TagPrefix;

            // See if there are entries registered in config with same tag prefix
            ArrayList registerEntries = null;
            if (_tagNamespaceRegisterEntries != null) {
                registerEntries = (ArrayList)_tagNamespaceRegisterEntries[tagPrefix];
            }

            // If there are config-based entries, and theres no mapper for them, make sure
            // a mapper has been created
            // We need all namespaces for a particular tag prefix to have a mapper, so we
            // can look for ambiguous types when a tag is parsed.
            if ((registerEntries != null) &&
                ((_prefixedMappers == null) || (_prefixedMappers[tagPrefix] == null))) {
                ProcessTagNamespaceRegistration(registerEntries);
            }
            ProcessTagNamespaceRegistrationCore(nsRegisterEntry);
        }

        private void ProcessTagNamespaceRegistration(ArrayList nsRegisterEntries) {
            foreach (TagNamespaceRegisterEntry nsRegisterEntry in nsRegisterEntries) {
                try {
                    ProcessTagNamespaceRegistrationCore(nsRegisterEntry);
                }
                catch (Exception e) {
                    // Make sure we throw the exception with the correct file/line info
                    throw new HttpParseException(e.Message, e,
                        nsRegisterEntry.VirtualPath, null, nsRegisterEntry.Line);
                }
            }
        }

        private void ProcessTagNamespaceRegistrationCore(TagNamespaceRegisterEntry nsRegisterEntry) {
            // Load the assembly if it was specified
            Assembly assembly = null;
            if (!String.IsNullOrEmpty(nsRegisterEntry.AssemblyName))
                assembly = _parser.AddAssemblyDependency(nsRegisterEntry.AssemblyName);

            // Import the namespace if there is one
            if (!String.IsNullOrEmpty(nsRegisterEntry.Namespace))
                _parser.AddImportEntry(nsRegisterEntry.Namespace);

            // Create a mapper for this specific namespace/assembly pair
            NamespaceTagNameToTypeMapper mapper = new NamespaceTagNameToTypeMapper(nsRegisterEntry, assembly, _parser);

            // Figure out which prefix mapper this new namespace mapper goes into
            if (_prefixedMappers == null) {
                _prefixedMappers = new Hashtable(StringComparer.OrdinalIgnoreCase);
            }

            TagPrefixTagNameToTypeMapper prefixMapper = (TagPrefixTagNameToTypeMapper)_prefixedMappers[nsRegisterEntry.TagPrefix];
            if (prefixMapper == null) {
                prefixMapper = new TagPrefixTagNameToTypeMapper(nsRegisterEntry.TagPrefix);
                _prefixedMappers[nsRegisterEntry.TagPrefix] = prefixMapper;
            }

            prefixMapper.AddNamespaceMapper(mapper);

            TagRegisterEntries.Add(nsRegisterEntry);
        }

        internal void ProcessUserControlRegistration(UserControlRegisterEntry ucRegisterEntry) {
            Type type = null;

            if (_parser.FInDesigner) {
                // Get the designer to load the appropriate type
                type = _parser.GetDesignTimeUserControlType(ucRegisterEntry.TagPrefix,
                    ucRegisterEntry.TagName);
            }
            else {
                // Compile it into a Type
                type = _parser.GetUserControlType(ucRegisterEntry.UserControlSource.VirtualPathString);
            }

            if (type == null)
                return;

            if (_userControlRegisterEntries == null) {  
                _userControlRegisterEntries = new Hashtable();
            }
            _userControlRegisterEntries[ucRegisterEntry.TagPrefix + ":" + ucRegisterEntry.TagName] = ucRegisterEntry;

            // Register the new tag, including its prefix
            RegisterTag(ucRegisterEntry.TagPrefix + ":" + ucRegisterEntry.TagName, type);
        }

        /*
         * Check if the tagName can be handled by a user control register directive.
         * If so, process the directive and return true.
         */
        private bool TryUserControlRegisterDirectives(string tagName) {
        
            if (_userControlRegisterEntries == null)
                return false;

            // Is there a registered user control for this tag
            UserControlRegisterEntry ucRegisterEntry = (UserControlRegisterEntry)
                _userControlRegisterEntries[tagName];
            if (ucRegisterEntry == null)
                return false;

            // Don't allow a config registered user control to be used by a
            // page that lives in the same directory, to avoid circular references.
            // More specifically, this would break batching because our simple dependency
            // parser would not be able to detect the *implicit* dependency triggered
            // by the presence of a tag (VSWhidbey 165042).
            if (ucRegisterEntry.ComesFromConfig) {
                VirtualPath ucDirectory = ucRegisterEntry.UserControlSource.Parent;
                if (ucDirectory == _parser.BaseVirtualDir) {
                    throw new HttpException(
                        SR.GetString(SR.Invalid_use_of_config_uc,
                            _parser.CurrentVirtualPath, ucRegisterEntry.UserControlSource));
                }
            }

            try {
                ProcessUserControlRegistration(ucRegisterEntry);
            }
            catch (Exception e) {
                // Make sure we throw the exception with the correct file/line info
                throw new HttpParseException(e.Message, e,
                    ucRegisterEntry.VirtualPath, null, ucRegisterEntry.Line);
            }
            return true;
        }

        /*
         * Check if the tagName can be handled by a namespace register directive.
         * If so, process the directive and return true.
         */
        private bool TryNamespaceRegisterDirectives(string prefix) {
            if (_tagNamespaceRegisterEntries == null)
                return false;

            // Are there registered entries for this prefix
            ArrayList entries = (ArrayList)_tagNamespaceRegisterEntries[prefix];
            if (entries == null) {
                return false;
            }

            ProcessTagNamespaceRegistration(entries);
            return true;
        }

        internal void RegisterTag(string tagName, Type type) {
            if (_mappedTags == null)
                _mappedTags = new Hashtable(StringComparer.OrdinalIgnoreCase);

            try {
                _mappedTags.Add(tagName, type);
            }
            catch (ArgumentException) {
                // Duplicate mapping
                throw new HttpException(SR.GetString(SR.Duplicate_registered_tag, tagName));
            }
        }

        internal /*public*/ Type GetControlType(string tagName, IDictionary attribs, bool fAllowHtmlTags) {
            Type type = GetControlType2(tagName, attribs, fAllowHtmlTags);

            if ((type != null) && _parser != null && !_parser.FInDesigner) {
                Hashtable tagMapEntries = _parser.PagesConfig.TagMapping.TagTypeMappingInternal;
                if (tagMapEntries != null) {
                    Type mappedType = (Type)tagMapEntries[type];
                    if (mappedType != null) {
                        type = mappedType;
                    }
                }
            }

            return type;
        }

        private /*public*/ Type GetControlType2(string tagName, IDictionary attribs, bool fAllowHtmlTags) {
            Type type;

            // First, check it the tag name has been mapped
            if (_mappedTags != null) {
                type = (Type) _mappedTags[tagName];

                if (type == null) {
                    // Maybe there is a register directive that we haven't yet processed
                    if (TryUserControlRegisterDirectives(tagName))
                        type = (Type) _mappedTags[tagName];
                }

                if (type != null) {
                    // If this is the special NoCompile UserControl specified by the PageParserFilter, give it the
                    // virtualPath via the attribute bag
                    if (_parser != null && _parser._pageParserFilter != null && _parser._pageParserFilter.GetNoCompileUserControlType() == type) {
                        UserControlRegisterEntry ucRegisterEntry = (UserControlRegisterEntry)_userControlRegisterEntries[tagName];
                        attribs["virtualpath"] = ucRegisterEntry.UserControlSource;
                    }

                    return type;
                }
            }

            // Check if there is a prefix
            int colonIndex = tagName.IndexOf(':');
            if (colonIndex >= 0) {
                // If ends with : don't try to match (88398)
                if (colonIndex == tagName.Length-1)
                    return null;

                // If so, parse the prefix and tagname
                string prefix = tagName.Substring(0, colonIndex);
                tagName = tagName.Substring(colonIndex+1);

                // Look for a mapper for the prefix

                ITagNameToTypeMapper mapper = null;
                if (_prefixedMappers != null)
                    mapper = (ITagNameToTypeMapper) _prefixedMappers[prefix];

                if (mapper == null) {
                    // Maybe there is a register directive that we haven't yet processed
                    if (TryNamespaceRegisterDirectives(prefix) && _prefixedMappers != null) {
                        mapper = (ITagNameToTypeMapper)_prefixedMappers[prefix];
                    }
                }

                if (mapper == null) {
                    return null;
                }

                // Try to get the type from the prefix mapper
                return mapper.GetControlType(tagName, attribs);
            }
            else {
                // There is no prefix.
                // Try the Html mapper if allowed
                if (fAllowHtmlTags) {
                    return _htmlMapper.GetControlType(tagName, attribs);
                }
            }

            return null;
        }
    }
}
