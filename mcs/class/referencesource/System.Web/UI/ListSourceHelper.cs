//------------------------------------------------------------------------------
// <copyright file="ListSourceHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.Collections;
    using System.ComponentModel;

    public static class ListSourceHelper {

        public static bool ContainsListCollection(IDataSource dataSource) {
            ICollection viewNames = dataSource.GetViewNames();
            if (viewNames != null && viewNames.Count > 0) {
                return true;
            }
            return false;
        }


        public static IList GetList(IDataSource dataSource) {
            ICollection viewNames = dataSource.GetViewNames();
            
            if (viewNames != null && viewNames.Count > 0) {
                return new ListSourceList(dataSource);
            }
            return null;
        }

        internal sealed class ListSourceList : CollectionBase, ITypedList {
            IDataSource _dataSource;
    
            public ListSourceList(IDataSource dataSource) {
                _dataSource = dataSource;
                ((IList)this).Add(new ListSourceRow(_dataSource));
            }

            #region ITypedList implementation
            string ITypedList.GetListName(PropertyDescriptor[] listAccessors) {
                return String.Empty;
            }
    
            PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors) {
                if (_dataSource != null) {
                    ICollection viewNames = _dataSource.GetViewNames();
                    if (viewNames != null && viewNames.Count > 0) {
                        string[] viewNamesArray = new string[viewNames.Count];
                        viewNames.CopyTo(viewNamesArray, 0);
                        PropertyDescriptor[] props = new PropertyDescriptor[viewNames.Count];
                        for (int i = 0; i < viewNamesArray.Length; i++) {
                            props[i] = new ListSourcePropertyDescriptor(viewNamesArray[i]);
                        }
                        return new PropertyDescriptorCollection(props);
                    }                    
                }
                return new PropertyDescriptorCollection(null);
            }
            #endregion ITypedList implementations
        }

        internal class ListSourceRow {
            IDataSource _dataSource;

            public ListSourceRow(IDataSource dataSource) {
                _dataSource = dataSource;
            }

            public IDataSource DataSource {
                get {
                    return _dataSource;
                }
            }
        }

        internal class ListSourcePropertyDescriptor : PropertyDescriptor {
            string _name;

            public ListSourcePropertyDescriptor(string name) : base(name, null) {
                _name = name;
            }

            public override Type ComponentType {
                get {
                    return typeof(ListSourceRow);
                }
            }

            public override bool IsReadOnly {
                get {
                    return true;
                }
            }

            public override Type PropertyType {
                get {
                    return typeof(IEnumerable);
                }
            }

            public override bool CanResetValue(object value) {
                return false;
            }

            public override object GetValue(object source) {
                if (source is ListSourceRow) {
                    ListSourceRow row = (ListSourceRow)source;
                    IDataSource dataSource = row.DataSource;
                    return dataSource.GetView(_name).ExecuteSelect(DataSourceSelectArguments.Empty);
                }
                return null;
            }

            public override void ResetValue(object component) {
                throw new NotSupportedException();
            }

            public override void SetValue(object component, object value) {
                throw new NotSupportedException();
            }

            public override bool ShouldSerializeValue(object component) {
                return false;
            }
        }
    }
}
