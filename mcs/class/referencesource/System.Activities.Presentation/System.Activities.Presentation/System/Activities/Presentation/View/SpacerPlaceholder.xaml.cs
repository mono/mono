//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;    
    using System.Windows;
    using System.Windows.Controls;
    
    internal partial class SpacerPlaceholder : UserControl
    {
        public SpacerPlaceholder()
        {
            this.InitializeComponent();
        }

        public bool TargetVisiable
        {
            set 
            {
                if (value == true)
                {
                    this.dropTarget.Visibility = Visibility.Visible;
                }
                else
                {
                    this.dropTarget.Visibility = Visibility.Collapsed;
                }
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            this.TargetVisiable = false;
        }
    }
}
