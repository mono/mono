using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Web.Resources;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.DynamicData.Util;

namespace System.Web.DynamicData {
    /// <summary>
    /// <para>A control that displays links to table actions based on routing rules. It will not generate links for actions that are not
    /// allowed by the routing rules. It can work in 3 modes: explicit, databinding to MetaTable, or databinding to a data row.</para>
    /// <para>Databinding to MetaTable allows for creating links to actions for a collection of MetaTable objects (such as in the Default.aspx
    /// page in the project templates)</para>
    /// <para>Databinding to a data row allows for creating links to actions for data rows retrieved from a database. These are usually used with
    /// Edit and Details actions.</para>
    /// <para>Explicit mode allows for links to non-item-specific actions (like List and Insert) and is achieved by properly setting
    /// ContextTypeName, Table, and Action properties. This is done in the PreRender phase if the NavigateUrl property is null (i.e. it has not
    /// been set explicitly or did not get set in one of the databinding scenarios.)</para>
    /// <para>Extra route parameters can be provided by declaring expando attributes on the controls markup.</para>
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HyperLink", Justification="It's an extension of the HyperLink class")]
    [DefaultProperty("Action")]
    [ToolboxBitmap(typeof(DynamicHyperLink), "DynamicHyperLink.bmp")]
    public class DynamicHyperLink : HyperLink, IAttributeAccessor {
        private HttpContextBase _context;
        private bool _dataBound;
        private object _dataItem;
        private Dictionary<string, string> _extraRouteParams = new Dictionary<string, string>();

        /// <summary>
        /// The name of the action
        /// </summary>
        [TypeConverter(typeof(ActionConverter))]
        [DefaultValue("")]
        [Category("Navigation")]
        [ResourceDescription("DynamicHyperLink_Action")]
        public string Action {
            get {
                object o = ViewState["Action"];
                return (o == null ? String.Empty: (string)o);
            }
            set {
                ViewState["Action"] = value;
            }
        }

        internal new HttpContextBase Context {
            get {
                return _context ?? new HttpContextWrapper(base.Context);
            }
            set {
                _context = value;
            }
        }

        /// <summary>
        /// The name of the context type
        /// </summary>
        [DefaultValue("")]
        [Category("Navigation")]
        [ResourceDescription("DynamicHyperLink_ContextTypeName")]
        public string ContextTypeName {
            get {
                object o = ViewState["ContextTypeName"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["ContextTypeName"] = value;
            }
        }

        /// <summary>
        /// The name of the column whose value will be used to populate the Text
        /// property if it is not already set in data binding scenarios.
        /// </summary>
        [DefaultValue("")]
        [Category("Navigation")]
        [ResourceDescription("DynamicHyperLink_DataField")]
        public string DataField {
            get {
                object o = ViewState["DataField"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["DataField"] = value;
            }
        }

        // for unit testing purposes
        internal object Page_DataItem {
            get {
                return _dataItem ?? Page.GetDataItem();
            }
            set {
                _dataItem = value;
            }
        }

        /// <summary>
        /// The name of the table
        /// </summary>
        [DefaultValue("")]
        [Category("Navigation")]
        [ResourceDescription("DynamicHyperLink_TableName")]
        public string TableName {
            get {
                object o = ViewState["TableName"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["TableName"] = value;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected override void OnDataBinding(EventArgs e) {
            base.OnDataBinding(e);

            if (DesignMode) {
                return;
            }

            if (!String.IsNullOrEmpty(NavigateUrl)) {
                // stop processing if there already is a URL
                return;
            }

            if (!String.IsNullOrEmpty(TableName) || !String.IsNullOrEmpty(ContextTypeName)) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.DynamicHyperLink_CannotSetTableAndContextWhenDatabinding, this.ID));
            }

            object dataItem = Page_DataItem;
            if (dataItem == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.DynamicHyperLink_CannotBindToNull, this.ID));
            }

            MetaTable table = dataItem as MetaTable;
            if (table != null) {
                BindToMetaTable(table);
            } else {
                BindToDataItem(dataItem);
            }

            _dataBound = true;
        }

