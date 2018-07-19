//------------------------------------------------------------------------------
// <copyright file="SelectionListDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls 
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Diagnostics;
    using System.Web.UI.MobileControls;
    using System.Web.UI.Design.MobileControls.Adapters;

    using DataBinding = System.Web.UI.DataBinding;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class SelectionListDesigner : 
        MobileControlDesigner, IListDesigner, IDataSourceProvider
    {
        private SelectionList           _selectionList;
        private DesignerVerbCollection  _designerVerbs;

        private DataTable               _dummyDataTable;
        private DataTable               _designTimeDataTable;

        private const String _dataSourcePropertyName = "DataSource";
        private const String _dataMemberPropertyName = "DataMember";
        private const String _dataTextFieldPropertyName = "DataTextField";
        private const String _dataValueFieldPropertyName = "DataValueField";
        private static readonly Attribute[] _emptyAttrs = new Attribute[0];

        /// <summary>
        /// </summary>
        public String DataValueField 
        {
            get 
            {
                return _selectionList.DataValueField;
            }
            set
            {
                _selectionList.DataValueField = value;
            }
        }

        /// <summary>
        /// </summary>
        public String DataTextField 
        {
            get 
            {
                return _selectionList.DataTextField;
            }
            set
            {
                _selectionList.DataTextField = value;
            }
        }

        public String DataMember 
        {
            get 
            {
                return _selectionList.DataMember;
            }
            set 
            {
                _selectionList.DataMember = value;
                OnDataSourceChanged();
            }
        }

        public MobileListItemCollection Items
        {
            get
            {
                return _selectionList.Items;
            }
        }

        /// <summary>
        ///    <para>
        ///       Gets or sets the data source property.
        ///    </para>
        /// </summary>
        /// <value>
        ///    <para>
        ///       A string indicating the data source for the designer's control.
        ///    </para>
        /// </value>
        /// <remarks>
        ///    <para>
        ///       Designer implementation of a DataSource property that operates on the
        ///       DataSource property in the control's binding collection.
        ///    </para>
        /// </remarks>
        public String DataSource 
        {
            get 
            {
                DataBinding binding = DataBindings[_dataSourcePropertyName];

                if (binding != null) 
                {
                    return binding.Expression;
                }
                return String.Empty;
            }
            set 
            {
                if ((value == null) || (value.Length == 0)) 
                {
                    DataBindings.Remove(_dataSourcePropertyName);
                }
                else 
                {
                    DataBinding binding = DataBindings[_dataSourcePropertyName];

                    if (binding == null) 
                    {
                        binding = new DataBinding(_dataSourcePropertyName, typeof(IEnumerable), value);
                    }
                    else 
                    {
                        binding.Expression = value;
                    }
                    DataBindings.Add(binding);
                }

                OnDataSourceChanged();
                OnBindingsCollectionChanged(_dataSourcePropertyName);
            }
        }

        /// <summary>
        ///    <para>
        ///       The designer's collection of verbs.
        ///    </para>
        /// </summary>
        /// <value>
        ///    <para>
        ///       An array of type <see cref='DesignerVerb'/> containing the verbs available to the
        ///       designer.
        ///    </para>
        /// </value>
        public override DesignerVerbCollection Verbs 
        {
            get 
            {
                if (null == _designerVerbs)
                {
                    _designerVerbs = base.Verbs;
                    _designerVerbs.Add(new DesignerVerb(SR.GetString(SR.PropertyBuilderVerb),
                        new EventHandler(this.OnPropertyBuilder)));
                }

                return _designerVerbs;
            }
        }

        /// <summary>
        ///    <para>
        ///       Gets sample data matching the schema of the selected data source.
        ///    </para>
        /// </summary>
        /// <param name='minimumRows'>
        ///    The minimum rows of sample data the data source data should contain.
        /// </param>
        /// <param name='dummyDataSource'>
        ///    Whether the data source being returned is a dummy data source.
        /// </param>
        /// <returns>
        ///    <para>
        ///       An IEnumerable containing a live data source for use at
        ///       design time.
        ///    </para>
        /// </returns>
        ///
        protected IEnumerable GetDesignTimeDataSource(
            int minimumRows, out bool dummyDataSource)
        {
            IEnumerable selectedDataSource = GetResolvedSelectedDataSource();
            return GetDesignTimeDataSource(
                selectedDataSource, minimumRows, out dummyDataSource);
        }

        /// <summary>
        ///    <para>
        ///       Gets sample data matching the schema of the selected data source.
        ///    </para>
        /// </summary>
        /// <param name='selectedDataSource'>
        ///    The selected data source to be used as a reference for the shape of the data.
        /// </param>
        /// <param name='minimumRows'>
        ///    The minimum rows of sample data the data source data should contain.
        /// </param>
        /// <param name='dummyDataSource'>
        ///    Whether the data source being returned is a dummy data source.
        /// </param>
        /// <returns>
        ///    <para>
        ///       An IEnumerable containing
        ///       a live data source for use at design time.
        ///    </para>
        /// </returns>
        ///
        protected IEnumerable GetDesignTimeDataSource(
            IEnumerable selectedDataSource, int minimumRows, out bool dummyDataSource)
        {
            DataTable dataTable = _designTimeDataTable;
            dummyDataSource = false;

            // use the datatable corresponding to the selected datasource if possible
            if (dataTable == null) 
            {
                if (selectedDataSource != null) 
                {
                    _designTimeDataTable = 
                        DesignTimeData.CreateSampleDataTable(selectedDataSource);
                    dataTable = _designTimeDataTable;
                }

                if (dataTable == null) 
                {
                    // fallback on a dummy datasource if we can't create a sample datatable
                    if (_dummyDataTable == null) 
                    {
                        _dummyDataTable = DesignTimeData.CreateDummyDataTable();
                    }

                    dataTable = _dummyDataTable;
                    dummyDataSource = true;
                }
            }

            IEnumerable liveDataSource = 
                DesignTimeData.GetDesignTimeDataSource(dataTable, minimumRows);
            return liveDataSource;
        }

        /// <summary>
        ///    <para>
        ///       Gets the HTML to be used for the design-time representation
        ///       of the control.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The design-time HTML.
        ///    </para>
        /// </returns>
        protected override String GetDesignTimeNormalHtml() 
        {
            const int numberOfStaticItems = 5;
            IEnumerable selectedDataSource = null;
            String oldDataTextField = null, oldDataValueField = null;
            bool dummyDataSource = false;

            DesignerTextWriter htmlWriter = new DesignerTextWriter(true);

            MobileListItemCollection items = _selectionList.Items;
            Debug.Assert(items != null, "Items is null in LisControl");

            if (items.Count > 0)
            {
                _selectionList.Adapter.Render(htmlWriter);
            }
            else
            {
                MobileListItem[] oldItems = items.GetAll();
                int sampleRows = numberOfStaticItems;

                // try designtime databinding.
                selectedDataSource = GetResolvedSelectedDataSource();

                IEnumerable designTimeDataSource = 
                    GetDesignTimeDataSource(
                    selectedDataSource,
                    sampleRows,
                    out dummyDataSource);

                // If dummy datasource is applied, change the data fields so that 
                // dummy source will be rendered.
                if (dummyDataSource)
                {
                    oldDataTextField    = _selectionList.DataTextField;
                    oldDataValueField   = _selectionList.DataValueField;
                    _selectionList.DataTextField    = "Column0";
                    _selectionList.DataValueField   = "Column1";
                }

                try
                {
                    _selectionList.DataSource = designTimeDataSource;
                    _selectionList.DataBind();
                    _selectionList.Adapter.Render(htmlWriter);
                }
                finally
                {
                    _selectionList.DataSource = null;
                    _selectionList.Items.SetAll(oldItems);

                    if (dummyDataSource)
                    {
                        _selectionList.DataTextField = oldDataTextField;
                        _selectionList.DataValueField = oldDataValueField;
                    }
                }
            }

            return htmlWriter.ToString();
        }

        public IEnumerable GetResolvedSelectedDataSource() 
        {
            IEnumerable selectedDataSource = null;

            DataBinding binding = DataBindings["DataSource"];

            if (binding != null) 
            {
                selectedDataSource = 
                    DesignTimeData.GetSelectedDataSource(
                    _selectionList,
                    binding.Expression,
                    DataMember);
            }

            return selectedDataSource;
        }

        /// <summary>
        ///    <para>
        ///       Gets the selected data source component from the component's container.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       An IEnumerable with the
        ///       selected data source, or <see langword='null'/> if a data source is not found, or if a data
        ///       source with the same name does not exist.
        ///    </para>
        /// </returns>
        /// <seealso cref='System.Web.UI.Design.IDataSourceProvider'/>
        public Object GetSelectedDataSource() 
        {
            Object selectedDataSource = null;

            DataBinding binding = DataBindings[_dataSourcePropertyName];

            if (binding != null) 
            {
                selectedDataSource = 
                    DesignTimeData.GetSelectedDataSource(_selectionList, binding.Expression);
            }

            return selectedDataSource;
        }

        /// <summary>
        ///    <para>
        ///       Initializes the designer.
        ///    </para>
        /// </summary>
        /// <param name='component'>
        ///    The control element being designed.
        /// </param>
        /// <remarks>
        ///    <para>
        ///       This is called by the designer host to establish the component being
        ///       designed.
        ///    </para>
        /// </remarks>
        /// <seealso cref='System.ComponentModel.Design.IDesigner'/>
        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is System.Web.UI.MobileControls.SelectionList,
                "SelectionListDesigner.Initialize - Invalid SelectionList Control");

            _selectionList = (SelectionList) component;
            base.Initialize(component);
        }

        /// <summary>
        ///    <para>
        ///       Invokes the property builder beginning with the specified page.
        ///    </para>
        /// </summary>
        /// <param name='initialPage'>
        ///    The page to begin with.
        /// </param>
        protected internal void InvokePropertyBuilder(int initialPage) 
        {
            IComponentChangeService changeService = null;
            bool result = false;

            changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
            if (changeService != null) 
            {
                try 
                {
                    changeService.OnComponentChanging(_selectionList, null);
                }
                catch (CheckoutException ex) 
                {
                    if (ex == CheckoutException.Canceled)
                    {
                        return;
                    }
                    throw;
                }
            }

            try 
            {
                SelectionListComponentEditor compEditor = new SelectionListComponentEditor(initialPage);
                result = compEditor.EditComponent(_selectionList);
            }
            finally
            {
                if (changeService != null)
                {
                    changeService.OnComponentChanged(_selectionList, null, null, null);

                    if (IMobileWebFormServices != null)
                    {
                        IMobileWebFormServices.ClearUndoStack();
                    }
                }
            }
        }

        /// <summary>
        ///    <para>
        ///       Represents the method that will handle the component change event.
        ///    </para>
        /// </summary>
        /// <param name='sender'>
        ///    The source of the event.
        /// </param>
        /// <param name=' e'>
        ///    The <see cref='System.ComponentModel.Design.ComponentChangedEventArgs'/> that provides data about the event.
        /// </param>
        public override void OnComponentChanged(Object sender, ComponentChangedEventArgs e) 
        {
            if (e.Member != null)
            {
                if (e.Member.Name.Equals(_dataSourcePropertyName) || 
                    e.Member.Name.Equals(_dataMemberPropertyName))
                {
                    OnDataSourceChanged();
                }
            }

            base.OnComponentChanged(sender, e);
        }

        /// <internalonly/>
        /// <summary>
        ///    <para>
        ///       Raises the DataSourceChanged event.
        ///    </para>
        /// </summary>
        public void OnDataSourceChanged() 
        {
            _designTimeDataTable = null;
        }

        /// <summary>
        ///    <para>
        ///       Represents the method that will handle the property builder event.
        ///    </para>
        /// </summary>
        /// <param name='sender'>
        ///    The source of the event.
        /// </param>
        /// <param name=' e'>
        ///    An EventArgs object that provides data about the event.
        /// </param>
        protected void OnPropertyBuilder(Object sender, EventArgs e) 
        {
            InvokePropertyBuilder(0);
        }

        /// <summary>
        ///    <para>
        ///       Filter the properties to replace the runtime DataSource property
        ///       descriptor with the designer's.
        ///    </para>
        /// </summary>
        /// <param name='properties'>
        ///    The set of properties to filter.
        /// </param>
        /// <seealso cref='IDesignerFilter'/>
        protected override void PreFilterProperties(IDictionary properties) 
        {
            base.PreFilterProperties(properties);

            Type designerType = this.GetType();

            DesignerAdapterUtil.AddAttributesToPropertiesOfDifferentType(
                designerType,
                typeof(String),
                properties,
                _dataSourcePropertyName, 
                new TypeConverterAttribute(typeof(DataSourceConverter)));

            DesignerAdapterUtil.AddAttributesToProperty(
                designerType,
                properties,
                _dataMemberPropertyName, 
                _emptyAttrs);

            DesignerAdapterUtil.AddAttributesToProperty(
                designerType,
                properties,
                _dataTextFieldPropertyName, 
                _emptyAttrs);

            DesignerAdapterUtil.AddAttributesToProperty(
                designerType,
                properties,
                _dataValueFieldPropertyName, 
                _emptyAttrs);
        }
    }
}
