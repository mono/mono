//------------------------------------------------------------------------------
// <copyright file="LinqDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Web.UI.WebControls.Expressions;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web.DynamicData;
    using System.Web.Resources;
    using System.Web.UI;    

    // Represents a data source that applies LINQ expressions against a business object in order to perform the Select
    // operation.  When the Delete, Insert and Update operations are enabled the business object, specified in
    // ContextTypeName, must be a LINQ TO SQL DataContext.  The LINQ expressions are applied in the order of Where,
    // OrderBy, GroupBy, OrderGroupsBy, Select.
    [
    DefaultEvent("Selecting"),
    DefaultProperty("ContextTypeName"),
    Designer("System.Web.UI.Design.WebControls.LinqDataSourceDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
    ParseChildren(true),
    PersistChildren(false),
    ResourceDescription("LinqDataSource_Description"),
    ResourceDisplayName("LinqDataSource_DisplayName"),
    ToolboxBitmap(typeof(LinqDataSource), "LinqDataSource.bmp")
    ]
    public class LinqDataSource : ContextDataSource, IDynamicDataSource {
        private const string DefaultViewName = "DefaultView";
        private LinqDataSourceView _view;

        public LinqDataSource() {
        }

        internal LinqDataSource(LinqDataSourceView view)
            : base(view) {
        }

        // internal constructor that takes page mock for unit tests.
        internal LinqDataSource(IPage page)
            : base(page) {
        }

        private LinqDataSourceView View {
            get {
                if (_view == null) {
                    _view = (LinqDataSourceView)GetView(DefaultViewName);
                }
                return _view;
            }
        }

        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription("LinqDataSource_AutoGenerateOrderByClause"),
        ]
        public bool AutoGenerateOrderByClause {
            get {
                return View.AutoGenerateOrderByClause;
            }
            set {
                View.AutoGenerateOrderByClause = value;
            }
        }

        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription("LinqDataSource_AutoGenerateWhereClause"),
        ]
        public bool AutoGenerateWhereClause {
            get {
                return View.AutoGenerateWhereClause;
            }
            set {
                View.AutoGenerateWhereClause = value;
            }
        }

        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription("LinqDataSource_AutoPage"),
        ]
        public bool AutoPage {
            get {
                return View.AutoPage;
            }
            set {
                View.AutoPage = value;
            }
        }

        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription("LinqDataSource_AutoSort"),
        ]
        public bool AutoSort {
            get {
                return View.AutoSort;
            }
            set {
                View.AutoSort = value;
            }
        }

        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        ResourceDescription("LinqDataSource_DeleteParameters"),
        Browsable(false)
        ]
        public ParameterCollection DeleteParameters {
            get {
                return View.DeleteParameters;
            }
        }

        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription("LinqDataSource_ContextTypeName")
        ]
        public override string ContextTypeName {
            get {
                return View.ContextTypeName;
            }
            set {
                View.ContextTypeName = value;
            }
        }

        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription("LinqDataSource_EnableDelete"),
        ]
        public bool EnableDelete {
            get {
                return View.EnableDelete;
            }
            set {
                View.EnableDelete = value;
            }
        }

        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription("LinqDataSource_EnableInsert"),
        ]
        public bool EnableInsert {
            get {
                return View.EnableInsert;
            }
            set {
                View.EnableInsert = value;
            }
        }

        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription("LinqDataSource_EnableObjectTracking"),
        ]
        public bool EnableObjectTracking {
            get {
                return View.EnableObjectTracking;
            }
            set {
                View.EnableObjectTracking = value;
            }
        }

        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription("LinqDataSource_EnableUpdate"),
        ]
        public bool EnableUpdate {
            get {
                return View.EnableUpdate;
            }
            set {
                View.EnableUpdate = value;
            }
        }

        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription("LinqDataSource_GroupBy"),
        ]
        public string GroupBy {
            get {
                return View.GroupBy;
            }
            set {
                View.GroupBy = value;
            }
        }

        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        ResourceDescription("LinqDataSource_GroupByParameters"),
        Browsable(false)
        ]
        public ParameterCollection GroupByParameters {
            get {
                return View.GroupByParameters;
            }
        }

        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        ResourceDescription("LinqDataSource_InsertParameters"),
        Browsable(false)
        ]
        public ParameterCollection InsertParameters {
            get {
                return View.InsertParameters;
            }
        }

        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription("LinqDataSource_OrderBy"),
        ]
        public string OrderBy {
            get {
                return View.OrderBy;
            }
            set {
                View.OrderBy = value;
            }
        }

        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        ResourceDescription("LinqDataSource_OrderByParameters"),
        Browsable(false)
        ]
        public ParameterCollection OrderByParameters {
            get {
                return View.OrderByParameters;
            }
        }

        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription("LinqDataSource_OrderGroupsBy"),
        ]
        public string OrderGroupsBy {
            get {
                return View.OrderGroupsBy;
            }
            set {
                View.OrderGroupsBy = value;
            }
        }

        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        ResourceDescription("LinqDataSource_OrderGroupsByParameters"),
        Browsable(false)
        ]
        public ParameterCollection OrderGroupsByParameters {
            get {
                return View.OrderGroupsByParameters;
            }
        }

        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription("LinqDataSource_Select"),
        ]
        public string Select {
            get {
                return View.SelectNew;
            }
            set {
                View.SelectNew = value;
            }
        }

        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        ResourceDescription("LinqDataSource_SelectParameters"),
        Browsable(false)
        ]
        public ParameterCollection SelectParameters {
            get {
                return View.SelectNewParameters;
            }
        }

        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription("LinqDataSource_StoreOriginalValuesInViewState"),
        ]
        public bool StoreOriginalValuesInViewState {
            get {
                return View.StoreOriginalValuesInViewState;
            }
            set {
                View.StoreOriginalValuesInViewState = value;
            }
        }

        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription("LinqDataSource_TableName"),
        ]
        public string TableName {
            get {
                return View.TableName;
            }
            set {
                View.TableName = value;
            }
        }

        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        ResourceDescription("LinqDataSource_UpdateParameters"),
        Browsable(false)
        ]
        public ParameterCollection UpdateParameters {
            get {
                return View.UpdateParameters;
            }
        }

        [
        DefaultValue(""),
        Category("Data"),
        ResourceDescription("LinqDataSource_Where"),
        ]
        public string Where {
            get {
                return View.Where;
            }
            set {
                View.Where = value;
            }
        }

        [
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        ResourceDescription("LinqDataSource_WhereParameters"),
        Browsable(false)
        ]
        public ParameterCollection WhereParameters {
            get {
                return View.WhereParameters;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_ContextCreated"),
        ]
        public event EventHandler<LinqDataSourceStatusEventArgs> ContextCreated {
            add {
                View.ContextCreated += value;
            }
            remove {
                View.ContextCreated -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_ContextCreating"),
        ]
        public event EventHandler<LinqDataSourceContextEventArgs> ContextCreating {
            add {
                View.ContextCreating += value;
            }
            remove {
                View.ContextCreating -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_ContextDisposing"),
        ]
        public event EventHandler<LinqDataSourceDisposeEventArgs> ContextDisposing {
            add {
                View.ContextDisposing += value;
            }
            remove {
                View.ContextDisposing -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_Deleted"),
        ]
        public event EventHandler<LinqDataSourceStatusEventArgs> Deleted {
            add {
                View.Deleted += value;
            }
            remove {
                View.Deleted -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_Deleting"),
        ]
        public event EventHandler<LinqDataSourceDeleteEventArgs> Deleting {
            add {
                View.Deleting += value;
            }
            remove {
                View.Deleting -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_Inserted"),
        ]
        public event EventHandler<LinqDataSourceStatusEventArgs> Inserted {
            add {
                View.Inserted += value;
            }
            remove {
                View.Inserted -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_Inserting"),
        ]
        public event EventHandler<LinqDataSourceInsertEventArgs> Inserting {
            add {
                View.Inserting += value;
            }
            remove {
                View.Inserting -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_Selected"),
        ]
        public event EventHandler<LinqDataSourceStatusEventArgs> Selected {
            add {
                View.Selected += value;
            }
            remove {
                View.Selected -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_Selecting"),
        ]
        public event EventHandler<LinqDataSourceSelectEventArgs> Selecting {
            add {
                View.Selecting += value;
            }
            remove {
                View.Selecting -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_Updated"),
        ]
        public event EventHandler<LinqDataSourceStatusEventArgs> Updated {
            add {
                View.Updated += value;
            }
            remove {
                View.Updated -= value;
            }
        }

        [
        Category("Data"),
        ResourceDescription("LinqDataSource_Updating"),
        ]
        public event EventHandler<LinqDataSourceUpdateEventArgs> Updating {
            add {
                View.Updating += value;
            }
            remove {
                View.Updating -= value;
            }
        }       

        protected virtual LinqDataSourceView CreateView() {
            return new LinqDataSourceView(this, DefaultViewName, Context);
        }

        protected override QueryableDataSourceView CreateQueryableView() {
            return CreateView();
        }

        public int Delete(IDictionary keys, IDictionary oldValues) {
            return View.Delete(keys, oldValues);
        }

        public int Insert(IDictionary values) {
            return View.Insert(values);
        }       

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);            
            if (StoreOriginalValuesInViewState && (EnableUpdate || EnableDelete)) {
                IPage.RegisterRequiresViewStateEncryption();
            }
        }
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected internal override void OnUnload(EventArgs e) {
            base.OnUnload(e);
            // keeping the select contexts alive until Unload so that users can use deferred query evaluation.
            if (View != null) {
                View.ReleaseSelectContexts();
            }
        }

        public int Update(IDictionary keys, IDictionary values, IDictionary oldValues) {
            return View.Update(keys, values, oldValues);
        }

        #region IDynamicDataSource members

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
                         Justification = "Property used for IDynamicDataSource abstraction that wraps the ContextTypeName.")]
        Type IDynamicDataSource.ContextType {
            get {
                if (String.IsNullOrEmpty(ContextTypeName)) {
                    return null;
                }
                return View.ContextType;
            }
            set {
                View.ContextTypeName = value.AssemblyQualifiedName;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
                         Justification = "Property used for IDynamicDataSource abstraction that wraps the TableName.")]
        string IDynamicDataSource.EntitySetName {
            get {
                return TableName;
            }
            set {
                TableName = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "IDynamicDataSource abstraction for handling exceptions available to user through other events.")]
        event EventHandler<DynamicValidatorEventArgs> IDynamicDataSource.Exception {
            add {
                View.Exception += value;
            }
            remove {
                View.Exception -= value;
            }
        }

        #endregion

    }
}

