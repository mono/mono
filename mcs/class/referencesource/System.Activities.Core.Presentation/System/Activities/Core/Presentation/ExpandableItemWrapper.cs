//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Model;
    using System.ComponentModel;

    internal class ExpandableItemWrapper : INotifyPropertyChanged
    {
        private bool isExpanded = false;
        private bool isPinned = false;

        public ExpandableItemWrapper()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }

            set
            {
                this.isExpanded = value;
                this.NotifyPropertyChanged("IsExpanded");
            }
        }

        public bool IsPinned
        {
            get
            {
                return this.isPinned;
            }

            set
            {
                this.isPinned = value;
                this.NotifyPropertyChanged("IsPinned");
            }
        }

        public ModelItem Item
        {
            get;
            set;
        }

        internal void SetPinState(bool isPinned)
        {
            this.isPinned = isPinned;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
