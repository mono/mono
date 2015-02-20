//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------


namespace System.Activities.Presentation.Toolbox
{
    using System;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;

    // This class is responsible for rendering cate----ezed tools collection
    // It also provides methods for notifing user about tool selection/creation events

    [TemplatePart(Name = "PART_SearchBox"), TemplatePart(Name = "PART_Tools")]
    [ContentProperty("Categories")]
    sealed public partial class ToolboxControl : Control
    {
        public static readonly DependencyProperty ToolboxFileProperty =
                DependencyProperty.Register("ToolboxFile",
                typeof(string),
                typeof(ToolboxControl),
                new PropertyMetadata(
                string.Empty,
                new PropertyChangedCallback(OnToolboxFileChanged)));

        static readonly DependencyPropertyKey SelectedToolPropertyKey =
                DependencyProperty.RegisterReadOnly("SelectedTool",
                typeof(ToolboxItem),
                typeof(ToolboxControl),
                new PropertyMetadata(
                null,
                new PropertyChangedCallback(OnToolSelected)));

        public static readonly DependencyProperty SelectedToolProperty = SelectedToolPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ToolItemStyleProperty =
                DependencyProperty.Register("ToolItemStyle",
                typeof(Style),
                typeof(ToolboxControl),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty CategoryItemStyleProperty =
                DependencyProperty.Register("CategoryItemStyle",
                typeof(Style),
                typeof(ToolboxControl),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty ToolTemplateProperty =
                DependencyProperty.Register("ToolTemplate",
                typeof(DataTemplate),
                typeof(ToolboxControl),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty CategoryTemplateProperty =
                DependencyProperty.Register("CategoryTemplate",
                typeof(DataTemplate),
                typeof(ToolboxControl),
                new UIPropertyMetadata(null));

        public static readonly RoutedEvent ToolCreatedEvent =
                EventManager.RegisterRoutedEvent("ToolCreated",
                RoutingStrategy.Bubble,
                typeof(ToolCreatedEventHandler),
                typeof(ToolboxControl));

        public static readonly RoutedEvent ToolSelectedEvent =
                EventManager.RegisterRoutedEvent("ToolSelected",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ToolboxControl));


        internal TextBox searchBox;
        TreeView toolsTreeView;
        ToolboxCategoryItems categories;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ToolboxControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolboxControl), new FrameworkPropertyMetadata(typeof(ToolboxControl)));
        }

        public ToolboxControl()
        {
            var callback = new NotifyCollectionChangedEventHandler(this.OnCategoryCollectionChanged);
            this.categories = new ToolboxCategoryItems(callback);
        }

        public event ToolCreatedEventHandler ToolCreated
        {
            add
            {
                AddHandler(ToolCreatedEvent, value);
            }
            remove
            {
                RemoveHandler(ToolCreatedEvent, value);
            }
        }

        public event RoutedEventHandler ToolSelected
        {
            add
            {
                AddHandler(ToolSelectedEvent, value);
            }
            remove
            {
                RemoveHandler(ToolSelectedEvent, value);
            }
        }

