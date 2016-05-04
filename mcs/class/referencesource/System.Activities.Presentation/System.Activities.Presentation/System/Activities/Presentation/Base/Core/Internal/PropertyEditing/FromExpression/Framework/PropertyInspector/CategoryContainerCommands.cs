

// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Properties
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector
{
    using System;
    using System.Windows;
    using System.Windows.Input;


    // <summary>
    // Standard commands and command implementations used by PropertyContainer templates
    // and implemented by property editing hosts
    // </summary>
    internal static class CategoryContainerCommands
    {
        private static readonly RoutedCommand togglePinAdvancedProperties = new RoutedCommand("TogglePinAdvancedProperties", typeof(CategoryContainerCommands));
        private static readonly RoutedCommand updateCategoryExpansionState = new RoutedCommand("UpdateCategoryExpansionState", typeof(CategoryContainerCommands));

        // <summary>
        // standard command to category edit host to togglePinAdvancedProperties
        // </summary>
        public static RoutedCommand TogglePinAdvancedProperties
        {
            get { return CategoryContainerCommands.togglePinAdvancedProperties; }
        }

        // <summary>
        // standard command to property edit host to updateCategoryExpansionState
        // </summary>
        public static RoutedCommand UpdateCategoryExpansionState
        {
            get { return CategoryContainerCommands.updateCategoryExpansionState; }
        }

    }
}
