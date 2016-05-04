//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.Annotations;
    using System.Windows;
    using System.Windows.Input;

    internal partial class AnnotationDialog : DialogWindow
    {
        public AnnotationDialog()
        {
            this.InitializeComponent();
            this.HelpKeyword = typeof(Annotation).FullName;
        }

        public string AnnotationText
        {
            get
            {
                return this.annotationTextBox.Text;
            }

            set
            {
                if (value == null)
                {
                    this.annotationTextBox.Text = string.Empty;
                }
                else
                {
                    this.annotationTextBox.Text = value;
                }
            }
        }

        protected override void OnKeyDown(Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        private void OnOkClick(object sender, RoutedEventArgs args)
        {
            this.DialogResult = true;
        }

        private void OnCancelClick(object sender, RoutedEventArgs args)
        {
            this.DialogResult = false;
        }
    }
}
