//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.ComponentModel;
    using System.Windows.Controls;

    internal partial class DynamicActivityPropertyChooser : UserControl
    {
        private DynamicActivityPropertyChooserViewModel model;

        public DynamicActivityPropertyChooser()
        {
            this.Model = new DynamicActivityPropertyChooserViewModel();

            this.InitializeComponent();

            this.Model.PropertyChanged += new PropertyChangedEventHandler(this.OnModelPropertyChanged);
            this.comboBox.DropDownOpened += new EventHandler(this.OnComboBoxDropDownOpened);
        }

        public event SelectedPropertyNameChangedEventHandler SelectedPropertyNameChanged;

        public DynamicActivityPropertyChooserViewModel Model
        {
            get
            {
                return this.model;
            }

            set
            {
                this.model = value;
            }
        }

        public ModelItemCollection Properties
        {
            set
            {
                this.Model.Properties = value;
            }
        }

        public Predicate<DynamicActivityProperty> Filter
        {
            set
            {
                this.Model.Filter = value;
            }
        }

        public string SelectedPropertyName
        {
            get
            {
                return this.Model.SelectedPropertyName;
            }

            set
            {
                this.Model.SelectedPropertyName = value;
            }
        }

        public bool IsUpdatingDropDownItems
        {
            get
            {
                return this.Model.IsUpdatingDropDownItems;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            this.DataContext = this.Model;
            base.OnInitialized(e);
        }

        private void OnComboBoxDropDownOpened(object sender, EventArgs e)
        {
            this.Model.UpdateDropDownItems();
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedPropertyName")
            {
                if (this.SelectedPropertyNameChanged != null)
                {
                    this.SelectedPropertyNameChanged(this, new SelectedPropertyNameChangedEventArgs(this.Model.SelectedPropertyName));
                }
            }
        }
    }
}
