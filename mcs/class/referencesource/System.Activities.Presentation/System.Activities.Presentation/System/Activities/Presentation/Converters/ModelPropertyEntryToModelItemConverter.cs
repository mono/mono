//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    public sealed class ModelPropertyEntryToModelItemConverter : IMultiValueConverter, IValueConverter
    {
        ModelPropertyEntryToOwnerActivityConverter propertyEntryConverter = new ModelPropertyEntryToOwnerActivityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PropertyEntry propertyEntry = value as PropertyEntry;
            if (null == propertyEntry)
            {
                PropertyValue propertyValue = value as PropertyValue;
                if (null != propertyValue)
                {
                    propertyEntry = propertyValue.ParentProperty;
                }
            }

            Container result = null;
            if (null != propertyEntry)
            {
                ModelItem item = null;
                ModelItem propertyParentItem = null;
                EditingContext context = null;
                GetPropertyData(propertyEntry, out item, out propertyParentItem, out context);
                result = new Container(item, context, item.View, propertyEntry.PropertyValue);
            }
            return result;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            CategoryEntry category = (CategoryEntry)
                values.FirstOrDefault<object>(p => p != null && typeof(CategoryEntry).IsAssignableFrom(p.GetType()));

            FrameworkElement categoryEditorVisual = (FrameworkElement)
                values.FirstOrDefault<object>(p => p != null && typeof(FrameworkElement).IsAssignableFrom(p.GetType()));
 
            Container result = null;

            if (null != category)
            {
                PropertyEntry property = GetPropertyEntry( category, parameter );

                if (null != property)
                {
                    ModelItem item = null;
                    ModelItem propertyParentItem = null;
                    EditingContext context = null;
                    GetPropertyData(property, out item, out propertyParentItem, out context);
                    result = new Container(item, context, item.View, property.PropertyValue);

                    category.PropertyChanged += (sender, e) =>
                    {
                        PropertyEntry selectedProperty = this.GetPropertyEntry((CategoryEntry)sender, parameter);
                        if (null != selectedProperty)
                        {
                            this.UpdateCategoryEditorDataContext(selectedProperty, categoryEditorVisual, result);
                        }
                    };
                }
            }
            return result;
        }

        PropertyEntry GetPropertyEntry(CategoryEntry category, object parameter)
        {
            PropertyEntry property = null;
            IEnumerable<PropertyEntry> properties = (category == null ? null : category.Properties);
            if (null != properties)
            {
                if (null == parameter)
                {
                    property = properties.ElementAtOrDefault<PropertyEntry>(0);
                }
                else
                {
                    property = properties.FirstOrDefault<PropertyEntry>(p => string.Equals(p.DisplayName, parameter));
                }
            }
            return property;
        }

        void GetPropertyData(PropertyEntry property, out ModelItem activityItem, out ModelItem propertyParentItem, out EditingContext context)
        {
            activityItem = (ModelItem)this.propertyEntryConverter.Convert(property, typeof(ModelItem), false, null);
            propertyParentItem = (ModelItem)this.propertyEntryConverter.Convert(property, typeof(ModelItem), true, null);
            context = activityItem.GetEditingContext();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", 
            Justification = "Catch all exceptions to prevent crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "Catch all exceptions to prevent crash.")]
        void UpdateCategoryEditorDataContext(PropertyEntry property, FrameworkElement editor, Container context)
        {
            try
            {
                editor.DataContext = null;
                ModelItem modelItem = null;
                ModelItem propertyParentItem = null;
                EditingContext editingContext = null;
                this.GetPropertyData(property, out modelItem, out propertyParentItem, out editingContext);
                context.Context = editingContext;
                context.WorkflowViewElement = (null == modelItem ? null : modelItem.View);
                context.ModelItem = modelItem;
                editor.DataContext = context;
            }
            catch (Exception err)
            {
                Fx.Assert(false, err.Message);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }



        internal sealed class Container : DependencyObject
        {
            public static readonly DependencyProperty ModelItemProperty =
                DependencyProperty.Register("ModelItem", typeof(ModelItem), typeof(Container), new UIPropertyMetadata(null));

            public static readonly DependencyProperty ContextProperty =
                DependencyProperty.Register("Context", typeof(EditingContext), typeof(Container), new UIPropertyMetadata(null));

            public static readonly DependencyProperty WorkflowViewElementProperty =
                DependencyProperty.Register("WorkflowViewElement", typeof(DependencyObject), typeof(Container), new UIPropertyMetadata(null));

            public static readonly DependencyProperty PropertyValueProperty =
                DependencyProperty.Register("PropertyValue", typeof(PropertyValue), typeof(Container), new UIPropertyMetadata(null));

            public Container(ModelItem item, EditingContext context, DependencyObject viewElement, PropertyValue value)
            {
                this.ModelItem = item;
                this.Context = context;
                this.WorkflowViewElement = viewElement;
                this.PropertyValue = value;
            }

            public ModelItem ModelItem
            {
                get { return (ModelItem)GetValue(ModelItemProperty); }
                set { SetValue(ModelItemProperty, value); }
            }

            public EditingContext Context
            {
                get { return (EditingContext)GetValue(ContextProperty); }
                set { SetValue(ContextProperty, value); }
            }

            public DependencyObject WorkflowViewElement
            {
                get { return (DependencyObject)GetValue(WorkflowViewElementProperty); }
                set { SetValue(WorkflowViewElementProperty, value); }
            }

            public PropertyValue PropertyValue
            {
                get { return (PropertyValue)GetValue(PropertyValueProperty); }
                set { SetValue(PropertyValueProperty, value); }
            }

        }
    }
}
