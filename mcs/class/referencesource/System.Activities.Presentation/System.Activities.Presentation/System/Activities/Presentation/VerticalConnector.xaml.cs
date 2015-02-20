//-----------------------------------------------------------------------
// <copyright file="VerticalConnector.xaml.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Statements;
    using System.Windows;
    using System.Windows.Media.Animation;

    internal partial class VerticalConnector
    {
        public VerticalConnector()
        {
            this.InitializeComponent();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            this.CheckAnimate(e, "Expand");
            this.dropTarget.Visibility = Visibility.Visible;
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            this.CheckAnimate(e, "Collapse");
            this.dropTarget.Visibility = Visibility.Collapsed;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            this.dropTarget.Visibility = Visibility.Collapsed;
            base.OnDrop(e);
        }

        private void CheckAnimate(DragEventArgs e, string storyboardResourceName)
        {
            if (!e.Handled)
            {
                BeginStoryboard((Storyboard)this.Resources[storyboardResourceName]);
                return;
            }
        }
    }
}
