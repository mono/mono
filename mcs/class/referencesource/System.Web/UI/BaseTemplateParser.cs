//------------------------------------------------------------------------------
// <copyright file="BaseTemplateParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Implements the ASP.NET template parser
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {

    using System.Text;
    using System;
    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Configuration;
    using System.Web.Caching;
    using System.Web.Util;
    using System.Web.Hosting;
    using System.Web.Compilation;
    using HttpException = System.Web.HttpException;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using System.Security.Permissions;

    /*
     * Parser for Template Files (TemplateControls and PageTheme)
     */

    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class BaseTemplateParser : TemplateParser {

        private const string _sourceString = "src";
        private const string _namespaceString = "namespace";
        private const string _tagnameString = "tagname";

        internal Type GetDesignTimeUserControlType(string tagPrefix, string tagName) {
            Debug.Assert(FInDesigner);

            Type type = typeof(UserControl);

            IDesignerHost host = DesignerHost;
            if (host != null) {
                IUserControlTypeResolutionService ucTypeResService =
                    (IUserControlTypeResolutionService)host.GetService(typeof(IUserControlTypeResolutionService));
                if (ucTypeResService != null) {
                    try {
                        type = ucTypeResService.GetType(tagPrefix, tagName);
                    }
                    catch {
                    }
                }
            }

            return type;
        }
    
        /*
         * Compile a nested .ascx file (a User Control) and return its Type
         */

        protected internal Type GetUserControlType(string virtualPath) {
            return GetUserControlType(VirtualPath.Create(virtualPath));
        }

        internal Type GetUserControlType(VirtualPath virtualPath) {
            Type t = GetReferencedType(virtualPath, false /*allowNoCompile*/);

            // Fail if it's a no compile uc, since it doesn't have a Type we can use
            if (t == null) {
                // First, check whether there is a PageParserFilter that can give us a type
                if (_pageParserFilter != null)
                    t = _pageParserFilter.GetNoCompileUserControlType();

                if (t == null)
                    ProcessError(SR.GetString(SR.Cant_use_nocompile_uc, virtualPath));
            }
            else {
                // Make sure it has the correct base type
                Util.CheckAssignableType(typeof(UserControl), t);
            }

            return t;
        }

        /*
         * Compile a .aspx/.ascx file and return its Type
         */

        protected Type GetReferencedType(string virtualPath) {
            return GetReferencedType(VirtualPath.Create(virtualPath));
        }

        internal Type GetReferencedType(VirtualPath virtualPath) {
            return GetReferencedType(virtualPath, true /*allowNoCompile*/);
        }

        internal Type GetReferencedType(VirtualPath virtualPath, bool allowNoCompile) {

            virtualPath = ResolveVirtualPath(virtualPath);

            // If we have a page parser filter, make sure the reference is allowed
            if (_pageParserFilter != null && !_pageParserFilter.AllowVirtualReference(CompConfig, virtualPath)) {
                ProcessError(SR.GetString(SR.Reference_not_allowed, virtualPath));
            }

            BuildResult result = null;
            Type t = null;

            try {
                result = BuildManager.GetVPathBuildResult(virtualPath);
            }
            catch (HttpCompileException e) {
                // Add the path depdencies properly so we know when
                // to invalidate the cached result.
                if (e.VirtualPathDependencies != null) {
                    foreach (string vPath in e.VirtualPathDependencies) {
                        AddSourceDependency(VirtualPath.Create(vPath));
                    }
                }

                throw;
            }
            catch {
                // Add the virtualPath to the dependency so that
                // we know when to check again. This could happen if the
                // virtualPath points to a file not created yet.
                // This only affects designtime code path since we do want to return
                // partial result even if there is an error, and that result is
                // cached. VSWhidbey 372585
                if (IgnoreParseErrors) {
                    AddSourceDependency(virtualPath);
                }

                throw;
            }

            // Is it a no-compile page/uc
            BuildResultNoCompileTemplateControl noCompileResult = result as BuildResultNoCompileTemplateControl;
            if (noCompileResult != null) {

                // If no-compile is not acceptable, return null
                if (!allowNoCompile)
                    return null;

                // In the no-compile case, use the base type, since we don't compile a type
                t = noCompileResult.BaseType;
            }
            else if (result is BuildResultCompiledType) {
                BuildResultCompiledType compiledResult = (BuildResultCompiledType) result;
                Debug.Assert(compiledResult != null);

                t = compiledResult.ResultType;
            }
            else {
                throw new HttpException(SR.GetString(SR.Invalid_typeless_reference, _sourceString));
            }

            Debug.Assert(t != null);

            // Add a dependency on the Type
            AddTypeDependency(t);

            // Add a dependency on the BuildResult
            AddBuildResultDependency(result);

            return t;
        }

        internal override void ProcessDirective(string directiveName, IDictionary directive) {
            if (StringUtil.EqualsIgnoreCase(directiveName, "register")) {
                // Register directive

                // Get the tagprefix, which is required
                string tagPrefix = Util.GetAndRemoveNonEmptyIdentifierAttribute(directive,
                    "tagprefix", true /*required*/);

                string tagName = Util.GetAndRemoveNonEmptyIdentifierAttribute(directive,
                    _tagnameString, false /*required*/);

                VirtualPath src = Util.GetAndRemoveVirtualPathAttribute(directive, 
                    _sourceString, false /*required*/);

                string ns = Util.GetAndRemoveNonEmptyNoSpaceAttribute(directive, 
                    _namespaceString, false /*required*/);

                // An Assembly can optionally be specified (ASURT 61326/VSWhidbey 87050)
                string assemblyName = Util.GetAndRemoveNonEmptyAttribute(directive, "assembly",
                    false /*required*/);

                RegisterDirectiveEntry registerEntry;
                if (tagName != null) {
                    // It's a user control registration

                    // 'src' is required
                    if (src == null) {
                        throw new HttpException(SR.GetString(SR.Missing_attr, _sourceString));
                    }

                    // 'namespace' is not allowed
                    if (ns != null) {
                        throw new HttpException(
                            SR.GetString(SR.Invalid_attr, _namespaceString, "tagname"));
                    }

                    // 'assembly' is not allowed
                    if (assemblyName != null) {
                        throw new HttpException(
                            SR.GetString(SR.Invalid_attr, "assembly", "tagname"));
                    }

                    UserControlRegisterEntry ucRegisterEntry = new UserControlRegisterEntry(tagPrefix, tagName);
                    ucRegisterEntry.UserControlSource = src;
                    registerEntry = ucRegisterEntry;

                    TypeMapper.ProcessUserControlRegistration(ucRegisterEntry);
                }
                else if (src != null) {
                    // It's missing the tagname attribute.
                    throw new HttpException(SR.GetString(SR.Missing_attr, _tagnameString));
                }
                else {
                    // It's a namespace prefix registration
                    // 'namespace' is required
                    if (ns == null) {
                        throw new HttpException(SR.GetString(SR.Missing_attr, _namespaceString));
                    }

                    TagNamespaceRegisterEntry nsRegisterEntry = new TagNamespaceRegisterEntry(tagPrefix, ns, assemblyName);
                    registerEntry = nsRegisterEntry;

                    TypeMapper.ProcessTagNamespaceRegistration(nsRegisterEntry);
                }

                registerEntry.Line = _lineNumber;
                registerEntry.VirtualPath = CurrentVirtualPathString;

                // If there are some attributes left, fail
                Util.CheckUnknownDirectiveAttributes(directiveName, directive);
            }
            else {
                base.ProcessDirective(directiveName, directive);
            }
        }
    }

    /*
     * Entry representing a register directive
     * e.g. <%@ Register tagprefix="tagprefix" Namespace="namespace" Assembly="assembly" %> OR
     * e.g. <%@ Register tagprefix="tagprefix" Tagname="tagname" Src="pathname" %>
     */
    internal abstract class RegisterDirectiveEntry: SourceLineInfo {

        internal RegisterDirectiveEntry(string tagPrefix) {
            _tagPrefix = tagPrefix;
        }

        private string _tagPrefix;
        internal string TagPrefix {
            get { return _tagPrefix;}
        }
    }

    /*
     * Entry representing the registration of a tag namespace
     * e.g. <%@ Register tagprefix="tagprefix" Namespace="namespace" Assembly="assembly" %>
     */
    internal class TagNamespaceRegisterEntry: RegisterDirectiveEntry {

        internal TagNamespaceRegisterEntry(string tagPrefix, string namespaceName, string assemblyName) : base(tagPrefix) {
            _ns = namespaceName;
            _assemblyName = assemblyName;
        }

        private string _ns;
        internal string Namespace {
            get { return _ns;}
        }

        private string _assemblyName;
        internal string AssemblyName {
            get { return _assemblyName;}
        }

#if DONT_COMPILE
        internal string Key {
            get {
                return TagPrefix + ":" + _ns + ":" + (_assemblyName == null ? String.Empty : _assemblyName);
            }
        }
#endif
    }

    /*
     * Entry representing the registration of a user control
     * e.g. <%@ Register tagprefix="tagprefix" Tagname="tagname" Src="pathname" %>
     */
    internal class UserControlRegisterEntry: RegisterDirectiveEntry {

        internal UserControlRegisterEntry(string tagPrefix, string tagName) : base(tagPrefix) {
            _tagName = tagName;
        }

        private string _tagName;
        internal string TagName {
            get { return _tagName;}
        }

        private VirtualPath _source;
        internal VirtualPath UserControlSource {
            get { return _source;}
            set { _source = value;}
        }

        private bool _comesFromConfig;
        internal bool ComesFromConfig {
            get { return _comesFromConfig;}
            set { _comesFromConfig = value;}
        }

        internal string Key {
            get {
                return TagPrefix + ":" + _tagName;
            }
        }
    }

    internal class TagNamespaceRegisterEntryTable : Hashtable {

        public TagNamespaceRegisterEntryTable() : base(StringComparer.OrdinalIgnoreCase) {
        }

        public override object Clone() {
            // We override clone to perform a deep copy of the hashtable contents but a shallow copy of
            // the contained arraylist itself

            TagNamespaceRegisterEntryTable newTable = new TagNamespaceRegisterEntryTable();
            foreach (DictionaryEntry entry in this) {
                newTable[entry.Key] = ((ArrayList)entry.Value).Clone();
            }

            return newTable;
        }
    }
}
