namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI.WebControls.Expressions;

    [
    ParseChildren(true),
    PersistChildren(false)
    ]
    public abstract class QueryableDataSource : DataSourceControl, IQueryableDataSource {
        private const string DefaultViewName = "DefaultView";
        private ReadOnlyCollection<string> _viewNames;
        private QueryableDataSourceView _view;
        private readonly new IPage _page;

        internal QueryableDataSource(IPage page) {
            _page = page;
        }

        internal QueryableDataSource(QueryableDataSourceView view) {
            _view = view;
        }

        protected QueryableDataSource() {

        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification="View is used by derived types")]
        private QueryableDataSourceView View {
            get {
                if (_view == null) {
                    _view = CreateQueryableView();
                }
                return _view;
            }
        }

        internal IPage IPage {
            get {
                if (_page != null) {
                    return _page;
                }
                else {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    return new PageWrapper(page);
                }
            }
        }

        protected abstract QueryableDataSourceView CreateQueryableView();

        protected override ICollection GetViewNames() {
            if (_viewNames == null) {
                _viewNames = new ReadOnlyCollection<string>(new[] { DefaultViewName });
            }
            return _viewNames;
        }
        
        protected override DataSourceView GetView(string viewName) {
            if (viewName == null) {
                throw new ArgumentNullException("viewName");
            }
            // viewName comes from the DataMember property on the databound control and is an empty string
            // by default.  An empty string should be treated as if it were the default view name.
            if ((viewName.Length != 0) &&
                !String.Equals(viewName, DefaultViewName, StringComparison.OrdinalIgnoreCase)) {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.LinqDataSource_InvalidViewName, ID, DefaultViewName), "viewName");
            }
            return View;
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            IPage.LoadComplete += new EventHandler(OnPageLoadComplete);
        }

        // Used for unit testing only
        internal void SetView(QueryableDataSourceView view) {
            _view = view;
        }
        
        protected virtual void UpdateParameterVales() {
            View.WhereParameters.UpdateValues(Context, this);
            View.OrderGroupsByParameters.UpdateValues(Context, this);
            View.GroupByParameters.UpdateValues(Context, this);
            View.OrderByParameters.UpdateValues(Context, this);
            View.SelectNewParameters.UpdateValues(Context, this);
        }

        private void OnPageLoadComplete(object sender, EventArgs e) {
            UpdateParameterVales();
        }

        protected override object SaveViewState() {
            Pair myState = new Pair();
            myState.First = base.SaveViewState();
            if (_view != null) {
                myState.Second = ((IStateManager)_view).SaveViewState();
            }
            if ((myState.First == null) &&
                (myState.Second == null)) {
                return null;
            }
            return myState;
        }

        protected override void TrackViewState() {
            base.TrackViewState();
            if (_view != null) {
                ((IStateManager)_view).TrackViewState();
            }
        }

        protected override void LoadViewState(object savedState) {
            if (savedState == null) {
                base.LoadViewState(null);
            }
            else {
                Pair myState = (Pair)savedState;
                base.LoadViewState(myState.First);
                if (myState.Second != null) {
                    ((IStateManager)View).LoadViewState(myState.Second);
                }
            }
        }

        #region IQueryableDataSource Members

        public void RaiseViewChanged() {
            View.RaiseViewChanged();
        }

        public event EventHandler<QueryCreatedEventArgs> QueryCreated {
            add {
                View.QueryCreated += value;
            }
            remove {
                View.QueryCreated -= value;
            }
        }

        #endregion
    }
}
