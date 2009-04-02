/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;

    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
        Justification = "Common shorthand for 'multiple'.")]
    public class MultiSelectList : IEnumerable<SelectListItem> {

        public MultiSelectList(IEnumerable items)
            : this(items, null /* selectedValues */) {
        }

        public MultiSelectList(IEnumerable items, IEnumerable selectedValues)
            : this(items, null /* dataValuefield */, null /* dataTextField */, selectedValues) {
        }

        public MultiSelectList(IEnumerable items, string dataValueField, string dataTextField)
            : this(items, dataValueField, dataTextField, null /* selectedValues */) {
        }

        public MultiSelectList(IEnumerable items, string dataValueField, string dataTextField, IEnumerable selectedValues) {
            if (items == null) {
                throw new ArgumentNullException("items");
            }

            Items = items;
            DataValueField = dataValueField;
            DataTextField = dataTextField;
            SelectedValues = selectedValues;
        }

        public string DataTextField {
            get;
            private set;
        }

        public string DataValueField {
            get;
            private set;
        }

        public IEnumerable Items {
            get;
            private set;
        }

        public IEnumerable SelectedValues {
            get;
            private set;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual IEnumerator<SelectListItem> GetEnumerator() {
            return GetListItems().GetEnumerator();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Operation performs conversions and returns a unique instance on each call.")]
        internal IList<SelectListItem> GetListItems() {
            return (!String.IsNullOrEmpty(DataValueField)) ?
                GetListItemsWithValueField() :
                GetListItemsWithoutValueField();
        }

        private IList<SelectListItem> GetListItemsWithValueField() {
            HashSet<string> selectedValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (SelectedValues != null) {
                selectedValues.UnionWith(from object value in SelectedValues select Convert.ToString(value, CultureInfo.CurrentCulture));
            }

            var listItems = from object item in Items
                            let value = Eval(item, DataValueField)
                            select new SelectListItem {
                                Value = value,
                                Text = Eval(item, DataTextField),
                                Selected = selectedValues.Contains(value)
                            };
            return listItems.ToList();
        }

        private IList<SelectListItem> GetListItemsWithoutValueField() {
            HashSet<object> selectedValues = new HashSet<object>();
            if (SelectedValues != null) {
                selectedValues.UnionWith(SelectedValues.Cast<object>());
            }

            var listItems = from object item in Items
                            select new SelectListItem {
                                Text = Eval(item, DataTextField),
                                Selected = selectedValues.Contains(item)
                            };
            return listItems.ToList();
        }

        private static string Eval(object container, string expression) {
            object value = container;
            if (!String.IsNullOrEmpty(expression)) {
                value = DataBinder.Eval(container, expression);
            }
            return Convert.ToString(value, CultureInfo.CurrentCulture);
        }

        #region IEnumerable Members
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion
    }
}
