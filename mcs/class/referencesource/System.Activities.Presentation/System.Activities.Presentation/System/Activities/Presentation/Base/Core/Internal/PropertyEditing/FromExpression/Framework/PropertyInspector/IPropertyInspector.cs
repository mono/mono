// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Properties
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Activities.Presentation.PropertyEditing;
    using System.Diagnostics;
    using System.Activities.Presentation;

    internal interface IPropertyInspector
    {
        bool IsCategoryExpanded(string categoryName);

        // <summary>
        // Calls Update on the current transaction, if one exists within the context
        // of this PropertyInspector
        // </summary>
        void UpdateTransaction();
    }

    internal class PropertyInspectorHelper
    {
        // OwningPropertyInspector Attached, Inherited DP
        public static readonly DependencyProperty OwningPropertyInspectorModelProperty = DependencyProperty.RegisterAttached("OwningPropertyInspectorModel", typeof(IPropertyInspector), typeof(PropertyInspectorHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty OwningPropertyInspectorElementProperty = DependencyProperty.RegisterAttached("OwningPropertyInspectorElement", typeof(UIElement), typeof(PropertyInspectorHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        // <summary>
        // Writes the attached property OwningPropertyInspector to the given element.
        // </summary>
        // <param name="d">The element to which to write the attached property.</param>
        // <param name="value">The property value to set</param>
        public static void SetOwningPropertyInspectorModel(DependencyObject dependencyObject, IPropertyInspector value)
        {
            if (dependencyObject == null)
            {
                throw FxTrace.Exception.ArgumentNull("dependencyObject");
            }
            dependencyObject.SetValue(PropertyInspectorHelper.OwningPropertyInspectorModelProperty, value);
        }

        // <summary>
        // Reads the attached property OwningPropertyInspector from the given element.
        // </summary>
        // <param name="d">The element from which to read the attached property.</param>
        // <returns>The property's value.</returns>
        public static IPropertyInspector GetOwningPropertyInspectorModel(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw FxTrace.Exception.ArgumentNull("dependencyObject");
            }
            return (IPropertyInspector)dependencyObject.GetValue(PropertyInspectorHelper.OwningPropertyInspectorModelProperty);
        }

        // OwningPropertyInspector Attached, Inherited DP

        // <summary>
        // Writes the attached property OwningPropertyInspector to the given element.
        // </summary>
        // <param name="d">The element to which to write the attached property.</param>
        // <param name="value">The property value to set</param>
        public static void SetOwningPropertyInspectorElement(DependencyObject dependencyObject, UIElement value)
        {
            if (dependencyObject == null)
            {
                throw FxTrace.Exception.ArgumentNull("dependencyObject");
            }
            dependencyObject.SetValue(PropertyInspectorHelper.OwningPropertyInspectorElementProperty, value);
        }

        // <summary>
        // Reads the attached property OwningPropertyInspector from the given element.
        // </summary>
        // <param name="d">The element from which to read the attached property.</param>
        // <returns>The property's value.</returns>
        public static UIElement GetOwningPropertyInspectorElement(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw FxTrace.Exception.ArgumentNull("dependencyObject");
            }
            return (UIElement)dependencyObject.GetValue(PropertyInspectorHelper.OwningPropertyInspectorElementProperty);
        }
    }
}

