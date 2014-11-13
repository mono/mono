//------------------------------------------------------------------------------
// <copyright file="BindableTemplateBuilder.cs" company="Microsoft">
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
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class BindableTemplateBuilder : TemplateBuilder, IBindableTemplate {
        private ExtractTemplateValuesMethod _extractTemplateValuesMethod;
        
        /*
         * No-compile delegate handler for ExtractValues.
         */
        private IOrderedDictionary ExtractTemplateValuesMethod(Control container) {
            /*System.Web.UI.OrderedDictionary @__table;
            System.Web.UI.WebControls.DropDownList ddl2;
            @__table = new System.Web.UI.OrderedDictionary();
            ddl2 = ((System.Web.UI.WebControls.DropDownList)(@__container.FindControl("ddl2")));
            if ((ddl2 != null)) {
                @__table["FavVegetable"] = ddl2.SelectedValue;
            }
            return @__table;*/
            
            BindableTemplateBuilder bindableTemplateBuilder = this as BindableTemplateBuilder;

            Debug.Assert(bindableTemplateBuilder != null, "ExtractTemplateValuesMethod called on non-BindableTemplateBuilder.");
            OrderedDictionary table = new OrderedDictionary();
            if (bindableTemplateBuilder != null) {
                ExtractTemplateValuesRecursive(bindableTemplateBuilder.SubBuilders, table, container);
            }

            return table;
        }

        private void ExtractTemplateValuesRecursive(ArrayList subBuilders, OrderedDictionary table, Control container) {
            foreach (object subBuilderObject in subBuilders) {
                ControlBuilder subBuilderControlBuilder = subBuilderObject as ControlBuilder;
                if (subBuilderControlBuilder != null) {
                    ICollection entries;
                    // filter out device filtered bound entries that don't apply to this device
                    if (!subBuilderControlBuilder.HasFilteredBoundEntries) {
                        entries = subBuilderControlBuilder.BoundPropertyEntries;
                    }
                    else {
                        Debug.Assert(subBuilderControlBuilder.ServiceProvider == null);
                        Debug.Assert(subBuilderControlBuilder.TemplateControl != null, "TemplateControl should not be null in no-compile pages. We need it for the FilterResolutionService.");

                        ServiceContainer serviceContainer = new ServiceContainer();
                        serviceContainer.AddService(typeof(IFilterResolutionService), subBuilderControlBuilder.TemplateControl);

                        try {
                            subBuilderControlBuilder.SetServiceProvider(serviceContainer);
                            entries = subBuilderControlBuilder.GetFilteredPropertyEntrySet(subBuilderControlBuilder.BoundPropertyEntries);
                        }
                        finally {
                            subBuilderControlBuilder.SetServiceProvider(null);
                        }
                    }

                    string previousControlName = null;
                    bool newControl = true;
                    Control control = null;

                    foreach (BoundPropertyEntry entry in entries) {
                        // Skip all entries that are not two-way
                        if (!entry.TwoWayBound)
                            continue;

                        // Reset the "previous" Property Entry if we're not looking at the same control.
                        // If we don't do this, Two controls that have conditionals on the same named property will have
                        // their conditionals incorrectly merged.
                        if (String.Compare(previousControlName, entry.ControlID, StringComparison.Ordinal) != 0) {
                            newControl = true;
                        }
                        else {
                            newControl = false;
                        }

                        previousControlName = entry.ControlID;

                        if (newControl) {
                            control = container.FindControl(entry.ControlID);

                            if (control == null || !entry.ControlType.IsInstanceOfType(control)) {
                                Debug.Assert(false, "BoundPropertyEntry is of wrong control type or couldn't be found.  Expected " + entry.ControlType.Name);
                                continue;
                            }
                        }

                        string propertyName;
                        // map the property in case it's a complex property
                        object targetObject = PropertyMapper.LocatePropertyObject(control, entry.Name, out propertyName, InDesigner);

                        // FastPropertyAccessor uses ReflectEmit for lightning speed
                        table[entry.FieldName] = FastPropertyAccessor.GetProperty(targetObject, propertyName, InDesigner);
                    }

                    ExtractTemplateValuesRecursive(subBuilderControlBuilder.SubBuilders, table, container);
                }
            }
        }
        
        /*
         * IBindableTemplate implementation
         * This implementation of ITemplate is used in the designer and no-compile.
         */
        public IOrderedDictionary ExtractValues(Control container) {
            if (_extractTemplateValuesMethod != null && !InDesigner) {
                return _extractTemplateValuesMethod(container);
            }
            return new OrderedDictionary();
        }

        public override void OnAppendToParentBuilder(ControlBuilder parentBuilder) {
            base.OnAppendToParentBuilder(parentBuilder);
            if (HasTwoWayBoundProperties) {
                _extractTemplateValuesMethod = new ExtractTemplateValuesMethod(ExtractTemplateValuesMethod);
            }
        }
    }


    // Delegates used for the compiled template and no-compile Bind

    /// <internalonly/>
    /// <devdoc>
    /// <para>[To be supplied.]</para>
    /// </devdoc>
    public delegate IOrderedDictionary ExtractTemplateValuesMethod(Control control);

    /*
     * This class is the ITemplate implementation that is called from the
     * generated page class code.  It just passes the Initialize call on to a
     * delegate.
     */

    /// <internalonly/>
    /// <devdoc>
    /// <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class CompiledBindableTemplateBuilder : IBindableTemplate {
        private BuildTemplateMethod _buildTemplateMethod;
        private ExtractTemplateValuesMethod _extractTemplateValuesMethod;


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public CompiledBindableTemplateBuilder(BuildTemplateMethod buildTemplateMethod, ExtractTemplateValuesMethod extractTemplateValuesMethod) {
            _buildTemplateMethod = buildTemplateMethod;
            _extractTemplateValuesMethod = extractTemplateValuesMethod;
        }

        // IBindableTemplate::ExtractValues

        /// <devdoc>
        /// <para>Calls the ExtractTemplateValuesMethod delegate, which will return a dictionary of values.</para>
        /// </devdoc>
        public IOrderedDictionary ExtractValues(Control container) {
            if (_extractTemplateValuesMethod != null) {
                return _extractTemplateValuesMethod(container);
            }
            return new OrderedDictionary();
        }
        
        // ITemplate::InstantiateIn

        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public void InstantiateIn(Control container) {
            _buildTemplateMethod(container);
        }
    }
}
