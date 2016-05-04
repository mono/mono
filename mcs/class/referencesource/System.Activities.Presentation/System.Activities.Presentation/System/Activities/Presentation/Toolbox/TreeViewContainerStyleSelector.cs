//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Toolbox
{
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Runtime;
    using System.Globalization;

    // This class is used to provide tree view item container template for Category
    // and Tool objects

    sealed class TreeViewContainerStyleSelector : StyleSelector
    {
        ToolboxControl owner;
        Style styleForToolboxItem;
        Style styleForCategoryItem;

        public TreeViewContainerStyleSelector(ToolboxControl owner)
        {
            this.owner = owner;
        }

        //default style for ToolboxItemWrapper - this one is required to enable context search in tree view control
        Style GetStyleForToolboxItem(Style baseStyle)
        {
            if (null == this.styleForToolboxItem)
            {
                this.styleForToolboxItem = 
                    (null == baseStyle ? new Style(typeof(TreeViewItem)) : new Style(typeof(TreeViewItem), baseStyle));

                //visibility information - i need to bind TreeViewItem to it
                if (null != this.owner.searchBox)
                {
                    MultiBinding visibilityBinding = new MultiBinding() { Converter = new ToolItemVisibilityConverter() };
                    Binding searchTextBinding = new Binding();
                    searchTextBinding.Source = this.owner.searchBox;
                    searchTextBinding.Path = new PropertyPath(TextBox.TextProperty);
                    searchTextBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    searchTextBinding.Mode = BindingMode.OneWay;

                    Binding toolItemBinding = new Binding();
                    toolItemBinding.RelativeSource = RelativeSource.Self;
                    toolItemBinding.Path = new PropertyPath(TreeViewItem.HeaderProperty);
                    toolItemBinding.Mode = BindingMode.OneWay;

                    visibilityBinding.Bindings.Add(searchTextBinding);
                    visibilityBinding.Bindings.Add(toolItemBinding);

                    Setter visibilitySetter = new Setter();
                    visibilitySetter.Property = TreeViewItem.VisibilityProperty;
                    visibilitySetter.Value = visibilityBinding;

                    //adding visibility setter to the style
                    this.styleForToolboxItem.Setters.Add(visibilitySetter);
                }

                //take care of the AutomationId
                Binding automationIdBinding = new Binding();
                automationIdBinding.RelativeSource = RelativeSource.Self;
                automationIdBinding.Path = new PropertyPath("Header.ToolName");
                automationIdBinding.Mode = BindingMode.OneWay;

                Setter automationIdSetter = new Setter();
                automationIdSetter.Property = AutomationProperties.AutomationIdProperty;
                automationIdSetter.Value = automationIdBinding;
                this.styleForToolboxItem.Setters.Add(automationIdSetter);

                //to enable smooth keyboard operation, hidden items have to be disabled in order not to receive
                //keyboard events - Trigger does the job
                Setter enableSetter = new Setter(TreeViewItem.IsEnabledProperty, false);

                Trigger enableTrigger = new Trigger();
                enableTrigger.Property = TreeViewItem.VisibilityProperty;
                enableTrigger.Value = Visibility.Collapsed;
                enableTrigger.Setters.Add(enableSetter);

                //to enable drag&drop support add event setter for 
                EventSetter mouseMoveSetter = new EventSetter();
                mouseMoveSetter.Event = TreeViewItem.MouseMoveEvent;
                mouseMoveSetter.Handler = new MouseEventHandler(this.owner.OnToolMouseMove);
                this.styleForToolboxItem.Setters.Add(mouseMoveSetter);

                EventSetter mouseDoubleClickSetter = new EventSetter();
                mouseDoubleClickSetter.Event = TreeViewItem.MouseDoubleClickEvent;
                mouseDoubleClickSetter.Handler = new MouseButtonEventHandler(this.owner.OnTreeViewDoubleClick);
                this.styleForToolboxItem.Setters.Add(mouseDoubleClickSetter);

                //adding trigger to the style
                this.styleForToolboxItem.Triggers.Add(enableTrigger);
            }
            return this.styleForToolboxItem;
        }

        //default style for CategoryItem 
        Style GetStyleForCategoryItem(Style baseStyle)
        {
            if (null == this.styleForCategoryItem)
            {
                this.styleForCategoryItem =
                    (null == baseStyle ? new Style(typeof(TreeViewItem)) : new Style(typeof(TreeViewItem), baseStyle));

                //take care of the AutomationId
                Binding automationIdBinding = new Binding();
                automationIdBinding.RelativeSource = RelativeSource.Self;
                automationIdBinding.Path = new PropertyPath("Header.CategoryName");
                automationIdBinding.Mode = BindingMode.OneWay;

                Setter automationIdSetter = new Setter();
                automationIdSetter.Property = AutomationProperties.AutomationIdProperty;
                automationIdSetter.Value = automationIdBinding;
                this.styleForCategoryItem.Setters.Add(automationIdSetter);
            }
            return this.styleForCategoryItem;
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            //try get default style
            Style result = base.SelectStyle(item, container);
            if (item is ToolboxItemWrapper)
            {
                result = GetStyleForToolboxItem(this.owner.ToolItemStyle);
            }
            if (item is ToolboxCategory && null != this.owner.CategoryItemStyle)
            {
                result = GetStyleForCategoryItem( this.owner.CategoryItemStyle );
            }
            return result;
        }

        //helper converter - used to show/hide tool items based on the search criteria
        sealed class ToolItemVisibilityConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                //get the search text string
                var text = values[0] as string;
                //get current tool
                var tool = values[1] as ToolboxItemWrapper;
                var result = Visibility.Collapsed;
                //if tool is set and is valid
                if (null != tool && tool.IsValid)
                {
                    //if search text is empty - show item
                    if (string.IsNullOrEmpty(text))
                    {
                        result = Visibility.Visible;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(tool.DisplayName) || tool.DisplayName.IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            result = Visibility.Collapsed;
                        }
                        else
                        {
                            result = Visibility.Visible;
                        }
                        
                    }
                }
                return result;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }
        }
    }
}
