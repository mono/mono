// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Properties
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Activities.Presentation.PropertyEditing;

    internal class StandardCategoryLayout : ItemsControl
    {
        public StandardCategoryLayout()
        {
        }

        // This override is present to allow us to skip the automatically inserted ContentPresenter that ItemsControl
        // generates by default.  PrepareContainerForItemOverride sets up the DataContext and PropertyBinding as though
        // it were contained within a ContentPresenter and had a Template that was <PropertyContainer Property="{Binding}" />.
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new PropertyContainer();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            PropertyContainer propertyContainer = element as PropertyContainer;
            if (propertyContainer != null)
            {
                propertyContainer.DataContext = item;
                propertyContainer.SetBinding(PropertyContainer.PropertyEntryProperty, new Binding());
            }

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
