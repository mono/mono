//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class DynamicActivityPropertyChooserViewModel : ViewModel
    {
        private string selectedPropertyName;
        private ModelItemCollection properties;
        private Predicate<DynamicActivityProperty> filter;

        private ReadOnlyCollection<DynamicActivityProperty> dropDownItems;

        public ModelItemCollection Properties
        {
            private get
            {
                return this.properties;
            }

            set
            {
                this.properties = value;
            }
        }

        public Predicate<DynamicActivityProperty> Filter
        {
            private get
            {
                return this.filter;
            }

            set
            {
                this.filter = value;
            }
        }

        public ReadOnlyCollection<DynamicActivityProperty> DropDownItems
        {
            get
            {
                if (this.dropDownItems == null)
                {
                    this.dropDownItems = new ReadOnlyCollection<DynamicActivityProperty>(new List<DynamicActivityProperty>());
                }

                return this.dropDownItems;
            }

            private set
            {
                if (this.dropDownItems != value)
                {
                    this.dropDownItems = value;
                    NotifyPropertyChanged("DropDownItems");
                }
            }
        }

        public bool IsUpdatingDropDownItems { get; private set; }

        public string SelectedPropertyName
        {
            get
            {
                return this.selectedPropertyName;
            }

            set
            {
                if (this.selectedPropertyName != value)
                {
                    this.selectedPropertyName = value;

                    this.UpdateDropDownItems();

                    this.NotifyPropertyChanged("SelectedPropertyName");
                }
            }
        }

        public void UpdateDropDownItems()
        {
            if (this.IsUpdatingDropDownItems)
            {
                return;
            }

            List<DynamicActivityProperty> list = new List<DynamicActivityProperty>();
            bool currentSelectionFound = false;

            if (this.Properties != null)
            {
                foreach (ModelItem modelItem in this.Properties)
                {
                    DynamicActivityProperty property = modelItem.GetCurrentValue() as DynamicActivityProperty;

                    if (property != null)
                    {
                        if (this.Filter == null || this.Filter(property))
                        {
                            DynamicActivityProperty clone = new DynamicActivityProperty();
                            clone.Name = property.Name;
                            clone.Type = property.Type;
                            list.Add(clone);
                            if (StringComparer.Ordinal.Equals(this.SelectedPropertyName, property.Name))
                            {
                                currentSelectionFound = true;
                            }
                        }
                    }
                }
            }

            string savedSelectedPropertyName = this.SelectedPropertyName;
            if (!currentSelectionFound)
            {
                if (!string.IsNullOrEmpty(this.SelectedPropertyName))
                {
                    DynamicActivityProperty unresolvedProperty = new DynamicActivityProperty();
                    unresolvedProperty.Name = this.SelectedPropertyName;
                    list.Add(unresolvedProperty);
                }
            }

            list.Sort(new DynamicaActivityPropertyComparer());

            this.IsUpdatingDropDownItems = true;
            this.DropDownItems = new ReadOnlyCollection<DynamicActivityProperty>(list);
            this.SelectedPropertyName = savedSelectedPropertyName;
            this.IsUpdatingDropDownItems = false;
        }

        private class DynamicaActivityPropertyComparer : IComparer<DynamicActivityProperty>
        {
            public int Compare(DynamicActivityProperty x, DynamicActivityProperty y)
            {
                return string.CompareOrdinal(x.Name, y.Name);
            }
        }
    }
}
