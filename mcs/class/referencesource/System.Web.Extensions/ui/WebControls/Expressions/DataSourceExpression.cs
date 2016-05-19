namespace System.Web.UI.WebControls.Expressions {
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    
  
    public abstract class DataSourceExpression : IStateManager {
        private bool _tracking;
        private StateBag _viewState;

        protected HttpContext Context {
            get;
            private set;
        }

        protected Control Owner {
            get;
            private set;
        }     

        public IQueryableDataSource DataSource {
            get;
            // Internal set for unit testing
            internal set;
        }


        protected bool IsTrackingViewState {
            get {
                return _tracking;
            }
        }        

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        protected StateBag ViewState {
            get {
                if (_viewState == null) {
                    _viewState = new StateBag();
                    if (_tracking)
                        ((IStateManager)_viewState).TrackViewState();
                }
                return _viewState;
            }
        }

        protected DataSourceExpression() {
        }

        // internal for unit testing
        internal DataSourceExpression(Control owner) {
            Owner = owner;
        }

        public void SetDirty() {
            ViewState.SetDirty(true);
        }

        protected virtual void LoadViewState(object savedState) {
            if (savedState != null) {
                ((IStateManager)ViewState).LoadViewState(savedState);
            }
        }

        protected virtual object SaveViewState() {
            return (_viewState != null) ? ((IStateManager)_viewState).SaveViewState() : null;
        }

        protected virtual void TrackViewState() {
            _tracking = true;

            if (_viewState != null) {
                ((IStateManager)_viewState).TrackViewState();
            }
        }

        public abstract IQueryable GetQueryable(IQueryable source);

        public virtual void SetContext(Control owner, HttpContext context, IQueryableDataSource dataSource) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }

            if (context == null) {
                throw new ArgumentNullException("context");
            }

            if (dataSource == null) {
                throw new ArgumentNullException("dataSource");
            }

            Owner = owner;
            Context = context;
            DataSource = dataSource;
        }

        #region IStateManager Members

        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        object IStateManager.SaveViewState() {
            return SaveViewState();
        }

        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        #endregion
    }
}
