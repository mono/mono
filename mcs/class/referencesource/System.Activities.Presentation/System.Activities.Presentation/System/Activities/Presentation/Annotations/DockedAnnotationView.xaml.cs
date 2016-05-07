//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System.Windows;
    using System.Windows.Controls;

    internal partial class DockedAnnotationView : UserControl, IDockedAnnotation
    {
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(DockedAnnotationView));

        private static DependencyProperty annotationTextProperty = DependencyProperty.Register("AnnotationText", typeof(string), typeof(DockedAnnotationView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public DockedAnnotationView()
        {
            this.InitializeComponent();
        }

        public event Action UndockButtonClicked;

        public static DependencyProperty AnnotationTextProperty
        {
            get { return annotationTextProperty; }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public string AnnotationText
        {
            get { return (string)GetValue(AnnotationTextProperty); }
            set { SetValue(AnnotationTextProperty, value); }
        }

        public void FocusOnContent()
        {
            this.textBox.Focus();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == "IsMouseOver" || e.Property.Name == "IsKeyboardFocusWithin")
            {
                if (this.IsMouseOver || this.IsKeyboardFocusWithin)
                {
                    this.undockButton.Visibility = Visibility.Visible;
                    this.border.Visibility = Visibility.Visible;
                }
                else
                {
                    this.undockButton.Visibility = Visibility.Hidden;
                    this.border.Visibility = Visibility.Hidden;
                }
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void OnUndockButtonClicked(object sender, RoutedEventArgs e)
        {
            if (this.UndockButtonClicked != null)
            {
                this.UndockButtonClicked();
            }
        }
    }
}
