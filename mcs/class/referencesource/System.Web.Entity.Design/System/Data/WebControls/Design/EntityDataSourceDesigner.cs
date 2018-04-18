//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Web.UI.WebControls;
using System.Web.UI.Design;
using System.Windows.Forms;
using System.Web.UI.Design.WebControls.Util;

namespace System.Web.UI.Design.WebControls
{
    public class EntityDataSourceDesigner : DataSourceDesigner
    {
        private EntityDataSourceDesignerHelper _helper;
        
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            _helper = new EntityDataSourceDesignerHelper(component as EntityDataSource, true /*interactiveMode*/);
            _helper.AddSystemWebEntityReference();
        }

        // Whether or not the EntityDataSource can be configured. Currently we have no conditions where you can't at least attempt to
        // configure it. If there is no metadata available, an error may occur, but you can still try to configure the control.
        public override bool CanConfigure
        {
            get
            {                
                return true;
            }
        }

        public override bool CanRefreshSchema
        {
            get
            {
                // Minimum properties required for schema are ConnectionString and DefaultContainerName, plus EntitySetName or CommandText
                return (!String.IsNullOrEmpty(_helper.ConnectionString) && !String.IsNullOrEmpty(_helper.DefaultContainerName)) &&
                    (!String.IsNullOrEmpty(_helper.EntitySetName) || !String.IsNullOrEmpty(_helper.CommandText));                
            }
        }

        public override void Configure()
        {
            InvokeTransactedChange(Component,
                new TransactedChangeCallback(ConfigureDataSourceChangeCallback),
                null, Strings.WizardTransactionDescription);
        }