        private void BindToMetaTable(MetaTable table) {
            string action = GetActionOrDefaultTo(PageAction.List);
            NavigateUrl = table.GetActionPath(action, GetRouteValues());
            if (String.IsNullOrEmpty(Text)) {
                Text = table.DisplayName;
            }
        }

        private void BindToDataItem(object dataItem) {
            dataItem = Misc.GetRealDataItem(dataItem);
            Debug.Assert(dataItem != null, "DataItem is null");
            // Try to get the MetaTable from the type and if we can't find it then ---- up.
            MetaTable table = Misc.GetTableFromTypeHierarchy(dataItem.GetType());
            if (table == null) {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture,
                    DynamicDataResources.MetaModel_EntityTypeDoesNotBelongToModel,
                    dataItem.GetType().FullName));
            }

            string action = GetActionOrDefaultTo(PageAction.Details);
            NavigateUrl = table.GetActionPath(action, GetRouteValues(table, dataItem));

            if (String.IsNullOrEmpty(Text)) {
                if (!String.IsNullOrEmpty(DataField)) {
                    Text = DataBinder.GetPropertyValue(dataItem, DataField).ToString();
                } else {
                    Text = table.GetDisplayString(dataItem);
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (DesignMode) {
                if (!String.IsNullOrEmpty(NavigateUrl)) {
                    NavigateUrl = "DesignTimeUrl";
                }
                return;
            }

            // check both _dataBound and NavigateUrl cause NavigateUrl might be empty if routing/scaffolding
            // does not allow a particular action
            if (!_dataBound && String.IsNullOrEmpty(NavigateUrl)) {
                MetaTable table;
                try {
                    table = GetTable();
                } catch (Exception exception) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.DynamicHyperLink_CannotDetermineTable, this.ID), exception);
                }

                if (table == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.DynamicHyperLink_CannotDetermineTable, this.ID));
                }

                var action = GetActionOrDefaultTo(PageAction.List);
                NavigateUrl = table.GetActionPath(action, GetRouteValues());
            }
        }

        private RouteValueDictionary GetRouteValues() {
            var routeValues = new RouteValueDictionary();
            foreach (var entry in _extraRouteParams) {
                string key = entry.Key;
                routeValues[key] = entry.Value;
            }
            return routeValues;
        }

        private RouteValueDictionary GetRouteValues(MetaTable table, object row) {
            RouteValueDictionary routeValues = GetRouteValues();
            foreach (var pk in table.GetPrimaryKeyDictionary(row)) {
                routeValues[pk.Key] = pk.Value;
            }
            return routeValues;
        }

        private string GetActionOrDefaultTo(string defaultAction) {
            return String.IsNullOrEmpty(Action) ? defaultAction : Action;
        }

        // internal for unit testing
        internal virtual MetaTable GetTable() {
            MetaTable table;
            if (!String.IsNullOrEmpty(TableName)) {
                table = GetTableFromTableName();
            } else {
                table = DynamicDataRouteHandler.GetRequestMetaTable(Context);
            }
            return table;
        }

        private MetaTable GetTableFromTableName() {
            var tableName = TableName;
            var contextTypeName = ContextTypeName;
            Debug.Assert(!String.IsNullOrEmpty(tableName));

            if (!String.IsNullOrEmpty(contextTypeName)) {
                // context type allows to disambiguate table names
                Type contextType = BuildManager.GetType(contextTypeName, /* throwOnError */ true, /* ignoreCase */ true);
                MetaModel model = MetaModel.GetModel(contextType);
                MetaTable table = model.GetTable(tableName, contextType);
                return table;
            } else {
                var table = DynamicDataRouteHandler.GetRequestMetaTable(Context);
                if (table == null) {
                    return null;
                }
                return table.Model.GetTable(tableName);
            }
        }

        #region IAttributeAccessor Members

        string IAttributeAccessor.GetAttribute(string key) {
            return (string)_extraRouteParams[key];
        }

        void IAttributeAccessor.SetAttribute(string key, string value) {
            _extraRouteParams[key] = value;
        }

        #endregion
    }
}
