//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Hosting;
    using System.Windows;
    using System.Windows.Media.Animation;

    partial class ParallelSeparator
    {
        public static readonly DependencyProperty AllowedItemTypeProperty =
            DependencyProperty.Register("AllowedItemType", typeof(Type), typeof(ParallelSeparator), new UIPropertyMetadata(typeof(object)));

        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register(
            "Context",
            typeof(EditingContext),
            typeof(ParallelSeparator));

        public ParallelSeparator()
        {
            this.InitializeComponent();
        }

        public Type AllowedItemType
        {
            get { return (Type)GetValue(AllowedItemTypeProperty); }
            set { SetValue(AllowedItemTypeProperty, value); }
        }

        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            CheckAnimate(e, "Expand");
            this.dropTarget.Visibility = Visibility.Visible;
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            CheckAnimate(e, "Collapse");
            this.dropTarget.Visibility = Visibility.Collapsed;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            this.dropTarget.Visibility = Visibility.Collapsed;
            base.OnDrop(e);
        }

        void CheckAnimate(DragEventArgs e, string storyboardResourceName)
        {
            if (!e.Handled)
            {
                if (!this.Context.Items.GetValue<ReadOnlyState>().IsReadOnly &&
                    DragDropHelper.AllowDrop(e.Data, this.Context, this.AllowedItemType))
                {
                    BeginStoryboard((Storyboard)this.Resources[storyboardResourceName]);
                    return;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
                e.Handled = true;
            }
        }
    }
}
