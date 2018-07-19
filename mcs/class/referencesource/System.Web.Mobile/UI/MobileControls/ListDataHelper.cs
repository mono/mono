//------------------------------------------------------------------------------
// <copyright file="ListDataHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;
using System.Diagnostics;

namespace System.Web.UI.MobileControls
{
    /*
     * List Data Helper class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class ListDataHelper
    {
        private IListControl _parent;
        private StateBag _parentViewState;
        private MobileListItemCollection _items;
        private Object _dataSource;
        private IEnumerable _resolvedDataSource;
        private int _dataSourceCount = -1;
        private String _dataTextField;
        private String _dataValueField;
        private bool _bindFromFields;

        internal /*public*/ ListDataHelper(IListControl parent, StateBag parentViewState)
        {
            _parent = parent;
            _parentViewState = parentViewState;
        }

        internal /*public*/ MobileListItemCollection Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new MobileListItemCollection();
                    if (_parent.TrackingViewState)
                    {
                        ((IStateManager)_items).TrackViewState();
                    }
                }
                return _items;
            }
        }
        
        internal /*public*/ bool HasItems()
        {
            return _items != null;
        }

        internal /*public*/ Object DataSource 
        {
            get 
            {
                return _dataSource;
            }

            set 
            {
                _dataSource = value;
                _resolvedDataSource = null;
            }
        }

        internal /*public*/ String DataMember
        {
            get 
            {
                String s = (String)_parentViewState["DataMember"];
                return s == null ? String.Empty : s;
            }

            set 
            {
                _parentViewState["DataMember"] = value;
            }
        }

        internal /*public*/ String DataTextField 
        {
            get 
            {
                String s = (String)_parentViewState["DataTextField"];
                return (s != null) ? s : String.Empty;
            }
            set 
            {
                _parentViewState["DataTextField"] = value;
            }
        }

        internal /*public*/ String DataValueField 
        {
            get 
            {
                String s = (String)_parentViewState["DataValueField"];
                return (s != null) ? s : String.Empty;
            }
            set 
            {
                _parentViewState["DataValueField"] = value;
            }
        }

        internal /*public*/ IEnumerable ResolvedDataSource
        {
            get
            {
                if (_resolvedDataSource == null)
                {
                    _resolvedDataSource = 
                        DataSourceHelper.GetResolvedDataSource(DataSource, DataMember);
                }
                return _resolvedDataSource;
            }
        }

        /*
        internal int DataSourceCount
        {
            get
            {
                if (_dataSourceCount == -1)
                {
                    IEnumerable dataSource = ResolvedDataSource;
                    if (dataSource != null)
                    {
                        ICollection collection = dataSource as ICollection;
                        if (collection != null)
                        {
                            _dataSourceCount = collection.Count;
                        }
                        else
                        {
                            int count = 0;
                            IEnumerator enumerator = dataSource.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                count++;
                            }
                            _dataSourceCount = count;
                        }
                    }
                    else
                    {
                        _dataSourceCount = 0;
                    }
                }
                return _dataSourceCount;
            }
        }
        */

        internal /*public*/ void CreateItems(IEnumerable dataSource) 
        {
            Debug.Assert (dataSource != null);
            Items.Clear();
            _dataTextField = DataTextField;
            _dataValueField = DataValueField;
            _bindFromFields = (_dataTextField.Length > 0) || (_dataValueField.Length > 0);
            foreach (Object dataItem in dataSource)
            {
                MobileListItem listItem = CreateItem(dataItem);

                if (listItem != null) 
                {
                    AddItem(listItem);
                }
            }
            _dataSourceCount = Items.Count;
        }

        private MobileListItem CreateItem(Object dataItem)
        {
            MobileListItem listItem = null;
            String textField = null;
            String valueField = null;

            if (_bindFromFields)
            {
                if (_dataTextField.Length > 0)
                {
                    textField = DataBinder.GetPropertyValue(dataItem, _dataTextField, "{0}");
                }
                if (_dataValueField.Length > 0)
                {
                    valueField = DataBinder.GetPropertyValue(dataItem, _dataValueField, "{0}");
                }
            }
            else
            {
                textField = dataItem.ToString();
            }
            listItem = new MobileListItem(dataItem, textField, valueField);

            // Use delegated data binding, if specified.
            if (dataItem != null)
            {
                _parent.OnItemDataBind(new ListDataBindEventArgs(listItem, dataItem));
            }

            return listItem;
        }

        internal /*public*/ void AddItem(MobileListItem item)
        {
            MobileListItemCollection items = Items;
            items.Add(item);
        }
    }
}
