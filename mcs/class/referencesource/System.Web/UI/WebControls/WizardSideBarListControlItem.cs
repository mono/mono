//------------------------------------------------------------------------------
// <copyright file="WizardSideBarListControlItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    internal sealed class WizardSideBarListControlItem {
        private Control _container;

        public object DataItem {
            get;
            private set;
        }

        public ListItemType ItemType {
            get;
            private set;
        }

        public int ItemIndex {
            get;
            private set;
        }

        public WizardSideBarListControlItem(object dataItem, ListItemType itemType, int itemIndex, Control container) {
            DataItem = dataItem;
            ItemType = itemType;
            ItemIndex = itemIndex;
            _container = container;
        }

        internal Control FindControl(string id) {
            return _container.FindControl(id);
        }
    }
}
