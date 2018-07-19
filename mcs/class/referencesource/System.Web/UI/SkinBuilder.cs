//------------------------------------------------------------------------------
// <copyright file="SkinBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Classes related to templated control support
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Web.Compilation;
    using System.Web.UI.WebControls;
    using System.Web.Util;
#if !FEATURE_PAL
    using System.Web.UI.Design;
#endif // !FEATURE_PAL


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class SkinBuilder : ControlBuilder {
        private ThemeProvider _provider;
        private Control _control;
        private ControlBuilder _skinBuilder;
        private string _themePath;
        internal static readonly Object[] EmptyParams = new Object[0];

        public SkinBuilder(ThemeProvider provider, Control control, ControlBuilder skinBuilder, string themePath) {
            _provider = provider;
            _control = control;
            _skinBuilder = skinBuilder;
            _themePath = themePath;
        }

        private void ApplyTemplateProperties(Control control) {
            object[] parameters = new object[1];
            ICollection entries = GetFilteredPropertyEntrySet(_skinBuilder.TemplatePropertyEntries);
            foreach (TemplatePropertyEntry entry in entries) {
                try {
                    object originalValue = FastPropertyAccessor.GetProperty(control, entry.Name, InDesigner);
                    if (originalValue == null) {
                        ControlBuilder controlBuilder = ((TemplatePropertyEntry)entry).Builder;
                        controlBuilder.SetServiceProvider(ServiceProvider);
                        try {
                            object objectValue = controlBuilder.BuildObject(true);
                            parameters[0] = objectValue;
                        }
                        finally {
                            controlBuilder.SetServiceProvider(null);
                        }

                        MethodInfo methodInfo = entry.PropertyInfo.GetSetMethod();
                        Util.InvokeMethod(methodInfo, control, parameters);
                    }
                }
                catch (Exception e) {
                    Debug.Fail(e.Message);
                }
#pragma warning disable 1058
                catch {
                }
#pragma warning restore 1058
            }
        }

        private void ApplyComplexProperties(Control control) {
            ICollection entries = GetFilteredPropertyEntrySet(_skinBuilder.ComplexPropertyEntries);
            foreach (ComplexPropertyEntry entry in entries) {
                ControlBuilder builder = entry.Builder;
                if (builder != null) {
                    string propertyName = entry.Name;
                    if (entry.ReadOnly) {
                        object objectValue = FastPropertyAccessor.GetProperty(control, propertyName, InDesigner);
                        if (objectValue == null) continue;

                        entry.Builder.SetServiceProvider(ServiceProvider);
                        try {
                            entry.Builder.InitObject(objectValue);
                        }
                        finally {
                            entry.Builder.SetServiceProvider(null);
                        }
                    }
                    else {
                        object childObj;
                        string actualPropName;
                        object value = entry.Builder.BuildObject(true);

                        // Make the UrlProperty based on theme path for control themes(Must be a string)
                        PropertyDescriptor desc = PropertyMapper.GetMappedPropertyDescriptor(control, PropertyMapper.MapNameToPropertyName(propertyName), out childObj, out actualPropName, InDesigner);
                        if (desc != null) {
                            string str = value as string;
                            if (value != null && desc.Attributes[typeof(UrlPropertyAttribute)] != null &&
                                UrlPath.IsRelativeUrl(str)) {
                                value = _themePath + str;
                            }
                        }

                        FastPropertyAccessor.SetProperty(childObj, propertyName, value, InDesigner);
                    }
                }
            }
        }

        private void ApplySimpleProperties(Control control) {
            ICollection entries = GetFilteredPropertyEntrySet(_skinBuilder.SimplePropertyEntries);
            foreach (SimplePropertyEntry entry in entries) {
                try {
                    if (entry.UseSetAttribute) {
                        SetSimpleProperty(entry, control);
                        continue;
                    }

                    string propertyName = PropertyMapper.MapNameToPropertyName(entry.Name);
                    object childObj;
                    string actualPropName;
                    PropertyDescriptor desc = PropertyMapper.GetMappedPropertyDescriptor(control, propertyName, out childObj, out actualPropName, InDesigner);

                    if (desc != null) {
                        DefaultValueAttribute defValAttr = (DefaultValueAttribute)desc.Attributes[typeof(DefaultValueAttribute)];
                        object currentValue = desc.GetValue(childObj);

                        // Only apply the themed value if different from default value.
                        if (defValAttr != null && !object.Equals(defValAttr.Value, currentValue)) {
                            continue;
                        }

                        object value = entry.Value;

                        // Make the UrlProperty based on theme path for control themes.
                        string str = value as string;
                        if (value != null && desc.Attributes[typeof(UrlPropertyAttribute)] != null &&
                            UrlPath.IsRelativeUrl(str)) {
                            value = _themePath + str;
                        }

                        SetSimpleProperty(entry, control);
                    }
                }
                catch (Exception e) {
                    Debug.Fail(e.Message);
                }
#pragma warning disable 1058
                catch {
                }
#pragma warning restore 1058
            }
        }

        private void ApplyBoundProperties(Control control) {
            DataBindingCollection dataBindings = null;
            IAttributeAccessor attributeAccessor = null;

            // If there are no filters in the picture, use the entries as is
            ICollection entries = GetFilteredPropertyEntrySet(_skinBuilder.BoundPropertyEntries);
            foreach (BoundPropertyEntry entry in entries) {
                InitBoundProperty(control, entry, ref dataBindings, ref attributeAccessor);
            }
        }

        private void InitBoundProperty(Control control, BoundPropertyEntry entry,
            ref DataBindingCollection dataBindings, ref IAttributeAccessor attributeAccessor) {

            string expressionPrefix = entry.ExpressionPrefix;
            // If we're in the designer, add the bound properties to the collections
            if (expressionPrefix.Length == 0) {
                if (dataBindings == null && control is IDataBindingsAccessor) {
                    dataBindings = ((IDataBindingsAccessor)control).DataBindings;
                }

                dataBindings.Add(new DataBinding(entry.Name, entry.Type, entry.Expression.Trim()));
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.ControlBuilder_ExpressionsNotAllowedInThemes));
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Control ApplyTheme() {
            if (_skinBuilder != null) {
                ApplySimpleProperties(_control);
                ApplyComplexProperties(_control);
                ApplyBoundProperties(_control);
                ApplyTemplateProperties(_control);
            }
            return _control;
        }
    }

    public sealed class ThemeProvider {
        private IDictionary _skinBuilders;
        private string[] _cssFiles;
        private string _themeName;
        private string _themePath;
        private int _contentHashCode;
        private IDesignerHost _host;

        public ThemeProvider(IDesignerHost host, string name, string themeDefinition, string[] cssFiles, string themePath) {
            _themeName = name;
            _themePath = themePath;
            _cssFiles = cssFiles;
            _host = host;

            ControlBuilder themeBuilder = DesignTimeTemplateParser.ParseTheme(host, themeDefinition, themePath);
            _contentHashCode = themeDefinition.GetHashCode();

            ArrayList subBuilders = themeBuilder.SubBuilders;
            _skinBuilders = new Hashtable();
            for (int i=0; i<subBuilders.Count; ++i) {
                ControlBuilder builder = subBuilders[i] as ControlBuilder;
                if (builder != null) {
                    IDictionary skins = _skinBuilders[builder.ControlType] as IDictionary;
                    if (skins == null) {
                        skins = new SortedList(StringComparer.OrdinalIgnoreCase);
                        _skinBuilders[builder.ControlType] = skins;
                    }
                    Control builtControl = builder.BuildObject() as Control;
                    if (builtControl != null) {
                        skins[builtControl.SkinID] = builder;
                    }
                }
            }
        }

        public int ContentHashCode {
            get {
                return _contentHashCode;
            }
        }

        public ICollection CssFiles {
            get {
                return _cssFiles;
            }
        }

        public IDesignerHost DesignerHost {
            get {
                return _host;
            }
        }

        public string ThemeName {
            get {
                return _themeName;
            }
        }

        public ICollection GetSkinsForControl(Type type) {
            IDictionary skins = _skinBuilders[type] as IDictionary;
            if (skins == null) {
                return new ArrayList();
            }
            return skins.Keys;
        }

        public SkinBuilder GetSkinBuilder(Control control) {
            IDictionary skins = _skinBuilders[control.GetType()] as IDictionary;
            if (skins == null) {
                return null;
            }

            ControlBuilder builder = skins[control.SkinID] as ControlBuilder;
            if (builder == null) {
                return null;
            }

            return new SkinBuilder(this, control, builder, _themePath);
        }

        public IDictionary GetSkinControlBuildersForControlType(Type type) {
            IDictionary skins = _skinBuilders[type] as IDictionary;

            return skins;
        }
    }
}
