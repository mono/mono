//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System.Activities.Presentation.Model;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    internal sealed partial class FloatingAnnotationView : UserControl, IFloatingAnnotation
    {
        public static readonly DependencyProperty ModelItemProperty = DependencyProperty.Register("ModelItem", typeof(ModelItem), typeof(FloatingAnnotationView));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(FloatingAnnotationView));

        private bool focusOnLoad;

        public FloatingAnnotationView()
        {
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(this.OnFloatingAnnotationViewLoaded);
        }

        public event Action DockButtonClicked;

        public event EventHandler IsMouseOverChanged
        {
            add
            {
                DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                descriptor.AddValueChanged(this, value);
            }

            remove
            {
                DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                descriptor.RemoveValueChanged(this, value);
            }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public ModelItem ModelItem
        {
            get { return (ModelItem)GetValue(ModelItemProperty); }
            set { SetValue(ModelItemProperty, value); }
        }

        public void FocusOnContent()
        {
            if (this.annotationTextBox.Focus() != true)
            {
                this.focusOnLoad = true;
            }
        }

        public void UpdateModelItem()
        {
            BindingExpression be = this.annotationTextBox.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void OnFloatingAnnotationViewLoaded(object sender, RoutedEventArgs e)
        {
            if (this.focusOnLoad)
            {
                Keyboard.Focus(this.annotationTextBox);
                this.focusOnLoad = false;
            }
        }

        private void OnDockButtonClicked(object sender, RoutedEventArgs e)
        {
            if (this.DockButtonClicked != null)
            {
                this.DockButtonClicked();
            }
        }
    }
}
