//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Windows;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.Resources;

    // <summary>
    // SubPropertyViewEditor is a "fake" value editor.  We only use it as a marker that exposes
    // MarkerSubPropertyTemplate as its editor DataTemplates.  The XAML code for PropertyContainer
    // specifically looks for these markers and it switches its InlineRowTemplate to a
    // sub-property-specific inline row template when found.
    // </summary>
    internal class SubPropertyViewEditor : ExtendedPropertyValueEditor 
    {

        private static SubPropertyViewEditor _instance;

        // This class can have a private ctor because we instantiate it through code,
        // not through XAML or attributes
        private SubPropertyViewEditor()
            : base(
            PropertyInspectorResources.GetResources()["MarkerSubPropertyTemplate"] as DataTemplate,
            PropertyInspectorResources.GetResources()["MarkerSubPropertyTemplate"] as DataTemplate)
        { 
        }

        // <summary>
        // Gets the static instance of this class
        // </summary>
        public static SubPropertyViewEditor Instance 
        {
            get {
                if (_instance == null)
                {
                    _instance = new SubPropertyViewEditor();
                }

                return _instance;
            }
        }
    }
}
