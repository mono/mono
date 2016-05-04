namespace System.Web.UI.WebControls {
    using System.Web.UI.WebControls.Expressions;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Security.Permissions;        
    using System.Web.UI;
    using System.Web.Resources;

    [TargetControlType(typeof(IQueryableDataSource))]
    [NonVisualControl()]
    [DefaultProperty("TargetControlID")]
    [ToolboxBitmap(typeof(QueryExtender), "QueryExtender.bmp")]
    [Designer("System.Web.UI.Design.QueryExtenderDesigner, " + AssemblyRef.SystemWebExtensionsDesign)]
    [ParseChildren(true, "Expressions")]
    [PersistChildren(false)]
    public class QueryExtender : Control {
        private QueryExpression _query;
        private string _targetControlID;

        private IQueryableDataSource _dataSource;

        public QueryExtender() {
        }

        internal QueryExtender(IQueryableDataSource dataSource) {
            _dataSource = dataSource;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IQueryableDataSource DataSource {
            get {
                if (_dataSource == null) {
                    if (String.IsNullOrEmpty(TargetControlID)) {
                        throw new InvalidOperationException(AtlasWeb.DataSourceControlExtender_TargetControlIDMustBeSpecified);
                    }
                    _dataSource = DataBoundControlHelper.FindControl(this, TargetControlID) as IQueryableDataSource;
                    // 
                    if (_dataSource == null) {
                        throw new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture,
                            AtlasWeb.QueryExtender_DataSourceMustBeIQueryableDataSource,
                            TargetControlID));
                    }
                }
                return _dataSource;
            }
        }

        [
        Category("Behavior"),
        ResourceDescription("QueryExtender_Expressions"),
        PersistenceMode(PersistenceMode.InnerDefaultProperty),
        ]
        public DataSourceExpressionCollection Expressions {
            get {
                return Query.Expressions;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        IDReferenceProperty,
        ResourceDescription("ExtenderControl_TargetControlID"),
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID"),
        ]
        public virtual string TargetControlID {
            get {
                return _targetControlID ?? String.Empty;
            }
            set {
                if (_targetControlID != value) {
                    _dataSource = null;
                    _targetControlID = value;
                }
            }
        }

        private QueryExpression Query {
            get {
                if (_query == null) {
                    _query = new QueryExpression();
                }
                return _query;
            }
        }


        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected internal override void OnInit(EventArgs e) {
            if (!DesignMode) {
                DataSource.QueryCreated += new EventHandler<QueryCreatedEventArgs>(OnDataSourceQueryCreated);
                // Initialize the Query
                Query.Initialize(this, Context, DataSource);
            }

            base.OnInit(e);
        }

        private void OnDataSourceQueryCreated(object sender, QueryCreatedEventArgs e) {
            e.Query = Query.GetQueryable(e.Query);
        }

        protected override object SaveViewState() {
            Pair p = new Pair();
            p.First = base.SaveViewState();
            p.Second = _query != null ? ((IStateManager)_query.Expressions).SaveViewState() : null;
            return p;
        }

        protected override void LoadViewState(object savedState) {
            Pair p = (Pair)savedState;
            base.LoadViewState(p.First);
            if (p.Second != null) {
                ((IStateManager)Query.Expressions).LoadViewState(p.Second);
            }
        }

        protected override void TrackViewState() {
            base.TrackViewState();
            if (_query != null) {
                ((IStateManager)_query.Expressions).TrackViewState();
            }
        }
    }
}
