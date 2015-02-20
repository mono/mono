//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Activities.Design
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Data;
    using System.Text;
    using System.Windows.Forms;

    internal partial class ServiceOperationViewControl : ListItemViewControl
    {
        private object item;

        public ServiceOperationViewControl()
        {
            InitializeComponent();
        }

        public override object Item
        {
            get { return item; }
            set
            {
                item = value;
                ServiceOperationListItem listItem = (ServiceOperationListItem) value;
                this.operationNameLabel.Text = listItem.Name;
                if (listItem.ImplementingActivities.Count > 0)
                {
                    this.isImplementedPictureBox.Visible = true;
                }
                else
                {
                    this.isImplementedPictureBox.Visible = false;
                }
            }
        }

        public override void UpdateView()
        {

            bool focused = (this.DrawItemState & DrawItemState.Focus) == DrawItemState.Focus;
            bool selected = (this.DrawItemState & DrawItemState.Selected) == DrawItemState.Selected;
            this.Height = this.operationNameLabel.Height;
            if (focused && selected)
            {
                this.backgroundPanel.BaseColor = System.Drawing.SystemColors.Window;
                this.backgroundPanel.LightingColor = Color.FromArgb(213, 246, 255);
                this.backgroundPanel.Glossy = true;
                this.backgroundPanel.Radius = 1;
                this.backgroundPanel.BorderColor = Color.FromArgb(155, 230, 255);
            }
            else if (selected)
            {
                this.backgroundPanel.BaseColor = System.Drawing.SystemColors.Window;
                this.backgroundPanel.LightingColor = Color.Gainsboro;
                this.backgroundPanel.Glossy = true;
                this.backgroundPanel.Radius = 1;
                this.backgroundPanel.BorderColor = Color.Gainsboro;
            }
            else
            {
                this.backgroundPanel.BaseColor = Color.Transparent;
                this.backgroundPanel.LightingColor = Color.Transparent;
                this.backgroundPanel.Glossy = false;
                this.backgroundPanel.BorderColor = Color.Transparent;
            }
            base.UpdateView();
        }

    }
}
