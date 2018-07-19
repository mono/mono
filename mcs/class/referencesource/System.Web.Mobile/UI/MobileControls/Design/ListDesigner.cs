//------------------------------------------------------------------------------
// <copyright file="ListDesigner.cs" company="Microsoft">
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
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.MobileControls;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Web.UI.Design.MobileControls.Adapters;

    using DataBinding = System.Web.UI.DataBinding;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ListDesigner :
        MobileTemplatedControlDesigner, IListDesigner, IDataSourceProvider
    {
        private System.Web.UI.MobileControls.List _list;
        private DesignerVerbCollection _designerVerbs;

        private int _numberItems;
        private DataTable _dummyDataTable;
        private DataTable _designTimeDataTable;

        private const String _dataSourcePropertyName        = "DataSource";
        private const String _dataMemberPropertyName        = "DataMember";
        private const String _dataTextFieldPropertyName     = "DataTextField";
        private const String _dataValueFieldPropertyName    = "DataValueField";

        private const int _headerFooterTemplates            = 0;
        private const int _itemTemplates                    = 1;
        private const int _separatorTemplate                = 2;
        private const int _numberOfTemplateFrames           = 3;

        private static readonly String[][] _templateFrameNames =
            new String[][] {
                               new String [] { Constants.HeaderTemplateTag, Constants.FooterTemplateTag },
                               new String [] { Constants.ItemTemplateTag, Constants.AlternatingItemTemplateTag },
                               new String [] { Constants.SeparatorTemplateTag }
                           };
        private static readonly Attribute[] _emptyAttrs = new Attribute[0];

        /// <summary>
        /// </summary>
        public String DataValueField 
        {
            get 
            {
                return _list.DataValueField;
            }
            set
            {
                _list.DataValueField = value;
            }
        }

        /// <summary>
        /// </summary>
        public String DataTextField 
        {
            get 
            {
                return _list.DataTextField;
            }
            set
            {
                _list.DataTextField = value;
            }
        }

        public String DataMember 
        {
            get 
            {
                return _list.DataMember;
            }
            set 
            {
                _list.DataMember = value;
                OnDataSourceChanged();
            }
        }

        public MobileListItemCollection Items
        {
            get
            {
                return _list.Items;
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
                if (_designerVerbs == null)
                {
                    _designerVerbs = base.Verbs;
                    _designerVerbs.Add(new DesignerVerb(SR.GetString(SR.PropertyBuilderVerb), 
                        new EventHandler(this.OnPropertyBuilder)));
                }

                Debug.Assert(_designerVerbs.Count == 2);
                _designerVerbs[0].Enabled = !this.InTemplateMode;
                _designerVerbs[1].Enabled = !this.InTemplateMode;

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
        private IEnumerable GetDesignTimeDataSource(int minimumRows, out bool dummyDataSource)
        {
            IEnumerable selectedDataSource = GetResolvedSelectedDataSource();
            return GetDesignTimeDataSource(selectedDataSource, minimumRows, out dummyDataSource);
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
        private IEnumerable GetDesignTimeDataSource(IEnumerable selectedDataSource, int minimumRows, out bool dummyDataSource) 
        {
            DataTable dataTable = _designTimeDataTable;
            dummyDataSource = false;

            // use the datatable corresponding to the selected datasource if possible
            if (dataTable == null) 
            {
                if (selectedDataSource != null) 
                {
                    _designTimeDataTable = DesignTimeData.CreateSampleDataTable(selectedDataSource);

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

            IEnumerable liveDataSource = DesignTimeData.GetDesignTimeDataSource(dataTable, minimumRows);
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
        /// <remarks>
        ///    <para>
        ///       The rule for handing DesignTimeHTML is similar to System.Web.UI.DataList control,
        ///       if list has a HTML templateset, then generate sample data from static data or 
        ///       dynamic data (if static data does not exist). Show the sample data with templates
        ///       applied.
        ///    </para>
        /// </remarks>
        protected override String GetDesignTimeNormalHtml() 
        {
            IEnumerable selectedDataSource = null;
            String oldDataTextField = null, oldDataValueField = null;
            bool dummyDataSource = false;

            DesignerTextWriter writer = new DesignerTextWriter(true);

            // Apply the current device specific
            if (_list.DeviceSpecific != null)
            {
                _list.DeviceSpecific.SetDesignerChoice(CurrentChoice);
            }

            MobileListItemCollection items = _list.Items;
            Debug.Assert(items != null, "Items is null in LisControl");
            MobileListItem[] oldItems = items.GetAll();

            // Hack: If List is templated, use DataBind() to create child controls.
            //       If it is empty, use dummy data source to create fake child controls.
            if (_list.IsTemplated || items.Count == 0)
            {
                int sampleRows = items.Count;

                // If List does not contain any items, use five dummy items.
                if (sampleRows == 0)
                {
                    sampleRows = 5;
                }

                // try designtime databinding.
                selectedDataSource = GetResolvedSelectedDataSource();

                // Recreate the dummy data table, if number of items changed. 
                if (sampleRows != _numberItems)
                {
                    OnDummyDataTableChanged();

                    // keep the new item count
                    _numberItems = sampleRows;
                }

                IEnumerable designTimeDataSource = 
                    GetDesignTimeDataSource(selectedDataSource, sampleRows, out dummyDataSource);

                // If dummy datasource is applied, change the data fields so that 
                // dummy source will be rendered.
                if (dummyDataSource)
                {
                    Debug.Assert(_dummyDataTable != null && _dummyDataTable.Columns.Count > 1);
                    oldDataTextField = _list.DataTextField;
                    oldDataValueField = _list.DataValueField;
                    _list.DataTextField = _dummyDataTable.Columns[0].ColumnName;
                    _list.DataValueField = _dummyDataTable.Columns[1].ColumnName;
                }

                try
                {
                    _list.DataSource = designTimeDataSource;
                    _list.DataBind();
                    _list.Adapter.Render(writer);
                }
                finally
                {
                    _list.DataSource = null;

                    // restore the old items since databinding creates items from templates.
                    _list.Items.SetAll(oldItems);

                    // clear all child controls created by databinding.
                    _list.Controls.Clear();

                    if (dummyDataSource)
                    {
                        _list.DataTextField = oldDataTextField;
                        _list.DataValueField = oldDataValueField;
                    }
                }
            }
            // Otherwise, list only contains static items, just render it directly.
            else
            {
                _list.Adapter.Render(writer);
            }

            return writer.ToString();
        }

        public IEnumerable GetResolvedSelectedDataSource() 
        {
            IEnumerable selectedDataSource = null;

            DataBinding binding = DataBindings[_dataSourcePropertyName];

            if (binding != null) 
            {
                selectedDataSource = DesignTimeData.GetSelectedDataSource(_list, binding.Expression, DataMember);
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
                selectedDataSource = DesignTimeData.GetSelectedDataSource(_list, binding.Expression);
            }

            return selectedDataSource;
        }

        public override string GetTemplateContainerDataItemProperty(string templateName) 
        {
            return "DataItem";
        }

        /// <summary>
        ///    <para>
        ///       Gets the template's container's data source.
        ///    </para>
        /// </summary>
        /// <param name='templateName'>
        ///    The name of the template to retrieve the data source for.
        /// </param>
        /// <returns>
        ///    <para>
        ///       An IEnumerable containing the data source or data sources available to
        ///       the template's container.
        ///    </para>
        /// </returns>
        public override IEnumerable GetTemplateContainerDataSource(String templateName) 
        {
            return GetResolvedSelectedDataSource();
        }

        protected override String[] GetTemplateFrameNames(int index)
        {
            Debug.Assert(index >= 0 & index <= _templateFrameNames.Length);
            return _templateFrameNames[index];
        }

        public override Type GetTemplatePropertyParentType(String templateName) 
        {
            return typeof(MobileTemplatedControlDesigner.TemplateContainer);
        }

        protected override TemplateEditingVerb[] GetTemplateVerbs()
        {
            TemplateEditingVerb[] templateVerbs = new TemplateEditingVerb[_numberOfTemplateFrames];

            templateVerbs[_headerFooterTemplates] = new TemplateEditingVerb(
                SR.GetString(SR.TemplateFrame_HeaderFooterTemplates),
                _headerFooterTemplates,
                this);
            templateVerbs[_itemTemplates] = new TemplateEditingVerb(
                SR.GetString(SR.TemplateFrame_ItemTemplates),
                _itemTemplates,
                this);
            templateVerbs[_separatorTemplate] = new TemplateEditingVerb(
                SR.GetString(SR.TemplateFrame_SeparatorTemplate),
                _separatorTemplate,
                this);

            return templateVerbs;
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
            Debug.Assert(component is System.Web.UI.MobileControls.List,
                "ListDesigner.Initialize - Invalid List Control");

            _list = (System.Web.UI.MobileControls.List) component;
            base.Initialize(component);

            Debug.Assert(_list.Items != null, "Items cannot be null.");
            _numberItems = _list.Items.Count;
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
                    changeService.OnComponentChanging(_list, null);
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
                ListComponentEditor compEditor = new ListComponentEditor(initialPage);
                result = compEditor.EditComponent(_list);
            }
            finally
            {
                if (changeService != null)
                {
                    changeService.OnComponentChanged(_list, null, null, null);

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
                String memberName = e.Member.Name;
                if (memberName.Equals(_dataSourcePropertyName) || 
                    memberName.Equals(_dataMemberPropertyName))
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

        private void OnDummyDataTableChanged() 
        {
            _dummyDataTable = null;
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
