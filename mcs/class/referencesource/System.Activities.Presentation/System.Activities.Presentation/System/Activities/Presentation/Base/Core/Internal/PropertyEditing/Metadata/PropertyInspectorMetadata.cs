//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Metadata 
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Effects;

    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Editors;

    // <summary>
    // Metadata specific to PropertyEditing -- associates specific PropertyValueEditors with
    // specific property Types or properties themselves.
    // </summary>
    [SuppressMessage("Microsoft.MSInternal", "CA903:InternalNamespaceShouldNotContainPublicTypes", Justification = "Suppress to avoid churning the code base.")]
    static class PropertyInspectorMetadata 
    {

        private static bool _initialized;

        // <summary>
        // Initializes the metadata provided by this class.  Multiple class
        // are ignored.
        // </summary>
        public static void Initialize() 
        {
            if (_initialized)
            {
                return;
            }

            // Introduce any Cider-specific customizations
            AttributeTableBuilder builder = new AttributeTableBuilder();

            // Make Name and FlowDirection properties browsable.  The reason why
            // these attributes are here instead of in the BaseOverridesAttributeTable
            // is because the BaseAttributeTable explicitly hides these properties
            // and adding conflicting attributes to the same table (via BaseOverridesAttributeTable
            // which derives from BaseAttributeTable) currently results in unspeciefied
            // behavior.  Hence we use this table to deal with these attributes.
            //
            MakeBasic(builder, typeof(FrameworkElement), FrameworkElement.FlowDirectionProperty);
            MakeBasic(builder, typeof(Control), Control.NameProperty);

            // Note: Add any new attributes here or into System.Activities.Presentation.Developer / 
            // System.Activities.Presentation.Internal.Metadata.BaseOverridesAttributeTable

            MetadataStore.AddAttributeTable(builder.CreateTable());

            _initialized = true;
        }

        private static void MakeBasic(AttributeTableBuilder builder, Type owningType, DependencyProperty property) 
        {
            builder.AddCustomAttributes(owningType, property, BrowsableAttribute.Yes);
            builder.AddCustomAttributes(owningType, property, new EditorBrowsableAttribute(EditorBrowsableState.Always));
        }

    }
}
