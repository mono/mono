//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    internal partial class ActivityDelegateInfoPresenter : UserControl
    {
        public static readonly DependencyProperty DelegateInfoProperty = DependencyProperty.Register("DelegateInfo", typeof(ActivityDelegateInfo), typeof(ActivityDelegateInfoPresenter), new PropertyMetadata(new PropertyChangedCallback(OnDelegateInfoChanged)));

        public ActivityDelegateInfoPresenter()
        {
            this.InitializeComponent();
        }

        public ActivityDelegateInfo DelegateInfo
        {
            get
            {
                return (ActivityDelegateInfo)GetValue(DelegateInfoProperty);
            }

            set
            {
                SetValue(DelegateInfoProperty, value);
            }
        }

        private static void OnDelegateInfoChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ActivityDelegateInfoPresenter presenter = sender as ActivityDelegateInfoPresenter;

            presenter.OnDelegateInfoChanged();
        }

        private void OnDelegateInfoChanged()
        {
            if (this.DelegateInfo != null)
            {
                Binding binding = new Binding(this.DelegateInfo.PropertyName);
                binding.Source = this.DelegateInfo.ModelItem;
                binding.Mode = BindingMode.TwoWay;

                this.activityDelegatePresenter.SetBinding(ActivityDelegatePresenter.ActivityDelegateProperty, binding);
            }
        }
    }
}
