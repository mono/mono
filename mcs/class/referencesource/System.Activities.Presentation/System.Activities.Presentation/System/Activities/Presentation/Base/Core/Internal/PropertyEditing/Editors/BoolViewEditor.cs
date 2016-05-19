//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System.Windows;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.Resources;

    // <summary>
    // Simple PropertyValueEditor that uses the BoolViewTemplate (see StylesCore.Editors.xaml)
    // </summary>
    internal class BoolViewEditor : PropertyValueEditor 
    {
        public BoolViewEditor()
            :
            base(PropertyInspectorResources.GetResources()["BoolViewTemplate"] as DataTemplate) { }
    }
}