        private bool ConfigureDataSourceChangeCallback(object context)
        {
            try
            {
                SuppressDataSourceEvents();

                IServiceProvider serviceProvider = Component.Site as IServiceProvider;
                EntityDataSourceWizardForm form = new EntityDataSourceWizardForm(serviceProvider, _helper.LoadEntityDataSourceState(), this);
                DialogResult result = UIHelper.ShowDialog(serviceProvider, form);
                if (result == DialogResult.OK)
                {
                    _helper.SaveEntityDataSourceProperties(form.EntityDataSourceState);

                    OnDataSourceChanged(EventArgs.Empty);
                    RefreshSchema(true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                ResumeDataSourceEvents();
            }
        }

        #region Design-time Schema Support
        public override void RefreshSchema(bool preferSilent)
        {
            try
            {
                SuppressDataSourceEvents();
                _helper.RefreshSchema(preferSilent);
            }
            finally
            {
                ResumeDataSourceEvents();
            }
        }

        public override DesignerDataSourceView GetView(string viewName)
        {
            return _helper.GetView(viewName);
        }

        public override string[] GetViewNames()
        {
            return _helper.GetViewNames();
        }

        internal void FireOnDataSourceChanged(EventArgs e)
        {
            // Clear metadata first because anything we have cached is now invalid since a property has changed
            _helper.ClearMetadata();
            OnDataSourceChanged(e);
        }

        internal void FireOnSchemaRefreshed(EventArgs e)
        {
            OnSchemaRefreshed(e);
        }

        internal bool InternalViewSchemasEquivalent(IDataSourceViewSchema viewSchema1, IDataSourceViewSchema viewSchema2)
        {
            return ViewSchemasEquivalent(viewSchema1, viewSchema2);
        }

        internal virtual object LoadFromDesignerState(string key)
        {
            return DesignerState[key];
        }

        internal void SaveDesignerState(string key, object value)
        {
            DesignerState[key] = value;
        }
        #endregion

        #region Overridden control properties for providing editors and dropdowns in property grid
        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_DefaultContainerName),
        TypeConverter(typeof(EntityDataSourceContainerNameConverter)),
        ]
        public string DefaultContainerName
        {
            get
            {
                return _helper.DefaultContainerName;
            }
            set
            {
                _helper.DefaultContainerName = value;
            }
        }


        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_EntitySetName),
        TypeConverter(typeof(EntityDataSourceEntitySetNameConverter)),
        ]
        public string EntitySetName
        {
            get
            {
                return _helper.EntitySetName;
            }
            set
            {
                _helper.EntitySetName = value;
            }
        }

        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_EntityTypeFilter),
        TypeConverter(typeof(EntityDataSourceEntityTypeFilterConverter)),
        ]
        public string EntityTypeFilter
        {
            get
            {
                return _helper.EntityTypeFilter;
            }
            set
            {
                _helper.EntityTypeFilter = value;
            }
        }

        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_CommandText),
        DefaultValue(null),
        Editor(typeof(EntityDataSourceStatementEditor), typeof(UITypeEditor)),
        MergableProperty(false),
        ]
        public string CommandText
        {
            get
            {
                return _helper.CommandText;
            }
            set
            {
                _helper.CommandText = value;
            }
        }

        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_ConnectionString)
        ]
        public string ConnectionString
        {
            get
            {
                return _helper.ConnectionString;
            }
            set
            {
                _helper.ConnectionString = value;
            }
        }

        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_OrderBy),
        DefaultValue(null),
        Editor(typeof(EntityDataSourceStatementEditor), typeof(UITypeEditor)),
        MergableProperty(false),
        ]
        public string OrderBy
        {
            get
            {
                return _helper.OrderBy;
            }
            set
            {
                _helper.OrderBy = value;
            }
        }

        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Select),
        DefaultValue(""),
        Editor(typeof(EntityDataSourceStatementEditor), typeof(UITypeEditor)),
        MergableProperty(false),
        ]
        public string Select
        {
            get
            {
                return _helper.Select;
            }
            set
            {
                _helper.Select = value;
            }
        }

        [
        Category("Data"),
        ResourceDescription(WebControlsRes.PropertyDescription_Where),
        DefaultValue(null),
        Editor(typeof(EntityDataSourceStatementEditor), typeof(UITypeEditor)),
        MergableProperty(false),
        ]
        public string Where
        {
            get
            {
                return _helper.Where;
            }
            set
            {
                _helper.Where = value;
            }
        }
        #endregion

        #region Helper methods to manage properties and parameters in the statement editor
        internal bool AutoGenerateOrderByClause
        {
            get
            {
                return _helper.AutoGenerateOrderByClause;
            }
        }
        
        internal bool AutoGenerateWhereClause
        {
            get
            {
                return _helper.AutoGenerateWhereClause;
            }
        }

        internal ParameterCollection CloneCommandParameters()
        {
            return CloneParameterCollection(_helper.CommandParameters);
        }

        internal ParameterCollection CloneOrderByParameters()
        {
            return CloneParameterCollection(_helper.OrderByParameters);
        }

        internal ParameterCollection CloneSelectParameters()
        {
            return CloneParameterCollection(_helper.SelectParameters);
        }        

        internal ParameterCollection CloneWhereParameters()
        {
            return CloneParameterCollection(_helper.WhereParameters);
        }
        
        private ParameterCollection CloneParameterCollection(ParameterCollection original)
        {
            ParameterCollection clones = new ParameterCollection();
            CloneParameters(original, clones);
            return clones;
        }

        internal void CloneParameters(ParameterCollection originalParameters, ParameterCollection newParameters)
        {
            foreach (ICloneable parameter in originalParameters)
            {
                Parameter clone = (Parameter)parameter.Clone();
                RegisterClone(parameter, clone);
                newParameters.Add(clone);
            }
        }

        internal void SetCommandParameterContents(ParameterCollection newParams)
        {
            SetParameters(_helper.CommandParameters, newParams);
        }

        internal void SetOrderByParameterContents(ParameterCollection newParams)
        {
            SetParameters(_helper.OrderByParameters, newParams);
        }

        internal void SetSelectParameterContents(ParameterCollection newParams)
        {
            SetParameters(_helper.SelectParameters, newParams);
        }
        
        internal void SetWhereParameterContents(ParameterCollection newParams)
        {
            SetParameters(_helper.WhereParameters, newParams);
        }

        private void SetParameters(ParameterCollection original, ParameterCollection newParams)
        {
            original.Clear();
            foreach (Parameter parameter in newParams)
            {
                original.Add(parameter);
            }
        }
        #endregion

        protected override void PreFilterProperties(System.Collections.IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // Properties that are overridden in the designer because they have custom editors or converters
            Type designerType = GetType();
            properties["ConnectionString"] = TypeDescriptor.CreateProperty(designerType, "ConnectionString", typeof(string));
            properties["DefaultContainerName"] = TypeDescriptor.CreateProperty(designerType, "DefaultContainerName", typeof(string));
            properties["EntitySetName"] = TypeDescriptor.CreateProperty(designerType, "EntitySetName", typeof(string));
            properties["EntityTypeFilter"] = TypeDescriptor.CreateProperty(designerType, "EntityTypeFilter", typeof(string));
            properties["CommandText"] = TypeDescriptor.CreateProperty(designerType, "CommandText", typeof(string));
            properties["OrderBy"] = TypeDescriptor.CreateProperty(designerType, "OrderBy", typeof(string));
            properties["Select"] = TypeDescriptor.CreateProperty(designerType, "Select", typeof(string));
            properties["Where"] = TypeDescriptor.CreateProperty(designerType, "Where", typeof(string));
            
            // Properties that should be browsable in intellisense, but not visible in the designer property grid
            properties.Remove("ContextType");
        }

        internal EntityDataSourceDesignerHelper Helper
        {
            get
            {
                return _helper;
            }
        }

    }
}
