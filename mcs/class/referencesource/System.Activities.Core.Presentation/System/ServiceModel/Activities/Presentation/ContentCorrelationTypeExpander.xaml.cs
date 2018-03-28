//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities.Presentation;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Xml;
    using System.Linq;
    using System.Collections;
    using System.Xml.Linq;

    partial class ContentCorrelationTypeExpander
    {
        static readonly DependencyPropertyKey IsSelectionValidPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsSelectionValid",
            typeof(bool),
            typeof(ContentCorrelationTypeExpander),
            new UIPropertyMetadata(false, OnIsSelectionValidChanged));

        public static readonly DependencyProperty IsSelectionValidProperty = IsSelectionValidPropertyKey.DependencyProperty;

        public static readonly RoutedEvent IsSelectionValidChangedEvent = EventManager.RegisterRoutedEvent(
            "IsSelectionValidChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ContentCorrelationTypeExpander));

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ContentCorrelationTypeExpander));

        public static readonly DependencyProperty TypesToExpandProperty = DependencyProperty.Register(
            "TypesToExpand",
            typeof(IList<ExpanderTypeEntry>),
            typeof(ContentCorrelationTypeExpander),
            new UIPropertyMetadata(null, OnTypesToExpandChanged));

        static readonly DependencyPropertyKey SelectedTypeEntryPropertyKey = DependencyProperty.RegisterReadOnly(
            "SelectedTypeEntry", 
            typeof(ExpanderTypeEntry), 
            typeof(ContentCorrelationTypeExpander), 
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty SelectedTypeEntryProperty = SelectedTypeEntryPropertyKey.DependencyProperty;

        static readonly Type[] PrimitiveTypesInXPath = new Type[]
            {
                typeof(DateTime),
                typeof(TimeSpan),
                typeof(XmlQualifiedName),                
                typeof(Uri),                
                typeof(Guid),
                typeof(XmlElement),
                typeof(string),
                typeof(object),
                typeof(Decimal),
                typeof(XElement),
            };

        MemberInfo[] path = null;
        Type selectedType = null;       

        public ContentCorrelationTypeExpander()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler IsSelectionValidChanged
        {
            add
            {
                AddHandler(IsSelectionValidChangedEvent, value);
            }
            remove
            {
                RemoveHandler(IsSelectionValidChangedEvent, value);
            }
        }

        public event RoutedEventHandler SelectionChanged
        {
            add
            {
                AddHandler(SelectionChangedEvent, value);
            }
            remove
            {
                RemoveHandler(SelectionChangedEvent, value);
            }
        }
        public bool IsSelectionValid
        {
            get { return (bool)GetValue(IsSelectionValidProperty); }
            private set { SetValue(IsSelectionValidPropertyKey, value); }
        }

        public IList<ExpanderTypeEntry> TypesToExpand
        {
            get { return (IList<ExpanderTypeEntry>)GetValue(TypesToExpandProperty); }
            set { SetValue(TypesToExpandProperty, value); }
        }

        public ExpanderTypeEntry SelectedTypeEntry
        {
            get { return (ExpanderTypeEntry)GetValue(SelectedTypeEntryProperty); }
            private set { SetValue(SelectedTypeEntryPropertyKey, value); }
        }

        public Type GetSelectedType()
        {
            return this.selectedType;
        }

        public MemberInfo[] GetMemberPath()
        {
            return this.path;
        }

        void RaiseSelectionValidChanged()
        {
            this.RaiseEvent(new RoutedEventArgs(IsSelectionValidChangedEvent, this));
        }

        void OnTypesToExpandChanged()
        {
            this.typeExpander.ItemsSource = this.TypesToExpand;
            this.emptyContent.Visibility = null == this.TypesToExpand || 0 == this.TypesToExpand.Count ? Visibility.Visible : Visibility.Collapsed;
        }

        void OnTypeExpanderLoaded(object sender, RoutedEventArgs e)
        {
            this.typeExpander.Focus();
        }

        void OnTreeViewItemLoaded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (null != item.Header)
            {
                if (item.Header is ExpanderTypeEntry)
                {
                    item.IsExpanded = true;
                }
                else if (item.Header is Type)
                {
                    var type = (Type)item.Header;
                    item.IsExpanded = true;
                }
            }
        }

        void OnTreeViewItemMouseAccept(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;
            if (null != item && item.Header is ExpanderTypeEntry)
            {
                this.SelectedTypeEntry = (ExpanderTypeEntry)item.Header;
            }
            if (null != item && item.IsSelected && item.IsSelectionActive)
            {
                this.Accept(item);
                e.Handled = true;
            }
        }

        void OnTreeViewItemKeyboardAccept(object sender, KeyEventArgs e)
        {
            var item = sender as TreeViewItem;
            if (null != item && item.Header is ExpanderTypeEntry)
            {
                this.SelectedTypeEntry = (ExpanderTypeEntry)item.Header;
            }
            if (null != item && item.IsSelected && item.IsSelectionActive && Key.Enter == e.Key && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Accept(item);
                e.Handled = true;
            }
        }

        void Accept(TreeViewItem item)
        {
            bool isType = item.Header is ExpanderTypeEntry;
            bool isMember  = item.Header is MemberInfo;
            if (isMember)
            {
                var members = new List<MemberInfo>(1);
                while (null != item && item.Header is MemberInfo)
                {
                    var member = (MemberInfo)item.Header;
                    members.Insert(0, member);

                    if (item.Tag is TreeViewItem)
                    {
                        item = (TreeViewItem)item.Tag;
                    }
                    else
                    {
                        item = null;
                    }
                }
                this.SelectedTypeEntry = (ExpanderTypeEntry)item.Header;
                this.selectedType = this.SelectedTypeEntry.TypeToExpand;
                this.path = members.ToArray();
            }
            else if (isType)
            {
                this.SelectedTypeEntry = (ExpanderTypeEntry)item.Header;
                this.selectedType = this.SelectedTypeEntry.TypeToExpand;
                this.path = new MemberInfo[0];
            }
            else
            {
                this.SelectedTypeEntry = null;
                this.selectedType = null;
                this.path = null;
            }
            this.IsSelectionValid = isType || isMember;
            this.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, this));
        }

        //The following types are considered as primitives as far as XPath generation is concerned and shouldn't be expanded any more
        // 1. CLR built-in types 
        // 2. Byte array, DateTime, TimeSpan, GUID, Uri, XmlQualifiedName, XmlElement and XmlNode array [This includes XElement and XNode array from .NET 3.5] 
        // 3. Enums 
        // 4. Arrays and Collection classes including List<T>, Dictionary<K,V> and Hashtable (Anything that implements IEnumerable or IDictionary or is an array is treated as a collection).
        // 5. Type has [CollectionDataContract] attribute
        internal static bool IsPrimitiveTypeInXPath(Type type)
        {
            return ((type.IsPrimitive) || type.IsEnum || PrimitiveTypesInXPath.Any((item => item == type))
                || (typeof(IEnumerable).IsAssignableFrom(type)) || typeof(IDictionary).IsAssignableFrom(type) || type.IsArray
                || (type.GetCustomAttributes(typeof(CollectionDataContractAttribute), false).Length > 0));
        }

        static void OnIsSelectionValidChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ContentCorrelationTypeExpander)sender).RaiseSelectionValidChanged();
        }

        static void OnTypesToExpandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (ContentCorrelationTypeExpander)sender;
            control.Dispatcher.BeginInvoke(new Action(() => { control.OnTypesToExpandChanged(); }), DispatcherPriority.Render);
        }
    }

    internal sealed class TypeEntryContainer
    {
        public string DisplayText { get; set; }
        public IList<ExpanderTypeEntry> Items { get; set; }

        public override string ToString()
        {
            return this.DisplayText ?? base.ToString();
        }
    }

    internal sealed class ExpanderTypeEntry : DependencyObject
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            "Name",
            typeof(string),
            typeof(ExpanderTypeEntry),
            new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty TypeToExpandProperty = DependencyProperty.Register(
            "TypeToExpand",
            typeof(Type),
            typeof(ExpanderTypeEntry),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty TagProperty = DependencyProperty.Register(
            "Tag",
            typeof(object),
            typeof(ExpanderTypeEntry),
            new UIPropertyMetadata(null));

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public Type TypeToExpand
        {
            get { return (Type)GetValue(TypeToExpandProperty); }
            set { SetValue(TypeToExpandProperty, value); }
        }

        public Type[] TypeToExpandSource
        {
            get { return new Type[] { this.TypeToExpand }; }
        }

        public object Tag
        {
            get { return GetValue(TagProperty); }
            set { SetValue(TagProperty, value); }
        }

        public override string ToString()
        {
            return this.Name ?? "<null>";
        }
    }
}
