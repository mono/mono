//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    internal partial class VersionEditor : UserControl, IVersionEditor
    {
        private static DependencyProperty versionProperty = DependencyProperty.Register("Version", typeof(Version), typeof(VersionEditor), new PropertyMetadata(new PropertyChangedCallback(VersionEditor.OnVersionChanged)));
        private static DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(VersionEditorViewModel), typeof(VersionEditor));

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "This value must be set in contructor to ensure that ViewModel could be used")]
        public VersionEditor()
        {
            this.InitializeComponent();
            this.ViewModel = new VersionEditorViewModel(this);
            this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
        }

        public static DependencyProperty VersionProperty
        {
            get { return versionProperty; }
        }

        public Version Version
        {
            get { return (Version)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        private static DependencyProperty ViewModelProperty
        {
            get { return viewModelProperty; }
        }

        private VersionEditorViewModel ViewModel
        {
            get { return (VersionEditorViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        void IVersionEditor.ShowErrorMessage(string message)
        {
            ErrorReporting.ShowErrorMessage(message);
        }

        private static void OnVersionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((VersionEditor)sender).OnVersionChanged(e);
        }

        private void OnVersionChanged(DependencyPropertyChangedEventArgs e)
        {
            this.ViewModel.Version = (Version)e.NewValue;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Version")
            {
                this.Version = this.ViewModel.Version;
            }
        }
    }
}
