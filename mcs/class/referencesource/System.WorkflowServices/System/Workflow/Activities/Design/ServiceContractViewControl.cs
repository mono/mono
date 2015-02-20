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
    using System.ServiceModel;

    internal partial class ServiceContractViewControl : ListItemViewControl
    {

        public ServiceContractViewControl()
        {
            InitializeComponent();
        }

        public override object Item
        {
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                base.Item = value;
                ServiceContractListItem listItem = value as ServiceContractListItem;
                if (!string.IsNullOrEmpty(listItem.Name))
                {
                    this.contractNameLabel.Text = listItem.Name;
                    if (listItem.IsCustomContract)
                    {
                        this.contractIconPictureBox.Image = ImageResources.Contract;
                    }
                    else
                    {
                        this.contractIconPictureBox.Image = ImageResources.ImportedContract;
                    }
                }
            }
        }

        public override void UpdateView()
        {
            bool focused = (this.DrawItemState & DrawItemState.Focus) == DrawItemState.Focus;
            bool selected = (this.DrawItemState & DrawItemState.Selected) == DrawItemState.Selected;

            this.Height = this.contractNameLabel.Height;
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
