//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Resources 
{
    using System;
    using System.Windows;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;

    // <summary>
    // Helper class that knows how to look up, load, and return PropertyInspector-specific
    // ResourceDictionary
    // </summary>

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class PropertyInspectorResources 
    {
        private static ResourceDictionary sharedResources;

        // <summary>
        // Wrapper around System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_DefaultCollectionStringValue,
        // which is internal and, hence, cannot be referenced from Xaml.  We use this property to
        // display "(Collection)" string in Xaml, rather than hard-coding it within Xaml, to make sure
        // all of the translations of "(Collection)" string is the same, regardless whether the string
        // comes from code or whether it comes from Xaml.
        // </summary>
        public static string DefaultCollectionStringValue 
        {
            get {
                return System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_DefaultCollectionStringValue;
            }
        }

        // <summary>
        // Getter for the header representing the NameProperty.  We don't want to hard-code that value in Xaml,
        // because then it would get localized and, since it's technically a property name, we don't
        // want it to get localized.
        // </summary>
        public static string NamePropertyHeader 
        {
            get {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_NamePropertyDisplayFormat,
                    FrameworkElement.NameProperty.Name);
            }
        }

        // <summary>
        // Wrapper around PropertyEditing_AlphabeticalCaption resource accessible from Xaml
        // </summary>
        public static string PropertyEditing_AlphabeticalCaption 
        {
            get {
                return System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_AlphabeticalCaption;
            }
        }

        // <summary>
        // Wrapper around PropertyEditing_AlphabeticalCaption resource accessible from Xaml
        // </summary>
        public static string PropertyEditing_CategorizedCaption 
        {
            get {
                return System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_CategorizedCaption;
            }
        }

        // <summary>
        // Wrapper around PropertyEditing_AlphabeticalCaption resource accessible from Xaml
        // </summary>
        public static string PropertyEditing_ClearButtonCaption 
        {
            get {
                return System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_ClearButtonCaption;
            }
        }

        // <summary>
        // Gets the PropertyInspector-specific ResourceDictionary
        // </summary>
        // <returns></returns>
        public static ResourceDictionary GetResources()
        {
            if (sharedResources == null)
            {
                Uri resourceLocator = new Uri(
                string.Concat(
                typeof(PropertyInspectorResources).Assembly.GetName().Name,
                @";component/System/Activities/Presentation/Base/Core/Internal/PropertyEditing/Resources/StylesCore.xaml"),
                UriKind.RelativeOrAbsolute);

                sharedResources = (ResourceDictionary)Application.LoadComponent(resourceLocator);
            }

            Fx.Assert(sharedResources != null, "Could not load PropertyInspector shared resources.");
            
            return sharedResources;
        }
    }
}
