//------------------------------------------------------------------------------
// <copyright file="ObjectListDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls 
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Design;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.MobileControls;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    using System.Web.UI.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Adapters;

    using DataBinding = System.Web.UI.DataBinding;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ObjectListDesigner : MobileTemplatedControlDesigner, IDataSourceProvider, IMobileDesigner
    {
        private DataTable _dummyDataTable;
        private DataTable _designTimeDataTable;

        private System.Web.UI.MobileControls.ObjectList _objectList;
        private DesignerVerbCollection _designerVerbs;

        private const String _dataSourcePropertyName    = "DataSource";
        private const String _dataMemberPropertyName    = "DataMember";

        private const int _headerFooterTemplates            = 0;
        private const int _itemTemplates                    = 1;
        private const int _separatorTemplate                = 2;
        private const int _numberOfTemplateFrames           = 3;

        private static readonly String[][] _templateFrameNames =
            new String[][] {
                               new String [] { Constants.HeaderTemplateTag, Constants.FooterTemplateTag },
                               new String [] { Constants.ItemTemplateTag, Constants.AlternatingItemTemplateTag, Constants.ItemDetailsTemplateTag },
                               new String [] { Constants.SeparatorTemplateTag },
                           };

        private static readonly Attribute[] _emptyAttrs = new Attribute[0];

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
            Debug.Assert(component is System.Web.UI.MobileControls.ObjectList,
                         "ObjectListDesigner.Initialize - Invalid ObjectList Control");

            _objectList = (System.Web.UI.MobileControls.ObjectList) component;
            base.Initialize(component);
        }

        protected override String[] GetTemplateFrameNames(int index)
        {
            Debug.Assert(index >= 0 & index <= _templateFrameNames.Length);
            return _templateFrameNames[index];
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
        ///       Gets the HTML to be used for the design time representation
        ///       of the control.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The design time HTML.
        ///    </para>
        /// </returns>
        protected override String GetDesignTimeNormalHtml()
        {
            int sampleRows = 5;
            bool dummyDataSource = false;
            String oldLabelField, oldTableFields;
            oldLabelField = _objectList.LabelField;
            oldTableFields = _objectList.TableFields;

            DesignerTextWriter htmlWriter = new DesignerTextWriter(true);

            if (_objectList.DeviceSpecific != null)
            {
                _objectList.DeviceSpecific.SetDesignerChoice(CurrentChoice);
            }

            IEnumerable designTimeDataSource = GetDesignTimeDataSource(sampleRows, out dummyDataSource);

            bool oldAutoGenerateFields = _objectList.AutoGenerateFields;
            if ((oldAutoGenerateFields == false) && (_objectList.Fields.Count == 0))
            {
                // ensure that AutoGenerateFields is true when we don't have
                // any fields defined, so we see atleast something at design time.
                _objectList.AutoGenerateFields = true;
            }

            // Replace original labelfield with empty string to ensure dummy datasource will be rendered 
            if (dummyDataSource)
            {
                _objectList.LabelField  = String.Empty;
                _objectList.TableFields = String.Empty;
            }

            try 
            {
                _objectList.DataSource = designTimeDataSource;
                _objectList.DataBind();
                _objectList.Adapter.Render(htmlWriter);
            }
            finally
            {
                _objectList.DataSource = null;
                _objectList.AutoGenerateFields = oldAutoGenerateFields;

                if (dummyDataSource)
                {
                    _objectList.LabelField = oldLabelField;
                    _objectList.TableFields = oldTableFields;
                }

                // Remove controls created by databinding the DataSource
                _objectList.Controls.Clear();
                _objectList.InvalidateDisplayFieldIndices();
            }

            return htmlWriter.ToString();
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

        /////////////////////////////////////////////////////////////////////////
        //  BEGIN OBJECTLIST DESIGNER DATASOURCE HANDLING
        /////////////////////////////////////////////////////////////////////////

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
        }

        public String DataMember 
        {
            get 
            {
                return _objectList.DataMember;
            }
            set 
            {
                _objectList.DataMember = value;
                OnDataSourceChanged();
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
                        binding = new DataBinding(
                                      _dataSourcePropertyName,
                                      typeof(IEnumerable),
                                      value);
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

        public IEnumerable GetResolvedSelectedDataSource() 
        {
            IEnumerable selectedDataSource = null;

            DataBinding binding = DataBindings[_dataSourcePropertyName];

            if (binding != null) 
            {
                selectedDataSource = DesignTimeData.GetSelectedDataSource(_objectList, binding.Expression, DataMember);
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
                selectedDataSource = DesignTimeData.GetSelectedDataSource(_objectList, binding.Expression);
            }

            return selectedDataSource;
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

        public override String GetTemplateContainerDataItemProperty(string templateName) 
        {
            return "DataItem";
        }

        public override Type GetTemplatePropertyParentType(String templateName) 
        {
            return typeof(MobileTemplatedControlDesigner.TemplateContainer);
        }

        /// <internalonly/>
        /// <summary>
        ///    <para>
        ///       Raises the DataSourceChanged event.
        ///    </para>
        /// </summary>
        protected internal virtual void OnDataSourceChanged()
        {
            _designTimeDataTable = null;
        }

        public new void UpdateRendering()
        {
            _objectList.LabelStyle.Refresh();
            _objectList.CommandStyle.Refresh();

            base.UpdateRendering();
        }

        /////////////////////////////////////////////////////////////////////////
        //  END OBJECTLIST DESIGNER DATASOURCE HANDLING
        /////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////
        //  BEGIN DESIGNER VERBS
        /////////////////////////////////////////////////////////////////////////

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
                Debug.Assert(_designerVerbs.Count == 2);

                _designerVerbs[0].Enabled = !this.InTemplateMode;
                _designerVerbs[1].Enabled = !this.InTemplateMode;
                return _designerVerbs;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  BEGIN OBJECTLIST DESIGNER EVENTHANDLERS
        /////////////////////////////////////////////////////////////////////////

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
                    changeService.OnComponentChanging(_objectList, null);
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
                ObjectListComponentEditor compEditor = new ObjectListComponentEditor(initialPage);
                result = compEditor.EditComponent(_objectList);
            }
            finally
            {
                if (changeService != null)
                {
                    changeService.OnComponentChanged(_objectList, null, null, null);

                    if (IMobileWebFormServices != null)
                    {
                        IMobileWebFormServices.ClearUndoStack();
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  END STYLE DESIGNER EVENTHANDLERS
        /////////////////////////////////////////////////////////////////////////
    }
}
