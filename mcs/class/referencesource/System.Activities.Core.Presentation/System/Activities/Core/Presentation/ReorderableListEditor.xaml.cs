//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    internal partial class ReorderableListEditor : UserControl
    {
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ReorderableListEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty ListProperty = DependencyProperty.Register("List", typeof(ObservableCollection<ExpandableItemWrapper>), typeof(ReorderableListEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedListItemProperty = DependencyProperty.Register("SelectedListItem", typeof(ExpandableItemWrapper), typeof(ReorderableListEditor), new PropertyMetadata(null));

        public ReorderableListEditor()
        {
            this.InitializeComponent();
        }
        
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
            set { this.SetValue(ItemTemplateProperty, value); }
        }

        public ObservableCollection<ExpandableItemWrapper> List
        {
            get { return (ObservableCollection<ExpandableItemWrapper>)this.GetValue(ListProperty); }
            set { this.SetValue(ListProperty, value); }
        }

        public ExpandableItemWrapper SelectedListItem
        {
            get { return (ExpandableItemWrapper)this.GetValue(SelectedListItemProperty); }
            set { this.SetValue(SelectedListItemProperty, value); }
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.listBox.IsKeyboardFocusWithin)
            {
                this.listBox.UnselectAll();
            }

            base.OnIsKeyboardFocusWithinChanged(e);
        }

        private void OnListBoxPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBoxItem item = VisualTreeUtils.FindVisualAncestor<ListBoxItem>(e.OriginalSource as DependencyObject);

            if (item != null)
            {
                this.SelectedListItem = item.Content as ExpandableItemWrapper;
            }
        }

        private void OnUpArrowClicked(object sender, RoutedEventArgs e)
        {
            int oldIndex = this.List.IndexOf(this.SelectedListItem);
            if (oldIndex > 0)
            {
                this.List.Move(oldIndex, oldIndex - 1);
            }
        }

        private void OnDownArrowClicked(object sender, RoutedEventArgs e)
        {
            int oldIndex = this.List.IndexOf(this.SelectedListItem);
            if (oldIndex < this.List.Count - 1)
            {
                this.List.Move(oldIndex, oldIndex + 1);
            }
        }
    }
}
