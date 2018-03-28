//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing;

    internal class EditableLabelControl : Panel
    {
        public TextBox TextBox;
        Label label;

        public EditableLabelControl()
        {
            label = new Label();
            TextBox = new TextBox();
            this.Controls.Add(label);
            label.BackColor = Color.Transparent;
            label.AutoEllipsis = true;
            label.Dock = DockStyle.Fill;
            this.BackColor = Color.Transparent;
            this.label.Click += new EventHandler(label_Click);
        }

        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                label.Font = value;
            }
        }

        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                label.ForeColor = value;
            }
        }

        public override string Text
        {
            get
            {
                return TextBox.Text;
            }
            set
            {
                label.Text = value;
                TextBox.Text = value;

            }
        }

        private void DisableEditMode()
        {
            if (this.Controls.Contains(TextBox))
            {
                this.Controls.Remove(TextBox);
            }
            if (!this.Controls.Contains(label))
            {
                this.Controls.Add(label);
            }
        }

        private void EnableEditMode()
        {
            TextBox.Text = label.Text;
            TextBox.ForeColor = label.ForeColor;
            TextBox.Font = this.Font;
            TextBox.Dock = DockStyle.Fill;
            TextBox.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Remove(label);
            this.Controls.Add(TextBox);
            this.TextBox.LostFocus += new EventHandler(textBox_LostFocus);
        }

        void label_Click(object sender, EventArgs e)
        {
            EnableEditMode();
        }

        void textBox_LostFocus(object sender, EventArgs e)
        {
            DisableEditMode();
            this.Text = TextBox.Text;
        }


    }
}
