using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web.Resources;
using System.Web.UI;
using System.Web.UI.WebControls.Expressions;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {
    /// <summary>
    /// A templated control that automatically generates a collection of filters for a table associated with a given data source.
    /// It is designed to work with the QueryExtender architecture and it will not render anything unless it's referenced by a
    /// DynamicFilterExpression inside of a QueryExtender.
    /// </summary>
    [ParseChildren(true)]
    [PersistChildren(false)]
    public class QueryableFilterRepeater : Control, IFilterExpressionProvider {
        private HttpContextBase _context;
        private IQueryableDataSource _dataSource;
        private List<DynamicFilter> _filters = new List<DynamicFilter>();
        private bool _initialized = false;

        // for unit testing
        private new HttpContextBase Context {
            get {
                return _context ?? new HttpContextWrapper(HttpContext.Current);
            }
        }

        /// <summary>
        /// The ID of a DynamicFilter control inside of the template that will be used configured to be a filter for a particular column.
        /// The default value is "DynamicFilter"
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue("DynamicFilter"),
        Themeable(false),
        IDReferenceProperty(typeof(QueryableFilterUserControl)),
        ResourceDescription("DynamicFilterRepeater_DynamicFilterContainerId")
        ]
        public string DynamicFilterContainerId {
            get {
                string id = ViewState["__FilterContainerId"] as string;
                return String.IsNullOrEmpty(id) ? "DynamicFilter" : id;
            }
            set {
                ViewState["__FilterContainerId"] = value;
            }
        }

        /// <summary>
        /// The template in which the layout of each filter can be specified. Just like ItemTempalte in Repeater.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(INamingContainer))]
        public virtual ITemplate ItemTemplate { get; set; }

        public QueryableFilterRepeater() {
        }

        // for unit testing
        internal QueryableFilterRepeater(HttpContextBase context)
            : this() {
            _context = context;
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected override void OnPreRender(EventArgs e) {
            if (_filters.Count == 0) {
                this.Visible = false;
            }
            base.OnPreRender(e);
        }

        #region IFilterExpressionProvider Members

        void IFilterExpressionProvider.Initialize(IQueryableDataSource dataSource) {
            if (dataSource == null) {
                throw new ArgumentNullException("dataSource");
            }

            if (ItemTemplate == null) {
                return;
            }

            _dataSource = dataSource;
            Page.InitComplete += new EventHandler(Page_InitComplete);
        }

        internal void Page_InitComplete(object sender, EventArgs e) {
            if (_initialized) {
                return;
            }

            Debug.Assert(_dataSource != null);

            MetaTable table = DynamicDataExtensions.GetMetaTable(_dataSource, Context);
            int itemIndex = 0;
            foreach (MetaColumn column in table.GetFilteredColumns()) {
                FilterRepeaterItem item = new FilterRepeaterItem() { DataItemIndex = itemIndex, DisplayIndex = itemIndex };
                itemIndex++;
                ItemTemplate.InstantiateIn(item);
                Controls.Add(item);

                DynamicFilter filter = item.FindControl(DynamicFilterContainerId) as DynamicFilter;
                if (filter == null) {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture,
                            DynamicDataResources.FilterRepeater_CouldNotFindControlInTemplate,
                            ID,
                            typeof(QueryableFilterUserControl).FullName,
                            DynamicFilterContainerId));
                }
                filter.Context = Context; // needed for unit testing
                filter.DataField = column.Name;

                item.DataItem = column;
                item.DataBind();
                item.DataItem = null;

                // Keep track of all the filters we create
                _filters.Add(filter);
            }

            _filters.ForEach(f => f.Initialize(_dataSource));
            _initialized = true;
        }

        IQueryable IFilterExpressionProvider.GetQueryable(IQueryable source) {
            foreach (DynamicFilter filter in _filters) {
                source = ((IFilterExpressionProvider)filter).GetQueryable(source);
            }
            return source;
        }

        #endregion

        private class FilterRepeaterItem : Control, IDataItemContainer {
            public object DataItem { get; internal set; }

            public int DataItemIndex { get; internal set; }

            public int DisplayIndex { get; internal set; }
        }
    }
}