        [Fx.Tag.KnownXamlExternal]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [SuppressMessage(FxCop.Category.Usage, "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "The setter implemenation is required for XAML support. The setter doesn't replace the collection instance, but copies its content to internal collection")]
        public ToolboxCategoryItems Categories
        {
            get { return this.categories; }
            set 
            {
                this.categories.Clear();
                if (null != value)
                {
                    foreach (var category in value)
                    {
                        this.categories.Add(category);
                    }
                }
            }
        }

        public string ToolboxFile
        {
            get { return (string)GetValue(ToolboxFileProperty); }
            set { SetValue(ToolboxFileProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public ToolboxItem SelectedTool
        {
            get { return (ToolboxItem)GetValue(SelectedToolProperty); }
            private set { SetValue(SelectedToolPropertyKey, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public Style ToolItemStyle
        {
            get { return (Style)GetValue(ToolItemStyleProperty); }
            set { SetValue(ToolItemStyleProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public Style CategoryItemStyle
        {
            get { return (Style)GetValue(CategoryItemStyleProperty); }
            set { SetValue(CategoryItemStyleProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public DataTemplate ToolTemplate
        {
            get { return (DataTemplate)GetValue(ToolTemplateProperty); }
            set { SetValue(ToolTemplateProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public DataTemplate CategoryTemplate
        {
            get { return (DataTemplate)GetValue(CategoryTemplateProperty); }
            set { SetValue(CategoryTemplateProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public WorkflowDesigner AssociatedDesigner
        {
            get;
            set;
        }

        static void OnToolboxFileChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ToolboxControl toolboxControl = sender as ToolboxControl;
            string fileName = args.NewValue as string;
            if (null == toolboxControl || null == fileName)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException(null == toolboxControl ? "toolboxControl" : "fileName"));
            }

            try
            {
                ToolboxItemLoader loader = ToolboxItemLoader.GetInstance();
                loader.LoadToolboxItems(fileName, toolboxControl.categories, false);
            }
            catch
            {
                if (!DesignerProperties.GetIsInDesignMode(toolboxControl))
                {
                    throw;
                }
            }
        }

        static void OnToolSelected(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ToolboxControl toolboxControl = sender as ToolboxControl;
            if (null == toolboxControl)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("sender"));
            }
            if (null != toolboxControl.SelectedTool)
            {
                toolboxControl.RaiseEvent(new RoutedEventArgs(ToolSelectedEvent, toolboxControl));
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //template is applied, look for required controls within it
            this.searchBox = this.Template.FindName("PART_SearchBox", this) as TextBox;
            this.toolsTreeView = this.Template.FindName("PART_Tools", this) as TreeView;
            //if tools tree view exists - assign style and container selectors (there are different styles
            //for Cateogries and Tools
            if (null != this.toolsTreeView)
            {
                this.toolsTreeView.ItemsSource = this.Categories;
                this.toolsTreeView.ItemContainerStyleSelector = new TreeViewContainerStyleSelector(this);
                this.toolsTreeView.ItemTemplateSelector = new TreeViewTemplateSelector(this);
                this.toolsTreeView.SelectedItemChanged += (s, e) =>
                    {
                        var toolWrapper = e.NewValue as ToolboxItemWrapper;
                        this.SelectedTool = toolWrapper != null ? toolWrapper.ToolboxItem : null;
                    };
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    if (null != this.searchBox && 
                        e.OriginalSource == this.searchBox && 
                        null != this.toolsTreeView)
                    {
                        this.toolsTreeView.Focus();
                    }
                    break;

                case Key.Enter:
                    ToolboxItemCreated();
                    e.Handled = true;
                    break;

                default:
                    if (null != this.searchBox && e.Source != this.searchBox)
                    {
                        if (((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9)) &&
                            (e.KeyboardDevice.Modifiers == ModifierKeys.None || e.KeyboardDevice.Modifiers == ModifierKeys.Shift))
                        {
                            this.searchBox.Focus();
                        }
                    }
                    break;
            }
            base.OnPreviewKeyDown(e);
        }

        void OnCategoryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ToolboxCategory category in e.NewItems)
                    {
                        if (null == category)
                        {
                            throw FxTrace.Exception.ArgumentNull("category");
                        }

                        var listener = new NotifyCollectionChangedEventHandler(OnToolsCollectionChange);
                        category.HandleToolCollectionNotification(listener, true);

                        var items = new List<ToolboxItemWrapper>();
                        foreach (ToolboxItemWrapper toolWrapper in category.Tools)
                        {
                            items.Add(toolWrapper);
                        }
                        OnToolsCollectionChange(category,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, 0));
                    }
                    break;

                default:
                    break;
            }
            if (null != this.toolsTreeView)
            {
                this.toolsTreeView.ItemsSource = null;
                this.toolsTreeView.ItemsSource = this.categories;
            }
        }


        void OnToolsCollectionChange(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ToolboxItemWrapper tool in args.NewItems)
                    {
                        if (null == tool)
                        {
                            throw FxTrace.Exception.ArgumentNull("tool");
                        }
                        tool.PropertyChanged += new PropertyChangedEventHandler(OnToolPropertyChanged);
                        OnToolPropertyChanged(tool, null);
                    }
                    break;

                default:
                    break;
            }
        }

        void OnToolPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                ToolboxItemWrapper tool = (ToolboxItemWrapper)sender;
                tool.ResolveToolboxItem();
            }
            catch
            {
                if (!DesignerProperties.GetIsInDesignMode(this))
                {
                    throw;
                }
            }
        }

        internal void OnToolMouseMove(object sender, MouseEventArgs args)
        {
            ToolboxItem tool;
            ToolboxItemWrapper toolWrapper;
            if (args.LeftButton == MouseButtonState.Pressed && TryGetSelectedToolboxItem(out tool, out toolWrapper))
            {
                IDataObject dataObject = toolWrapper.DataObject ?? new DataObject();
                dataObject.SetData(DragDropHelper.WorkflowItemTypeNameFormat, toolWrapper.Type.AssemblyQualifiedName);
                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Link | DragDropEffects.Copy);
            }
        }

        internal void OnTreeViewDoubleClick(object sender, MouseEventArgs args)
        {
            ToolboxItemCreated();
        }

        void ToolboxItemCreated()
        {
            ToolboxItem tool;
            ToolboxItemWrapper toolWrapper;
            if (TryGetSelectedToolboxItem(out tool, out toolWrapper))
            {
                if (null != this.AssociatedDesigner && null != this.AssociatedDesigner.Context)
                {
                    DesignerView target = this.AssociatedDesigner.Context.Services.GetService<DesignerView>();
                    IDataObject dataObject = toolWrapper.DataObject ?? new DataObject();
                    dataObject.SetData(DragDropHelper.WorkflowItemTypeNameFormat, toolWrapper.Type.AssemblyQualifiedName);
                    ((RoutedCommand)DesignerView.CreateWorkflowElementCommand).Execute(dataObject, target);
                }
                ToolCreatedEventArgs args = new ToolCreatedEventArgs(ToolCreatedEvent, this, tool.CreateComponents());
                RaiseEvent(args);
            }
        }

        bool TryGetSelectedToolboxItem(out ToolboxItem toolboxItem, out ToolboxItemWrapper toolboxItemWrapper)
        {
            toolboxItem = null;
            toolboxItemWrapper = null;
            if (null != this.toolsTreeView && null != this.toolsTreeView.SelectedItem)
            {
                ToolboxItemWrapper tool = this.toolsTreeView.SelectedItem as ToolboxItemWrapper;
                if (null != tool && null != tool.ToolboxItem)
                {
                    toolboxItem = tool.ToolboxItem;
                    toolboxItemWrapper = tool;

                }
            }
            return (null != toolboxItem);
        }

    }
}
