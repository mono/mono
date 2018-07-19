//------------------------------------------------------------------------------
// <copyright file="DesignTimeTemplateParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Configuration;
    using System.IO;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Util;
    using System.Web.Configuration;


    /// <internalonly/>
    public static class DesignTimeTemplateParser {

        public static Control ParseControl(DesignTimeParseData data) {
            Control[] parsedControls = ParseControlsInternal(data, true);

            if (parsedControls.Length > 0) {
                return parsedControls[0];
            }

            return null;
        }


        public static Control[] ParseControls(DesignTimeParseData data) {
            return ParseControlsInternal(data, false);
        }

        /// <devdoc>
        /// Convenience method for parsing one or more controls
        /// </devdoc>
        // DesignTimeTemplateParser is only meant for use within the designer
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        internal static Control[] ParseControlsInternal(DesignTimeParseData data, bool returnFirst) {
            try {
                // Make the DesignerHost available only for the duration for this call
                if (data.DesignerHost != null) {
                    TargetFrameworkUtil.DesignerHost = data.DesignerHost;
                }
                return ParseControlsInternalHelper(data, returnFirst);
            } finally {
                // Remove reference to the DesignerHost so that it cannot be used outside
                // of this call.
                TargetFrameworkUtil.DesignerHost = null;
            }
        }

        private static Control[] ParseControlsInternalHelper(DesignTimeParseData data, bool returnFirst) {
            TemplateParser parser = new PageParser();
            parser.FInDesigner = true;
            parser.DesignerHost = data.DesignerHost;
            parser.DesignTimeDataBindHandler = data.DataBindingHandler;
            parser.Text = data.ParseText;
            parser.Parse();

            ArrayList parsedControls = new ArrayList();
            ArrayList subBuilders = parser.RootBuilder.SubBuilders;

            if (subBuilders != null) {
                // Look for the first control builder
                IEnumerator en = subBuilders.GetEnumerator();

                for (int i = 0; en.MoveNext(); i++) {
                    object cur = en.Current;

                    if ((cur is ControlBuilder) && !(cur is CodeBlockBuilder)) {
                        // Instantiate the control
                        ControlBuilder controlBuilder = (ControlBuilder)cur;

                        System.Diagnostics.Debug.Assert(controlBuilder.CurrentFilterResolutionService == null);

                        IServiceProvider builderServiceProvider = null;

                        // If there's a designer host, use it as the service provider
                        if (data.DesignerHost != null) {
                            builderServiceProvider = data.DesignerHost;
                        }
                        // If it doesn't exist, use a default ---- filter resolution service
                        else {
                            ServiceContainer serviceContainer = new ServiceContainer();
                            serviceContainer.AddService(typeof(IFilterResolutionService), new SimpleDesignTimeFilterResolutionService(data.Filter));
                            builderServiceProvider = serviceContainer;
                        }

                        controlBuilder.SetServiceProvider(builderServiceProvider);
                        try {
                            Control control = (Control)controlBuilder.BuildObject(data.ShouldApplyTheme);
                            parsedControls.Add(control);
                        }
                        finally {
                            controlBuilder.SetServiceProvider(null);
                        }
                        if (returnFirst) {
                            break;
                        }
                    }
                        // To preserve backwards compatibility, we don't add LiteralControls
                        // to the control collection when parsing for a single control
                    else if (!returnFirst && (cur is string)) {
                        LiteralControl literalControl = new LiteralControl(cur.ToString());
                        parsedControls.Add(literalControl);
                    }
                }
            }

            data.SetUserControlRegisterEntries(parser.UserControlRegisterEntries, parser.TagRegisterEntries);

            return (Control[])parsedControls.ToArray(typeof(Control));
        }


        // DesignTimeTemplateParser is only meant for use within the designer
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static ITemplate ParseTemplate(DesignTimeParseData data) {
            TemplateParser parser = new PageParser();

            parser.FInDesigner = true;
            parser.DesignerHost = data.DesignerHost;
            parser.DesignTimeDataBindHandler = data.DataBindingHandler;
            parser.Text = data.ParseText;
            parser.Parse();

            // Set the Text property of the TemplateBuilder to the input text
            parser.RootBuilder.Text = data.ParseText;
            parser.RootBuilder.SetDesignerHost(data.DesignerHost);
            return parser.RootBuilder;
        }

        public static ControlBuilder ParseTheme(IDesignerHost host, string theme, string themePath) {
            try {
                TemplateParser parser = new DesignTimePageThemeParser(themePath);
                parser.FInDesigner = true;
                parser.DesignerHost = host;
                parser.ThrowOnFirstParseError = true;
                
                parser.Text = theme;
                parser.Parse();

                return parser.RootBuilder;
            } catch (Exception e) {
                throw new Exception(SR.GetString(SR.DesignTimeTemplateParser_ErrorParsingTheme) + " " + e.Message);
            }
        }

        // Implementation of IDeviceFilterTester used at design time
        private class SimpleDesignTimeFilterResolutionService : IFilterResolutionService {
            private string _currentFilter;

            public SimpleDesignTimeFilterResolutionService(string filter) {
                _currentFilter = filter;
            }

            bool IFilterResolutionService.EvaluateFilter(string filterName) {
                if (String.IsNullOrEmpty(filterName)) {
                    return true;
                }

                if (StringUtil.EqualsIgnoreCase(((_currentFilter == null) ? String.Empty : _currentFilter), filterName)) {
                    return true;
                }

                return false;
            }

            int IFilterResolutionService.CompareFilters(string filter1, string filter2) {
                if (String.IsNullOrEmpty(filter1)) {
                    if (!String.IsNullOrEmpty(filter2)) {
                        return 1;
                    }

                    return 0;
                }

                if (String.IsNullOrEmpty(filter2)) {
                    return -1;
                }

                return 0;
            }
        }
    }
}

