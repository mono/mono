using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Expressions;
using System.Web.Resources;

namespace System.Web.DynamicData {
    public class DynamicFilter : Control, IFilterExpressionProvider {
        private HttpContextBase _context;
        private IQueryableDataSource _dataSource;
        private Func<MetaColumn, string, QueryableFilterUserControl> _filterLoader;
        private QueryableFilterUserControl _filterUserControl;

        protected internal MetaColumn Column {
            get;
            private set;
        }

        /// <summary>
        /// The name of the column that this control handles
        /// </summary>
        [
        Category("Data"),
        DefaultValue(""),
        ResourceDescription("DynamicFilter_DataField")
        ]
        public string DataField {
            get {
                object o = ViewState["DataField"];
                return (o == null) ? String.Empty : (string)o;
            }
            set {
                ViewState["DataField"] = value;
            }
        }

        /// <summary>
        /// An optional property that can be used to override the column's default default filter UI hint.
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        ResourceDescription("DynamicFilter_FilterUIHint")
        ]
        public string FilterUIHint {
            get {
                object o = ViewState["FilterUIHint"];
                return (o == null) ? String.Empty : (string)o;
            }
            set {
                ViewState["FilterUIHint"] = value;
            }
        }

        public DynamicFilter()
            : this(CreateUserControl) {
        }

        // internal for unit testing
        internal DynamicFilter(Func<MetaColumn, string, QueryableFilterUserControl> filterLoader) {
            _filterLoader = filterLoader;
        }

        // internal for unit testing
        internal static QueryableFilterUserControl CreateUserControl(MetaColumn column, string filterUiHint) {
            return column.Model.FilterFactory.CreateFilterControl(column, filterUiHint);
        }

        internal new HttpContextBase Context {
            get {
                return _context ?? new HttpContextWrapper(HttpContext.Current);
            }
            set {
                _context = value;
            }
        }

        public event EventHandler FilterChanged;

        /// <summary>
        /// Returns the filter template that was created for this control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control FilterTemplate {
            get {
                return _filterUserControl;
            }
        }

        protected override void Render(HtmlTextWriter writer) {
            if (DesignMode) {
                writer.Write("[" + GetType().Name + "]");
            }
            else {
                base.Render(writer);
            }
        }

        private void EnsureInit(IQueryableDataSource dataSource) {
            if (_filterUserControl == null) {
                MetaTable table = DynamicDataExtensions.GetMetaTable(dataSource, Context);
                Column = table.GetColumn(DataField);
                _filterUserControl = _filterLoader(Column, FilterUIHint);
                _filterUserControl.Initialize(Column, dataSource, Context);

                _filterUserControl.FilterChanged += new EventHandler(Child_FilterChanged);

                Controls.Add(_filterUserControl);
            }
        }

        private void Child_FilterChanged(object sender, EventArgs e) {
            EventHandler eventHandler = FilterChanged;
            if (eventHandler != null) {
                eventHandler(sender, e);
            }
        }

        internal void Initialize(IQueryableDataSource dataSource) {
            Debug.Assert(dataSource != null);
            EnsureInit(dataSource);
        }

        #region IFilterExpressionProvider Members

        void IFilterExpressionProvider.Initialize(IQueryableDataSource dataSource) {
            if (dataSource == null) {
                throw new ArgumentNullException("dataSource");
            }

            _dataSource = dataSource;

            Page.InitComplete += new EventHandler(Page_InitComplete);
        }

        void Page_InitComplete(object sender, EventArgs e) {
            Debug.Assert(_dataSource != null);
            EnsureInit(_dataSource);
        }

        IQueryable IFilterExpressionProvider.GetQueryable(IQueryable source) {
            return _filterUserControl.GetQueryable(source);
        }

        #endregion
    }
}
