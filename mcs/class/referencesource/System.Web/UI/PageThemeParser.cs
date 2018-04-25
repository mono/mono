//------------------------------------------------------------------------------
// <copyright file="PageThemeParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Web.Compilation;
    using System.Web.Util;
    using System.Web.UI;

    internal class PageThemeParser : BaseTemplateParser {

        internal const string defaultDirectiveName = "skin";
        private bool _mainDirectiveProcessed;
        private IList _skinFileList;
        private IList _cssFileList;
        private ControlBuilder _currentSkinBuilder;

        private VirtualPath _virtualDirPath;
        internal VirtualPath VirtualDirPath {
            get { return _virtualDirPath; }
        }

        internal PageThemeParser(VirtualPath virtualDirPath, IList skinFileList, IList cssFileList) {
            _virtualDirPath = virtualDirPath;
            _skinFileList = skinFileList;
            _cssFileList = cssFileList;
        }

        internal ICollection CssFileList {
            get { return _cssFileList; }
        }

        internal override Type DefaultBaseType {
            get {
                return typeof(PageTheme);
            }
        }

        internal override string DefaultDirectiveName {
            get {
                return defaultDirectiveName;
            }
        }

        /* code is not allowed in skin files */
        internal override bool IsCodeAllowed {
            get {
                return false;
            }
        }

        // The current processing controlbuilder for the ITemplate in the skin file.
        internal ControlBuilder CurrentSkinBuilder {
            get {
                return _currentSkinBuilder;
            }
            set {
                _currentSkinBuilder = value;
            }
        }

        internal override RootBuilder CreateDefaultFileLevelBuilder() {
            return new FileLevelPageThemeBuilder();
        }

        internal override void ParseInternal() {
            if (_skinFileList != null) {
                foreach(string virtualPath in _skinFileList) {
                    ParseFile(null /*physicalPath*/, virtualPath);
                }
            }

            AddSourceDependency(_virtualDirPath);
        }

        internal override void ProcessDirective(string directiveName, IDictionary directive) {
            if (directiveName == null ||
                directiveName.Length == 0 ||
                StringUtil.EqualsIgnoreCase(directiveName, DefaultDirectiveName)) {

                // Make sure the main directive was not already specified
                if (_mainDirectiveProcessed) {
                    ProcessError(SR.GetString(SR.Only_one_directive_allowed, DefaultDirectiveName));
                    return;
                }

                ProcessMainDirective(directive);
                _mainDirectiveProcessed = true;
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "register")) {
                base.ProcessDirective(directiveName, directive);
            }
            else {
                ProcessError(SR.GetString(SR.Unknown_directive, directiveName));
                return;
            }
        }

        internal override bool ProcessMainDirectiveAttribute(string deviceName, string name,
            string value, IDictionary parseData) {

            switch (name) {

            // Fail on the following unsupported attributes.  Note that our base class
            // TemplateParser does support them, hence the special casing
            case "classname":
            case "compilationmode":
            case "inherits":
                ProcessError(SR.GetString(SR.Attr_not_supported_in_directive,
                        name, DefaultDirectiveName));
                return false;

            default:
                // We didn't handle the attribute.  Try the base class
                return base.ProcessMainDirectiveAttribute(deviceName, name, value, parseData);
            }
        }
    }

    internal class DesignTimePageThemeParser : PageThemeParser {
        private string _themePhysicalPath;

        internal DesignTimePageThemeParser(string virtualDirPath) : base(null, null, null) {
            _themePhysicalPath = virtualDirPath;
        }

        internal string ThemePhysicalPath {
            get {
                return _themePhysicalPath;
            }
        }

        // Parse the designtime theme content here.
        internal override void ParseInternal() {
            if (Text != null) {
                ParseString(Text, CurrentVirtualPath, Encoding.UTF8);
            }
        }
    }
}
